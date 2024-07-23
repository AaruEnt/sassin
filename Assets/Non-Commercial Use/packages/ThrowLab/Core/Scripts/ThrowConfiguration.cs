using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;

namespace CloudFine.ThrowLab
{
    [CreateAssetMenu(fileName = "NewThrowConfig", menuName = "ThrowLab/ThrowConfiguration", order = 1)]
    public class ThrowConfiguration : ScriptableObject
    {
        public enum EstimationAlgorithm
        {
            SIMPLE_AVERAGE,
            WEIGHTED_AVERAGE,
            EXPONENTIAL_AVERAGE,
            CUSTOM_CURVE,
        }

        public enum PeriodMeasurement
        {
            FRAMES,
            TIME
        }

        public enum SampleTime
        {
            SCALED,
            UNSCALED,
            FIXED
        }

        public enum VelocitySource
        {
            DEVICE_CENTER_OF_MASS,
            HAND_TRACKED_POSITION,
            OBJECT_CENTER,
            OBJECT_CUSTOM_OFFSET,
        }

        public enum AssistTargetMethod
        {
            GAZE,
            NEAREST
        }

       

        public enum FalloffSource
        {
            TIME,
            DISTANCE,
            MIN_TIME_DIST,
        }


        [Header("Velocity Smoothing")]
        public bool smoothingEnabled = true;
        public EstimationAlgorithm estimationFunction = EstimationAlgorithm.EXPONENTIAL_AVERAGE;
        public PeriodMeasurement samplePeriodMeasurement = PeriodMeasurement.TIME;
        public SampleTime sampleTime = SampleTime.UNSCALED;
        public int periodFrames = 5;
        public float periodSeconds = .2f;
        public VelocitySource sampleSourceType;
        public AnimationCurve smoothingAverageCustomCurve = new AnimationCurve(new Keyframe(0, .2f), new Keyframe(.5f, 1), new Keyframe(1, .2f));


        [Header("Velocity Scaling")]
        public bool scaleEnabled = true;
        public float scaleMultiplier = 2.5f;
        public float scaleThreshold = 6;
        [Range(0, 5)] public float scaleRampExponent = 2;
        public bool useScaleRampCustomCurve;
        public AnimationCurve scaleRampCustomCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("Aim Assist")]
        public bool assistEnabled = true;
        [Range(0, 1)] public float assistWeight = 1;
        [Range(0, 180)] public float assistRangeDegrees = 45;
        [Range(0, 5)] public float assistRampExponent = 2;
        public bool useAssistRampCustomCurve;
        public AnimationCurve assistRampCustomCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        public AssistTargetMethod assistTargetMethod;

        [Header("Friction")]
        public bool frictionEnabled = false;
        public float frictionFalloffSeconds = .02f;
        [Range(0, 5)] public float frictionFalloffExponent = 2;
        public bool useFrictionFalloffCustomCurve;
        public AnimationCurve frictionFalloffCustomCurve = AnimationCurve.Linear(0, 1, 1, 0);


        private void Awake()
        {
#if !UNITY_EDITOR
            LoadFromJSON();
#endif
        }

        public float SampleAssistCurve(float t)
        {
            if (useAssistRampCustomCurve)
            {
                return assistRampCustomCurve.Evaluate(t);
            }

            return SampleExponentialCurve(t, assistRampExponent);
        }

        public float SampleFrictionCurve(float t)
        {
            if (useFrictionFalloffCustomCurve)
            {
                return frictionFalloffCustomCurve.Evaluate(t);
            }

            return SampleExponentialCurve(t, frictionFalloffExponent, true);
        }

        public float SampleScalingCurve(float t)
        {
            if (useScaleRampCustomCurve)
            {
                return scaleRampCustomCurve.Evaluate(t);
            }

            return SampleExponentialCurve(t, scaleRampExponent);
        }

        private static float SampleExponentialCurve(float t, float exp, bool flip = false)
        {
            t = Mathf.Clamp01(t);
            
            float val = (Mathf.Pow(t, exp));
            if (flip)
            {
                val = 1 - val;
            }
            return val;
        }

        public Vector3 GetEstimate(Vector3[] inputs, out float[] componentWeights)
        {
            switch (estimationFunction)
            {
                case ThrowConfiguration.EstimationAlgorithm.EXPONENTIAL_AVERAGE:
                    return EstimationUtility.ExponentialMovingAverage(inputs, out componentWeights);
                case ThrowConfiguration.EstimationAlgorithm.SIMPLE_AVERAGE:
                    return EstimationUtility.SimpleAverage(inputs, out componentWeights);
                case ThrowConfiguration.EstimationAlgorithm.WEIGHTED_AVERAGE:
                    return EstimationUtility.WeightedMovingAverage(inputs, out componentWeights);
                case ThrowConfiguration.EstimationAlgorithm.CUSTOM_CURVE:
                    return EstimationUtility.CustomCurveAverage(inputs, smoothingAverageCustomCurve, out componentWeights);
                default:
                    return EstimationUtility.SimpleAverage(inputs, out componentWeights);
            }
        }

        public float[] GetWeights(Vector3[] inputs)
        {
            float[] weights;
            GetEstimate(inputs, out weights);
            return weights;
        }


        public ThrowConfiguration Clone()
        {
            ThrowConfiguration clone = ScriptableObject.CreateInstance<ThrowConfiguration>();
            CopyTo(clone);      
            return clone;
        }

        public void CopyTo(ThrowConfiguration other)
        {
            var sourceFields = this.GetType().GetFields().ToList();
            foreach (var sourceField in sourceFields)
            {
                sourceField.SetValue(other, sourceField.GetValue(this));
            }
        }

        [SerializeField, HideInInspector]
        private string uniqueID = System.Guid.NewGuid().ToString();

        private string path
        {
            get
            {
                return saveDirectory + "/" + this.name + "_" + uniqueID + ".json";
            }
        }
        private string saveDirectory
        {
            get
            {
                return Application.persistentDataPath + "/ThrowConfigs";
            }
        }

        public void SaveToJSON()
        {
            if (!Directory.Exists(saveDirectory))
            {
                Directory.CreateDirectory(saveDirectory);
            }
            if (!File.Exists(path))
            {
                File.CreateText(path).Dispose();
            }
            File.WriteAllText(path, JsonUtility.ToJson(this));
        }

        public void LoadFromJSON()
        {
            if (File.Exists(path))
            {
                JsonUtility.FromJsonOverwrite(File.ReadAllText(path), this);
            }
        }
    }
}