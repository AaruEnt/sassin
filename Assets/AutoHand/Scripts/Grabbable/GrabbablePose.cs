using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Autohand{


#if UNITY_EDITOR
    [CanEditMultipleObjects]
#endif
    [HelpURL("https://app.gitbook.com/s/5zKO0EvOjzUDeT2aiFk3/auto-hand/custom-poses"), DefaultExecutionOrder(1000)]
    public class GrabbablePose : HandPoseDataContainer{
        public bool poseEnabled = true;
        [Tooltip("Whether or not this pose can be used by both hands at once or only one hand at a time")]
        public bool singleHanded = false;


        [AutoSmallHeader("Advanced Settings")]
        public bool showAdvanced = true;
        public float positionWeight = 1;
        public float rotationWeight = 1;
        [Tooltip("These poses will only be enabled when this pose is active. Great for secondary poses like holding the front of a gun with your second hand, only while holding the trigger")]
        public GrabbablePose[] linkedPoses;


        protected HandPoseData poseDataNonAlloc;

        public Grabbable grabbable { get; internal set; }
        public List<Hand> posingHands { get; protected set; }



        protected virtual void Awake()  {
            posingHands = new List<Hand>();
            if (poseScriptable != null)
            {
                if (poseScriptable.leftSaved)
                    leftPoseSet = true;
                if (poseScriptable.rightSaved)
                    rightPoseSet = true;
            }

            for (int i = 0; i < linkedPoses.Length; i++)
                linkedPoses[i].poseEnabled = false;

            if(leftPoseSet)
                poseDataNonAlloc = new HandPoseData(ref leftPose);
            else if(rightPoseSet)
                poseDataNonAlloc = new HandPoseData(ref rightPose);
        }

        protected virtual void OnEnable() {
            grabbable.onRelease.AddListener(OnRelease);
        }

        protected virtual void OnDisable() {
            grabbable.onRelease.RemoveListener(OnRelease);
        }

        protected virtual void OnRelease(Hand hand, Grabbable grab) {
            if(posingHands.Contains(hand))
                posingHands.Remove(hand);
        }


        public bool CanSetPose(Hand hand, Grabbable grab) {
            if(singleHanded && posingHands.Count > 0 && !posingHands.Contains(hand) && !(grab.singleHandOnly && grab.allowHeldSwapping))
                return false;
            if(hand.poseIndex != poseIndex)
                return false;
            if(hand.left && !leftPoseSet)
                return false;
            if(!hand.left && !rightPoseSet)
                return false;

            return poseEnabled;
        }


        public virtual ref HandPoseData GetHandPoseData(Hand hand) {
            if(poseScriptable != null) {
                if(hand.left)
                    return ref poseScriptable.leftPose;
                else
                    return ref poseScriptable.rightPose;
            }
            
            if(hand.left)
                return ref leftPose;
            else                 
                return ref rightPose;
        }


        /// <summary>Sets the hand to this pose, make sure to check CanSetPose() flag for proper use</summary>
        /// <param name="isProjection">for pose projections, so they wont fill condition for single handed before grab</param>
        public virtual void SetHandPose(Hand hand, bool isProjection = false) {
            if(!isProjection) {
                if(!posingHands.Contains(hand))
                    posingHands.Add(hand);

                for(int i = 0; i < linkedPoses.Length; i++)
                    linkedPoses[i].poseEnabled = true;
            }

            GetHandPoseData(hand).SetPose(hand, transform);
        }


        public virtual void CancelHandPose(Hand hand) {
            if(posingHands.Contains(hand)) {
                posingHands.Remove(hand);
            }

            for(int i = 0; i < linkedPoses.Length; i++)
                linkedPoses[i].poseEnabled = false;
        }

    }
}
