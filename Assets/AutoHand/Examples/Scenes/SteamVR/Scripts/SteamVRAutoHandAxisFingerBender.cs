using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_STANDALONE_WIN || UNITY_EDITOR
using Valve.VR;
namespace Autohand.Demo{
    public class SteamVRAutoHandAxisFingerBender : MonoBehaviour{
        public SteamVRHandControllerLink controller;
        public SteamVR_Action_Single axis;
        
        [HideInInspector]
        public float[] bendOffsets;
        float lastAxis;

        void LateUpdate(){
            var currAxis = controller.GetAxis(axis);
            for(int i = 0; i < controller.hand.fingers.Length; i++) {
                controller.hand.fingers[i].bendOffset += (currAxis-lastAxis)*bendOffsets[i];
            }

            lastAxis = currAxis;
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
