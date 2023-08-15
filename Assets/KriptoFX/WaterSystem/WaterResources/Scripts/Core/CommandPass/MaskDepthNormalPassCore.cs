using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using static KWS.KWS_CoreUtils;
using static KWS.KWS_ShaderConstants;

namespace KWS
{
    public class MaskDepthNormalPassCore: WaterPassCore
    {
        public Action<CommandBuffer, RTHandle, RTHandle, RTHandle> OnInitializedRenderTarget;

        RTHandle _waterMaskRT;
        RTHandle _waterIdRT;
        RTHandle _waterDepthRT;
        private Dictionary<WaterSystem, Material> _materials = new Dictionary<WaterSystem, Material>();
        readonly Vector2 _rtScale = new Vector2(0.5f, 0.5f);

        public MaskDepthNormalPassCore(WaterSystem waterInstance)
        {
            PassName = "Water.MaskDepthNormalPass";
            WaterInstance = waterInstance;
            InitializeTextures();
        }

        public override void Release()
        {
            ReleaseTextures();
            foreach (var waterMaterial in _materials) KW_Extensions.SafeDestroy(waterMaterial.Value);
            _materials.Clear();

            //KW_Extensions.WaterLog(this, "Release", KW_Extensions.WaterLogMessageType.Release);
        }

        void InitializeTextures()
        {
            ReleaseTextures();

             _waterMaskRT = KWS_CoreUtils.RTHandleAllocVR(_rtScale, name: "_waterMaskRT", colorFormat: GraphicsFormat.R8G8B8A8_UNorm);
            _waterIdRT = KWS_CoreUtils.RTHandleAllocVR(_rtScale, name: "_waterIdRT", colorFormat: GraphicsFormat.R8_UNorm);
            _waterDepthRT = KWS_CoreUtils.RTHandleAllocVR(_rtScale, name: "_waterDepthRT", depthBufferBits: DepthBits.Depth24);

            //KW_Extensions.WaterLog(this, _waterMaskRT, _waterDepthRT);
        }

        Material GetMaterial(WaterSystem currentInstance)
        {
            if (!_materials.ContainsKey(currentInstance))
            {
                var newMat = KWS_CoreUtils.CreateMaterial(KWS_ShaderConstants.ShaderNames.WaterShaderName);
                currentInstance.AddShaderToWaterRendering(newMat);
                _materials.Add(currentInstance, newMat);

            }
            return _materials[currentInstance];
        }


        public void Execute(Camera cam, CommandBuffer cmd)
        {
            var waterInstances = WaterSystem.VisibleWaterInstances;
           
            OnInitializedRenderTarget?.Invoke(cmd, _waterMaskRT, _waterIdRT, _waterDepthRT);
          
            foreach (var instance in waterInstances)
            {
                ExecuteInstance(cam, cmd, instance);
            }

            Shader.SetGlobalTexture(MaskPassID.KW_WaterDepth_ID, _waterDepthRT.rt);
            Shader.SetGlobalTexture(MaskPassID.KWS_WaterTexID, _waterIdRT.rt);
            Shader.SetGlobalTexture(MaskPassID.KW_WaterMaskScatterNormals_ID, _waterMaskRT.rt);
            Shader.SetGlobalVector(MaskPassID.KWS_WaterMask_RTHandleScale, _waterMaskRT.rtHandleProperties.rtHandleScale);
        }

        public void ExecuteInstance(Camera cam, CommandBuffer cmd, WaterSystem currentInstance)
        {
            if (!currentInstance.Settings.EnabledMeshRendering) return;
            if (!CanBeRenderCurrentWaterInstance(currentInstance)) return;

            if (!currentInstance.Settings.EnabledMeshRendering && !currentInstance.Settings.UseCausticEffect) return;
            if (!currentInstance.Settings.UseVolumetricLight
             && !currentInstance.Settings.UseCausticEffect
             && !currentInstance.Settings.UseUnderwaterEffect
             && !currentInstance.Settings.UseScreenSpaceReflection) return;

            
            cmd.SetGlobalVector(DynamicWaterParams.KW_WaterPosition, currentInstance.WaterRelativeWorldPosition);
            cmd.SetGlobalFloat(DynamicWaterParams.KW_Time, currentInstance.UseNetworkTime ? currentInstance.NetworkTime : KW_Extensions.TotalTime());
            cmd.SetGlobalInt(DynamicWaterParams.KWS_WaterPassID, currentInstance.SharedData.WaterShaderPassID);
            cmd.SetGlobalInt(DynamicWaterParams.KWS_UnderwaterVisible, currentInstance.IsCameraUnderwater ? 1 : 0);

            currentInstance.SharedData.UpdateShaderParams(cmd, SharedData.PassEnum.FFT);
            if (currentInstance.Settings.UseFlowMap) currentInstance.SharedData.UpdateShaderParams(cmd,               SharedData.PassEnum.Flowing);
            if (currentInstance.Settings.UseFluidsSimulation) currentInstance.SharedData.UpdateShaderParams(cmd,      SharedData.PassEnum.FluidsSimulation);
            if (currentInstance.Settings.UseDynamicWaves) currentInstance.SharedData.UpdateShaderParams(cmd,          SharedData.PassEnum.DynamicWaves);
            if (currentInstance.Settings.UseShorelineRendering) currentInstance.SharedData.UpdateShaderParams(cmd,    SharedData.PassEnum.Shoreline);

            switch (currentInstance.Settings.WaterMeshType)
            {
                case WaterSystem.WaterMeshTypeEnum.InfiniteOcean:
                case WaterSystem.WaterMeshTypeEnum.FiniteBox:
                    DrawInstancedQuadTree(cam, cmd, currentInstance);
                    break;
                case WaterSystem.WaterMeshTypeEnum.CustomMesh:
                    DrawCustomMesh(cmd, currentInstance);
                    break;
                case WaterSystem.WaterMeshTypeEnum.River:
                    DrawRiverSpline(cmd, currentInstance);
                    break;
            }
        }

        private void DrawInstancedQuadTree(Camera cam, CommandBuffer cmd, WaterSystem currentInstance)
        {
            var data = currentInstance.GetQuadTreeChunksData();
            if (!data.CanRender) return;

            var instancedMesh = data.Instances[data.ActiveInstanceIndex];
            cmd.SetGlobalBuffer(StructuredBuffers.InstancedMeshData, data.VisibleChunksComputeBuffer);

            var waterMaterial = GetMaterial(currentInstance);
           
            var shaderPass = currentInstance.CanRenderTesselation ? (int)WaterMaterialPasses.MaskDepthNormal_QuadTreeTesselated : (int)WaterMaterialPasses.MaskDepthNormal_QuadTree;
            if (currentInstance.IsCameraUnderwater)
            {
                var args = data.InstancesArgs[data.ActiveInstanceIndex];
                cmd.DrawMeshInstancedIndirect(instancedMesh, 0, waterMaterial, shaderPass, args);

                var size = (currentInstance.Settings.WaterMeshType == WaterSystem.WaterMeshTypeEnum.FiniteBox) ? currentInstance.Settings.MeshSize : cam.farClipPlane * new Vector3(1, 0.5f, 1);
                var matrix = Matrix4x4.TRS(currentInstance.WaterRelativeWorldPosition + Vector3.up * 0.01f, currentInstance.WaterPivotWorldRotation, size);
                cmd.DrawMesh(data.BottomUnderwaterSkirt, matrix, waterMaterial, 0, (int)WaterMaterialPasses.MaskDepthNormal_Side);

            }
            else
            {
                var optimizedLevel = Mathf.Max(0, data.ActiveInstanceIndex - 4);
                var args = data.InstancesArgs[optimizedLevel];
                var mesh = data.Instances[optimizedLevel];

                cmd.DrawMeshInstancedIndirect(mesh, 0, waterMaterial, shaderPass, args);

                if (currentInstance.Settings.WaterMeshType == WaterSystem.WaterMeshTypeEnum.FiniteBox)
                {
                    if (data.BottomUnderwaterSkirt == null) return;

                    var matrix = Matrix4x4.TRS(currentInstance.WaterRelativeWorldPosition + Vector3.up * 0.01f, currentInstance.WaterPivotWorldRotation, currentInstance.Settings.MeshSize);
                    cmd.DrawMesh(data.BottomUnderwaterSkirt, matrix, waterMaterial, 0, (int)WaterMaterialPasses.MaskDepthNormal_Side);
                }
            }
        }

        private void DrawCustomMesh(CommandBuffer cmd, WaterSystem currentInstance)
        {
            var mesh = currentInstance.Settings.CustomMesh;
            if (mesh == null) return;
            var matrix = Matrix4x4.TRS(currentInstance.WaterPivotWorldPosition, currentInstance.WaterPivotWorldRotation, currentInstance.Settings.MeshSize);
            var shaderPass = currentInstance.CanRenderTesselation ? (int)WaterMaterialPasses.MaskDepthNormal_CustomOrRiverTesselated : (int)WaterMaterialPasses.MaskDepthNormal_CustomOrRiver;
            cmd.DrawMesh(mesh, matrix, GetMaterial(currentInstance), 0, shaderPass);
        }

        private void DrawRiverSpline(CommandBuffer cmd, WaterSystem currentInstance)
        {
            var mesh = currentInstance.SplineRiverMesh;
            if (mesh == null) return;
            var matrix = Matrix4x4.TRS(currentInstance.WaterPivotWorldPosition, Quaternion.identity, Vector3.one);
            var shaderPass = currentInstance.CanRenderTesselation ? (int)WaterMaterialPasses.MaskDepthNormal_CustomOrRiverTesselated : (int)WaterMaterialPasses.MaskDepthNormal_CustomOrRiver;
            cmd.DrawMesh(mesh, matrix, GetMaterial(currentInstance), 0, shaderPass);
        }

        void ReleaseTextures()
        {
            if(_waterMaskRT !=null) _waterMaskRT.Release();
            if(_waterDepthRT != null) _waterDepthRT.Release();
            if(_waterIdRT != null) _waterIdRT.Release();
        }

    }
}