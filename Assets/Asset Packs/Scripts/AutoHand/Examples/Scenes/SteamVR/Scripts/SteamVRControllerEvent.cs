using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Photon.Pun;
#if !UNITY_ANDROID
using Valve.VR;
#endif

namespace Autohand.Demo
{
    public class SteamVRControllerEvent : MonoBehaviourPunCallbacks {
#if !UNITY_ANDROID
        public SteamVR_Input_Sources controller;
        public SteamVR_Action_Boolean button;
        public UnityEvent Pressed;
        public UnityEvent Released;
        bool pressed;


        void Update()
        {
            if (photonView && !photonView.IsMine)
            {
                return;
            }
            if (!pressed && button.GetState(controller))
            {
                pressed = true;
                Pressed?.Invoke();
            }
            if (pressed && !button.GetState(controller))
            {
                pressed = false;
                Released?.Invoke();
            }
        }
#endif
    }
}