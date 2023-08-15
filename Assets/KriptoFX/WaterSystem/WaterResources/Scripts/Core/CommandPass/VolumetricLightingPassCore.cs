using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using static KWS.KWS_CoreUtils;
using static KWS.KWS_ShaderConstants;

namespace KWS
{
    public class VolumetricLightingPassCore : WaterPassCore
    {
        public Action<CommandBuffer, Camera, RTHandle> OnSetRenderTarget;

        public RTHandle _volumeLightRT_Blured;
        public RTHandle _volumeLightRT;
        public RTHandle _volumeLightRT_Temp;
        public RTHandle _lowResDepthRT;
        //public RTHandle volumeLightRT_Temp;
        //public RTHandle lowResDepthRT;

        private Material volumeLightMat;
        private Material volumeLightBlurMat;
        private KW_PyramidBlur pyramidBlur = new KW_PyramidBlur();

        public VolumetricLightingPassCore(WaterSystem waterInstance)
        {
            PassName = "Water.VolumetricLightingPass";
            WaterInstance = waterInstance;

            WaterSystem.OnWaterSettingsChanged += OnWaterSettingsChanged;

            OnWaterSettingsChanged(waterInstance, WaterSystem.WaterTab.All);
        }

        public override void Release()
        {
            WaterSystem.OnWaterSettingsChanged -= OnWaterSettingsChanged;
            ReleaseTextures();
      
            KW_Extensions.SafeDestroy(volumeLightMat, volumeLightBlurMat);
            //KW_Extensions.WaterLog(this, "Release", KW_Extensions.WaterLogMessageType.Release);
        }

        private void OnWaterSettingsChanged(WaterSystem waterInstance, WaterSystem.WaterTab changedTab)
        {
            if (!changedTab.HasFlag(WaterSystem.WaterTab.VolumetricLighting)) return;

            InitializeTextures();
            InitializeMaterials();
            UpdateConstantShaderParams();
        }


        Vector4 ComputeMieVector(float MieG)
        {
            return new Vector4(1 - (MieG * MieG), 1 + (MieG * MieG), 2 * MieG, 1.0f / (4.0f * Mathf.PI));
        }


        Vector2Int ComputeRTHandleSize(Vector2Int screenSize)
        {
            var resolutionDownsample = (int)WaterInstance.Settings.VolumetricLightResolutionQuality / 100f;

            if (WaterInstance.Settings.VolumetricLightFilter == WaterSystem.VolumetricLightFilterEnum.Bilateral)
            {
                return new Vector2Int((int)(screenSize.x * resolutionDownsample), (int)(screenSize.y * resolutionDownsample));
            }
            else
            {
                return new Vector2Int((int)(screenSize.x * resolutionDownsample), (int)(screenSize.y * resolutionDownsample));
            }
        }

        Vector2Int ComputeRTHandleSizeBlured(Vector2Int screenSize)
        {
            if (WaterInstance.Settings.VolumetricLightFilter == WaterSystem.VolumetricLightFilterEnum.Bilateral) return screenSize;
            else return ComputeRTHandleSize(screenSize);
        }

        public void InitializeTextures()
        {
            ReleaseTextures();

            _volumeLightRT = KWS_CoreUtils.RTHandleAllocVR(ComputeRTHandleSize, name: "_volumeLightRT", colorFormat: GraphicsFormat.R16G16B16A16_SFloat);
            _volumeLightRT_Blured = KWS_CoreUtils.RTHandleAllocVR(ComputeRTHandleSizeBlured, name: "_volumeLightRT_Blured", colorFormat: GraphicsFormat.R16G16B16A16_SFloat);
            if (WaterInstance.Settings.VolumetricLightFilter == WaterSystem.VolumetricLightFilterEnum.Bilateral)
            {
                _volumeLightRT_Temp = KWS_CoreUtils.RTHandleAllocVR(ComputeRTHandleSize, name: "_volumeLightRT_Temp", colorFormat: GraphicsFormat.R16G16B16A16_SFloat);
                _lowResDepthRT = KWS_CoreUtils.RTHandleAllocVR(ComputeRTHandleSize, name: "_lowResDepthRT", colorFormat: GraphicsFormat.R32_SFloat);
            }

            WaterInstance.SharedData.VolumetricLightingRT     = _volumeLightRT_Blured;

           // KW_Extensions.WaterLog(this, _volumeLightRT, _volumeLightRT_Blured);
        }

        void InitializeMaterials()
        {
            if (volumeLightMat == null)
            {
                volumeLightMat = CreateMaterial(KWS_ShaderConstants.ShaderNames.VolumetricLightingShaderName);
                WaterInstance.AddShaderToWaterRendering(volumeLightMat);
            }

            if (volumeLightBlurMat == null)
            {
                volumeLightBlurMat = CreateMaterial(KWS_ShaderConstants.ShaderNames.BlurBilateralName);
                WaterInstance.AddShaderToWaterRendering(volumeLightBlurMat);
            }
        }


        public void Execute(Camera cam, CommandBuffer cmd)
        {
            if (!WaterInstance.Settings.UseVolumetricLight) return;
            if (!WaterInstance.Settings.EnabledMeshRendering) return;
            if (!KWS_CoreUtils.CanBeRenderCurrentWaterInstance(WaterInstance)) return;

            UpdateShaderParams(cam, cmd);
            DisableCausticForLowSettings();
            OnSetRenderTarget?.Invoke(cmd, cam, _volumeLightRT);
            cmd.BlitTriangleRTHandle(_volumeLightRT, volumeLightMat, ClearFlag.None, Color.clear, 0);

            if (WaterInstance.Settings.VolumetricLightFilter == WaterSystem.VolumetricLightFilterEnum.Bilateral)
            {
                volumeLightBlurMat.SetTexture(VolumetricLightConstantsID.KWS_CameraDepthTextureLowRes, _lowResDepthRT);
                cmd.BlitTriangleRTHandle(_lowResDepthRT, volumeLightBlurMat, ClearFlag.None, Color.clear, pass: KWS_Settings.VolumetricLighting.UseFastBilateralMode ? 1 : 0);

                cmd.BlitTriangleRTHandle(_volumeLightRT, _volumeLightRT_Temp, volumeLightBlurMat, ClearFlag.None, Color.clear, pass: KWS_Settings.VolumetricLighting.UseFastBilateralMode ? 3 : 2);
                cmd.BlitTriangleRTHandle(_volumeLightRT_Temp, _volumeLightRT, volumeLightBlurMat, ClearFlag.None, Color.clear, pass: KWS_Settings.VolumetricLighting.UseFastBilateralMode ? 5 : 4);

                cmd.BlitTriangleRTHandle(_volumeLightRT, _volumeLightRT_Blured, volumeLightBlurMat, ClearFlag.None, Color.clear, pass: 6);
            }
            else
            {
                pyramidBlur.ComputeBlurPyramid(WaterInstance.Settings.VolumetricLightBlurRadius, _volumeLightRT, _volumeLightRT_Blured, cmd, ComputeRTHandleSize);
            }

            //var rtSize = WaterInstance.Settings.VolumetricLightFilter == WaterSystem.VolumetricLightFilterEnum.Bilateral ? _volumeLightRT_Blured.rtHandleProperties.rtHandleScale : Vector4.one;
            //WaterInstance.SetTextures(cmd, requireUpdateComputeShaders: false, (VolumetricLightConstantsID.KWS_VolumetricLightRT, _volumeLightRT_Blured.rt));
            //WaterInstance.SetVectors(cmd, requireUpdateComputeShaders: false, (VolumetricLightConstantsID.KWS_VolumetricLight_RTHandleScale, rtSize));
           
        }

        private void DisableCausticForLowSettings()
        {
            if (WaterInstance.Settings.VolumetricLightResolutionQuality == WaterSystem.VolumetricLightResolutionQualityEnum.VeryLow
             || WaterInstance.Settings.VolumetricLightResolutionQuality == WaterSystem.VolumetricLightResolutionQualityEnum.Low) volumeLightMat.DisableKeyword(WaterKeywords.USE_CAUSTIC);
            else if (WaterInstance.Settings.UseCausticEffect) volumeLightMat.EnableKeyword(WaterKeywords.USE_CAUSTIC);
        }

        private void UpdateConstantShaderParams()
        {
            var anisoMie = ComputeMieVector(0.05f);
            volumeLightMat.SetVector(VolumetricLightConstantsID.KWS_LightAnisotropy, anisoMie);
        }

        private void UpdateShaderParams(Camera cam, CommandBuffer cmd)
        {
            var anisoMie = ComputeMieVector(0.05f);

            float volumeLightFade = 1;
            if (WaterInstance.Settings.WaterMeshType == WaterSystem.WaterMeshTypeEnum.InfiniteOcean) volumeLightFade
                = Mathf.Clamp01(Mathf.Exp(-1 * (WaterInstance.transform.position.y - cam.transform.position.y) / WaterInstance.Settings.Transparent)); //todo add depth fading relative to the actual surface height

            var rtHandleScale = _volumeLightRT.rtHandleProperties.rtHandleScale;
            var actualRTSize  = new Vector2(_volumeLightRT.rt.width * rtHandleScale.x, _volumeLightRT.rt.height * rtHandleScale.y);
            cmd.SetGlobalFloat(VolumetricLightConstantsID.KWS_VolumeDepthFade, volumeLightFade);

            cmd.SetGlobalVector(CausticID.KW_CausticLodOffset, KW_Extensions.GetCameraForwardFast(cam) * 0.5f);
            cmd.SetGlobalVector(VolumetricLightConstantsID.KWS_LightAnisotropy, anisoMie);
            cmd.SetGlobalVector(VolumetricLightConstantsID.KWS_VolumeTexSceenSize, actualRTSize);
            cmd.SetGlobalVector(VolumetricLightConstantsID.KWS_WorldSpaceCameraPosCompillerFixed, cam.transform.position);
            WaterInstance.SharedData.UpdateShaderParams(cmd, SharedData.PassEnum.Caustic);
        }

        void ReleaseTextures()
        {
            if (_volumeLightRT        != null) _volumeLightRT.Release();
            if (_volumeLightRT_Blured != null) _volumeLightRT_Blured.Release();
            if (_volumeLightRT_Temp   != null) _volumeLightRT_Temp.Release();
            if (_lowResDepthRT        != null) _lowResDepthRT.Release();
            pyramidBlur?.Release();
        }

    }
}