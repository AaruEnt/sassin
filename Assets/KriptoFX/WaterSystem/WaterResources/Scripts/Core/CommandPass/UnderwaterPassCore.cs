using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace KWS
{
    public class UnderwaterPassCore : WaterPassCore
    {
        public Action<CommandBuffer, RTHandle> OnSetRenderTarget;

        KW_PyramidBlur _pyramidBlur;
        private Material _underwaterMaterial;

        RTHandle _underwaterRT;
        RTHandle _underwaterRTBlured;

        readonly Vector2 _rtScale = new Vector2(0.35f, 0.35f);

        public UnderwaterPassCore(WaterSystem waterInstance)
        {
            PassName = "Water.UnderwaterPass";
            WaterInstance = waterInstance;
            
            WaterSystem.OnWaterSettingsChanged += OnWaterSettingsChanged;
            OnWaterSettingsChanged(waterInstance, WaterSystem.WaterTab.All);
        }

        private void OnWaterSettingsChanged(WaterSystem waterInstance, WaterSystem.WaterTab changedTab)
        {
            if (!changedTab.HasFlag(WaterSystem.WaterTab.Underwater)) return;
            //if (!changedTab.HasFlag(WaterSystem.WaterTab.Underwater) && !changedTab.HasFlag(WaterSystem.WaterTab.VolumetricLighting)) return;

            InitializeMaterials();
            InitializeTextures();
        }

        public override void Release()
        {
            WaterSystem.OnWaterSettingsChanged += OnWaterSettingsChanged;
            if (_underwaterRT       != null) _underwaterRT.Release();
            if (_underwaterRTBlured != null) _underwaterRTBlured.Release();
            if (_pyramidBlur        != null) _pyramidBlur.Release();
            KW_Extensions.SafeDestroy(_underwaterMaterial);
           

            //KW_Extensions.WaterLog(this, "Release", KW_Extensions.WaterLogMessageType.Release);
        }
        void InitializeTextures()
        {
            if (_underwaterRT != null) _underwaterRT.Release();
            if (_underwaterRTBlured != null) _underwaterRTBlured.Release();

            var hdrFormat = KWS_CoreUtils.GetGraphicsFormatHDR();
            _underwaterRT = KWS_CoreUtils.RTHandleAllocVR(_rtScale, name: "_underwaterRT", colorFormat: hdrFormat);
            _underwaterRTBlured = KWS_CoreUtils.RTHandleAllocVR(_rtScale, name: "_underwaterRT_Blured", colorFormat: hdrFormat);

            //KW_Extensions.WaterLog(this, _underwaterRT, _underwaterRTBlured);
        }


        void InitializeMaterials()
        {
            if (_underwaterMaterial == null)
            {
                _underwaterMaterial = KWS_CoreUtils.CreateMaterial(KWS_ShaderConstants.ShaderNames.UnderwaterShaderName);
                WaterInstance.AddShaderToWaterRendering(_underwaterMaterial);
            }
        }

        public void Execute()
        {

        }

        public void Execute(Camera cam, CommandBuffer cmd, RenderTargetIdentifier colorBuffer)
        {
            if (!WaterInstance.Settings.UseUnderwaterEffect || !WaterInstance.IsCameraUnderwater) return;
            if (!WaterInstance.Settings.EnabledMeshRendering) return;
            if (!KWS_CoreUtils.CanBeRenderCurrentWaterInstance(WaterInstance)) return;

            if (WaterInstance.Settings.UseUnderwaterBlur)
            {
                OnSetRenderTarget?.Invoke(cmd, _underwaterRT);
                var targetViewPortSize = KWS_CoreUtils.GetCameraRTHandleViewPortSize(cam);
                cmd.BlitTriangleRTHandle(_underwaterRT, _underwaterMaterial, ClearFlag.None, Color.clear, 0);

                if (_pyramidBlur == null) _pyramidBlur = new KW_PyramidBlur();
                if(WaterInstance.Settings.UnderwaterBlurRadius < 2.5)
                    _pyramidBlur.ComputeSeparableBlur(WaterInstance.Settings.UnderwaterBlurRadius, _underwaterRT, _underwaterRTBlured, cmd, _rtScale);
                else _pyramidBlur.ComputeBlurPyramid(WaterInstance.Settings.UnderwaterBlurRadius - 3.0f, _underwaterRT, _underwaterRTBlured, cmd, _rtScale);

                var destRT = WaterInstance.Settings.UseUnderwaterBlur ? _underwaterRTBlured : _underwaterRT;
                cmd.SetGlobalVector(KWS_ShaderConstants.UnderwaterID.KWS_Underwater_RTHandleScale, WaterInstance.Settings.UseUnderwaterBlur ? Vector4.one : _underwaterRTBlured.rtHandleProperties.rtHandleScale);

                OnSetRenderTarget?.Invoke(cmd, null);
                cmd.BlitTriangle(destRT, destRT.rtHandleProperties.rtHandleScale, colorBuffer, targetViewPortSize, _underwaterMaterial, 1);
            }
            else
            {
                cmd.BlitTriangle(colorBuffer, _underwaterMaterial);
            }
        }

    }
}