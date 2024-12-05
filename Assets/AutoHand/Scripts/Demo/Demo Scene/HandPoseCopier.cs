using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Autohand
{
    public class HandPoseCopier : MonoBehaviour
    {
        public HandPoseDataContainer handPose;
        public Hand hand;

        [ContextMenu("Copy Pose To Hand")]
        public void CopyPoseToHand() {
              if(handPose == null || hand == null)
                return;

              handPose.SetHandPose(hand);
        }

    }
}
