using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace KWS
{
    public class SharedData
    {

        //native data require manual release
        internal ComputeBuffer WaterConstantBuffer;

        internal int WaterShaderPassID;


        /////////////////// FFT data ////////////////////////////////////////
        internal int FFTActiveLods;
        internal float[] DomainSizes = new float[3];
        internal RenderTexture[] FftDisplacements = new RenderTexture[3];

        internal RenderTexture[] FftNormals = new RenderTexture[3];
        //////////////////////////////////////////////////////////////////////


        /////////////////// ortho depth //////////////////////////////////////
        internal RenderTexture OrthoDepth;
        internal Vector4 OrthoDepthPosition;

        internal Vector4 OrthoDepthNearFarSize;
        //////////////////////////////////////////////////////////////////////



        /////////////////// volumetric light ///////////////////////////
        internal RTHandle VolumetricLightingRT;
        ////////////////////////////////////////////////////////////////


        /////////////////// reflection ///////////////////////////
        internal RenderTexture PlanarReflectionRaw;
        internal RenderTexture PlanarReflectionFinal;
        internal RenderTexture CubemapReflection;

        internal RTHandle SsrReflection;
        ////////////////////////////////////////////////////////////////

        /////////////////// caustic /////////////////////////////////
        internal int CausticActiveLods;
        internal RenderTexture[] CausticLods = new RenderTexture[4];
        ////////////////////////////////////////////////////////////////



        /////////////////// flowing /////////////////////////////////
        internal float FluidsAreaSize;
        internal float FluidsSpeed;
        internal Texture Flowmap;

        internal Vector3 FluidsAreaPositionLod0;
        internal Vector3 FluidsAreaPositionLod1;

        internal RenderTexture[] FluidsRT = new RenderTexture[2];
        internal RenderTexture[] FluidsFoamRT = new RenderTexture[2];

        ////////////////////////////////////////////////////////////////

        /////////////////// Dynamic Waves /////////////////////////////////
        internal Vector3 DynamicWavesAreaPos;
        internal float DynamicWavesAreaSize;
        internal RenderTexture DynamicWaves;

        internal RenderTexture DynamicWavesNormal;
        ////////////////////////////////////////////////////////////////

        /////////////////// Shoreline /////////////////////////////////
        internal KW_ShorelineWaves.WaveBuffers ShorelineWaveBuffers;
        internal Vector4 ShorelineAreaPosSize;
        internal RenderTexture ShorelineWavesDisplacement;
        internal RenderTexture ShorelineWavesNormal;
        internal RTHandle FoamRawRT;
        ////////////////////////////////////////////////////////////////

        public enum PassEnum
        {
            FFT,
            SSR,
            PlanarReflection,
            CubemapReflection,
            Flowing,
            FluidsSimulation,
            DynamicWaves,
            Shoreline,
            VolumetricLighting,
            Caustic
        }

        public void UpdateShaderParams(CommandBuffer cmd, PassEnum pass)
        {
            switch (pass)
            {
                case PassEnum.FFT:
                    for (int i = 0; i < FFTActiveLods; i++)
                    {
                        cmd.SetGlobalTexture(KWS_ShaderConstants.FFT.KW_DispTex[i], FftDisplacements[i].GetSafeTexture());
                        cmd.SetGlobalTexture(KWS_ShaderConstants.FFT.KW_NormTex[i], FftNormals[i].GetSafeTexture());
                        cmd.SetGlobalFloat(KWS_ShaderConstants.FFT.KW_FFTDomainSize[i], DomainSizes[i]);
                    }

                    break;

                case PassEnum.SSR:
                    var rtScale = SsrReflection?.rtHandleProperties.rtHandleScale ?? Vector4.one;
                    cmd.SetGlobalVector(KWS_ShaderConstants.SSR_ID.KWS_ScreenSpaceReflection_RTHandleScale, rtScale);
                    cmd.SetGlobalTexture(KWS_ShaderConstants.SSR_ID.KWS_ScreenSpaceReflectionRT, SsrReflection.GetSafeTexture());
                    break;

                case PassEnum.PlanarReflection:
                    cmd.SetGlobalTexture(KWS_ShaderConstants.ReflectionsID.KWS_PlanarReflectionRT,  PlanarReflectionFinal.GetSafeTexture());
                    break;

                case PassEnum.CubemapReflection:
                    cmd.SetGlobalTexture(KWS_ShaderConstants.ReflectionsID.KWS_CubemapReflectionRT, CubemapReflection.GetSafeTexture());
                    break;

                case PassEnum.Caustic:
                    for (var i = 0; i < CausticActiveLods; i++)
                    {
                        cmd.SetGlobalTexture(KWS_ShaderConstants.CausticID.KW_CausticLod[i], CausticLods[i].GetSafeTexture());
                    }
                    break;

                case PassEnum.VolumetricLighting:
                    var rtSize = VolumetricLightingRT != null ? VolumetricLightingRT.rtHandleProperties.rtHandleScale : Vector4.one;
                    cmd.SetGlobalVector(KWS_ShaderConstants.VolumetricLightConstantsID.KWS_VolumetricLight_RTHandleScale, rtSize);
                    cmd.SetGlobalTexture(KWS_ShaderConstants.VolumetricLightConstantsID.KWS_VolumetricLightRT, VolumetricLightingRT.GetSafeTexture());
                    break;

                case PassEnum.Flowing:
                    cmd.SetGlobalTexture(KWS_ShaderConstants.FlowmapID.KW_FlowMapTex, Flowmap.GetSafeTexture(Color.gray));
                    break;

                case PassEnum.FluidsSimulation:
                    cmd.SetGlobalFloat(KWS_ShaderConstants.FlowmapID.KW_FluidsVelocityAreaScale, (0.5f * FluidsAreaSize / 40f) * FluidsSpeed);
                    cmd.SetGlobalFloat(KWS_ShaderConstants.FlowmapID.KW_FluidsMapAreaSize_lod0, FluidsAreaSize);
                    cmd.SetGlobalFloat(KWS_ShaderConstants.FlowmapID.KW_FluidsMapAreaSize_lod1, FluidsAreaSize * KWS_Settings.Flowing.AreaSizeMultiplierLod1);
                    cmd.SetGlobalVector(KWS_ShaderConstants.FlowmapID.KW_FluidsMapWorldPosition_lod0, FluidsAreaPositionLod0);
                    cmd.SetGlobalVector(KWS_ShaderConstants.FlowmapID.KW_FluidsMapWorldPosition_lod1, FluidsAreaPositionLod1);
                    cmd.SetGlobalTexture(KWS_ShaderConstants.FlowmapID.KW_Fluids_Lod0, FluidsRT[0].GetSafeTexture());
                    cmd.SetGlobalTexture(KWS_ShaderConstants.FlowmapID.KW_Fluids_Lod1, FluidsRT[1].GetSafeTexture());
                    cmd.SetGlobalTexture(KWS_ShaderConstants.FlowmapID.KW_FluidsFoam_Lod0, FluidsFoamRT[0].GetSafeTexture());
                    cmd.SetGlobalTexture(KWS_ShaderConstants.FlowmapID.KW_FluidsFoam_Lod1, FluidsFoamRT[1].GetSafeTexture());
                    break;

                case PassEnum.DynamicWaves:
                    cmd.SetGlobalFloat(KWS_ShaderConstants.DynamicWaves.KW_InteractiveWavesAreaSize, DynamicWavesAreaSize);
                    cmd.SetGlobalVector(KWS_ShaderConstants.DynamicWaves.KW_DynamicWavesWorldPos, DynamicWavesAreaPos);
                    cmd.SetGlobalTexture(KWS_ShaderConstants.DynamicWaves.KW_DynamicWaves, DynamicWaves.GetSafeTexture());
                    cmd.SetGlobalTexture(KWS_ShaderConstants.DynamicWaves.KW_DynamicWavesNormal, DynamicWavesNormal.GetSafeTexture());
                    break;

                case PassEnum.Shoreline:
                    cmd.SetGlobalInt(KWS_ShaderConstants.ShorelineID.KWS_ShorelineAreaWavesCount, ShorelineWaveBuffers?.VisibleFoamWaves.Count ?? 0);
                    cmd.SetGlobalVector(KWS_ShaderConstants.ShorelineID.KWS_ShorelineAreaPosSize, ShorelineAreaPosSize);
                    cmd.SetGlobalTexture(KWS_ShaderConstants.ShorelineID.KWS_ShorelineWavesDisplacement, ShorelineWavesDisplacement.GetSafeTexture());
                    cmd.SetGlobalTexture(KWS_ShaderConstants.ShorelineID.KWS_ShorelineWavesNormal, ShorelineWavesNormal.GetSafeTexture());
                    break;

               
                default:
                    throw new ArgumentOutOfRangeException(nameof(pass), pass, null);
            }


        }

        public void UpdateShaderParams(CommandBuffer cmd, PassEnum pass, ComputeShader cs, int kernel)
        {
                switch (pass)
                {
                    case PassEnum.FFT:
                        for (int i = 0; i < 3; i++)
                        {
                            cmd.SetComputeTextureParam(cs, kernel, KWS_ShaderConstants.FFT.KW_DispTex[i], FftDisplacements[i].GetSafeTexture());
                            cmd.SetComputeTextureParam(cs, kernel, KWS_ShaderConstants.FFT.KW_NormTex[i], FftNormals[i].GetSafeTexture());
                            cmd.SetComputeFloatParam(cs, KWS_ShaderConstants.FFT.KW_FFTDomainSize[i], DomainSizes[i]);
                        }
                        break;

                    case PassEnum.SSR:
                        var rtScale = SsrReflection?.rtHandleProperties.rtHandleScale ?? Vector4.one;
                        cmd.SetComputeVectorParam(cs, KWS_ShaderConstants.SSR_ID.KWS_ScreenSpaceReflection_RTHandleScale, rtScale);
                        cmd.SetComputeTextureParam(cs, kernel, KWS_ShaderConstants.SSR_ID.KWS_ScreenSpaceReflectionRT, SsrReflection.GetSafeTexture());
                        break;

                    case PassEnum.PlanarReflection:
                        cmd.SetComputeTextureParam(cs, kernel, KWS_ShaderConstants.ReflectionsID.KWS_PlanarReflectionRT, PlanarReflectionFinal.GetSafeTexture());
                        break;

                    case PassEnum.CubemapReflection:
                        cmd.SetComputeTextureParam(cs, kernel, KWS_ShaderConstants.ReflectionsID.KWS_CubemapReflectionRT, CubemapReflection.GetSafeCubeTexture());
                        break;

                    case PassEnum.Caustic:
                        for (var i = 0; i < CausticActiveLods; i++)
                        {
                            cmd.SetComputeTextureParam(cs, kernel, KWS_ShaderConstants.CausticID.KW_CausticLod[i], CausticLods[i].GetSafeTexture());
                        }
                        break;

                    case PassEnum.VolumetricLighting:
                        var rtSize = VolumetricLightingRT != null ? VolumetricLightingRT.rtHandleProperties.rtHandleScale : Vector4.one;
                        cmd.SetComputeVectorParam(cs, KWS_ShaderConstants.VolumetricLightConstantsID.KWS_VolumetricLight_RTHandleScale, rtSize);
                        cmd.SetComputeTextureParam(cs, kernel, KWS_ShaderConstants.VolumetricLightConstantsID.KWS_VolumetricLightRT, VolumetricLightingRT.GetSafeTexture());
                        break;

                    case PassEnum.Flowing:
                        cmd.SetComputeTextureParam(cs, kernel, KWS_ShaderConstants.FlowmapID.KW_FlowMapTex, Flowmap.GetSafeTexture(Color.gray));
                        break;

                    case PassEnum.DynamicWaves:
                        cmd.SetComputeFloatParam(cs, KWS_ShaderConstants.DynamicWaves.KW_InteractiveWavesAreaSize, DynamicWavesAreaSize);
                        cmd.SetComputeVectorParam(cs, KWS_ShaderConstants.DynamicWaves.KW_DynamicWavesWorldPos, DynamicWavesAreaPos);
                        cmd.SetComputeTextureParam(cs, kernel, KWS_ShaderConstants.DynamicWaves.KW_DynamicWaves,       DynamicWaves.GetSafeTexture());
                        cmd.SetComputeTextureParam(cs, kernel, KWS_ShaderConstants.DynamicWaves.KW_DynamicWavesNormal, DynamicWavesNormal.GetSafeTexture());
                        break;

                    case PassEnum.FluidsSimulation:
                        cmd.SetComputeFloatParam(cs, KWS_ShaderConstants.FlowmapID.KW_FluidsVelocityAreaScale, (0.5f * FluidsAreaSize / 40f) * FluidsSpeed);
                        cmd.SetComputeFloatParam(cs, KWS_ShaderConstants.FlowmapID.KW_FluidsMapAreaSize_lod0,  FluidsAreaSize);
                        cmd.SetComputeFloatParam(cs, KWS_ShaderConstants.FlowmapID.KW_FluidsMapAreaSize_lod1,  FluidsAreaSize * KWS_Settings.Flowing.AreaSizeMultiplierLod1);
                        cmd.SetComputeVectorParam(cs, KWS_ShaderConstants.FlowmapID.KW_FluidsMapWorldPosition_lod0, FluidsAreaPositionLod0);
                        cmd.SetComputeVectorParam(cs, KWS_ShaderConstants.FlowmapID.KW_FluidsMapWorldPosition_lod1, FluidsAreaPositionLod1);
                        cmd.SetComputeTextureParam(cs, kernel, KWS_ShaderConstants.FlowmapID.KW_Fluids_Lod0,     FluidsRT[0]);
                        cmd.SetComputeTextureParam(cs, kernel, KWS_ShaderConstants.FlowmapID.KW_Fluids_Lod1,     FluidsRT[1]);
                        cmd.SetComputeTextureParam(cs, kernel, KWS_ShaderConstants.FlowmapID.KW_FluidsFoam_Lod0, FluidsFoamRT[0]);
                        cmd.SetComputeTextureParam(cs, kernel, KWS_ShaderConstants.FlowmapID.KW_FluidsFoam_Lod1, FluidsFoamRT[1]);
                        break;

                    case PassEnum.Shoreline:
                        cmd.SetComputeVectorParam(cs, KWS_ShaderConstants.ShorelineID.KWS_ShorelineAreaPosSize, ShorelineAreaPosSize);
                        cmd.SetComputeTextureParam(cs, kernel, KWS_ShaderConstants.ShorelineID.KWS_ShorelineWavesDisplacement, ShorelineWavesDisplacement.GetSafeTexture());
                        cmd.SetComputeTextureParam(cs, kernel, KWS_ShaderConstants.ShorelineID.KWS_ShorelineWavesNormal,       ShorelineWavesNormal.GetSafeTexture());
                        break;

                    
                    default:
                        throw new ArgumentOutOfRangeException(nameof(pass), pass, null);
                }
        }

        public void Release()
        {
            if (WaterConstantBuffer != null)
            {
                WaterConstantBuffer.Release();
                WaterConstantBuffer = null;
            }
        }
    }
}

