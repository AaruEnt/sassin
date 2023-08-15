using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using static KWS.KWS_CoreUtils;

namespace KWS
{
    public class DrawToPosteffectsDepthPassCore : WaterPassCore
    {
        public Action<CommandBuffer, Camera> OnSetRenderTarget;

        RTHandle _sceneDepthRT;
        private Material _drawToDepthMaterial;
        private Material _copyDepthMaterial;

        bool _isTexturesInitialized;
        private bool _lastStereoEnabled;

        public DrawToPosteffectsDepthPassCore(WaterSystem waterInstance)
        {
            PassName = "Water.DrawToPostFxDepthPass";
            WaterInstance = waterInstance;
        }

        public override void Release()
        {
            if (_sceneDepthRT != null) _sceneDepthRT.Release();
            _isTexturesInitialized = false;
            KW_Extensions.SafeDestroy(_drawToDepthMaterial, _copyDepthMaterial);

            //KW_Extensions.WaterLog(this, "Release", KW_Extensions.WaterLogMessageType.Release);
        }
        void CheckAndReinitializeTextures()
        {
            if (!_isTexturesInitialized || _lastStereoEnabled != WaterSystem.IsSinglePassStereoEnabled)
            {
                InitializeTextures();
                _lastStereoEnabled = WaterSystem.IsSinglePassStereoEnabled;
                //KW_Extensions.WaterLog(this, "Reset RTHandle Reference Size");
            }
        }

        void InitializeTextures()
        {
            _sceneDepthRT = KWS_CoreUtils.RTHandleAllocVR(Vector2.one, name: "_depthRT", colorFormat: GraphicsFormat.R32_SFloat);
            _isTexturesInitialized = true;

            //KW_Extensions.WaterLog(this, _sceneDepthRT);
        }

        public RenderTargetIdentifier GetTargetColorBuffer()
        {
            if (_sceneDepthRT == null) CheckAndReinitializeTextures();
            return _sceneDepthRT;
        }

        void InitializeMaterials()
        {
            if (_drawToDepthMaterial == null)
            {
                _drawToDepthMaterial = KWS_CoreUtils.CreateMaterial(KWS_ShaderConstants.ShaderNames.DrawToDepthShaderName);
                WaterInstance.AddShaderToWaterRendering(_drawToDepthMaterial);
            }

            if (_copyDepthMaterial == null)
            {
                _copyDepthMaterial = KWS_CoreUtils.CreateMaterial(KWS_ShaderConstants.ShaderNames.CopyDepthShaderName);
            }
        }

        public void Execute(Camera cam, CommandBuffer cmd, RenderTargetIdentifier depthBuffer)
        {
            if (!WaterInstance.Settings.DrawToPosteffectsDepth) return;
            if (!WaterInstance.Settings.EnabledMeshRendering) return;
            if (!CanBeRenderCurrentWaterInstance(WaterInstance)) return;
            if (!KWS_Settings.Water.IsDepthOfFieldRequireWaterDepth) return;
            if (WaterInstance.Settings.UnderwaterQueue == WaterSystem.UnderwaterQueueEnum.AfterTransparent && WaterInstance.IsCameraUnderwater) return;

            InitializeMaterials();
            CheckAndReinitializeTextures();
            cmd.BlitTriangleRTHandle(_sceneDepthRT, _copyDepthMaterial, ClearFlag.None, Color.clear);
            OnSetRenderTarget?.Invoke(cmd, cam);
            cmd.BlitTriangle(_sceneDepthRT, _sceneDepthRT.rtHandleProperties.rtHandleScale, depthBuffer, _drawToDepthMaterial);
        }

    }
}