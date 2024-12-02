using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Autohand {

    public struct FingerTouchEventArgs {
        public FingerEnum finger1;
        public FingerEnum finger2;
    }

    public delegate void FingerTouchStartEvent(OpenXRAutoHandTracking hand, HandFingerGestureTracker gestureTracker, FingerTouchEventArgs e);
    public delegate void FingerTouchStopEvent(OpenXRAutoHandTracking hand, HandFingerGestureTracker gestureTracker, FingerTouchEventArgs e);

    public class HandFingerGestureTracker : MonoBehaviour {
        public OpenXRAutoHandTracking handTracking;
        public float fingerTipScale = 1.5f;
        public float fingerTouchEventDelay = 0.1f;

        bool[][] fingerTouchState;
        bool[][] lastFingerTouchState;
        float[][] lastFingerTouchStateChangeTime;



        public event FingerTouchStartEvent OnFingerTouchStart;
        public event FingerTouchStopEvent OnFingerTouchStop;

        void OnEnable() {
            fingerTouchState = new bool[handTracking.hand.fingers.Length][];
            lastFingerTouchState = new bool[handTracking.hand.fingers.Length][];
            lastFingerTouchStateChangeTime = new float[handTracking.hand.fingers.Length][];

            foreach(var finger in handTracking.hand.fingers) {
                var fingerIndex = (int)finger.fingerType;
                fingerTouchState[fingerIndex] = new bool[handTracking.hand.fingers.Length];
                lastFingerTouchState[fingerIndex] = new bool[handTracking.hand.fingers.Length];
                lastFingerTouchStateChangeTime[fingerIndex] = new float[handTracking.hand.fingers.Length];
            }
        }

        private void FixedUpdate() {
            for (int i = 0; i < handTracking.hand.fingers.Length; i++) {
                for (int j = 0; j < handTracking.hand.fingers.Length; j++) {
                    var finger1 = handTracking.hand.fingers[i];
                    var finger2 = handTracking.hand.fingers[j];

                    var finger1Index = (int)finger1.fingerType;
                    var finger2Index = (int)finger2.fingerType;

                    var distance = Vector3.Distance(finger1.tip.position, finger2.tip.position);
                    var radius = finger1.tipRadius * fingerTipScale + finger2.tipRadius * fingerTipScale;

                    fingerTouchState[finger1Index][finger2Index] = distance < radius;

                    if(fingerTouchState[finger1Index][finger2Index] != lastFingerTouchState[finger1Index][finger2Index] ) {
                        if(Time.fixedTime - lastFingerTouchStateChangeTime[finger1Index][finger2Index] < fingerTouchEventDelay) {
                            continue;
                        }

                        if(fingerTouchState[finger1Index][finger2Index]) {
                            OnFingerTouchStart?.Invoke(handTracking, this, new FingerTouchEventArgs { finger1 = finger1.fingerType, finger2 = finger2.fingerType });
                        }
                        else {
                            OnFingerTouchStop?.Invoke(handTracking, this, new FingerTouchEventArgs { finger1 = finger1.fingerType, finger2 = finger2.fingerType });
                        }

                        lastFingerTouchState[finger1Index][finger2Index] = fingerTouchState[finger1Index][finger2Index];
                        lastFingerTouchStateChangeTime[finger1Index][finger2Index] = Time.fixedTime;
                    }
                }
            }
        }
    }
}