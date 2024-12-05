using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Autohand {
    public class HandFingerGestureEvent : MonoBehaviour {
        public HandFingerGestureTracker handFingerGestureTracker;
        public FingerEnum finger1;
        public FingerEnum[] finger2;

        public UnityEvent<HandFingerGestureTracker, FingerEnum, FingerEnum> OnFingerTouchStartEvent;
        public UnityEvent<HandFingerGestureTracker, FingerEnum, FingerEnum> OnFingerTouchStopEvent;

            void OnEnable() {
            handFingerGestureTracker.OnFingerTouchStart += OnFingerTouchStart;
            handFingerGestureTracker.OnFingerTouchStop += OnFingerTouchStop;
        }

        void OnDisable() {
            handFingerGestureTracker.OnFingerTouchStart -= OnFingerTouchStart;
            handFingerGestureTracker.OnFingerTouchStop -= OnFingerTouchStop;
        }

        void OnFingerTouchStart(OpenXRAutoHandTracking hand, HandFingerGestureTracker gestureTracker, FingerTouchEventArgs e) {
            if (e.finger1 == finger1 && finger2.Contains(e.finger2)) {
                OnFingerTouchStartEvent?.Invoke(gestureTracker, e.finger1, e.finger2);
            }
        }

        void OnFingerTouchStop(OpenXRAutoHandTracking hand, HandFingerGestureTracker gestureTracker, FingerTouchEventArgs e) {
            if (e.finger1 == finger1 && finger2.Contains(e.finger2)) {
                OnFingerTouchStopEvent?.Invoke(handFingerGestureTracker, e.finger1, e.finger2);
            }
        }
    }
}