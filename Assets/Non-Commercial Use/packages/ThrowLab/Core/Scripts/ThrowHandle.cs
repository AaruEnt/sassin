using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using UnityEngine.Serialization;

namespace CloudFine.ThrowLab
{
    public class ThrowHandle : MonoBehaviour
    {

        public Action onDetachFromHand;
        public Action<GameObject, GameObject> onPickUp;
        public Action<Vector3> onFinalTrajectory;
        public Action<VelocitySample> OnSampleRecorded;
        public Action<ThrowHandle> OnDestroyHandle;
        public Action onFrictionApplied; //this exists so ThrowTracker knows to visualize friction, if necessary.

        public bool _attached { get; private set; }
        public float _timeOfRelease { get; protected set; }
        public bool _applyingInfluence { get; protected set; }
        public bool _frictionActive
        {
            get
            {
                return !_attached && (_timeOfRelease > 0) && ((Time.time - _timeOfRelease) < Settings.frictionFalloffSeconds);
            }
        }

        public ThrowConfiguration Settings
        {
            get { return _throwConfigurationSet.GetConfigForDevice(_attachedDevice); }
        }

        //ThrowConfigurations are no longer stored like this. Data will be migrated into ThrowConfigurationSet
        [SerializeField, HideInInspector, FormerlySerializedAs("_controllerConfigurations")]
        private ThrowConfiguration[] _deviceConfigurations;
        ///////////////

        [SerializeField] private ThrowConfigurationSet _throwConfigurationSet;

        private Device _attachedDevice = Device.UNSPECIFIED;

        private Transform _velocitySensor;
        private Rigidbody _rigidbody;
        private GameObject _handCollisionRoot;

        private ThrowTarget currentTarget;

        public Action<ThrowTarget> OnTargetedThrow;

        //SAMPLES
        public struct VelocitySample
        {
            public VelocitySample(Vector3 position, Vector3 velocity, Quaternion rotation, Vector3 angular, float time)
            {
                this.position = position;
                this.velocity = velocity;
                this.rotation = rotation;
                this.angularVelocity = angular;
                this.time = time;
            }
            public Vector3 position;
            public Vector3 velocity;
            public Quaternion rotation;
            public Vector3 angularVelocity;
            public float time;
        }

        private List<VelocitySample> _velocityHistory = new List<VelocitySample>();
        Vector3 _sampledPreviousPosition;
        Quaternion _sampledPreviousRotation;

        //Track root motion to prevent it from being included with velocity scaling
        private Transform _rootMotionTransform;
        private Vector3 _rootVelocity;
        private Vector3 _previousRootPosition;
        //
        #region Lifecycle

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _velocitySensor = new GameObject().transform;
            _velocitySensor.name = "VelocitySensor";

            _velocitySensor.SetParent(this.transform);
            _velocitySensor.localPosition = Vector3.zero;
            _velocitySensor.localRotation = Quaternion.identity;

            MigrateData();
        }

        private void OnValidate()
        {
            MigrateData();
        }

        private void MigrateData()
        {
            if (_deviceConfigurations != null && _deviceConfigurations.Length != 0)
            {
                _throwConfigurationSet.SetConfigs(_deviceConfigurations.ToArray());
                _deviceConfigurations = new ThrowConfiguration[0];
            }
        }

        private void Update()
        {
            if (_attached || _applyingInfluence)
            {
                if (Settings.sampleTime == ThrowConfiguration.SampleTime.SCALED)
                {
                    RecordVelocitySample(Time.deltaTime, Time.time);
                }
                else if (Settings.sampleTime == ThrowConfiguration.SampleTime.UNSCALED)
                {
                    RecordVelocitySample(Time.unscaledDeltaTime, Time.unscaledTime);
                }
            }

            if (_attached)
            {
                if (Settings.assistEnabled)
                {
                    switch (Settings.assistTargetMethod)
                    {
                        case ThrowConfiguration.AssistTargetMethod.GAZE:
                            ThrowTarget best = FindBestGazeBasedThrowTarget(ThrowTarget.AllTargets);
                            if (currentTarget)
                            {
                                if (currentTarget != best)
                                {
                                    currentTarget.RemoveTargettingHandle(this);
                                    currentTarget = best;
                                    if (currentTarget)
                                    {
                                        currentTarget.AddTargettingHandle(this);
                                    }
                                }
                            }
                            else if (best)
                            {
                                currentTarget = best;
                                currentTarget.AddTargettingHandle(this);
                            }
                            break;
                        case ThrowConfiguration.AssistTargetMethod.NEAREST:
                            if (currentTarget)
                            {
                                currentTarget.RemoveTargettingHandle(this);
                                currentTarget = null;
                            }
                            break;
                    }
                }
            }

            if (_applyingInfluence && !_attached && !_frictionActive)
            {
                _applyingInfluence = false;
                if (onFinalTrajectory != null) onFinalTrajectory.Invoke(_rigidbody.velocity);
            }
        }

        private void FixedUpdate()
        {
            if (_attached || _applyingInfluence)
            {
                if (Settings.sampleTime == ThrowConfiguration.SampleTime.FIXED)
                {
                    RecordVelocitySample(Time.fixedDeltaTime, Time.time);
                }

                if (_frictionActive)
                {
                    ApplyFriction();
                }
            }
        }

        private void LateUpdate()
        {
            if (_rootMotionTransform != null)
            {
                _rootVelocity = (_rootMotionTransform.position - _previousRootPosition) / Time.deltaTime;
                _previousRootPosition = _rootMotionTransform.position;
            }
        }

        private void OnDestroy()
        {
            if (_velocitySensor)
            {
                GameObject.Destroy(_velocitySensor.gameObject);
            }
            if (OnDestroyHandle != null) OnDestroyHandle.Invoke(this);
        }

        #endregion

        #region DeviceConfigs

        public ThrowConfigurationSet GetConfigSet()
        {
            return _throwConfigurationSet;
        }
        public void SetConfigSet(ThrowConfigurationSet set)
        {
            _throwConfigurationSet = set;
        }
        public ThrowConfiguration GetConfigForDevice(Device device)
        {
            return _throwConfigurationSet.GetConfigForDevice(device);
        }

        public void SetConfigForDevice(Device device, ThrowConfiguration config)
        {
            _throwConfigurationSet.SetConfigForDevice(device, config);
        }

        #endregion

        #region SampleRecording

        private void RecordVelocitySample(float deltaTime, float time)
        {
            Transform anchor = GetSampleSource();
            Vector3 positionDelta = anchor.position - _sampledPreviousPosition;
            Quaternion deltaRotation = anchor.rotation * Quaternion.Inverse(_sampledPreviousRotation);
            Vector3 angularDelta = new Vector3(deltaRotation.x, deltaRotation.y, deltaRotation.z);

            VelocitySample newSample = new VelocitySample(anchor.position, positionDelta / deltaTime, anchor.rotation, angularDelta / deltaTime, time);
            _velocityHistory.Add(newSample);
            _sampledPreviousPosition = anchor.position;
            _sampledPreviousRotation = anchor.rotation;

            ClearOldSamples();
            if (OnSampleRecorded != null) OnSampleRecorded.Invoke(newSample);
        }

        public Transform GetSampleSource()
        {
            if (_velocitySensor) return _velocitySensor;
            return transform;
        }

        private void ClearOldSamples()
        {

            switch (Settings.samplePeriodMeasurement)
            {
                case ThrowConfiguration.PeriodMeasurement.FRAMES:
                    while (_velocityHistory.Count > Settings.periodFrames)
                    {
                        _velocityHistory.RemoveAt(0);
                    }
                    break;
                case ThrowConfiguration.PeriodMeasurement.TIME:
                    float cutoffTime = (Settings.sampleTime == ThrowConfiguration.SampleTime.UNSCALED ? Time.unscaledTime : Time.time) - Settings.periodSeconds;
                    _velocityHistory.RemoveAll(x => (cutoffTime - x.time) > 0);
                    break;
            }
        }

        public List<VelocitySample> GetSampleWeights(out float[] weights)
        {
            Vector3[] dummySamples = _velocityHistory.Select(x => x.velocity).ToArray();
            Settings.GetEstimate(dummySamples, out weights);
            return _velocityHistory;
        }

        #endregion

        #region Events

        public void OnAttach(GameObject hand, GameObject collisionRoot)
        {
            _attached = true;
            _applyingInfluence = true;
            DeviceDetector detector = hand.GetComponentInChildren<DeviceDetector>();
            if (detector != null)
            {
                _attachedDevice = detector.DetectedDevice;
            }
            switch (Settings.sampleSourceType)
            {
                case ThrowConfiguration.VelocitySource.DEVICE_CENTER_OF_MASS:
                    _velocitySensor.SetParent(hand.transform);
                    HandSide side = HandSide.RIGHT;
                    if (detector)
                    {
                        side = detector.Side;
                    }
                    _velocitySensor.localPosition = DeviceDetectionUtility.GetCenterOfMassOffset(_attachedDevice, side);
                    break;
                case ThrowConfiguration.VelocitySource.HAND_TRACKED_POSITION:
                    _velocitySensor.SetParent(hand.transform);
                    break;
                case ThrowConfiguration.VelocitySource.OBJECT_CENTER:
                    _velocitySensor.SetParent(this.transform);
                    break;
                case ThrowConfiguration.VelocitySource.OBJECT_CUSTOM_OFFSET:
                    Transform custom = hand.transform.Find("CustomVelocitySensor");
                    if (custom != null)
                    {
                        _velocitySensor.SetParent(custom);
                    }
                    else
                    {
                        Debug.LogWarning("Could not find CustomVelocitySensor on " + hand.name, hand.gameObject);
                        _velocitySensor.SetParent(this.transform);
                    }
                    break;
            }

            _velocitySensor.localRotation = Quaternion.identity;
            _handCollisionRoot = collisionRoot;
            _rootMotionTransform = hand.transform.root;
            _previousRootPosition = _rootMotionTransform.position;

            if (onPickUp != null) onPickUp.Invoke(hand, collisionRoot);
        }

        public void OnDetach()
        {
            SetPhysicsEnabled(true);

            _rigidbody.velocity = GetVelocityEstimate();
            _rigidbody.angularVelocity = GetAngularVelocityEstimate();
            _rootMotionTransform = null;

            _attached = false;
            if (currentTarget)
            {
                if (OnTargetedThrow != null)
                {
                    OnTargetedThrow.Invoke(currentTarget);
                }
                currentTarget.RemoveTargettingHandle(this);
                currentTarget = null;
            }

            if (_handCollisionRoot)
            {
                _velocitySensor.SetParent(_handCollisionRoot.transform);
            }

            if (_handCollisionRoot)
            {
                IgnoreCollisionWithOtherForFixedUpdate(_handCollisionRoot);
            }

            if (onDetachFromHand != null) onDetachFromHand.Invoke();
            _timeOfRelease = Time.time;
        }

        #endregion

        #region Physics

        public void SetPhysicsEnabled(bool collision)
        {
            if (collision)
            {
                transform.SetParent(null);
            }
            _rigidbody.isKinematic = !collision;
            foreach (Collider col in GetComponentsInChildren<Collider>())
            {
                col.enabled = collision;
            }
        }

        public void IgnoreCollisionWithOther(GameObject other, bool ignore)
        {
            foreach (Collider i_col in this.GetComponentsInChildren<Collider>())
            {
                foreach (Collider j_col in other.GetComponentsInChildren<Collider>())
                {
                    Physics.IgnoreCollision(i_col, j_col, ignore);
                }
            }
        }

        public void IgnoreCollisionWithOtherForFixedUpdate(GameObject other)
        {
            StartCoroutine(IgnoreCollisionWithOtherRoutine(other));
        }

        protected IEnumerator IgnoreCollisionWithOtherRoutine(GameObject other)
        {
            IgnoreCollisionWithOther(other, true);
            yield return new WaitForSeconds(.25f);
            yield return new WaitForEndOfFrame();
            yield return new WaitForFixedUpdate();
            IgnoreCollisionWithOther(other, false);
        }

        #endregion

        #region Smoothing

        public Vector3 GetVelocityEstimate()
        {
            Vector3 velocity = Vector3.zero;

            //SMOOTHING
            if (Settings.smoothingEnabled)
            {
                Vector3[] inputs = _velocityHistory.Select(x => x.velocity).ToArray();
                velocity = GetEstimate(inputs);
            }
            else
            {
                velocity = _velocityHistory[_velocityHistory.Count - 1].velocity;
            }

            //SCALE
            if (Settings.scaleEnabled)
            {
                velocity = ApplySpeedIncrease(velocity);
            }

            //ASSIST
            if (Settings.assistEnabled)
            {
                switch (Settings.assistTargetMethod)
                {
                    case ThrowConfiguration.AssistTargetMethod.GAZE:
                        if (currentTarget)
                        {
                            velocity = ApplyAssist(velocity, transform.position, currentTarget.GetTargetPosition());
                        }
                        break;
                    case ThrowConfiguration.AssistTargetMethod.NEAREST:
                        currentTarget = FindClosestThrowTarget(transform.position, velocity, ThrowTarget.AllTargets);
                        if (currentTarget)
                        {
                            currentTarget.AddTargettingHandle(this);
                            velocity = ApplyAssist(velocity, transform.position, currentTarget.GetTargetPosition());
                            currentTarget.RemoveTargettingHandle(this);
                        }

                        break;
                }
            }
            return velocity;

        }

        public Vector3 GetAngularVelocityEstimate()
        {
            Vector3 angularVelocity = Vector3.zero;
            if (Settings.smoothingEnabled)
            {
                Vector3[] inputs = _velocityHistory.Select(x => x.angularVelocity).ToArray();
                angularVelocity = GetEstimate(inputs);
            }
            else
            {
                angularVelocity = _velocityHistory[_velocityHistory.Count - 1].angularVelocity;
            }
            return angularVelocity;
        }

        private Vector3 GetEstimate(Vector3[] inputs)
        {
            float[] weights;
            return Settings.GetEstimate(inputs, out weights);
        }



        #endregion

        #region Assist

        private ThrowTarget FindBestGazeBasedThrowTarget(List<ThrowTarget> targets)
        {
            Camera cam = Camera.main;
            ThrowTarget best = null;
            float minAngle = float.MaxValue;
            for (int i = 0; i < targets.Count; i++)
            {
                float angle = Vector3.Angle(cam.transform.forward, (targets[i].transform.position - cam.transform.position));
                if (angle < minAngle)
                {
                    minAngle = angle;
                    best = targets[i];
                }
            }
            return best;
        }

        private ThrowTarget FindClosestThrowTarget(Vector3 origin, Vector3 rawVelocity, List<ThrowTarget> targets)
        {
            ThrowTarget best = null;
            Vector3 ideal1, ideal2;

            float minAngle = float.MaxValue;
            for (int i = 0; i < targets.Count; i++)
            {
                int numSolutions = BallisticsUtility.solve_ballistic_arc(origin, rawVelocity.magnitude, targets[i].GetTargetPosition(), -Physics.gravity.y, out ideal1, out ideal2);
                if (numSolutions == 0) continue;

                float angle = Mathf.Min(Vector3.Angle(ideal1, rawVelocity), Vector3.Angle(ideal2, rawVelocity));
                if (angle < minAngle)
                {
                    minAngle = angle;
                    best = targets[i];
                }
            }
            return best;
        }

        private Vector3 ApplyAssist(Vector3 rawVelocity, Vector3 origin, Vector3 targetPosition)
        {
            Vector3 ideal1, ideal2;
            int numSolutions = BallisticsUtility.solve_ballistic_arc(origin, rawVelocity.magnitude, targetPosition, -Physics.gravity.y, out ideal1, out ideal2);

            //cannot apply assist
            if (numSolutions == 0) return rawVelocity;
            if (Settings.assistRangeDegrees == 0) return rawVelocity;

            //find which potential solution is closer to actual throw
            Vector3 idealVelocity = Vector3.Angle(rawVelocity, ideal1) < Vector3.Angle(rawVelocity, ideal2) ? ideal1 : ideal2;

            //1 is perfect accuracy, 0 is the edge of assistable range
            float rawAccuracy = (1 - Vector3.Angle(rawVelocity, idealVelocity) / Settings.assistRangeDegrees);
            if (rawAccuracy < 0) return rawVelocity;

            float ramp = Settings.SampleAssistCurve(rawAccuracy);
            ramp *= Settings.assistWeight;

            return Vector3.Lerp(rawVelocity, idealVelocity, ramp);
        }

        #endregion

        #region Scaling

        private Vector3 ApplySpeedIncrease(Vector3 rawVelocity)
        {

            float ramp = 1;
            Vector3 localHandVelocity = rawVelocity - _rootVelocity;
            if (Settings.scaleThreshold > 0 && Settings.scaleRampExponent > 0) ramp = Settings.SampleScalingCurve((localHandVelocity.magnitude / Settings.scaleThreshold));
            return localHandVelocity.normalized * localHandVelocity.magnitude * Mathf.Lerp(1, Settings.scaleMultiplier, ramp) + _rootVelocity;
        }

        #endregion

        #region Friction

        public float GetHandFriction()
        {
            if (_attached)
            {
                return 1;
            }
            else if (Settings.frictionEnabled)
            {
                if (_timeOfRelease > 0)
                {
                    float t = (Time.time - _timeOfRelease) / Settings.frictionFalloffSeconds;
                    if (t > 0 && t <= 1)
                    {
                        return Settings.SampleFrictionCurve(t);
                    }
                }
            }

            return 0;
        }

        private void ApplyFriction()
        {
            _rigidbody.velocity = Vector3.Lerp(_rigidbody.velocity, GetVelocityEstimate(), GetHandFriction());
            if (onFrictionApplied != null) onFrictionApplied.Invoke();
        }

        #endregion
    }
}
