using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Autohand {
    //Coupled with the OpenXRHeldGrabbableInput script
    public class OpenXRGrabbableInput : MonoBehaviour {
        Grabbable grabbable;
        public bool inputReleaseOnHandRelease = true;
        public int inputLayer = 0;
        public UnityHandGrabEvent inputPressed;
        public UnityHandGrabEvent inputReleased;

        public bool inputActive { get; set; }

        private void OnEnable() {
            grabbable = GetComponent<Grabbable>();
            grabbable.onRelease.AddListener(OnRelease);
        }

        private void OnDisable() {
            grabbable.onRelease.RemoveListener(OnRelease);
        }

        public void OnRelease(Hand hand, Grabbable grab) {
            if(inputReleaseOnHandRelease)
                ReleaseInput(hand);
        }

        public void PressInput(Hand hand) {
            if(!inputActive)
                inputPressed.Invoke(hand, grabbable);
            inputActive = true;
        }

        public void ReleaseInput(Hand hand) {
            if(inputActive)
                inputReleased.Invoke(hand, grabbable);
            inputActive = false;
        }
    }
}