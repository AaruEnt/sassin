using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using NaughtyAttributes;
using UnityEditor;
using UnityEngine.Serialization;
using UnityEngine.XR;


namespace Autohand {
    public enum HandGrabType {
        Default,
        HandToGrabbable,
        GrabbableToHand
    }

    public enum HandGrabPoseType {
        Grab,
        Pinch
    }

    [HelpURL("https://app.gitbook.com/s/5zKO0EvOjzUDeT2aiFk3/auto-hand/grabbable"), DefaultExecutionOrder(-100)]
    public class Grabbable : GrabbableBase, IGrabbableEvents {


        [Tooltip("This will copy the given grabbables settings to this grabbable when applied"), OnValueChanged("EditorCopyGrabbable")]
        public Grabbable CopySettings;

        [Header("Grab Settings")]
        [Tooltip("Which hand this can be held by")]
        public HandGrabType grabType = HandGrabType.Default;

        [Tooltip("Which hand pose this will grab with")]
        public HandGrabPoseType grabPoseType = HandGrabPoseType.Grab;

        [Tooltip("Which hand this can be held by")]
        public HandType handType = HandType.both;

        [Tooltip("Whether or not this can be grabbed with more than one hand")]
        public bool singleHandOnly = false;

        [Space]
        [Tooltip("Will the item automatically return the hand on grab - good for saved poses, bad for heavy things")]
        public bool instantGrab = false;

        [DisableIf("instantGrab"), Tooltip("If true (and using HandToGrabbable) the hand will only return to the follow while moving. Good for picking up objects without disrupting the things around them - you can change the speed of the hand return on the hand through the gentleGrabSpeed value")]
        public bool useGentleGrab = false;

        [Tooltip("Creates an offset an grab so the hand will not return to the hand on grab - Good for statically jointed grabbable objects")]
        public bool maintainGrabOffset = false;

        [Tooltip("This will NOT parent the object under the hands on grab. This will parent the object to the parents of the hand, which allow you to move the hand parent object smoothly while holding an item, but will also allow you to move items that are very heavy - recommended for all objects that aren't very heavy or jointed to other rigidbodies")]
        public bool parentOnGrab = true;


        [Header("Held Settings")]

        [Tooltip("Replaces the physics material with the resources NoFriction material while held")]
        public bool heldNoFriction = true;

        [Tooltip("Experimental - ignores weight of held object while held")]
        public bool ignoreWeight = false;

        [ShowIf("singleHandOnly")]
        [Tooltip("if false single handed items cannot be passes back and forth on grab")]
        public bool allowHeldSwapping = true;

        [Header("Release Settings")]
        [Tooltip("How much to multiply throw by for this grabbable when releasing - 0-1 for no or reduced throw strength")]
        [FormerlySerializedAs("throwMultiplyer")]
        public float throwPower = 1;

        [Tooltip("The required force to break the fixedJoint\n " +
                 "Turn this to \"infinity\" to disable (Might cause jitter)\n" +
                "Ideal value depends on hand mass and velocity settings")]
        public float jointBreakForce = 3500;



        [AutoSmallHeader("Advanced Settings")]
        public bool showAdvancedSettings = true;

        [Tooltip("Adds and links a GrabbableChild to each child with a collider on start - So the hand can grab them")]
        public bool makeChildrenGrabbable = true;

        [Min(0), Tooltip("I.E. Grab Prioirty - BIGGER IS BETTER - divides highlight distance by this when calculating which object to grab. Hands always grab closest object to palm")]
        public float grabPriorityWeight = 1;

        [Tooltip("The number of seconds that the hand collision should ignore the released object\n (Good for increased placement precision and resolves clipping errors)"), Min(0)]
        public float ignoreReleaseTime = 0.5f;


        [Tooltip("The minimum allow drag a held objects rigidbody can have, this can help prevent dramatic wobbling on held objects"), Min(0)]
        public float minHeldDrag = 1.5f;
        [Tooltip("The minimum allow drag a held objects rigidbody can have, this can help prevent dramatic wobbling on held objects"), Min(0)]
        public float minHeldAngleDrag = 3f;
        [Tooltip("The minimum allow drag a held objects rigidbody can have, this can help prevent dramatic wobbling on held objects"), Min(0)]
        public float minHeldMass = 0.1f;

        [Tooltip("Lowing this value will help allow for more stable joint interactions and make objects seems heavier if lowered enough"), Min(0)]
        public float maxHeldVelocity = 10f;
        [Space]

        [Tooltip("Offsets the grabbable by this much when being held")]
        public Vector3 heldPositionOffset;

        [Tooltip("Offsets the grabbable by this many degrees when being held")]
        public Vector3 heldRotationOffset;

        [Space]

        [Min(0), Tooltip("The joint that connects the hand and the grabbable. Defaults to the joint in AutoHand/Resources/DefaultJoint.prefab if empty")]
        public ConfigurableJoint customGrabJoint;

        [Space]

        [Tooltip("For the special use case of having grabbable objects with physics jointed peices move properly while being held")]
        public List<Rigidbody> jointedBodies = new List<Rigidbody>();

        [Tooltip("For the special use case of having things connected to the grabbable that the hand should ignore while being held (good for doors and drawers) -> for always active use the [GrabbableIgnoreHands] Component")]
        public List<Collider> heldIgnoreColliders = new List<Collider>();

        [Space]

        [Tooltip("Whether or not the break call made only when holding with multiple hands - if this is false the break event can be called by forcing an object into a static collider"), HideInInspector]
        public bool pullApartBreakOnly = true;

        [AutoToggleHeader("Show Events")]
        public bool showEvents = true;
        [Space]
        [ShowIf("showEvents")]
        public UnityHandGrabEvent onGrab = new UnityHandGrabEvent();
        [ShowIf("showEvents")]
        public UnityHandGrabEvent onRelease = new UnityHandGrabEvent();

        [ShowIf("showEvents")]
        [Space, Space]
        public UnityHandGrabEvent onSqueeze = new UnityHandGrabEvent();
        [ShowIf("showEvents")]
        public UnityHandGrabEvent onUnsqueeze = new UnityHandGrabEvent();

        [Space, Space]
        [ShowIf("showEvents")]
        public UnityHandGrabEvent onHighlight = new UnityHandGrabEvent();
        [ShowIf("showEvents")]
        public UnityHandGrabEvent onUnhighlight = new UnityHandGrabEvent();
        [Space, Space]

        [ShowIf("showEvents")]
        public UnityHandGrabEvent OnJointBreak = new UnityHandGrabEvent();


        //Advanced Hidden Settings
        [HideInInspector, Tooltip("Lock hand in place on grab (This is a legacy setting, set hand kinematic on grab/release instead)")]
        public bool lockHandOnGrab = false;



        //For programmers <3
        public HandGrabEvent OnBeforeGrabEvent;
        public HandGrabEvent OnGrabEvent;

        public HandGrabEvent OnBeforeReleaseEvent;
        public HandGrabEvent OnReleaseEvent;
        public HandGrabEvent OnJointBreakEvent;

        public HandGrabEvent OnSqueezeEvent;
        public HandGrabEvent OnUnsqueezeEvent;

        public HandGrabEvent OnHighlightEvent;
        public HandGrabEvent OnUnhighlightEvent;

        public PlacePointEvent OnPlacePointHighlightEvent;
        public PlacePointEvent OnPlacePointUnhighlightEvent;
        public PlacePointEvent OnPlacePointAddEvent;
        public PlacePointEvent OnPlacePointRemoveEvent;


        /// <summary>Whether or not this object was force released (dropped) when last released (as opposed to being intentionally released)</summary>
        public bool wasForceReleased { get; protected internal set; } = false;
        public Hand lastHeldBy { get; protected set; } = null;


#if UNITY_EDITOR
        void EditorCopyGrabbable() {
            if(CopySettings != null)
                EditorUtility.CopySerialized(CopySettings, this);
        }
#endif


        public void Start()
        {
#if UNITY_EDITOR
            if (Selection.activeGameObject == gameObject)
            {
                Selection.activeGameObject = null;
                Debug.Log("Auto Hand: highlighting grabbables and rigidbodies in the inspector can cause lag and quality reduction at runtime in VR. (Automatically deselecting at runtime) Remove this code at any time.", this);
                editorSelected = true;
            }
            Application.quitting += () => { if (editorSelected && Selection.activeGameObject == null) Selection.activeGameObject = gameObject; };
#endif
        }

        public override void Awake() {
            base.Awake();

            if(makeChildrenGrabbable)
                MakeChildrenGrabbable();

            if(body.transform != transform && !body.gameObject.HasGrabbable(out Grabbable grab)) {
                var grabChild = body.gameObject.AddComponent<GrabbableChild>();
                grabChild.grabParent = this;
            }


            for(int i = 0; i < jointedBodies.Count; i++) {
                jointedParents.Add(jointedBodies[i].transform.parent != null ? jointedBodies[i].transform.parent : null);
                if(jointedBodies[i].gameObject.HasGrabbable(out var grabbable) && !jointedGrabbables.Contains(grabbable))
                    jointedGrabbables.Add(grabbable);
            }

            for(int i = 0; i < transform.childCount; i++) {
                ConnectGrabbablesRecursive(transform.GetChild(i));
            }

            void ConnectGrabbablesRecursive(Transform obj) {
                Grabbable grab1;
                for(int i = 0; i < obj.childCount; i++) {
                    if(obj.CanGetComponent<Grabbable>(out grab1)) {
                        if(!grabbableChildren.Contains(grab1))
                            grabbableChildren.Add(grab1);

                        if(!grab1.grabbableParents.Contains(this))
                            grab1.grabbableParents.Add(this);
                    }

                    ConnectGrabbablesRecursive(obj.GetChild(i));
                }
            }
            grabbableChildren = new List<Grabbable>(GetComponentsInChildren<Grabbable>(true));
            if(grabbableChildren.Contains(this))
                grabbableChildren.Remove(this);


            grabbableParents = new List<Grabbable>(GetComponentsInParent<Grabbable>(true));
            if(grabbableParents.Contains(this))
                grabbableParents.Remove(this);


            //This will automatically prevent grabbables from being placed into any place points that are children of this grabbable
            var placePointChildren = GetComponentsInChildren<PlacePoint>(true);
            for(int i = 0; i < placePointChildren.Length; i++) {

                if(placePointChildren[i].dontAllows == null)
                    placePointChildren[i].dontAllows = new List<Grabbable>();
                placePointChildren[i].dontAllows.Add(this);

                if(!childPlacePoints.Contains(placePointChildren[i]))
                    childPlacePoints.Add(placePointChildren[i]);

                if(grabbableParents.Count == 0)
                    placePointChildren[i].parentGrabbable = this;

                foreach(var grabbable in grabbableChildren) {
                    if(grabbable != this && !placePointChildren[i].dontAllows.Contains(grabbable))
                        placePointChildren[i].dontAllows.Add(grabbable);
                }

                foreach(var grabbable in grabbableParents) {
                    if(grabbable != this && !placePointChildren[i].dontAllows.Contains(grabbable))
                        placePointChildren[i].dontAllows.Add(grabbable);
                }
            }

            if(grabbableParents.Count == 0) {
                foreach(var grabbable in grabbableChildren) {
                    grabbable.rootGrabbable = this;
                }
                rootGrabbable = this;
            }
        }

        protected override void OnDisable() {
            base.OnDisable();
            if(heldBy.Count != 0)
                ForceHandsRelease();
        }

        protected virtual void OnDestroy()
        {
            beingDestroyed = true;

            if (heldBy.Count != 0)
                ForceHandsRelease();

            foreach(var routine in resetLayerRoutine) {
                if(routine.Value != null)
                    StopCoroutine(routine.Value);

                IgnoreHand(routine.Key, false);
            }
            resetLayerRoutine.Clear();

            MakeChildrenUngrabbable();
            if (placePoint != null && !placePoint.disablePlacePointOnPlace)
                placePoint.Remove(this);

            Destroy(poseCombiner);
        }

        public override void HeldFixedUpdate() {
            base.HeldFixedUpdate();

            if(wasIsGrabbable && !(isGrabbable || enabled))
                ForceHandsRelease();

            wasIsGrabbable = isGrabbable || enabled;
            lastUpdateTime = Time.fixedTime;
        }



        public void IgnoreColliders(Collider collider, bool ignore = true) {
            foreach(var col in grabColliders)
                Physics.IgnoreCollision(collider, col, ignore);
        }

        public void IgnoreColliders(Collider[] colliders, bool ignore = true)
        {
            foreach (var col in grabColliders)
                foreach (var col1 in colliders)
                    Physics.IgnoreCollision(col1, col, ignore);
        }
        public void IgnoreColliders(List<Collider> colliders, bool ignore = true)
        {
            foreach (var col in grabColliders)
                foreach (var col1 in colliders)
                    Physics.IgnoreCollision(col1, col, ignore);
        }


        void TryCreateHighlight(Material customMat, Hand hand)
        {

            var highlightMat = customMat != null ? customMat : hightlightMaterial;
            highlightMat = highlightMat != null ? highlightMat : hand.defaultHighlight;
            if (highlightMat != null && !highlightObjs.ContainsKey(highlightMat))
            {
                highlightObjs.Add(highlightMat, new List<GameObject>());
                AddHighlightObject(transform);


                bool AddHighlightObject(Transform obj)
                {

                    //This will stop the highlighting subsearch if there is another grabbable so that grabbable can create its own highlight settings/section
                    if (obj.CanGetComponent<Grabbable>(out var grab) && grab != this)
                        return false;
                    if((highlightObjs[highlightMat].Contains(obj.gameObject)))
                        return true;

                    for (int i = 0; i < obj.childCount; i++)
                    {
                        if (!AddHighlightObject(obj.GetChild(i)))
                            break;
                    }

                    MeshRenderer meshRenderer;
                    if (obj.CanGetComponent(out meshRenderer))
                    {
                        //Creates a slightly larger copy of the mesh and sets its material to highlight material
                        var highlightObj = new GameObject();
                        highlightObj.transform.parent = obj;
                        highlightObj.transform.localPosition = Vector3.zero;
                        highlightObj.transform.localRotation = Quaternion.identity;
                        highlightObj.transform.localScale = Vector3.one * 1.001f;
                        highlightObj.AddComponent<MeshFilter>().sharedMesh = obj.GetComponent<MeshFilter>().sharedMesh;
                        var highlightRenderer = highlightObj.AddComponent<MeshRenderer>();
                        var mats = new Material[meshRenderer.materials.Length];
                        for (int i = 0; i < mats.Length; i++)
                            mats[i] = highlightMat;
                        highlightRenderer.materials = mats;
                        highlightObjs[highlightMat].Add(highlightObj);
                    }

                    return true;
                }
            }

        }

        void ToggleHighlight(Hand hand, Material customMat, bool enableHighlight)
        {
            var highlightMat = customMat != null ? customMat : hightlightMaterial;
            highlightMat = highlightMat != null ? highlightMat : hand.defaultHighlight;
            if (highlightMat != null && highlightObjs.ContainsKey(highlightMat))
                for (int i = 0; i < highlightObjs[highlightMat].Count; i++)
                    highlightObjs[highlightMat][i].SetActive(enableHighlight);
        }

        /// <summary>Called when the hand starts aiming at this item for pickup</summary>
        protected internal virtual void Highlight(Hand hand, Material customMat = null, bool ignoreHighlightEvents = false) {
            if(!hightlighting) {
                hightlighting = true;
                if(!ignoreHighlightEvents && onHighlight != null)
                    onHighlight.Invoke(hand, this);
                if(!ignoreHighlightEvents && OnHighlightEvent != null)
                    OnHighlightEvent.Invoke(hand, this);
                TryCreateHighlight(customMat, hand);
                ToggleHighlight(hand, customMat, true);
            }
        }

        /// <summary>Called when the hand stops aiming at this item</summary>
        protected internal virtual void Unhighlight(Hand hand, Material customMat = null, bool ignoreHighlightEvents = false) {
            if(hightlighting) {
                if(!ignoreHighlightEvents && onUnhighlight != null)
                    onUnhighlight.Invoke(hand, this);
                if(!ignoreHighlightEvents && OnUnhighlightEvent != null)
                    OnUnhighlightEvent.Invoke(hand, this);
                hightlighting = false;
                ToggleHighlight(hand, customMat, false);
            }
        }





        /// <summary>Called by the hands Squeeze() function is called and this item is being held</summary>
        protected internal virtual void OnSqueeze(Hand hand) {
            OnSqueezeEvent?.Invoke(hand, this);
            onSqueeze?.Invoke(hand, this);
        }

        /// <summary>Called by the hands Unsqueeze() function is called and this item is being held</summary>
        protected internal virtual void OnUnsqueeze(Hand hand) {
            OnUnsqueezeEvent?.Invoke(hand, this);
            onUnsqueeze?.Invoke(hand, this);
        }

        /// <summary>Called by the hand when this item is started being grabbed</summary>
        protected internal virtual void OnBeforeGrab(Hand hand) {

            foreach(var collider in heldIgnoreColliders)
                hand.HandIgnoreCollider(collider, true);

            beingGrabbedBy.Add(hand);
            OnBeforeGrabEvent?.Invoke(hand, this);
            Unhighlight(hand, null);
            beingGrabbed = true;

            StartIgnoreRoutine(hand, false);
        }

        public bool IsOnlyBeingGrabbedBy(Hand hand) {
            return beingGrabbedBy.Contains(hand) && beingGrabbedBy.Count == 1;
        }


        protected void StartIgnoreRoutine(Hand hand, bool untilNone) {
            foreach(var grabbable in grabbableParents)
                if(grabbable.resetLayerRoutine.ContainsKey(hand)) {
                    if(grabbable.resetLayerRoutine[hand] != null)
                        grabbable.StopIgnoreRoutine(hand);
                    grabbable.resetLayerRoutine.Remove(hand);
                }

            foreach(var grabbable in grabbableChildren)
                if(grabbable.resetLayerRoutine.ContainsKey(hand)) {
                    if(grabbable.resetLayerRoutine[hand] != null)
                        grabbable.StopIgnoreRoutine(hand);
                    grabbable.resetLayerRoutine.Remove(hand);
                }

            if(resetLayerRoutine.ContainsKey(hand)) {
                if(resetLayerRoutine[hand] != null)
                    StopCoroutine(resetLayerRoutine[hand]);
                resetLayerRoutine.Remove(hand);
            }

            if(gameObject.activeInHierarchy) {
                if(untilNone)
                    resetLayerRoutine.Add(hand, StartCoroutine(IgnoreHandCollisionUntilNoneRoutine(hand, hand.maxGrabTime)));
                else
                    resetLayerRoutine.Add(hand, StartCoroutine(IgnoreHandCollision(hand, hand.maxGrabTime)));
            }

        }

        protected void StopIgnoreRoutine(Hand hand) {
            StopCoroutine(resetLayerRoutine[hand]);
        }

        /// <summary>Whether or not the hand can grab this grabbable</summary>
        public virtual bool CanGrab(HandBase hand) {
            return enabled && isGrabbable && (handType == HandType.both || (handType == HandType.left && hand.left) || (handType == HandType.right && !hand.left));
        }

        /// <summary>Called by the hand whenever this item is grabbed</summary>
        protected internal virtual void OnGrab(Hand hand) {

            if(beingGrabbedBy.Contains(hand))
                beingGrabbedBy.Remove(hand);

            if (rigidbodyDeactivated)
                ActivateRigidbody();

            if (lockHandOnGrab)
                hand.body.isKinematic = true;

            SetGrabbedRigidbodySettings();

            if(parentOnGrab) {
                rootTransform.parent = hand.transform.parent;
                foreach(var jointedBody in jointedBodies) {
                    jointedBody.transform.parent = hand.transform.parent;
                }
            }


            if(ignoreWeight) {
                if(!body.gameObject.CanGetComponent(out WeightlessFollower heldFollower))
                    heldFollower = body.gameObject.AddComponent<WeightlessFollower>();
                heldFollower?.Set(hand, this);
            }

            collisionTracker.enabled = true;

            heldBy?.Add(hand);
            placePoint?.Remove(this);
            onGrab?.Invoke(hand, this);
            OnGrabEvent?.Invoke(hand, this);

            wasForceReleased = false;
            beingGrabbed = false;
        }


        /// <summary>Called by the hand whenever this item is release</summary>
        protected internal virtual void OnRelease(Hand hand){

            if (heldBy.Contains(hand)) {
                bool canPlace = placePoint != null && placePoint.CanPlace(this);


                BreakHandConnection(hand);

                SetThrowVelocity(hand.ThrowVelocity(), hand.ThrowAngularVelocity());

                if(placePoint != null && canPlace)
                    placePoint.Place(this);



                OnReleaseEvent?.Invoke(hand, this);
                onRelease?.Invoke(hand, this);

                Unhighlight(hand, null);

            }
            else if(beingGrabbedBy.Contains(hand))
                hand.BreakGrabConnection();
        }

        /// <summary>Usually called through the release function. This function will release the connection to the hand and grabbable without calling the release events or applying throw force</summary>
        protected internal virtual void BreakHandConnection(Hand hand)
        {
            if(beingGrabbedBy.Contains(hand))
                beingGrabbedBy.Remove(hand);

            if (!heldBy.Remove(hand))
                return;

            ResetGrabbableAfterRlease();

            foreach(var collider in heldIgnoreColliders)
                hand.HandIgnoreCollider(collider, false);

            if (lockHandOnGrab)
                hand.body.isKinematic = false;

            if(ignoringHand.ContainsKey(hand))
                IgnoreHand(hand, false);

            if(gameObject.activeInHierarchy && !beingDestroyed)
                StartIgnoreRoutine(hand, true);

            if(beingGrabbedBy.Count == 0 && waitingToGrabHands.Count == 0)
                beingGrabbed = false;

            lastHeldBy = hand;
        }

        /// <summary>Tells each hand holding this object to release</summary>
        public virtual void HandsRelease() {
            for(int i = heldBy.Count - 1; i >= 0; i--)
                heldBy[i].Release();
        }

        /// <summary>Tells each hand holding this object to release</summary>
        public virtual void HandRelease(Hand hand) {
            if(heldBy.Contains(hand))
                hand.Release();
        }

        /// <summary>Forces all the hands on this object to relese without applying throw force or calling OnRelease event</summary>
        public virtual void ForceHandsRelease() {
            for(int i = waitingToGrabHands.Count - 1; i >= 0; i--) {
                waitingToGrabHands[i].BreakGrabConnection();
                waitingToGrabHands.RemoveAt(i);
            }

            for(int i = beingGrabbedBy.Count - 1; i >= 0; i--) {
                beingGrabbedBy[i].BreakGrabConnection();
            }

            for(int i = heldBy.Count - 1; i >= 0; i--) {
                wasForceReleased = true;
                ForceHandRelease(heldBy[i]);
            }
        }

        /// <summary>Forces all the hands on this object to relese without applying throw force</summary>
        public virtual void ForceHandRelease(Hand hand) {

            if(heldBy.Contains(hand)) {
                var throwMult = throwPower;
                throwPower = 0;
                wasForceReleased = true;
                hand.Release();
                throwPower = throwMult;
                if(body != null && !body.isKinematic)
                    body.velocity = body.velocity.normalized * Mathf.Clamp(body.velocity.magnitude, 0, 1);
            }
            else if(beingGrabbedBy.Contains(hand))
                hand.BreakGrabConnection();
        }


        /// <summary>Called when the joint between the hand and this item is broken\n - Works to simulate pulling item apart event</summary>
        public virtual void OnHandJointBreak(Hand hand) {
            if(heldBy.Contains(hand)) {
                if (body != null){
                    body.WakeUp();
                    body.velocity *= 0;
                    body.angularVelocity *= 0;
                }

                if(!pullApartBreakOnly) {
                    OnJointBreakEvent?.Invoke(hand, this);
                    OnJointBreak?.Invoke(hand, this);
                }
                if(pullApartBreakOnly && HeldCount() > 1) {
                    OnJointBreakEvent?.Invoke(hand, this);
                    OnJointBreak?.Invoke(hand, this);
                }

                ForceHandRelease(hand);

                if(heldBy.Count > 0)
                    heldBy[0].handFollow.SetHandLocation(heldBy[0].moveTo.position, heldBy[0].transform.rotation);
            }
        }

        //============================ GETTERS ============================
        //=================================================================
        //=================================================================


        /// <summary>Returns the list of hands that are currently holding this grabbables</summary>
        public List<Hand> GetHeldBy() {
            return heldBy;
        }

        /// <summary>Returns the list of hands that are currently holding this grabbables</summary>
        public List<Hand> GetHeldBy(bool includeChildGrabbables, bool includeParentrabbables) {
            List<Hand> hands = new List<Hand>();
            for(int i = 0; i < heldBy.Count; i++) {
                hands.Add(heldBy[i]);
            }

            if(includeChildGrabbables)
                for(int i = 0; i < grabbableChildren.Count; i++)
                    for(int j = 0; j < grabbableChildren[i].heldBy.Count; j++) 
                        hands.Add(grabbableChildren[i].heldBy[j]);

            if(includeParentrabbables)
                for(int i = 0; i < grabbableParents.Count; i++)
                    for(int j = 0; j < grabbableParents[i].heldBy.Count; j++)
                        hands.Add(grabbableParents[i].heldBy[j]);

            return hands;
        }

        /// <summary>Returns the hands local and held by jointed grabbables</summary>
        public List<Hand> GetJointedHeldBy()
        {
            List<Hand> hands = new List<Hand>();
            for (int i = 0; i < heldBy.Count; i++)
            {
                hands.Add(heldBy[i]);
            }
            for(int i = 0; i < jointedGrabbables.Count; i++) {
                for(int j = 0; j < jointedGrabbables[i].heldBy.Count; j++) {
                    hands.Add(jointedGrabbables[i].heldBy[j]);
                }
            }
            return hands;
        }


        /// <summary>Returns the number of hands currently holding this object [Call GetHeldBy() to get a list of the hand references]</summary>
        /// <param name="includedJointedCount">Whether or not to return the held count of only this grabbable, or the total of this grabbable and any jointed bodies with a grabbable attached</param>
        public virtual int HeldCount(bool includedJointedCount = true, bool includeChildGrabbables = true, bool includeParentrabbables = true) {
            var count = heldBy.Count;
            if(includedJointedCount)
                for(int i = 0; i < jointedGrabbables.Count; i++)
                    count += jointedGrabbables[i].HeldCount(false, true, true);

            if(includeChildGrabbables)
                for(int i = 0; i < grabbableChildren.Count; i++) 
                    count += grabbableChildren[i].HeldCount(false, false, false);

            if(includeParentrabbables)
                for(int i = 0; i < grabbableParents.Count; i++) 
                    count += grabbableParents[i].HeldCount(false, false, false);

            return count;
        }





        /// <summary>Returns true if this grabbable is currently being held</summary>
        public bool IsHeld() {
            return heldBy.Count > 0;
        }

        /// <summary>Returns true during hand grabbing coroutine</summary>
        public bool BeingGrabbed() {
            return beingGrabbed;
        }



        /// <summary>Plays haptic on each hand holding this grabbable</summary>
        public void PlayHapticVibration() {
            foreach(var hand in heldBy) {
                hand.PlayHapticVibration();
            }
        }

        /// <summary>Plays haptic on each hand holding this grabbable</summary>
        public void PlayHapticVibration(float duration = 0.025f) {
            foreach(var hand in heldBy) {
                hand.PlayHapticVibration(duration);
            }
        }

        /// <summary>Plays haptic on each hand holding this grabbable</summary>
        public void PlayHapticVibration(float duration, float amp = 0.5f) {
            foreach(var hand in heldBy) {
                hand.PlayHapticVibration(duration, amp);
            }
        }



        protected internal void SetThrowVelocity(Vector3 throwVel, Vector3 throwAngularVel) {
            if(body != null && !body.isKinematic && heldBy.Count == 0) {
                body.velocity = throwVel * throwPower;
                if(!float.IsNaN(throwAngularVel.x) && !float.IsNaN(throwAngularVel.y) && !float.IsNaN(throwAngularVel.z))
                    body.angularVelocity = throwAngularVel;
            }
        }


        /// <summary>Returns the velocity of the grabbable, old function, recommend using body.velocity instead</summary>
        public Vector3 GetVelocity() {
            if (body == null)
                return Vector3.zero;
            return lastCenterOfMassPos - body.position;
        }


        /// <summary>Returns the angular velocity of the grabbable, old function, recommend using body.velocity instead</summary>
        public Vector3 GetAngularVelocity() {
            Quaternion deltaRotation = body.rotation * Quaternion.Inverse(lastCenterOfMassRot);
            deltaRotation.ToAngleAxis(out var angle, out var axis);
            angle *= Mathf.Deg2Rad;
            return (1.0f / Time.fixedDeltaTime) * angle / 1.2f * axis;
        }


        /// <summary>
        /// Adds a grabbable as a child to this one.
        /// A grabbable child is usually applied automatically when a grabbable parented below this another grabbable. 
        /// This connects the grabbable and allows events to be called and settings to be changed
        /// </summary>
        public void AddChildGrabbable(Grabbable grab) {
            if(!grabbableChildren.Contains(grab))
                grabbableChildren.Add(grab);
        }

        /// <summary>
        /// Removes a grabbable as a child to this one.
        /// </summary>
        public void RemoveChildGrabbable(Grabbable grab) {
            if(grabbableChildren.Contains(grab))
                grabbableChildren.Remove(grab);
        }

        /// <summary>Add a jointed rigidbody to this grabbable, important for continuity between a held object and it's jointed bodies</summary>
        public void AddJointedBody(Rigidbody body)
        {
            if (!jointedBodies.Contains(body))
            {
                if(body.gameObject.HasGrabbable(out Grabbable otherGrabbable)) {
                    if (otherGrabbable.parentOnGrab && HeldCount() > 0 && otherGrabbable.HeldCount() == 0 && rootTransform.parent != originalParent) { 
                        otherGrabbable.rootTransform.parent = rootTransform.parent;
                    }

                    jointedParents.Add(otherGrabbable.originalParent);
                    if(!jointedGrabbables.Contains(otherGrabbable))
                        jointedGrabbables.Add(otherGrabbable);

                }
                else
                    jointedParents.Add(body.transform.parent);

                jointedBodies.Add(body);

            }
        }

        /// <summary>Remove a jointed rigidbody in the jointedBodies list</summary>
        public void RemoveJointedBody(Rigidbody body) {
            if (jointedBodies.Contains(body))
            {
                var i = jointedBodies.IndexOf(body);

                jointedBodies.RemoveAt(i);

                if (body.gameObject.HasGrabbable(out var otherGrabbable)) {
                    if(jointedGrabbables.Contains(otherGrabbable)) {
                        jointedGrabbables.Remove(otherGrabbable);
                        if(otherGrabbable.HeldCount(false, true, true) == 0) {
                            otherGrabbable.rootTransform.parent = otherGrabbable.originalParent;
                            otherGrabbable.ResetGrabbedRigidbodySettings();
                        }
                    }

                }
                else
                    body.transform.parent = jointedParents[i];

                if(HeldCount() == 0 && rootTransform.parent != originalParent && parentOnGrab) {
                    ResetGrabbableAfterRlease();
                }

                jointedParents.RemoveAt(i);
            }
        }

        public void DoDestroy() {
            Destroy(gameObject);
        }

        /// <summary>Returns the total collision count of all this grabbable</summary>
        public int CollisionCount() {
            return collisionTracker.collisionObjects.Count;
        }

        /// <summary>Returns the total collision count of all the "jointed grabbables"</summary>
        public int JointedCollisionCount() {
            int count = 0;
            for(int i = 0; i < jointedGrabbables.Count; i++)
                count += jointedGrabbables[i].HeldCount();

            return count;
        }


        /// <summary>Sets the given place point to ignore this grabbables and all it's children</summary>
        public void PlacePointIgnore(PlacePoint point) {
            point.dontAllows.Add(rootGrabbable);
            foreach(var grabbable in rootGrabbable.grabbableChildren) {
                if(grabbable != this)
                    point.dontAllows.Add(grabbable);
            }
        }


        /// <summary>Sets the given place point to allow this grabbables and all it's children</summary>
        public void PlacePointAllow(PlacePoint point) {
            if(point.dontAllows.Contains(rootGrabbable))
                point.dontAllows.Remove(rootGrabbable);
            foreach(var grabbable in rootGrabbable.grabbableChildren) {
                if(grabbable != this && point.dontAllows.Contains(grabbable))
                    point.dontAllows.Remove(grabbable);
            }
        }


        //Adds a reference script to child colliders so they can be grabbed
        void MakeChildrenGrabbable() {
            for(int i = 0; i < transform.childCount; i++) {
                AddChildGrabbableRecursive(transform.GetChild(i));
            }

            void AddChildGrabbableRecursive(Transform obj) {
                if(obj.CanGetComponent(out Collider col) && col.isTrigger == false && !obj.CanGetComponent<IGrabbableEvents>(out _) && !obj.CanGetComponent<GrabbableChild>(out _) && !obj.CanGetComponent<PlacePoint>(out _)) {
                    var child = obj.gameObject.AddComponent<GrabbableChild>();
                    child.grabParent = this;
                }
                for(int i = 0; i < obj.childCount; i++) {
                    if(!obj.CanGetComponent<Grabbable>(out _))
                        AddChildGrabbableRecursive(obj.GetChild(i));
                }
            }
        }


        //Adds a reference script to child colliders so they can be grabbed
        void MakeChildrenUngrabbable() {
            for(int i = 0; i < transform.childCount; i++) {
                RemoveChildGrabbableRecursive(transform.GetChild(i));
            }

            void RemoveChildGrabbableRecursive(Transform obj) {
                if(obj.GetComponent<GrabbableChild>() && obj.GetComponent<GrabbableChild>().grabParent == this) {
                    Destroy(obj.gameObject.GetComponent<GrabbableChild>());
                }
                for(int i = 0; i < obj.childCount; i++) {
                    RemoveChildGrabbableRecursive(obj.GetChild(i));
                }
            }
        }


        bool rigidbodyGrabbedState;
        internal void SetGrabbedRigidbodySettings() {
            if(rigidbodyGrabbedState == false) {
                rigidbodyGrabbedState = true;

                if(body != null) {
                    body.collisionDetectionMode = body.isKinematic ? CollisionDetectionMode.ContinuousSpeculative : CollisionDetectionMode.ContinuousDynamic;
                    body.interpolation = RigidbodyInterpolation.None;
                    body.solverIterations = 100;
                    body.solverVelocityIterations = 100;

                    if(targetDrag == 0 && body.drag < minHeldDrag) {
                        body.drag = minHeldDrag;
                    }
                    if(targetAngularDrag == 0 && body.angularDrag < minHeldAngleDrag) {
                        body.angularDrag = minHeldAngleDrag;
                    }

                    if(targetMass == 0 && body.mass < minHeldMass) {
                        body.mass = minHeldMass;
                    }


                    for(int i = 0; i < jointedBodies.Count; i++) {
                        if(jointedBodies[i] != null && jointedBodies[i].gameObject.HasGrabbable(out var grab) && grab != this) {
                            grab.SetGrabbedRigidbodySettings();
                        }
                    }

                    if(heldNoFriction) {
                        var colliderMat = Resources.Load<PhysicMaterial>("NoFriction");
                        SetPhysicsMaterial(colliderMat);
                    }

                }
            }
        }


        //Resets to original collision dection
        protected void ResetGrabbedRigidbodySettings() {
            if(rigidbodyGrabbedState) {
                if(body != null) {
                    body.collisionDetectionMode = detectionMode;
                    body.interpolation = startInterpolation;
                    body.solverIterations = Physics.defaultSolverIterations;
                    body.solverVelocityIterations = Physics.defaultSolverVelocityIterations;
                    body.drag = targetDrag;
                    body.angularDrag = targetAngularDrag;
                    body.mass = targetMass;

                    if(heldNoFriction) {
                        ResetPhysicsMateiral();
                    }
                }

                rigidbodyGrabbedState = false;
                collisionTracker.enabled = false;
            }
        }


        /// <summary>INTERNAL - Sets the grabbables original layers</summary>
        protected internal void ResetGrabbableAfterRlease() {
            if(!beingDestroyed) {

                if(HeldCount() == 0 && gameObject.activeInHierarchy && parentOnGrab && (placePoint == null || !(placePoint.placedObject == this && placePoint.parentOnPlace))) {
                    rootTransform.parent = originalParent;
                }

                if(HeldCount() == 0) {
                    ResetGrabbedRigidbodySettings();
                }

                for(int i = 0; i < jointedBodies.Count; i++) {
                    if(jointedBodies[i].gameObject.HasGrabbable(out var grab)) {
                        if(grab.HeldCount() == 0) {
                            grab.rootTransform.parent = grab.originalParent;
                            grab.ResetGrabbedRigidbodySettings();
                        }
                        else if(parentOnGrab && grab.rootTransform.parent != grab.originalParent) {
                            rootTransform.parent = grab.rootTransform.parent;
                        }
                    }
                    else
                        jointedBodies[i].transform.parent = jointedParents[i];
                }
            }
        }

        public bool IsHolding(Rigidbody body)
        {

            foreach (var holding in heldBy)
            {
                if (holding.body == body)
                    return true;
            }

            return false;
        }

        public bool IsHolding(Hand hand)
        {
            foreach (var held in heldBy)
            {
                if (held == hand)
                    return true;
            }

            return false;
        }

        public virtual void OnHighlight(Hand hand) {
            Highlight(hand);
        }

        public virtual void OnUnhighlight(Hand hand) {
            OnUnhighlight(hand);
        }

        void IGrabbableEvents.OnGrab(Hand hand) {
            OnGrab(hand);
        }

        void IGrabbableEvents.OnRelease(Hand hand) {
            OnRelease(hand);
        }

        public Grabbable GetGrabbable() {
            return this;
        }

        public bool CanGrab(Hand hand) {
            if (hand == null)
                return false;
            return CanGrab(hand as HandBase);
        }


        internal void AddWaitingForGrab(Hand hand) {
            waitingToGrabHands.Add(hand);
        }

        internal void RemoveWaitingForGrab(Hand hand) {
            waitingToGrabHands.Remove(hand);
        }

    }
}
