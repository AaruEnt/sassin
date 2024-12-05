using System;
using System.Collections.Generic;
using UnityEngine;

namespace Autohand {

    [System.Serializable]
    public struct FingerPoseData {
        public Matrix4x4[] poseRelativeMatrix;

        //Local rotations are calculated once when the matrix array is created, this prevents wasted calculations from calling matrix.extractrotation multiple times
        //These arent the same transform.localRotation, they represent the difference in rotation between the joints, ignoring any joints between them. Useful for quickly calulating blended poses
        public Quaternion[] localRotations;

        public bool isLocalSet => localRotations != null && localRotations.Length > 0;

        public bool isSet => poseRelativeMatrix != null && poseRelativeMatrix.Length > 0;


        public FingerPoseData(Hand hand, Finger finger) {
            poseRelativeMatrix = new Matrix4x4[4];
            poseRelativeMatrix[(int)FingerJointEnum.knuckle] = hand.transform.worldToLocalMatrix * finger.knuckleJoint.localToWorldMatrix;
            poseRelativeMatrix[(int)FingerJointEnum.middle] =  finger.knuckleJoint.worldToLocalMatrix * finger.middleJoint.localToWorldMatrix;
            poseRelativeMatrix[(int)FingerJointEnum.distal] =  finger.middleJoint.worldToLocalMatrix * finger.distalJoint.localToWorldMatrix;
            poseRelativeMatrix[(int)FingerJointEnum.tip] =  finger.distalJoint.worldToLocalMatrix * finger.tip.localToWorldMatrix;
            localRotations = new Quaternion[4];
            CalculateAdditionalValues(hand.transform.lossyScale);

        }

        public FingerPoseData(Transform hand, Transform knuckleJoint, Transform middleJoint, Transform distalJoint, Transform tip) {
            poseRelativeMatrix = new Matrix4x4[4];
            poseRelativeMatrix[(int)FingerJointEnum.knuckle] = hand.worldToLocalMatrix * knuckleJoint.localToWorldMatrix;
            poseRelativeMatrix[(int)FingerJointEnum.middle] =  knuckleJoint.worldToLocalMatrix * middleJoint.localToWorldMatrix;
            poseRelativeMatrix[(int)FingerJointEnum.distal] =  middleJoint.worldToLocalMatrix * distalJoint.localToWorldMatrix;
            poseRelativeMatrix[(int)FingerJointEnum.tip] =  distalJoint.worldToLocalMatrix * tip.localToWorldMatrix;
            localRotations = new Quaternion[4];
            CalculateAdditionalValues(hand.lossyScale);
        }

        public FingerPoseData(ref FingerPoseData data) {
            poseRelativeMatrix = new Matrix4x4[data.poseRelativeMatrix.Length];
            data.poseRelativeMatrix.CopyTo(poseRelativeMatrix, 0);
            localRotations = new Quaternion[4];
            data.localRotations.CopyTo(localRotations, 0);
        }

        public FingerPoseData(FingerPoseData data) {
            poseRelativeMatrix = new Matrix4x4[data.poseRelativeMatrix.Length];
            data.poseRelativeMatrix.CopyTo(poseRelativeMatrix, 0);
            localRotations = new Quaternion[4];
            data.localRotations.CopyTo(localRotations, 0);
        }


        public void SetPoseData(ref FingerPoseData data, FingerJointEnum[] fingerJoints) {
            if(data.poseRelativeMatrix == null || data.poseRelativeMatrix.Length == 0)
                poseRelativeMatrix = new Matrix4x4[4];

            for(int i = 0; i < fingerJoints.Length; i++) {
                int fingerJointIndex = (int)fingerJoints[i];
                poseRelativeMatrix[fingerJointIndex] = data.poseRelativeMatrix[fingerJointIndex];
                localRotations[fingerJointIndex] = data.localRotations[fingerJointIndex];
            }
        }


        public void SetPoseData(Hand hand, Finger finger) {
            if(poseRelativeMatrix == null || poseRelativeMatrix.Length == 0)
                poseRelativeMatrix = new Matrix4x4[4];
            poseRelativeMatrix[(int)FingerJointEnum.knuckle] = hand.transform.worldToLocalMatrix * finger.knuckleJoint.localToWorldMatrix;
            poseRelativeMatrix[(int)FingerJointEnum.middle] =  finger.knuckleJoint.worldToLocalMatrix * finger.middleJoint.localToWorldMatrix;
            poseRelativeMatrix[(int)FingerJointEnum.distal] =  finger.middleJoint.worldToLocalMatrix * finger.distalJoint.localToWorldMatrix;
            poseRelativeMatrix[(int)FingerJointEnum.tip] =  finger.distalJoint.worldToLocalMatrix * finger.tip.localToWorldMatrix;
            CalculateAdditionalValues(hand.transform.lossyScale);
        }

        public void SetPoseData(Transform hand, Transform knuckleJoint, Transform middleJoint, Transform distalJoint, Transform tip) {
            poseRelativeMatrix[(int)FingerJointEnum.knuckle] = hand.worldToLocalMatrix * knuckleJoint.localToWorldMatrix;
            poseRelativeMatrix[(int)FingerJointEnum.middle] =  knuckleJoint.worldToLocalMatrix * middleJoint.localToWorldMatrix;
            poseRelativeMatrix[(int)FingerJointEnum.distal] =  middleJoint.worldToLocalMatrix * distalJoint.localToWorldMatrix;
            poseRelativeMatrix[(int)FingerJointEnum.tip] =  distalJoint.worldToLocalMatrix * tip.localToWorldMatrix;
            CalculateAdditionalValues(hand.transform.lossyScale);
        }

        public void CopyFromData(ref FingerPoseData fingerPoseData) {
            fingerPoseData.poseRelativeMatrix.CopyTo(poseRelativeMatrix, 0);
            fingerPoseData.localRotations.CopyTo(localRotations, 0);
        }

        public void CopyFromData(FingerPoseData fingerPoseData) {
            fingerPoseData.poseRelativeMatrix.CopyTo(poseRelativeMatrix, 0);
            fingerPoseData.localRotations.CopyTo(localRotations, 0);
        }

        /// <summary>Interpolates this pose data to the given pose data by the given point 0.0 - 1.0</summary>
        /// <param name="updateMatrixData">If you know that you aren't going to use the matrix data for this PoseData</param>
        public void LerpDataTo(ref FingerPoseData otherPose, float point, bool updateMatrixData = false) {
            var length = poseRelativeMatrix.Length;
            for(int i = 0; i < length; i++) {
                var interpolatedRotation = Quaternion.Lerp(localRotations[i], otherPose.localRotations[i], point);
                localRotations[i] = interpolatedRotation;

                if(updateMatrixData)
                    poseRelativeMatrix[i].SetTRS(AutoHandExtensions.ExtractPosition(ref poseRelativeMatrix[i]), interpolatedRotation, AutoHandExtensions.ExtractScale(ref poseRelativeMatrix[i]));
            }
        }
        
        /// <summary>Interpolates this pose data to the given pose data by the given point 0.0 - 1.0</summary>
         /// <param name="updateMatrixData">If you know that you aren't going to use the matrix data for this PoseData</param>
        public void LerpData(ref FingerPoseData fromPose, ref FingerPoseData toPose, float point, bool updateMatrixData = false) {
            var length = poseRelativeMatrix.Length;
            for(int i = 0; i < length; i++) {
                var fromRotation = fromPose.localRotations[i];
                var toRotation = toPose.localRotations[i];
                var interpolatedRotation = Quaternion.Lerp(fromRotation, toRotation, point);

                localRotations[i] = interpolatedRotation;

                if(updateMatrixData)
                    poseRelativeMatrix[i].SetTRS(AutoHandExtensions.ExtractPosition(ref poseRelativeMatrix[i]), interpolatedRotation, AutoHandExtensions.ExtractScale(ref poseRelativeMatrix[i]));
            }
        }

        /// <summary>Sets the finger to match this poses data</summary>
        public void SetFingerPose(Finger finger) {
            var handToWorldRotation = finger.hand.transform.rotation;
            var knuckleRotation = handToWorldRotation * localRotations[(int)FingerJointEnum.knuckle];
            var middleRotation = knuckleRotation * localRotations[(int)FingerJointEnum.middle];
            var distalRotation = middleRotation * localRotations[(int)FingerJointEnum.distal];

            finger.knuckleJoint.rotation = knuckleRotation;
            finger.middleJoint.rotation = middleRotation;
            finger.distalJoint.rotation = distalRotation;
        }

        /// <summary>Sets the finger to match this poses data, faster than the simpler SetFingerPose(Finger finger) method because it doesn't call the transform getters when blending through two poses</summary>
        public void SetFingerPose(Finger finger, Quaternion handRotation, Transform knuckleJoint, Transform middleJoint, Transform distalJoint) {
            var handToWorldRotation = handRotation;
            var knuckleRotation = handToWorldRotation * localRotations[(int)FingerJointEnum.knuckle];
            var middleRotation = knuckleRotation * localRotations[(int)FingerJointEnum.middle];
            var distalRotation = middleRotation * localRotations[(int)FingerJointEnum.distal];

            knuckleJoint.rotation = knuckleRotation;
            middleJoint.rotation = middleRotation;
            distalJoint.rotation = distalRotation;
        }



        /// <summary>Returns Rotation Difference</summary>
        public float GetPoseDifferenceByAngle(ref FingerPoseData otherPose) {
            float angleDifference = 0;
            var length = poseRelativeMatrix.Length;
            for(int i = 0; i < length; i++) {
                Quaternion rotation = localRotations[i];
                Quaternion otherRotation = otherPose.localRotations[i];
                angleDifference += Quaternion.Angle(rotation, otherRotation);
            }

            return angleDifference;
        }

        public void CalculateAdditionalValues(Vector3 handLossyScale) {
            if(localRotations == null || localRotations.Length != 4)
                localRotations = new Quaternion[4];

            Matrix4x4 handGlobalMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, handLossyScale);
            Matrix4x4 knuckleGlobalMatrix = handGlobalMatrix * poseRelativeMatrix[(int)FingerJointEnum.knuckle];
            Matrix4x4 middleGlobalMatrix = knuckleGlobalMatrix * poseRelativeMatrix[(int)FingerJointEnum.middle];
            Matrix4x4 distalGlobalMatrix = middleGlobalMatrix * poseRelativeMatrix[(int)FingerJointEnum.distal];
            Matrix4x4 tipGlobalMatrix = distalGlobalMatrix * poseRelativeMatrix[(int)FingerJointEnum.tip];

            Quaternion knuckleRotation = AutoHandExtensions.ExtractRotation(ref knuckleGlobalMatrix);
            Quaternion middleRotation = AutoHandExtensions.ExtractRotation(ref middleGlobalMatrix);
            Quaternion distalRotation = AutoHandExtensions.ExtractRotation(ref distalGlobalMatrix);
            Quaternion tipRotation = AutoHandExtensions.ExtractRotation(ref tipGlobalMatrix);

            localRotations[(int)FingerJointEnum.knuckle] = knuckleRotation;
            localRotations[(int)FingerJointEnum.middle] = Quaternion.Inverse(knuckleRotation) * middleRotation;
            localRotations[(int)FingerJointEnum.distal] = Quaternion.Inverse(middleRotation) * distalRotation;
            localRotations[(int)FingerJointEnum.tip] = Quaternion.Inverse(distalRotation) * tipRotation;
        }

    }


    [System.Serializable]
    public struct PoseIdentifier {
        public float[] fingerLengths;
        
        public PoseIdentifier(Hand hand) {
            fingerLengths = new float[5];
            for(int i = 0; i < 5; i++) {
                var finger = hand.fingers[i];
                if(finger.tip != null && finger.fingerType != FingerEnum.none)
                    fingerLengths[(int)finger.fingerType] = finger.tip.localPosition.sqrMagnitude;
            }
        }

        public static bool operator == (PoseIdentifier a, PoseIdentifier b) {
            for(int i = 0; i < 5; i++) {
                if(a.fingerLengths[i] != b.fingerLengths[i]) {
                    return false;
                }
            }
            return true;
        }

        public static bool operator != (PoseIdentifier a, PoseIdentifier b) {
            return !(a == b);
        }

        public override bool Equals(object obj) {
            if(obj is PoseIdentifier) {
                return this == (PoseIdentifier)obj;
            }
            return false;
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }
    }


    [System.Serializable]
    public struct HandPoseData {

        //DEPRECATED POSE DATA
        public Vector3 rotationOffset;
        public Vector3[] posePositions;
        public Quaternion[] poseRotations;

        //NEWER POSE DATA VALUES
        public Vector3 handOffset;
        public Quaternion localQuaternionOffset;
        public Vector3 globalHandScale;
        public FingerPoseData[] fingerPoses;
        public PoseIdentifier poseID;

        public bool isSet {
            get {
                return fingerPoses != null && fingerPoses.Length > 0;
            }
        }

        public bool isDataDeprecated {
            get {
                if(posePositions != null && posePositions.Length > 0) {
                    if(fingerPoses == null || fingerPoses.Length == 0) {
                        return true;
                    }

                    foreach(var fingerPose in fingerPoses) {
                        if(fingerPose.isSet && !fingerPose.isLocalSet) {
                            return true;
                        }
                    }
                }

                return false;
            }
        }

        /// <summary>Creates a new pose using the current hand relative to a given grabbable</summary>
        public HandPoseData(Hand hand, Grabbable grabbable) {
            //OLD POSE DATA
            posePositions = new Vector3[0];
            poseRotations = new Quaternion[0];
            rotationOffset = Vector3.zero;

            //NEW POSE DATA
            handOffset = new Vector3();
            localQuaternionOffset = Quaternion.identity;
            globalHandScale = hand.transform.lossyScale;
            fingerPoses = new FingerPoseData[5];

            poseID = new PoseIdentifier(hand);
            SavePose(hand, grabbable.transform);
        }

        /// <summary>Creates a new pose using the current hand relative to a given grabbable</summary>
        public HandPoseData(Hand hand, Transform point) {
            //OLD POSE DATA
            posePositions = new Vector3[0];
            poseRotations = new Quaternion[0];
            rotationOffset = Vector3.zero;

            //NEW POSE DATA
            handOffset = new Vector3();
            localQuaternionOffset = Quaternion.identity;
            globalHandScale = hand.transform.lossyScale;
            fingerPoses = new FingerPoseData[5];
            poseID = new PoseIdentifier(hand);

            SavePose(hand, point);
        }

        /// <summary>Creates a new pose using the current hand shape</summary>
        public HandPoseData(Hand hand) {
            //OLD POSE DATA
            posePositions = new Vector3[0];
            poseRotations = new Quaternion[0];
            rotationOffset = Vector3.zero;

            //NEW POSE DATA
            handOffset = new Vector3();
            localQuaternionOffset = Quaternion.identity;
            globalHandScale = hand.transform.lossyScale;
            fingerPoses = new FingerPoseData[5];
            poseID = new PoseIdentifier(hand);

            SavePose(hand, null);
        }

        /// <summary>Creates a new pose using the current hand shape</summary>
        public HandPoseData(ref HandPoseData data) {
            //OLD POSE DATA;
            posePositions = new Vector3[0];
            poseRotations = new Quaternion[0];
            rotationOffset = Vector3.zero;

            //NEW POSE DATA
            handOffset = data.handOffset;
            localQuaternionOffset = data.localQuaternionOffset;
            globalHandScale = data.globalHandScale;
            fingerPoses = new FingerPoseData[5];
            poseID = data.poseID;

            for(int i = 0; i < data.fingerPoses.Length; i++)
                fingerPoses[i] = new FingerPoseData(ref data.fingerPoses[i]);
        }

        /// <summary>Creates a new pose using the current hand shape</summary>
        public void CopyFromData(ref HandPoseData data) {
            //OLD POSE DATA;
            posePositions = new Vector3[0];
            poseRotations = new Quaternion[0];
            rotationOffset = Vector3.zero;

            //NEW POSE DATA
            handOffset = data.handOffset;
            localQuaternionOffset = data.localQuaternionOffset;
            globalHandScale = data.globalHandScale;
            if(fingerPoses == null || fingerPoses.Length == 0) {
                fingerPoses = new FingerPoseData[5];
                for(int i = 0; i < data.fingerPoses.Length; i++)
                    fingerPoses[i] = new FingerPoseData(ref data.fingerPoses[i]);
            }
            else {
                for(int i = 0; i < data.fingerPoses.Length; i++)
                    fingerPoses[i].CopyFromData(ref data.fingerPoses[i]);
            }

            poseID = data.poseID;

        }


        /// <summary> Saves the pose data to the match the current shape of the given hand, relative to is the grabbable transform the hand is posing to hold, or null if empty hand pose </summary>
        public void SavePose(Hand hand, Transform relativeTo = null) {
            foreach(var finger in hand.fingers) {
                if(finger.fingerType == FingerEnum.none)
                    Debug.LogError("AUTO HAND: Finger type is not set, finger type needs to be set on finger component", finger);

                int fingerIndex = (int)finger.fingerType;
                if(!fingerPoses[fingerIndex].isSet)
                    fingerPoses[fingerIndex] = new FingerPoseData(hand, finger);
                else
                    fingerPoses[fingerIndex].SetPoseData(hand, finger);
            }

            if(relativeTo != null) {
                handOffset = relativeTo.InverseTransformPoint(hand.transform.position);
                localQuaternionOffset = Quaternion.Inverse(relativeTo.rotation) * hand.transform.rotation;
                globalHandScale = hand.transform.lossyScale;
            }
            else { 
                handOffset = Vector3.zero;
                localQuaternionOffset = Quaternion.identity;
                globalHandScale = hand.transform.lossyScale;
            }
        }



        /// <summary>Sets the hand pose to match the given hand, relative to the given transform </summary>
        public void SetPose(Hand hand, Transform relativeTo = null) {
            SetPosition(hand, relativeTo);
            SetFingerPose(hand);
        }


        /// <summary>Sets the finger pose without changing the hands position</summary>
        public void SetFingerPose(Hand hand) {
            foreach(var finger in hand.fingers)
                fingerPoses[(int)finger.fingerType].SetFingerPose(finger);
        }


        /// <summary>Sets the position without setting the finger pose</summary>
        public void SetPosition(Hand hand, Transform relativeTo = null) {
            if(relativeTo != null && relativeTo != hand.transform) {
                Matrix4x4 relativeToWorldMatrix = GetHandToWorldMatrix(relativeTo);

                Vector3 newPosition = AutoHandExtensions.ExtractPosition(ref relativeToWorldMatrix);
                Quaternion newRotation = AutoHandExtensions.ExtractRotation(ref relativeToWorldMatrix);

                hand.transform.SetPositionAndRotation(newPosition, newRotation);
                if(hand.body != null) {
                    hand.body.position = newPosition;
                    hand.body.rotation = newRotation;
                }
            }
        }

        public Matrix4x4 GetHandToWorldMatrix(Transform relativeTo) {
            if(relativeTo == null)
                return Matrix4x4.TRS(handOffset, localQuaternionOffset, globalHandScale);

            var globalHandPosition = relativeTo.TransformPoint(handOffset);
            var globalHandRotation = relativeTo.rotation * localQuaternionOffset;
            return Matrix4x4.TRS(globalHandPosition, globalHandRotation, globalHandScale);
        }



        public void LerpPose(ref HandPoseData from, ref HandPoseData to, float point) {
            handOffset = Vector3.Lerp(from.handOffset, to.handOffset, point);
            globalHandScale = Vector3.Lerp(from.globalHandScale, to.globalHandScale, point);
            localQuaternionOffset = Quaternion.Lerp(from.localQuaternionOffset, to.localQuaternionOffset, point);

            for(int i = 0; i < 5; i++) {
                fingerPoses[i].CopyFromData(ref from.fingerPoses[i]);
                fingerPoses[i].LerpDataTo(ref to.fingerPoses[i], point);
            }
        }


        public static void LerpPose(ref HandPoseData lerpPose, ref HandPoseData from, ref HandPoseData to, float point) {
            lerpPose.handOffset = Vector3.Lerp(from.handOffset, to.handOffset, point);
            lerpPose.globalHandScale = Vector3.Lerp(from.globalHandScale, to.globalHandScale, point);
            lerpPose.localQuaternionOffset = Quaternion.Lerp(from.localQuaternionOffset, to.localQuaternionOffset, point);

            for(int i = 0; i < 5; i++) {
                lerpPose.fingerPoses[i].CopyFromData(ref from.fingerPoses[i]);
                lerpPose.fingerPoses[i].LerpDataTo(ref to.fingerPoses[i], point);
            }
        }

        public void GetPoseDifference(ref HandPoseData otherPose, out float[] fingerDistances) {
            fingerDistances = new float[5];
            for(int i = 0; i < 5; i++) {
                fingerDistances[i] = fingerPoses[i].GetPoseDifferenceByAngle(ref otherPose.fingerPoses[i]);
            }
        }

        public void GetPoseDifference(ref HandPoseData otherPose, out float indexDifference, out float middleDifference, out float ringDifference, out float pinkyDifference, out float thumbDifference) {
            GetPoseDifference(ref otherPose, out var fingerDistances);
            indexDifference = fingerDistances[(int)FingerEnum.index];
            middleDifference = fingerDistances[(int)FingerEnum.middle];
            ringDifference = fingerDistances[(int)FingerEnum.ring];
            pinkyDifference = fingerDistances[(int)FingerEnum.pinky];
            thumbDifference = fingerDistances[(int)FingerEnum.thumb];
        }

        public void UpdateDepricatedData(Hand hand, Transform relativeTo) {

            List<Transform> poseTransformsList = new List<Transform>();

            foreach(var finger in hand.fingers)
                AssignChildrenPose(finger.transform);

            void AssignChildrenPose(Transform obj) {
                poseTransformsList.Add(obj);

                for(int j = 0; j < obj.childCount; j++)
                    AssignChildrenPose(obj.GetChild(j));
            }

            if(poseRotations.Length == 0 || posePositions.Length == 0 || posePositions.Length != poseTransformsList.Count) {
                Debug.LogWarning("AUTO HAND: Pose data is not set, skipping updating this pose data", relativeTo);
                return;
            }

            fingerPoses = new FingerPoseData[5];


            var newFingerPose = new FingerPoseData[5];
            foreach(var finger in hand.fingers) {
                newFingerPose[(int)finger.fingerType].poseRelativeMatrix = new Matrix4x4[3];
            }

            for(int i = 0; i < poseTransformsList.Count; i++) {
                var transformJoint = poseTransformsList[i];

                foreach(var finger in hand.fingers) {
                    if(finger.isDataDepricated) {
                        Debug.LogError("AUTO HAND: Finger data is depricated - please try manually updating the finger references on your hand and try again", finger);
                        return;
                    }

                    var knuckleJointIndex = poseTransformsList.IndexOf(finger.knuckleJoint);
                    var middleJointIndex = poseTransformsList.IndexOf(finger.middleJoint);
                    var distalJointIndex = poseTransformsList.IndexOf(finger.distalJoint);

                    var knuckleJointPosition = posePositions[knuckleJointIndex];
                    var middleJointPosition = posePositions[middleJointIndex];
                    var distalJointPosition = posePositions[distalJointIndex];

                    var knuckleJointRotation = poseRotations[knuckleJointIndex];
                    var middleJointRotation = poseRotations[middleJointIndex];
                    var distalJointRotation = poseRotations[distalJointIndex];

                    var knuckleStartPosition = finger.knuckleJoint.localPosition;
                    var middleStartPosition = finger.middleJoint.localPosition;
                    var distalStartPosition = finger.distalJoint.localPosition;

                    var knuckleStartRotation = finger.knuckleJoint.localRotation;
                    var middleStartRotation = finger.middleJoint.localRotation;
                    var distalStartRotation = finger.distalJoint.localRotation;

                    finger.knuckleJoint.localPosition = knuckleJointPosition;
                    finger.middleJoint.localPosition = middleJointPosition;
                    finger.distalJoint.localPosition = distalJointPosition;

                    finger.knuckleJoint.localRotation = knuckleJointRotation;
                    finger.middleJoint.localRotation = middleJointRotation;
                    finger.distalJoint.localRotation = distalJointRotation;

                    newFingerPose[(int)finger.fingerType] = new FingerPoseData(hand, finger);

                    finger.knuckleJoint.localPosition = knuckleStartPosition;
                    finger.middleJoint.localPosition = middleStartPosition;
                    finger.distalJoint.localPosition = distalStartPosition;

                    finger.knuckleJoint.localRotation = knuckleStartRotation;
                    finger.middleJoint.localRotation = middleStartRotation;
                    finger.distalJoint.localRotation = distalStartRotation;
                }
            }

            globalHandScale = hand.transform.lossyScale;



            newFingerPose.CopyTo(fingerPoses, 0);
        }

        internal void SetPositionData(Transform handPoint, Transform relativeTo) {
            if(relativeTo != null) {
                handOffset = relativeTo.InverseTransformPoint(handPoint.position);
                localQuaternionOffset = Quaternion.Inverse(relativeTo.rotation) * handPoint.rotation;
                globalHandScale = handPoint.lossyScale;
            }
            else {
                handOffset = Vector3.zero;
                localQuaternionOffset = Quaternion.identity;
                globalHandScale = Vector3.one;
            }
        }
    }






    public static class HandPoseExtentions {
        public static HandPoseData GetApproximateHandPose(this HandPoseData approximatePose, Hand hand, HandPoseData matchPose, Matrix4x4 offset) {

            approximatePose.handOffset = hand.transform.position;
            approximatePose.localQuaternionOffset = hand.transform.rotation;
            matchPose.localQuaternionOffset = hand.transform.rotation;
            matchPose.handOffset = hand.transform.position;

            Matrix4x4 handToWorld = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, approximatePose.globalHandScale);
            Matrix4x4 otherHandToWorld = Matrix4x4.TRS(AutoHandExtensions.ExtractPosition(ref offset), AutoHandExtensions.ExtractRotation(ref offset), matchPose.globalHandScale);

            FingerPoseData highPose = new FingerPoseData(hand, hand.fingers[0]);
            FingerPoseData lowPose = new FingerPoseData(ref highPose);

            FingerPoseData highLerpPose = new FingerPoseData(ref highPose);
            FingerPoseData lowLerpPose = new FingerPoseData(ref highPose);
            FingerPoseData targetPose = new FingerPoseData(ref highPose);

            //Initial Open Hand Pose
            foreach(var finger in hand.fingers) {
                targetPose.CopyFromData(ref matchPose.fingerPoses[(int)finger.fingerType]);
                FingerPoseData[] fingerPoses = { finger.poseData[(int)FingerPoseEnum.Open], finger.poseData[(int)FingerPoseEnum.PinchOpen] };

                int closestPoseIndex1 = -1;
                int closestPoseIndex2 = -1;
                float closestBlendValue = float.MaxValue;
                float closestPoseValue = float.MaxValue;

                for(int i = 0; i < fingerPoses.Length; i++) { 
                    var pose1 = fingerPoses[i];
                    highPose.CopyFromData(ref pose1);
                    lowPose.CopyFromData(ref pose1);

                    for(int j = i; j < fingerPoses.Length; j++) {
                        if(i == j)
                            continue;

                        var pose2 = fingerPoses[j];
                        float low = 0f;
                        float high = 1f;

                        for(int k = 0; k < 8; k++) {
                            float mid = (low + high) / 2f;

                            highLerpPose.CopyFromData(ref pose2);
                            highLerpPose.LerpDataTo(ref highPose, high);
                            float highValue = GetFingerPoseDistanceDifferenceValue(targetPose, highLerpPose, otherHandToWorld, handToWorld);

                            lowLerpPose.CopyFromData(ref pose2);
                            lowLerpPose.LerpDataTo(ref lowPose, low);
                            float lowValue = GetFingerPoseDistanceDifferenceValue(targetPose, lowLerpPose, otherHandToWorld, handToWorld);

                            if(highValue < lowValue) {
                                low = mid;
                                if(highValue < closestPoseValue) {
                                    closestBlendValue = high;
                                    closestPoseValue = highValue;
                                    closestPoseIndex1 = i;
                                    closestPoseIndex2 = j;
                                }
                            }
                            else {
                                high = mid;
                                if(lowValue < closestPoseValue) {
                                    closestBlendValue = low;
                                    closestPoseValue = lowValue;
                                    closestPoseIndex1 = i;
                                    closestPoseIndex2 = j;
                                }
                            }
                        }
                    }
                }

                highPose.CopyFromData(ref fingerPoses[closestPoseIndex2]);
                lowPose.CopyFromData(ref fingerPoses[closestPoseIndex1]);
                highPose.LerpDataTo(ref lowPose, closestBlendValue);
                approximatePose.fingerPoses[(int)finger.fingerType].CopyFromData(ref highPose);
            }


            return approximatePose;



            float GetFingerPoseDistanceDifferenceValue(FingerPoseData currentPose, FingerPoseData target, Matrix4x4 currentLocalToWorld, Matrix4x4 targetOffset) {
                float value = 0;


                Matrix4x4 kuckleToHandMatrix = currentPose.poseRelativeMatrix[(int)FingerJointEnum.knuckle];
                Matrix4x4 middleToKnuckleMatrix = currentPose.poseRelativeMatrix[(int)FingerJointEnum.middle];

                Matrix4x4 knuckleGlobalMatrix = currentLocalToWorld * kuckleToHandMatrix;
                Matrix4x4 middleGlobalMatrix = knuckleGlobalMatrix * middleToKnuckleMatrix;

                Vector3 knuckleCurrentPosition = AutoHandExtensions.ExtractPosition(ref knuckleGlobalMatrix);
                Vector3 middleCurrentPosition = AutoHandExtensions.ExtractPosition(ref middleGlobalMatrix);

                kuckleToHandMatrix = target.poseRelativeMatrix[(int)FingerJointEnum.knuckle];
                middleToKnuckleMatrix = target.poseRelativeMatrix[(int)FingerJointEnum.middle];

                knuckleGlobalMatrix = targetOffset * kuckleToHandMatrix;
                middleGlobalMatrix = knuckleGlobalMatrix * middleToKnuckleMatrix;

                Vector3 knuckleTargetPosition = AutoHandExtensions.ExtractPosition(ref knuckleGlobalMatrix);
                Vector3 middleTargetPosition = AutoHandExtensions.ExtractPosition(ref middleGlobalMatrix);

                value += Vector3.Distance(middleCurrentPosition, middleTargetPosition);

                return value;
            }
        }

        public static Matrix4x4 ApproximateOffsetMatrix(this HandPoseData fromPose, HandPoseData poseData) {

            Vector3[] localKnucklePositions = new Vector3[5];
            localKnucklePositions[0] = AutoHandExtensions.ExtractPosition(ref fromPose.fingerPoses[(int)FingerEnum.thumb].poseRelativeMatrix[(int)FingerJointEnum.knuckle]);
            localKnucklePositions[1] = AutoHandExtensions.ExtractPosition(ref fromPose.fingerPoses[(int)FingerEnum.index].poseRelativeMatrix[(int)FingerJointEnum.knuckle]);
            localKnucklePositions[2] = AutoHandExtensions.ExtractPosition(ref fromPose.fingerPoses[(int)FingerEnum.middle].poseRelativeMatrix[(int)FingerJointEnum.knuckle]);
            localKnucklePositions[3] = AutoHandExtensions.ExtractPosition(ref fromPose.fingerPoses[(int)FingerEnum.ring].poseRelativeMatrix[(int)FingerJointEnum.knuckle]);
            localKnucklePositions[4] = AutoHandExtensions.ExtractPosition(ref fromPose.fingerPoses[(int)FingerEnum.pinky].poseRelativeMatrix[(int)FingerJointEnum.knuckle]);

            Vector3[] globalKnucklePositions = new Vector3[5];
            var handToWorld = fromPose.GetHandToWorldMatrix(null);
            globalKnucklePositions[0] = handToWorld*localKnucklePositions[0];
            globalKnucklePositions[1] = handToWorld*localKnucklePositions[1];
            globalKnucklePositions[2] = handToWorld*localKnucklePositions[2];
            globalKnucklePositions[3] = handToWorld*localKnucklePositions[3];
            globalKnucklePositions[4] = handToWorld*localKnucklePositions[4];

            Vector3[] otherLocalKnucklePositions = new Vector3[5];
            otherLocalKnucklePositions[0] = AutoHandExtensions.ExtractPosition(ref poseData.fingerPoses[(int)FingerEnum.thumb].poseRelativeMatrix[(int)FingerJointEnum.knuckle]);
            otherLocalKnucklePositions[1] = AutoHandExtensions.ExtractPosition(ref poseData.fingerPoses[(int)FingerEnum.index].poseRelativeMatrix[(int)FingerJointEnum.knuckle]);
            otherLocalKnucklePositions[2] = AutoHandExtensions.ExtractPosition(ref poseData.fingerPoses[(int)FingerEnum.middle].poseRelativeMatrix[(int)FingerJointEnum.knuckle]);
            otherLocalKnucklePositions[3] = AutoHandExtensions.ExtractPosition(ref poseData.fingerPoses[(int)FingerEnum.ring].poseRelativeMatrix[(int)FingerJointEnum.knuckle]);
            otherLocalKnucklePositions[4] = AutoHandExtensions.ExtractPosition(ref poseData.fingerPoses[(int)FingerEnum.pinky].poseRelativeMatrix[(int)FingerJointEnum.knuckle]);


            Vector3[] otherGlobalKnucklePositions = new Vector3[5];
            handToWorld = poseData.GetHandToWorldMatrix(null);
            otherGlobalKnucklePositions[0] = handToWorld*otherLocalKnucklePositions[0];
            otherGlobalKnucklePositions[1] = handToWorld*otherLocalKnucklePositions[1];
            otherGlobalKnucklePositions[2] = handToWorld*otherLocalKnucklePositions[2];
            otherGlobalKnucklePositions[3] = handToWorld*otherLocalKnucklePositions[3];
            otherGlobalKnucklePositions[4] = handToWorld*otherLocalKnucklePositions[4];

            Vector3[] array1 = new Vector3[4];
            Vector3[] array2 = new Vector3[4];
            array1[0] = globalKnucklePositions[1] - globalKnucklePositions[0];
            array1[1] = globalKnucklePositions[2] - globalKnucklePositions[1];
            array1[2] = globalKnucklePositions[3] - globalKnucklePositions[2];
            array1[3] = globalKnucklePositions[4] - globalKnucklePositions[3];
            array2[0] = otherGlobalKnucklePositions[1] - otherGlobalKnucklePositions[0];
            array2[1] = otherGlobalKnucklePositions[2] - otherGlobalKnucklePositions[1];
            array2[2] = otherGlobalKnucklePositions[3] - otherGlobalKnucklePositions[2];
            array2[3] = otherGlobalKnucklePositions[4] - otherGlobalKnucklePositions[3];

            Plane fromHandPlane = new Plane(globalKnucklePositions[0], globalKnucklePositions[1], globalKnucklePositions[2]);
            Plane otherHandPlane = new Plane(otherGlobalKnucklePositions[0], otherGlobalKnucklePositions[1], otherGlobalKnucklePositions[2]);

            Vector3 normal1 = fromHandPlane.normal;
            Vector3 normal2 = otherHandPlane.normal;

            var rotationOffset = Quaternion.FromToRotation(normal1, normal2);
            var localToWorld = Matrix4x4.TRS(Vector3.zero, rotationOffset, Vector3.one);

            //This will rotate the hand around the normal of the plane until the localToWorld matrix is as close as possible to the other hand
            //We will use binary search principle along that normal axis to find the best rotation with as few iterations as possible

            Quaternion[] quaternionOffset = new Quaternion[3];
            quaternionOffset[0] = Quaternion.Euler(90, 0, 0);
            quaternionOffset[1] = Quaternion.Euler(0, 90, 0);
            quaternionOffset[2] = Quaternion.Euler(0, 0, 90);

            for(int q = 0; q < quaternionOffset.Length; q++) {
                float closestDistance = CompareVector3ArrayCross(array1, array2, localToWorld);
                float closestAngle = -1;
                var rotation = quaternionOffset[q];
                var normal = rotation * normal1;
                var high = 359;
                var low = 0;

                for(int i = 0; i < 9; i++) {
                    var mid = (high + low) / 2;
                    var highRotation = Quaternion.AngleAxis(high, normal);
                    var lowRotation = Quaternion.AngleAxis(low, normal);

                    var lowDistance = CompareVector3ArrayCross(array1, array2, localToWorld * Matrix4x4.Rotate(lowRotation));
                    var highDistance = CompareVector3ArrayCross(array1, array2, localToWorld * Matrix4x4.Rotate(highRotation));

                    if(lowDistance < closestDistance) {
                        closestDistance = lowDistance;
                        closestAngle = low;
                        high = mid;
                    }

                    if(highDistance < closestDistance) {
                        closestDistance = highDistance;
                        closestAngle = high;
                        low = mid;
                    }
                }


                if(closestAngle != -1) {
                    var rotationAdjustment = Quaternion.AngleAxis(closestAngle, normal);
                    localToWorld = localToWorld * Matrix4x4.Rotate(rotationAdjustment);
                    normal1 = rotationAdjustment * normal1;
                }
            }


            Vector3[] directions = new Vector3[] { Vector3.right, Vector3.up, Vector3.forward, Vector3.right, Vector3.up, Vector3.forward };

            var directionMagnitude = 0f;
            for(int i = 0; i < globalKnucklePositions.Length; i++) {
                var newDirectionMagnitude = Vector3.Distance(globalKnucklePositions[i], otherGlobalKnucklePositions[i]);
                if(directionMagnitude < newDirectionMagnitude) {
                    directionMagnitude = newDirectionMagnitude;
                }
            }


            foreach(var direction in directions) {
                float minDistance = -1;
                float maxDistance = 1;
                float midDistance = 0f;
                float closestDistance = float.MaxValue;

                for(int i = 0; i < 10; i++){
                    midDistance = (minDistance + maxDistance) / 2f;
                    Vector3 minDirection = minDistance * direction;
                    Vector3 maxDirection = maxDistance * direction;

                    float minDistanceResult = CompareVector3ArrayDistance(globalKnucklePositions, otherGlobalKnucklePositions, localToWorld * Matrix4x4.Translate(minDirection));
                    float maxDistanceResult = CompareVector3ArrayDistance(globalKnucklePositions, otherGlobalKnucklePositions, localToWorld * Matrix4x4.Translate(maxDirection));

                    if(minDistanceResult < maxDistanceResult) {
                        if(minDistanceResult < closestDistance) {
                            closestDistance = minDistanceResult;
                        }
                        maxDistance = midDistance;
                    }
                    else {
                        if(maxDistanceResult < closestDistance) {
                            closestDistance = maxDistanceResult;
                        }
                        minDistance = midDistance;
                    }
                }

                localToWorld = localToWorld*Matrix4x4.Translate(midDistance * direction);
            }




            for(int q = 0; q < quaternionOffset.Length; q++) {
                float closestDistance = CompareVector3ArrayCross(array1, array2, localToWorld);
                float closestAngle = -1;
                var rotation = quaternionOffset[q];
                var normal = rotation * normal1;
                var high = 359;
                var low = 0;

                for(int i = 0; i < 9; i++) {
                    var mid = (high + low) / 2;
                    var highRotation = Quaternion.AngleAxis(high, normal);
                    var lowRotation = Quaternion.AngleAxis(low, normal);

                    var lowDistance = CompareVector3ArrayCross(array1, array2, localToWorld * Matrix4x4.Rotate(lowRotation));
                    var highDistance = CompareVector3ArrayCross(array1, array2, localToWorld * Matrix4x4.Rotate(highRotation));

                    if(lowDistance < closestDistance) {
                        closestDistance = lowDistance;
                        closestAngle = low;
                        high = mid;
                    }

                    if(highDistance < closestDistance) {
                        closestDistance = highDistance;
                        closestAngle = high;
                        low = mid;
                    }
                }


                if(closestAngle != -1) {
                    var rotationAdjustment = Quaternion.AngleAxis(closestAngle, normal);
                    localToWorld = localToWorld * Matrix4x4.Rotate(rotationAdjustment);
                    normal1 = rotationAdjustment * normal1;
                }
            }


            return localToWorld;
        }

        public static float CompareVector3ArrayCross(Vector3[] target, Vector3[] offset, Matrix4x4 localToWorld) {
            float value = 0;

            for(int i = 0; i < target.Length; i++) {
                var targetPosition = target[i];
                var offsetPosition = offset[i];

                var localPosition = localToWorld.MultiplyPoint3x4(offsetPosition);
                float dotProduct = Vector3.Dot(targetPosition, localPosition);
                value += (1 - dotProduct) / 2f;
            }

            return value;
        }

        public static float CompareVector3ArrayDistance(Vector3[] target, Vector3[] offset, Matrix4x4 localToWorld) {
            float value = 0;

            for(int i = 0; i < target.Length; i++) {
                var targetPosition = target[i];
                var offsetPosition = offset[i];

                var localPosition = localToWorld.MultiplyPoint3x4(offsetPosition);
                float distance = Vector3.Distance(targetPosition, localPosition);
                value += distance;
            }

            return value;
        }
    }


}
