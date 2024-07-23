using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

namespace CloudFine.ThrowLab.SteamVR
{
    [RequireComponent(typeof(Hand))]
    public class SteamVR_GrabThresholdModifier : GrabThresholdModifier
    {
        private Hand _hand;

        public SteamVR_Action_Single triggerAction;// = SteamVR_Input.GetAction<SteamVR_Action_Single>("Squeeze");
        private static SteamVR_Action_Boolean_Dummy dummyAction {
            get
            {
                if(_dummyAction == null)
                {
                    _dummyAction = new SteamVR_Action_Boolean_Dummy();
                    _dummyAction.ForceAddSourceToUpdateList(SteamVR_Input_Sources.LeftHand);
                    _dummyAction.ForceAddSourceToUpdateList(SteamVR_Input_Sources.RightHand);
                }
                return _dummyAction;
            }
        }     
        private static SteamVR_Action_Boolean_Dummy _dummyAction;
        public static float grabBegin = 0.55f;
        public static float grabEnd = 0.35f;

        public static float leftGrip { get; private set; }
        public static float rightGrip { get; private set; }

        private static bool updatedThisFrame = false;

        public override float GripValue()
        {
            return triggerAction.GetAxis(_hand.handType);
        }

        public override void SetGrabThreshold(float grip)
        {
            grabBegin = grip;
        }

        public override void SetReleaseThreshold(float grip)
        {
            grabEnd = grip;
        }

        protected void Awake()
        {
            _hand = GetComponent<Hand>();
        }

        protected void Start()
        {
            _hand.grabGripAction = dummyAction;
            _hand.grabPinchAction = dummyAction;
        }

        protected void Update()
        {
            if(_hand.handType == SteamVR_Input_Sources.LeftHand)
            {
                leftGrip = GripValue();
            }
            if(_hand.handType == SteamVR_Input_Sources.RightHand)
            {
                rightGrip = GripValue();
            }
            if (!updatedThisFrame)
            {
                dummyAction.UpdateValues();
                updatedThisFrame = true;
            }

        }

        protected void LateUpdate()
        {
            updatedThisFrame = false;
        }


    }

    #region Steam_VR SPOOF
    /// <summary>
    /// This is non-standard use of Steam_VR for the purpose of playing with grab thresholds at runtime. It is NOT recommended  to be used as final code because not all Steam_VR functionality is intact (like Events).
    /// What IS intact are GetStateDown(handType), GetStateUp(handType), and GetState(handType), which is sufficient for use with Interactables.
    /// It would be much less invasive to make changes to Hand.cs, but I wanted this asset to work with stock SteamVR.
    /// </summary>
    public class SteamVR_Action_Boolean_Dummy : SteamVR_Action_Boolean
    {
        public SteamVR_Action_Boolean_Dummy()
        {
            PreInitialize("GrabGrip");
            Initialize(createNew: true, throwErrors: false);
        }

        public override void PreInitialize(string newActionPath)
        {
            actionPath = newActionPath;

            sourceMap = new SteamVR_Action_Boolean_Source_Map_Dummy();
            sourceMap.PreInitialize(this, actionPath);

            initialized = true;
        }

    }


    public class SteamVR_Action_Boolean_Source_Dummy : SteamVR_Action_Boolean_Source
    {
        public override void UpdateValue()
        {
            float gripVal = 0;
            if (inputSource == SteamVR_Input_Sources.LeftHand)
            {
                gripVal = SteamVR_GrabThresholdModifier.leftGrip;
            }
            if (inputSource == SteamVR_Input_Sources.RightHand)
            {
                gripVal = SteamVR_GrabThresholdModifier.rightGrip;
            }

            lastActionData = actionData;

            actionData.bState = lastActionData.bState ? (gripVal > SteamVR_GrabThresholdModifier.grabEnd) : gripVal > SteamVR_GrabThresholdModifier.grabBegin;
            actionData.bChanged = lastActionData.bState != actionData.bState;

            lastActive = active;

            if (changed)
                changedTime = Time.realtimeSinceStartup + actionData.fUpdateTime;

            updateTime = Time.realtimeSinceStartup;
        }

            public override bool active
            {
                get
                {
                    return true;
                }
            }
    }
    

    public class SteamVR_Action_Boolean_Source_Map_Dummy : SteamVR_Action_Boolean_Source_Map
    {
        public override void Initialize()
        {
            //this will cause errors
            //base.Initialize();
        }

        public override void PreInitialize(SteamVR_Action wrappingAction, string actionPath, bool throwErrors = true)
        {
            fullPath = actionPath;
            action = wrappingAction;

            //this will cause errors
            //actionSet = SteamVR_Input.GetActionSetFromPath(GetActionSetPath());

            direction = SteamVR_ActionDirections.In;

            SteamVR_Input_Sources[] sources = SteamVR_Input_Source.GetAllSources();
            for (int sourceIndex = 0; sourceIndex < sources.Length; sourceIndex++)
            {
                PreinitializeMap(sources[sourceIndex], wrappingAction);
            }

        }
        protected override void PreinitializeMap(SteamVR_Input_Sources inputSource, SteamVR_Action wrappingAction)
        {
            int sourceIndex = (int)inputSource;
            sources[sourceIndex] = new SteamVR_Action_Boolean_Source_Dummy();
            sources[sourceIndex].Preinitialize(wrappingAction, inputSource);
        }
    }
    #endregion
}