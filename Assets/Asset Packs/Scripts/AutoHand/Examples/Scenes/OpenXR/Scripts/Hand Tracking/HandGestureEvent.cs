using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Events;

namespace Autohand {
    public class HandPoseGestureData {
        public float currentPoseDifference;
        public float[] fingerPoseDifference = new float[5];
        public bool currentPoseState;
        public bool currentActivatedPoseState;
        public bool validPoseDirection;
        public float validDirectionAngleDistance;
        public float gestureStartTime;
        public float gestureStopTime;

        public void UpdatePoseDifferenceValues(Hand hand, ref HandPoseData poseData, FingerMask[] fingerMasks) {
            if(hand == null || hand.handAnimator.currentHandPose.fingerPoses == null)
                return;

            currentPoseDifference = 0f;
            int fingerCount = 0;
            for(int i = 0; i < hand.fingers.Length; i++) {
                var finger = hand.fingers[i];
                var fingerIndex = (int)finger.fingerType;

                float fingerWeight = 1f;
                if(fingerMasks != null) {
                    for(int j = 0; j < fingerMasks.Length; j++) { 
                        if(fingerMasks[j].finger == finger.fingerType) {
                            fingerWeight = fingerMasks[j].weight;
                            break;
                        }
                    }
                }

                fingerPoseDifference[fingerIndex] = Mathf.Clamp(hand.handAnimator.currentHandPose.fingerPoses[fingerIndex].GetPoseDifferenceByAngle(ref poseData.fingerPoses[fingerIndex]) / 90f, 0, 3) * fingerWeight;
                currentPoseDifference += fingerPoseDifference[fingerIndex] ;

                if(fingerWeight > 0)
                    fingerCount++;
            }

            currentPoseDifference /= fingerCount;
        }

        internal void SetInvalidData() {
            if(currentPoseState)
                gestureStopTime = Time.time;
            currentPoseState = false;
            currentActivatedPoseState = false;
            validPoseDirection = false;
        }
    }



    public class HandGestureEvent : HandPoseDataContainer {
        [Header("Gesture Event Settings")]
        public List<OpenXRAutoHandTracking> trackingHands;
        public float minimumPoseDistance = 0.85f;
        public float requiredPoseHoldActivationTime = 0.1f;
        public float requiredPoseStopActivationTime = 0.2f;

        [Header("(Optional) Finger Mask Settings")]
        public FingerMask[] fingerMasks = new FingerMask[0];

        [Header("(Optional) Directional Requirements")]
        [Tooltip("The angle distance required to ACTIVATE the pose, relative to the given axis and relative transform settings")]
        public float requiredActivationAngle = 25f;
        [Tooltip("The angle distance required to MAINTAIN the pose, relative to the given axis and relative transform settings")]
        public float requiredMaintainActivationAngle = 80f;
        public Axis palmAxis = Axis.None;
        public Axis targetAxis = Axis.None;
        public Transform relativeTransform;

        [Header("Gesture Events")]
        public UnityEvent<Hand, HandPoseGestureData> OnGestureStartEvent;
        public UnityEvent<Hand, HandPoseGestureData> OnGestureStopEvent;


        Dictionary<Hand, HandPoseGestureData> currentHandGestureData = new Dictionary<Hand, HandPoseGestureData>();
        int lastHandCount = 0;



        protected virtual void Start() {
            if(requiredActivationAngle > requiredMaintainActivationAngle)
                requiredActivationAngle = requiredMaintainActivationAngle;
        }

        protected virtual void LateUpdate() {
            if (trackingHands == null || trackingHands.Count == 0)
                return;

            CheckForHandListChanges();
            UpdatePoseDifferenceValues();
            CheckGestureEventState();
        }

        public bool IsTrackingActive() {
            foreach(OpenXRAutoHandTracking hand in trackingHands) {
                if(hand.hand == null)
                    return false;
                if(hand.handTrackingActive)
                    return true;
            }

            return false;
        }

        public HandPoseGestureData GetCurrentGestureData(Hand hand) {
            if (currentHandGestureData.ContainsKey(hand))
                return currentHandGestureData[hand];
            return null;
        }

        public bool IsGestureActive(Hand hand) {
            if(currentHandGestureData.ContainsKey(hand))
                return currentHandGestureData[hand].currentPoseState;
            return false;
        }

        public float GetCurrentGestureActivationAmount(Hand hand) {
            if(currentHandGestureData.ContainsKey(hand)) {
                var handGestureData = currentHandGestureData[hand];
                if(!handGestureData.currentPoseState)
                    return Mathf.Clamp01((Time.time - handGestureData.gestureStartTime)/requiredPoseHoldActivationTime);
                else
                    return Mathf.Clamp01(1f-(Time.time - handGestureData.gestureStopTime)/requiredPoseStopActivationTime);
            }
            return 0;
        }

        protected void CheckForHandListChanges() {
            if (trackingHands.Count != lastHandCount) {
                lastHandCount = trackingHands.Count;
                currentHandGestureData.Clear();
                foreach(OpenXRAutoHandTracking hand in trackingHands) {
                    currentHandGestureData.Add(hand.hand, new HandPoseGestureData());
                }
            }
        }

        protected virtual void UpdatePoseDifferenceValues() {

            foreach (OpenXRAutoHandTracking handTracking in trackingHands) {
                var hand = handTracking.hand;
                if (hand == null || (hand.left && !leftPose.isSet) || (!hand.left && !rightPose.isSet)) 
                    continue;

                HandPoseGestureData gestureData = currentHandGestureData[hand];

                if(hand.left)
                    gestureData.UpdatePoseDifferenceValues(hand, ref leftPose, fingerMasks);
                else
                    gestureData.UpdatePoseDifferenceValues(hand, ref rightPose, fingerMasks);

                if(IsGestureActive(handTracking.hand))
                    gestureData.validPoseDirection = IsValidPoseDirection(hand, ref gestureData);
                else
                    gestureData.validPoseDirection = IsValidActivationPoseDirection(hand, ref gestureData);

            }

        }
        protected bool IsValidPoseState(Hand hand, ref HandPoseGestureData gestureData) {
            return gestureData.currentPoseDifference < minimumPoseDistance && gestureData.validPoseDirection && hand.holdingObj == null;
        }

        protected void CheckGestureEventState() {
            foreach (OpenXRAutoHandTracking handTracking in trackingHands) {
                var hand = handTracking.hand;
                var gestureData = currentHandGestureData[hand];
                if(IsValidPoseState(hand, ref gestureData)) 
                    OnGestureStart(hand, ref gestureData);
                else 
                    OnGestureStop(hand, ref gestureData);
            }
        }

        protected virtual void OnGestureStart(Hand hand, ref HandPoseGestureData gestureData) {
            if(!gestureData.currentPoseState) {
                gestureData.currentPoseState = true;
                gestureData.gestureStartTime = Time.time;
            }
            if((Time.time - gestureData.gestureStartTime < requiredPoseHoldActivationTime))
                return;

            if(!gestureData.currentActivatedPoseState) { 
                gestureData.currentActivatedPoseState = true;
                OnGestureStartEvent.Invoke(hand, gestureData);
            }
        }

        protected virtual void OnGestureStop(Hand hand, ref HandPoseGestureData gestureData) {
            if(gestureData.currentPoseState) {
                gestureData.currentPoseState = false;
                gestureData.gestureStopTime = Time.time;
            }

            if(Time.time - gestureData.gestureStopTime < requiredPoseStopActivationTime)
                return;

            if(gestureData.currentActivatedPoseState) {
                gestureData.currentActivatedPoseState = false;
                OnGestureStopEvent.Invoke(hand, gestureData);
            }
        }

        protected virtual bool IsValidActivationPoseDirection(Hand hand, ref HandPoseGestureData data) {
            if(!IsTrackingActive())
                return false;

            if(palmAxis == Axis.None || targetAxis == Axis.None) {
                data.validDirectionAngleDistance = 0;
                return true;
            }

            var angle = Vector3.Angle(GetPalmAxis(hand), GetTargetAxis());
            var angleDistance = angle - requiredActivationAngle;
            angleDistance = Mathf.Clamp(angleDistance, 0, angleDistance);
            data.validDirectionAngleDistance = angleDistance;
            return angleDistance == 0;
        }

        protected virtual bool IsValidPoseDirection(Hand hand, ref HandPoseGestureData data) {
            if(!IsTrackingActive())
                return false;

            if(palmAxis == Axis.None || targetAxis == Axis.None) {
                data.validDirectionAngleDistance = 0;
                return true;
            }

            var angle = Vector3.Angle(GetPalmAxis(hand), GetTargetAxis());
            var angleDistance = angle - requiredMaintainActivationAngle;
            angleDistance = Mathf.Clamp(angleDistance, 0, angleDistance);
            data.validDirectionAngleDistance = angleDistance;
            return angleDistance == 0;
        }



        Vector3 GetPalmAxis(Hand hand) {
            if(palmAxis == Axis.X)
                return hand.palmTransform.right;
            else if(palmAxis == Axis.Y)
                return hand.palmTransform.up;
            else if(palmAxis == Axis.Z)
                return hand.palmTransform.forward;
            return Vector3.zero;
        }

        Vector3 GetTargetAxis() {
            if(relativeTransform == null) {
                if(targetAxis == Axis.X)
                    return Vector3.right;
                else if(targetAxis == Axis.Y)
                    return Vector3.up;
                else if(targetAxis == Axis.Z)
                    return Vector3.forward;
            }
            
            if(targetAxis == Axis.X)
                return relativeTransform.right;
            else if(targetAxis == Axis.Y)
                return relativeTransform.up;
            else if(targetAxis == Axis.Z)
                return relativeTransform.forward;

            return Vector3.zero;
        }
    }
}
