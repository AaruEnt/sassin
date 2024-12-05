using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

namespace Autohand {
    public enum PlacePointNameType
    {
        name,
        tag
    }

    public enum PlacePointShape {
        Sphere,
        Box
    }

    public delegate void PlacePointEvent(PlacePoint point, Grabbable grabbable);
    [Serializable]
    public class UnityPlacePointEvent : UnityEvent<PlacePoint, Grabbable> { }
    [HelpURL("https://app.gitbook.com/s/5zKO0EvOjzUDeT2aiFk3/auto-hand/place-point")]
    //You can override this by turning the radius to zero, and using any other trigger collider
    [DefaultExecutionOrder(1000)]
    public class PlacePoint : MonoBehaviour, IGrabbableEvents{
        [AutoHeader("Place Point")]
        public bool ignoreMe;

        [AutoSmallHeader("Place Settings")]
        public bool showPlaceSettings = true;
        [Tooltip("Snaps an object to the point at start, leave empty for no target")]
        public Grabbable startPlaced;
        [Tooltip("This will offset where the object position is when placed")]
        public Transform placedOffset;

        [Space]
        public PlacePointShape shapeType = PlacePointShape.Sphere;
        [Tooltip("The radius of the place point (relative to scale)"), ShowIf("shapeType", PlacePointShape.Sphere)]
        public float placeRadius = 0.1f;
        [Tooltip("The radius of the place point (relative to scale)"), ShowIf("shapeType", PlacePointShape.Box)]
        public Vector3 placeSize = new Vector3(0.1f, 0.1f, 0.1f);
        [Tooltip("The local offset of the enter radius of the place point (not the offset of the placement)"), FormerlySerializedAs("radiusOffset")]
        public Vector3 shapeOffset;

        [Space]
        [Tooltip("This will make the place point itself targetable for grab istead of just the object inside. Functionally just makes the place point an easier grab target, also essential if turning off the colliders on the placed object")]
        public bool grabbablePlacePoint = true;
        [Tooltip("This will make the point place the object as soon as it enters the radius, instead of on release")]
        public bool forcePlace = false;
        [Tooltip("If true and will force hand to release on place when force place is called. If false the hand will attempt to keep the connection to the held object (but can still break due to max distances/break forces)")]
        [ShowIf("forcePlace")]
        public bool forceHandRelease = true;
        [Tooltip("This will make the object parent to be under this point when placed")]
        public bool parentOnPlace = true;

        [Space]
        [Tooltip("If disabled the place point will not set the placed object to match its position")]
        public bool matchPosition = true;
        [Tooltip("If disabled the place point will not set the placed object to match its rotation")]
        public bool matchRotation = true;
        [Tooltip("This will resize the object, depending on it's render bounds, to fit into the place point radius (- resizeOffset) on place")]

        [Space] 
        public bool resizeOnPlace = false;
        [ShowIf("resizeOnPlace")]
        public float resizeOffset = -0.02f;

        [Space]
        [Tooltip("Whether or not the placed object should have its rigidbody disabled on place, good for parenting placed objects under dynamic objects")]
        public bool disableRigidbodyOnPlace = false;
        [Tooltip("Whether or not the grabbable should be disabled on place")]
        public bool disableGrabOnPlace = false;
        [Tooltip("Whether or not this place point should be disabled on placement. It will maintain its connection and can no longer accept new items. Causes less overhead if true")]
        public bool disablePlacePointOnPlace = false;
        [Tooltip("Whether or not the placed object should be disabled on placement (this will hide the placed object and leave the place point active for a new object)")]
        public bool destroyObjectOnPlace = false;


        [Tooltip("If true and will force release on place")]
        [DisableIf("disableRigidbodyOnPlace")]
        public bool makePlacedKinematic = true;
        
        [DisableIf("disableRigidbodyOnPlace")]
        [Tooltip("The rigidbody to attach the placed grabbable to - leave empty means no joint")]
        public Rigidbody placedJointLink;
        [DisableIf("disableRigidbodyOnPlace")]
        public float jointBreakForce = 1000;

        [AutoSmallHeader("Place Requirements")]
        public bool showPlaceRequirements = true;

        [Tooltip("Whether or not to only allow placement of an object while it's being held (or released)")]
        public bool heldPlaceOnly = false;

        [Tooltip("Whether the placeNames should compare names or tags")]
        public PlacePointNameType nameCompareType;
        [Tooltip("Will allow placement for any grabbable with a name containing this array of strings, leave blank for any grabbable allowed")]
        public string[] placeNames;
        [Tooltip("Will prevent placement for any name containing this array of strings")]
        public string[] blacklistNames;

        [Tooltip("(Unless empty) Will only allow placement any object contained here")]
        public List<Grabbable> onlyAllows;
        [Tooltip("Will NOT allow placement any object contained here")]
        public List<Grabbable> dontAllows;
        [Tooltip("The layer that this place point will check for placeable objects, if none will default to Grabbable")]
        public LayerMask placeLayers;

        [Space]

        [AutoToggleHeader("Show Events")]
        public bool showEvents = true;
        [ShowIf("showEvents")]
        public UnityPlacePointEvent OnPlace;
        [ShowIf("showEvents")]
        public UnityPlacePointEvent OnRemove;
        [ShowIf("showEvents")]
        public UnityPlacePointEvent OnHighlight;
        [ShowIf("showEvents")]
        public UnityPlacePointEvent OnStopHighlight;
        
        //For the programmers
        public PlacePointEvent OnPlaceEvent;
        public PlacePointEvent OnRemoveEvent;
        public PlacePointEvent OnHighlightEvent;
        public PlacePointEvent OnStopHighlightEvent;

        public Grabbable highlightingObj { get; protected set; } = null;
        public Grabbable placedObject { get; protected set; } = null;
        public Grabbable lastPlacedObject { get; protected set; } = null;

        /// <summary>If this place point is set under a grabbable, this value will reference that grabbable</summary>
        internal Grabbable parentGrabbable;

        protected FixedJoint joint = null;
        protected float lastPlacedTime;
        protected CollisionDetectionMode placedObjDetectionMode;
        protected float tickRate = 0.05f;

        Coroutine checkRoutine;
        Collider[] collidersNonAlloc = new Collider[50];
        Vector3 lastPlacePosition = Vector3.zero;
        bool wasInstantGrab = false;
        Vector3 prefitScale;
        protected bool placingFrame;


        protected virtual void Awake(){
            if (placedOffset == null)
                placedOffset = transform;

            if(placeLayers == 0)
                placeLayers = LayerMask.GetMask(Hand.grabbableLayerNameDefault);

            //If the place point is set to be grabbable, make sure it's on the default layer
            if(grabbablePlacePoint) {
                var colliders = GetComponents<Collider>();
                foreach(Collider col in colliders) {
                    if(col.gameObject.layer == LayerMask.NameToLayer("Default") || LayerMask.LayerToName(col.gameObject.layer) == "")
                        col.gameObject.layer = LayerMask.NameToLayer(Hand.grabbableLayerNameDefault);
                }


                if(shapeType == PlacePointShape.Sphere && !gameObject.CanGetComponent<SphereCollider>(out var sphereCollider)) {
                    sphereCollider = gameObject.AddComponent<SphereCollider>();
                    sphereCollider.isTrigger = true;
                    sphereCollider.radius = placeRadius;
                    sphereCollider.center = shapeOffset;
                }
                if(shapeType == PlacePointShape.Box && !gameObject.CanGetComponent<BoxCollider>(out var boxCollider)) {
                    boxCollider = gameObject.AddComponent<BoxCollider>();
                    boxCollider.isTrigger = true;
                    boxCollider.size = placeSize;
                    boxCollider.center = shapeOffset;
                }
            }

            CheckInvalidSettings();

            if(startPlaced != null && startPlaced.childPlacePoints.Count == 0)
                SetStartPlaced();
            else if(startPlaced != null)
                StartCoroutine(LateStart());


        }

        protected virtual void CheckInvalidSettings() {
            if(parentGrabbable && !disableRigidbodyOnPlace && parentOnPlace) {
                Debug.LogWarning("Place Points placed under a grabbable cannot support parenting other rigidbody grabbables, disable rigibody on place is being enabled" , this);
                disableRigidbodyOnPlace = true;
                makePlacedKinematic = false;
            }
        }

        //This function helps solve the specific case where a place point is set to be placed into a different place point,
        //but the objects set to be placed in this place point are not yet placed, so we wait until they are placed before setting this place point to be placed
        IEnumerator LateStart() {
            bool waitForChildPointPlacement = false;
            int maxWaitFrames = 10;
            while(true) {
                yield return new WaitForFixedUpdate();

                foreach(var childPoint in startPlaced.childPlacePoints) {
                    if(childPoint.startPlaced != null && childPoint.placedObject == null) {
                        if(parentGrabbable == null || childPoint.startPlaced != parentGrabbable) {
                            waitForChildPointPlacement = true;
                            break;
                        }
                    }
                }

                if(waitForChildPointPlacement && maxWaitFrames > 0) {
                    maxWaitFrames--;
                    continue;
                }
                else {
                    if(startPlaced != null && startPlaced.childPlacePoints.Count > 0)
                        SetStartPlaced();

                    break;
                }

            }
        }



        protected virtual void OnEnable() {
            if(placedOffset == null)
                placedOffset = transform;

            if(checkRoutine == null)
                checkRoutine = StartCoroutine(CheckPlaceObjectLoop());

        }

        protected virtual void OnDisable() {
            if(checkRoutine != null) {
                StopCoroutine(checkRoutine);
                checkRoutine = null;
            }
            StopHighlight();
        }

        ///<summary>Sets the placement of the object set in the start placed value</summary>
        protected virtual void SetStartPlaced() {
            if(startPlaced != null) {
                //Checks if the start placed object is already in the scene
                if(startPlaced.gameObject.scene.IsValid()) {
                    Highlight(startPlaced);
                    Place(startPlaced);
                }
                //or if it's a prefab that needs to be instantiated
                else {
                    var instance = GameObject.Instantiate(startPlaced);
                    instance.transform.position = placedOffset.position;
                    instance.transform.rotation = placedOffset.rotation;
                    Highlight(instance);
                    Place(instance);

                }
            }
        }

        /// <summary>Places the object current placed to this place point</summary>
        public Grabbable GetPlacedObject() {
            return placedObject;
        }

        

        /// <summary>Whether or not the place point can accept an object based on its settings</summary>
        public virtual bool CanPlace(Grabbable placeObj, bool checkRoot = true) {
            if(checkRoot && CanPlace(placeObj.rootGrabbable, false))
                return true;

            if(placedObject != null) {
                return false;
            }

            //This prevents place points from accepting objects that arent parent on grab
            if(!placeObj.parentOnGrab && parentGrabbable != null) {
                return false;
            }

            //This prevents place points from accepting objects that arent being held while heldPlaceOnly is true
            if(heldPlaceOnly && placeObj.HeldCount() == 0) {
                return false;
            }

            //This prevents place points from accepting objects that arent being held while heldPlaceOnly is true
            if(onlyAllows.Count > 0 && !onlyAllows.Contains(placeObj)) {
                return false;
            }

            //This prevents place points from accepting objects that are in the dontAllows list
            if(dontAllows.Count > 0 && dontAllows.Contains(placeObj)) {
                return false;
            }

            //If no place names are set, then any grabbable is allowed
            if(placeNames.Length == 0 && blacklistNames.Length == 0) {
                return true;
            }

            //This prevents place points from accepting objects that are in the blacklistNames list
            if (blacklistNames.Length > 0)
                foreach(var badName in blacklistNames)
                {
                    if (nameCompareType == PlacePointNameType.name && placeObj.name.Contains(badName))
                        return false;
                    if (nameCompareType == PlacePointNameType.tag && placeObj.CompareTag(badName))
                        return false;
                }

            //This prevents place points from accepting objects that arent in the placeNames list
            if (placeNames.Length > 0)
                foreach (var placeName in placeNames)
                {
                    if (nameCompareType == PlacePointNameType.name && placeObj.name.Contains(placeName))
                        return true;
                    if (nameCompareType == PlacePointNameType.tag && placeObj.CompareTag(placeName))
                        return true;
                }
            else
                return true;

            return false;
        }


        /// <summary>Places the given grabbable into this place point if the settings allow it</summary>
        public virtual void TryPlace(Grabbable placeObj) {
            if(CanPlace(placeObj))
                Place(placeObj);
        }


        /// <summary>Places the given grabbable into this place point</summary>
        public virtual void Place(Grabbable placeObj) {
            if (placedObject != null)
                return;

            placeObj = placeObj.rootGrabbable;
            placingFrame = true;

            //Remove from any other place points then initialize
            if(placeObj.placePoint != null && placeObj.placePoint != this)
                placeObj.placePoint.Remove(placeObj);

            placedObject = placeObj.rootGrabbable;
            placedObject.SetPlacePoint(this);


            //Release any hands holding the grabbable or its children
            if((forceHandRelease || disableRigidbodyOnPlace) && placeObj.HeldCount() > 0) {
                placeObj.ForceHandsRelease();
                foreach(var grab in placeObj.grabbableChildren)
                    grab.ForceHandsRelease();
            }



            //Placement, set position, create whatever joints based on settings
            if(matchPosition)
                placeObj.rootTransform.position = placedOffset.position;
            if(matchRotation)
                placeObj.rootTransform.rotation = placedOffset.rotation;

            if (placeObj.body != null){
                placeObj.body.velocity = Vector3.zero;
                placeObj.body.angularVelocity = Vector3.zero;
                placedObjDetectionMode = placeObj.body.collisionDetectionMode;

                if (makePlacedKinematic && !disableRigidbodyOnPlace){
                    placeObj.body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
                    placeObj.body.isKinematic = makePlacedKinematic;
                }

                if (placedJointLink != null){
                    joint = placedJointLink.gameObject.AddComponent<FixedJoint>();
                    joint.connectedBody = placeObj.body;
                    joint.breakForce = jointBreakForce;
                    joint.breakTorque = jointBreakForce;
                
                    joint.connectedMassScale = 1;
                    joint.massScale = 1;
                    joint.enableCollision = false;
                    joint.enablePreprocessing = false;
                }
            }

            StopHighlight(placeObj);

            //If a child of the root grabbable is grabbed, this calls the remove function
            foreach(var grab in placeObj.grabbableChildren) 
                grab.OnGrabEvent += OnPlaceObjectChildGrabbed;

            //Call Events
            placeObj.OnPlacePointAddEvent?.Invoke(this, placeObj);
            foreach(var grabChild in placeObj.grabbableChildren)
                grabChild.OnPlacePointAddEvent?.Invoke(this, grabChild);

            OnPlaceEvent?.Invoke(this, placeObj);
            OnPlace?.Invoke(this, placeObj);
            lastPlacedTime = Time.time;

            //Apply place point settings
            if(destroyObjectOnPlace) {
                Destroy(placeObj.gameObject);
                return;
            }

            if(parentOnPlace) {
                placeObj.rootTransform.parent = transform;
            }

            if (disableRigidbodyOnPlace)
                placeObj.DeactivateRigidbody();

            if (disablePlacePointOnPlace)
                enabled = false;

            if (disableGrabOnPlace || disablePlacePointOnPlace)
                placeObj.isGrabbable = false;


            if(resizeOnPlace) {
                //It's important this happens in the before grab instead of on remove so the grab pose is calculated correctly
                placeObj.OnBeforeGrabEvent += ResizeBeforeGrab;
                foreach(var grabbable in placeObj.grabbableChildren)
                    grabbable.OnBeforeGrabEvent += ResizeBeforeGrab;

                //Ensures the pose looks correct after resizing
                placeObj.OnGrabEvent += RecacluatePoseAfterGrab;
                foreach(var grabChild in placeObj.rootGrabbable.grabbableChildren)
                    grabChild.OnGrabEvent += RecacluatePoseAfterGrab;

                //Save relevent values
                prefitScale = placeObj.rootTransform.localScale;
                wasInstantGrab = placeObj.instantGrab;
                placeObj.instantGrab = true;

                //Calculate and resize the object
                var scale = Mathf.Abs(transform.lossyScale.x < transform.lossyScale.y ? transform.lossyScale.x : transform.lossyScale.y);
                scale = Mathf.Abs(scale < transform.lossyScale.z ? scale : transform.lossyScale.z);
                if(shapeType == PlacePointShape.Sphere)
                    FitAndCenterToBounds(placeObj.rootTransform.gameObject, placeRadius*scale +  resizeOffset*scale);
                else if(shapeType == PlacePointShape.Box)
                    FitAndCenterToBounds(placeObj.rootTransform.gameObject, (placeSize + placeSize * resizeOffset)*scale);
            }


            //If the place point is grabbable add an event to recalculate the before grab event to ensure the pose is correct
            if(grabbablePlacePoint) {
                placeObj.OnBeforeGrabEvent += RecalculateBeforeGrab;
                foreach(var grabbable in placeObj.grabbableChildren)
                    grabbable.OnBeforeGrabEvent += RecalculateBeforeGrab;
            }

            //If the place point is found to be a child of a grabbable
            if(parentGrabbable != null) {
                //Disable the place points of the placed object from interacting with the parent grabbable
                foreach(var childPlacePoint in placeObj.childPlacePoints) {
                    parentGrabbable.PlacePointIgnore(childPlacePoint);
                    childPlacePoint.StopHighlight();
                    childPlacePoint.enabled = false;
                    if(childPlacePoint.placedObject != null)
                        childPlacePoint.placedObject.enabled = false;
                }
                //Combine the colliders of the placed object with the parent grabbable
                if(disableRigidbodyOnPlace && parentOnPlace)
                    parentGrabbable.AddGrabbableColliders(placeObj);
            }


        }


        /// <summary>Removes the object if it matches the given object</summary>
        public virtual void Remove(Grabbable placeObj) {
            placeObj = placeObj.rootGrabbable;

            if (placeObj == null || placeObj != placedObject || disablePlacePointOnPlace)
                return;

            //Remove Events
            foreach(var grab in placeObj.grabbableChildren)
                grab.OnGrabEvent -= OnPlaceObjectChildGrabbed;


            //Trigger highlight
            Highlight(placeObj);

            if(disableRigidbodyOnPlace)
                placeObj.ActivateRigidbody();


            //Reset rigidbody settings
            if (placeObj.body != null){
                if (makePlacedKinematic && !disableRigidbodyOnPlace)
                    placeObj.body.isKinematic = false;

                placeObj.body.collisionDetectionMode = placedObjDetectionMode;
            }


            //If the place point is a child of a grabbable
            if(parentGrabbable != null) {
                //Enable the place points of the placed object to interact with the parent grabbable
                if(placeObj.childPlacePoints.Count > 0) {
                    foreach(var childPlacePoint in placeObj.childPlacePoints) {
                        parentGrabbable.PlacePointAllow(childPlacePoint);
                        childPlacePoint.enabled = true;
                        if(childPlacePoint.placedObject != null)
                            childPlacePoint.placedObject.enabled = true;
                    }
                }

                //Remove the colliders of the placed object from the parent grabbable
                parentGrabbable.RemoveGrabbableColliders(placeObj);

                //Ignore the collisions between the placed object and the parent grabbable
                parentGrabbable.IgnoreGrabbableCollisionUntilNone(placeObj);
                //Ignore the collisions between the placed object and the hands holding the parent grabbable
                foreach(var hand in parentGrabbable.GetHeldBy()) 
                    placeObj.IgnoreHandCollisionUntilNone(hand);
                //Ignore the collisions between the placed object and the hands holding the placed object
                foreach(var hand in placeObj.GetHeldBy()) 
                    parentGrabbable.IgnoreHandCollisionUntilNone(hand);
            }

            //Reset size
            if(resizeOnPlace) {
                //Remove Events
                placedObject.OnBeforeGrabEvent -= ResizeBeforeGrab;
                foreach(var grabbable in placedObject.grabbableChildren)
                    grabbable.OnBeforeGrabEvent -= ResizeBeforeGrab;

                //If the remove function is called without grabbing
                if(placeObj.HeldCount() == 0)
                    placeObj.rootTransform.localScale = prefitScale;

                placeObj.instantGrab = wasInstantGrab;
            }
             
            if(grabbablePlacePoint) {
                //Remove Events
                placedObject.OnBeforeGrabEvent -= RecalculateBeforeGrab;
                foreach(var grabbable in placedObject.grabbableChildren)
                    grabbable.OnBeforeGrabEvent -= RecalculateBeforeGrab;
            }


            //Reset parent if the parenting isnt already being handled by a hand
            if((!placeObj.parentOnGrab || placeObj.HeldCount() == 0) && parentOnPlace && gameObject.activeInHierarchy) {
                placeObj.rootTransform.parent = placeObj.originalParent;
            }


            if(joint != null){
                Destroy(joint);
                joint = null;
            }

            //Call Events
            placedObject.OnPlacePointRemoveEvent?.Invoke(this, highlightingObj);
            foreach(var grabChild in placedObject.grabbableChildren)
                grabChild.OnPlacePointRemoveEvent?.Invoke(this, grabChild);
            OnRemoveEvent?.Invoke(this, placeObj);
            OnRemove?.Invoke(this, placeObj);


            lastPlacedObject = placedObject;
            placedObject = null;
        }


        /// <summary>Removes the object if it has one</summary>
        [ContextMenu("Remove Placed")]
        public void Remove() {
            if(placedObject != null)
                Remove(placedObject);
        }



        public virtual void Highlight(Grabbable from) {
            from = from.rootGrabbable;
            if(highlightingObj == null){
                highlightingObj = from;
                from.SetPlacePoint(this);

                highlightingObj.OnPlacePointHighlightEvent?.Invoke(this, highlightingObj);
                foreach(var grabChild in highlightingObj.grabbableChildren)
                    grabChild.OnPlacePointHighlightEvent?.Invoke(this, grabChild);

                OnHighlightEvent?.Invoke(this, from);
                OnHighlight?.Invoke(this, from);

                if(placedObject == null && (forcePlace || (!heldPlaceOnly && from.HeldCount() == 0)))
                    Place(from);
            }
        }

        public virtual void StopHighlight(Grabbable grab) {
            grab = grab.rootGrabbable;
            if(highlightingObj == grab) {
                StopHighlight();
            }
        }

        public virtual void StopHighlight() {
            if(highlightingObj != null) {
                highlightingObj.OnPlacePointUnhighlightEvent?.Invoke(this, highlightingObj);
                foreach(var grabChild in highlightingObj.grabbableChildren)
                    grabChild.OnPlacePointUnhighlightEvent?.Invoke(this, grabChild);

                OnStopHighlightEvent?.Invoke(this, highlightingObj);
                OnStopHighlight?.Invoke(this, highlightingObj);

                if (placedObject == null)
                    highlightingObj.SetPlacePoint(null);

                highlightingObj = null;
            }
        }




        //CHECK PLACEMENT FUNCTIONS
        int lastOverlapCount = 0;
        protected virtual IEnumerator CheckPlaceObjectLoop() {
            yield return new WaitForSeconds(0.2f);
            yield return new WaitForSeconds(UnityEngine.Random.Range(0f, tickRate));

            while(gameObject.activeInHierarchy) {
                var scale = Mathf.Abs(transform.lossyScale.x < transform.lossyScale.y ? transform.lossyScale.x : transform.lossyScale.y);
                scale = Mathf.Abs(scale < transform.lossyScale.z ? scale : transform.lossyScale.z);
                if(!disablePlacePointOnPlace && !disableRigidbodyOnPlace && placedObject != null &&
                    lastPlacePosition != placedObject.transform.position && !IsStillOverlapping(placedObject, scale) && !placingFrame) {
                    Remove(placedObject);
                }

                if(placedObject != null)
                    lastPlacePosition = placedObject.transform.position;

                CheckHighlight(scale);

                yield return new WaitForSeconds(tickRate);
                if(placedObject != null && placingFrame) {
                    if(matchPosition)
                        placedObject.rootTransform.position = placedOffset.position;
                    if(matchRotation)
                        placedObject.rootTransform.rotation = placedOffset.rotation;

                    lastPlacePosition = placedObject.transform.position;
                }

                placingFrame = false;
            }
        }

        protected virtual void CheckPlaceObject(float scale) {
            if(!disablePlacePointOnPlace && !disableRigidbodyOnPlace && placedObject != null &&
                lastPlacePosition != placedObject.transform.position && !IsStillOverlapping(placedObject, scale)) {
                Remove(placedObject);
            }

            if(placedObject != null)
                lastPlacePosition = placedObject.transform.position;

            CheckHighlight(scale);
        }

        protected virtual void CheckHighlight(float scale) {
            if(placedObject == null && highlightingObj == null) {
                var overlapCenterPos = placedOffset.position + transform.rotation * shapeOffset;
                int overlaps = 0;
                switch(shapeType) {
                    case PlacePointShape.Sphere:
                        overlaps = Physics.OverlapSphereNonAlloc(overlapCenterPos, placeRadius * scale, collidersNonAlloc, placeLayers);
                        break;
                    case PlacePointShape.Box:
                        overlaps = Physics.OverlapBoxNonAlloc(overlapCenterPos, placeSize/2f * scale, collidersNonAlloc, transform.rotation, placeLayers);
                        break;
                }

                if(overlaps != lastOverlapCount) {
                    var updateOverlaps = true;
                    for(int i = 0; i < overlaps; i++) {
                        if(AutoHandExtensions.HasGrabbable(collidersNonAlloc[i].gameObject, out var tempGrabbable)) {
                            tempGrabbable = tempGrabbable.rootGrabbable;
                            updateOverlaps = false;

                            if(CanPlace(tempGrabbable)) {
                                var existingPlacePoint = tempGrabbable.placePoint;
                                if(existingPlacePoint) {
                                    var grabbablePos = tempGrabbable.transform.position;
                                    var concurrentCenterPos = existingPlacePoint.placedOffset.position + existingPlacePoint.transform.rotation * existingPlacePoint.shapeOffset;
                                    var concurrentDist = Vector3.Distance(concurrentCenterPos, grabbablePos);
                                    var currentDist = Vector3.Distance(overlapCenterPos, grabbablePos);
                                    if(currentDist >= concurrentDist)
                                        continue;

                                    existingPlacePoint.StopHighlight(tempGrabbable);
                                }

                                Highlight(tempGrabbable);
                                break;
                            }
                        }
                    }

                    if(updateOverlaps) {
                        lastOverlapCount = overlaps;
                    }
                }
            }
            else if(highlightingObj != null) {
                if(!IsStillOverlapping(highlightingObj, scale)) {
                    StopHighlight(highlightingObj);
                }
            }
        }


        protected bool IsStillOverlapping(Grabbable from, float scale = 1) {
            var overlapCenterPos = placedOffset.position + transform.rotation * shapeOffset;
            int overlaps = 0;
            switch(shapeType) {
                case PlacePointShape.Sphere:
                    overlaps = Physics.OverlapSphereNonAlloc(overlapCenterPos, placeRadius * scale, collidersNonAlloc, placeLayers);
                    break;
                case PlacePointShape.Box:
                    overlaps = Physics.OverlapBoxNonAlloc(overlapCenterPos, placeSize/2f * scale, collidersNonAlloc, transform.rotation, placeLayers);
                    break;
            }

            for (int i = 0; i < overlaps; i++){
                if (collidersNonAlloc[i].attachedRigidbody == from.body) {
                    return true;
                }
            }
            
            return false;
        }


        protected virtual void OnPlaceObjectChildGrabbed(Hand pHand, Grabbable pGrabbable){
            Remove();
        }




        protected void ResizeBeforeGrab(Hand hand, Grabbable grab) {
            grab.rootTransform.localScale = prefitScale;
            Physics.SyncTransforms();
            if(grab.body != null) {
                grab.body.WakeUp();
                grab.body.detectCollisions = false;
                grab.body.detectCollisions = true;
            }

        }

        protected void RecalculateBeforeGrab(Hand hand, Grabbable grab) {
            hand.RecalculateBeforeGrab(grab);
        }

        protected void RecacluatePoseAfterGrab(Hand hand, Grabbable grab) {
            hand.RecaculateHeldAutoPose();
            grab.rootGrabbable.OnGrabEvent -= RecacluatePoseAfterGrab;
            foreach(var grabChild in grab.rootGrabbable.grabbableChildren) {
                grabChild.OnGrabEvent -= RecacluatePoseAfterGrab;
            }
        }

        protected void FitAndCenterToBounds(GameObject obj, float radius) {
            Bounds bounds = CalculateCombinedBounds(obj);
            var scaleOffset = ScaleToFitRadius(obj, bounds, radius);
            obj.transform.localScale *= scaleOffset;
            bounds.extents *= scaleOffset;
            bounds = CalculateCombinedBounds(obj);
            if(matchPosition)
                obj.transform.position = placedOffset.position + (obj.transform.position - bounds.center);
            if(matchRotation)
                obj.transform.rotation = placedOffset.rotation;
        }


        protected float ScaleToFitRadius(GameObject obj, Bounds bounds, float radius) {
            float maxExtent = bounds.extents.magnitude;
            float scale = radius / maxExtent;
            return scale;
        }
        protected void FitAndCenterToBounds(GameObject obj, Vector3 size) {
            Bounds bounds = CalculateCombinedBounds(obj);
            float scaleOffset = ScaleToFitSize(obj, bounds, size);
            obj.transform.localScale *= scaleOffset;
            bounds.extents *= scaleOffset;
            bounds = CalculateCombinedBounds(obj);
            if(matchPosition)
                obj.transform.position = placedOffset.position + (obj.transform.position - bounds.center);
            if(matchRotation)
                obj.transform.rotation = placedOffset.rotation;
        }

        protected float ScaleToFitSize(GameObject obj, Bounds bounds, Vector3 size) {
            Vector3 currentSize = bounds.size;
            float scaleX = size.x / currentSize.x;
            float scaleY = size.y / currentSize.y;
            float scaleZ = size.z / currentSize.z;
            float scale = Mathf.Min(scaleX, Mathf.Min(scaleY, scaleZ));
            return scale;
        }

        protected Bounds CalculateCombinedBounds(GameObject obj) {
            var meshRenderers = obj.GetComponentsInChildren<MeshRenderer>().OfType<Renderer>();
            var skinnedMeshRenderers = obj.GetComponentsInChildren<SkinnedMeshRenderer>().OfType<Renderer>();
            List<Renderer> renderers = new List<Renderer>();

            renderers.AddRange(meshRenderers);
            renderers.AddRange(skinnedMeshRenderers);
            Bounds combinedBounds = new Bounds(obj.transform.position, Vector3.zero);

            foreach(Renderer renderer in renderers)
                combinedBounds.Encapsulate(renderer.bounds);

            return combinedBounds;
        }





        protected virtual void OnJointBreak(float breakForce) {
            if(placedObject != null)
                Remove(placedObject);
        }




        void OnDrawGizmos() {
            if(placedOffset == null)
                placedOffset = transform;

            var scale = Mathf.Abs(transform.lossyScale.x < transform.lossyScale.y ? transform.lossyScale.x : transform.lossyScale.y);
            scale = Mathf.Abs(scale < transform.lossyScale.z ? scale : transform.lossyScale.z);

            Gizmos.color = Color.white; 
            Gizmos.matrix = transform.localToWorldMatrix;

            if(shapeType == PlacePointShape.Box) {
                Gizmos.DrawWireCube(shapeOffset, placeSize);
                
                if(resizeOffset != 0 && resizeOnPlace) {
                    Gizmos.color = Color.red;
                    Gizmos.DrawWireCube(shapeOffset, (placeSize + placeSize * resizeOffset));
                }
            }
            else if(shapeType == PlacePointShape.Sphere) {

                Gizmos.DrawWireSphere(shapeOffset, placeRadius);

                if(resizeOffset != 0 && resizeOnPlace) {
                    Gizmos.color = Color.red;
                    Gizmos.DrawWireSphere(shapeOffset, placeRadius + resizeOffset);
                }
            }
        }


        //INTERFACE FUNCTIONS FOR GRABBABLE PLACE POINT
        void IGrabbableEvents.OnHighlight(Hand hand) {
            if(!grabbablePlacePoint)
                return; 

            if(placedObject != null)
                placedObject.Highlight(hand);
        }

        public virtual void OnUnhighlight(Hand hand) {
            if(!grabbablePlacePoint)
                return;

            if(placedObject != null)
                placedObject.Unhighlight(hand);

        }

        public virtual void OnGrab(Hand hand) {
            if(!grabbablePlacePoint)
                return;

            hand.RecaculateHeldAutoPose();
        }

        public virtual void OnRelease(Hand hand) {
            if(!grabbablePlacePoint)
                return;
        }

        public virtual bool CanGrab(Hand hand) {
            if(!grabbablePlacePoint || placedObject == null)
                return false;

            return placedObject.CanGrab(hand);
        }

        public virtual Grabbable GetGrabbable() {
            if(!grabbablePlacePoint || placedObject == null || !enabled)
                return null;

            return placedObject;

        }
    }
}
