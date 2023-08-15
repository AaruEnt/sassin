using UnityEngine;
using static KWS.WaterSystem;

namespace KWS
{
    public class WaterSystemScriptableData : ScriptableObject
    {
        //Color settings
        public float Transparent = 5;
        public Color WaterColor = new Color(175 / 255.0f, 225 / 255.0f, 240 / 255.0f);
        public Color TurbidityColor = new Color(10 / 255.0f, 110 / 255.0f, 100 / 255.0f);
        public float Turbidity = 0.25f;


        //Waves settings
        public FFT_GPU.SizeSetting FFT_SimulationSize = FFT_GPU.SizeSetting.Size_256;
        public float WindSpeed = 1.5f;
        public float WindRotation = 0;
        public float WindTurbulence = 0.5f;
        public float TimeScale = 1;



        //Reflection settings
        public WaterProfileEnum ReflectionProfile = WaterProfileEnum.High;
        public bool UseScreenSpaceReflection = true;
        public ScreenSpaceReflectionResolutionQualityEnum ScreenSpaceReflectionResolutionQuality = ScreenSpaceReflectionResolutionQualityEnum.High;
        public bool UseScreenSpaceReflectionHolesFilling = true;
        public float ScreenSpaceBordersStretching = 0.015f;

        public bool UsePlanarReflection = false;
        public int PlanarCullingMask = ~0;
        public PlanarReflectionResolutionQualityEnum PlanarReflectionResolutionQuality = PlanarReflectionResolutionQualityEnum.Medium;
        public float ReflectionClipPlaneOffset = 0.0025f;
        public bool RenderPlanarShadows = false;
        public bool RenderPlanarVolumetricsAndFog = false;
        public bool RenderPlanarClouds = false;

        public float CubemapUpdateInterval = 10;
        public int CubemapCullingMask = KWS_Settings.Water.DefaultCubemapCullingMask;
        public int CubemapCullingMaskWithIndoorSkylingReflectionFix = ~0 & ~(1 << KWS_Settings.Water.WaterLayer);
        public CubemapReflectionResolutionQualityEnum CubemapReflectionResolutionQuality = CubemapReflectionResolutionQualityEnum.Medium;
        public bool FixCubemapIndoorSkylightReflection = false;

        public bool UseAnisotropicReflections = true;
        public bool AnisotropicReflectionsHighQuality = false;
        public float AnisotropicReflectionsScale = 0.75f;
        public bool UseAnisotropicCubemapSkyForSSR = false;

        public bool ReflectSun = true;
        public float ReflectedSunCloudinessStrength = 0.04f;
        public float ReflectedSunStrength = 1.0f;



        //Refraction settings
        public WaterProfileEnum RefractionProfile = WaterProfileEnum.High;
        public RefractionModeEnum RefractionMode = RefractionModeEnum.PhysicalAproximationIOR;
        public float RefractionAproximatedDepth = 2f;
        public float RefractionSimpleStrength = 0.25f;
        public bool UseRefractionDispersion = true;
        public float RefractionDispersionStrength = 0.35f;


        //Volumetric settings
        public WaterProfileEnum VolumetricLightProfile = WaterProfileEnum.High;
        public bool UseVolumetricLight = true;
        public VolumetricLightResolutionQualityEnum VolumetricLightResolutionQuality = VolumetricLightResolutionQualityEnum.High;
        public int VolumetricLightIteration = 6;
        public float VolumetricLightBlurRadius = 2.0f;
        public VolumetricLightFilterEnum VolumetricLightFilter = VolumetricLightFilterEnum.Bilateral;



        //FlowMap settings
        public WaterProfileEnum FlowmapProfile = WaterProfileEnum.High;
        public bool UseFlowMap = false;
        public Vector3 FlowMapAreaPosition = new Vector3(0, 0, 0);
        public int FlowMapAreaSize = 200;
        public FlowmapTextureResolutionEnum FlowMapTextureResolution = FlowmapTextureResolutionEnum._2048;
        public float FlowMapSpeed = 1;
        public bool UseFluidsSimulation = false;
        public int FluidsAreaSize = 40;
        public int FluidsSimulationIterrations = 2;
        public int FluidsTextureSize = 1024;
        public int FluidsSimulationFPS = 60;
        public float FluidsSpeed = 1;
        public float FluidsFoamStrength = 0.5f;
        public FlowingScriptableData FlowingScriptableData;


        //Dynamic waves settings
        public WaterProfileEnum DynamicWavesProfile = WaterProfileEnum.High;
        public bool UseDynamicWaves = false;
        public int DynamicWavesAreaSize = 25;
        public int DynamicWavesSimulationFPS = 60;
        public int DynamicWavesResolutionPerMeter = 40;
        public float DynamicWavesPropagationSpeed = 1.0f;
        public bool UseDynamicWavesRainEffect;
        public float DynamicWavesRainStrength = 0.2f;



        //Shoreline settings
        public WaterProfileEnum ShorelineProfile = WaterProfileEnum.High;
        public bool UseShorelineRendering = false;
        public Color ShorelineColor = new Color(0.85f, 0.85f, 0.85f, 0.9f);
        public ShorelineFoamQualityEnum ShorelineFoamLodQuality = ShorelineFoamQualityEnum.High;
        public bool UseShorelineFoamFastMode = false;
        public bool ShorelineFoamReceiveDirShadows = true;
        //public bool FoamReceiveAdditionalLightsShadows = false;
        public ShorelineWavesScriptableData ShorelineWavesScriptableData;

        //Foam Settings
        public WaterProfileEnum FoamProfile = WaterProfileEnum.High;
        public bool UseFoamRendering = false;
        public Color FoamColor = new Color(0.85f, 0.85f, 0.85f, 0.9f);
        public float FoamFadeDistance = 1.5f;
        public float FoamSize = 20;

        //Caustic settings
        public WaterProfileEnum CausticProfile = WaterProfileEnum.High;
        public bool UseCausticEffect = true;
        public bool UseCausticBicubicInterpolation = true;
        public bool UseCausticDispersion = true;
        public int CausticTextureSize = 768;
        public int CausticMeshResolution = 320;
        public int CausticActiveLods = 3;
        public float CausticStrength = 1;
        public bool UseDepthCausticScale = false;
        public float CausticDepthScale = 1;
        public Vector3 CausticOrthoDepthPosition = Vector3.positiveInfinity;
        public int CausticOrthoDepthAreaSize = 512;
        public int CausticOrthoDepthTextureResolution = 2048;



        //Underwater settings
        public WaterProfileEnum UnderwaterProfile = WaterProfileEnum.High;
        public bool UseUnderwaterEffect = true;
        public bool UseUnderwaterBlur = false;
        public float UnderwaterBlurRadius = 2.6f;
        public UnderwaterQueueEnum UnderwaterQueue = UnderwaterQueueEnum.AfterTransparent;


        //Mesh settings
        public WaterProfileEnum MeshProfile = WaterProfileEnum.High;
        public WaterMeshTypeEnum WaterMeshType;
        public int OceanDetailingFarDistance = 5000;
        public float RiverSplineNormalOffset = 1;
        public int RiverSplineVertexCountBetweenPoints = 20;
        public int RiverSplineDepth = 10;
        public Mesh CustomMesh;
        public WaterMeshQualityEnum WaterMeshQualityInfinite = WaterMeshQualityEnum.High;
        public WaterMeshQualityEnum WaterMeshQualityFinite = WaterMeshQualityEnum.High;
        public Vector3 MeshSize = new Vector3(10, 10, 10);
        public bool UseTesselation = false;
        public float TesselationFactor = 0.6f;
        public float TesselationInfiniteMeshMaxDistance = 2000f;
        public float TesselationOtherMeshMaxDistance = 100f;
        public SplineScriptableData SplineScriptableData;


        //Rendering settings
        public WaterProfileEnum RenderingProfile = WaterProfileEnum.High;
        public bool EnabledMeshRendering = true;
        public bool UseFiltering = true;
        public bool UseAnisotropicFiltering = false;
        public bool DrawToPosteffectsDepth;
        public bool WireframeMode;

    }
}