using System;

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace OccaSoftware.Buto.Runtime
{
    [Serializable, VolumeComponentMenuForRenderPipeline("OccaSoftware/Buto", typeof(UniversalRenderPipeline))]
    public sealed class ButoVolumetricFog : VolumeComponent, IPostProcessComponent
    {
        [Tooltip("Set to On to enable Buto.")]
        public VolumetricFogModeParameter mode = new VolumetricFogModeParameter(VolumetricFogMode.Off);

        public QualityLevelParameter qualityLevel = new QualityLevelParameter(QualityLevel.Low);

        // Performance and baseline rendering

        [Tooltip("Early Exit improves performance but may look worse.")]
        public DepthInteractionModeParameter depthInteractionMode = new DepthInteractionModeParameter(DepthInteractionMode.MaximizeSamples, false);

        [Tooltip("Constant generally yields better results, but Prefer Nearby can provide better quality for close additional lights.")]
        public RayLengthModeParameter rayLengthMode = new RayLengthModeParameter(RayLengthMode.Constant, false);

        [Tooltip("Number of times that Buto samples the fog. Expensive.")]
        public ClampedIntParameter sampleCount = new ClampedIntParameter(48, 8, 128);

        [Tooltip("Randomly adjusts sample points each frame. Helps with static noise.")]
        public BoolParameter animateSamplePosition = new BoolParameter(false);

        [Tooltip("Enables self-shadowing. Expensive.")]
        public BoolParameter selfShadowingEnabled = new BoolParameter(true);

        [Tooltip("Controls the octave count for fog self-shadowing. Expensive.")]
        public ClampedIntParameter maximumSelfShadowingOctaves = new ClampedIntParameter(1, 1, 3);

        [Tooltip("Shadows the fog based on a simulated horizon line. Expensive.")]
        public BoolParameter horizonShadowingEnabled = new BoolParameter(true);

        [Tooltip("Sets the range for volumetric fog.")]
        public MinFloatParameter maxDistanceVolumetric = new MinFloatParameter(64, 10);

        [Tooltip("Long-range fog. Disabling can improve performance.")]
        public BoolParameter analyticFogEnabled = new BoolParameter(false);

        [Tooltip("Sets the range for distant fog.")]
        public MinFloatParameter maxDistanceAnalytic = new MinFloatParameter(1000, 100);

        [Tooltip("Density of distant fog.")]
        public MinFloatParameter distantFogDensity = new MinFloatParameter(1f, 0f);

        [Tooltip("Outset the start distance for distant fog.")]
        public MinFloatParameter distantFogStartDistance = new MinFloatParameter(0f, 0f, false);

        [Tooltip("Temporal Anti-Aliasing can reduce noisy fog.")]
        public BoolParameter temporalAntiAliasingEnabled = new BoolParameter(false);

        [Tooltip("Set the strength of the current frame during TAA integration.")]
        public ClampedFloatParameter temporalAntiAliasingIntegrationRate = new ClampedFloatParameter(0.03f, 0.01f, 0.99f);

        [Tooltip("Adjusts fog density based on proximity to nearby geometry.")]
        public MinFloatParameter depthSofteningDistance = new MinFloatParameter(1f, 0f, false);

        // Fog Parameters
        [Tooltip("Density of fog in the scene.")]
        public MinFloatParameter fogDensity = new MinFloatParameter(10, 0);

        [Tooltip("Sets the directionality of scattered light. <0 indicates backscattering.")]
        public ClampedFloatParameter anisotropy = new ClampedFloatParameter(0.2f, -1, 1);

        [Tooltip("Modifies fog density in lit areas. [Default: 1]")]
        public MinFloatParameter lightIntensity = new MinFloatParameter(1, 0);

        [Tooltip("Modifies fog density in shadowed areas. [Default: 1]")]
        public MinFloatParameter shadowIntensity = new MinFloatParameter(1, 0);

        // Geometry
        [Tooltip("Sets the base of the fog volume. Fog falloff starts after this height.")]
        public FloatParameter baseHeight = new FloatParameter(0);

        [Tooltip("Controls how the fog attenuates over height.")]
        public MinFloatParameter attenuationBoundarySize = new MinFloatParameter(10, 1);

        [Tooltip("Sets the base for the distant fog.")]
        public FloatParameter distantBaseHeight = new FloatParameter(0);

        [Tooltip("Controls how the fog attenuates over height.")]
        public MinFloatParameter distantAttenuationBoundarySize = new MinFloatParameter(100, 1);

        // Custom Colors
        [Tooltip("Used to override the fog color in lit regions.")]
        public ColorParameter litColor = new ColorParameter(Color.white, true, false, false);

        [Tooltip("Used to override the fog color in shadowed regions.")]
        public ColorParameter shadowedColor = new ColorParameter(Color.white, true, false, false);

        [Tooltip("Used to add emissivity to the fog.")]
        public ColorParameter emitColor = new ColorParameter(Color.white, true, false, false);

        [Tooltip("Used to override the fog color over distance. See readme for usage details.")]
        public Texture2DParameter colorRamp = new Texture2DParameter(null);

        [Tooltip("Sets the strength of the override values.")]
        public ClampedFloatParameter colorInfluence = new ClampedFloatParameter(0, 0, 1);

        [Tooltip("Tints the fog when looking towards the main light.")]
        public ColorParameter directionalForward = new ColorParameter(Color.white, true, false, false);

        [Tooltip("Tints the fog when looking away from the main light.")]
        public ColorParameter directionalBack = new ColorParameter(Color.white, true, false, false);

        [Tooltip("Set the falloff between the forward and back directional lighting terms.")]
        public FloatParameter directionalRatio = new FloatParameter(1f);

        // Noise
        [Obsolete("Replaced by the volume noise parameter. Use that instead.")]
        [Tooltip(
            "A 3D Texture that will be used to define the fog intensity. Repeats over the noise tiling domain. A value of 0 means the fog density is attenuated to 0. A value of 1 means the fog density is not attenuated and matches what is set in the Fog Density parameter."
        )]
        public Texture3DParameter noiseTexture = new Texture3DParameter(null);

        [Tooltip("Increases level of detail. Computationally expensive.")]
        public ClampedIntParameter octaves = new ClampedIntParameter(1, 1, 3);

        [Tooltip("Controls frequency for each level of detail.")]
        public ClampedFloatParameter lacunarity = new ClampedFloatParameter(2, 1, 8);

        [Tooltip("Controls intensity of each level of detail.")]
        public ClampedFloatParameter gain = new ClampedFloatParameter(0.3f, 0, 1);

        [Tooltip("Scale of noise texture in meters.")]
        public MinFloatParameter noiseTiling = new MinFloatParameter(30, 0);

        [Tooltip("Controls the wind speed in meters per second.")]
        public Vector3Parameter noiseWindSpeed = new Vector3Parameter(new Vector3(0, -1, 0));

        [Tooltip("Remap the noise to a smaller range.")]
        public FloatRangeParameter noiseMap = new FloatRangeParameter(new Vector2(0, 1), 0, 1);

        public VolumeNoiseParameter volumeNoise = new VolumeNoiseParameter(new VolumeNoise());

        public bool IsActive()
        {
            if (mode.value != VolumetricFogMode.On || fogDensity.value <= 0)
                return false;

            return true;
        }

        public bool IsTileCompatible() => false;
    }

    public enum VolumetricFogMode
    {
        Off,
        On
    }

    [Serializable]
    public sealed class VolumetricFogModeParameter : VolumeParameter<VolumetricFogMode>
    {
        public VolumetricFogModeParameter(VolumetricFogMode value, bool overrideState = false)
            : base(value, overrideState) { }
    }

    public enum QualityLevel
    {
        Low,
        Medium,
        High,
        Ultra,
        Custom
    }

    [Serializable]
    public sealed class QualityLevelParameter : VolumeParameter<QualityLevel>
    {
        public QualityLevelParameter(QualityLevel value, bool overrideState = false)
            : base(value, overrideState) { }
    }

    public enum DepthInteractionMode
    {
        EarlyExit,
        MaximizeSamples
    }

    [Serializable]
    public sealed class DepthInteractionModeParameter : VolumeParameter<DepthInteractionMode>
    {
        public DepthInteractionModeParameter(DepthInteractionMode value, bool overrideState = false)
            : base(value, overrideState) { }
    }

    public enum RayLengthMode
    {
        Constant,
        PreferNearby
    }

    [Serializable]
    public sealed class RayLengthModeParameter : VolumeParameter<RayLengthMode>
    {
        public RayLengthModeParameter(RayLengthMode value, bool overrideState = false)
            : base(value, overrideState) { }
    }
}
