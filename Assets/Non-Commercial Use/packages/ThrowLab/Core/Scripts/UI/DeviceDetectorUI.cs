using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;

namespace CloudFine.ThrowLab {
    public class DeviceDetectorUI : MonoBehaviour {
        public Text _leftDetected;
        public Text _rightDetected;
        public Text _warning;

        private DeviceDetector _leftDetector;
        private DeviceDetector _rightDetector;

        public Device DetectedDevice { get { return _detected; } }
        private Device _detected = Device.UNSPECIFIED;
        public Action<Device> OnDeviceDetected;

        // Use this for initialization
        void Start() {
            DeviceDetector[] detectors = FindObjectsOfType<DeviceDetector>();
            if (detectors.Length > 0)
            {
                _leftDetector = detectors.FirstOrDefault(x => x.Side == HandSide.LEFT);
                _rightDetector = detectors.FirstOrDefault(x => x.Side == HandSide.RIGHT);
            }

            _warning.enabled = (_leftDetector == null && _rightDetector == null);
            if (!_leftDetector)
            {
                _leftDetected.color = Color.red;
                _leftDetected.text = "NOT FOUND";
            }
            if (!_rightDetector)
            {
                _rightDetected.color = Color.red;
                _rightDetected.text = "NOT FOUND";
            }
        }

        // Update is called once per frame
        void Update() {
            if (_leftDetector)
            {
                Device left = _leftDetector.DetectedDevice;
                _leftDetected.color = left == Device.UNSPECIFIED ? Color.white : Color.green;
                _leftDetected.text = left.ToString();
                if(_detected == Device.UNSPECIFIED && left != Device.UNSPECIFIED)
                {
                    _detected = left;
                    if (OnDeviceDetected != null) OnDeviceDetected(_detected);
                }
            }
            if (_rightDetector)
            {
                Device right = _rightDetector.DetectedDevice;
                _rightDetected.color = right == Device.UNSPECIFIED ? Color.white : Color.green;
                _rightDetected.text = right.ToString();
                if (_detected == Device.UNSPECIFIED && right != Device.UNSPECIFIED)
                {
                    _detected = right;
                    if (OnDeviceDetected != null) OnDeviceDetected(_detected);

                }
            }
        }
    } }
