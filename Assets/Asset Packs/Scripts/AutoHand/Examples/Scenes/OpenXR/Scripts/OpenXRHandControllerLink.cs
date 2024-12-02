using System.Collections.Generic;
using System.Data;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using InputDevice = UnityEngine.XR.InputDevice;
//using UnityEngine.XR.OpenXR.Input;

namespace Autohand.Demo {
    [HelpURL("https://app.gitbook.com/s/5zKO0EvOjzUDeT2aiFk3/auto-hand/controller-input")]
    public class OpenXRHandControllerLink : HandControllerLink {
        public InputActionProperty grabAxis;
        public InputActionProperty squeezeAxis;
        public InputActionProperty grabAction;
        public InputActionProperty releaseAction;
        public InputActionProperty squeezeAction;
        public InputActionProperty stopSqueezeAction;
        public InputActionProperty hapticAction;

        XRNode role;
        InputDevice device;
        List<InputDevice> devices = new List<InputDevice>();

        private bool squeezing;
        private bool grabbing;
        private void Start() {
            if(hand.left)
                handLeft = this;
            else
                handRight = this;
        }
            

        public void OnEnable(){
            if (grabAction == squeezeAction){
                Debug.LogError("AUTOHAND: You are using the same button for grab and squeeze on HAND CONTROLLER LINK, this will create conflict or errors", this);
            }

            if(grabAxis.action != null) grabAxis.action.Enable();
            if(squeezeAxis.action != null) squeezeAxis.action.Enable();
            if(hapticAction.action != null) hapticAction.action.Enable();
            if(grabAction.action != null) grabAction.action.performed += Grab;
            if (grabAction.action != null) grabAction.action.Enable();
            if (grabAction.action != null) grabAction.action.performed += Grab;
            if (releaseAction.action != null) releaseAction.action.Enable();
            if (releaseAction.action != null) releaseAction.action.performed += Release;
            if (squeezeAction.action != null) squeezeAction.action.Enable();
            if (squeezeAction.action != null) squeezeAction.action.performed += Squeeze;
            if (stopSqueezeAction.action != null) stopSqueezeAction.action.Enable();
            if (stopSqueezeAction.action != null) stopSqueezeAction.action.performed += StopSqueeze;


            if(hand.left)
                role = XRNode.LeftHand;
            else
                role = XRNode.RightHand;

        }


        void OnDisable() {
            if(grabAction.action != null) grabAction.action.performed -= Grab;
            if(releaseAction.action != null) releaseAction.action.performed -= Release;
            if(squeezeAction.action != null) squeezeAction.action.performed -= Squeeze;
            if(stopSqueezeAction.action != null) stopSqueezeAction.action.performed -= StopSqueeze;

        }




        private void Update() {
            hand.SetGrip(grabAxis.action.ReadValue<float>(), squeezeAxis.action.ReadValue<float>());
        }

        private void Grab(InputAction.CallbackContext grab){
            if (!grabbing){
                hand.Grab();
                grabbing = true;
            }
        }
        
        private void Release(InputAction.CallbackContext grab){
            if (grabbing){
                hand.Release();
                grabbing = false;
            }
        }

        private void Squeeze(InputAction.CallbackContext grab){
            if (!squeezing){
                hand.Squeeze();
                squeezing = true;
            }
        }
        
        private void StopSqueeze(InputAction.CallbackContext grab){
            if (squeezing){
                hand.Unsqueeze();
                squeezing = false;
            }
        }

        public override void TryHapticImpulse(float duration, float amp, float freq = 10) {

            InputDevices.GetDevicesAtXRNode(role, devices);
            //OpenXRInput.SendHapticImpulse(hapticAction.action, amp, duration, hand.left ? UnityEngine.InputSystem.XR.XRController.leftHand : UnityEngine.InputSystem.XR.XRController.rightHand);
            foreach(var device in devices) {
                if(device.TryGetHapticCapabilities(out var capabilities) && capabilities.supportsImpulse) {
                    device.SendHapticImpulse(0u, amp, duration);
                }
            }

            base.TryHapticImpulse(duration, amp, freq);
        }

    }
}
