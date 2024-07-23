using UnityEngine;

namespace CloudFine.ThrowLab
{
    public abstract class DeviceDetector : MonoBehaviour
    {
        public HandSide Side { get { return _side; } }
#pragma warning disable 0649
        [SerializeField] private HandSide _side;
#pragma warning restore 0649

        public Device DetectedDevice { get { return _detected; } }
        private Device _detected = Device.UNSPECIFIED;

        public bool _drawGizmo = true;

        protected void OnControllerTypeDetermined(Device device)
        {
            _detected = device;
        }



        private void OnDrawGizmos()
        {
            if (_drawGizmo)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(transform.TransformPoint(DeviceDetectionUtility.GetCenterOfMassOffset(_detected,Side)), .01f);
            }
        }

    }
}