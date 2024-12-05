using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Autohand {

    public static class FingerGizmo {


        public static void DrawFingerPoseExample(FingerPoseData fingerPose, Matrix4x4 grabbableToWorld, Matrix4x4 handToGrabbable, Color color) {
            Matrix4x4 handGlobalMatrix = grabbableToWorld * handToGrabbable;
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

            Gizmos.DrawWireSphere(knucklePosition, 0.004f);
            Gizmos.DrawWireSphere(middlePosition, 0.004f);
            Gizmos.DrawWireSphere(distalPosition, 0.004f);
            Gizmos.DrawWireSphere(tipPosition, 0.004f);
        }

    }
}