using UnityEngine;

namespace KWS
{
    public static class KWS_ShaderConstants
    {
        #region Shader names

        public static class ShaderNames
        {
            public static readonly string WaterShaderName = "KriptoFX/KWS/Water";
            public static readonly string MaskDepthNormalShaderName = "Hidden/KriptoFX/KWS/MaskDepthNormal";
            public static readonly string VolumetricLightingShaderName = "Hidden/KriptoFX/KWS/VolumetricLighting";
            public static readonly string BlurBilateralName = "Hidden/KriptoFX/KWS/BlurBilateral";
            public static readonly string ReflectionFiltering = "Hidden/KriptoFX/KWS/AnisotropicFiltering";
            public static readonly string CausticComputeShaderName = "Hidden/KriptoFX/KWS/Caustic_Pass";
            public static readonly string CausticDecalShaderName = "Hidden/KriptoFX/KWS/CausticDecal";
            public static readonly string FlowMapShaderName = "Hidden/KriptoFX/KWS/FlowMapEdit";
            public static readonly string ShorelineFoamParticles_shaderName = "Hidden/KriptoFX/KWS/FoamParticles";
            public static readonly string ShorelineFoamParticlesShadow_shaderName = "Hidden/KriptoFX/KWS/FoamParticlesShadow";
            public static readonly string UnderwaterShaderName = "Hidden/KriptoFX/KWS/Underwater";
            public static readonly string CopyColorShaderName = "Hidden/KriptoFX/KWS/CopyColorTexture";
            public static readonly string DrawToDepthShaderName = "Hidden/KriptoFX/KWS/DrawToDepth";
            public static readonly string CopyDepthShaderName = "Hidden/KriptoFX/KWS/CopyDepthTexture";
            public static readonly string CombineSeparatedTexturesToArrayVR = "Hidden/KriptoFX/KWS/KWS_CombineSeparatedTexturesToArrayVR";
            public static readonly string ShorelineBakedWavesShaderName = "Hidden/KriptoFX/KWS/ShorelineWaves";
            public static readonly string ShorelineFoamDrawToScreenName = "Hidden/KriptoFX/KWS/ShorelineFoamDrawToScreen";
            public static readonly string InfiniteOceanShadowCullingAnchorName = "Hidden/KriptoFX/KWS/ShadowFix";
        }

        #endregion

        #region ShaderID

        public static class CameraMatrix
        {
            public static readonly int KWS_MATRIX_VP = Shader.PropertyToID("KWS_MATRIX_VP");
            public static readonly int KWS_PREV_MATRIX_VP = Shader.PropertyToID("KWS_PREV_MATRIX_VP");
            public static readonly int KWS_MATRIX_I_VP = Shader.PropertyToID("KWS_MATRIX_I_VP");
            public static readonly int KWS_MATRIX_CAMERA_PROJECTION_STEREO = Shader.PropertyToID("KWS_MATRIX_CAMERA_PROJECTION_STEREO");
            public static readonly int KWS_MATRIX_VP_STEREO = Shader.PropertyToID("KWS_MATRIX_VP_STEREO");
            public static readonly int KWS_PREV_MATRIX_VP_STEREO = Shader.PropertyToID("KWS_PREV_MATRIX_VP_STEREO");
            public static readonly int KWS_MATRIX_I_VP_STEREO = Shader.PropertyToID("KWS_MATRIX_I_VP_STEREO");
            public static readonly int KWS_MATRIX_CAMERA_PROJECTION = Shader.PropertyToID("KWS_MATRIX_CAMERA_PROJECTION");

            public static readonly int KW_ViewToWorld = Shader.PropertyToID("KW_ViewToWorld");
            public static readonly int KW_ProjToView = Shader.PropertyToID("KW_ProjToView");


        }

        public enum WaterMaterialPasses
        {
            QuadTree = 0,
            QuadTree_Tesselated = 1,
            CustomOrRiver = 2,
            CustomOrRiver_Tesselated = 3,
            WaterSide = 4,
            MaskDepthNormal_QuadTree = 5,
            MaskDepthNormal_QuadTreeTesselated = 6,
            MaskDepthNormal_CustomOrRiver = 7,
            MaskDepthNormal_CustomOrRiverTesselated = 8,
            MaskDepthNormal_Side = 9,
        }

        public static class ConstantWaterParams
        {
            public static readonly int KW_Transparent = Shader.PropertyToID("KW_Transparent");
            public static readonly int KW_WaterColor = Shader.PropertyToID("KW_WaterColor");
            public static readonly int KW_Turbidity = Shader.PropertyToID("KW_Turbidity");
            public static readonly int KW_TurbidityColor = Shader.PropertyToID("KW_TurbidityColor");

            public static readonly int KW_FFT_Size_Normalized = Shader.PropertyToID("KW_FFT_Size_Normalized");
            public static readonly int KW_WindSpeed = Shader.PropertyToID("KW_WindSpeed");
            public static readonly int KWS_WindRotation = Shader.PropertyToID("KWS_WindRotation");
            public static readonly int KWS_WindTurbulence = Shader.PropertyToID("KWS_WindTurbulence");
            public static readonly int KW_GlobalTimeScale = Shader.PropertyToID("KW_GlobalTimeScale");

            public static readonly int KWS_ScreenSpaceBordersStretching = Shader.PropertyToID("KWS_ScreenSpaceBordersStretching");
            public static readonly int KWS_IgnoreAnisotropicScreenSpaceSky = Shader.PropertyToID("KWS_IgnoreAnisotropicScreenSpaceSky");
            public static readonly int KWS_AnisoReflectionsScale = Shader.PropertyToID("KWS_AnisoReflectionsScale");
            public static readonly int KW_ReflectionClipOffset = Shader.PropertyToID("KWS_ReflectionClipOffset");
            public static readonly int KWS_SunCloudiness = Shader.PropertyToID("KWS_SunCloudiness");
            public static readonly int KWS_SunStrength = Shader.PropertyToID("KWS_SunStrength");
            public static readonly int KWS_SunMaxValue = Shader.PropertyToID("KWS_SunMaxValue");

            public static readonly int KWS_RefractionAproximatedDepth = Shader.PropertyToID("KWS_RefractionAproximatedDepth");
            public static readonly int KWS_RefractionSimpleStrength = Shader.PropertyToID("KWS_RefractionSimpleStrength");
            public static readonly int KWS_RefractionDispersionStrength = Shader.PropertyToID("KWS_RefractionDispersionStrength");

            public static readonly int KW_FlowMapOffset = Shader.PropertyToID("KW_FlowMapOffset");
            public static readonly int KW_FlowMapSize = Shader.PropertyToID("KW_FlowMapSize");
            public static readonly int KW_FlowMapSpeed = Shader.PropertyToID("KW_FlowMapSpeed");
            public static readonly int KW_FlowMapFluidsStrength = Shader.PropertyToID("KW_FlowMapFluidsStrength");

            public static readonly int _TesselationMaxDisplace = Shader.PropertyToID("_TesselationMaxDisplace");
            public static readonly int _TesselationFactor = Shader.PropertyToID("_TesselationFactor");
            public static readonly int _TesselationMaxDistance = Shader.PropertyToID("_TesselationMaxDistance");
            public static readonly int KWS_IsCustomMesh = Shader.PropertyToID("KWS_IsCustomMesh");
            public static readonly int KWS_InstancingRotationMatrix = Shader.PropertyToID("KWS_InstancingRotationMatrix");
            public static readonly int KWS_InstancingWaterScale = Shader.PropertyToID("KWS_InstancingWaterScale");
            public static readonly int KWS_IsFiniteTypeInstancing = Shader.PropertyToID("KWS_IsFiniteTypeInstancing");
            public static readonly int KW_WaterFarDistance = Shader.PropertyToID("KW_WaterFarDistance");

            public static readonly int KWS_FoamFadeSize = Shader.PropertyToID("KWS_FoamFadeSize");
            public static readonly int KWS_FoamColor = Shader.PropertyToID("KWS_FoamColor");

            public static readonly int KWS_UseWireframeMode = Shader.PropertyToID("KWS_UseWireframeMode");
            public static readonly int KWS_UseMultipleSimulations = Shader.PropertyToID("KWS_UseMultipleSimulations");
            public static readonly int KWS_UseRefractionIOR = Shader.PropertyToID("KWS_UseRefractionIOR");
            public static readonly int KWS_UseRefractionDispersion = Shader.PropertyToID("KWS_UseRefractionDispersion");
        }

        public static class DynamicWaterParams
        {
            public static readonly int KW_WaterPosition = Shader.PropertyToID("KW_WaterPosition");
            public static readonly int KW_Time = Shader.PropertyToID("KW_Time");
            public static readonly int KWS_AmbientColor = Shader.PropertyToID("KWS_AmbientColor");
            public static readonly int KWS_WaterPassID = Shader.PropertyToID("KWS_WaterPassID");
            public static readonly int KWS_CameraForward = Shader.PropertyToID("KWS_CameraForward");
            public static readonly int KWS_UnderwaterVisible = Shader.PropertyToID("KWS_UnderwaterVisible");
            public static readonly int KWS_FogState = Shader.PropertyToID("KWS_FogState");
        }

        public static class WaterID
        {
            // public static readonly int KW_Time = Shader.PropertyToID("KW_Time");


            //public static readonly int KW_Transparent = Shader.PropertyToID("KW_Transparent");
            //public static readonly int KW_Turbidity = Shader.PropertyToID("KW_Turbidity");
            //public static readonly int KW_TurbidityColor = Shader.PropertyToID("KW_TurbidityColor");
            // public static readonly int KW_WaterColor = Shader.PropertyToID("KW_WaterColor");

            //public static readonly int KW_FFT_Size_Normalized = Shader.PropertyToID("KW_FFT_Size_Normalized");
            //public static readonly int KW_WindSpeed = Shader.PropertyToID("KW_WindSpeed");
            //public static readonly int KWS_WindRotation = Shader.PropertyToID("KWS_WindRotation");
            //public static readonly int KWS_WindTurbulence = Shader.PropertyToID("KWS_WindTurbulence");
            //public static readonly int KW_GlobalTimeScale = Shader.PropertyToID("KW_GlobalTimeScale");

            //public static readonly int _TesselationMaxDisplace = Shader.PropertyToID("_TesselationMaxDisplace");
            //public static readonly int _TesselationFactor = Shader.PropertyToID("_TesselationFactor");
            //public static readonly int _TesselationMaxDistance = Shader.PropertyToID("_TesselationMaxDistance");

            // public static readonly int KW_WaterPosition = Shader.PropertyToID("KW_WaterPosition");
            //public static readonly int KW_ViewToWorld = Shader.PropertyToID("KW_ViewToWorld");
            // public static readonly int KW_ProjToView = Shader.PropertyToID("KW_ProjToView");
            // public static readonly int KWS_CameraProjectionMatrix = Shader.PropertyToID("KWS_CameraProjectionMatrix");

            //public static readonly int KW_WaterFarDistance = Shader.PropertyToID("KW_WaterFarDistance");

            //public static readonly int KW_FlowMapSize = Shader.PropertyToID("KW_FlowMapSize");
            //public static readonly int KW_FlowMapOffset = Shader.PropertyToID("KW_FlowMapOffset");
            //public static readonly int KW_FlowMapSpeed = Shader.PropertyToID("KW_FlowMapSpeed");
            //public static readonly int KW_FlowMapFluidsStrength = Shader.PropertyToID("KW_FlowMapFluidsStrength");

            //public static readonly int KW_ReflectionClipOffset = Shader.PropertyToID("KWS_ReflectionClipOffset");
            //public static readonly int KWS_SunCloudiness = Shader.PropertyToID("KWS_SunCloudiness");
            //public static readonly int KWS_SunStrength = Shader.PropertyToID("KWS_SunStrength");
            //public static readonly int KWS_SunMaxValue = Shader.PropertyToID("KWS_SunMaxValue");
            //public static readonly int KWS_RefractionAproximatedDepth = Shader.PropertyToID("KWS_RefractionAproximatedDepth");
            //public static readonly int KWS_RefractionSimpleStrength = Shader.PropertyToID("KWS_RefractionSimpleStrength");
            //public static readonly int KWS_RefractionDispersionStrength = Shader.PropertyToID("KWS_RefractionDispersionStrength");

            //public static readonly int KWS_AmbientColor = Shader.PropertyToID("KWS_AmbientColor");
            //public static readonly int KWS_IsCustomMesh = Shader.PropertyToID("KWS_IsCustomMesh");

            //public static readonly int KWS_InstancingRotationMatrix = Shader.PropertyToID("KWS_InstancingRotationMatrix");
            //public static readonly int KWS_InstancingWaterScale = Shader.PropertyToID("KWS_InstancingWaterScale");
            //public static readonly int KWS_IsFiniteTypeInstancing = Shader.PropertyToID("KWS_IsFiniteTypeInstancing");
        }

        public static class FFT
        {
            public static readonly int[] KW_DispTex =
            {
                Shader.PropertyToID("KW_DispTex"),
                Shader.PropertyToID("KW_DispTex_LOD1"),
                Shader.PropertyToID("KW_DispTex_LOD2")
            };

            public static readonly int[] KW_NormTex =
            {
                Shader.PropertyToID("KW_NormTex"),
                Shader.PropertyToID("KW_NormTex_LOD1"),
                Shader.PropertyToID("KW_NormTex_LOD2")
            };

            public static readonly int[] KW_FFTDomainSize =
            {
                Shader.PropertyToID("KW_FFTDomainSize"),
                Shader.PropertyToID("KW_FFTDomainSize_LOD1"),
                Shader.PropertyToID("KW_FFTDomainSize_LOD2")
            };
        }

        public static class OrthoDepth
        {
            public static readonly int KWS_OrthoDepthPos = Shader.PropertyToID("KWS_OrthoDepthPos");
            public static readonly int KWS_OrthoDepthNearFarSize = Shader.PropertyToID("KWS_OrthoDepthNearFarSize");
            public static readonly int KWS_WaterOrthoDepthRT = Shader.PropertyToID("KWS_WaterOrthoDepthRT");
        }

        public static class MaskPassID
        {
            public static readonly int KW_WaterMaskScatterNormals_ID = Shader.PropertyToID("KW_WaterMaskScatterNormals");
            public static readonly int KW_WaterDepth_ID = Shader.PropertyToID("KW_WaterDepth");
            public static readonly int KWS_WaterTexID = Shader.PropertyToID("KWS_WaterTexID");
            public static readonly int KW_WaterMaskScatterNormals_Blured_ID = Shader.PropertyToID("KW_WaterMaskScatterNormals_Blured");
            public static readonly int KWS_WaterMask_RTHandleScale = Shader.PropertyToID("KWS_WaterMask_RTHandleScale");
        }

        public static class ReflectionsID
        {
            //public static readonly int KWS_ScreenSpaceBordersStretching = Shader.PropertyToID("KWS_ScreenSpaceBordersStretching");
            //public static readonly int KWS_IgnoreAnisotropicScreenSpaceSky = Shader.PropertyToID("KWS_IgnoreAnisotropicScreenSpaceSky");
            //public static readonly int KWS_AnisoReflectionsScale = Shader.PropertyToID("KWS_AnisoReflectionsScale");
            //public static readonly int KWS_NormalizedWind = Shader.PropertyToID("KWS_NormalizedWind");
            public static readonly int KWS_PlanarReflectionRT = Shader.PropertyToID("KWS_PlanarReflectionRT");
            public static readonly int KWS_CubemapReflectionRT = Shader.PropertyToID("KWS_CubemapReflectionRT");
            public static readonly int KWS_IsCubemapReflectionPlanar = Shader.PropertyToID("KWS_IsCubemapReflectionPlanar");
            public static readonly int KWS_IsCubemapSide = Shader.PropertyToID("KWS_IsCubemapSide");
            public static readonly int KWS_ReprojectedFrameReady = Shader.PropertyToID("KWS_ReprojectedFrameReady");
        }

        public static class RefractionID
        {

        }

        public static class SSR_ID
        {
            public static readonly int unity_StereoEyeIndex = Shader.PropertyToID("unity_StereoEyeIndex");
            public static readonly int _RTSize = Shader.PropertyToID("_RTSize");
            public static readonly int _HorizontalPlaneHeightWS = Shader.PropertyToID("_HorizontalPlaneHeightWS");
            public static readonly int _DepthHolesFillDistance = Shader.PropertyToID("_DepthHolesFillDistance");

            public static readonly int HashRT = Shader.PropertyToID("HashRT");
            public static readonly int ColorRT = Shader.PropertyToID("ColorRT");
            public static readonly int _CameraDepthTexture = Shader.PropertyToID("_CameraDepthTexture");
            public static readonly int _CameraOpaqueTexture = Shader.PropertyToID("_CameraOpaqueTexture");
            public static readonly int KWS_ScreenSpaceReflectionRT = Shader.PropertyToID("KWS_ScreenSpaceReflectionRT");
            public static readonly int KWS_ScreenSpaceReflection_RTHandleScale = Shader.PropertyToID("KWS_ScreenSpaceReflection_RTHandleScale");
            public static readonly int KWS_LastTargetRT = Shader.PropertyToID("KWS_LastTargetRT");
            public static readonly int KWS_UseSSRHolesFilling = Shader.PropertyToID("KWS_UseSSRHolesFilling");
        }

        public static class VolumetricLightConstantsID
        {
            public static readonly int KWS_CameraDepthTextureLowRes = Shader.PropertyToID("KWS_CameraDepthTextureLowRes");
            public static readonly int KWS_VolumetricLightRT = Shader.PropertyToID("KWS_VolumetricLightRT");

            public static readonly int KWS_Transparent = Shader.PropertyToID("KWS_Transparent");
            public static readonly int KWS_LightAnisotropy = Shader.PropertyToID("KWS_LightAnisotropy");
            public static readonly int KWS_VolumeDepthFade = Shader.PropertyToID("KWS_VolumeDepthFade");
            public static readonly int KWS_VolumeLightMaxDistance = Shader.PropertyToID("KWS_VolumeLightMaxDistance");
            public static readonly int KWS_RayMarchSteps = Shader.PropertyToID("KWS_RayMarchSteps");
            public static readonly int KWS_VolumeLightBlurRadius = Shader.PropertyToID("KWS_VolumeLightBlurRadius");
            public static readonly int KWS_VolumeTexSceenSize = Shader.PropertyToID("KWS_VolumeTexSceenSize");
            public static readonly int KWS_VolumetricLight_RTHandleScale = Shader.PropertyToID("KWS_VolumetricLight_RTHandleScale");
            public static readonly int KWS_WorldSpaceCameraPosCompillerFixed = Shader.PropertyToID("KWS_WorldSpaceCameraPosCompillerFixed");

        }
        public static class CausticID
        {
            public static readonly int KW_CaustisStrength = Shader.PropertyToID("KW_CaustisStrength");
            public static readonly int KW_CausticDispersionStrength = Shader.PropertyToID("KW_CausticDispersionStrength");
            public static readonly int KW_MeshScale = Shader.PropertyToID("KW_MeshScale");
            public static readonly int KW_CausticDepthScale = Shader.PropertyToID("KW_CausticDepthScale");
            public static readonly int KW_CausticCameraOffset = Shader.PropertyToID("KW_CausticCameraOffset");
            
            public static readonly int[] KW_CausticLod =
            {
                Shader.PropertyToID("KW_CausticLod0"),
                Shader.PropertyToID("KW_CausticLod1"),
                Shader.PropertyToID("KW_CausticLod2"),
                Shader.PropertyToID("KW_CausticLod3")
            };

            public static readonly int KW_CausticLodSettings = Shader.PropertyToID("KW_CausticLodSettings");
            public static readonly int KW_CausticLodOffset = Shader.PropertyToID("KW_CausticLodOffset");
            public static readonly int KW_CausticLodPosition = Shader.PropertyToID("KW_CausticLodPosition");
            public static readonly int KW_DecalScale = Shader.PropertyToID("KW_DecalScale");


            public static readonly int KW_CausticDepthTex = Shader.PropertyToID("KW_CausticDepthTex");
            public static readonly int KW_CausticDepthOrthoSize = Shader.PropertyToID("KW_CausticDepthOrthoSize");
            public static readonly int KW_CausticDepth_Near_Far_Dist = Shader.PropertyToID("KW_CausticDepth_Near_Far_Dist");
            public static readonly int KW_CausticDepthPos = Shader.PropertyToID("KW_CausticDepthPos");
        }

        public static class FlowmapID
        {
            public static readonly int KW_FlowMapTex = Shader.PropertyToID("KW_FlowMapTex");

            public static readonly int KW_FluidsVelocityAreaScale = Shader.PropertyToID("KW_FluidsVelocityAreaScale");
            public static readonly int KW_FluidsMapWorldPosition_lod0 = Shader.PropertyToID("KW_FluidsMapWorldPosition_lod0");
            public static readonly int KW_FluidsMapWorldPosition_lod1 = Shader.PropertyToID("KW_FluidsMapWorldPosition_lod1");
            public static readonly int KW_FluidsMapAreaSize_lod0 = Shader.PropertyToID("KW_FluidsMapAreaSize_lod0");
            public static readonly int KW_FluidsMapAreaSize_lod1 = Shader.PropertyToID("KW_FluidsMapAreaSize_lod1");
            public static readonly int KW_Fluids_Lod0 = Shader.PropertyToID("KW_Fluids_Lod0");
            public static readonly int KW_FluidsFoam_Lod0 = Shader.PropertyToID("KW_FluidsFoam_Lod0");
            public static readonly int KW_Fluids_Lod1 = Shader.PropertyToID("KW_Fluids_Lod1");
            public static readonly int KW_FluidsFoam_Lod1 = Shader.PropertyToID("KW_FluidsFoam_Lod1");
            public static readonly int KW_FluidsRequiredReadPrebakedSim = Shader.PropertyToID("KW_FluidsRequiredReadPrebakedSim");
        }

        public static class DynamicWaves
        {
            public static readonly int KW_DynamicWavesWorldPos = Shader.PropertyToID("KW_DynamicWavesWorldPos");
            public static readonly int KW_DynamicWavesAreaSize = Shader.PropertyToID("KW_DynamicWavesAreaSize");
            public static readonly int KW_DynamicObjectsMap = Shader.PropertyToID("KW_DynamicObjectsMap");
            public static readonly int KW_InteractiveWavesPixelSpeed = Shader.PropertyToID("KW_InteractiveWavesPixelSpeed");
            public static readonly int _PrevTex = Shader.PropertyToID("_PrevTex");
            public static readonly int KW_AreaOffset = Shader.PropertyToID("KW_AreaOffset");
            public static readonly int KW_LastAreaOffset = Shader.PropertyToID("KW_LastAreaOffset");
            public static readonly int KW_InteractiveWavesAreaSize = Shader.PropertyToID("KW_InteractiveWavesAreaSize");
            public static readonly int KW_DynamicWaves = Shader.PropertyToID("KW_DynamicWaves");
            public static readonly int KW_DynamicWavesNormal = Shader.PropertyToID("KW_DynamicWavesNormal");

        }

        public static class ShorelineID
        {
            public static readonly int KWS_ShorelineWavesDisplacement = Shader.PropertyToID("KWS_ShorelineWavesDisplacement");
            public static readonly int KWS_ShorelineWavesNormal = Shader.PropertyToID("KWS_ShorelineWavesNormal");
            public static readonly int KWS_ShorelineAreaPosSize = Shader.PropertyToID("KWS_ShorelineAreaPosSize");
            public static readonly int KWS_ShorelineAreaWavesCount = Shader.PropertyToID("KWS_ShorelineAreaWavesCount");

        }

        public static class UnderwaterID
        {
            public static readonly int KWS_TargetResolutionMultiplier = Shader.PropertyToID("KWS_TargetResolutionMultiplier");
            public static readonly int KWS_UnderwaterRT_Blured = Shader.PropertyToID("KWS_UnderwaterRT_Blured");
            public static readonly int KWS_Underwater_RTHandleScale = Shader.PropertyToID("KWS_Underwater_RTHandleScale");
        }

        public static class DrawToDepthID
        {

        }

        #endregion

        #region Shader Keywords

        public static class WaterKeywords
        {
            public static readonly string STEREO_INSTANCING_ON = "STEREO_INSTANCING_ON";
            public static readonly string USE_WATER_INSTANCING = "USE_WATER_INSTANCING";
            public static readonly string USE_MULTIPLE_SIMULATIONS = "USE_MULTIPLE_SIMULATIONS";
            public static readonly string KW_FLOW_MAP_EDIT_MODE = "KW_FLOW_MAP_EDIT_MODE";
            public static readonly string KW_FLOW_MAP = "KW_FLOW_MAP";
            public static readonly string KW_FLOW_MAP_FLUIDS = "KW_FLOW_MAP_FLUIDS";
            public static readonly string KW_DYNAMIC_WAVES = "KW_DYNAMIC_WAVES";
            public static readonly string USE_CAUSTIC = "USE_CAUSTIC";
            public static readonly string REFLECT_SUN = "REFLECT_SUN";
            public static readonly string USE_VOLUMETRIC_LIGHT = "USE_VOLUMETRIC_LIGHT";
            public static readonly string USE_FILTERING = "USE_FILTERING";
            public static readonly string USE_SHORELINE = "USE_SHORELINE";
            public static readonly string PLANAR_REFLECTION = "PLANAR_REFLECTION";
            public static readonly string SSPR_REFLECTION = "SSPR_REFLECTION";
            public static readonly string USE_HOLES_FILLING = "USE_HOLES_FILLING";
            public static readonly string USE_REFRACTION_DISPERSION = "USE_REFRACTION_DISPERSION";
            public static readonly string USE_REFRACTION_IOR = "USE_REFRACTION_IOR";
            public static readonly string USE_FOAM = "USE_FOAM";
        }

        public static class CausticKeywords
        {
            public static readonly string USE_DEPTH_SCALE = "USE_DEPTH_SCALE";
            public static readonly string USE_DISPERSION = "USE_DISPERSION";
            public static readonly string USE_LOD1 = "USE_LOD1";
            public static readonly string USE_LOD2 = "USE_LOD2";
            public static readonly string USE_LOD3 = "USE_LOD3";
            public static readonly string USE_CAUSTIC_FILTERING = "USE_CAUSTIC_FILTERING";
        }

        public static class ShorelineKeywords
        {
            public static readonly string FOAM_RECEIVE_SHADOWS = "FOAM_RECEIVE_SHADOWS";
            public static readonly string FOAM_COMPUTE_WATER_OFFSET = "FOAM_COMPUTE_WATER_OFFSET";
            public static readonly string KWS_FOAM_USE_FAST_PATH = "KWS_FOAM_USE_FAST_MODE";
        }

        public static class SSRKeywords
        {
            public static readonly string USE_HOLES_FILLING = "USE_HOLES_FILLING";
        }

        public static class FogKeywords
        {
            public static readonly string FOG_LINEAR = "FOG_LINEAR";
            public static readonly string FOG_EXP = "FOG_EXP";
            public static readonly string FOG_EXP2 = "FOG_EXP2";
        }

        #endregion

        #region Compute kernels

        public static class SSR_Kernels
        {
            public static readonly string Clear_kernel = "Clear_kernel";
            public static readonly string RenderHash_kernel = "RenderHash_kernel";
            public static readonly string RenderColorFromHash_kernel = "RenderColorFromHash_kernel";
            public static readonly string RenderSinglePassSSR_kernel = "RenderSinglePassSSR_kernel";
            public static readonly string FillHoles_kernel = "FillHoles_kernel";
            public static readonly string TemporalFiltering_kernel = "TemporalFiltering_kernel";
        }

        #endregion

        #region StructuredBuffers

        public static class StructuredBuffers
        {
            public static readonly int InstancedMeshData = Shader.PropertyToID("InstancedMeshData");
            public static readonly int KWS_ShorelineDataBuffer = Shader.PropertyToID("KWS_ShorelineDataBuffer");
        }

        #endregion
    }
}

