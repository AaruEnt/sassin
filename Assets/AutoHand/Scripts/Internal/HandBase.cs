using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

namespace Autohand {

    /// <summary>
    /// 
    /// </summary>
    public enum HandMovementType {
        /// <summary>Movement method for Auto Hand V2 and below</summary>
        Legacy,
        /// <summary>Uses physics forces</summary>
        Forces
    }

    public enum HandType {
        both,
        right,
        left,
        none
    }

    public enum GrabType {
        /// <summary>On grab, hand will move to the grabbable, create grab connection, then return to follow position</summary>
        HandToGrabbable,
        /// <summary>On grab, grabbable will move to the hand, then create grab connection</summary>
        GrabbableToHand,
        /// <summary>On grab, grabbable instantly travel to the hand</summary>
        InstantGrab
    }

    [Serializable]
    public struct VelocityTimePair {
        public float time;
        public Vector3 velocity;
    }

    public delegate void HandGrabEvent(Hand hand, Grabbable grabbable);
    public delegate void HandGameObjectEvent(Hand hand, GameObject other);

    [Serializable]  public class UnityHandGrabEvent : UnityEvent<Hand, Grabbable> { }
    [Serializable] public class UnityHandEvent : UnityEvent<Hand> { }



    [RequireComponent(typeof(Rigidbody)), RequireComponent(typeof(HandFollow)), RequireComponent(typeof(HandAnimator)), RequireComponent(typeof(HandGrabbableHighlighter)), DefaultExecutionOrder(10)]
    /// <summary>This is the base of the Auto Hand hand class, used for organizational purposes</summary>
    public class HandBase : MonoBehaviour {


        [AutoHeader("AUTO HAND")]
        public bool ignoreMe;

        public Finger[] fingers;

        [Tooltip("An empty GameObject that should be placed on the surface of the center of the palm")]
        public Transform palmTransform;
        [Tooltip("An empty GameObject that should be placed on the surface of the center of pinch point")]
        public Transform pinchPointTransform;

        [FormerlySerializedAs("isLeft")]
        [Tooltip("Whether this is the left (on) or right (off) hand")]
        public bool left = false;


        [Space]
        [Tooltip("Maximum distance for pickup"), Min(0.01f)]
        public float reachDistance = 0.2f;


        [AutoToggleHeader("Enable Movement", 0, 0, tooltip = "Whether or not to enable the hand's Rigidbody Physics movement")]
        public bool enableMovement = true;

        [EnableIf("enableMovement"), Tooltip("Follow target, the hand will always try to match this transforms position with rigidbody movements")]
        public Transform follow;

        [EnableIf("enableMovement"), Tooltip("Amplifier for applied velocity on released object"), Min(0)]
        public float throwPower = 1.25f;

        [Tooltip("Speed at which the gentle grab returns the grabbable"), Min(0)]
        [FormerlySerializedAs("smoothReturnSpeed")]
        public float gentleGrabSpeed = 1;

        [HideInInspector]
        public bool advancedFollowSettings = true;

        [AutoToggleHeader("Enable Auto Posing", 0, 0, tooltip = "Auto Posing will override Unity Animations -- This will disable all the Auto Hand IK, including animations from: finger sway, pose areas, finger bender scripts (runtime Auto Posing will still work)")]
        [Tooltip("Turn this on when you want to animate the hand or use other IK Drivers")]
        public bool enableIK = true;

        [EnableIf("enableIK"), Tooltip("How much the fingers sway from the velocity")]
        public float swayStrength = 0.4f;

        [EnableIf("enableIK"), Tooltip("This will offset each fingers bend (0 is no bend, 1 is full bend)")]
        public float gripOffset = 0.14f;



        [HideInInspector, NonSerialized, Tooltip("After this many seconds velocity data within a 'throw window' will be tossed out. (This allows you to get only use acceeleration data from the last 'x' seconds of the throw.)")]
        public float throwVelocityExpireTime = 0.125f;
        [HideInInspector, NonSerialized, Tooltip("After this many seconds velocity data within a 'throw window' will be tossed out. (This allows you to get only use acceeleration data from the last 'x' seconds of the throw.)")]
        public float throwAngularVelocityExpireTime = 0.25f;

        [HideInInspector, NonSerialized, Tooltip("Increase for closer finger tip results / Decrease for less physics checks - The number of steps the fingers take when bending to grab something")]
        public int fingerBendSteps = 40;

        [HideInInspector]
        public bool usingPoseAreas = true;

        [HideInInspector]
        public QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.Ignore;




        HandFollow _handFollow;
        public HandFollow handFollow {
            get {
                if(_handFollow == null)
                    _handFollow = GetComponent<HandFollow>();

                return _handFollow;
            }
        }


        HandAnimator _handAnimator;
        public HandAnimator handAnimator {
            get {
                if(_handAnimator == null)
                    _handAnimator = GetComponent<HandAnimator>();

                return _handAnimator;
            }
        }


        HandGrabbableHighlighter _highlighter;
        public HandGrabbableHighlighter highlighter {
            get {
                if(_highlighter == null)
                    _highlighter = GetComponent<HandGrabbableHighlighter>();

                return _highlighter;
            }
        }


        private CollisionTracker _collisionTracker;
        public CollisionTracker collisionTracker {
            get {
                if(_collisionTracker == null)
                    _collisionTracker = gameObject.AddComponent<CollisionTracker>();
                return _collisionTracker;
            }
            protected set {
                if(_collisionTracker != null)
                    Destroy(_collisionTracker);

                _collisionTracker = value;
            }
        }


        public HandVelocityTracker velocityTracker {
            get; protected set;
        }


        Rigidbody _body;
        public Rigidbody body { 
            get{
                if(_body == null)
                    _body = GetComponent<Rigidbody>();

                return _body;
            }
            internal set { _body = body; } 
        }

        public Transform moveTo {
            get {
                return handFollow.moveTo;
            }
        }


        Grabbable HoldingObj = null;
        public Grabbable holdingObj {
            get { return HoldingObj; }
            internal set { HoldingObj = value; }
        }


        protected GrabbablePose _currentHeldPose;
        public GrabbablePose currentHeldPose {
            get {
                return _currentHeldPose;
            }
    
            internal set {
                if(value == null && _currentHeldPose != null)
                    _currentHeldPose.CancelHandPose(this as Hand);

                _currentHeldPose = value;
            }
        }


        Transform _handGrabPoint;
        /// <summary>This is a transform the represents where the held hand is relative to the object in local space</summary>
        public Transform handGrabPoint {
            get {
                if(_handGrabPoint == null && gameObject.scene.isLoaded) {
                    _handGrabPoint = new GameObject().transform;
                    _handGrabPoint.name = "grabPoint";
                }
                return _handGrabPoint;
            }
        }


        Transform _localGrabbablePoint;
        /// <summary>This is a transform the represents where the held object should be relative to the hand in local space</summary>
        public Transform localGrabbablePoint {
            get {
                if(!gameObject.activeInHierarchy)
                    _localGrabbablePoint = null;
                else if(gameObject.activeInHierarchy && _localGrabbablePoint == null) {
                    _localGrabbablePoint = new GameObject().transform;
                    _localGrabbablePoint.name = "grabPosition";
                    _localGrabbablePoint.parent = transform;
                }


                return _localGrabbablePoint;
            }
        }



        Vector3 _grabPositionOffset = Vector3.zero;
        public Vector3 grabPositionOffset {
            get { return _grabPositionOffset; }
            set { _grabPositionOffset = value; }
        }

        Quaternion _grabRotationOffset = Quaternion.identity;
        public Quaternion grabRotationOffset {
            get { return _grabRotationOffset; }
            set { _grabRotationOffset = value; }
        }




        [HideInInspector, NonSerialized]
        public ConfigurableJoint heldJoint;

        public bool grabbing { get; protected set; }
        public bool squeezing { get; protected set; }

        protected float gripAxis;
        protected float squeezeAxis;
        
        internal List<Collider> handColliders = new List<Collider>();

        internal float lastGrabTime = 0;
        internal float lastReleaseTime = 0;


        BoxCollider _handEncapsulationCollider;
        internal BoxCollider handEncapsulationBox {
            get {
                if(!gameObject.activeInHierarchy)
                    _handEncapsulationCollider = null;
                else if(gameObject.activeInHierarchy && _handEncapsulationCollider == null) {
                    _handEncapsulationCollider = new GameObject().AddComponent<BoxCollider>();
                    _handEncapsulationCollider.name = "handEncapsulationBox";
                    _handEncapsulationCollider.transform.parent = transform;
                    _handEncapsulationCollider.transform.localPosition = Vector3.zero;
                    _handEncapsulationCollider.transform.localRotation = Quaternion.identity;
                    _handEncapsulationCollider.transform.localScale = Vector3.one;
                    _handEncapsulationCollider.isTrigger = true;
                    _handEncapsulationCollider.enabled = false;
                }

                return _handEncapsulationCollider;
            }
        }

        internal int handLayers;
        internal int handIgnoreCollisionLayers;

        protected Transform palmChild;
        protected Collider palmCollider;
        protected RaycastHit grabbingHit;

        protected int noCollisionFrames = 0;
        protected int collisionFrames = 0;

        protected bool prerendered = false;
        protected Vector3 preRenderPos;
        protected Quaternion preRenderRot;

        protected virtual void Awake() {
            body = GetComponent<Rigidbody>();
            body.interpolation = RigidbodyInterpolation.None;
            body.useGravity = false;

            body.solverIterations = 100;
            body.solverVelocityIterations = 100;

            if(palmCollider == null) {
                palmCollider = palmTransform.gameObject.AddComponent<BoxCollider>();
                (palmCollider as BoxCollider).size = new Vector3(0.2f, 0.15f, 0.05f);
                (palmCollider as BoxCollider).center = new Vector3(0f, 0f, -0.025f);
                palmCollider.enabled = false;
            }

            if(palmChild == null) {
                palmChild = new GameObject().transform;
                palmChild.parent = palmTransform;
            }

            var cams = AutoHandExtensions.CanFindObjectsOfType<Camera>(true);
            foreach(var cam in cams) {
                if(cam.targetDisplay == 0) {
                    bool found = false;
                    var handStabilizers = cam.gameObject.GetComponents<HandStabilizer>();
                    foreach(var handStabilizer in handStabilizers) {
                        if(handStabilizer.hand == this)
                            found = true;
                    }
                    if(!found)
                        cam.gameObject.AddComponent<HandStabilizer>().hand = this;
                }
            }
            
            if(velocityTracker == null)
                velocityTracker = new HandVelocityTracker(this);


            if(AutoHandSettings.UsingDynamicTimestep()) {
                if(AutoHandExtensions.CanFindObjectOfType<DynamicTimestepSetter>() == null) {
                    new GameObject() { name = "DynamicFixedTimeSetter" }.AddComponent<DynamicTimestepSetter>();
                    Debug.Log("AUTO HAND: Creating Dynamic Timestepper");
                }
            }


            //Update the hand encapsulation sphere
            var bounds = new Bounds(Vector3.zero, Vector3.zero);
            foreach(var finger in fingers) {
                var fingerJoints = finger.FingerJoints;
                for(int i = 0; i < fingerJoints.Length; i++)
                    bounds.Encapsulate(transform.InverseTransformPoint(fingerJoints[i].position));
                bounds.Encapsulate(transform.InverseTransformPoint(finger.tip.position + (finger.tip.position - transform.position)*finger.tipRadius));
            }

            bounds.Encapsulate(transform.InverseTransformPoint(palmTransform.position + palmTransform.forward*0.01f));
            bounds.Encapsulate(transform.InverseTransformPoint(palmTransform.position - palmTransform.forward*0.01f));

            handEncapsulationBox.center = bounds.center;
            handEncapsulationBox.size = bounds.size;
            handEncapsulationBox.gameObject.layer = LayerMask.NameToLayer(left ? Hand.leftHandLayerName : Hand.rightHandLayerName);
        }

        protected virtual void OnEnable() {
            SetHandCollidersRecursive(transform);
        }

        protected virtual void OnDisable() {
            handColliders.Clear();
        }

        protected virtual void OnDestroy() {
            if(_handGrabPoint != null)
                Destroy(_handGrabPoint.gameObject);
            if(_localGrabbablePoint != null)
                Destroy(_localGrabbablePoint.gameObject);
        }

        protected virtual void FixedUpdate(){
            velocityTracker.UpdateThrowing();

            if(CollisionCount() > 0) {
                noCollisionFrames = 0;
                collisionFrames++;
            }
            else {
                noCollisionFrames++;
                collisionFrames = 0;
            }

            if(holdingObj != null)
                holdingObj.HeldFixedUpdate();
        }


        //This is used to force the hand to always look like its where it should be even when physics is being weird
        //public virtual void OnPreRender() {
        //    if(!prerendered) {
        //        preRenderPos = transform.position;
        //        preRenderRot = transform.rotation;
        //    }

        //    //Hides fixed joint jitterings
        //    if(holdingObj != null && holdingObj.customGrabJoint == null && !IsGrabbing()) {
        //        //Debug.Log(name + ": PRERENDERED");
        //        transform.position = handGrabPoint.position;
        //        transform.rotation = handGrabPoint.rotation;
        //        prerendered = true;
        //    }
        //}

        //This is used to force the hand to always look like its where it should be even when physics is being weird
        public virtual void OnWillRenderObject(){
            if(!prerendered) {
                preRenderPos = transform.position;
                preRenderRot = transform.rotation;
            }

            //Hides fixed joint jitterings
            if(holdingObj != null && holdingObj.customGrabJoint == null && !IsGrabbing()) {
                //Debug.Log(name + ": PRERENDERED - " + Time.time);
                transform.position = handGrabPoint.position;
                transform.rotation = handGrabPoint.rotation;
                prerendered = true;
            }
        }

        //This puts everything where it should be for the physics update
        public virtual void OnPostRender(){
            //Returns position after hiding for camera
            if(prerendered && holdingObj != null && holdingObj.customGrabJoint == null && !IsGrabbing()) {
                //Debug.Log(name + ": POSTRENDERED - " + Time.time);
                transform.position = preRenderPos;
                transform.rotation = preRenderRot;
            }

            prerendered = false;
        }


        public float GetTriggerAxis() {
            return gripAxis;
        }



        protected void SetHandCollidersRecursive(Transform obj) {
            handColliders.Clear();
            AddHandCol(obj);

            void AddHandCol(Transform obj1) {
                foreach(var col in obj1.GetComponents<Collider>())
                    handColliders.Add(col);

                for(int i = 0; i < obj1.childCount; i++) {
                    AddHandCol(obj1.GetChild(i));
                }
            }
        }



        /// <summary>Returns the current throw velocity</summary>
        public Vector3 ThrowVelocity() { return velocityTracker.ThrowVelocity(); }

        /// <summary>Returns the current throw angular velocity</summary>
        public Vector3 ThrowAngularVelocity() { return velocityTracker.ThrowAngularVelocity(); }



        public  int CollisionCount() {
            if(holdingObj != null)
                return collisionTracker.collisionObjects.Count + holdingObj.CollisionCount();
            return collisionTracker.collisionObjects.Count;
        }


        /// <summary>Returns true during the time between when a grab starts and a hold begins</summary>
        public bool IsGrabbing() {
            return grabbing;
        }


        public bool IsHolding() {
            return holdingObj != null;
        }


        public static int GetHandsLayerMask() {
            return LayerMask.GetMask(Hand.rightHandLayerName, Hand.leftHandLayerName);
        }



        protected virtual void OnDrawGizmosSelected() {
            var radius = reachDistance;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(palmTransform.position + palmTransform.forward * radius, radius);
        }
    }
}