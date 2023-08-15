using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace KWS
{
    public class ScreenSpaceReflectionPassCore : WaterPassCore
    {
        public Action<CommandBuffer, Camera, RTHandle> OnSetRenderTarget;

        readonly PassData _currentPassData       = new PassData();
        readonly PassData _currentPassDataEditor = new PassData();

        private Material      _filteringMaterial;
        private ComputeShader _cs;

        int _kernelClear;
        int _kernelRenderHash;
        int _kernelRenderColorFromHash;

        const int shaderNumthreadX = 8;
        const int shaderNumthreadY = 8;

        class PassData
        {
            public RTHandle[] ReflectionRT = new RTHandle[2];
            public RTHandle   ReflectionRTFilteredMip;
            public ComputeBuffer  HashBuffer;
            public int            Frame;

            public Matrix4x4   PrevVPMatrix;
            public Matrix4x4[] PrevVPMatrixStereo = new Matrix4x4[2];

            public Vector2Int GetCurrentResolution()
            {
                var scale = ReflectionRT[0].rtHandleProperties.rtHandleScale;
                return new Vector2Int(Mathf.RoundToInt(ReflectionRT[0].rt.width * scale.x), Mathf.RoundToInt(ReflectionRT[0].rt.height * scale.y));
            }

            public void ReInitializeTextures(WaterSystem waterInstance)
            {
                Release();

                var colorFormat = GraphicsFormat.R16G16B16A16_SFloat;

                Vector2Int ScaleFunc(Vector2Int size)
                {
                    float scale = (int) waterInstance.Settings.ScreenSpaceReflectionResolutionQuality / 100f;
                    return new Vector2Int(Mathf.RoundToInt(scale * size.x), Mathf.RoundToInt(scale * size.y));
                }

                if (waterInstance.Settings.UseAnisotropicReflections)
                {
                    for (int idx = 0; idx < 2; idx++) ReflectionRT[idx] = KWS_CoreUtils.RTHandleAllocVR(ScaleFunc, name: "_reflectionRT" + idx, colorFormat: colorFormat, enableRandomWrite: true);
                    ReflectionRTFilteredMip = KWS_CoreUtils.RTHandleAllocVR(ScaleFunc, name: "_reflectionRT_Mip", colorFormat: colorFormat, useMipMap: true, autoGenerateMips: true, mipMapCount: 5);
                }
                else
                {
                    for (int idx = 0; idx < 2; idx++)
                        ReflectionRT[idx] = KWS_CoreUtils.RTHandleAllocVR(ScaleFunc, name: "_reflectionRT" + idx, colorFormat: colorFormat,
                                                                  enableRandomWrite: true, useMipMap: true, autoGenerateMips: false, mipMapCount: 5);
                }

                //KW_Extensions.WaterLog(this, ReflectionRT[0], ReflectionRT[1], ReflectionRTFilteredMip);
            }

            public void InitializeHashBuffer(Vector2Int resolution)
            {
                var size                                        = resolution.x * resolution.y;
                if (WaterSystem.IsSinglePassStereoEnabled) size *= 2;
                HashBuffer = KWS_CoreUtils.GetOrUpdateBuffer<uint>(ref HashBuffer, size);
            }


            public void Release()
            {
                foreach (var rtHandle in ReflectionRT)
                    if (rtHandle != null)
                        rtHandle.Release();
                if (ReflectionRTFilteredMip != null) ReflectionRTFilteredMip.Release();
                if (HashBuffer              != null) HashBuffer.Release();
                HashBuffer = null;
            }
        }

        public ScreenSpaceReflectionPassCore(WaterSystem waterInstance)
        {
            PassName      = "Water.ScreenSpaceReflectionPass";
            WaterInstance = waterInstance;

            WaterSystem.OnWaterSettingsChanged += OnWaterSettingsChanged;
            InitializeMaterials(WaterInstance);

            OnWaterSettingsChanged(waterInstance, WaterSystem.WaterTab.Reflection);

        }

        public override void Release()
        {
            WaterSystem.OnWaterSettingsChanged -= OnWaterSettingsChanged;
            _currentPassData.Release();
            _currentPassDataEditor.Release();

            Resources.UnloadAsset(_cs);
            KW_Extensions.SafeDestroy(_filteringMaterial);

            //KW_Extensions.WaterLog(this, "Release", KW_Extensions.WaterLogMessageType.Release);
        }

        private void OnWaterSettingsChanged(WaterSystem waterInstance, WaterSystem.WaterTab changedTab)
        {
            if (!changedTab.HasFlag(WaterSystem.WaterTab.Reflection)) return;

            _currentPassData.ReInitializeTextures(WaterInstance);
            _currentPassData.InitializeHashBuffer(_currentPassData.GetCurrentResolution());

#if UNITY_EDITOR
            _currentPassDataEditor.ReInitializeTextures(WaterInstance);
            _currentPassDataEditor.InitializeHashBuffer(_currentPassDataEditor.GetCurrentResolution());
#endif
        }

        void InitializeMaterials(WaterSystem waterInstance)
        {
            if (_cs == null)
            {
                _cs = (ComputeShader) Resources.Load($"PlatformSpecific/KWS_ScreenSpaceReflectionCompute");

                _kernelClear               = _cs.FindKernel("Clear_kernel");
                _kernelRenderHash          = _cs.FindKernel("RenderHash_kernel");
                _kernelRenderColorFromHash = _cs.FindKernel("RenderColorFromHash_kernel");

                waterInstance.AddShaderToWaterRendering(_cs, 1, 2);
            }

            if (_filteringMaterial == null)
            {
                _filteringMaterial = KWS_CoreUtils.CreateMaterial(KWS_ShaderConstants.ShaderNames.ReflectionFiltering);
                waterInstance.AddShaderToWaterRendering(_filteringMaterial);
            }
        }


        public void Execute(Camera cam, CommandBuffer cmd, RenderTargetIdentifier depthBuffer, RenderTargetIdentifier? colorBuffer)
        {
            var settings = WaterInstance.Settings;
            if (!settings.UseScreenSpaceReflection) return;
            if (!WaterInstance.Settings.EnabledMeshRendering) return;
            if (!KWS_CoreUtils.CanBeRenderCurrentWaterInstance(WaterInstance)) return;

            var camType  = cam.cameraType;
            var passData = camType == CameraType.SceneView ? _currentPassDataEditor : _currentPassData;

            var targetRT     = passData.Frame % 2 == 0 ? passData.ReflectionRT[0] : passData.ReflectionRT[1];
            var lastTargetRT = passData.Frame % 2 == 0 ? passData.ReflectionRT[1] : passData.ReflectionRT[0];

            OnSetRenderTarget?.Invoke(cmd, cam, targetRT);

            var currentResolution = passData.GetCurrentResolution();
            var dispatchSize      = new Vector2Int(Mathf.CeilToInt((float) currentResolution.x / shaderNumthreadX), Mathf.CeilToInt((float) currentResolution.y / shaderNumthreadY));
            var waterOffsetFix    = settings.ReflectionClipPlaneOffset * 15;
            var stereoPasses      = WaterSystem.IsSinglePassStereoEnabled ? 2 : 1;
            if (targetRT.rt.volumeDepth != stereoPasses) return;
            passData.InitializeHashBuffer(currentResolution);

            cmd.SetComputeIntParam(_cs, KWS_ShaderConstants.ReflectionsID.KWS_ReprojectedFrameReady, passData.Frame > 0 ? 1 : 0);
            cmd.SetComputeFloatParam(_cs, KWS_ShaderConstants.SSR_ID._HorizontalPlaneHeightWS, WaterInstance.WaterPivotWorldPosition.y + waterOffsetFix);
            cmd.SetComputeVectorParam(_cs, KWS_ShaderConstants.SSR_ID._RTSize, new Vector4(currentResolution.x, currentResolution.y, 1f / currentResolution.x, 1f / currentResolution.y));
            if (WaterSystem.IsSinglePassStereoEnabled) cmd.SetComputeMatrixArrayParam(_cs, KWS_ShaderConstants.CameraMatrix.KWS_PREV_MATRIX_VP_STEREO, passData.PrevVPMatrixStereo);
            else cmd.SetComputeMatrixParam(_cs, KWS_ShaderConstants.CameraMatrix.KWS_PREV_MATRIX_VP, passData.PrevVPMatrix);


            ///////////////////////////clear pass//////////////////////////////////
            cmd.SetComputeBufferParam(_cs, _kernelClear, KWS_ShaderConstants.SSR_ID.HashRT, passData.HashBuffer);
            cmd.SetComputeTextureParam(_cs, _kernelClear, KWS_ShaderConstants.SSR_ID.ColorRT, targetRT.rt);
            cmd.DispatchCompute(_cs, _kernelClear, dispatchSize.x, dispatchSize.y, stereoPasses);
            ///////////////////////////clear pass//////////////////////////////////


            ///////////////////////////render to hash pass//////////////////////////////////
            cmd.SetComputeBufferParam(_cs, _kernelRenderHash, KWS_ShaderConstants.SSR_ID.HashRT, passData.HashBuffer);
            cmd.SetComputeTextureParam(_cs, _kernelRenderHash, KWS_ShaderConstants.SSR_ID.ColorRT,             targetRT.rt);
            cmd.SetComputeTextureParam(_cs, _kernelRenderHash, KWS_ShaderConstants.SSR_ID._CameraDepthTexture, depthBuffer);
            cmd.DispatchCompute(_cs, _kernelRenderHash, dispatchSize.x, dispatchSize.y, stereoPasses);
            ///////////////////////////render to hash pass//////////////////////////////////

          
            ///////////////////////////render color pass//////////////////////////////////
            //cmd.SetComputeTextureParam(_cs, _kernelRenderColorFromHash, KWS_ShaderConstants.ReflectionsID.KWS_PlanarReflectionRT,  WaterInstance.SharedData.PlanarReflectionRaw);
            //cmd.SetComputeTextureParam(_cs, _kernelRenderColorFromHash, KWS_ShaderConstants.ReflectionsID.KWS_CubemapReflectionRT, WaterInstance.SharedData.CubemapReflection);
            WaterInstance.SharedData.UpdateShaderParams(cmd, SharedData.PassEnum.PlanarReflection, _cs, _kernelRenderColorFromHash);
            WaterInstance.SharedData.UpdateShaderParams(cmd, SharedData.PassEnum.CubemapReflection, _cs, _kernelRenderColorFromHash);
            
            cmd.SetComputeTextureParam(_cs, _kernelRenderColorFromHash, KWS_ShaderConstants.SSR_ID.KWS_LastTargetRT,               lastTargetRT);
            cmd.SetComputeBufferParam(_cs, _kernelRenderColorFromHash, KWS_ShaderConstants.SSR_ID.HashRT, passData.HashBuffer);
            cmd.SetComputeTextureParam(_cs, _kernelRenderColorFromHash, KWS_ShaderConstants.SSR_ID.ColorRT, targetRT.rt);
            if (colorBuffer != null) cmd.SetComputeTextureParam(_cs, _kernelRenderColorFromHash, KWS_ShaderConstants.SSR_ID._CameraOpaqueTexture, (RenderTargetIdentifier) colorBuffer);
            cmd.DispatchCompute(_cs, _kernelRenderColorFromHash, dispatchSize.x, dispatchSize.y, stereoPasses);
            ///////////////////////////render color pass//////////////////////////////////

            if (settings.UseAnisotropicReflections)
            {
                cmd.SetKeyword("USE_STEREO_ARRAY", true);
                cmd.BlitTriangleRTHandle(targetRT, targetRT.rtHandleProperties.rtHandleScale, passData.ReflectionRTFilteredMip, _filteringMaterial, ClearFlag.None, Color.clear, settings.AnisotropicReflectionsHighQuality ? 1 : 0);
                cmd.SetKeyword("USE_STEREO_ARRAY", false);
            }
            else
            {
                cmd.GenerateMips(targetRT.rt);
            }

            passData.Frame++;
            if (WaterSystem.IsSinglePassStereoEnabled) passData.PrevVPMatrixStereo = WaterInstance.CurrentVPMatrixStereo;
            else passData.PrevVPMatrix                                             = WaterInstance.CurrentVPMatrix;


            WaterInstance.SharedData.SsrReflection = settings.UseAnisotropicReflections ? passData.ReflectionRTFilteredMip : targetRT;
        }


    }
}