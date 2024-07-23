using UnityEngine;
using Valve.VR;


namespace Wacki {

    public class ViveUILaserPointer : IUILaserPointer {

        public SteamVR_Behaviour_Pose pose;

        //public SteamVR_Action_Boolean interactWithUI = SteamVR_Input.__actions_default_in_InteractUI;
        public SteamVR_Action_Boolean interactWithUI = SteamVR_Input.GetBooleanAction("InteractUI");

        private SteamVR_TrackedObject _trackedObject;
        private bool _connected = false;

        protected override void Initialize()
        {
            base.Initialize();

            _trackedObject = GetComponent<SteamVR_TrackedObject>();

            if(_trackedObject != null) {
                _connected = true;
            }
        }

        public override bool ButtonDown()
        {
            if (!pose) return false;
            return interactWithUI.GetStateDown(pose.inputSource);
        }

        public override bool ButtonUp()
        {
            if (!pose) return false;
            return interactWithUI.GetStateUp(pose.inputSource);
        }

        
        public override void OnEnterControl(GameObject control)
        {
            if (!_connected)
                return;
        }

        public override void OnExitControl(GameObject control)
        {
            if (!_connected)
                return; 
        }

    }

}