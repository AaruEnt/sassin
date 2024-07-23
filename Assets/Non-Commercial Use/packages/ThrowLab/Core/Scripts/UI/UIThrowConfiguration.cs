using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

namespace CloudFine.ThrowLab.UI
{
    public class UIThrowConfiguration : MonoBehaviour
    {
        private ThrowConfiguration currentConfig;
        private Color currentColor;
        public Text configLabel;
        public GameObject configOptionsRoot;
        public GameObject variantPanelRoot;

        [Header("Smoothing")]
        public Toggle smoothingToggle;
        public GameObject smoothingOptionsRoot;
        public Dropdown smoothingAlgorithmDropdown;
        public Dropdown smoothingPeriodDropdown;
        public Dropdown smoothingTimeDropdown;
        public UIStepper smoothingFramesStepper;
        public UIStepper smoothignSecondsStepper;
        public Dropdown smoothingPointDropdown;
        public UISmoothingVisual smoothingUI;

        [Header("Friction")]
        public Toggle frictionToggle;
        public GameObject frictionOptionsRoot;
        public Slider frictionFalloffSlider;
        public Toggle frictionCustomCurveToggle;
        public UIStepper frictionSecondsStepper;
        public UICurveLine frictionCurveUI;

        [Header("Assist")]
        public Toggle assistToggle;
        public GameObject assistOptionsRoot;
        public Toggle assistCustomCurveToggle;
        public Slider assistGravitySlider;
        public Slider assistWeightSlider;
        public Slider assistRangeSlider;
        public UICurveLine assistCurveUI;
        public Dropdown targetSelectionDropdown;

        [Header("Scale")]
        public Toggle scaleToggle;
        public GameObject scaleOptionsRoot;
        public Toggle scaleCustomCurveToggle;
        public Slider scaleRampSlider;
        public UIStepper scaleStepper;
        public UIStepper scaleThresholdStepper;
        public UICurveLine scaleCurveUI;

        private void Awake()
        {
            smoothingAlgorithmDropdown.ClearOptions();
            smoothingAlgorithmDropdown.AddOptions(System.Enum.GetNames(typeof(ThrowConfiguration.EstimationAlgorithm)).ToList());

            smoothingPeriodDropdown.ClearOptions();
            smoothingPeriodDropdown.AddOptions(System.Enum.GetNames(typeof(ThrowConfiguration.PeriodMeasurement)).ToList());

            smoothingTimeDropdown.ClearOptions();
            smoothingTimeDropdown.AddOptions(System.Enum.GetNames(typeof(ThrowConfiguration.SampleTime)).ToList());

            smoothingPointDropdown.ClearOptions();
            smoothingPointDropdown.AddOptions(System.Enum.GetNames(typeof(ThrowConfiguration.VelocitySource)).ToList());

            targetSelectionDropdown.ClearOptions();
            targetSelectionDropdown.AddOptions(System.Enum.GetNames(typeof(ThrowConfiguration.AssistTargetMethod)).ToList());
        }

        public void LoadConfig(ThrowConfiguration config, Color color, bool enabled)
        {
            //set values on ui
            currentConfig = config;
            configLabel.text = config.name;
            currentColor = color;

            //SMOOTHING
            if (smoothingAlgorithmDropdown)
            {
                smoothingAlgorithmDropdown.value = (int)config.estimationFunction;
                smoothingAlgorithmDropdown.onValueChanged.Invoke((int)config.estimationFunction);
            }

            if (smoothingPeriodDropdown)
            {
                smoothingPeriodDropdown.value = (int)config.samplePeriodMeasurement;
                smoothingPeriodDropdown.onValueChanged.Invoke((int)config.samplePeriodMeasurement);
            }

            if (smoothingTimeDropdown) smoothingTimeDropdown.value = (int)config.sampleTime;
            if (smoothingFramesStepper) smoothingFramesStepper.value = config.periodFrames;
            if (smoothignSecondsStepper) smoothignSecondsStepper.value = config.periodSeconds;
            if (smoothingPointDropdown) smoothingPointDropdown.value = (int)config.sampleSourceType;

            if (smoothingUI)
            {
                smoothingUI.SetFunc(config.GetWeights);
            }

            //ASSIST
            if (assistGravitySlider)
            {
                assistGravitySlider.value = config.assistRampExponent;
                assistGravitySlider.onValueChanged.Invoke(config.assistRampExponent);
            }

            if (assistWeightSlider)
            {
                assistWeightSlider.value = config.assistWeight;
                assistWeightSlider.onValueChanged.Invoke(config.assistWeight);
            }

            if (assistRangeSlider)
            {
                assistRangeSlider.value = config.assistRangeDegrees;
                assistRangeSlider.onValueChanged.Invoke(config.assistRangeDegrees);
            }

            if (assistCurveUI)
            {
                assistCurveUI.SetCurveFunc(config.SampleAssistCurve);
            }
            if (assistCustomCurveToggle)
            {
                assistCustomCurveToggle.isOn = config.useAssistRampCustomCurve;
            }
            if (targetSelectionDropdown)
            {
                targetSelectionDropdown.value = (int)config.assistTargetMethod;
            }


            //SCALE
            if (scaleStepper)
            {
                scaleStepper.value = config.scaleMultiplier;
            }
            if (scaleThresholdStepper)
            {
                scaleThresholdStepper.value = config.scaleThreshold;
            }
            if (scaleRampSlider)
            {
                scaleRampSlider.value = config.scaleRampExponent;
                scaleRampSlider.onValueChanged.Invoke(config.scaleRampExponent);
            }


            if (scaleCurveUI)
            {
                scaleCurveUI.SetCurveFunc(config.SampleScalingCurve);
            }
            if (scaleCustomCurveToggle)
            {
                scaleCustomCurveToggle.isOn = config.useScaleRampCustomCurve;
            }

            //FRICTION
            if (frictionFalloffSlider)
            {
                frictionFalloffSlider.value = config.frictionFalloffExponent;
                frictionFalloffSlider.onValueChanged.Invoke(config.frictionFalloffExponent);
            }
            if (frictionSecondsStepper)
            {
                frictionSecondsStepper.value = config.frictionFalloffSeconds;
            }
            if (frictionCurveUI)
            {
                frictionCurveUI.SetCurveFunc(config.SampleFrictionCurve);
            }
            if (frictionCustomCurveToggle)
            {
                frictionCustomCurveToggle.isOn = config.useFrictionFalloffCustomCurve;
            }

            if (smoothingToggle)
            {
                smoothingToggle.isOn = config.smoothingEnabled;
                smoothingToggle.interactable = enabled;
            }
            if (assistToggle)
            {
                assistToggle.isOn = config.assistEnabled;
                assistToggle.interactable = enabled;
            }
            if (frictionToggle)
            {
                frictionToggle.isOn = config.frictionEnabled;
                frictionToggle.interactable = enabled;
            }
            if (scaleToggle)
            {
                scaleToggle.isOn = config.scaleEnabled;
                scaleToggle.interactable = enabled;
            }


            SetAssistEnabled(config.assistEnabled, enabled);
            SetFrictionEnabled(config.frictionEnabled, enabled);
            SetSmoothingEnabled(config.smoothingEnabled, enabled);
            SetScalingEnabled(config.scaleEnabled, enabled);

            SetChildrenColor(variantPanelRoot, color);
        }


        public void SetAssistEnabled(bool enabled)
        {
            SetAssistEnabled(enabled, true);
        }

        public void SetAssistEnabled(bool enabled, bool configEnabled)
        {
            if (currentConfig) currentConfig.assistEnabled = enabled;
            SetPanelEnabled(assistOptionsRoot, assistToggle.gameObject, enabled && configEnabled);
            SetAssistCustomCurve(currentConfig.useAssistRampCustomCurve);
        }

        public void SetFrictionEnabled(bool enabled)
        {
            SetFrictionEnabled(enabled, true);
        }

        public void SetFrictionEnabled(bool enabled, bool configEnabled)
        {
            if (currentConfig) currentConfig.frictionEnabled = enabled;
            SetPanelEnabled(frictionOptionsRoot, frictionToggle.gameObject, enabled && configEnabled);
            SetFrictionCustomCurve(currentConfig.useFrictionFalloffCustomCurve);
        }

        public void SetSmoothingEnabled(bool enabled)
        {
            SetSmoothingEnabled(enabled, true);
        }

        public void SetSmoothingEnabled(bool enabled, bool configEnabled)
        {
            if (currentConfig) currentConfig.smoothingEnabled = enabled;
            SetPanelEnabled(smoothingOptionsRoot, smoothingToggle.gameObject, enabled && configEnabled);
        }

        public void SetScalingEnabled(bool enabled)
        {
            SetScalingEnabled(enabled, true);
        }

        public void SetScalingEnabled(bool enabled, bool configEnabled)
        {
            if (currentConfig) currentConfig.scaleEnabled = enabled;
            SetPanelEnabled(scaleOptionsRoot, scaleToggle.gameObject, enabled && configEnabled);
            SetScalingCustomCurve(currentConfig.useScaleRampCustomCurve);
        }

        private void SetPanelEnabled(GameObject panelRoot, GameObject toggle, bool enabled)
        {
            foreach (Selectable select in panelRoot.GetComponentsInChildren<Selectable>())
            {
                select.interactable = enabled;
            }
            Color c = enabled ? currentColor : new Color(1, 1, 1, .25f);

            SetChildrenColor(panelRoot, c);
            SetChildrenColor(toggle, currentColor);            
        }

        private void SetChildrenColor(GameObject root, Color c)
        {
            foreach (Graphic select in root.GetComponentsInChildren<Graphic>(includeInactive: true))
            {
                if (select.GetComponent<UIColorMeTag>() != null)
                {
                    select.color = c;
                }
            }
        }


        public void SetEstimationAlgorithm(int value)
        {
            if (currentConfig) currentConfig.estimationFunction = (ThrowConfiguration.EstimationAlgorithm)value;
        }

        public void SetPeriodMeasurement(int value)
        {
            ThrowConfiguration.PeriodMeasurement period = (ThrowConfiguration.PeriodMeasurement)value;
            if (currentConfig) currentConfig.samplePeriodMeasurement = period;

            switch (period)
            {
                case ThrowConfiguration.PeriodMeasurement.FRAMES:
                    smoothignSecondsStepper.gameObject.SetActive(false);
                    smoothingFramesStepper.gameObject.SetActive(true);
                    smoothingTimeDropdown.gameObject.SetActive(false);
                    break;
                case ThrowConfiguration.PeriodMeasurement.TIME:
                    smoothingFramesStepper.gameObject.SetActive(false);
                    smoothignSecondsStepper.gameObject.SetActive(true);
                    smoothingTimeDropdown.gameObject.SetActive(true);
                    break;
            }

        }


        //SMOOTHING
        public void SetSampleSource(int value)
        {
            if(currentConfig) currentConfig.sampleSourceType = (ThrowConfiguration.VelocitySource)value;
        }

        public void SetSmoothingSampleTime(int value)
        {
            if (currentConfig) currentConfig.sampleTime = (ThrowConfiguration.SampleTime)value;
        }

        public void SetSmoothingSeconds(float seconds)
        {
            if (currentConfig) currentConfig.periodSeconds = seconds;
        }

        public void SetSmoothingFrames(float frames)
        {
            if (currentConfig) currentConfig.periodFrames = (int)frames;
        }




        //ASSIST
        public void SetAssistRange(float range)
        {
            if (currentConfig) currentConfig.assistRangeDegrees = range;
        }

        public void SetAssistGravity(float gravity)
        {
            if (currentConfig)
            {
                currentConfig.assistRampExponent = gravity;
                
            }
        }

        public void SetAssistWeight(float weight)
        {
            if (currentConfig) currentConfig.assistWeight = weight;
        }

        public void SetAssistCustomCurve(bool value)
        {
            if (currentConfig) currentConfig.useAssistRampCustomCurve = value;
            if (assistGravitySlider) assistGravitySlider.interactable = !value;
            if (assistCurveUI) assistCurveUI.RefreshCurve();
        }

        public void SetTargetSelectionMethod(int value)
        {
            if (currentConfig) currentConfig.assistTargetMethod = (ThrowConfiguration.AssistTargetMethod)value;
        }


        //SCALING
        public void SetScalingMultiplier(float scale)
        {
            if (currentConfig) currentConfig.scaleMultiplier = scale;
        }

        public void SetScalingThreshold(float threshold)
        {
            if (currentConfig) currentConfig.scaleThreshold = threshold;
        }

        public void SetScalingRamp(float value)
        {
            if (currentConfig) currentConfig.scaleRampExponent = value;
        }

        public void SetScalingCustomCurve(bool value)
        {
            if (currentConfig) currentConfig.useScaleRampCustomCurve = value;
            if (scaleRampSlider) scaleRampSlider.interactable = !value;
            if (scaleCurveUI) scaleCurveUI.RefreshCurve();
        }


        //FRICTION
        public void SetFrictionDuration(float value)
        {
            if (currentConfig) currentConfig.frictionFalloffSeconds = value;
        }

        public void SetFrictionFalloff(float value)
        {
            if (currentConfig) currentConfig.frictionFalloffExponent = value;
        }

        public void SetFrictionCustomCurve(bool value)
        {
            if (currentConfig) currentConfig.useFrictionFalloffCustomCurve = value;
            if (frictionFalloffSlider) frictionFalloffSlider.interactable = !value;
            if (frictionCurveUI) frictionCurveUI.RefreshCurve();
        }
    }
}
