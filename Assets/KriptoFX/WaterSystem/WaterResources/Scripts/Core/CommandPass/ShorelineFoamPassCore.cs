//#define BAKE_AO

using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace KWS
{
    public class ShorelineFoamPassCore : WaterPassCore
    {
        public Action<CommandBuffer, Camera, RTHandle> OnSetRenderTarget;

        RTHandle _targetRT;
        private ComputeBuffer _computeFoamBuffer;
        private ComputeBuffer _computeFoamLightBuffer;
        private ComputeBuffer _foamDataBuffer;
        private ComputeBuffer _foamDataCountOffsetBuffer;
        private ComputeBuffer _lodIndexesBuffer;
        private ComputeBufferObject _rawBuffer;
        private ComputeBufferObject _rawBufferOffset;

        private int _foamRT_ID = Shader.PropertyToID("_FoamRT");
        public readonly int FoamBufferID = Shader.PropertyToID("_FoamBuffer");
        public readonly int FoamDepthBufferID = Shader.PropertyToID("_FoamDepthBuffer");
        public readonly int FoamLightBufferID = Shader.PropertyToID("_FoamLightBuffer");


        private int _clearKernel;
        private int _drawToBufferKernel;
        private int _drawToTextureKernel;

        ComputeShader _foamComputeShader;

        private int _renderToBufferDispatchSize;
        private Vector2Int _renderToTextureDispatchSize;
        Vector4 _targetRTSize;


        private const int maxParticles = 385871;
        private const int kernelSize = 8;

        private readonly Vector2Int _foamRTSize = new Vector2Int(1920, 1080);
        private readonly Vector2Int _foamRTSizeVR = new Vector2Int(1440, 1440);

#if BAKE_AO
        private ComputeBuffer _foamAOBuffer;
        public readonly int _AOBufferID = Shader.PropertyToID("_AOBuffer");
        private float[] _aoData;

        private int _AOframeNumber = 0;
        private uint[] _currentData;
#endif

        private bool _lastFastPath;

        public ShorelineFoamPassCore(WaterSystem waterInstance)
        {
            PassName = "Water.ShorelineFoamPass";
            WaterInstance = waterInstance;
        }
        public override void Release()
        {
            _targetRT?.Release();
            _targetRT = null;

            if (_computeFoamBuffer         != null) _computeFoamBuffer.Dispose();
            if (_computeFoamLightBuffer    != null) _computeFoamLightBuffer.Dispose();
            if (_foamDataBuffer            != null) _foamDataBuffer.Dispose();
            if (_foamDataCountOffsetBuffer != null) _foamDataCountOffsetBuffer.Dispose();
            if (_lodIndexesBuffer          != null) _lodIndexesBuffer.Dispose();
#if BAKE_AO
            _foamAOBuffer?.Dispose();
            _foamAOBuffer = null;
#endif

            Resources.UnloadAsset(_foamComputeShader);
            _foamComputeShader = null;
            Resources.UnloadUnusedAssets();
           
            _computeFoamBuffer = _computeFoamLightBuffer = _foamDataBuffer = _foamDataCountOffsetBuffer = _lodIndexesBuffer = null;
            _lastFastPath      = false;

            //KW_Extensions.WaterLog(this, "Release", KW_Extensions.WaterLogMessageType.Release);
        }
        void InitializeRT(WaterSystem waterInstance)
        {
            var format = waterInstance.Settings.UseShorelineFoamFastMode ? GraphicsFormat.R32_UInt : GraphicsFormat.R8G8B8A8_UNorm;
            var dimension = WaterSystem.IsSinglePassStereoEnabled ? TextureDimension.Tex2DArray : TextureDimension.Tex2D;
            var slices = WaterSystem.IsSinglePassStereoEnabled ? 2 : 1;
            
            var vrUsage = VRTextureUsage.None;
#if ENABLE_VR
           if (WaterSystem.IsSinglePassStereoEnabled) vrUsage = UnityEngine.XR.XRSettings.eyeTextureDesc.vrUsage;
#endif

            var size = WaterSystem.IsSinglePassStereoEnabled ? _foamRTSizeVR : _foamRTSize;

            _targetRT = KWS_CoreUtils.RTHandleAllocVR(size.x, size.y,  colorFormat: format, name : "_foamRT", enableRandomWrite : true);
            _targetRTSize = new Vector4(size.x, size.y, 1.0f / size.x, 1.0f / size.y);
        }

        void InitializeComputeData(WaterSystem waterInstance)
        {
            if (_targetRT == null || waterInstance.Settings.UseShorelineFoamFastMode != _lastFastPath)
            {
                if (_targetRT != null) _targetRT.Release();
                _lastFastPath = waterInstance.Settings.UseShorelineFoamFastMode;
                InitializeRT(waterInstance);
            }

            var stereoPasses = WaterSystem.IsSinglePassStereoEnabled ? 2 : 1;
            _computeFoamBuffer = KWS_CoreUtils.GetOrUpdateBuffer<uint>(ref _computeFoamBuffer, _targetRT.rt.width * _targetRT.rt.height * stereoPasses);
            _computeFoamLightBuffer = KWS_CoreUtils.GetOrUpdateBuffer<uint>(ref _computeFoamLightBuffer, _targetRT.rt.width * _targetRT.rt.height * stereoPasses);

            if (_foamComputeShader == null)
            {
                _foamComputeShader = (ComputeShader)Resources.Load($"PlatformSpecific/KWS_ShorelineFoamParticlesCompute");
                _clearKernel = _foamComputeShader.FindKernel("ClearFoamBuffer");
                _drawToBufferKernel = _foamComputeShader.FindKernel("RenderFoamToBuffer");
                _drawToTextureKernel = _foamComputeShader.FindKernel("RenderFoamBufferToTexture");

                _rawBuffer = Resources.Load<ComputeBufferObject>("ComputeBuffers/ComputeFoamData0");
                _foamDataBuffer = _rawBuffer.GetComputeBuffer();

                _rawBufferOffset = Resources.Load<ComputeBufferObject>("ComputeBuffers/ComputeFoamDataOffset0");
                _foamDataCountOffsetBuffer = _rawBufferOffset.GetComputeBuffer();

                waterInstance.AddShaderToWaterRendering(_foamComputeShader, 1);
            }
            _renderToBufferDispatchSize = Mathf.CeilToInt(Mathf.Sqrt(1f * maxParticles / (kernelSize * kernelSize)));
            _renderToTextureDispatchSize = new Vector2Int(Mathf.CeilToInt(_targetRT.rt.width / 256f), Mathf.CeilToInt(_targetRT.rt.width / 1f));

#if BAKE_AO
            _currentData = data;
            _foamAOBuffer = new ComputeBuffer(packedData.Length, Marshal.SizeOf(typeof(float)));
            _aoData = new float[packedData.Length];
            _foamAOBuffer.SetData(_aoData);
#endif
        }


#if BAKE_AO
        uint BakeAOToData(uint data, float ao)
        {
            uint encodedAO = (uint)(Mathf.Clamp01(ao) * 255.0f); //8 bits = 2^8 - 1
            encodedAO = encodedAO & 0xFF;

            return ((data >> 12) & 0xFFFFF) << 12 | (encodedAO << 4) | data & 0xF;
        }
#endif

        public void Execute(Camera cam, CommandBuffer cmd, RenderTargetIdentifier colorRT, RenderTargetIdentifier depthRT)
        {
            if (!WaterInstance.Settings.UseShorelineRendering) return;
            if (!KWS_CoreUtils.CanBeRenderCurrentWaterInstance(WaterInstance)) return;

            RenderFoam(cam, cmd, colorRT, depthRT);
        }

        void RenderFoam(Camera cam, CommandBuffer cmd, RenderTargetIdentifier colorRT, RenderTargetIdentifier depthRT)
        {
            var buffer = WaterInstance.SharedData.ShorelineWaveBuffers;
            if (buffer == null || buffer.FoamWavesComputeBuffer == null || buffer.VisibleFoamWaves.Count == 0) return;

          
            InitializeComputeData(WaterInstance);
            _foamComputeShader.SetBuffer(_drawToBufferKernel, KWS_ShaderConstants.StructuredBuffers.KWS_ShorelineDataBuffer, buffer.FoamWavesComputeBuffer);
          
#if BAKE_AO
            cmd.SetComputeBufferParam(_foamComputeShader, clearKernel, _AOBufferID, _foamAOBuffer);
            cmd.SetComputeBufferParam(_foamComputeShader, drawToBufferKernel, _AOBufferID, _foamAOBuffer);

            if (WaterSystem.Test4.x >= 1)
            {
                if (_AOframeNumber == 0)
                {
                    _aoData = new float[_currentData.Length / 2];
                    _foamAOBuffer.SetData(_aoData);
                }

                cmd.SetComputeFloatParam(_foamComputeShader, "_customTime", _AOframeNumber);
                _AOframeNumber++;
                WaterSystem.Test4.x = _AOframeNumber;
                if (_AOframeNumber > 70)
                {
                    WaterSystem.Test4.x = 0;

                    _foamAOBuffer.GetData(_aoData);
                    for (int i = 0; i < _currentData.Length - 1; i += 2)
                    {
                        _currentData[i + 1] = BakeAOToData(_currentData[i + 1], _aoData[i / 2]);
                    }

                    var pathToBakedDataFolder = KW_Extensions.GetPathToStreamingAssetsFolder();
                    KW_Extensions.SaveDataToFile(_currentData, Path.Combine(pathToBakedDataFolder, _pathToShoreLineFolder, _nameFoamData));
                    Debug.Log("saved");
                    _AOframeNumber = 0;
                }
            }
#endif
            var settings = WaterInstance.Settings;
            var time = WaterInstance.UseNetworkTime ? WaterInstance.NetworkTime : KW_Extensions.TotalTime();
            var stereoPasses = WaterSystem.IsSinglePassStereoEnabled ? 2 : 1;
            if (_targetRT.rt.volumeDepth != stereoPasses) return;

            OnSetRenderTarget?.Invoke(cmd, cam, _targetRT);
            
            ///////////////////////////////////// clear pass ////////////////////////////////////////////////////////////////////////////////////////////////////
            cmd.SetComputeFloatParam(_foamComputeShader, "KW_Time", time);
            cmd.SetComputeFloatParam(_foamComputeShader, "KW_GlobalTimeScale", settings.TimeScale);

            cmd.SetComputeBufferParam(_foamComputeShader, _clearKernel, FoamBufferID, _computeFoamBuffer);
            cmd.SetComputeBufferParam(_foamComputeShader, _drawToBufferKernel, FoamBufferID, _computeFoamBuffer);
            cmd.SetComputeBufferParam(_foamComputeShader, _drawToTextureKernel, FoamBufferID, _computeFoamBuffer);

            cmd.SetComputeBufferParam(_foamComputeShader, _clearKernel, FoamLightBufferID, _computeFoamLightBuffer);
            cmd.SetComputeBufferParam(_foamComputeShader, _drawToBufferKernel, FoamLightBufferID, _computeFoamLightBuffer);
            cmd.SetComputeBufferParam(_foamComputeShader, _drawToTextureKernel, FoamLightBufferID, _computeFoamLightBuffer);

            cmd.SetComputeTextureParam(_foamComputeShader, _clearKernel, _foamRT_ID, _targetRT);
            cmd.DispatchCompute(_foamComputeShader, _clearKernel, _renderToTextureDispatchSize.x, _renderToTextureDispatchSize.y, stereoPasses);
            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


            /////////////////////////////////////////////////  RenderToBuffer //////////////////////////////////////////////////////////////////////////////////////////////////
            WaterInstance.SharedData.UpdateShaderParams(cmd, SharedData.PassEnum.VolumetricLighting, _foamComputeShader, _drawToBufferKernel);
            WaterInstance.SharedData.UpdateShaderParams(cmd, SharedData.PassEnum.FFT, _foamComputeShader, _drawToBufferKernel);

            cmd.SetComputeVectorParam(_foamComputeShader, Shader.PropertyToID("_FoamRTSize"), _targetRTSize);
            cmd.SetComputeBufferParam(_foamComputeShader, _drawToBufferKernel, "_foamDataBuffer", _foamDataBuffer);
            cmd.SetComputeBufferParam(_foamComputeShader, _drawToBufferKernel, "_foamDataCountOffsetBuffer", _foamDataCountOffsetBuffer);

            cmd.SetComputeFloatParam(_foamComputeShader, Shader.PropertyToID("_DispatchSize"), _renderToBufferDispatchSize);
            cmd.SetComputeFloatParam(_foamComputeShader, Shader.PropertyToID("_MaxParticles"), maxParticles);

            //cmd.SetComputeTextureParam(_foamComputeShader, _drawToBufferKernel, "_CameraDepthTexture", depthRT); //todo urp rendering doesn't work properly with editor camera?
            cmd.SetComputeTextureParam(_foamComputeShader, _drawToBufferKernel, _foamRT_ID, _targetRT);

            _foamComputeShader.SetKeyword(KWS_ShaderConstants.WaterKeywords.USE_MULTIPLE_SIMULATIONS, WaterInstance.UseMultipleSimulations);
            _foamComputeShader.SetKeyword(KWS_ShaderConstants.WaterKeywords.USE_VOLUMETRIC_LIGHT, WaterInstance.Settings.UseVolumetricLight);
            _foamComputeShader.SetKeyword(KWS_ShaderConstants.ShorelineKeywords.KWS_FOAM_USE_FAST_PATH, WaterInstance.Settings.UseShorelineFoamFastMode);
            _foamComputeShader.SetKeyword(KWS_ShaderConstants.ShorelineKeywords.FOAM_RECEIVE_SHADOWS, WaterInstance.Settings.ShorelineFoamReceiveDirShadows);

            _foamComputeShader.SetKeyword(KWS_ShaderConstants.FogKeywords.FOG_LINEAR, RenderSettings.fog && RenderSettings.fogMode == FogMode.Linear);
            _foamComputeShader.SetKeyword(KWS_ShaderConstants.FogKeywords.FOG_EXP, RenderSettings.fog && RenderSettings.fogMode == FogMode.Exponential);
            _foamComputeShader.SetKeyword(KWS_ShaderConstants.FogKeywords.FOG_EXP2, RenderSettings.fog && RenderSettings.fogMode == FogMode.ExponentialSquared);

          
            var lods = buffer.VisibleFoamWavesWithLods;
            var allIndexes = buffer.VisibleFoamWavesLodIndexes;
            _lodIndexesBuffer = KWS_CoreUtils.GetOrUpdateBuffer<int>(ref _lodIndexesBuffer, allIndexes.Count);
            _lodIndexesBuffer.SetData(allIndexes);
            cmd.SetComputeBufferParam(_foamComputeShader, _drawToBufferKernel, "_lodIndexes", _lodIndexesBuffer);
           
            cmd.SetComputeTextureParam(_foamComputeShader, _drawToBufferKernel, KWS_ShaderConstants.OrthoDepth.KWS_WaterOrthoDepthRT, WaterInstance.SharedData.OrthoDepth);
            cmd.SetComputeVectorParam(_foamComputeShader, KWS_ShaderConstants.OrthoDepth.KWS_OrthoDepthPos, WaterInstance.SharedData.OrthoDepthPosition);
            cmd.SetComputeVectorParam(_foamComputeShader, KWS_ShaderConstants.OrthoDepth.KWS_OrthoDepthNearFarSize, WaterInstance.SharedData.OrthoDepthNearFarSize);
            
            //WaterInstance.SetDynamicShaderParams(_foamComputeShader);
            //WaterInstance.SetConstantShaderParams(_foamComputeShader, WaterSystem.WaterTab.Shoreline);
            //cmd.SetComputeConstantBufferParam(_foamComputeShader, nameof(KWS_ShaderVariables), WaterInstance.SharedData.WaterConstantBuffer, 0, WaterInstance.SharedData.WaterConstantBuffer.stride);

            //cmd.SetComputeTextureParam(_foamComputeShader, _drawToBufferKernel, KWS_ShaderConstants.VolumetricLightConstantsID.KWS_VolumetricLightRT, WaterInstance.SharedData.VolumetricLightingRT);
            //cmd.SetComputeVectorParam(_foamComputeShader, KWS_ShaderConstants.VolumetricLightConstantsID.KWS_VolumetricLight_RTHandleScale, WaterInstance.SharedData.VolumetricLightingRTSize);

            cmd.SetComputeShadersDefaultPlatformSpecificValues(_foamComputeShader, 1);
            
            int lodOffset = 0;
            float lodSizeMultiplier = 1;
            var particlesMultiplier = KWS_Settings.Shoreline.LodParticlesMultiplier[settings.ShorelineFoamLodQuality];
            
            foreach (var lod in lods)
            {
                var lodDispatchSize = (int)(_renderToBufferDispatchSize / lodSizeMultiplier);
                if (lod.Count != 0 && lodDispatchSize > 1)
                {
                    cmd.SetComputeFloatParam(_foamComputeShader, "_lodSizeMultiplier", lodSizeMultiplier);
                    cmd.SetComputeIntParam(_foamComputeShader, "_lodOffset", lodOffset);
                    cmd.DispatchCompute(_foamComputeShader, _drawToBufferKernel, lodDispatchSize, lodDispatchSize, lod.Count);
                }
                lodOffset += lod.Count;
                lodSizeMultiplier *= particlesMultiplier;
            }
            
            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


            ///////////////////////////////////////////////// DrawFromBufferToTexture ///////////////////////////////////////////////////
            if (!settings.UseShorelineFoamFastMode)
            {
                cmd.SetComputeTextureParam(_foamComputeShader, _drawToTextureKernel, _foamRT_ID, _targetRT);
                cmd.DispatchCompute(_foamComputeShader, _drawToTextureKernel, _renderToTextureDispatchSize.x, _renderToTextureDispatchSize.y, stereoPasses);
            }
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            WaterInstance.SharedData.FoamRawRT = _targetRT;
        }

    }
}