using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if !UNITY_ANDROID
using Valve.VR;
#endif

namespace Autohand.Demo
{
    public class SteamVRHandPlayerLink : MonoBehaviour{
#if !UNITY_ANDROID
        public AutoHandPlayer player;
        public SteamVR_Input_Sources moveController;
        public SteamVR_Action_Vector2 moveAxis;

        public SteamVR_Input_Sources TurnController;
        public SteamVR_Action_Boolean turnRightButton;
        public SteamVR_Action_Boolean turnLeftButton;
        bool rightPressed;
        bool leftPressed;

        void Update(){
            player.Move(moveAxis.GetAxis(moveController));
            if(!moveAxis.active)
                player.Move(Vector2.zero);

            if (!rightPressed && turnRightButton.GetState(TurnController)) {
                player.Turn(1);
                rightPressed = true;
            }
            else if (rightPressed && !turnRightButton.GetState(TurnController)){
                player.Turn(0);
                rightPressed = false;
            }

            if (!leftPressed && turnLeftButton.GetState(TurnController)) {
                player.Turn(-1);
                leftPressed = true;
            }
            else if (leftPressed && !turnLeftButton.GetState(TurnController)) {
                player.Turn(0);
                leftPressed = false;
            }
        }
#endif
    }
}