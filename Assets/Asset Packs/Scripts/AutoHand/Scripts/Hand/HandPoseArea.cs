using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Autohand{
    [HelpURL("https://app.gitbook.com/s/5zKO0EvOjzUDeT2aiFk3/auto-hand/custom-poses#hand-pose-areas")]
    public class HandPoseArea : HandPoseDataContainer {
        public bool useDistancePose = false;
        public float transitionTime = 0.2f;

        [Header("Events")]
        public UnityHandEvent OnHandEnter = new UnityHandEvent();
        public UnityHandEvent OnHandExit = new UnityHandEvent();

        internal HandPoseArea[] poseAreas;
        List<Hand> posingHands = new List<Hand>();



        protected virtual void Start(){
            poseAreas = GetComponents<HandPoseArea>();
        }

        protected virtual void OnEnable() {
            OnHandEnter.AddListener(HandEnter);
            OnHandExit.AddListener(HandExit);
        }

        protected virtual void OnDisable() {
            for(int i = posingHands.Count - 1; i >= 0; i--) 
                posingHands[i].handAnimator.TryRemoveHandPoseArea(this);
            OnHandEnter.RemoveListener(HandEnter);
            OnHandExit.RemoveListener(HandExit);
        }

        protected virtual void HandEnter(Hand hand) {
            posingHands.Add(hand);
        }

        protected virtual void HandExit(Hand hand) {
            posingHands.Remove(hand);
        }
    }
}
