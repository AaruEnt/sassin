using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Autohand {
    public class HandGrabbableHighlighter : MonoBehaviour {
        Hand _hand;
        public Hand hand {
            get {
                if(_hand == null)
                    _hand = GetComponent<Hand>();
                return _hand;
            }
        }

        [Tooltip("When choosing between multiple targets to highlight, " +
            "the hand will favor objects in the palms local forward direction (0) " +
            "or the palms local right direction (1) or a blend between the two recommended (0.5-0.75). " +
            "The forward direction should be facing away from the palm and the right direction should be pointing to the finger tips")]
        public float palmForwardRightDirection = 0.65f;


        /// <summary>Called when highlighting starts</summary>
        public event HandGrabEvent OnHighlight;
        /// <summary>Called when highlighting ends</summary>
        public event HandGrabEvent OnStopHighlight;



        //Highlighting doesn't need to be called every update, it can be called every 4th update without causing any noticable differrences 
        IEnumerator HighlightUpdate(float timestep) {
            yield return new WaitForEndOfFrame();
            yield return new WaitForFixedUpdate();

            //This will smooth out the highlight calls to help prevent lag spikes
            if(hand.left)
                yield return new WaitForSecondsRealtime(timestep / 2);

            while(true) {
                if(hand.usingHighlight) {
                    UpdateHighlight();
                }
                yield return new WaitForSecondsRealtime(timestep);
            }
        }

        RaycastHit _highlightHit = new RaycastHit();
        public RaycastHit highlightHit {
            get { return _highlightHit; }
            protected set { _highlightHit = value; }
        }

        [HideInInspector]
        public Collider[] highlightCollidersNonAlloc = new Collider[128];
        [HideInInspector]
        public int highlightColliderNonAllocCount = 0;
        [HideInInspector]
        public List<Grabbable> foundHighlightGrabbables = new List<Grabbable>();

        Coroutine highlightRoutine;
        protected List<RaycastHit> closestHits = new List<RaycastHit>();
        protected List<Grabbable> closestGrabs = new List<Grabbable>();

        public Grabbable currentHighlightTarget { get; protected set; }


        public virtual void OnEnable() {
            highlightRoutine = StartCoroutine(HighlightUpdate(1/30f));
        }

        public virtual void OnDisable() {
            if(highlightRoutine != null)
                StopCoroutine(highlightRoutine);

            if(currentHighlightTarget != null) {
                OnStopHighlight?.Invoke(hand, currentHighlightTarget);
                currentHighlightTarget.Unhighlight(hand);
            }
        }

        public virtual void Update() {
            if(hand.holdingObj != null || hand.IsGrabbing())
                currentHighlightTarget = null;
        }



        /// <summary>Manages the highlighting for grabbables</summary>
        public virtual void UpdateHighlight(bool overrideIgnoreHighlight = false, bool ignoreHighlightEvents = false) {

            if((overrideIgnoreHighlight || hand.usingHighlight) && hand.highlightLayers != 0 && (overrideIgnoreHighlight || hand.holdingObj == null && !hand.IsGrabbing())) {
                int grabbingLayer = LayerMask.NameToLayer(Hand.grabbingLayerName);
                int gabbingMask = LayerMask.GetMask(Hand.grabbingLayerName);
                highlightColliderNonAllocCount = Physics.OverlapSphereNonAlloc(hand.palmTransform.position + hand.palmTransform.forward * hand.reachDistance / 3f, hand.reachDistance, highlightCollidersNonAlloc, hand.highlightLayers & ~(hand.ignoreGrabCheckLayers.value), QueryTriggerInteraction.Collide);
                foundHighlightGrabbables.Clear();

                for(int i = 0; i < highlightColliderNonAllocCount; i++) {
                    if(highlightCollidersNonAlloc[i].gameObject.HasGrabbable(out var grab)) {
                        grab.SetLayerRecursive(grabbingLayer);
                        foundHighlightGrabbables.Add(grab);
                    }
                }

                if(foundHighlightGrabbables.Count > 0) {
                    Vector3 dir = HandClosestHit(out _highlightHit, out IGrabbableEvents newLookingAtObjEvent, ~(hand.handLayers | hand.ignoreGrabCheckLayers.value));
                    Grabbable newLookingAtObj = null;
                    if(newLookingAtObjEvent != null && newLookingAtObjEvent.GetGrabbable() != null && newLookingAtObjEvent.GetGrabbable().enabled == true)
                        newLookingAtObj = newLookingAtObjEvent.GetGrabbable();

                    //Zero means it didn't hit
                    if(dir != Vector3.zero && (newLookingAtObj != null && newLookingAtObj.CanGrab(hand))) {
                        //Changes look target
                        if(newLookingAtObj != currentHighlightTarget) {
                            //Unhighlights current target if found
                            if(currentHighlightTarget != null) {
                                if(!ignoreHighlightEvents && OnStopHighlight != null)
                                    OnStopHighlight.Invoke(hand, currentHighlightTarget);

                                currentHighlightTarget.Unhighlight(hand, null, ignoreHighlightEvents);
                            }

                            currentHighlightTarget = newLookingAtObj;

                            //Highlights new target if found
                            if(!ignoreHighlightEvents && OnHighlight != null)
                                OnHighlight.Invoke(hand, currentHighlightTarget);
                            currentHighlightTarget.Highlight(hand, null, ignoreHighlightEvents);
                        }
                    }
                    //If it was looking at something but now it's not there anymore
                    else if(newLookingAtObj == null && currentHighlightTarget != null) {
                        //Just in case the object your hand is looking at is destroyed
                        if(!ignoreHighlightEvents && OnStopHighlight != null)
                            OnStopHighlight.Invoke(hand, currentHighlightTarget);
                        currentHighlightTarget.Unhighlight(hand, null, ignoreHighlightEvents);
                        currentHighlightTarget = null;
                    }

                    for(int i = 0; i < foundHighlightGrabbables.Count; i++) {
                        foundHighlightGrabbables[i].ResetOriginalLayers();
                    }
                }
                else if(currentHighlightTarget != null) {
                    //Just in case the object your hand is looking at is destroyed
                    if(!ignoreHighlightEvents && OnStopHighlight != null)
                        OnStopHighlight.Invoke(hand, currentHighlightTarget);

                    currentHighlightTarget.Unhighlight(hand, null, ignoreHighlightEvents);

                    currentHighlightTarget = null;
                }
            }
        }

        public void ClearHighlights() {
            if(currentHighlightTarget != null) {
                OnStopHighlight?.Invoke(hand, currentHighlightTarget);
                currentHighlightTarget.Unhighlight(hand);
                currentHighlightTarget = null;
            }
        }

        /// <summary>Returns the closest raycast hit from the hand's highlighting system, if no highlight, returns blank raycasthit</summary>
        public RaycastHit GetHighlightHit() {
            _highlightHit.point = hand.handGrabPoint.position;
            _highlightHit.normal = hand.handGrabPoint.up;
            return _highlightHit;
        }



        Collider[] handHighlightNonAlloc = new Collider[128];
        /// <summary>Finds the closest raycast from a cone of rays -> Returns average direction of all hits</summary>
        public virtual Vector3 HandClosestHit(out RaycastHit closestHit, out IGrabbableEvents grabbable, int layerMask, Grabbable target = null) {
            Grabbable grab;
            Vector3 palmForward = hand.palmTransform.forward;
            Vector3 palmRight = hand.palmTransform.right;
            Vector3 palmPosition = hand.palmTransform.position;
            GameObject rayHitObject;
            Grabbable lastRayHitGrabbable = null;
            Ray ray = new Ray();
            RaycastHit hit;
            Collider col;

            closestGrabs.Clear();
            closestHits.Clear();
            var checkSphereRadius = hand.reachDistance * 1.35f;
            int overlapCount = Physics.OverlapSphereNonAlloc(palmPosition + palmForward * (checkSphereRadius * 0.5f), checkSphereRadius, handHighlightNonAlloc, layerMask, QueryTriggerInteraction.Collide);


            for(int i = 0; i < overlapCount; i++) {
                col = handHighlightNonAlloc[i];

                if(!(col is MeshCollider) || (col as MeshCollider).convex == true) {
                    Vector3 closestPoint = col.ClosestPoint(hand.palmTransform.transform.position);
                    ray.direction = closestPoint -hand.palmTransform.position;
                }
                else
                    ray.direction =hand.palmTransform.forward;

                ray.origin =hand.palmTransform.transform.position;
                ray.origin = Vector3.MoveTowards(ray.origin, col.bounds.center, 0.001f);

                var queryTriggerInteraction = QueryTriggerInteraction.Ignore;
                if(col.isTrigger)
                    queryTriggerInteraction = QueryTriggerInteraction.Collide;


                if(ray.direction != Vector3.zero && Vector3.Angle(ray.direction,hand.palmTransform.forward) < 120 && Physics.Raycast(ray, out hit, checkSphereRadius*2, layerMask, queryTriggerInteraction)) {

                    rayHitObject = hit.collider.gameObject;
                    if(closestGrabs.Count > 0)
                        lastRayHitGrabbable = closestGrabs[closestGrabs.Count - 1];

                    if(closestGrabs.Count > 0 && rayHitObject == lastRayHitGrabbable.gameObject) {
                        if(target == null) {
                            closestGrabs.Add(lastRayHitGrabbable);
                            closestHits.Add(hit);
                        }
                    }
                    else if(rayHitObject.HasGrabbable(out grab) && hand.CanGrab(grab)) {
                        if(target == null || target == grab) {
                            closestGrabs.Add(grab);
                            closestHits.Add(hit);
                        }
                    }
                }
            }

            int closestHitCount = closestHits.Count;

            if(closestHitCount > 0) {
                closestHit = closestHits[0];
                grabbable = closestGrabs[0];
                Vector3 dir = Vector3.zero;
                float grabPriorityWeight = (grabbable is Grabbable) ? (grabbable as Grabbable).grabPriorityWeight : 1f;
                var targetDirection = Vector3.Lerp(palmForward, palmRight, palmForwardRightDirection);

                for(int i = 0; i < closestHitCount; i++) {
                    var newDistance = closestHits[i].distance / closestGrabs[i].grabPriorityWeight;
                    var newDot = Vector3.Dot(targetDirection, closestHits[i].point -hand.palmTransform.position)/2f * hand.reachDistance;
                    var currentDistance = closestHit.distance / grabPriorityWeight;
                    var currentDot = Vector3.Dot(targetDirection, closestHit.point -hand.palmTransform.position)/2f * hand.reachDistance;

                    if(newDistance-newDot < currentDistance-currentDot) {
                        closestHit = closestHits[i];
                        grabbable = closestGrabs[i];
                    }

                    dir += closestHits[i].point - hand.palmTransform.position;
                }

                if(hand.holdingObj == null && !hand.IsGrabbing()) {
                    if(hand.handGrabPoint.parent != closestHit.transform)
                        hand.handGrabPoint.parent = closestHit.collider.transform;
                    hand.handGrabPoint.position = closestHit.point;
                    hand.handGrabPoint.up = closestHit.normal;
                }

                return dir / closestHitCount;
            }

            closestHit = new RaycastHit();
            grabbable = null;
            return Vector3.zero;
        }

    }
}
