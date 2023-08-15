using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;


namespace KWS
{
    public class ShorelineWavesPassCore: WaterPassCore
    {
        public Action<CommandBuffer, Camera, RTHandle, RTHandle> OnSetRenderTarget;

        private ComputeBuffer _meshPropertiesBuffer;
        private ComputeBuffer _argsBuffer;
        private ComputeBuffer _computeFoamBuffer;

        private Mesh _instancedQuadMesh;
        private Material _waveMaterial;
        private RTHandle _displacementRT, _normalRT;

        private int ID_ShorelineDisplacement = Shader.PropertyToID("KWS_ShorelineDisplacement");
        private int ID_ShorelineNormal = Shader.PropertyToID("KWS_ShorelineNormal");
        private int ID_ShorelineAlpha = Shader.PropertyToID("KWS_ShorelineAlpha");


        private Texture2D _shorelineDisplacementTex;
        private Texture2D _shorelineNormalTex;
        private Texture2D _shorelineAlphaTex;


        public ShorelineWavesPassCore(WaterSystem waterInstance)
        {
            PassName = "Water.ShorelineWavesPass";
            WaterInstance = waterInstance;
            WaterSystem.OnWaterSettingsChanged += OnWaterSettingsChanged;

            InitializeTextures(KWS_Settings.Shoreline.ShorelineWavesTextureResolution); 
            InitializeMaterials();
        }

        private void OnWaterSettingsChanged(WaterSystem waterInstance, WaterSystem.WaterTab changedTab)
        {
            if (!changedTab.HasFlag(WaterSystem.WaterTab.Shoreline)) return;

        }

        public override void Release()
        {
            WaterSystem.OnWaterSettingsChanged -= OnWaterSettingsChanged;
            if (_meshPropertiesBuffer != null) _meshPropertiesBuffer.Release();
            _meshPropertiesBuffer = null;

            if (_argsBuffer != null) _argsBuffer.Release();
            _argsBuffer = null;

            if (_computeFoamBuffer != null) _computeFoamBuffer.Dispose();
            _computeFoamBuffer   = null;

            _displacementRT?.Release();
            _normalRT?.Release();

            _displacementRT = null;
            _normalRT = null;

            Resources.UnloadAsset(_shorelineDisplacementTex);
            Resources.UnloadAsset(_shorelineNormalTex);
            Resources.UnloadAsset(_shorelineAlphaTex);
            KW_Extensions.SafeDestroy(_instancedQuadMesh, _waveMaterial);
            //KW_Extensions.WaterLog(this, "Release", KW_Extensions.WaterLogMessageType.Release);
        }
        void InitializeTextures(int size)
        {
            if (_displacementRT == null)
            {
                _displacementRT = WaterSystem.RTHandles.Alloc(size, size, colorFormat: GraphicsFormat.R16G16B16A16_SFloat, name : "_displacement");
                _normalRT = WaterSystem.RTHandles.Alloc(size, size, colorFormat : GraphicsFormat.R16G16_SFloat, name: "_norm");
                WaterInstance.SharedData.ShorelineWavesDisplacement = _displacementRT;
                WaterInstance.SharedData.ShorelineWavesNormal = _normalRT;
            }

            if (_shorelineDisplacementTex == null) _shorelineDisplacementTex = Resources.Load<Texture2D>(KWS_Settings.ResourcesPaths.ShorelinePos);
            if (_shorelineNormalTex == null) _shorelineNormalTex = Resources.Load<Texture2D>(KWS_Settings.ResourcesPaths.ShorelineNorm);
            if (_shorelineAlphaTex == null) _shorelineAlphaTex = Resources.Load<Texture2D>(KWS_Settings.ResourcesPaths.ShorelineAlpha);
        }

        void InitializeMaterials()
        {
            if (_waveMaterial == null)
            {
                _waveMaterial = KWS_CoreUtils.CreateMaterial(KWS_ShaderConstants.ShaderNames.ShorelineBakedWavesShaderName);
                WaterInstance.AddShaderToWaterRendering(_waveMaterial);
            }
        }

        public void Execute(Camera cam, CommandBuffer cmd)
        {
            if (!WaterInstance.Settings.UseShorelineRendering) return;
            if (!WaterInstance.Settings.EnabledMeshRendering) return;
            if (!KWS_CoreUtils.CanBeRenderCurrentWaterInstance(WaterInstance)) return;

            var buffers = WaterInstance.SharedData.ShorelineWaveBuffers;
            if (buffers == null) return;

            if (buffers.SurfaceWavesComputeBuffer == null || buffers.VisibleSurfaceWaves.Count == 0) return;

            if (_instancedQuadMesh == null) _instancedQuadMesh = KWS_CoreUtils.CreateQuad();

            var pos = buffers.SurfaceWavesAreaPos;
            var posSize = new Vector4(pos.x, pos.y, pos.z, KWS_Settings.Shoreline.ShorelineWavesAreaSize);
           
            _waveMaterial.SetVector(KWS_ShaderConstants.ShorelineID.KWS_ShorelineAreaPosSize, posSize);
            _waveMaterial.SetTexture(ID_ShorelineDisplacement, _shorelineDisplacementTex);
            _waveMaterial.SetTexture(ID_ShorelineNormal, _shorelineNormalTex);
            _waveMaterial.SetTexture(ID_ShorelineAlpha, _shorelineAlphaTex);
            _waveMaterial.SetBuffer(KWS_ShaderConstants.StructuredBuffers.KWS_ShorelineDataBuffer, buffers.SurfaceWavesComputeBuffer);
           
            _argsBuffer = MeshUtils.InitializeInstanceArgsBuffer(_instancedQuadMesh, buffers.VisibleSurfaceWaves.Count, _argsBuffer);

            OnSetRenderTarget?.Invoke(cmd, cam, _displacementRT, _normalRT);
            cmd.DrawMeshInstancedIndirect(_instancedQuadMesh, 0, _waveMaterial, 0, _argsBuffer);

            WaterInstance.SharedData.ShorelineAreaPosSize = posSize;
        }

    
    }
}