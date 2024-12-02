using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Autohand { 
    [RequireComponent(typeof(Hand)), DefaultExecutionOrder(0)]
    public class HandFollow : MonoBehaviour {

        Hand _hand;
        public Hand hand {
            get {
                if(_hand == null)
                    _hand = GetComponent<Hand>();
                return _hand;
            }
        }

        Transform follow;

        [Header("Move To Settings")]
        public float maxMoveToDistance = 0.1f;
        public float maxMoveToAngle = 45f;

        [Tooltip("Returns hand to the target after this distance [helps just in case it gets stuck]"), Min(0)]
        public float maxFollowDistance = 0.5f;
        [Tooltip("The maximum allowed velocity of the hand"), Min(0)]
        public float maxVelocity = 12f;

        [Header("Position Settings")]
        [Tooltip("Follow target speed (Can cause jittering if turned too high - recommend increasing drag with speed)"), Min(0)]
        public float followPositionStrength = 60;
        public float startDrag = 20f;
        [Tooltip("The drag multiplier the hand will lerp between the (start drag), and the (start drag * this) to when less than the dragDamperDistance from the follow target")]
        public float dragDamper = 3f;
        [Tooltip("The distance at which the drag damper starts to take effect")]
        public float dragDamperDistance = 0.025f;
        public float minVelocityChange = 1f;
        public float minVelocityDistanceMulti = 5f;

        [Header("Rotation Settings")]
        [Tooltip("Follow target rotation speed (Can cause jittering if turned too high - recommend increasing angular drag with speed)"), Min(0)]
        public float followRotationStrength = 100;
        public float startAngularDrag = 20;
        [Tooltip("The angular drag multiplier the hand will lerp between the (start angular drag), and the (start angular drag * this) to when less than the angleDragDamperDistance from the follow target")]
        public float angleDragDamper = 5f;
        [Tooltip("The distance at which the angular drag damper starts to take effect, in degrees")]
        public float angleDragDamperDistance = 3f;

        [Header("Mass Settings")]
        public float minMass = 0.25f;
        public float maxMass = 10f;
        public float heldMassDivider = 2f;
        public float distanceMassDifference = 10f;
        public float distanceMassMaxDistance = 0.5f;
        public float angleMassDifference = 10f;
        public float angleMassMaxAngle = 45f;


        public Vector3 lastAngularVelocity { get; protected set; }
        public Vector3 lastVelocity { get; protected set; }


        public Vector3 lastFollowDeltaPosition {
            get { return follow.position - lastFrameFollowPosition; }
        }
        public Quaternion lastFollowDeltaRotation {
            get { return follow.rotation * Quaternion.Inverse(lastFrameFollowRotation); }
        }


        internal Vector3 followVel;
        internal Vector3 followAngularVel;
        internal bool ignoreMoveFrame;

        internal Vector3 lastFrameFollowPosition;
        internal Quaternion lastFrameFollowRotation;
        internal Vector3 lastFollowPosition;
        internal Vector3 lastFollowRotation;

        internal Vector3[] updatePositionTracked = new Vector3[3];

        protected int tryMaxDistanceCount;

        protected float targetMass;
        protected float targetHeldMass;

        public Vector3 targetMoveToPosition { get; protected set; }
        public Quaternion targetMoveToRotation { get; protected set; }

        Vector3 lastSetMoveToFollowPosition;
        Quaternion lastSetMoveToFollowRotation;
        float lastSetMoveToTime;
        float lastSetVelocityTime;
        float lastSetAngularVelocityTime;
        float lastSetAverageMoveToTime;
        float lastSetMassTime;

        float currentFollowRotationStrength;
        float currentFollowPositionStrength;


        Transform _moveTo = null;
        public Transform moveTo {
            get {
                if(!gameObject.activeInHierarchy)
                    return null;

                if(_moveTo == null) {
                    _moveTo = new GameObject().transform;
                    _moveTo.parent = transform.parent;
                    _moveTo.name = "HAND FOLLOW POINT";
                }

                return _moveTo;
            }
        }

        List<Hand> currentHands {
            get{
                if(hand.holdingObj == null || hand.holdingObj.HeldCount() < 2)
                    return null;

                return hand.holdingObj.GetHeldBy(true, true);
            }
        }

        Rigidbody moveToBody;




        protected virtual void OnEnable() {
            currentFollowRotationStrength = followRotationStrength;
            currentFollowPositionStrength = followPositionStrength;
        }


        protected virtual void Awake() {
            hand.body.drag = startDrag;
            hand.body.angularDrag = startAngularDrag;
            hand.body.useGravity = false;
        }

        protected virtual void OnDestroy() {
            if(_moveTo != null)
                Destroy(_moveTo.gameObject);
        }

        protected virtual void Update() {
            UpdateHandOffset();
        }

        protected virtual void FixedUpdate() {
            UpdateHandPhysicsMovement();
        }


        protected virtual void UpdateHandPhysicsMovement() {

            if(follow == null || follow != hand.follow)
                follow = hand.follow;

            if(follow != null && hand.enableMovement) {

                followVel = follow.position - lastFollowPosition;
                followAngularVel = follow.rotation.eulerAngles - lastFollowRotation;
                lastFollowPosition = follow.position;
                lastFollowRotation = follow.rotation.eulerAngles;


                if(!hand.IsGrabbing() && !hand.body.isKinematic) {
                    SetMoveTo();
                    AverageSetMoveTo();
                    SetMass();
                    MoveTo(Time.fixedDeltaTime);
                    TorqueTo(Time.fixedDeltaTime);
                }

                if(ignoreMoveFrame) {
                    hand.body.velocity = Vector3.zero;
                    hand.body.angularVelocity = Vector3.zero;
                }
                ignoreMoveFrame = false;


                for(int i = 1; i < updatePositionTracked.Length; i++)
                    updatePositionTracked[i] = updatePositionTracked[i - 1];
                updatePositionTracked[0] = transform.localPosition;

                ignoreMoveFrame = false;
            }
        }



        float timeOffset;
        protected virtual void UpdateHandOffset() {
            if(follow == null || !hand.enableMovement)
                return;

            if(hand.enableMovement) {
                var deltaDist = Vector3.Distance(follow.position, lastFrameFollowPosition);
                var deltaRot = Quaternion.Angle(follow.rotation, lastFrameFollowRotation);

                if(hand.holdingObj && !hand.IsGrabbing() && !hand.holdingObj.maintainGrabOffset) {

                    //Returns the hand to the original position and rotation based on input movement
                    //A value of 1 gentle grab speed will return the hands position/rotation 1:1 with the controller movement
                    hand.grabPositionOffset = Vector3.MoveTowards(hand.grabPositionOffset, Vector3.zero, (deltaDist) * hand.gentleGrabSpeed * Time.deltaTime * 60f);
                    hand.grabRotationOffset = Quaternion.RotateTowards(hand.grabRotationOffset, Quaternion.identity, (deltaRot) * hand.gentleGrabSpeed * Time.deltaTime * 60f);
                    
                    if(!hand.holdingObj.useGentleGrab) {
                        UpdateOffset(true);
                    }
                }
                else if(!hand.holdingObj  && !hand.IsGrabbing()) {
                    UpdateOffset(false);
                }

                void UpdateOffset(bool isGrab) {

                    float grabTime = isGrab ? hand.lastGrabTime : hand.lastReleaseTime;
                    float grabReturnRotationDistance = Vector3.Angle(hand.grabRotationOffset.eulerAngles, Vector3.zero);
                    float grabReturnPositionDistance = hand.grabPositionOffset.magnitude;

                    var point = (Time.time - grabTime)/(hand.GetGrabTime()*2f);

                    timeOffset = ((timeOffset - 1f) + deltaDist * Time.deltaTime * hand.velocityGrabHandAmplifier)/2f + 1f;


                    var smoothTime = hand.GetGrabTime() * timeOffset;

                    hand.grabPositionOffset = Vector3.MoveTowards(hand.grabPositionOffset, Vector3.zero, grabReturnRotationDistance * smoothTime * Time.deltaTime);
                    hand.grabRotationOffset = Quaternion.RotateTowards(hand.grabRotationOffset, Quaternion.identity, grabReturnPositionDistance * smoothTime * Time.deltaTime);

                    hand.grabPositionOffset = Vector3.Lerp(hand.grabPositionOffset, Vector3.zero, point);
                    hand.grabRotationOffset = Quaternion.Lerp(hand.grabRotationOffset, Quaternion.identity, point);
                }
            }

            lastFrameFollowPosition = follow.position;
            lastFrameFollowRotation = follow.rotation;
        }








        internal virtual void MoveTo(float deltaTime) {

            if(followPositionStrength <= 0)
                return;

            if(Time.fixedTime - lastSetVelocityTime != 0)
                lastSetVelocityTime = Time.fixedTime;
            else
                return;

            if(currentHands != null) {
                foreach(var hand in currentHands) 
                    if(hand != null && hand != this.hand) 
                        hand.handFollow.MoveTo(deltaTime);
            }

            float minVelocityChange = this.minVelocityChange;
            var movePos = moveTo.position;
            var currentPos = hand.holdingObj != null && !hand.IsGrabbing() ? hand.handGrabPoint.position : hand.transform.position;
            var distance = Vector3.Distance(movePos, currentPos);

            var velocityClamp = hand.holdingObj != null ? hand.holdingObj.maxHeldVelocity : maxVelocity;
            Vector3 vel = (movePos - currentPos) * followPositionStrength;

            vel.x = Mathf.Clamp(vel.x, -velocityClamp, velocityClamp);
            vel.y = Mathf.Clamp(vel.y, -velocityClamp, velocityClamp);
            vel.z = Mathf.Clamp(vel.z, -velocityClamp, velocityClamp);

            float deltaOffset = Time.fixedDeltaTime / 0.011111f;
            float inverseDeltaOffset = 0.011111f / Time.fixedDeltaTime;
            Vector3 currentVelocity = hand.body.velocity;
            minVelocityChange *= deltaOffset;
            minVelocityChange *= 1 + (distance)*minVelocityDistanceMulti;

            if(currentHands == null)
                hand.body.drag = Mathf.Lerp((startDrag * dragDamper), startDrag, distance/dragDamperDistance) * inverseDeltaOffset;
            else 
                hand.body.drag = startDrag * inverseDeltaOffset;

            Vector3 towardsVel;
            if(currentHands != null) {
                towardsVel = new Vector3(
                    Mathf.MoveTowards(currentVelocity.x, vel.x, minVelocityChange),
                    Mathf.MoveTowards(currentVelocity.y, vel.y, minVelocityChange),
                    Mathf.MoveTowards(currentVelocity.z, vel.z, minVelocityChange)
                );
            }
            else {
                towardsVel = new Vector3(
                    Mathf.MoveTowards(currentVelocity.x, vel.x, minVelocityChange/5f + Mathf.Abs(currentVelocity.x)/1.5f),
                    Mathf.MoveTowards(currentVelocity.y, vel.y, minVelocityChange/5f + Mathf.Abs(currentVelocity.y)/1.5f),
                    Mathf.MoveTowards(currentVelocity.z, vel.z, minVelocityChange/5f + Mathf.Abs(currentVelocity.z)/1.5f)
                );
            }

            hand.body.velocity = towardsVel;
            lastVelocity = hand.body.velocity;
        }





        /// <summary>Rotates the hand to the controller rotation using physics movement</summary>
        internal virtual void TorqueTo(float deltaTime) {

            if(currentFollowRotationStrength <= 0)
                return;

            if(Time.fixedTime - lastSetAngularVelocityTime != 0)
                lastSetAngularVelocityTime = Time.fixedTime;
            else
                return;

            var delta = (moveTo.rotation * Quaternion.Inverse(hand.body.rotation));
            delta.ToAngleAxis(out float angle, out Vector3 axis);
            if(float.IsInfinity(axis.x))
                return;


            if(currentHands != null) {
                foreach(var hand in currentHands)
                    if(hand != null && hand != this.hand)
                        hand.handFollow.TorqueTo(deltaTime);
            }

            if(angle > 180f)
                angle -= 360f;

            float multiLinear = Mathf.Deg2Rad * angle * currentFollowRotationStrength;
            Vector3 angular = multiLinear * axis.normalized;
            angle = Mathf.Abs(angle);


            float inverseDeltaOffset = 0.011111f / Time.fixedDeltaTime;

            if(currentHands == null)
                hand.body.angularDrag = Mathf.Lerp((startAngularDrag * angleDragDamper), startAngularDrag, angle/angleDragDamperDistance) * inverseDeltaOffset;
            else
                hand.body.angularDrag = startAngularDrag * inverseDeltaOffset;

            hand.body.angularVelocity = angular;
            lastAngularVelocity = hand.body.angularVelocity;
        }



        /// <summary>Sets the mass of the hands based on the follow target parameters </summary>
        protected virtual void SetMass() {
            if(Time.fixedTime - lastSetMassTime < 1/1000f)
                return;
            lastSetMassTime = Time.fixedTime;

            if(currentHands != null) {
                foreach(var hand in currentHands)
                    if(hand != null && hand != this.hand)
                        hand.handFollow.SetMass();
            }

            //Converts the distance to a mass value
            float lerpPoint = 0;
            float angleLerpPoint = 0;

            var currentPos = hand.holdingObj != null && !hand.IsGrabbing() ? hand.handGrabPoint.position : hand.transform.position;
            var currentRot = hand.holdingObj != null && !hand.IsGrabbing() ? hand.handGrabPoint.rotation : hand.transform.rotation;
            lerpPoint = Vector3.Distance(moveTo.position, currentPos)/distanceMassMaxDistance;
            angleLerpPoint = Mathf.Abs(Quaternion.Angle(moveTo.rotation, currentRot)) / angleMassMaxAngle;

            float distanceMass = Mathf.Lerp(minMass, maxMass, lerpPoint) * distanceMassDifference / (distanceMassDifference + angleMassDifference);
            float angleMass = Mathf.Lerp(minMass, maxMass, angleLerpPoint) * angleMassDifference / (distanceMassDifference + angleMassDifference);
            targetMass = (angleMass + distanceMass);
            hand.body.mass = targetMass;

            if(hand.holdingObj != null && !hand.IsGrabbing()) {
                float startHeldMass = hand.holdingObj.targetMass/heldMassDivider;
                var heldDistanceMass = Mathf.Lerp(startHeldMass * (minMass/maxMass), startHeldMass, lerpPoint)* distanceMassDifference / (distanceMassDifference + angleMassDifference);
                float heldAngleMass = Mathf.Lerp(startHeldMass * (minMass/maxMass), startHeldMass, angleLerpPoint) * angleMassDifference / (distanceMassDifference + angleMassDifference);
                targetHeldMass = heldAngleMass + heldDistanceMass;
                hand.holdingObj.body.mass = targetHeldMass;
            }

            AverageMass();
        }



        /// <summary>Averages the masses if there are multiple hands to stabilize the held state</summary>
        protected virtual void AverageMass() {

            if(currentHands == null)
                return;

            float averageMass = 0;
            float averageHeldMass = 0;
            foreach(var hand in currentHands) {
                averageMass += hand.handFollow.targetMass;
                averageHeldMass += hand.handFollow.targetHeldMass;
            }

            averageMass /= currentHands.Count;
            foreach(var hand in currentHands)
                hand.body.mass = averageMass;

            hand.holdingObj.body.mass = averageHeldMass/currentHands.Count;
        }




        protected virtual void CheckHandMaxDistance() {
            var currentHandPos = hand.holdingObj != null && !hand.IsGrabbing() ? hand.handGrabPoint.position : hand.transform.position;
            var distance = Vector3.Distance(currentHandPos, targetMoveToPosition);

            //Returns if out of distance, if you aren't holding anything
            if(distance > maxFollowDistance) {
                if(hand.holdingObj != null) {
                    if(hand.holdingObj.parentOnGrab && tryMaxDistanceCount < 1) {
                        SetHandLocation(targetMoveToPosition, hand.transform.rotation);
                        tryMaxDistanceCount += 2;
                    }
                    //If the object is not parented and the hand cant teleport, release the it then teleport the hand
                    else if(!hand.holdingObj.parentOnGrab || tryMaxDistanceCount >= 1) {
                        hand.holdingObj.ForceHandRelease(hand);
                        SetHandLocation(targetMoveToPosition, hand.transform.rotation);
                    }
                }
                else {
                    SetHandLocation(targetMoveToPosition, hand.transform.rotation);
                }
            }

            if(tryMaxDistanceCount > 0)
                tryMaxDistanceCount--;
        }



        ///<summary>Moves the hand and whatever it might be holding (if teleport allowed) to given pos/rot</summary>
        public virtual void SetHandLocation(Vector3 targetPosition, Quaternion targetRotation) {
            var currentTransformPosition = hand.holdingObj != null && !hand.IsGrabbing() ? hand.handGrabPoint.position : hand.transform.position;
            var currentTransformRotation = hand.holdingObj != null && !hand.IsGrabbing() ? hand.handGrabPoint.rotation : hand.transform.rotation;
            var deltaPos = targetPosition - currentTransformPosition;
            var deltaRot = targetRotation * Quaternion.Inverse(currentTransformRotation);

            if(hand.holdingObj && hand.holdingObj.parentOnGrab) {
                if(!hand.IsGrabbing()) {
                    ignoreMoveFrame = true;

                    if(currentHands != null) {
                        targetPosition += currentTransformPosition - moveTo.position;
                        targetRotation *= (Quaternion.Inverse(moveTo.rotation)* currentTransformRotation);
                    }

                    var handRuler = AutoHandExtensions.transformRuler;
                    handRuler.position = currentTransformPosition;
                    handRuler.rotation = currentTransformRotation;

                    var grabRuler = AutoHandExtensions.transformRulerChild;
                    grabRuler.position = hand.holdingObj.body.transform.position;
                    grabRuler.rotation = hand.holdingObj.body.transform.rotation;

                    handRuler.position = targetPosition;
                    handRuler.rotation = targetRotation;

                    var deltaHandRot = targetRotation * Quaternion.Inverse(currentTransformRotation);
                    var deltaGrabPos = grabRuler.position - hand.holdingObj.body.transform.position;
                    var deltaGrabRot = Quaternion.Inverse(grabRuler.rotation) * hand.holdingObj.body.transform.rotation;

                    hand.transform.position = handRuler.position;
                    hand.transform.rotation = handRuler.rotation;
                    hand.body.position = handRuler.position;
                    hand.body.rotation = handRuler.rotation;

                    hand.holdingObj.body.transform.position = grabRuler.position;
                    hand.holdingObj.body.transform.rotation = grabRuler.rotation;
                    hand.holdingObj.body.position = grabRuler.position;
                    hand.holdingObj.body.rotation = grabRuler.rotation;

                    hand.body.velocity = deltaHandRot * hand.body.velocity;
                    hand.body.angularVelocity = deltaHandRot * hand.body.angularVelocity;
                    

                    hand.grabPositionOffset = deltaGrabRot * hand.grabPositionOffset;

                    foreach(var jointed in hand.holdingObj.jointedBodies) {
                        if(!(jointed.CanGetComponent(out Grabbable grab) && grab.HeldCount() > 0)) {
                            jointed.position += deltaGrabPos;
                            jointed.transform.RotateAround(hand.holdingObj.body.transform, deltaGrabRot);
                        }
                    }

                    hand.velocityTracker.ClearThrow();
                }
            }
            else {
                ignoreMoveFrame = true;
                hand.transform.position = targetPosition;
                hand.transform.rotation = targetRotation;
                hand.body.position = targetPosition;
                hand.body.rotation = targetRotation;
                hand.body.velocity = Vector3.zero;
                hand.body.angularVelocity = Vector3.zero;
            }

            moveTo.position += deltaPos;
            moveTo.rotation *= deltaRot;
            SetMoveTo();
            //AverageSetMoveTo();

        }



        ///<summary>Moves the hand and keeps the local rotation</summary>
        public virtual void SetHandLocation(Vector3 targetPosition) {
            SetMoveTo();
            SetHandLocation(targetPosition, hand.transform.rotation);
        }



        /// <summary>Resets the hand location to the follow</summary>
        public void ResetHandLocation() {
            SetHandLocation(moveTo.position, moveTo.rotation);
        }



        /// <summary>Updates the target used to calculate velocity / movements towards follow</summary>
        public virtual void SetMoveTo(bool ignoreRedundancyCheck = false) {
            if(follow == null)
                return;

            if(ignoreRedundancyCheck || Time.fixedTime - lastSetMoveToTime > 0 || lastSetMoveToFollowPosition != (follow.position + hand.grabPositionOffset) || lastSetMoveToFollowRotation != (follow.rotation * hand.grabRotationOffset)) {
                lastSetMoveToFollowPosition = follow.position + hand.grabPositionOffset;
                lastSetMoveToFollowRotation = follow.rotation * hand.grabRotationOffset;
                lastSetMoveToTime = Time.fixedTime;
                lastSetVelocityTime = 0;
                lastSetAngularVelocityTime = 0;
                lastSetAverageMoveToTime = 0;
                lastSetMassTime = 0;
            }
            else {
                return;
            }

            if(currentHands != null) {
                foreach(var hand in currentHands)
                    if(hand != null && !hand.Equals(this.hand))
                        hand.handFollow.SetMoveTo();
            }


            var targetMoveToPosition = follow.position + hand.grabPositionOffset;
            var targetMoveToRotation = follow.rotation * hand.grabRotationOffset;

            if(hand.holdingObj != null) {
                if(hand.left) {
                    var moveLeft = hand.holdingObj.heldPositionOffset; moveLeft.x *= -1;
                    var leftRot = -hand.holdingObj.heldRotationOffset; leftRot.x *= -1;
                    targetMoveToPosition += transform.rotation * moveLeft;
                    targetMoveToRotation *= Quaternion.Euler(leftRot);
                }
                else {
                    targetMoveToPosition += transform.rotation * hand.holdingObj.heldPositionOffset;
                    targetMoveToRotation *= Quaternion.Euler(hand.holdingObj.heldRotationOffset);
                }
            }

            //Instead of just setting the moveto directly like in AutoHand V3,
            //I've found this method move using a moveTowards with a square root distance creates snappy forces
            //that get stronger the further away the hand is from the target, but in such a way that doesn't create sudden instabilities
            if(hand.holdingObj != null && !hand.IsGrabbing()) {
                var distance = Vector3.Distance(targetMoveToPosition, hand.handGrabPoint.position);
                var angleDistance = Quaternion.Angle(targetMoveToRotation, hand.handGrabPoint.rotation);
                moveTo.position = Vector3.MoveTowards(hand.handGrabPoint.position, targetMoveToPosition, maxMoveToDistance + (Mathf.Sqrt(distance+1f))-1f);
                moveTo.rotation = Quaternion.RotateTowards(hand.handGrabPoint.rotation, targetMoveToRotation, maxMoveToAngle + Mathf.Sqrt(angleDistance+1f)-1f);
            }
            else {
                var distance = Vector3.Distance(targetMoveToPosition, hand.transform.position);
                var angleDistance = Quaternion.Angle(targetMoveToRotation, hand.transform.rotation);
                moveTo.position = Vector3.MoveTowards(hand.transform.position, targetMoveToPosition, maxMoveToDistance + (Mathf.Sqrt(distance+1f)-1f));
                moveTo.rotation = Quaternion.RotateTowards(hand.transform.rotation, targetMoveToRotation, maxMoveToAngle + Mathf.Sqrt(angleDistance+1f)-1f);
            }

            //If you were using the old method of tracking the moveTo values, you can use these values to get the same results as V3.3 and older
            this.targetMoveToPosition = targetMoveToPosition;
            this.targetMoveToRotation = targetMoveToRotation;

            CheckHandMaxDistance();
        }




        public virtual void AverageSetMoveTo(bool ignoreRedundencyCheck = false) {

            if(Time.fixedTime - lastSetAverageMoveToTime != 0)
                lastSetAverageMoveToTime = Time.fixedTime;
            else if(!ignoreRedundencyCheck)
                return;

            //This line should stay between the fixed time return and the currentHands null check
            currentFollowRotationStrength = followRotationStrength;

            if(currentHands == null)
                return;

            var distance = Vector3.Distance(targetMoveToPosition, hand.handGrabPoint.position);
            var angleDistance = Quaternion.Angle(targetMoveToRotation, hand.handGrabPoint.rotation);

            var totalDistance = 0f;
            float totalAngleDistance = 0;
            foreach(var hand in currentHands) {
                if(hand != null) {
                    totalDistance += Vector3.Distance(hand.handFollow.targetMoveToPosition, hand.handGrabPoint.position);
                    totalAngleDistance +=  Quaternion.Angle(hand.handFollow.targetMoveToRotation, hand.handGrabPoint.rotation);
                }
            }

            if(currentHands != null) {
                foreach(var hand in currentHands)
                    if(hand != null && !hand.Equals(this.hand))
                        hand.handFollow.AverageSetMoveTo();
            }

            List<Hand> heldBy = currentHands;
            for(int i = 0; i < heldBy.Count; i++) {
                if(!heldBy[i].Equals(hand)) {
                    var otherHandFollow = heldBy[i].handFollow;
                    var otherMoveRotation = otherHandFollow.targetMoveToRotation;
                    var otherAngleDistance = Quaternion.Angle(otherMoveRotation, heldBy[i].handGrabPoint.rotation);

                    var massOffset = Mathf.Clamp01(heldBy[i].body.mass / hand.body.mass);
                    var deltaRotation = Quaternion.Inverse(heldBy[i].handGrabPoint.rotation) * heldBy[i].handFollow.targetMoveToRotation;

                    var totalDistanceDiff = Mathf.Clamp01(1-(totalDistance/maxFollowDistance));
                    moveTo.rotation = Quaternion.Lerp(moveTo.rotation, moveTo.rotation * deltaRotation, otherAngleDistance/totalAngleDistance * massOffset * 0.5f * totalDistanceDiff);

                }
            }

            currentFollowRotationStrength = Mathf.Lerp(followRotationStrength, followRotationStrength/4f, Mathf.Sqrt(distance/maxMoveToDistance));
        }
    }
}