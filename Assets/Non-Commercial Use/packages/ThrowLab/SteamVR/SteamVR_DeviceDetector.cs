using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

namespace CloudFine.ThrowLab.SteamVR
{
    [RequireComponent(typeof(SteamVR_Behaviour_Pose))]
    public class SteamVR_DeviceDetector : DeviceDetector
    {
        private SteamVR_Behaviour_Pose _pose;

        private void Awake()
        {
            _pose = GetComponent<SteamVR_Behaviour_Pose>();
            _pose.onDeviceIndexChanged.AddListener(DetectControllerType);
        }


        private ETrackedPropertyError error = ETrackedPropertyError.TrackedProp_Success;
        private void DetectControllerType(SteamVR_Behaviour_Pose pose, SteamVR_Input_Sources source, int index)
        {
            var result = new System.Text.StringBuilder((int)64);
            OpenVR.System.GetStringTrackedDeviceProperty((uint)index, ETrackedDeviceProperty.Prop_ControllerType_String, result, 64, ref error);
            Debug.Log("device " + index + " " + result.ToString() + " " + error);
            if(error == ETrackedPropertyError.TrackedProp_Success)
            {
                Device device;
                string controllerName = result.ToString();
                if (controllerName.Contains("vive_controller"))
                {
                    device = Device.VIVE;
                }
                else if (controllerName.Contains("knuckles"))
                {
                    device = Device.KNUCKLES;
                }
                else if (controllerName.Contains("oculus_touch"))
                {
                    device = Device.OCULUS_TOUCH;
                }
                else if (controllerName.Contains("holographic_controller"))
                {
                    device = Device.WINDOWS_MR;
                }
                else
                {
                    device = Device.UNSPECIFIED;
                    Debug.LogWarning("could not identify controller type " + controllerName);
                }

                OnControllerTypeDetermined(device);  
            }
        }

       
    }
}
