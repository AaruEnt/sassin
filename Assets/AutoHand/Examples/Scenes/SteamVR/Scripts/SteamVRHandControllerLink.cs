#if UNITY_STANDALONE_WIN || UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

namespace Autohand.Demo{
    public class SteamVRHandControllerLink : HandControllerLink {
        public SteamVR_Input_Sources handType;
        public SteamVR_Action_Single grabAxis;
        public SteamVR_Action_Boolean grabAction;
        public SteamVR_Action_Boolean squeezeAction;

        bool grabbing;
        bool squeezing;

        [HideInInspector]
        public float vibrationFrequency = 100f;

        private void Start() {
            if(hand.left)
                handLeft = this;
            else
                handRight = this;
        }


        public void Update() {
            
            if(squeezeAction != null && squeezeAction.GetState(handType) && !squeezing) {
                squeezing = true;
                hand.Squeeze();
            }
            else if(squeezeAction != null && !squeezeAction.GetState(handType) && squeezing) {
                squeezing = false;
                hand.Unsqueeze();
            }
            
            if(grabAction != null && grabAction.GetState(handType) && !grabbing) {
                grabbing = true;
                hand.Grab();
            }
            else if(grabAction != null && !grabAction.GetState(handType) && grabbing) {
                grabbing = false;
                hand.Release();
            }

            if(grabAxis != null)
                hand.SetGrip(grabAxis.GetAxis(handType));  
        }
        
        public bool ButtonPressed(SteamVR_Action_Boolean button) {
            return button.GetState(handType);
        }

        public float GetAxis(SteamVR_Action_Single axis1D) {
            return axis1D.GetAxis(handType);
        }

        public Vector2 GetAxis2D(SteamVR_Action_Vector2 axis2D) {
            return axis2D.GetAxis(handType);
        }

        public override void TryHapticImpulse(float duration, float amp, float freq) {
            try {
                SteamVR_Actions.default_Haptic[handType].Execute(0, duration, freq, amp);
            }
            catch { }
        }
    }
}
#endif
