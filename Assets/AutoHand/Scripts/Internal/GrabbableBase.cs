using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using NaughtyAttributes;
using UnityEditor;
using UnityEngine.Serialization;

namespace Autohand {

    [DefaultExecutionOrder(-100)]
    public class GrabbableBase : MonoBehaviour {

        [AutoHeader("Grabbable")]
        public bool ignoreMe;

        [Tooltip("The physics body to connect this colliders grab to - if left empty will default to local body")]
        public Rigidbody body;

        [Tooltip("A copy of the mesh will be created and slighly scaled and this material will be applied to create a highlight effect with options")]
        public Material hightlightMaterial;

        [HideInInspector]
        public bool isGrabbable = true;


        private PlacePoint _placePoint = null;
        public PlacePoint placePoint { get { return _placePoint; } protected set { _placePoint = value; } }

        protected List<PlacePoint> _childPlacePoints = new List<PlacePoint>();
        public List<PlacePoint> childPlacePoints { get { return _childPlacePoints; } }


        internal List<Collider> _grabColliders = new List<Collider>();
        public List<Collider> grabColliders { get { return _grabColliders; } }


        internal Grabbable rootGrabbable;
        internal List<Grabbable> grabbableChildren = new List<Grabbable>();
        internal List<Grabbable> grabbableParents = new List<Grabbable>();
        internal List<Grabbable> jointedGrabbables = new List<Grabbable>();
        internal List<GrabbableChild> grabChildren = new List<GrabbableChild>();

        public float targetMass { get; protected set; }
        public float targetDrag { get; protected set; }
        public float targetAngularDrag { get; protected set; }



        protected Dictionary<Collider, PhysicMaterial> grabColliderMaterials = new Dictionary<Collider, PhysicMaterial>();
        protected Dictionary<Transform, int> originalLayers = new Dictionary<Transform, int>();

        private List<Hand> _heldBy = new List<Hand>();
        public List<Hand> heldBy {
            get { return _heldBy; }
        }

        private List<Hand> _beingGrabbedBy = new List<Hand>();
        public List<Hand> beingGrabbedBy {
            get { return _beingGrabbedBy; }
        }

        protected List<Hand> waitingToGrabHands = new List<Hand>();

        protected bool hightlighting;
        protected GameObject highlightObj;
        protected PlacePoint lastPlacePoint = null;

        public Transform originalParent { get; set; }
        protected Vector3 lastCenterOfMassPos;
        protected Quaternion lastCenterOfMassRot;
        protected CollisionDetectionMode detectionMode;
        protected RigidbodyInterpolation startInterpolation;

        public bool beingGrabbed { get; protected internal set; }
        internal bool beforeGrabFrame { get; set; }
        protected bool wasIsGrabbable = false;
        protected bool beingDestroyed = false;
        protected Dictionary<Hand, Coroutine> resetLayerRoutine = new Dictionary<Hand, Coroutine>();
        protected Dictionary<Hand, Coroutine> ignoreWhileGrabbingRoutine = new Dictionary<Hand, Coroutine>();
        protected List<Transform> jointedParents = new List<Transform>();
        protected Dictionary<Material, List<GameObject>> highlightObjs = new Dictionary<Material, List<GameObject>>();

        protected GrabbablePoseCombiner poseCombiner;
        protected float lastUpdateTime;

        protected bool rigidbodyDeactivated = false;
        protected SaveRigidbodyData rigidbodyData;

        /// <summary>This transform represents the root rigidbody gameobject. This is used in place a rigidbody call just in case the rigidbody is disabled</summary>
        public Transform rootTransform {
            get {
                if(body != null)
                    return body.transform;
                else if(rigidbodyData.IsSet())
                    return rigidbodyData.GetOrigin();
                else if(gameObject.CanGetComponent<Rigidbody>(out var rigidbody))
                    return rigidbody.transform;
                else if(gameObject.GetComponentInParent<Rigidbody>() != null)
                    return gameObject.GetComponentInParent<Rigidbody>().transform;
                else
                    return null;
            }
        }


        private CollisionTracker _collisionTracker;
        public CollisionTracker collisionTracker {
            get {
                if(_collisionTracker == null) {
                    if(!(_collisionTracker = GetComponent<CollisionTracker>())) {
                        _collisionTracker = gameObject.AddComponent<CollisionTracker>();
                        _collisionTracker.disableTriggersTracking = true;
                    }
                }
                return _collisionTracker;
            }
            protected set {
                if(_collisionTracker != null)
                    Destroy(_collisionTracker);

                _collisionTracker = value;
            }
        }

#if UNITY_EDITOR
        protected bool editorSelected = false;
#endif

        public virtual void Awake() {
            if(!gameObject.CanGetComponent(out poseCombiner))
                poseCombiner = gameObject.AddComponent<GrabbablePoseCombiner>();

            GetPoseSaves(transform);

            void GetPoseSaves(Transform obj) {
                if(obj.CanGetComponent(out Grabbable grab) && grab != this)
                    return;

                var poses = obj.GetComponents<GrabbablePose>();
                for(int i = 0; i < poses.Length; i++) {
                    poseCombiner.AddPose(poses[i]);
                    poses[i].grabbable = (this as Grabbable);
                }

                for(int i = 0; i < obj.childCount; i++)
                    GetPoseSaves(obj.GetChild(i));
            }

            if(body == null) {
                if(GetComponent<Rigidbody>())
                    body = GetComponent<Rigidbody>();
                else
                    Debug.LogError("RIGIDBODY MISSING FROM GRABBABLE: " + transform.name + " \nPlease add/attach a rigidbody", this);
            }

#if UNITY_EDITOR
            if(Selection.activeGameObject == gameObject) {
                Selection.activeGameObject = null;
                Debug.Log("Auto Hand (EDITOR ONLY): Selecting the grabbable in the inspector can cause lag and quality reduction at runtime. (Automatically deselecting at runtime) Remove this code at any time.", this);
                editorSelected = true;
            }

            Application.quitting += () => { if(editorSelected) Selection.activeGameObject = gameObject; };
#endif

            originalParent = body.transform.parent;
            detectionMode = body.collisionDetectionMode;
            startInterpolation = body.interpolation;
            UpdateGrabbableColliderSettings();
            UpdateGrabbableRigidbodySettings(body.drag, body.angularDrag, body.mass);
        }


        private void OnDestroy() {
            beingDestroyed = true;
        }

        public virtual void HeldFixedUpdate() {
            if(heldBy.Count > 0) {
                lastCenterOfMassRot = body.transform.rotation;
                lastCenterOfMassPos = body.transform.position;
            }

        }

        protected virtual void OnDisable() {
            foreach(var routine in resetLayerRoutine) {
                IgnoreHand(routine.Key, false);
                if(routine.Value != null)
                    StopCoroutine(routine.Value);
            }
            resetLayerRoutine.Clear();

            foreach(var routine in ignoreGrabbableCollisions) {
                if(routine.Value != null)
                    StopCoroutine(routine.Value);
            }
            ignoreGrabbableCollisions.Clear();

            foreach(var routine in ignoreHandCollisions) {
                if(routine.Value != null)
                    StopCoroutine(routine.Value);
            }
            ignoreHandCollisions.Clear();

        }


        public void SetPlacePoint(PlacePoint point) {
            this.placePoint = point;

            foreach(var grabbable in grabbableChildren) {
                grabbable.placePoint = point;
            }
        }

        public void SetGrabbableChild(GrabbableChild child) {
            child.grabParent = this as Grabbable;
            if(!grabChildren.Contains(child))
                grabChildren.Add(child);
        }


        public void DeactivateRigidbody() {
            if(body != null) {
                if(body != null)
                    rigidbodyData = new SaveRigidbodyData(body);

                body = null;
                rigidbodyDeactivated = true;
            }

            foreach(var grabbable in grabbableChildren) {
                if(grabbable.body != null) {
                    grabbable.body = null;
                    grabbable.rigidbodyData = new SaveRigidbodyData(rigidbodyData);
                    grabbable.rigidbodyDeactivated = true;
                }
            }
        }


        public void ActivateRigidbody() {
            if(rigidbodyDeactivated && !beingDestroyed) {
                rigidbodyDeactivated = false;
                body = rigidbodyData.ReloadRigidbody();

                foreach(var grabbable in grabbableChildren) {
                    grabbable.rigidbodyDeactivated = false;
                    if(grabbable.body == null)
                        grabbable.body = body;
                }
            }
        }



        protected internal void SetLayerRecursive(int newLayer) {
            foreach(var transform in originalLayers) {
                transform.Key.gameObject.layer = newLayer;
            }
        }

        /// <summary>Sets the grabbable and children to the physics layers it had on Start()</summary>
        protected internal void ResetOriginalLayers() {
            foreach(var transform in originalLayers) {
                transform.Key.gameObject.layer = transform.Value;
            }
        }


        Dictionary<Grabbable, Coroutine> ignoreGrabbableCollisions = new Dictionary<Grabbable, Coroutine>();
        public void IgnoreGrabbableCollisionUntilNone(Grabbable other) {
            if(!beingDestroyed && !ignoreGrabbableCollisions.ContainsKey(other))
                ignoreGrabbableCollisions.Add(other, StartCoroutine(IgnoreGrabbableCollisionUntilNoneRoutine(other)));
        }

        protected IEnumerator IgnoreGrabbableCollisionUntilNoneRoutine(Grabbable other) {
            IgnoreGrabbableColliders(other, true);

            yield return new WaitForSeconds(0.05f);
            while(IsGrabbableOverlapping(other))
                yield return new WaitForSeconds(0.1f);

            IgnoreGrabbableColliders(other, false);
            ignoreGrabbableCollisions.Remove(other);

            if(ignoreGrabbableCollisions.ContainsKey(other))
                ignoreGrabbableCollisions.Remove(other);

        }

        public bool IsGrabbableOverlapping(Grabbable other) {
            foreach(var col1 in grabColliders) {
                foreach(var col2 in other.grabColliders) {
                    if(col1.enabled && !col1.isTrigger && !col1.isTrigger && col2.enabled && !col2.isTrigger && !col2.isTrigger &&
                        Physics.ComputePenetration(col1, col1.transform.position, col1.transform.rotation, col2, col2.transform.position, col2.transform.rotation, out _, out _)) {
                        return true;
                    }
                }
            }

            return false;
        }

        public void IgnoreGrabbableColliders(Grabbable other, bool ignore) {
            foreach(var col1 in grabColliders) {
                foreach(var col2 in other.grabColliders) {
                    Physics.IgnoreCollision(col1, col2, ignore);
                }
            }
        }




        Dictionary<Hand, Coroutine> ignoreHandCollisions = new Dictionary<Hand, Coroutine>();
        public void IgnoreHandCollisionUntilNone(Hand hand, float minIgnoreTime = 1) {
            if(gameObject.activeInHierarchy && !beingDestroyed && !ignoreHandCollisions.ContainsKey(hand))
                ignoreHandCollisions.Add(hand, StartCoroutine(IgnoreHandCollisionUntilNoneRoutine(hand, minIgnoreTime)));
        }

        protected IEnumerator IgnoreHandCollisionUntilNoneRoutine(Hand hand, float minIgnoreTime) {
            if(!ignoringHand.ContainsKey(hand) || !ignoringHand[hand]) {
                IgnoreHand(hand, true);

                yield return new WaitForSeconds(minIgnoreTime);
                if(minIgnoreTime != 0)
                    while(IsHandOverlapping(hand))
                        yield return new WaitForSeconds(0.1f);

                IgnoreHand(hand, false);
                if(resetLayerRoutine.ContainsKey(hand))
                    resetLayerRoutine.Remove(hand);
                if(ignoreHandCollisions.ContainsKey(hand))
                    ignoreHandCollisions.Remove(hand);
            }
        }


        protected IEnumerator IgnoreHandCollision(Hand hand, float time) {
            if(!ignoringHand.ContainsKey(hand) || !ignoringHand[hand]) {
                IgnoreHand(hand, true);

                yield return new WaitForSeconds(time);

                IgnoreHand(hand, false);
                resetLayerRoutine.Remove(hand);
            }
        }

        protected Dictionary<Hand, bool> ignoringHand = new Dictionary<Hand, bool>();
        public void IgnoreHand(Hand hand, bool ignore, bool overrideIgnoreRoutines = false) {
            if(overrideIgnoreRoutines && resetLayerRoutine.ContainsKey(hand) && resetLayerRoutine[hand] != null) {
                StopCoroutine(resetLayerRoutine[hand]);
                resetLayerRoutine[hand] = null;
            }

            foreach(var col in grabColliders)
                hand.HandIgnoreCollider(col, ignore);

            foreach(var grab in grabbableChildren)
                foreach(var col in grab.grabColliders)
                    hand.HandIgnoreCollider(col, ignore);

            foreach(var grab in grabbableParents)
                foreach(var col in grab.grabColliders)
                    hand.HandIgnoreCollider(col, ignore);

            if(!ignoringHand.ContainsKey(hand))
                ignoringHand.Add(hand, ignore);
            else
                ignoringHand[hand] = ignore;
        }


        public bool IsHandOverlapping(Hand hand) {
            float dist;
            Vector3 dir;
            foreach(var col2 in grabColliders) {
                foreach(var col1 in hand.handColliders) {
                    if(col1.enabled && !col1.isTrigger && !col1.isTrigger && col2.enabled && !col2.isTrigger && !col2.isTrigger &&
                    Physics.ComputePenetration(col1, col1.transform.position, col1.transform.rotation, col2, col2.transform.position, col2.transform.rotation, out dir, out dist)) {
                        return true;
                    }
                }
            }

            return false;
        }








        public bool GetSavedPose(out GrabbablePoseCombiner pose) {
            if(poseCombiner != null && poseCombiner.PoseCount() > 0) {
                pose = poseCombiner;
                return true;
            }
            else {
                pose = null;
                return false;
            }
        }

        public bool HasCustomPose() {
            return poseCombiner.PoseCount() > 0;
        }


        /// <summary>Resets the physics materials on all the colliders to the given physics material</summary>
        public void SetPhysicsMaterial(PhysicMaterial physMat) {
            foreach(var collider in grabColliders) {
                collider.material = physMat;
            }
        }

        /// <summary>Resets the physics materials on all the colliders to how it was during last UpdateGrabbableColliderSettings()</summary>
        public void ResetPhysicsMateiral() {
            foreach(var col in grabColliderMaterials)
                col.Key.sharedMaterial = col.Value;
        }


        /// <summary> Saves the grabbables target collider settings to be whatever the current collider settings are. DO NOT CALL THIS WHILE THE GRABBABLE IS HELD </summary>
        public void UpdateGrabbableColliderSettings() {
            grabColliders.Clear();
            grabColliderMaterials.Clear();
            originalLayers.Clear();

            var colliders = body.GetComponentsInChildren<Collider>();
            foreach(var col in colliders) {
                if(col.isTrigger)
                    continue;

                grabColliders.Add(col);
                if(col.sharedMaterial == null)
                    grabColliderMaterials.Add(col, null);
                else
                    grabColliderMaterials.Add(col, col.sharedMaterial);

                if(!originalLayers.ContainsKey(col.transform)) {
                    if(col.gameObject.layer == LayerMask.NameToLayer("Default") || LayerMask.LayerToName(col.gameObject.layer) == "")
                        col.gameObject.layer = LayerMask.NameToLayer(Hand.grabbableLayerNameDefault);
                    originalLayers.Add(col.transform, col.gameObject.layer);
                }
            }
        }


        /// <summary>Sets the grabbables rigidbody settigns and target settings when not held</summary>
        public void UpdateGrabbableRigidbodySettings(float drag, float angularDrag, float mass) {
            targetAngularDrag = angularDrag;
            targetDrag = drag;
            targetMass = mass;

            if(body != null && (this as Grabbable).HeldCount() == 0) {
                body.drag = drag;
                body.angularDrag = angularDrag;
                body.mass = mass;
            }
    }


        /// <summary>Adds a grabbables collider to this list of colliders on this grabbable, REMOVE_GRABBABLE_COLLIDERS() MUST BE CALLED</summary>
        public void AddGrabbableColliders(Grabbable other) {
            var ignoreHandKeys = new List<Hand>(ignoreHandCollisions.Keys);
            foreach(var col in other.grabColliders) {
                if(!grabColliders.Contains(col)) {
                    grabColliders.Add(col);
                    for(int i = 0; i < ignoreHandKeys.Count; i++)
                        ignoreHandKeys[i].HandIgnoreCollider(col, true);
                }
            }
        }

        public void RemoveGrabbableColliders(Grabbable other) {
            var ignoreHandKeys = new List<Hand>(ignoreHandCollisions.Keys);
            foreach(var col in other.grabColliders) {
                if(grabColliders.Contains(col)) {
                    grabColliders.Remove(col);
                    for(int i = 0; i < ignoreHandKeys.Count; i++)
                        ignoreHandKeys[i].HandIgnoreCollider(col, false);
                }
            }
        }




        public bool BeingDestroyed() {
            return beingDestroyed;
        }

        public void DebugBreak() {
#if UNITY_EDITOR
            Debug.Break();
#endif
        }


    }
}