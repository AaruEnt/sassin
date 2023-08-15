using UnityEngine;
using UnityEngine.Rendering;
using static KWS.KWS_CoreUtils;
using static KWS.KWS_ShaderConstants;

namespace KWS
{
    public class DrawMeshPassCore: WaterPassCore
    {
        private Material _waterMaterial;

        public DrawMeshPassCore(WaterSystem waterInstance)
        {
            PassName = "Water.DrawWaterMesh";
            WaterInstance = waterInstance;
            InitializeMaterial();
        }

        public override void Release()
        {
            KW_Extensions.SafeDestroy(_waterMaterial);

            //KW_Extensions.WaterLog(this, "Release", KW_Extensions.WaterLogMessageType.Release);
        }

        void InitializeMaterial()
        {
            if (_waterMaterial == null)
            {
                _waterMaterial = KWS_CoreUtils.CreateMaterial(KWS_ShaderConstants.ShaderNames.WaterShaderName);
                WaterInstance.AddShaderToWaterRendering(_waterMaterial);
            }
        }
        public void Execute(Camera cam, CommandBuffer cmd)
        {
           
            if (!WaterInstance.Settings.EnabledMeshRendering) return;
            if (!CanBeRenderCurrentWaterInstance(WaterInstance)) return;

            UpdateMaterialParams(_waterMaterial, cmd); 
            switch (WaterInstance.Settings.WaterMeshType)
            {
                case WaterSystem.WaterMeshTypeEnum.InfiniteOcean:
                case WaterSystem.WaterMeshTypeEnum.FiniteBox:
                    DrawInstancedQuadTree(cmd);
                    break;
                case WaterSystem.WaterMeshTypeEnum.CustomMesh:
                    DrawCustomMesh(cmd);
                    break;
                case WaterSystem.WaterMeshTypeEnum.River:
                    DrawRiverSpline(cmd);
                    break;
            }
        }

        private void DrawInstancedQuadTree(CommandBuffer cmd)
        {
            var data          = WaterInstance.GetQuadTreeChunksData();
            if (!data.CanRender) return;

            var instancedMesh = data.Instances[data.ActiveInstanceIndex];
            var args          = data.InstancesArgs[data.ActiveInstanceIndex];
            _waterMaterial.SetBuffer(StructuredBuffers.InstancedMeshData, data.VisibleChunksComputeBuffer);

            var shaderPass = WaterInstance.CanRenderTesselation ? (int)WaterMaterialPasses.QuadTree_Tesselated : (int)WaterMaterialPasses.QuadTree;
            cmd.DrawMeshInstancedIndirect(instancedMesh, 0, _waterMaterial, shaderPass, args);

            if (WaterInstance.Settings.WaterMeshType == WaterSystem.WaterMeshTypeEnum.FiniteBox)
            {
                //var underwaterMesh = data.UnderwaterMeshes[data.ActiveInstanceIndex];
                if (data.BottomUnderwaterSkirt == null) return;

                var matrix = Matrix4x4.TRS(WaterInstance.WaterPivotWorldPosition + Vector3.up * 0.01f, WaterInstance.WaterPivotWorldRotation, WaterInstance.Settings.MeshSize);
                cmd.DrawMesh(data.BottomUnderwaterSkirt, matrix, _waterMaterial, 0, (int)WaterMaterialPasses.WaterSide);
            }
        }

        private void DrawCustomMesh(CommandBuffer cmd)
        {
            var mesh = WaterInstance.Settings.CustomMesh;
            if (mesh == null) return;
            var matrix = Matrix4x4.TRS(WaterInstance.WaterPivotWorldPosition, WaterInstance.WaterPivotWorldRotation, WaterInstance.Settings.MeshSize);
            var shaderPass = WaterInstance.CanRenderTesselation ? (int)WaterMaterialPasses.CustomOrRiver_Tesselated : (int)WaterMaterialPasses.CustomOrRiver;
            cmd.DrawMesh(mesh, matrix, _waterMaterial, 0, shaderPass);
        }

        private void DrawRiverSpline(CommandBuffer cmd)
        {
            var mesh = WaterInstance.SplineRiverMesh;
            if (mesh == null) return;
            var matrix        = Matrix4x4.TRS(WaterInstance.WaterPivotWorldPosition, Quaternion.identity, Vector3.one);

            var shaderPass = WaterInstance.CanRenderTesselation ? (int)WaterMaterialPasses.CustomOrRiver_Tesselated : (int)WaterMaterialPasses.CustomOrRiver;
            cmd.DrawMesh(mesh, matrix, _waterMaterial, 0, shaderPass);
        }
            
        private void UpdateMaterialParams(Material mat, CommandBuffer cmd)
        {
            var settings = WaterInstance.Settings;
            var sharedData = WaterInstance.SharedData;
            
            cmd.SetGlobalVector(DynamicWaterParams.KW_WaterPosition, WaterInstance.WaterRelativeWorldPosition);
            cmd.SetGlobalFloat(DynamicWaterParams.KW_Time, WaterInstance.UseNetworkTime ? WaterInstance.NetworkTime : KW_Extensions.TotalTime());
            cmd.SetGlobalInt(DynamicWaterParams.KWS_WaterPassID, sharedData.WaterShaderPassID);
            cmd.SetGlobalInt(DynamicWaterParams.KWS_UnderwaterVisible, WaterInstance.IsCameraUnderwater ? 1 : 0);
            
            WaterInstance.SharedData.UpdateShaderParams(cmd, SharedData.PassEnum.FFT);
            WaterInstance.SharedData.UpdateShaderParams(cmd, SharedData.PassEnum.CubemapReflection);

            if (settings.UseScreenSpaceReflection) WaterInstance.SharedData.UpdateShaderParams(cmd, SharedData.PassEnum.SSR);
            if (settings.UsePlanarReflection) WaterInstance.SharedData.UpdateShaderParams(cmd, SharedData.PassEnum.PlanarReflection);
            if (settings.UseFlowMap) WaterInstance.SharedData.UpdateShaderParams(cmd, SharedData.PassEnum.Flowing);
            if (settings.UseFluidsSimulation) WaterInstance.SharedData.UpdateShaderParams(cmd, SharedData.PassEnum.FluidsSimulation);
            if (settings.UseDynamicWaves) WaterInstance.SharedData.UpdateShaderParams(cmd, SharedData.PassEnum.DynamicWaves);
            if (settings.UseShorelineRendering) WaterInstance.SharedData.UpdateShaderParams(cmd, SharedData.PassEnum.Shoreline);
            if (settings.UseVolumetricLight) WaterInstance.SharedData.UpdateShaderParams(cmd, SharedData.PassEnum.VolumetricLighting);
            if (settings.UseCausticEffect) WaterInstance.SharedData.UpdateShaderParams(cmd, SharedData.PassEnum.Caustic);
        }
    }
}