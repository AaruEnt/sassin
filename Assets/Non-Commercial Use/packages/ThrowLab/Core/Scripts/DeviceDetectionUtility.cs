using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CloudFine.ThrowLab {

    public enum Device
    {
        UNSPECIFIED,
        OCULUS_TOUCH,
        VIVE,
        KNUCKLES,
        WINDOWS_MR,
        //add more here
    }

    public enum HandSide
    {
        RIGHT, LEFT,
    }

    public static class DeviceDetectionUtility {
        


        public static Vector3 GetCenterOfMassOffset(Device controller, HandSide side)
        {
            Vector3 output = Vector3.zero;
            if(!_centerOfMassOfssets.TryGetValue(controller, out output))
            {
                Debug.LogWarning("Center of Mass not known for controller type " + controller);
            }
            if (side == HandSide.LEFT) output.x *= -1f;

            return output;
        }

        /// <summary>
        /// Center of mass offset right hand controllers.
        /// </summary>
        private static readonly Dictionary<Device, Vector3> _centerOfMassOfssets = new Dictionary<Device, Vector3>
        {
            {Device.UNSPECIFIED, Vector3.zero },
            {Device.OCULUS_TOUCH, new Vector3(0.012f,-0.025f,-0.02f) },
            {Device.VIVE, new Vector3(0f,-0.016f, -.05f) },
            {Device.KNUCKLES, new Vector3(0,0,-0.108f) },
            {Device.WINDOWS_MR, new Vector3(-.0113f,-0.0052f,-0.0805f) }
            //add more there
        };

	
    }
}