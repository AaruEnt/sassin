
#if UNITY_STANDALONE_WIN || UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

namespace Autohand.Demo{
    public class SteamVRAutoHandFingerBender : MonoBehaviour{
        public SteamVRHandControllerLink controller;
        public SteamVR_Action_Boolean button;
        
        [HideInInspector]
        public float[] bendOffsets;

        bool pressed;
        
        void Update(){
            if(!pressed && controller.ButtonPressed(button)) {
                pressed = true;
                for(int i = 0; i < controller.hand.fingers.Length; i++) {
                    controller.hand.fingers[i].bendOffset += bendOffsets[i];
                }
            }
            else if(pressed && !controller.ButtonPressed(button)) {
                pressed = false;
                for(int i = 0; i < controller.hand.fingers.Length; i++) {
                    controller.hand.fingers[i].bendOffset -= bendOffsets[i];
                }
            }
        }
        
        private void OnDrawGizmosSelected() {
            if(controller == null && GetComponent<SteamVRHandControllerLink>()){
                controller = GetComponent<SteamVRHandControllerLink>();
                bendOffsets = new float[controller.hand.fingers.Length];
            }
        }
    }
}
#endif
