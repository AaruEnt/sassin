using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace Autohand {
    [DefaultExecutionOrder(10002)]
    public class OpenXRAutoHandTrackingGrabber : MonoBehaviour {
        [Tooltip("Reference to the hand tracker responsible for tracking hand movements and poses")]
        public OpenXRAutoHandTracking handTracker;
        [Header("Holding Settings")]
        public bool allowHeldFingerMovement = true;
        [Space]
        [Header("Touch Grab Settings")]
        [Tooltip("The delay in seconds before a grab is released after the grab condition is no longer met")]
        [Min(0)]
        public float releaseGrabDelay = 0.35f;
        [Tooltip("Multiplier for the radius of the finger tip detection spheres used in touch grabbing")]
        [Min(0)]
        public float fingerTipRadiusMultiplier = 2f;
        [Tooltip("Enables or disables grabbing objects by touching them with fingers")]
        public bool useFingerTouchGrabbing = true;
        [Tooltip("Enables or disables releasing objects by touching them with fingers")]
        public bool useFingerTouchReleasing = true;
        [Tooltip("Enables or disables maintaining a held pose while holding an object with finger touch grabbing")]
        public bool useTouchHoldingWithHeldPose = false;


        [Header("Pose Grab Settings")]
        [Tooltip("Enables or disables grabbing objects using predefined hand poses")]
        public bool usePoseGrabbing = true;
        [Tooltip("The minimum closeness required for the hand pose to initiate a grab - closeness is near 0 with open hand pose, near 1 with closed fist pose")]
        [Min(0)]
        public float minPoseGrabCloseness = 0.35f;
        [Tooltip("The maximum closeness allowed for the hand pose to initiate a grab - closeness is near 0 with open hand pose, near 1 with closed fist pose")]
        [Min(0)]
        public float maxPoseGrabCloseness = 0.9f;
        [Space]
        [Tooltip("The delta in the OpenCloseness value required to trigger any grab type. This happens when the hand is moving from open to closed pose, and helps prevent grabbing from triggering by just touching an object. Recommended value range of 0.03f-0.005f")]
        [Min(0)]
        public float minDeltaPoseActivation = 0.01f;

        [Tooltip("The required change in open to closed pose state to guarentee trigger a grab - this allows you to grab objects by quickly making a fist from an open state - this value will become harder to activated when a higher value")]
        [Min(0)]
        public float maxDeltaPoseActivation = 0.035f;


        [Header("Pose Release Settings")]
        [Tooltip("Enables or disables releasing objects using predefined hand poses")]
        public bool usePoseRelease = true; 
        [Tooltip("The minimum openness required for the hand pose to initiate a release")]
        [Min(0)]
        public float minPoseReleaseOpenness = 0f;
        [Tooltip("The maximum openness allowed for the hand pose to initiate a release")]
        [Min(0)]
        public float maxPoseReleaseOpenness = 0.5f;
        [Tooltip("The required change in openness to trigger a release - this allows you to release objects just by quickly making an open hand pose even if the other release condititions aren't met. Helps prevent hand from getting stuck holding something")]
        [Min(0)]
        public float requiredDeltaPoseReleaseOpenness = 0.07f;

        [Header("Pose Squeeze Settings")]
        [Tooltip("Enables or disables squeezing objects using predefined hand poses")]
        public bool usePoseSqueezing = true;
        [Tooltip("The delay in seconds before a squeeze is unsqueezed after the squeeze condition is no longer met")]
        [Min(0)]
        public float squeezeUnsqueezeDelay = 0.5f;
        [Tooltip("Multiplier for the sensitivity of the squeezing pose detection")]
        [Min(0)]
        public float squeezePoseSensitvityMultiplier = 1.5f;



        FingerPoseData[] startOpenPose = new FingerPoseData[5];
        FingerPoseData[] startClosedPose = new FingerPoseData[5];
        FingerPoseData[] currentFingerPoses = new FingerPoseData[5];
        FingerPoseData[] currentSmoothFingerPoses = new FingerPoseData[5];
        FingerPoseData[] grabPoseTarget = new FingerPoseData[5];


        float currentHandOpenCloseState = 0f;
        float[] fingerOpenDifferences = new float[5];
        float[] fingerCloseDifferences = new float[5];
        float[] fingerCurrentOpenClose = new float[5];
        float[] fingerCurrentOpenCloseLastFrame = new float[5];


        float currentHeldOpenState = 0f;
        float currentHeldCloseState = 0f;
        float[] fingerHeldDifferences = new float[5];
        float[] fingerCurrentHeldOpen = new float[5];
        float[] fingerCurrentHeldClose = new float[5];

        
        float currentHeldAnimationFromToState = 0f;
        float[] fingerHeldAnimationFromDifferences = new float[5];
        float[] fingerHeldAnimationToDifferences = new float[5];
        float[] fingerCurrentHeldAnimationFromTo = new float[5];

        
        float deltaCurrentHandOpenClosedState = 0f;


        Grabbable[] currentFingerTouch = new Grabbable[5];
        Collider[] overlapSphereResults = new Collider[128];
        int[] lastColliderLayer = new int[128];

        bool usingHeldPose;
        GrabbablePoseAnimaion heldPoseAnimation;

        float releaseTime;
        float grabTime;



        protected virtual void OnEnable() {
            for(int i = 0; i < currentFingerTouch.Length; i++) {
                currentFingerTouch[i] = null;
            }

            var hand = handTracker.hand;
            for(int i = 0; i < startOpenPose.Length; i++) {
                var finger = hand.fingers[i];
                var fingerIndex = (int)finger.fingerType;
                startOpenPose[fingerIndex] = new FingerPoseData(finger.poseData[(int)FingerPoseEnum.Open]);
                startClosedPose[fingerIndex] = new FingerPoseData(finger.poseData[(int)FingerPoseEnum.Closed]);
                currentFingerPoses[fingerIndex] = new FingerPoseData(finger.poseData[(int)FingerPoseEnum.Closed]);
                currentSmoothFingerPoses[fingerIndex] = new FingerPoseData(finger.poseData[(int)FingerPoseEnum.Closed]);
                grabPoseTarget[fingerIndex] = new FingerPoseData(finger.poseData[(int)FingerPoseEnum.Closed]);
            }

            hand.OnBeforeGrabbed += OnBeforeGrab;
            hand.OnGrabbed += OnGrab;
            hand.OnReleased += OnRelease;
            InputTracking.trackingAcquired += OnControllerTrackingAquired;
        }


        protected virtual void OnDisable() {
            var hand = handTracker.hand;
            hand.OnBeforeGrabbed -= OnBeforeGrab;
            hand.OnGrabbed -= OnGrab;
            hand.OnReleased -= OnRelease;
            InputTracking.trackingAcquired -= OnControllerTrackingAquired;
        }
        


        void OnControllerTrackingAquired(XRNodeState node) {
            var hand = handTracker.hand;
            if((node.nodeType == XRNode.LeftHand && hand.left) || (node.nodeType == XRNode.RightHand && !hand.left)) {
                ResetHandPoseData(hand);
            }
        }

        void ResetHandPoseData(Hand hand) {
            foreach(var finger in hand.fingers) {
                var fingerIndex = (int)finger.fingerType;
                finger.poseData[(int)FingerPoseEnum.Open].CopyFromData(ref startOpenPose[fingerIndex]);
                finger.poseData[(int)FingerPoseEnum.Closed].CopyFromData(ref startClosedPose[fingerIndex]);
            }
        }

        void OnBeforeGrab(Hand hand, Grabbable grab) {
            if(handTracker.controllerTrackingActive)
                return;

            //Physics.SyncTransforms();
            if(!handTracker.hand.usingHighlight || hand.highlighter.currentHighlightTarget == null || !hand.highlighter.currentHighlightTarget.Equals(grab)) {
                handTracker.hand.highlighter.UpdateHighlight(true, true);
                Debug.Log("Backup Highlight");
            }

            ResetHandPoseData(hand);
            for(int i = 0; i < hand.fingers.Length; i++) {
                var finger = hand.fingers[i];
                var fingerIndex = (int)finger.fingerType;
                currentSmoothFingerPoses[fingerIndex].CopyFromData(ref handTracker.currentHandTrackingPose[fingerIndex]);
            }
        }

        void OnGrab(Hand hand, Grabbable grab) {
            if(handTracker.controllerTrackingActive)
                return;

            if(!handTracker.hand.usingHighlight || hand.highlighter.currentHighlightTarget == null || !hand.highlighter.currentHighlightTarget.Equals(grab)) {
                handTracker.hand.highlighter.UpdateHighlight(true, true);
                Debug.Log("Backup Highlight");
            }

            if(grab.GetGrabPose(hand, out var grabPose)) {
                usingHeldPose = true;

                if(grabPose.CanGetComponent<GrabbablePoseAnimaion>(out var poseAnimation))
                    heldPoseAnimation = poseAnimation;
            }

            for(int i = 0; i < hand.fingers.Length; i++) {
                var finger = hand.fingers[i];
                var fingerIndex = (int)finger.fingerType;

                grabPoseTarget[fingerIndex].CopyFromData(ref hand.handAnimator.targetGrabPose.fingerPoses[fingerIndex]);
                currentSmoothFingerPoses[fingerIndex].CopyFromData(ref grabPoseTarget[fingerIndex]);
            }

            grabTime = Time.time;
        }


        void OnRelease(Hand hand, Grabbable grab) {
            if(handTracker.controllerTrackingActive)
                return;

            releaseTime = Time.time;
            usingHeldPose = false; 
            heldPoseAnimation = null;
        }







        public void FixedUpdate() {
            if(handTracker.controllerTrackingActive)
                return;

            CalculateHandPoseState();
            CheckSqueezeState();
            if(usingHeldPose)
                CalculateHeldPoseState();
        }


        public void LateUpdate() {
            if(handTracker.controllerTrackingActive)
                return;

            CheckSqueezeState();
            UpdateHandHeldPose();
            CheckCurrentFingerTouch();
            CheckForValidGrab();
            CheckForValidRelease();

            if(!handTracker.hand.IsGrabbing() && !handTracker.hand.holdingObj)
                handTracker.hand.handAnimator.currentHandPose.SavePose(handTracker.hand);
        }




        private void UpdateHandHeldPose() {

            var hand = handTracker.hand;
            if(hand.holdingObj != null && !hand.IsGrabbing()) {
                if(allowHeldFingerMovement) {
                    var grabbingLayer = LayerMask.NameToLayer(Hand.grabbingLayerName);
                    var grabbingMask = LayerMask.GetMask(Hand.grabbingLayerName);


                    for(int i = 0; i < hand.highlighter.highlightColliderNonAllocCount; i++) {
                        var collider = hand.highlighter.highlightCollidersNonAlloc[i];
                        if(collider == null)
                            continue;
                        lastColliderLayer[i] = collider.gameObject.layer;
                        collider.gameObject.layer = grabbingLayer;
                    }

                    for(int i = 0; i < hand.fingers.Length; i++) {
                        var finger = hand.fingers[i];
                        var fingerIndex = (int)finger.fingerType;

                        if(usingHeldPose) {
                            if(heldPoseAnimation != null) {
                                grabPoseTarget[fingerIndex].CopyFromData(ref heldPoseAnimation.currentAnimationPose.fingerPoses[fingerIndex]);
                            }

                            currentFingerPoses[fingerIndex].CopyFromData(ref grabPoseTarget[fingerIndex]);
                            if(fingerCurrentHeldOpen[fingerIndex] > fingerCurrentHeldClose[fingerIndex])
                                currentFingerPoses[fingerIndex].LerpDataTo(ref handTracker.currentHandTrackingPose[fingerIndex], (fingerCurrentHeldOpen[fingerIndex] - fingerCurrentHeldClose[fingerIndex])*2f);
                        }
                        else {
                            if(currentFingerTouch[fingerIndex] != null) {
                                currentFingerPoses[fingerIndex].CopyFromData(ref currentSmoothFingerPoses[fingerIndex]);

                                currentFingerPoses[fingerIndex].LerpDataTo(ref handTracker.currentHandTrackingPose[fingerIndex], (Mathf.Clamp01(1f-fingerCurrentOpenClose[fingerIndex])));
                                finger.BendFingerUntilNoHit(hand.fingerBendSteps, grabbingMask, ref currentFingerPoses[fingerIndex], ref startOpenPose[fingerIndex]);
                                currentFingerPoses[fingerIndex].SetPoseData(hand, finger);
                            }
                            else {
                                currentFingerPoses[fingerIndex].CopyFromData(ref handTracker.currentHandTrackingPose[fingerIndex]);
                                //currentFingerPoses[fingerIndex].SetPoseData(hand, finger);
                            }
                        }

                        var angleDiff = Mathf.Sqrt(currentSmoothFingerPoses[fingerIndex].GetPoseDifferenceByAngle(ref currentFingerPoses[fingerIndex]));
                        currentSmoothFingerPoses[fingerIndex].LerpDataTo(ref currentFingerPoses[fingerIndex], Time.deltaTime * angleDiff * 6f);
                        currentSmoothFingerPoses[fingerIndex].SetFingerPose(finger);
                    }

                    for(int i = 0; i < hand.highlighter.highlightColliderNonAllocCount; i++) {
                        if(hand.highlighter.highlightCollidersNonAlloc[i] != null)
                        hand.highlighter.highlightCollidersNonAlloc[i].gameObject.layer = lastColliderLayer[i];
                    }
                }
                else if(!hand.IsGrabbing()){

                    for(int i = 0; i < hand.fingers.Length; i++) {
                        var finger = hand.fingers[i];
                        var fingerIndex = (int)finger.fingerType;

                        if(heldPoseAnimation != null) {
                            grabPoseTarget[fingerIndex].CopyFromData(ref heldPoseAnimation.currentAnimationPose.fingerPoses[fingerIndex]);
                        }

                        currentFingerPoses[fingerIndex].CopyFromData(ref grabPoseTarget[fingerIndex]);
                        currentFingerPoses[fingerIndex].SetFingerPose(finger);
                    }
                }
            }
        }




        void CalculateHandPoseState() {
            var hand = handTracker.hand;

            var fingerCount = hand.fingers.Length;
            currentHandOpenCloseState = 0f;
            deltaCurrentHandOpenClosedState = 0f;

            for(int i = 0; i < fingerCount; i++) {
                var finger = hand.fingers[i];
                var fingerIndex = (int)finger.fingerType;
                fingerCurrentOpenCloseLastFrame[fingerIndex] = fingerCurrentOpenClose[fingerIndex];

                fingerOpenDifferences[fingerIndex] = handTracker.currentHandTrackingPose[fingerIndex].GetPoseDifferenceByAngle(ref startOpenPose[fingerIndex]);
                fingerCloseDifferences[fingerIndex] = handTracker.currentHandTrackingPose[fingerIndex].GetPoseDifferenceByAngle(ref startClosedPose[fingerIndex]);
                fingerCurrentOpenClose[fingerIndex] = CalculatePoseMatch(fingerOpenDifferences[fingerIndex], fingerCloseDifferences[fingerIndex]);

                if(!hand.IsGrabbing())
                    finger.poseData[(int)FingerPoseEnum.Open].CopyFromData(ref handTracker.currentHandTrackingPose[fingerIndex]);
            }

            for(int i = 0; i < fingerCurrentOpenClose.Length; i++) {
                currentHandOpenCloseState += fingerCurrentOpenClose[i];
                deltaCurrentHandOpenClosedState += fingerCurrentOpenClose[i] - fingerCurrentOpenCloseLastFrame[i];
            }
            currentHandOpenCloseState /= fingerCurrentOpenClose.Length;
            deltaCurrentHandOpenClosedState /= fingerCurrentOpenClose.Length;



            float CalculatePoseMatch(float openStateDistance, float closeStateDistance) {
                float openWeight = 1f / (openStateDistance + 1f);
                float closeWeight = 1f / (closeStateDistance + 1f);
                float normalizedValue = closeWeight / (openWeight + closeWeight);

                return normalizedValue;
            }
        }


        void CalculateHeldPoseState() {
            var hand = handTracker.hand;

            var fingerCount = hand.fingers.Length;
            currentHeldOpenState = 0f;
            currentHeldCloseState = 0f;

            for(int i = 0; i < fingerCount; i++) {
                var finger = hand.fingers[i];
                var fingerIndex = (int)finger.fingerType;
                fingerHeldDifferences[fingerIndex] = handTracker.currentHandTrackingPose[fingerIndex].GetPoseDifferenceByAngle(ref grabPoseTarget[fingerIndex]);
                fingerCurrentHeldOpen[fingerIndex] = CalculatePoseMatch(fingerHeldDifferences[fingerIndex], fingerOpenDifferences[fingerIndex]);
                fingerCurrentHeldClose[fingerIndex] = CalculatePoseMatch(fingerHeldDifferences[fingerIndex], fingerCloseDifferences[fingerIndex]);
            }

            for(int i = 0; i < fingerCurrentOpenClose.Length; i++) {
                currentHeldOpenState += fingerCurrentHeldOpen[i];
                currentHeldCloseState += fingerCurrentHeldClose[i];
            }
            currentHeldOpenState /= fingerCurrentHeldOpen.Length;
            currentHeldCloseState /= fingerCurrentHeldClose.Length;

            float CalculatePoseMatch(float openStateDistance, float closeStateDistance) {
                float openWeight = 1f / (openStateDistance + 1f);
                float closeWeight = 1f / (closeStateDistance + 1f);
                float normalizedValue = closeWeight / (openWeight + closeWeight);

                return normalizedValue;
            }
        }


        public void CheckSqueezeState() {
            if(usePoseSqueezing) {
                var hand = handTracker.hand;
                float squeezeState = 0f;
                if(usingHeldPose) {
                    if(heldPoseAnimation != null) {

                        var fingerCount = hand.fingers.Length;
                        currentHeldAnimationFromToState = 0f;

                        for(int i = 0; i < fingerCount; i++) {
                            var finger = hand.fingers[i];
                            var fingerIndex = (int)finger.fingerType;
                            fingerHeldAnimationFromDifferences[fingerIndex] = handTracker.currentHandTrackingPose[fingerIndex].GetPoseDifferenceByAngle(ref heldPoseAnimation.fromPose.GetHandPoseData(hand).fingerPoses[fingerIndex]);
                            fingerHeldAnimationToDifferences[fingerIndex] = handTracker.currentHandTrackingPose[fingerIndex].GetPoseDifferenceByAngle(ref heldPoseAnimation.toPose.GetHandPoseData(hand).fingerPoses[fingerIndex]);
                            fingerCurrentHeldAnimationFromTo[fingerIndex] = CalculatePoseMatch(fingerHeldAnimationFromDifferences[fingerIndex], fingerHeldAnimationToDifferences[fingerIndex]);
                        }

                        int addCount = 0;
                        for(int i = 0; i < fingerCurrentHeldAnimationFromTo.Length; i++) {
                            //If the value is 0.5f it means the fingers on each pose are in the same position
                            //so the squeeze state shouldn't be checking these fingers for the final squeeze amount
                            if(fingerCurrentHeldAnimationFromTo[i] != 0.5f) {
                                currentHeldAnimationFromToState += fingerCurrentHeldAnimationFromTo[i];
                                addCount++;
                            }

                        }
                        currentHeldAnimationFromToState /= addCount;

                        float CalculatePoseMatch(float openStateDistance, float closeStateDistance) {
                            float openWeight = 1 / (openStateDistance + 1);
                            float closeWeight = 1 / (closeStateDistance + 1);
                            float normalizedValue = closeWeight / (openWeight + closeWeight);

                            return normalizedValue;
                        }

                        squeezeState = currentHeldAnimationFromToState * squeezePoseSensitvityMultiplier * 1.25f;
                        hand.SetGrip(squeezeState, squeezeState);

                    }
                    else {
                        squeezeState = Mathf.Clamp01(currentHeldCloseState) * squeezePoseSensitvityMultiplier;
                        hand.SetGrip(squeezeState, squeezeState);
                    }
                }
                else {
                    squeezeState = currentHandOpenCloseState * squeezePoseSensitvityMultiplier;
                    hand.SetGrip(squeezeState, squeezeState);
                }

                if(squeezeState >= 1f && !hand.squeezing)
                    hand.Squeeze();
                else if(squeezeState < 1f && hand.squeezing)
                    hand.Unsqueeze();
            }
        }




        public void CheckCurrentFingerTouch() {
            if(!(useFingerTouchGrabbing && useFingerTouchReleasing))
                return;

            var hand = handTracker.hand;

            var grabbingLayer = LayerMask.NameToLayer(Hand.grabbingLayerName);
            var grabbingMask = LayerMask.GetMask(Hand.grabbingLayerName);
            for (int i = 0; i < hand.highlighter.highlightColliderNonAllocCount; i++) {
                if(hand.highlighter.highlightCollidersNonAlloc[i] == null)
                    continue;
                var collider = hand.highlighter.highlightCollidersNonAlloc[i].gameObject;
                lastColliderLayer[i] = collider.layer;
                collider.layer = grabbingLayer;
            }

            foreach (var finger in hand.fingers) {
                int fingerIndex = (int)finger.fingerType;
                var fingerTip = finger.tip;
                for (int i = 0; i < hand.highlighter.highlightColliderNonAllocCount; i++) {
                    var resultCount = Physics.OverlapSphereNonAlloc(fingerTip.position, finger.tipRadius*fingerTipRadiusMultiplier, overlapSphereResults, grabbingMask);
                    if (resultCount > 0) {
                        //Checks all the overlaps until it finds a grabbable object
                        int index = -1;
                        do {
                            index++;
                            if(index >= resultCount) break;
                        }
                        while(!AutoHandExtensions.HasGrabbable(overlapSphereResults[index], out currentFingerTouch[fingerIndex]) || (currentFingerTouch[fingerIndex] == null || !(currentFingerTouch[fingerIndex] is Grabbable)));
                    }
                    else {
                        currentFingerTouch[fingerIndex] = null;
                    }
                }
            }

            for (int i = 0; i < hand.highlighter.highlightColliderNonAllocCount; i++) {
                if(hand.highlighter.highlightCollidersNonAlloc[i] == null)
                    continue;
                hand.highlighter.highlightCollidersNonAlloc[i].gameObject.layer = lastColliderLayer[i];
            }
        }



        public void CheckForValidGrab() {
            var hand = handTracker.hand;
            if(hand.holdingObj != null || hand.IsGrabbing() || deltaCurrentHandOpenClosedState < minDeltaPoseActivation || Time.time - releaseTime < releaseGrabDelay)
                return;

            if(useFingerTouchGrabbing && GetValidFingerGrabState(out var touchingGrabbable)) {
                if(touchingGrabbable.GetGrabPose(hand, out _)) {
                    hand.Grab(GrabType.InstantGrab);
                }
                else if(touchingGrabbable.CanGrab(hand)) {
                    hand.CreateGrabConnection(touchingGrabbable, true);
                }
            }
            else if(usePoseRelease && currentHandOpenCloseState > minPoseGrabCloseness && currentHandOpenCloseState < maxPoseGrabCloseness && deltaCurrentHandOpenClosedState > maxDeltaPoseActivation) {
                hand.Grab();
            }
        }




        public void CheckForValidRelease() {
            var hand = handTracker.hand;
            if(hand.holdingObj == null || hand.IsGrabbing() || Time.time - grabTime < releaseGrabDelay)
                return;

            if(useFingerTouchReleasing && allowHeldFingerMovement) { 
                if(usingHeldPose && !GetValidFingerPoseHoldState()) {
                    hand.Release();
                }
                else if(!usingHeldPose && !GetValidFingerGrabState(out _)) {
                    hand.Release();
                }
            }
            
            if(usePoseGrabbing && (currentHandOpenCloseState > minPoseReleaseOpenness && currentHandOpenCloseState < maxPoseReleaseOpenness && deltaCurrentHandOpenClosedState < -requiredDeltaPoseReleaseOpenness)) {
                hand.Release();
            }
        }


        bool GetValidFingerGrabState(out Grabbable currentTouchingGrabbable) {
            //Making sure that all the fingers are touching the same object
            Grabbable firstFound = null;
            if(handTracker.hand.holdingObj != null)
                firstFound = handTracker.hand.holdingObj;
            else {
                for(int i = 0; i < currentFingerTouch.Length; i++) {
                    if(currentFingerTouch[i] == null)
                        continue;
                    if(firstFound == null)
                        firstFound = currentFingerTouch[i];
                }
            }

            bool thumbTouching = currentFingerTouch[(int)FingerEnum.thumb] != null;
            bool indexTouching = currentFingerTouch[(int)FingerEnum.index] != null && currentFingerTouch[(int)FingerEnum.index] == firstFound;
            bool middleTouching = currentFingerTouch[(int)FingerEnum.middle] != null && currentFingerTouch[(int)FingerEnum.middle] == firstFound;
            bool ringTouching = currentFingerTouch[(int)FingerEnum.ring] != null && currentFingerTouch[(int)FingerEnum.ring] == firstFound;
            bool pinkyTouching = currentFingerTouch[(int)FingerEnum.pinky] != null && currentFingerTouch[(int)FingerEnum.pinky] == firstFound;

            bool[] fingerTouchs = new bool[5];
            fingerTouchs[(int)FingerEnum.thumb] = thumbTouching;
            fingerTouchs[(int)FingerEnum.index] = indexTouching;
            fingerTouchs[(int)FingerEnum.middle] = middleTouching;
            fingerTouchs[(int)FingerEnum.ring] = ringTouching;
            fingerTouchs[(int)FingerEnum.pinky] = pinkyTouching;

            bool[] validGrabs = {
                thumbTouching && (indexTouching || middleTouching || ringTouching || pinkyTouching),
                (indexTouching || pinkyTouching) && middleTouching && ringTouching,
                (indexTouching && middleTouching),
                (middleTouching && ringTouching),
            };

            //This is an advanced requirement for grabbing,
            //It didn't feel right to grab objects with an almost fully open hand without using a thumb,
            //but it should be possible to hold an object without using the thumb when the fingers are more bent
            //This is a requirement for how much the fingers should be closed when grabbing for each of the above conditions
            float[][] validGrabClosedFingerRequirements = new float[validGrabs.Length][];
            validGrabClosedFingerRequirements[0] = new float[5];
            validGrabClosedFingerRequirements[0][(int)FingerEnum.thumb] = 0.1f;
            validGrabClosedFingerRequirements[0][(int)FingerEnum.index] = 0.1f;
            validGrabClosedFingerRequirements[0][(int)FingerEnum.middle] = 0.1f;
            validGrabClosedFingerRequirements[0][(int)FingerEnum.ring] = 0.1f;
            validGrabClosedFingerRequirements[0][(int)FingerEnum.pinky] = 0.1f;

            validGrabClosedFingerRequirements[1] = new float[5];
            validGrabClosedFingerRequirements[1][(int)FingerEnum.index] = 0.15f;
            validGrabClosedFingerRequirements[1][(int)FingerEnum.middle] = 0.15f;
            validGrabClosedFingerRequirements[1][(int)FingerEnum.ring] = 0.15f;
            validGrabClosedFingerRequirements[1][(int)FingerEnum.pinky] = 0.15f;

            validGrabClosedFingerRequirements[2] = new float[5];
            validGrabClosedFingerRequirements[2][(int)FingerEnum.index] = 0.3f;
            validGrabClosedFingerRequirements[2][(int)FingerEnum.middle] = 0.3f;

            validGrabClosedFingerRequirements[3] = new float[5];
            validGrabClosedFingerRequirements[3][(int)FingerEnum.middle] = 0.3f;
            validGrabClosedFingerRequirements[3][(int)FingerEnum.ring] = 0.3f;

            //As a part of checking the previous validGrabClosedFingerRequirements we need to know which finger bend values matter
            int[][] fingerIndecies = new int[][] {
                new int[] { (int)FingerEnum.thumb, (int)FingerEnum.index, (int)FingerEnum.middle, (int)FingerEnum.ring, (int)FingerEnum.pinky },
                new int[] { (int)FingerEnum.index, (int)FingerEnum.middle, (int)FingerEnum.ring, (int)FingerEnum.pinky },
                new int[] { (int)FingerEnum.index, (int)FingerEnum.middle },
                new int[] { (int)FingerEnum.middle, (int)FingerEnum.ring },
            };

            bool validGrabFound = false;
            int resultIndex = -1;
            for(int i = 0; i < validGrabs.Length; i++) {
                //Checking if the fingers are touching the same object
                if(validGrabs[i]) {
                    validGrabFound = true;
                    //Checking if the fingers are bent enough to hold the object
                    for(int j = 0; j < fingerIndecies[i].Length; j++) {
                        int fingerIndex = fingerIndecies[i][j];
                        if(fingerTouchs[fingerIndex] && fingerCurrentOpenClose[fingerIndex] < validGrabClosedFingerRequirements[i][fingerIndex]) {
                            validGrabFound = false;
                            break;
                        }
                    }

                    if(validGrabFound) {
                        resultIndex = i;
                        break;
                    }
                }
            }

            if(resultIndex != -1)
                currentTouchingGrabbable = firstFound;
            else
                currentTouchingGrabbable = null;

            var validFingerState = validGrabFound && (currentTouchingGrabbable != null);

            return validFingerState;
        }


        bool[] fingerTouchs = new bool[5];
        bool GetValidFingerPoseHoldState() {

            bool thumbTouching = fingerCurrentHeldOpen[(int)FingerEnum.thumb] < (fingerCurrentHeldClose[(int)FingerEnum.thumb] + 0.025f);
            bool indexTouching = fingerCurrentHeldOpen[(int)FingerEnum.index] < (fingerCurrentHeldClose[(int)FingerEnum.index] + 0.025f);
            bool middleTouching = fingerCurrentHeldOpen[(int)FingerEnum.middle] < (fingerCurrentHeldClose[(int)FingerEnum.middle] + 0.025f);
            bool ringTouching = fingerCurrentHeldOpen[(int)FingerEnum.ring] < (fingerCurrentHeldClose[(int)FingerEnum.ring] + 0.025f);
            bool pinkyTouching = fingerCurrentHeldOpen[(int)FingerEnum.pinky] < (fingerCurrentHeldClose[(int)FingerEnum.pinky] + 0.025f);

            fingerTouchs[(int)FingerEnum.thumb] = thumbTouching;
            fingerTouchs[(int)FingerEnum.index] = indexTouching;
            fingerTouchs[(int)FingerEnum.middle] = middleTouching;
            fingerTouchs[(int)FingerEnum.ring] = ringTouching;
            fingerTouchs[(int)FingerEnum.pinky] = pinkyTouching;

            bool[] validGrabs = {
                thumbTouching && (indexTouching || middleTouching || ringTouching || pinkyTouching),
                (indexTouching || pinkyTouching) && middleTouching && ringTouching,
                (indexTouching && middleTouching),
                (middleTouching && ringTouching)
            };

            bool validGrabFound = false;
            for(int i = 0; i < validGrabs.Length; i++)
                if(validGrabs[i]) {
                    validGrabFound = true;
                    break;
                }

            return validGrabFound;
        }

    }
}