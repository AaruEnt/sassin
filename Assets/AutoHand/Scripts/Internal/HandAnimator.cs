using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Autohand {
    [RequireComponent(typeof(Hand)), DefaultExecutionOrder(10000)]
    public class HandAnimator : MonoBehaviour {
        Hand hand;

        public float defaultPoseTransitionTime = 0.3f;
        public AnimationCurve defaultPoseTransitionCurve = AnimationCurve.Linear(0, 0, 1, 1);
        public HandPoseArea currentPoseArea { get; protected set; }



        HandPoseData _handPoseDataNonAlloc;
        internal ref HandPoseData handPoseDataNonAlloc {
            get {
                if(!_handPoseDataNonAlloc.isSet)
                    _handPoseDataNonAlloc = new HandPoseData(hand);

                return ref _handPoseDataNonAlloc;
            }
        }


        HandPoseData _openHandPose;
        public ref HandPoseData openHandPose {
            get {
                if(!_openHandPose.isSet)
                    _openHandPose = new HandPoseData(hand);

                return ref _openHandPose;
            }
        }


        HandPoseData _closeHandPose;
        public ref HandPoseData closeHandPose {
            get {
                if(!_closeHandPose.isSet)
                    _closeHandPose = new HandPoseData(hand);

                return ref _closeHandPose;
            }
        }

        HandPoseData _targetGrabPose;
        public ref HandPoseData targetGrabPose {
            get {
                if(!_targetGrabPose.isSet)
                    _targetGrabPose = new HandPoseData(hand);

                return ref _targetGrabPose;
            }
        }

        HandPoseData _currentInputPose;
        public ref HandPoseData currentInputPose {
            get {
                if(!_currentInputPose.isSet)
                    _currentInputPose = new HandPoseData(hand);

                return ref _currentInputPose;
            }
        }


        float targetPoseStartTransitionTime = 0;
        float targetPoseStopTransitionTime = 0;
        float targetPoseTotalTransitionTime = 0;
        AnimationCurve targetTransitionAnimationCurve = null;
        bool poseActive = false;
        HandPoseData _currentTargetPose;
        public ref HandPoseData currentTargetPose {
            get {
                if(!_currentTargetPose.isSet)
                    _currentTargetPose = new HandPoseData(hand);

                return ref _currentTargetPose;
            }
        }




        HandPoseData _currentHandPose;
        public ref HandPoseData currentHandPose {
            get {
                if(!_currentHandPose.isSet)
                    _currentHandPose = new HandPoseData(hand);

                return ref _currentHandPose;
            }
        }

        HandPoseData _currentHandSmoothPose;
        public ref HandPoseData currentHandSmoothPose {
            get {
                if(!_currentHandSmoothPose.isSet)
                    _currentHandSmoothPose = new HandPoseData(hand);

                return ref _currentHandSmoothPose;
            }
        }



        float fingerSwayVel;


        public void Start() {
            for(int i = 0; i < hand.fingers.Length; i++) {
                Finger finger = hand.fingers[i];
                int fingerIndex = (int)finger.fingerType;
                openHandPose.fingerPoses[fingerIndex].CopyFromData(ref finger.poseData[(int)FingerPoseEnum.Open]);
                closeHandPose.fingerPoses[fingerIndex].CopyFromData(ref finger.poseData[(int)FingerPoseEnum.Closed]);
            }

            if(defaultPoseTransitionCurve == null || defaultPoseTransitionCurve.keys.Length == 0)
                defaultPoseTransitionCurve = AnimationCurve.Linear(0, 0, 1, 1);

            targetTransitionAnimationCurve = defaultPoseTransitionCurve;
        }

        protected virtual void OnEnable() {
            hand = GetComponent<Hand>();
            hand.collisionTracker.OnTriggerFirstEnter += OnTriggerFirstEnter;
            hand.collisionTracker.OnTriggerLastExit += OnTriggerLastExit;
        }

        protected virtual void OnDisable() {
            hand.collisionTracker.OnTriggerFirstEnter -= OnTriggerFirstEnter;
            hand.collisionTracker.OnTriggerLastExit -= OnTriggerLastExit;
        }


        protected virtual void LateUpdate() {
            if(hand.enableIK) {
                UpdateInputPoseState();
                UpdateTargetPoseState();
            }
        }

        /// <summary>Determines how the hand should look/move based on its flags</summary>
        protected virtual void UpdateTargetPoseState() {
            float currentPoseState;
            if(poseActive)
                currentPoseState = targetTransitionAnimationCurve.Evaluate(Mathf.Clamp01((Time.time - targetPoseStartTransitionTime) / targetPoseTotalTransitionTime));
            else
                currentPoseState = targetTransitionAnimationCurve.Evaluate(1f - Mathf.Clamp01((Time.time - targetPoseStopTransitionTime) / targetPoseTotalTransitionTime));


            if(currentPoseState < 1f)
                currentHandPose.LerpPose(ref currentInputPose, ref currentTargetPose, currentPoseState);
            else if(poseActive)
                currentHandPose.CopyFromData(ref currentTargetPose);
            else
                currentHandPose.CopyFromData(ref currentInputPose);


            currentHandSmoothPose.LerpPose(ref currentHandSmoothPose, ref currentHandPose, 0.33f);
            if(!hand.IsGrabbing())
                currentHandSmoothPose.SetFingerPose(hand);
        }



        /// <summary>Determines how the hand should look/move based on its flags</summary>
        protected virtual void UpdateInputPoseState() {
            var averageVel = Vector3.zero;
            for(int i = 1; i < hand.handFollow.updatePositionTracked.Length; i++)
                averageVel += hand.handFollow.updatePositionTracked[i] - hand.handFollow.updatePositionTracked[i - 1];
            averageVel /= hand.handFollow.updatePositionTracked.Length;

            if(transform.parent != null)
                averageVel = (Quaternion.Inverse(hand.palmTransform.rotation) * transform.parent.rotation) * averageVel;


            //Responsable for movement finger sway
            float vel = (averageVel * 60).z;

            if(hand.CollisionCount() > 0) vel = 0;
            fingerSwayVel = Mathf.MoveTowards(fingerSwayVel, vel, Time.deltaTime * (Mathf.Abs((fingerSwayVel - vel) * 30f)));

            float grip = hand.gripOffset + hand.swayStrength * fingerSwayVel;
            foreach(var finger in hand.fingers) {
                int fingerIndex = (int)finger.fingerType;
                currentInputPose.fingerPoses[fingerIndex].LerpData(ref openHandPose.fingerPoses[fingerIndex], ref closeHandPose.fingerPoses[fingerIndex], grip + finger.GetCurrentBend());
            }
        }
        protected virtual void OnTriggerFirstEnter(GameObject other) {
            CheckEnterPoseArea(other);
        }

        protected virtual void OnTriggerLastExit(GameObject other) {
            CheckExitPoseArea(other);
        }



        /// <summary>Takes a new pose and an amount of time and poses the hand</summary>
        public virtual void SetTargetPose(ref HandPoseData poseData, float transitionTime, AnimationCurve animationCurve) {
            targetTransitionAnimationCurve = animationCurve;
            targetPoseTotalTransitionTime = transitionTime;
            targetPoseStartTransitionTime = Time.time;
            this.poseActive = true;
            currentTargetPose.CopyFromData(ref poseData);
            if(transitionTime == 0)
                currentHandPose.CopyFromData(ref poseData);
        }



        /// <summary>Takes a new pose and an amount of time and poses the hand</summary>
        public void SetPose(ref HandPoseData pose, float transitionTime, AnimationCurve animationCurve) => SetTargetPose(ref pose, transitionTime, animationCurve);
        public void SetPose(ref HandPoseData pose, float transitionTime) => SetTargetPose(ref pose, transitionTime, defaultPoseTransitionCurve);
        public void SetPose(ref HandPoseData pose) => SetTargetPose(ref pose, defaultPoseTransitionTime, defaultPoseTransitionCurve);


        /// <summary>Returns the current hand pose, ignoring what is being held - (IF SAVING A HELD POSE USE GetHeldPose())</summary>
        public ref HandPoseData GetCurrentHandPose() => ref currentHandPose;


        /// <summary>Copies the current hand pose to the given handPose, ignoring what is being held - (IF SAVING A HELD POSE USE GetHeldPose())</summary>
        public void CopyHandData(ref HandPoseData handPose) {
            if(!handPose.isSet)
                handPose = new HandPoseData(ref currentHandPose);
            else
                handPose.CopyFromData(ref currentHandPose);
        }


        /// <summary>Ensures any pose being made is canceled</summary>
        public void CancelPose(float cancelPoseTransitionTime) {
            targetPoseTotalTransitionTime = cancelPoseTransitionTime;
            targetPoseStopTransitionTime = Time.time;
            poseActive = false;
        }

        public void CancelPose() => CancelPose(defaultPoseTransitionTime);









        /// <summary>Checks and manages if any of the hands colliders enter a pose area</summary>
        protected virtual void CheckEnterPoseArea(GameObject other) {
            if(hand.holdingObj || !hand.usingPoseAreas || !other.activeInHierarchy)
                return;

            if(other && other.CanGetComponent(out HandPoseArea tempPose)) {
                for(int i = 0; i < tempPose.poseAreas.Length; i++) {
                    if(tempPose.poseIndex == hand.poseIndex) {
                        if(tempPose.HasPose(hand.left) && (currentPoseArea == null || currentPoseArea != tempPose)){
                            if(currentPoseArea != null)
                                TryRemoveHandPoseArea(currentPoseArea);

                            currentPoseArea = tempPose;
                            currentPoseArea?.OnHandEnter?.Invoke(hand);
                            if(hand.holdingObj == null)
                                SetPose(ref currentPoseArea.GetHandPoseData(hand.left), currentPoseArea.transitionTime);
                        }

                        break;
                    }
                }
            }
        }


        /// <summary>Checks if manages any of the hands colliders exit a pose area</summary>
        protected virtual void CheckExitPoseArea(GameObject other) {
            if(!hand.usingPoseAreas || !other.gameObject.activeInHierarchy)
                return;

            if(other.CanGetComponent(out HandPoseArea poseArea))
                TryRemoveHandPoseArea(poseArea);
        }

        public void TryRemoveHandPoseArea(HandPoseArea poseArea) {
            if(this.currentPoseArea != null && this.currentPoseArea.gameObject.Equals(poseArea.gameObject)) {
                if(hand.holdingObj == null) {
                    CancelPose();
                    this.currentPoseArea?.OnHandExit?.Invoke(hand);
                    this.currentPoseArea = null;
                }
                else if(hand.holdingObj != null) {
                    this.currentPoseArea?.OnHandExit?.Invoke(hand);
                    this.currentPoseArea = null;
                }
            }
        }

        public void ClearPoseArea() {
            if(currentPoseArea != null)
                currentPoseArea.OnHandExit?.Invoke(hand);
            currentPoseArea = null;
        }



        /// <returns>Returns true if that hand is currently locked into a pose</returns>
        public bool IsPosing() {
            return currentPoseArea != null || (hand.holdingObj != null && hand.holdingObj.HasCustomPose()) || (Time.time - targetPoseStartTransitionTime < targetPoseTotalTransitionTime) || poseActive;
        }
        


    }
}