using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Autohand {
    public class GrabbablePoseGizmo : MonoBehaviour {
        public GrabbablePose grabbablePose;
        public bool showRightPose = true;
        public bool showLeftPose = false;

        private void OnDrawGizmos() {
            if(grabbablePose == null)
                return;

            if(grabbablePose.rightPoseSet && showRightPose) {
                foreach(var fingerPose in grabbablePose.rightPose.fingerPoses) {
                    if(fingerPose.isSet) {
                        DrawFingerPoseExample(fingerPose, grabbablePose.rightPose.GetHandToWorldMatrix(grabbablePose.transform), Color.cyan);
                    }
                }

                DrawHandShapeExample(grabbablePose.rightPose, grabbablePose.transform.localToWorldMatrix, Color.cyan);
            }

            if(grabbablePose.leftPoseSet && showLeftPose) {
                foreach(var fingerPose in grabbablePose.leftPose.fingerPoses) {
                    if(fingerPose.isSet) {
                        DrawFingerPoseExample(fingerPose, grabbablePose.leftPose.GetHandToWorldMatrix(grabbablePose.transform), Color.cyan);
                    }
                }
            }
        }

        private void DrawFingerPoseRecursiveExample(FingerPoseData fingerPose, Matrix4x4 handToWorld, Color color) {
            Matrix4x4 parentMatrix = handToWorld;
            Matrix4x4 prevMatrix = parentMatrix;
            
            for(int i = 0; i < fingerPose.poseRelativeMatrix.Length; i++) {
                Matrix4x4 jointMatrix = prevMatrix * fingerPose.poseRelativeMatrix[i];
                Vector3 position = AutoHandExtensions.ExtractPosition(ref jointMatrix);
                var newColor = color * (i + 1f)/3f;
                newColor.a = 1f;
                Gizmos.color = newColor;
                Gizmos.DrawWireSphere(position, 0.004f);
                if(i > 0)
                    Gizmos.DrawLine(AutoHandExtensions.ExtractPosition(ref prevMatrix), position);

                prevMatrix = jointMatrix; 
            }
        }

        public static void DrawHandGizmo(HandPoseData pose, Transform relativeTo, Color color) {
            var handGlobalMatrix = pose.GetHandToWorldMatrix(relativeTo);
            foreach(var fingerPose in pose.fingerPoses)
                if(fingerPose.isSet)
                    DrawFingerPoseExample(fingerPose, handGlobalMatrix, color);
            DrawHandShapeExample(pose, handGlobalMatrix, color);

        }

        public static void DrawHandGizmo(HandPoseData pose, Matrix4x4 relativeTo, Color color) {
            var handGlobalMatrix = relativeTo;
            foreach(var fingerPose in pose.fingerPoses)
                if(fingerPose.isSet)
                    DrawFingerPoseExample(fingerPose, handGlobalMatrix, color);
            DrawHandShapeExample(pose, handGlobalMatrix, color);

        }


        public static void DebugDrawFingerPose(FingerPoseData fingerPose, Matrix4x4 handToWorld, Color color) {

            Matrix4x4 handGlobalMatrix = handToWorld;
            var kuckleToHandMatrix = fingerPose.poseRelativeMatrix[(int)FingerJointEnum.knuckle];
            var middleToKnuckleMatrix = fingerPose.poseRelativeMatrix[(int)FingerJointEnum.middle];
            var distalToMiddleMatrix = fingerPose.poseRelativeMatrix[(int)FingerJointEnum.distal];
            var tipToDistalMatrix = fingerPose.poseRelativeMatrix[(int)FingerJointEnum.tip];

            var knuckleGlobalMatrix = handGlobalMatrix * kuckleToHandMatrix;
            var middleGlobalMatrix = knuckleGlobalMatrix * middleToKnuckleMatrix;
            var distalGlobalMatrix = middleGlobalMatrix * distalToMiddleMatrix;
            var tipGlobalMatrix = distalGlobalMatrix * tipToDistalMatrix;

            var handPosition = AutoHandExtensions.ExtractPosition(ref handGlobalMatrix);
            var knucklePosition = AutoHandExtensions.ExtractPosition(ref knuckleGlobalMatrix);
            var middlePosition = AutoHandExtensions.ExtractPosition(ref middleGlobalMatrix);
            var distalPosition = AutoHandExtensions.ExtractPosition(ref distalGlobalMatrix);
            var tipPosition = AutoHandExtensions.ExtractPosition(ref tipGlobalMatrix);

            Debug.DrawLine(handPosition, knucklePosition, color);
            Debug.DrawLine(knucklePosition, middlePosition, color);
            Debug.DrawLine(middlePosition, distalPosition, color);
            Debug.DrawLine(distalPosition, tipPosition, color);
        }

            public static void DrawFingerPoseExample(FingerPoseData fingerPose, Matrix4x4 handToWorld,  Color color) {
           // grabbableToWorld = Matrix4x4.TRS(grabbableToWorld.ExtractPosition(), grabbableToWorld.ExtractRotation(), Vector3.one);
           // handToGrabbable = Matrix4x4.TRS(handToGrabbable.ExtractPosition(), handToGrabbable.ExtractRotation(), Vector3.one);
           
            Matrix4x4 handGlobalMatrix = handToWorld;
            var kuckleToHandMatrix = fingerPose.poseRelativeMatrix[(int)FingerJointEnum.knuckle];
            var middleToKnuckleMatrix = fingerPose.poseRelativeMatrix[(int)FingerJointEnum.middle];
            var distalToMiddleMatrix = fingerPose.poseRelativeMatrix[(int)FingerJointEnum.distal];
            var tipToDistalMatrix = fingerPose.poseRelativeMatrix[(int)FingerJointEnum.tip];

            var knuckleGlobalMatrix = handGlobalMatrix * kuckleToHandMatrix;
            var middleGlobalMatrix = knuckleGlobalMatrix * middleToKnuckleMatrix;
            var distalGlobalMatrix = middleGlobalMatrix * distalToMiddleMatrix;
            var tipGlobalMatrix = distalGlobalMatrix * tipToDistalMatrix;

            var handPosition = AutoHandExtensions.ExtractPosition(ref handGlobalMatrix);
            var knucklePosition = AutoHandExtensions.ExtractPosition(ref knuckleGlobalMatrix);
            var middlePosition = AutoHandExtensions.ExtractPosition(ref middleGlobalMatrix);
            var distalPosition = AutoHandExtensions.ExtractPosition(ref distalGlobalMatrix);
            var tipPosition = AutoHandExtensions.ExtractPosition(ref tipGlobalMatrix);


            Gizmos.color = color;
            Gizmos.DrawLine(handPosition, knucklePosition);
            Gizmos.DrawLine(knucklePosition, middlePosition);
            Gizmos.DrawLine(middlePosition, distalPosition);
            Gizmos.DrawLine(distalPosition, tipPosition);
        }

        public static void DrawHandShapeExample(HandPoseData pose, Matrix4x4 handGlobalMatrix, Color color) {
            var thumbKnuckleGlobalMatrix = handGlobalMatrix * pose.fingerPoses[(int)FingerEnum.thumb].poseRelativeMatrix[(int)FingerJointEnum.knuckle];
            var indexKnuckleGlobalMatrix = handGlobalMatrix * pose.fingerPoses[(int)FingerEnum.index].poseRelativeMatrix[(int)FingerJointEnum.knuckle];
            var middleKnuckleGlobalMatrix = handGlobalMatrix * pose.fingerPoses[(int)FingerEnum.middle].poseRelativeMatrix[(int)FingerJointEnum.knuckle];
            var ringKnuckleGlobalMatrix = handGlobalMatrix * pose.fingerPoses[(int)FingerEnum.ring].poseRelativeMatrix[(int)FingerJointEnum.knuckle];
            var pinkyKnuckleGlobalMatrix = handGlobalMatrix * pose.fingerPoses[(int)FingerEnum.pinky].poseRelativeMatrix[(int)FingerJointEnum.knuckle];        

            //Line from thumb to index
            Gizmos.color = color;
            Gizmos.DrawLine(AutoHandExtensions.ExtractPosition(ref thumbKnuckleGlobalMatrix), AutoHandExtensions.ExtractPosition(ref indexKnuckleGlobalMatrix));
            Gizmos.DrawLine(AutoHandExtensions.ExtractPosition(ref indexKnuckleGlobalMatrix), AutoHandExtensions.ExtractPosition(ref middleKnuckleGlobalMatrix));
            Gizmos.DrawLine(AutoHandExtensions.ExtractPosition(ref middleKnuckleGlobalMatrix), AutoHandExtensions.ExtractPosition(ref ringKnuckleGlobalMatrix));
            Gizmos.DrawLine(AutoHandExtensions.ExtractPosition(ref ringKnuckleGlobalMatrix), AutoHandExtensions.ExtractPosition(ref pinkyKnuckleGlobalMatrix));
            Gizmos.DrawLine(AutoHandExtensions.ExtractPosition(ref pinkyKnuckleGlobalMatrix), AutoHandExtensions.ExtractPosition(ref thumbKnuckleGlobalMatrix));
        }
    }
}
