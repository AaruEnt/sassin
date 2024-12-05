using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Autohand {
    [DefaultExecutionOrder(999)]
    public class WeightlessFollower : MonoBehaviour {
        [HideInInspector]
        public Transform follow1 = null;
        [HideInInspector]
        public Transform follow2 = null;
        [HideInInspector]
        public Hand hand1 = null;
        [HideInInspector]
        public Hand hand2 = null;

        public Dictionary<Hand, Transform> heldMoveTo = new Dictionary<Hand, Transform>();

        [HideInInspector]
        public float followPositionStrength = 30;
        [HideInInspector]
        public float followRotationStrength = 30;

        [HideInInspector]
        public float maxVelocity = 5;

        [HideInInspector]
        public Grabbable grab;

        Transform _pivot = null;
        public Transform pivot {
            get {
                if(!gameObject.activeInHierarchy)
                    return null;

                if(_pivot == null) {
                    _pivot = new GameObject().transform;
                    _pivot.parent = transform.parent;
                    _pivot.name = "WEIGHTLESS PIVOT";
                }

                return _pivot;
            }
        }

        internal Rigidbody body;
        Transform moveTo = null;

        float startMass = 0;
        float startDrag;
        float startAngleDrag;
        float startHandMass;
        float startHandDrag;
        float startHandAngleDrag;
        bool useGravity;


        public void Start() {
            if(body == null)
                body = GetComponent<Rigidbody>();
        }


        public virtual void Set(Hand hand, Grabbable grab) {
            if (body == null)
                body = grab.body;

            if(moveTo == null) {
                moveTo = new GameObject().transform;
                moveTo.name = gameObject.name + " FOLLOW POINT";
                moveTo.parent = AutoHandExtensions.transformParent;
            }

            if(!heldMoveTo.ContainsKey(hand)) {
                heldMoveTo.Add(hand, new GameObject().transform);
                heldMoveTo[hand].name = "HELD FOLLOW POINT";
            }

            var tempTransform = AutoHandExtensions.transformRuler;
            tempTransform.position = hand.transform.position;
            tempTransform.rotation = hand.transform.rotation;

            var tempTransformChild = AutoHandExtensions.transformRulerChild;
            tempTransformChild.position = grab.rootTransform.position;
            tempTransformChild.rotation = grab.rootTransform.rotation;

            if(grab.maintainGrabOffset) {
                tempTransform.position = hand.moveTo.position + hand.grabPositionOffset;
                tempTransform.rotation = hand.moveTo.rotation * hand.grabRotationOffset;
            }
            else {
                tempTransform.position = hand.moveTo.position;
                tempTransform.rotation = hand.moveTo.rotation;
            }

            heldMoveTo[hand].parent = hand.moveTo;
            heldMoveTo[hand].position = tempTransformChild.position;
            heldMoveTo[hand].rotation = tempTransformChild.rotation;


            if(follow1 == null) {
                follow1 = heldMoveTo[hand];
                hand1 = hand;
            }
            else if(follow2 == null) {
                follow2 = heldMoveTo[hand];
                hand2 = hand;
                pivot.parent = body.transform;
                pivot.position = Vector3.Lerp(hand1.handGrabPoint.position, hand2.handGrabPoint.position, 0.5f);
                pivot.rotation = Quaternion.LookRotation((hand1.handGrabPoint.position - hand2.handGrabPoint.position).normalized, 
                                 Vector3.Lerp(hand1.handGrabPoint.up, hand2.handGrabPoint.up, 0.5f));
            }


            if (startMass == 0) {
                startMass = body.mass;
                startDrag = grab.targetDrag;
                startAngleDrag = grab.targetAngularDrag;
                useGravity = body.useGravity;
            }


            startHandMass = hand.body.mass;
            startHandDrag = hand.handFollow.startDrag;
            startHandAngleDrag = hand.handFollow.startAngularDrag;

            body.mass = startHandMass;
            body.drag = startHandDrag;
            body.angularDrag = startHandAngleDrag;
            body.useGravity = false;

            followPositionStrength = hand.handFollow.followPositionStrength;
            followRotationStrength = hand.handFollow.followRotationStrength;


            maxVelocity = grab.maxHeldVelocity;
            this.grab = grab;

            hand.OnBeforeReleased += OnHandReleased;
        }


        void OnHandReleased(Hand hand, Grabbable grab){
            if(heldMoveTo.ContainsKey(hand))
                RemoveFollow(hand, heldMoveTo[hand]);
        }
        public virtual void FixedUpdate() {
            if(follow1 == null)
                return;

            if(follow2 != null) {
                pivot.position = Vector3.Lerp(hand1.handGrabPoint.position, hand2.handGrabPoint.position, 0.5f);
                pivot.rotation = Quaternion.LookRotation(
                    (hand1.handGrabPoint.position - hand2.handGrabPoint.position).normalized,
                    Vector3.Lerp(hand1.handGrabPoint.up, hand2.handGrabPoint.up, 0.5f)
                );
            }

            MoveTo();
            TorqueTo();

            if(grab.HeldCount() == 0)
                Destroy(this);
        }



        protected void SetMoveTo() {
            if(follow1 == null || moveTo == null)
                return;

            if(follow2 != null) {
                moveTo.position = Vector3.Lerp(hand1.moveTo.position, hand2.moveTo.position, 0.5f);
                moveTo.rotation = Quaternion.LookRotation((hand1.moveTo.position - hand2.moveTo.position).normalized,
                                 Vector3.Lerp(hand1.moveTo.up, hand2.moveTo.up, 0.5f));
                moveTo.position -= pivot.position - pivot.parent.transform.position;
                moveTo.rotation *= Quaternion.Inverse(pivot.localRotation);
            }
            else {
                moveTo.position = follow1.position;
                moveTo.rotation = follow1.rotation;
            }
        }



        /// <summary>Moves the hand to the controller position using physics movement</summary>
        protected virtual void MoveTo() {
            if(followPositionStrength <= 0 || moveTo == null)
                return;

            SetMoveTo();


            var movePos = moveTo.position;
            var distance = Vector3.Distance(movePos, transform.position);

            distance = Mathf.Clamp(distance, 0, 0.5f);

            SetVelocity(0.55f);


            void SetVelocity(float minVelocityChange) {
                var velocityClamp = grab.maxHeldVelocity;
                Vector3 vel = (movePos - transform.position).normalized * followPositionStrength * distance;

                vel.x = Mathf.Clamp(vel.x, -velocityClamp, velocityClamp);
                vel.y = Mathf.Clamp(vel.y, -velocityClamp, velocityClamp);
                vel.z = Mathf.Clamp(vel.z, -velocityClamp, velocityClamp);

                var deltaOffset = Time.fixedDeltaTime / 0.011111f;
                var inverseDeltaOffset = 0.011111f / Time.fixedDeltaTime;
                body.drag = startDrag * inverseDeltaOffset;
                var maxDelta = deltaOffset;
                minVelocityChange *= deltaOffset;

                body.velocity = new Vector3(
                    Mathf.MoveTowards(body.velocity.x, vel.x, minVelocityChange + Mathf.Abs(body.velocity.x) * maxDelta),
                    Mathf.MoveTowards(body.velocity.y, vel.y, minVelocityChange + Mathf.Abs(body.velocity.y) * maxDelta),
                    Mathf.MoveTowards(body.velocity.z, vel.z, minVelocityChange + Mathf.Abs(body.velocity.z) * maxDelta)
                );
            }
        }


        /// <summary>Rotates the hand to the controller rotation using physics movement</summary>
        protected virtual void TorqueTo() {
            var moveRot = moveTo.rotation;
            var delta = (moveRot * Quaternion.Inverse(body.rotation));
            delta.ToAngleAxis(out float angle, out Vector3 axis);

            if(float.IsInfinity(axis.x))
                return;

            if(angle > 180f)
                angle -= 360f;

            var multiLinear = Mathf.Deg2Rad * angle * followRotationStrength;
            Vector3 angular = multiLinear * axis.normalized;
            angle = Mathf.Abs(angle);

            var angleStrengthOffset = Mathf.Lerp(1f, 1.5f, angle/16f);
            var deltaOffset = Time.fixedDeltaTime / 0.011111f;
            var inverseDeltaOffset = 0.011111f / Time.fixedDeltaTime;
            body.angularDrag = Mathf.Lerp((startAngleDrag * 1.2f), startAngleDrag, angle/4f) * inverseDeltaOffset;
            var maxDelta = followRotationStrength * 50f * angleStrengthOffset;


            body.angularVelocity = new Vector3(
                Mathf.MoveTowards(body.angularVelocity.x, angular.x, maxDelta),
                Mathf.MoveTowards(body.angularVelocity.y, angular.y, maxDelta),
                Mathf.MoveTowards(body.angularVelocity.z, angular.z, maxDelta)
            );

        }


        int CollisionCount() {
            return grab.CollisionCount();
        }

        public void RemoveFollow(Hand hand, Transform follow) {
            hand.OnReleased -= OnHandReleased;

            if(this.follow1 == follow) {
                this.follow1 = null;
                hand1 = null;
            }
            if(follow2 == follow) {
                follow2 = null;
                hand2 = null;
            }

            if(this.follow1 == null && follow2 != null) {
                this.follow1 = follow2;
                this.hand1 = hand2;
                hand2 = null;
                follow2 = null;
            }

            if(this.follow1 == null && follow2 == null && !grab.beingGrabbed) {
                if(body != null) {
                    body.mass = startMass;
                    body.drag = startDrag;
                    body.angularDrag = startAngleDrag;
                    body.useGravity = useGravity;
                }
                Destroy(this);
            }

            heldMoveTo.Remove(hand);
        }

        private void OnDestroy()
        {
            if(moveTo != null)
                Destroy(moveTo.gameObject);

            foreach(var transform in heldMoveTo)
                Destroy(transform.Value.gameObject);

            if (body != null)
            {
                body.mass = startMass;
                body.drag = startDrag;
                body.angularDrag = startAngleDrag;
                body.useGravity = useGravity;
            }
        }


    }


}
