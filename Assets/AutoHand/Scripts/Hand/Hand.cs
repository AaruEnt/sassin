using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.XR;

namespace Autohand {


    [HelpURL("https://app.gitbook.com/s/5zKO0EvOjzUDeT2aiFk3/auto-hand/hand"), DefaultExecutionOrder(10)]
    public class Hand : HandBase {


        [AutoToggleHeader("Enable Highlight", 0, 0, tooltip = "Raycasting for grabbables to highlight is expensive, you can disable it here if you aren't using it")]
        public bool usingHighlight = true;

        [EnableIf("usingHighlight")]
        [Tooltip("The layers to highlight and use look assist on --- Nothing will default on start")]
        public LayerMask highlightLayers;

        [EnableIf("usingHighlight")]
        [Tooltip("Leave empty for none - used as a default option for all grabbables with empty highlight material")]
        public Material defaultHighlight;


        [AutoSmallHeader("Advanced Settings")]
        public bool showAdvanced = false;

        [Tooltip("The hand will automatically apply a no friction physics material to each hand collider on start")]
        public bool noHandFriction = true;

        [Tooltip("Any layers in this mask will be removed from the spherecast checking if a grab is possible: " +
            "IMPORTANT!!! This does not only apply to grabbables, any layers included in  this mask will be completely ignored meaning the hand can grab and highlight objects through with these layers")]
        public LayerMask ignoreGrabCheckLayers;

        [Tooltip("Whether the hand should go to the object and come back on grab, or the object to float to the hand on grab. Will default to HandToGrabbable for objects that have \"parentOnGrab\" disabled")]
        public GrabType grabType = GrabType.HandToGrabbable;

        [Tooltip("The animation curve based on the grab time 0-1"), Min(0)]
        public AnimationCurve grabCurve;

        [Tooltip("Makes grab smoother; also based on range and reach distance - a very near grab is minGrabTime and a max distance grab is maxGrabTime"), Min(0)]
        public float minGrabTime = 0.04f;

        [Tooltip("Makes grab smoother; also based on range and reach distance - a very near grab is minGrabTime and a max distance grab is maxGrabTime"), Min(0)]
        public float maxGrabTime = 0.33f;


        [Tooltip("Increasing this value will make grabbing faster based on controller velocity during grab. Setting this value to 0 will disable this feature. When grabbing an object the speed it takes for the hand to reach the object is decreased based on the velocity of the controller while grabbing"), Min(0)]
        public float velocityGrabHandAmplifier = 600;

        [Tooltip("Increasing this value will make grabbing faster based on grab target velocity during grab. Setting this value to 0 will disable this feature. When grabbing an object the speed it takes for the hand to reach the object is decreased based on the velocity of the controller while grabbing"), Min(0)]
        public float velocityGrabObjectAmplifier = 100;

        [Tooltip("The point along the grab time 0-1 where the hand has been transitioned from whatever pose it was when grabbing to its open hand pose"), Range(0, 1)]
        public float grabOpenHandPoint = 0.5f;

        [Tooltip("This is used in conjunction with custom poses. For a custom pose to work it must has the same PoseIndex as the hand. Used for when your game has multiple hands")]
        public int poseIndex = 0;

        [AutoLine]
        public bool ignoreMe1;





#if UNITY_EDITOR
        bool editorSelected = false;
#endif


        public static string[] grabbableLayers = { "Grabbable", "Grabbing" };

        //The layer is used and applied to all grabbables in if the hands layer is set to default
        public static string grabbableLayerNameDefault = "Grabbable";
        //This helps the auto grab distinguish between what item is being grabbaed and the items around it
        public static string grabbingLayerName = "Grabbing";

        //This was added by request just in case you want to add different layers for left/right hand
        public static string rightHandLayerName = "Hand";
        public static string leftHandLayerName = "Hand";



        ///Events for all my programmers out there :)/// 
        /// <summary>Called when the grab event is triggered, event if nothing is being grabbed</summary>
        public event HandGrabEvent OnTriggerGrab;
        /// <summary>Called at the very start of a grab before anything else</summary>
        public event HandGrabEvent OnBeforeGrabbed;
        /// <summary>Called when the hand grab connection is made (the frame the hand touches the grabbable)</summary>
	    public event HandGrabEvent OnGrabbed;

        /// <summary>Called when the release event is triggered, event if nothing is being held</summary>
        public event HandGrabEvent OnTriggerRelease;
        public event HandGrabEvent OnBeforeReleased;
        /// <summary>Called at the end the release</summary>
        public event HandGrabEvent OnReleased;

        /// <summary>Called when the squeeze button is pressed, regardless of whether an object is held or not (grab returns null)</summary>
        public event HandGrabEvent OnSqueezed;
        /// <summary>Called when the squeeze button is released, regardless of whether an object is held or not (grab returns null)</summary>
        public event HandGrabEvent OnUnsqueezed;


        /// <summary>Called whenever joint breaks or force release event is called</summary>
        public event HandGrabEvent OnForcedRelease;
        /// <summary>Called when the physics joint between the hand and the grabbable is broken by force</summary>
        public event HandGrabEvent OnGrabJointBreak;

        /// <summary>Legacy Event - same as OnRelease</summary>
        public event HandGrabEvent OnHeldConnectionBreak;

        public event HandGameObjectEvent OnHandCollisionStart;
        public event HandGameObjectEvent OnHandCollisionStop;
        public event HandGameObjectEvent OnHandTriggerStart;
        public event HandGameObjectEvent OnHandTriggerStop;

        public Hand copyFromHand;

        public Grabbable lastHoldingObj { get; private set; }

        public Grabbable lookingAtObj { get { return highlighter.currentHighlightTarget; } }
        public Vector3 lastFollowPosition { get { return handFollow.lastFrameFollowPosition; }  }
        public Quaternion lastFollowRotation { get { return handFollow.lastFrameFollowRotation; } }



        List<HandTriggerAreaEvents> triggerEventAreas = new List<HandTriggerAreaEvents>();

        float startGrabDist;
        Vector3 startHandLocalGrabPosition;


        Coroutine _grabRoutine;
        Coroutine grabRoutine {
            get { return _grabRoutine; }
            set {
                if(value != null && _grabRoutine != null) {
                    StopCoroutine(_grabRoutine);
                    if(holdingObj != null) {
                        holdingObj.body.velocity = Vector3.zero;
                        holdingObj.body.angularVelocity = Vector3.zero;
                        holdingObj.beingGrabbed = false;
                    }
                    BreakGrabConnection();
                }
                _grabRoutine = value;
            }
        }


        protected override void Awake() {
            this.SetLayerRecursive(transform, LayerMask.NameToLayer(left ? Hand.leftHandLayerName : Hand.rightHandLayerName));

            if(highlightLayers.value == 0 || highlightLayers == LayerMask.GetMask("")) {
                highlightLayers = LayerMask.GetMask(grabbableLayerNameDefault);
            }

            handLayers = LayerMask.GetMask(rightHandLayerName, leftHandLayerName, AutoHandPlayer.HandPlayerLayer);
            handIgnoreCollisionLayers = AutoHandExtensions.GetPhysicsLayerMask(LayerMask.NameToLayer(rightHandLayerName)) & AutoHandExtensions.GetPhysicsLayerMask(LayerMask.NameToLayer(leftHandLayerName));

            if(grabCurve == null || grabCurve.keys.Length == 0)
                grabCurve = AnimationCurve.Linear(0, 0, 1, 1);

            base.Awake();
        }


        protected virtual void Start()
        {
            if(noHandFriction) {
                var noFrictionMat = Resources.Load<PhysicMaterial>("NoFriction");
                foreach(var collider in handColliders) {
                    collider.material = noFrictionMat;
                }
            }

#if UNITY_EDITOR
            if (Selection.activeGameObject == gameObject)
            {
                Selection.activeGameObject = null;
                Debug.Log("Auto Hand: highlighting hand component in the inspector can cause lag and quality reduction at runtime in VR. (Automatically deselecting at runtime) Remove this code at any time.", this);
                editorSelected = true;
            }

            Application.quitting += () => { if (editorSelected && Selection.activeGameObject == null) Selection.activeGameObject = gameObject; };
#endif
        }


        protected override void OnEnable() {
            base.OnEnable();
            collisionTracker.OnCollisionFirstEnter += OnCollisionFirstEnter;
            collisionTracker.OnCollisionLastExit += OnCollisionLastExit;
            collisionTracker.OnTriggerFirstEnter += OnTriggerFirstEnter;
            collisionTracker.OnTriggerLastExit += OnTriggerLastExit;

            collisionTracker.OnCollisionFirstEnter += OnCollisionFirstEnterEvent;
            collisionTracker.OnCollisionLastExit += OnCollisionLastExitEvent;
            collisionTracker.OnTriggerFirstEnter += OnTriggerFirstEnterEvent;
            collisionTracker.OnTriggerLastExit += OnTriggeLastExitEvent;

        }

        protected override void OnDisable() {
            foreach(var trigger in triggerEventAreas)
                trigger.Exit(this);

            base.OnDisable();
            collisionTracker.OnCollisionFirstEnter -= OnCollisionFirstEnter;
            collisionTracker.OnCollisionLastExit -= OnCollisionLastExit;
            collisionTracker.OnTriggerFirstEnter -= OnTriggerFirstEnter;
            collisionTracker.OnTriggerLastExit -= OnTriggerLastExit;

            collisionTracker.OnCollisionFirstEnter -= OnCollisionFirstEnterEvent;
            collisionTracker.OnCollisionLastExit -= OnCollisionLastExitEvent;
            collisionTracker.OnTriggerFirstEnter -= OnTriggerFirstEnterEvent;
            collisionTracker.OnTriggerLastExit -= OnTriggeLastExitEvent;
        }




        //================================================================
        //================== CORE INTERACTION FUNCTIONS ==================
        //================================================================


        /// <summary>Whether or not this hand can grab the grabbbale based on hand and grabbable settings</summary>
        public bool CanGrab(Grabbable grab) {
            if(grab == null)
                return false;

            var cantHandSwap = (grab.IsHeld() && grab.singleHandOnly && !grab.allowHeldSwapping);
            return (!IsGrabbing() && !cantHandSwap) && grab.CanGrab(this);
        }

        /// <summary>Function for controller trigger fully pressed -> Grabs whatever is directly in front of and closest to the hands palm (by default this is called by the hand controller link component)</summary>
        public virtual void Grab() {
            Grab(grabType);
        }

        /// <summary>Function for controller trigger fully pressed -> Grabs whatever is directly in front of and closest to the hands palm</summary>
        public virtual void Grab(GrabType grabType) {
            OnTriggerGrab?.Invoke(this, null);
            foreach(var triggerArea in triggerEventAreas) {
                triggerArea.Grab(this);
            }
            if(usingHighlight && !grabbing && holdingObj == null && highlighter.currentHighlightTarget != null) {
                
                grabType = GetGrabType(highlighter.currentHighlightTarget);
                grabRoutine = StartCoroutine(GrabObject(highlighter.GetHighlightHit(), highlighter.currentHighlightTarget, grabType));
            }
            else if(!grabbing && holdingObj == null) {
                highlighter.UpdateHighlight(true, true);
                if(highlighter.currentHighlightTarget != null) {

                    grabType = GetGrabType(highlighter.currentHighlightTarget);
                    grabRoutine = StartCoroutine(GrabObject(highlighter.GetHighlightHit(), highlighter.currentHighlightTarget, grabType));
                }

                highlighter.ClearHighlights();
            }

            else if(holdingObj != null && holdingObj.CanGetComponent(out GrabLock grabLock)) {
                grabLock.OnGrabPressed?.Invoke(this, holdingObj);
            }
        }

        /// <summary>Grabs based on raycast and grab input data</summary>
        public virtual void Grab(RaycastHit hit, Grabbable grab, GrabType grabType = GrabType.InstantGrab) {
            bool objectFree = grab.body.isKinematic != true && grab.body.constraints == RigidbodyConstraints.None;
            if(!grabbing && holdingObj == null && this.CanGrab(grab) && objectFree) {
                grabRoutine = StartCoroutine(GrabObject(hit, grab, grabType));
            }
        }

        GrabType GetGrabType(Grabbable grabbable) {
            GrabType grabType = this.grabType;
            if(grabbable.instantGrab)
                grabType = GrabType.InstantGrab;
            else if(grabbable.grabType != HandGrabType.Default) {
                switch(grabbable.grabType) {
                    case HandGrabType.HandToGrabbable:
                        grabType = GrabType.HandToGrabbable;
                        break;
                    case HandGrabType.GrabbableToHand:
                        grabType = GrabType.GrabbableToHand;
                        break;
                }
            }

            return grabType;
        }

        /// <summary>Grab a given grabbable</summary>
        public virtual void TryGrab(Grabbable grab) {
            ForceGrab(grab);
        }


        /// <summary>Alwyas grab a given grabbable, only works if grab is possible will automaticlly Instantiate a new copy of the given grabbable if using a prefab reference</summary>
        public virtual void ForceGrab(Grabbable grab, bool createCopy = false) {
            if(createCopy || !grab.gameObject.scene.IsValid())
                grab = Instantiate(grab);

            RaycastHit closestHit = new RaycastHit();
            closestHit.distance = float.MaxValue;
            if(!grabbing && holdingObj == null && this.CanGrab(grab)) {
                if(GetClosestGrabbableHit(grab, out closestHit))
                    Grab(closestHit, grab, GrabType.InstantGrab);
            }
        }

        /// <summary>Function for controller trigger unpressed (by default this is called by the hand controller link component)</summary>
        public virtual void Release() {
            OnTriggerRelease?.Invoke(this, null);

            foreach(var triggerArea in triggerEventAreas) {
                triggerArea.Release(this);
            }

            if(holdingObj && !holdingObj.wasForceReleased && holdingObj.CanGetComponent<GrabLock>(out GrabLock grablock) && grablock.enabled)
                return;

            if(holdingObj != null) {
                OnBeforeReleased?.Invoke(this, holdingObj);
                holdingObj.OnBeforeReleaseEvent?.Invoke(this, holdingObj);
                holdingObj.OnRelease(this);
                handFollow.ignoreMoveFrame = true;
            }

            BreakGrabConnection();
        }

        /// <summary>This will force release the hand without throwing or calling OnRelease\n like losing grip on something instead of throwing</summary>
        public virtual void ForceReleaseGrab() {
            if(holdingObj != null) {
                OnForcedRelease?.Invoke(this, holdingObj);
                holdingObj?.ForceHandRelease(this);
            }
        }

        /// <summary>Old function left for backward compatability -> Will release grablocks, recommend using ForceReleaseGrab() instead</summary>
        public virtual void ReleaseGrabLock() {
            ForceReleaseGrab();
        }

        /// <summary>Event for controller grip (by default this is called by the hand controller link component)</summary>
        public virtual void Squeeze() {
            OnSqueezed?.Invoke(this, holdingObj);
            holdingObj?.OnSqueeze(this);

            foreach(var triggerArea in triggerEventAreas)
                triggerArea.Squeeze(this);

            squeezing = true;
        }

        /// <summary>Returns the grab axis value from zero to one, (by default this is set by the hand controller link)</summary>
        public virtual float GetGripAxis() {
            return gripAxis;
        }

        /// <summary>Returns the squeeze value from zero to one, (by default this is set by the hand controller link)</summary>
        public float GetSqueezeAxis() {
            return squeezeAxis;
        }

        /// <summary>Event for controller ungrip</summary>
        public virtual void Unsqueeze() {
            squeezing = false;
            OnUnsqueezed?.Invoke(this, holdingObj);
            holdingObj?.OnUnsqueeze(this);

            foreach(var triggerArea in triggerEventAreas) {
                triggerArea.Unsqueeze(this);
            }
        }

        /// <summary>Breaks the grab event without calling the release functions and events</summary>
        public virtual void BreakGrabConnection(bool callEvent = true) {

            if(holdingObj != null) {
                if(squeezing)
                    holdingObj.OnUnsqueeze(this);

                if(grabbing) {
                    if (holdingObj.body != null){
                        holdingObj.body.velocity = Vector3.zero;
                        holdingObj.body.angularVelocity = Vector3.zero;
                    }
                }

                if(holdingObj.HeldCount() > 1)
                    ResetGrabOffset();

                if(holdingObj.ignoreReleaseTime == 0) {
                    transform.position = holdingObj.transform.InverseTransformPoint(startHandLocalGrabPosition);
                    body.position = transform.position;
                }

                holdingObj.BreakHandConnection(this);
                lastHoldingObj = holdingObj;
                lastReleaseTime = Time.time;
                holdingObj = null;
                OnHeldConnectionBreak?.Invoke(this, lastHoldingObj);
                OnReleased?.Invoke(this, lastHoldingObj);
            }
            else if(grabRoutine != null) {
                StopCoroutine(grabRoutine);
            }

            velocityTracker.Disable(throwVelocityExpireTime);
            currentHeldPose = null;
            grabRoutine = null;

            handAnimator.CancelPose(0.05f);

            if(heldJoint != null) {
                Destroy(heldJoint);
                heldJoint = null;
            }
        }



        /// <summary>Creates the grab connection at the current position of the hand and given grabbable</summary>
        public virtual void CreateGrabConnection(Grabbable grab, bool executeGrabEvents = false) {
            CreateGrabConnection(grab, transform.position, transform.rotation, grab.transform.position, grab.transform.rotation, executeGrabEvents);
        }

        /// <summary>Creates the grab connection</summary>
        public virtual void CreateGrabConnection(Grabbable grab, Vector3 handPos, Quaternion handRot, Vector3 grabPos, Quaternion grabRot, bool executeGrabEvents = false, bool ignorePoses = false) {

            if(executeGrabEvents) {
                OnBeforeGrabbed?.Invoke(this, grab);
                grab.OnBeforeGrab(this);
            }

            transform.position = handPos;
            body.position = handPos;
            transform.rotation = handRot;
            body.rotation = handRot;

            if(grab.body == null)
                grab.ActivateRigidbody();

            grab.transform.position = grabPos;
            grab.body.position = grabPos;
            grab.transform.rotation = grabRot;
            grab.body.rotation = grabRot;

            handGrabPoint.parent = grab.transform;
            handGrabPoint.position = handPos;
            handGrabPoint.rotation = handRot;


            startGrabDist = Vector3.Distance(palmTransform.position, handGrabPoint.position);
            startHandLocalGrabPosition = grab.transform.InverseTransformPoint(transform.position);

            holdingObj = grab;

            localGrabbablePoint.transform.position = holdingObj.rootTransform.position;
            localGrabbablePoint.transform.rotation = holdingObj.rootTransform.rotation;

            if(!(holdingObj.grabType == HandGrabType.GrabbableToHand) && !(grabType == GrabType.GrabbableToHand)) {
                ResetGrabOffset();
            }

            //If it's a predetermined Pose
            if(!ignorePoses && holdingObj.GetSavedPose(out var poseCombiner)) {
                if(poseCombiner.CanSetPose(this, holdingObj)) {
                    currentHeldPose = poseCombiner.GetClosestPose(this, holdingObj);
                    currentHeldPose.SetHandPose(this);
                }
            }


            //Hand Swap - One Handed Items
            if(holdingObj.singleHandOnly && holdingObj.HeldCount(false, false, false) > 0) {
                holdingObj.ForceHandRelease(holdingObj.GetHeldBy()[0]);
                if(holdingObj.body != null) {
                    holdingObj.body.velocity = Vector3.zero;
                    holdingObj.body.angularVelocity = Vector3.zero;
                }
            }

            handAnimator.targetGrabPose.SavePose(this, holdingObj.transform);

            if(executeGrabEvents) {
                OnGrabbed?.Invoke(this, holdingObj);
                holdingObj.OnGrab(this);
            }

            grabbing = false;
            grabRoutine = null;

            CreateJoint(holdingObj, holdingObj.jointBreakForce, float.PositiveInfinity);
        }


        /// <summary>Creates Joints between hand and grabbable, does not call grab events</summary>
        public virtual void CreateJoint(Grabbable grab) {
            CreateJoint(grab, grab.jointBreakForce, float.PositiveInfinity);
        }

        /// <summary>Creates Joints between hand and grabbable, does not call grab events</summary>
        public virtual void CreateJoint(Grabbable grab, float breakForce, float breakTorque) {
            if(grab.customGrabJoint == null) {
                var jointCopy = (Resources.Load<ConfigurableJoint>("DefaultJoint"));
                var newJoint = gameObject.AddComponent<ConfigurableJoint>().GetCopyOf(jointCopy);
                newJoint.anchor = Vector3.zero;
                newJoint.breakForce = breakForce;
                if(grab.HeldCount() == 1)
                    newJoint.breakForce += 500;
                newJoint.breakTorque = breakTorque;
                newJoint.connectedBody = grab.body;
                newJoint.enablePreprocessing = jointCopy.enablePreprocessing;
                newJoint.autoConfigureConnectedAnchor = false;
                newJoint.connectedAnchor = grab.body.transform.InverseTransformPoint(handGrabPoint.position);
                newJoint.angularXMotion = jointCopy.angularXMotion;
                newJoint.angularYMotion = jointCopy.angularYMotion;
                newJoint.angularZMotion = jointCopy.angularZMotion;

                heldJoint = newJoint;
            }
            else {
                var newJoint = grab.body.gameObject.AddComponent<ConfigurableJoint>().GetCopyOf(grab.customGrabJoint);
                newJoint.anchor = Vector3.zero;
                if(grab.HeldCount() == 1)
                    newJoint.breakForce += 500;
                newJoint.breakForce = breakForce;
                newJoint.breakTorque = breakTorque;
                newJoint.connectedBody = body;
                newJoint.enablePreprocessing = grab.customGrabJoint.enablePreprocessing;
                newJoint.autoConfigureConnectedAnchor = false;
                newJoint.connectedAnchor = grab.body.transform.InverseTransformPoint(handGrabPoint.position);
                newJoint.angularXMotion = grab.customGrabJoint.angularXMotion;
                newJoint.angularYMotion = grab.customGrabJoint.angularYMotion;
                newJoint.angularZMotion = grab.customGrabJoint.angularZMotion;
                heldJoint = newJoint;
            }
        }


        public virtual void OnJointBreak(float breakForce) {
            if(heldJoint != null) {
                Destroy(heldJoint);
                heldJoint = null;
            }
            if(holdingObj != null) {
                holdingObj.body.velocity /= 100f;
                holdingObj.body.angularVelocity /= 100f;
                OnGrabJointBreak?.Invoke(this, holdingObj);
                holdingObj?.OnHandJointBreak(this);
            }
        }




        //=================================================================
        //======================== GETTERS AND SETTERS ====================
        //=================================================================

        /// <summary>Takes a raycasthit and grabbable and automatically poses the hand</summary>
        public void AutoPose(RaycastHit hit, Grabbable grabbable) {
            var grabbingLayer = LayerMask.NameToLayer(Hand.grabbingLayerName);
            grabbable.SetLayerRecursive(grabbingLayer);

            Transform palmTransform = this.palmTransform;

            if(grabbable.grabPoseType == HandGrabPoseType.Pinch) {
                palmTransform = pinchPointTransform;
            }

            Vector3 palmLocalPos = palmTransform.localPosition;
            Quaternion palmLocalRot = palmTransform.localRotation;

            Vector3 hitColliderPosition = hit.collider.transform.position;
            Quaternion hitColliderRotation = hit.collider.transform.rotation;

            var palmColliderTransform = palmCollider.transform;

            var handTransform = transform;

            palmCollider.enabled = true;
            for(int i = 0; i < 12; i++)
                Calculate();
            palmCollider.enabled = false;

            void Calculate() {
                Align();

                var grabDir = hit.point - palmTransform.position;
                handTransform.position += grabDir;
                body.position = handTransform.position;

                if(Physics.ComputePenetration(hit.collider, hitColliderPosition, hitColliderRotation,
                    palmCollider, palmColliderTransform.position, palmColliderTransform.rotation, 
                    out var dir, out var dist)) {
                        handTransform.position -= dir * dist / 2f;
                        body.position = handTransform.position;
                }

                handTransform.position -= palmTransform.forward * grabDir.magnitude / 3f;
                body.position = handTransform.position;
            }

            void Align() {
                palmChild.position = handTransform.position;
                palmChild.rotation = handTransform.rotation;

                palmTransform.LookAt(hit.point, palmTransform.up);

                handTransform.position = palmChild.position;
                handTransform.rotation = palmChild.rotation;

                palmTransform.localPosition = palmLocalPos;
                palmTransform.localRotation = palmLocalRot;
            }

            var mask = LayerMask.GetMask(Hand.grabbingLayerName);
            if(grabbable.grabPoseType == HandGrabPoseType.Grab)
                foreach(var finger in fingers)
                    finger.BendFingerUntilHit(fingerBendSteps, mask);
            else
                foreach(var finger in fingers) {
                    if(!finger.BendFingerUntilHit(fingerBendSteps, mask, FingerPoseEnum.PinchOpen, FingerPoseEnum.PinchClosed))
                        finger.BendFingerUntilHit(fingerBendSteps, mask);
                }

            grabbable.ResetOriginalLayers();
        }


        public void BendFingersUntilHit(Grabbable grabbable) {
            var grabbingLayer = LayerMask.NameToLayer(Hand.grabbingLayerName);
            grabbable.SetLayerRecursive(grabbingLayer);

            var mask = LayerMask.GetMask(Hand.grabbingLayerName);
            if(grabbable.grabPoseType == HandGrabPoseType.Grab) {
                foreach(var finger in fingers)
                    finger.BendFingerUntilHit(fingerBendSteps, mask);
            }
            else {
                foreach(var finger in fingers) {
                    if(!finger.BendFingerUntilHit(fingerBendSteps, mask, FingerPoseEnum.PinchOpen, FingerPoseEnum.PinchClosed))
                        finger.BendFingerUntilHit(fingerBendSteps, mask);
                }
            }

            grabbable.ResetOriginalLayers();
        }

        /// <summary>Recalculates the grab point for the grabbing pose - should only be called in an OnBeforeGrab event</summary>
        public void RecalculateBeforeGrab(Grabbable grab) {
            if(GetClosestGrabbableHit(grab, out var closestHit)) {
                grabbingHit = closestHit;
            }
        }

        /// <summary>Recalulate Held Auto Pose - should only be called while holding an object</summary>
        public void RecaculateHeldAutoPose() {
            if(holdingObj != null && currentHeldPose == null) {
                transform.position -= palmTransform.forward * reachDistance;
                body.position = transform.position;
                var didHit = GetClosestGrabbableHit(holdingObj, out var closestHit);

                if(didHit) {
                    AutoPose(closestHit, holdingObj);
                    handGrabPoint.position = transform.position;
                    handGrabPoint.rotation = transform.rotation;
                    localGrabbablePoint.position = holdingObj.rootTransform.position;
                    localGrabbablePoint.rotation = holdingObj.rootTransform.rotation;
                    heldJoint.connectedAnchor = holdingObj.body.transform.InverseTransformPoint(handGrabPoint.position);
                    handAnimator.targetGrabPose.SavePose(this, holdingObj.transform);
                }
                else {
                    transform.position += palmTransform.forward * reachDistance;
                    body.position = transform.position;
                }
            }
        }


        /// <summary>Returns the current held object - null if empty (Same as GetHeld())</summary>
        public Grabbable GetHeldGrabbable() {
            return holdingObj;
        }

        /// <summary>Returns the current held object - null if empty (Same as GetHeldGrabbable())</summary>
        public Grabbable GetHeld() {
            return holdingObj;
        }

        /// <summary>Returns true if squeezing has been triggered</summary>
        public bool IsSqueezing() {
            return squeezing;
        }



        //=================================================================
        //========================= HELPER FUNCTIONS ======================
        //=================================================================



        /// <summary>Resets the grab offset created on grab for a smoother hand return</summary>
        public void ResetGrabOffset() {
            grabPositionOffset = transform.position - follow.transform.position;
            grabRotationOffset = Quaternion.Inverse(follow.transform.rotation) * transform.rotation;
        }

        /// <summary>Sets the hands grip 0 is open 1 is closed</summary>
        public void SetGrip(float grip, float squeeze) {
            gripAxis = grip;
            squeezeAxis = squeeze;
        }

        [ContextMenu("Set Pose - Relax Hand")]
        public void RelaxHand() {
            foreach(var finger in fingers)
                finger.SetFingerBend(gripOffset);
        }

        [ContextMenu("Set Pose - Open Hand")]
        public void OpenHand() {
            foreach(var finger in fingers)
                finger.SetFingerBend(0);
        }

        [ContextMenu("Set Pose - Close Hand")]
        public void CloseHand() {
            foreach(var finger in fingers)
                finger.SetFingerBend(1);
        }

        [ContextMenu("Set Pose - Pinch Hand")]
        public void PinchHand() {
            foreach(var finger in fingers)
                finger.SetFingerBend(1, FingerPoseEnum.Open, FingerPoseEnum.PinchClosed);
        }

        [ContextMenu("Set Pose - Open Pinch Hand")]
        public void OpenPinchHand() {
            foreach(var finger in fingers)
                finger.SetFingerBend(1, FingerPoseEnum.Open, FingerPoseEnum.PinchOpen);
        }

        [ContextMenu("Bend Fingers Until Hit")]
        /// <summary>Bends each finger until they hit</summary>
        public void ProceduralFingerBend() {
            ProceduralFingerBend(~LayerMask.GetMask(rightHandLayerName, leftHandLayerName));
        }

        /// <summary>Bends each finger until they hit</summary>
        public void ProceduralFingerBend(int layermask) {
            foreach(var finger in fingers) {
                finger.BendFingerUntilHit(fingerBendSteps, layermask);
            }
        }

        /// <summary>Bends each finger until they hit</summary>
        public void ProceduralFingerBend(RaycastHit hit) {
            foreach(var finger in fingers) {
                finger.BendFingerUntilHit(fingerBendSteps, hit.transform.gameObject.layer);
            }
        }

        /// <summary>Plays haptic vibration on the hand controller if supported by controller link</summary>
        public void PlayHapticVibration() {
            PlayHapticVibration(0.05f, 0.5f);
        }

        /// <summary>Plays haptic vibration on the hand controller if supported by controller link</summary>
        public void PlayHapticVibration(float duration) {
            PlayHapticVibration(duration, 0.5f);
        }

        /// <summary>Plays haptic vibration on the hand controller if supported by controller link</summary>
        public void PlayHapticVibration(float duration, float amp = 0.5f) {
            if(left)
                HandControllerLink.handLeft?.TryHapticImpulse(duration, amp);
            else
                HandControllerLink.handRight?.TryHapticImpulse(duration, amp);
        }


            

        #region INTERNAL FUNCTIONS

        //=================================================================
        //======================= INTERNAL FUNCTIONS ======================
        //=================================================================



        /// <summary>Takes a hit from a grabbable object and moves the hand towards that point, then calculates ideal hand shape</summary>
        protected IEnumerator GrabObject(RaycastHit hit, Grabbable grab, GrabType grabType) {
            /////////////////////////
            ////Initialize values////
            /////////////////////////
            if(!CanGrab(grab))
                yield break;


            grab.AddWaitingForGrab(this);

            bool waitingForGrab = false;
            while(grab.beingGrabbed) {
                waitingForGrab = true;
                yield return new WaitForFixedUpdate();
            }

            grab.RemoveWaitingForGrab(this);

            grab.beforeGrabFrame = true;
            var startHandPosition = transform.position;
            var startHandRotation = transform.rotation;
            var startGrabbablePosition = grab.transform.position;
            var startGrabbableRotation = grab.transform.rotation;
            if(grab.body != null) {
                startGrabbablePosition = grab.body.transform.position;
                startGrabbableRotation = grab.body.transform.rotation;
            }

            grabbing = true;
            grab.beforeGrabFrame = false;


            handAnimator.CancelPose();
            handAnimator.ClearPoseArea();

            currentHeldPose = null;
            holdingObj = grab;
            var startHoldingObj = holdingObj;

            body.velocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;

            grabbingHit = hit;

            OnBeforeGrabbed?.Invoke(this, holdingObj);
            holdingObj.OnBeforeGrab(this);

            if(waitingForGrab)
                RecalculateBeforeGrab(grab);

            handGrabPoint.parent = grab.rootTransform;
            handGrabPoint.position = grabbingHit.point;
            handGrabPoint.up = grabbingHit.normal;

            if(holdingObj == null || grabbingHit.collider == null) {
                CancelGrab();
                yield break;
            }

            var instantGrab = holdingObj.instantGrab || grabType == GrabType.InstantGrab;
            startGrabDist = Vector3.Distance(palmTransform.position, handGrabPoint.position);
            startHandLocalGrabPosition = holdingObj.transform.InverseTransformPoint(transform.position);


            if(holdingObj == null || grabbingHit.collider == null) {
                CancelGrab();
                yield break;
            }

            if(instantGrab)
                holdingObj.ActivateRigidbody();

            /////////////////
            ////Sets Pose////
            /////////////////
            
            HandPoseData startGrabPose;
            if(holdingObj.GetGrabPose(this, out var tempGrabPose)) {
                startGrabPose = new HandPoseData(this, tempGrabPose.transform);
                currentHeldPose = tempGrabPose;
                currentHeldPose.SetHandPose(this);
            }
            else {
                startGrabPose = new HandPoseData(this, holdingObj.transform);
                transform.position -= palmTransform.forward * 0.08f;
                body.position = transform.position;
                AutoPose(grabbingHit, holdingObj);
            }

            if(currentHeldPose != null)
                handAnimator.targetGrabPose.CopyFromData(ref currentHeldPose.GetHandPoseData(this));
            else
                handAnimator.targetGrabPose.SavePose(this, holdingObj.transform);

            localGrabbablePoint.position = grab.rootTransform.position;
            localGrabbablePoint.rotation = grab.rootTransform.rotation;


            //////////////////////////
            ////Grabbing Animation////
            //////////////////////////

            handAnimator.SetPose(ref handAnimator.targetGrabPose, 0f);

            //Instant Grabbing
            if(instantGrab) {
                if(currentHeldPose != null)
                    currentHeldPose.SetHandPose(this);

                //Hand Swap - One Handed Items
                if(holdingObj.singleHandOnly && holdingObj.HeldCount(false, false, false) > 0) {
                    holdingObj.ForceHandRelease(holdingObj.GetHeldBy()[0]);
                    if(holdingObj.body != null) {
                        holdingObj.body.velocity = Vector3.zero;
                        holdingObj.body.angularVelocity = Vector3.zero;
                    }
                }
            }
            //Smooth Grabbing
            else {
                transform.position = startHandPosition;
                transform.rotation = startHandRotation;
                body.position = startHandPosition;
                body.rotation = startHandRotation;

                var adjustedGrabTime = GetGrabTime();
                instantGrab = instantGrab || adjustedGrabTime == 0;
                Transform grabTarget = currentHeldPose != null ? currentHeldPose.transform : holdingObj.transform;

                var targetOpenPose = new HandPoseData(this);
                targetOpenPose.CopyFromData(ref handAnimator.openHandPose);
                foreach(var finger in fingers) {
                    int fingerIndex = (int)finger.fingerType;
                    targetOpenPose.fingerPoses[fingerIndex].LerpDataTo(ref handAnimator.closeHandPose.fingerPoses[fingerIndex], finger.GetLastHitBend() / 2f);
                }

                /////////////////////////
                ////Hand To Grabbable////
                /////////////////////////
                if(grabType == GrabType.HandToGrabbable || (grabType == GrabType.GrabbableToHand && (holdingObj.HeldCount() > 0 || !holdingObj.parentOnGrab))) {
                    //Loop until the hand is at the object
                    for(float i = 0; i < adjustedGrabTime; i += Time.deltaTime) {
                        if(holdingObj != null) {
                            //Will move the hand faster if the controller or object is moving
                            var deltaDist = Vector3.Distance(follow.position, lastFollowPosition);
                            float maxDeltaTimeOffset = minGrabTime/adjustedGrabTime * Time.deltaTime * 5f;

                            float timeOffset = deltaDist * Time.deltaTime * velocityGrabHandAmplifier;
                            timeOffset += holdingObj.GetVelocity().magnitude * Time.deltaTime * velocityGrabObjectAmplifier * 3f;
                            i += Mathf.Clamp(timeOffset, 0, maxDeltaTimeOffset);

                            if(i < adjustedGrabTime) {
                                var point = Mathf.Clamp01(i / adjustedGrabTime);
                                var handTargetTime = 1.25f;

                                if(point < grabOpenHandPoint) {
                                    HandPoseData.LerpPose(ref handAnimator.handPoseDataNonAlloc, ref startGrabPose, ref targetOpenPose, grabCurve.Evaluate(point * 1f / grabOpenHandPoint));
                                    handAnimator.handPoseDataNonAlloc.SetFingerPose(this);
                                }
                                else {
                                    HandPoseData.LerpPose(ref handAnimator.handPoseDataNonAlloc, ref targetOpenPose, ref handAnimator.targetGrabPose, grabCurve.Evaluate((point - grabOpenHandPoint) * (1f / (1f - grabOpenHandPoint))));
                                    handAnimator.handPoseDataNonAlloc.SetFingerPose(this);
                                }

                                HandPoseData.LerpPose(ref handAnimator.handPoseDataNonAlloc, ref startGrabPose, ref handAnimator.targetGrabPose, point * handTargetTime);
                                handAnimator.handPoseDataNonAlloc.SetPosition(this, grabTarget);

                                body.position = transform.position;
                                body.rotation = transform.rotation;

                                if(holdingObj.body != null && !holdingObj.body.isKinematic) {
                                    holdingObj.body.angularVelocity *= 0.5f;
                                    if(point * handTargetTime >= 1f)
                                        holdingObj.body.velocity *= 0.9f;
                                    else
                                        holdingObj.body.velocity *= 0.98f;
                                }
                                yield return new WaitForEndOfFrame();
                            }
                        }
                    }

                    //Hand Swap - One Handed Items
                    if(holdingObj != null && holdingObj.singleHandOnly && holdingObj.GetHeldBy().Count > 0)
                        holdingObj.ForceHandRelease(holdingObj.GetHeldBy()[0]);
                }



                /////////////////////////
                ////Grabbable to Hand////
                /////////////////////////
                else if(grabType == GrabType.GrabbableToHand) {
                    holdingObj.ActivateRigidbody();


                    //Hand Swap - One Handed Items
                    if(holdingObj.singleHandOnly && holdingObj.HeldCount() > 0)
                        holdingObj.ForceHandRelease(holdingObj.GetHeldBy()[0]);

                    //Disable grabbable while item is moving towards hand
                    bool useGravity = holdingObj.body.useGravity;
                    holdingObj.body.useGravity = false;
                    
                    //Loop until the object is at the hand
                    for(float i = 0; i < adjustedGrabTime; i += Time.deltaTime) {
                        if(holdingObj != null) {
                            //Will move the hand faster if the controller or object is moving
                            var deltaDist = Vector3.Distance(follow.position, lastFollowPosition);
                            float minDeltaTime = minGrabTime/adjustedGrabTime * Time.deltaTime * 5f;

                            float timeOffset = deltaDist * Time.deltaTime * velocityGrabHandAmplifier;
                            i += Mathf.Clamp(timeOffset, 0, minDeltaTime);

                            var point = Mathf.Clamp01(i / adjustedGrabTime);

                            if(point < grabOpenHandPoint) {
                                HandPoseData.LerpPose(ref handAnimator.handPoseDataNonAlloc, ref startGrabPose, ref targetOpenPose, grabCurve.Evaluate(point / grabOpenHandPoint));
                                handAnimator.handPoseDataNonAlloc.SetFingerPose(this);
                            }
                            else {
                                HandPoseData.LerpPose(ref handAnimator.handPoseDataNonAlloc, ref targetOpenPose, ref handAnimator.targetGrabPose, grabCurve.Evaluate((point - grabOpenHandPoint) * (1f / (1f - grabOpenHandPoint))));
                                handAnimator.handPoseDataNonAlloc.SetFingerPose(this);
                            }

                            if(holdingObj.body != null && !holdingObj.body.isKinematic) {
                                holdingObj.body.transform.position = Vector3.Lerp(startGrabbablePosition, localGrabbablePoint.position, grabCurve.Evaluate(point / grabOpenHandPoint));
                                holdingObj.body.transform.rotation = Quaternion.Lerp(startGrabbableRotation, localGrabbablePoint.rotation, grabCurve.Evaluate(point / grabOpenHandPoint));
                                holdingObj.body.position = holdingObj.body.transform.position;
                                holdingObj.body.rotation = holdingObj.body.transform.rotation;
                                holdingObj.body.velocity = Vector3.zero;
                                holdingObj.body.angularVelocity = Vector3.zero;
                            }
                            else {
                                holdingObj.transform.position = Vector3.Lerp(startGrabbablePosition, localGrabbablePoint.position, grabCurve.Evaluate(point / grabOpenHandPoint));
                                holdingObj.transform.rotation = Quaternion.Lerp(startGrabbableRotation, localGrabbablePoint.rotation, grabCurve.Evaluate(point / grabOpenHandPoint));
                            }

                            handFollow.SetMoveTo(true);
                            handFollow.MoveTo(Time.fixedDeltaTime);
                            handFollow.TorqueTo(Time.fixedDeltaTime);

                            yield return new WaitForEndOfFrame();

                        }
                    }

                    //Reset Gravity
                    if(holdingObj != null && holdingObj.body != null)
                        holdingObj.body.useGravity = useGravity;
                    else if(startHoldingObj.body != null)
                        startHoldingObj.body.useGravity = useGravity;
                }

                //Ensure final pose
                if(holdingObj != null)
                    handAnimator.targetGrabPose.SetPose(this, grabTarget);
            }

            if(holdingObj == null) {
                CancelGrab();
                yield break;
            }




            //////////////////////////////////
            ////Finalize Values and Events////
            //////////////////////////////////

            handGrabPoint.position = transform.position;
            handGrabPoint.rotation = transform.rotation;

            holdingObj.ActivateRigidbody();
            localGrabbablePoint.position = holdingObj.rootTransform.position;
            localGrabbablePoint.rotation = holdingObj.rootTransform.rotation;


            if(!instantGrab || !holdingObj.parentOnGrab) {
                ResetGrabOffset();
            }


            CreateJoint(holdingObj, holdingObj.jointBreakForce , float.PositiveInfinity);
            handFollow.SetMoveTo();
            holdingObj?.OnGrab(this);
            OnGrabbed?.Invoke(this, holdingObj);

            if(holdingObj == null) {
                CancelGrab();
                yield break;
            }

            void CancelGrab() {
                BreakGrabConnection();
                if(startHoldingObj)
                {
                    if (startHoldingObj.body != null)
                    {
                        startHoldingObj.body.velocity = Vector3.zero;
                        startHoldingObj.body.angularVelocity = Vector3.zero;
                    }
                    startHoldingObj.beingGrabbed = false;
                }
                grabbing = false;
                grabRoutine = null;
            }

            grabbing = false;
            startHoldingObj.beingGrabbed = false;
            lastGrabTime = Time.time;

            grabRoutine = null;

            if(instantGrab && holdingObj.parentOnGrab) {
                handFollow.SetHandLocation(handFollow.moveTo.position, handFollow.moveTo.rotation);
            }
        }




        internal float GetGrabTime() {
            var distanceDivider = Mathf.Clamp01(startGrabDist / reachDistance);
            return Mathf.Clamp(minGrabTime*2 + ((maxGrabTime - minGrabTime) * distanceDivider), 0, maxGrabTime);
        }

        bool GetClosestGrabbableHit(Grabbable grab, out RaycastHit closestHit) {
            closestHit = new RaycastHit();
            closestHit.distance = float.MaxValue;
            Ray ray = new Ray();
            ray.origin = palmTransform.position;

            bool didHit = false;
            foreach(var collider in grab.grabColliders) {
                Vector3 closestPoint = collider.ClosestPoint(palmTransform.transform.position);
                ray.direction = closestPoint - palmTransform.position;
                ray.direction = ray.direction.normalized;
                if(ray.direction == Vector3.zero) {
                    ray.direction = collider.bounds.center - palmTransform.position;
                }
                if(collider.Raycast(ray, out var hit, 1000)) {
                    if(hit.distance < closestHit.distance) {
                        closestHit = hit;
                        didHit = true;
                    }
                }
                else {
                    ray.origin = Vector3.MoveTowards(ray.origin, collider.bounds.center, 0.002f);
                    if(collider.Raycast(ray, out hit, 1000) && hit.distance < closestHit.distance)
                        closestHit = hit;
                    else {
                        ray.origin = Vector3.MoveTowards(ray.origin, collider.bounds.center, 0.01f);
                        if(collider.Raycast(ray, out hit, 1000) && hit.distance < closestHit.distance)
                            closestHit = hit;
                    }
                }
            }

            return didHit;
        }


        #endregion


        protected virtual void OnCollisionFirstEnter(GameObject collision) {
            if(collision.CanGetComponent(out HandTouchEvent touchEvent)) {
                touchEvent.Touch(this);
            }
        }

        protected virtual void OnCollisionLastExit(GameObject collision) {
            if(collision.CanGetComponent(out HandTouchEvent touchEvent))
                touchEvent.Untouch(this);
        }

        protected virtual void OnTriggerFirstEnter(GameObject other) {
            if(other.CanGetComponent(out HandTriggerAreaEvents area)) {
                triggerEventAreas.Add(area);
                area.Enter(this);
            }
        }

        protected virtual void OnTriggerLastExit(GameObject other) {
            if(other.CanGetComponent(out HandTriggerAreaEvents area)) {
                triggerEventAreas.Remove(area);
                area.Exit(this);
            }
        }

        internal virtual void RemoveHandTriggerArea(HandTriggerAreaEvents handTrigger) {
            handTrigger.Exit(this);
            triggerEventAreas.Remove(handTrigger);
        }


        void OnCollisionFirstEnterEvent(GameObject collision) { OnHandCollisionStart?.Invoke(this, collision); }
        void OnCollisionLastExitEvent(GameObject collision) { OnHandCollisionStop?.Invoke(this, collision); }
        void OnTriggerFirstEnterEvent(GameObject collision) { OnHandTriggerStart?.Invoke(this, collision); }
        void OnTriggeLastExitEvent(GameObject collision) { OnHandTriggerStop?.Invoke(this, collision); }


        public void ResetGrabConnectionOffset() {
            if(heldJoint != null) {
                ResetGrabOffset();

                handGrabPoint.position = transform.position;
                handGrabPoint.rotation = transform.rotation;
                localGrabbablePoint.position = holdingObj.rootTransform.position;
                localGrabbablePoint.rotation = holdingObj.rootTransform.rotation;
                heldJoint.connectedAnchor = holdingObj.body.transform.InverseTransformPoint(handGrabPoint.position);

            }
        }

    }
}