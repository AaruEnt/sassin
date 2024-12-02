using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Autohand {
    public class HandGestureEventTextWriter : MonoBehaviour {
        public HandGestureEvent handGestureEvent;
        public TMPro.TextMeshPro currentPoseDistance;
        public TMPro.TextMeshPro currentPoseRequiredActivation;
        public TMPro.TextMeshPro currentPoseActivation;
        public string materialColorName = "_Color";
        public Renderer[] colorMaterials;
        public Gradient colorGradient;
        

        private void OnEnable() {
            handGestureEvent.OnGestureStartEvent.AddListener(OnGestureStart);
            handGestureEvent.OnGestureStopEvent.AddListener(OnGestureStop);
        }

        private void OnDisable() {
            handGestureEvent.OnGestureStartEvent.RemoveListener(OnGestureStart);
            handGestureEvent.OnGestureStopEvent.RemoveListener(OnGestureStop);
        }

        void OnGestureStart(Hand hand, HandPoseGestureData data) {
            currentPoseDistance.color = Color.green;
        }

        void OnGestureStop(Hand hand, HandPoseGestureData data) {
            currentPoseDistance.color = Color.red;
        }

        private void Update() {
            if(handGestureEvent != null) {
                var gesture = handGestureEvent.GetCurrentGestureData(handGestureEvent.trackingHands[0].hand);
                if(gesture == null)
                    return;

                float currentPoseDistanceValue = gesture.currentPoseDifference + gesture.validDirectionAngleDistance;

                if(currentPoseDistance != null)
                    currentPoseDistance.text = (currentPoseDistanceValue).ToString();
                if(currentPoseRequiredActivation != null)
                    currentPoseRequiredActivation.text = handGestureEvent.minimumPoseDistance.ToString();
                if(currentPoseActivation != null)
                    currentPoseActivation.text = handGestureEvent.GetCurrentGestureActivationAmount(handGestureEvent.trackingHands[0].hand).ToString();

                foreach(var rend in colorMaterials) {
                    if(rend != null && rend.material != null && colorGradient != null)
                        rend.material.SetColor(materialColorName, colorGradient.Evaluate(1f - (currentPoseDistanceValue - handGestureEvent.minimumPoseDistance)));
                }
            }
        }

    }
}