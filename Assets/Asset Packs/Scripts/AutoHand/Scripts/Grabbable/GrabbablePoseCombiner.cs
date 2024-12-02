using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Autohand{
    public class GrabbablePoseCombiner : MonoBehaviour{
        public List<GrabbablePose> poses = new List<GrabbablePose>();


        public bool CanSetPose(Hand hand, Grabbable grab) {
            foreach(var pose in poses) {
                if(pose != null && pose.CanSetPose(hand, grab))
                    return true;
            }
            return false;
        }

        public void AddPose(GrabbablePose pose) {
            if(!poses.Contains(pose))
                poses.Add(pose);
        }

        private void OnDestroy()
        {
            for (int i = poses.Count - 1; i >= 0; i--)
            {
                Destroy(poses[i]);
            }
        }

        public GrabbablePose GetClosestPose(Hand hand, Grabbable grab) {
            List<GrabbablePose> possiblePoses = new List<GrabbablePose>();
            foreach(var handPose in this.poses)
                if(handPose != null && handPose.CanSetPose(hand, grab))
                    possiblePoses.Add(handPose);
            
            float closestValue = float.MaxValue;
            int closestIndex = 0;

            var handPosition = hand.transform.position;
            var handRotation = hand.transform.rotation;


            for (int i = 0; i < possiblePoses.Count; i++){

                var pose = possiblePoses[i].GetHandPoseData(hand);
                var poseGlobalPosition = possiblePoses[i].transform.TransformPoint(pose.handOffset);
                var poseGlobalRotation = possiblePoses[i].transform.rotation * pose.localQuaternionOffset;

                var distance = Vector3.Distance(poseGlobalPosition, handPosition);
                var angleDistance = Quaternion.Angle(poseGlobalRotation, handRotation) / 270f;

                var closenessValue = distance / possiblePoses[i].positionWeight + angleDistance / possiblePoses[i].rotationWeight;
                if(closenessValue < closestValue) {
                    closestIndex = i;
                    closestValue = closenessValue;
                }
            }

            hand.transform.position = handPosition;
            hand.transform.rotation = handRotation;
            Physics.SyncTransforms();

            return possiblePoses[closestIndex];
        }

        internal int PoseCount() {
            return poses.Count;
        }
    }
}
