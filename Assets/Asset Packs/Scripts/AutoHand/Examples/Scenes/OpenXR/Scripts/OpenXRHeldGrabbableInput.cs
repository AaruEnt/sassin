using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Autohand {
    //Coupled with the OpenXRGrabbableInput script
    public class OpenXRHeldGrabbableInput : MonoBehaviour {
        public Hand hand;
        public InputActionProperty startAction;
        public InputActionProperty stopAction;
        [Tooltip("Must match the input layer of the GrabbableInput layer value")]
        public int inputLayer = 0;

        [Space]
        [Tooltip("If false, the events will only trigger if the hand is holding a grabbable with a GrabbableInput script of the same input layer")]
        public bool alwaysTriggerEvents = false;
        public UnityHandGrabEvent startInput;
        public UnityHandGrabEvent stopInput;

        public bool inputActive { get; set; }


        public void OnEnable() {
            if(hand == null && !gameObject.CanGetComponent(out hand))
                Debug.LogError("AUTOHAND: Hand not found on OpenXRHeldGrabbableInput", this);

            if(startAction.action != null) startAction.action.Enable();
            if(stopAction.action != null) stopAction.action.Enable();
            if(startAction.action != null) startAction.action.performed += StartInput;
            if(stopAction.action != null) stopAction.action.performed += StopInput;
        }


        private void OnDisable() {
            if(startAction.action != null) startAction.action.performed -= StartInput;
            if(stopAction.action != null) stopAction.action.performed -= StopInput;
        }


        private void StartInput(InputAction.CallbackContext grab) {
            if(!inputActive) {
                if(alwaysTriggerEvents)
                    startInput.Invoke(hand, hand.holdingObj);
                
                if(hand.holdingObj != null) {
                    if(hand.holdingObj.CanGetComponent<OpenXRGrabbableInput>(out var grabbableInput) && grabbableInput.inputLayer == inputLayer) {
                        grabbableInput.PressInput(hand);
                        if(!alwaysTriggerEvents)
                            startInput.Invoke(hand, hand.holdingObj);
                    }
                }


                inputActive = true;
            }
        }

        private void StopInput(InputAction.CallbackContext grab) {
            if(inputActive) {
                if(alwaysTriggerEvents)
                    stopInput.Invoke(hand, hand.holdingObj);

                if(hand.holdingObj != null) {
                    if(hand.holdingObj.CanGetComponent<OpenXRGrabbableInput>(out var grabbableInput) && grabbableInput.inputLayer == inputLayer) {
                        grabbableInput.ReleaseInput(hand);
                        if(!alwaysTriggerEvents)
                            stopInput.Invoke(hand, hand.holdingObj);
                    }
                }
                inputActive = false;
            }
        }
    }
}