using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using static KWS.KWS_CoreUtils;
using static KWS.KWS_ShaderConstants;

namespace KWS
{
    public class CausticPassCore: WaterPassCore
    {
        public Action<CommandBuffer, RTHandle> OnRenderToCausticTarget;
        public Action<CommandBuffer> OnRenderToCameraTarget;

        private Dictionary<WaterSystem, CausticData> _causticDatas = new Dictionary<WaterSystem, CausticData>();

        class CausticData
        {
            public Material PassMaterial;
            public Material DecalMaterial;

            public RTHandle[] Lods = new RTHandle[4];
         
            public void InitializeMaterials()
            {
                PassMaterial = CreateMaterial(ShaderNames.CausticComputeShaderName);
                DecalMaterial = CreateMaterial(ShaderNames.CausticDecalShaderName);
            }

            public void InitializeTextures(WaterSystem waterInstance)
            {
                var size = waterInstance.Settings.CausticTextureSize;
                for (var i = 0; i < waterInstance.Settings.CausticActiveLods; i++)
                {
                    Lods[i] = WaterSystem.RTHandles.Alloc(size, size, colorFormat: GraphicsFormat.R8_UNorm, name: "_causticLod" + i, useMipMap: true, autoGenerateMips: true);
                    waterInstance.SharedData.CausticLods[i] = Lods[i];
                }
                //KW_Extensions.WaterLog(this, Lods[0], Lods[1], Lods[2], Lods[3]);
            }

            public void Release()
            {
                KW_Extensions.SafeDestroy(PassMaterial, DecalMaterial, Lods[0], Lods[1], Lods[2], Lods[3]);
            }
        }

        private Mesh _causticMesh;
        private Mesh _decalMesh;

        public CausticPassCore(WaterSystem waterInstance)
        {
            PassName = "Water.CausticPass";
            WaterInstance = waterInstance;
            OnWaterSettingsChanged(waterInstance, WaterSystem.WaterTab.All);
            WaterSystem.OnWaterSettingsChanged += OnWaterSettingsChanged;
        }

        public override void Release()
        {
            WaterSystem.OnWaterSettingsChanged -= OnWaterSettingsChanged;
            KW_Extensions.SafeDestroy(_causticMesh, _decalMesh);

            foreach (var causticData in _causticDatas) causticData.Value.Release();
            _causticDatas.Clear();

           // KW_Extensions.WaterLog(this, "Release", KW_Extensions.WaterLogMessageType.Release);
        }

        private void OnWaterSettingsChanged(WaterSystem waterInstance, WaterSystem.WaterTab changedTab)
        {
            if (!changedTab.HasFlag(WaterSystem.WaterTab.Caustic)) return;

            _causticMesh = GeneratePlane(_causticMesh, waterInstance.Settings.CausticMeshResolution, 1.2f);
            foreach (var causticData in _causticDatas) causticData.Value.Release();
            _causticDatas.Clear();

        }

        CausticData GetCausticData(WaterSystem waterInstance)
        {
            if (!_causticDatas.ContainsKey(waterInstance))
            {
                var cm = new CausticData();
                cm.InitializeMaterials();
                cm.InitializeTextures(waterInstance);

                waterInstance.AddShaderToWaterRendering(cm.PassMaterial);
                waterInstance.AddShaderToWaterRendering(cm.DecalMaterial);

                _causticDatas.Add(waterInstance, cm);
               
            }
            return _causticDatas[waterInstance];
        }

        public void Execute(Camera cam, CommandBuffer cmd)
        {
            var waterInstances = WaterSystem.VisibleWaterInstances;
            foreach (var instance in waterInstances)
            {
                ExecuteInstance(cam, cmd, instance);
            }
        }

        public void ExecuteInstance(Camera cam, CommandBuffer cmd, WaterSystem waterInstance)
        {
            if (!waterInstance.Settings.UseCausticEffect) return;
            if (!CanBeRenderCurrentWaterInstance(waterInstance)) return;

            ComputeCaustic(cam, cmd, waterInstance);
            DrawCausticDecal(cam, cmd, waterInstance);
        }

        void ComputeCaustic(Camera cam, CommandBuffer cmd, WaterSystem waterInstance)
        {
            var causticData = GetCausticData(waterInstance);
          
            var camPos = KW_Extensions.GetCameraPositionFast(cam);
            var camDir = KW_Extensions.GetCameraForwardFast(cam);

            cmd.SetKeyword(CausticKeywords.USE_DEPTH_SCALE, waterInstance.Settings.UseDepthCausticScale);
            waterInstance.SharedData.UpdateShaderParams(cmd, SharedData.PassEnum.FFT);
           
            cmd.SetKeyword(CausticKeywords.USE_CAUSTIC_FILTERING, waterInstance.Settings.UseCausticBicubicInterpolation);
            cmd.SetGlobalFloat(CausticID.KW_CaustisStrength, waterInstance.Settings.CausticStrength);
            cmd.SetGlobalFloat(CausticID.KW_CausticDepthScale, waterInstance.Settings.CausticDepthScale);

            if (waterInstance.Settings.UseShorelineRendering) waterInstance.SharedData.UpdateShaderParams(cmd, SharedData.PassEnum.Shoreline);

            var lodSettings = KWS_Settings.Caustic.LodSettings;
            RenderLod(waterInstance, causticData.PassMaterial, cmd, camPos, camDir, lodSettings.x, causticData.Lods[0]);
            if (waterInstance.Settings.CausticActiveLods > 1) RenderLod(waterInstance, causticData.PassMaterial, cmd, camPos, camDir, lodSettings.y, causticData.Lods[1]);
            if (waterInstance.Settings.CausticActiveLods > 2) RenderLod(waterInstance, causticData.PassMaterial, cmd, camPos, camDir, lodSettings.z, causticData.Lods[2]);
            if (waterInstance.Settings.CausticActiveLods > 3) RenderLod(waterInstance, causticData.PassMaterial, cmd, camPos, camDir, lodSettings.w, causticData.Lods[3]);

            waterInstance.SharedData.CausticActiveLods = waterInstance.Settings.CausticActiveLods;
        }

        void RenderLod(WaterSystem waterInstance, Material passMaterial, CommandBuffer cmd, Vector3 camPos, Vector3 camDir, float lodSize, RTHandle target)
        {
            var bakeCamPos = camPos + camDir * lodSize * 0.5f;

            cmd.SetGlobalFloat(CausticID.KW_MeshScale, lodSize);
            cmd.SetGlobalVector(CausticID.KW_CausticCameraOffset, bakeCamPos);

            OnRenderToCausticTarget?.Invoke(cmd, target);

            cmd.DrawMesh(_causticMesh, Matrix4x4.identity, passMaterial, 0);
        }

        void DrawCausticDecal(Camera cam, CommandBuffer cmd, WaterSystem waterInstance)
        {
            var camPos = KW_Extensions.GetCameraPositionFast(cam);
            var causticData = GetCausticData(waterInstance);
           
            var decalScale = KWS_Settings.Caustic.LodSettings[waterInstance.Settings.CausticActiveLods - 1] * 2;
            var decalPos = camPos;
            decalPos.y = waterInstance.WaterRelativeWorldPosition.y - (KWS_Settings.Caustic.CausticDecalHeight * 0.5f - 2);
            
            var decalMaterial = causticData.DecalMaterial;

            cmd.SetGlobalVector(CausticID.KW_CausticLodOffset, KW_Extensions.GetCameraForwardFast(cam) * 0.5f);
            cmd.SetGlobalFloat(CausticID.KW_CaustisStrength, waterInstance.Settings.CausticStrength);
            if (waterInstance.Settings.UseCausticDispersion)
            {
                var dispersionStrength = 1 - (Mathf.RoundToInt(Mathf.Log((int)waterInstance.Settings.FFT_SimulationSize, 2)) - 5) / 4.0f; // 0 - 4 => 1-0
                if (dispersionStrength > 0.1f)
                {
                    dispersionStrength = Mathf.Lerp(dispersionStrength * 0.5f, dispersionStrength * 2, waterInstance.Settings.CausticTextureSize / 1024f);
                    decalMaterial.SetFloat(CausticID.KW_CausticDispersionStrength, dispersionStrength);
                }
            }
            cmd.SetKeyword(CausticKeywords.USE_DISPERSION, waterInstance.Settings.UseCausticDispersion);
            waterInstance.SharedData.UpdateShaderParams(cmd, SharedData.PassEnum.Caustic);
            cmd.SetGlobalInt(DynamicWaterParams.KWS_WaterPassID, waterInstance.SharedData.WaterShaderPassID);

            if (waterInstance.Settings.UseDynamicWaves) waterInstance.SharedData.UpdateShaderParams(cmd, SharedData.PassEnum.DynamicWaves);

            var decalTRS = Matrix4x4.TRS(decalPos, Quaternion.identity, new Vector3(decalScale, KWS_Settings.Caustic.CausticDecalHeight, decalScale));
            if (_decalMesh == null) GenerateDecalMesh();
           
            OnRenderToCameraTarget?.Invoke(cmd);
            cmd.DrawMesh(_decalMesh, decalTRS, decalMaterial);

        }

        Mesh GeneratePlane(Mesh mesh, int meshResolution, float scale)
        {
            if (mesh == null)
            {
                mesh = new Mesh();
                mesh.hideFlags = HideFlags.DontSave;
                mesh.indexFormat = IndexFormat.UInt32;
            }

            var vertices = new Vector3[(meshResolution + 1) * (meshResolution + 1)];
            var uv = new Vector2[vertices.Length];
            var triangles = new int[meshResolution * meshResolution * 6];

            for (int i = 0, y = 0; y <= meshResolution; y++)
                for (var x = 0; x <= meshResolution; x++, i++)
                {
                    vertices[i] = new Vector3(x * scale / meshResolution - 0.5f * scale, y * scale / meshResolution - 0.5f * scale, 0);
                    uv[i] = new Vector2(x * scale / meshResolution, y * scale / meshResolution);
                }

            for (int ti = 0, vi = 0, y = 0; y < meshResolution; y++, vi++)
                for (var x = 0; x < meshResolution; x++, ti += 6, vi++)
                {
                    triangles[ti] = vi;
                    triangles[ti + 3] = triangles[ti + 2] = vi + 1;
                    triangles[ti + 4] = triangles[ti + 1] = vi + meshResolution + 1;
                    triangles[ti + 5] = vi + meshResolution + 2;
                }

            mesh.Clear();
            mesh.vertices = vertices;
            mesh.uv = uv;
            mesh.triangles = triangles;
            return mesh;
        }

        void GenerateDecalMesh()
        {
            Vector3[] vertices =
            {
                new Vector3(-0.5f, -0.5f, -0.5f),
                new Vector3(0.5f, -0.5f, -0.5f),
                new Vector3(0.5f, 0.5f, -0.5f),
                new Vector3(-0.5f, 0.5f, -0.5f),
                new Vector3(-0.5f, 0.5f, 0.5f),
                new Vector3(0.5f, 0.5f, 0.5f),
                new Vector3(0.5f, -0.5f, 0.5f),
                new Vector3(-0.5f, -0.5f, 0.5f),
            };

            int[] triangles =
            {
                0, 2, 1, //face front
                0, 3, 2,
                2, 3, 4, //face top
                2, 4, 5,
                1, 2, 5, //face right
                1, 5, 6,
                0, 7, 4, //face left
                0, 4, 3,
                5, 4, 7, //face back
                5, 7, 6,
                0, 6, 7, //face bottom
                0, 1, 6
            };

            if (_decalMesh == null)
            {
                _decalMesh = new Mesh();
                _decalMesh.hideFlags = HideFlags.DontSave;
            }

            _decalMesh.Clear();
            _decalMesh.vertices = vertices;
            _decalMesh.triangles = triangles;
            _decalMesh.RecalculateNormals();
        }
    }
}