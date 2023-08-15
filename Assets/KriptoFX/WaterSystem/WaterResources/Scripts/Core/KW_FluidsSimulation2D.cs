using System.IO;

using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using static KWS.KWS_CoreUtils;

namespace KWS
{
    public class KW_FluidsSimulation2D 
    {
        FluidsData[] fluidsData = new FluidsData[4];

        Texture2D foamTex;
        public TemporaryRenderTexture prebakedFluidsRT = new TemporaryRenderTexture();

        Material fluidsMaterial;
        private Vector3? lastPosition = null;
        private Vector3? lastPosition_lod1;
        private int frameNumber;
        int lastWidth;
        int lastHeight;
        float currentJitterOffsetTime;

        private const string fluidSimulationShaderName = "Hidden/KriptoFX/KWS/FluidSimulation";
   

        private int ID_KW_FluidsDepth = Shader.PropertyToID("KW_FluidsDepthTex");
        private int ID_KW_FluidsDepthOrthoSize = Shader.PropertyToID("KW_FluidsDepthOrthoSize");
        private int ID_KW_FluidsDepthNearFarDistance = Shader.PropertyToID("KW_FluidsDepth_Near_Far_Dist");
        private int ID_KW_FluidsDepthPos = Shader.PropertyToID("KW_FluidsDepthPos");

        private const int nearPlaneDepth = -2;
        private const int farPlaneDepth = 100;

       

        class FluidsData
        {
            public TemporaryRenderTexture DataRT = new TemporaryRenderTexture();
            public TemporaryRenderTexture FoamRT = new TemporaryRenderTexture();
            public RenderBuffer[] MRT = new RenderBuffer[2];
        }

        void InitializeTextures(int width, int height)
        {
            for (int i = 0; i < 4; i++)
            {
                if (fluidsData[i] == null) fluidsData[i] = new FluidsData();

                fluidsData[i].DataRT.Alloc("fluidsDataRT", width, height, 0, GraphicsFormat.R16G16B16A16_SFloat);
                fluidsData[i].FoamRT.Alloc("fluidsFoamRT", width, height, 0, GraphicsFormat.R8_UNorm);
                KWS_CoreUtils.ClearRenderTexture(fluidsData[i].DataRT.rt, ClearFlag.Color, Color.black);
                KWS_CoreUtils.ClearRenderTexture(fluidsData[i].FoamRT.rt, ClearFlag.Color, Color.black);
                fluidsData[i].MRT[0] = fluidsData[i].DataRT.rt.colorBuffer;
                fluidsData[i].MRT[1] = fluidsData[i].FoamRT.rt.colorBuffer;
            }
            lastWidth = width;
            lastHeight = height;
            frameNumber = 0;
        }

        void SetShaderParams(WaterSystem waterInstance, Material mat)
        {
            var flowingData = waterInstance.Settings.FlowingScriptableData;
            if (flowingData == null || mat == null) return;

            if (flowingData.FlowmapTexture != null)
            {
                mat.SetTexture(KWS_ShaderConstants.FlowmapID.KW_FlowMapTex, waterInstance.Settings.FlowingScriptableData.FlowmapTexture);
            }

            if (flowingData.FluidsMaskTexture != null)
            {
                mat.SetTexture(ID_KW_FluidsDepth, flowingData.FluidsMaskTexture);
            }

            if (flowingData.FluidsPrebakedTexture != null)
            {
                mat.SetTexture("KW_FluidsPrebaked", flowingData.FluidsPrebakedTexture);
            }
        }

        void LoadFoamTexture()
        {
            if (foamTex == null)
            {
                foamTex = Resources.Load<Texture2D>(KWS_Settings.ResourcesPaths.KW_Foam1);
                Shader.SetGlobalTexture("KW_FluidsFoamTex", foamTex);
            }
        }

        //private void LoadDepthTexture(WaterSystem waterInstance)
        //{
        //    var flowingData = waterInstance.Settings.FlowingScriptableData;
        //    if (flowingData == null || flowingData.FluidsMaskTexture == null || flowingData.FluidsPrebakedTexture == null) return;

        //    waterInstance.SetTextures(false, (ID_KW_FluidsDepth, flowingData.FluidsMaskTexture));
        //    waterInstance.SetFloats(false, (ID_KW_FluidsDepthOrthoSize, flowingData.AreaSize));
        //    waterInstance.SetVectors(false, (ID_KW_FluidsDepthNearFarDistance, new Vector3(nearPlaneDepth, farPlaneDepth, farPlaneDepth - nearPlaneDepth)));
        //    waterInstance.SetVectors(false, (ID_KW_FluidsDepthPos, flowingData.AreaPosition));
        //}

        public void Release()
        {
            for (int i = 0; i < 4; i++)
            {
                if (fluidsData[i] != null)
                {
                    fluidsData[i].DataRT.Release(true);
                    fluidsData[i].FoamRT.Release(true);
                }
            }

            prebakedFluidsRT.Release(true);
            KW_Extensions.SafeDestroy(fluidsMaterial);
            Resources.UnloadAsset(foamTex);
            foamTex = null;
            lastPosition = null;
            lastWidth = 0;
            lastHeight = 0;
            frameNumber = 0;
        }

        public void InitializeMaterials(WaterSystem waterInstance)
        {
            if (fluidsMaterial == null)
            {
                fluidsMaterial = KWS_CoreUtils.CreateMaterial(fluidSimulationShaderName);
                waterInstance.AddShaderToWaterRendering(fluidsMaterial);
            }
           
        }

        Vector3 ComputeAreaSimulationJitter(float offsetX, float offsetZ)
        {
            var jitterSin = Mathf.Sin(currentJitterOffsetTime);
            var jitterCos = Mathf.Cos(currentJitterOffsetTime);
            var jitter = new Vector3(offsetX * jitterSin, 0, offsetZ * jitterCos);
            currentJitterOffsetTime += 2.0f;
            return jitter;
        }

        Vector3 RayToWaterPos(Camera currentCamera, float height)
        {
            var ray = currentCamera.ViewportPointToRay(new Vector3(0.5f, 0.0f, 0));
            var plane = new Plane(Vector3.down, height);
            float distanceToPlane;
            if (plane.Raycast(ray, out distanceToPlane))
            {
                return ray.GetPoint(distanceToPlane);
            }
            return currentCamera.transform.position;
        }

        bool CheckIfFlowmapDataContains(WaterSystem waterInstance)
        {
            if (waterInstance.Settings.FlowingScriptableData == null)
            {
                Debug.LogError("You should draw a flow map before baking the simulation! Use 'Water->Flowing->Flowmap painter'");
                return false;
            }

            return true;
        }

        public void SaveOrthoDepth(WaterSystem waterInstance, Vector3 position, int areaSize, int texSize)
        {
#if UNITY_EDITOR
            if (!CheckIfFlowmapDataContains(waterInstance)) return;

            var pathToInstanceFolder = KW_Extensions.GetPathToWaterInstanceFolder(waterInstance.WaterInstanceID);
            var pathToFile           = Path.Combine(pathToInstanceFolder, KWS_Settings.DataPaths.FluidsMaskTexture);


            texSize = Mathf.Clamp(texSize, 512, 2048);
            var depthTex = MeshUtils.GetRiverOrthoDepth(waterInstance, position, areaSize, texSize, nearPlaneDepth, farPlaneDepth);
            depthTex.SaveTexture(pathToFile, useAutomaticCompressionFormat: true, KW_Extensions.UsedChannels._R, isHDR: false, mipChain: false);

            waterInstance.Settings.FlowingScriptableData.FluidsMaskTexture = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(pathToFile + ".kwsTexture");
            UnityEditor.EditorUtility.SetDirty(waterInstance.Settings.FlowingScriptableData);

            KW_Extensions.SafeDestroy(depthTex);
#else
            Debug.LogError("You can't save ortho depth data in runtime");
            return;
#endif

        }

        private int KW_FluidsVelocityAreaScale = Shader.PropertyToID("KW_FluidsVelocityAreaScale");
        private int KW_FluidsMapWorldPosition_lod0 = Shader.PropertyToID("KW_FluidsMapWorldPosition_lod0");
        private int KW_FluidsMapWorldPosition_lod1 = Shader.PropertyToID("KW_FluidsMapWorldPosition_lod1");
        private int KW_FluidsMapAreaSize_lod0 = Shader.PropertyToID("KW_FluidsMapAreaSize_lod0");
        private int KW_FluidsMapAreaSize_lod1 = Shader.PropertyToID("KW_FluidsMapAreaSize_lod1");
        int KW_Fluids_Lod0 = Shader.PropertyToID("KW_Fluids_Lod0");
        int KW_FluidsFoam_Lod0 = Shader.PropertyToID("KW_FluidsFoam_Lod0");
        int KW_Fluids_Lod1     = Shader.PropertyToID("KW_Fluids_Lod1");
        int KW_FluidsFoam_Lod1 = Shader.PropertyToID("KW_FluidsFoam_Lod1");
        private int KW_FluidsRequiredReadPrebakedSim = Shader.PropertyToID("KW_FluidsRequiredReadPrebakedSim");

        string KW_FLUIDS_PREBAKE_SIM = "KW_FLUIDS_PREBAKE_SIM";

        public void PrebakeSimulation(WaterSystem waterInstance, Vector3 waterPosition, int areaSize, int resolution, float flowSpeed, float foamStrength)
        {
            var areaPosition = waterPosition;

            areaPosition += ComputeAreaSimulationJitter(1f * areaSize / resolution, 1f * areaSize / resolution);
            if (lastPosition == null) lastPosition = areaPosition;
            var offset                             = areaPosition - lastPosition;


            if (lastWidth != resolution || lastHeight != resolution) InitializeTextures(resolution, resolution);
            LoadFoamTexture();
            SetShaderParams(waterInstance, fluidsMaterial);
            
            Shader.SetGlobalFloat(KW_FluidsVelocityAreaScale, (0.5f * areaSize / 40f) * flowSpeed);
            Shader.SetGlobalFloat(KW_FluidsMapAreaSize_lod0, areaSize);
            Shader.SetGlobalFloat(KW_FluidsMapAreaSize_lod1, areaSize * 4);

            Shader.SetGlobalVector(KW_FluidsMapWorldPosition_lod0, areaPosition);
            Shader.SetGlobalVector(KW_FluidsMapWorldPosition_lod1, areaPosition * 4);

            InitializeMaterials(waterInstance);
            fluidsMaterial.SetKeyword(KW_FLUIDS_PREBAKE_SIM, true);

            var target_lod0 = RenderFluidLod(fluidsData[2], fluidsData[3], flowSpeed * 1.0f, areaSize, foamStrength * 5f, (Vector3) offset, areaPosition, false);

            Shader.SetGlobalTexture(KW_Fluids_Lod0, target_lod0.DataRT.rt);
            Shader.SetGlobalTexture(KW_FluidsFoam_Lod0, target_lod0.FoamRT.rt);
         
            prebakedFluidsRT = target_lod0.DataRT;

            lastPosition = areaPosition;
            frameNumber++;

        }

        public void SavePrebakedSimulation(WaterSystem waterInstance)
        {
#if UNITY_EDITOR
            if (prebakedFluidsRT == null) return;

            var pathToInstanceFolder = KW_Extensions.GetPathToWaterInstanceFolder(waterInstance.WaterInstanceID);
            var pathToFile           = Path.Combine(pathToInstanceFolder, KWS_Settings.DataPaths.FluidsPrebakedTexture);

            InitializeMaterials(waterInstance);

            var tempRT = new RenderTexture(prebakedFluidsRT.rt.width, prebakedFluidsRT.rt.height, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);
            Graphics.Blit(prebakedFluidsRT.rt, tempRT, fluidsMaterial, 1);
            tempRT.SaveRenderTexture(pathToFile, useAutomaticCompressionFormat: true, KW_Extensions.UsedChannels._RGBA, isHDR: false, mipChain: false);
            tempRT.Release();

            waterInstance.Settings.FlowingScriptableData.FluidsPrebakedTexture = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(pathToFile + ".kwsTexture");
            UnityEditor.EditorUtility.SetDirty(waterInstance.Settings.FlowingScriptableData);

            frameNumber = 0;
            fluidsMaterial.SetKeyword(KW_FLUIDS_PREBAKE_SIM, false);
#endif
        }


        public void RenderFluids(WaterSystem waterInstance, Camera currentCamera, Vector3 waterPosition, int areaSize, int resolution, float flowSpeed, float foamStrength)
        {
            var flowingData = waterInstance.Settings.FlowingScriptableData;
            if (flowingData == null || flowingData.FluidsMaskTexture == null || flowingData.FluidsPrebakedTexture == null) return;

            InitializeMaterials(waterInstance);
            
            var centerAreaPosition = RayToWaterPos(currentCamera, waterPosition.y);
            var areaPosition0 = centerAreaPosition + currentCamera.transform.forward * areaSize * 0.5f;

            areaPosition0 += ComputeAreaSimulationJitter(1f * areaSize / resolution, 1f * areaSize / resolution);
            if (lastPosition == null) lastPosition = areaPosition0;
            var offset = areaPosition0 - lastPosition;

            var lod1Multiplier = KWS_Settings.Flowing.AreaSizeMultiplierLod1;
            var areaPosition1 = centerAreaPosition;
            areaPosition1 += ComputeAreaSimulationJitter(lod1Multiplier * areaSize / resolution, lod1Multiplier * areaSize / resolution);
            if (lastPosition_lod1 == null) lastPosition_lod1 = areaPosition1;
            var offset_lod1 = areaPosition1 - lastPosition_lod1;

            if (lastWidth != resolution || lastHeight != resolution) InitializeTextures(resolution, resolution);
            LoadFoamTexture();
            SetShaderParams(waterInstance, fluidsMaterial);

            fluidsMaterial.SetFloat(KW_FluidsRequiredReadPrebakedSim, frameNumber < 10 ? 1 : 0);
            fluidsMaterial.SetFloat(KW_FluidsMapAreaSize_lod1, areaSize * lod1Multiplier);
            fluidsMaterial.SetVector(KW_FluidsMapWorldPosition_lod1, areaPosition1);

            var target1 = RenderFluidLod(fluidsData[0], fluidsData[1], flowSpeed * 0.5f, areaSize * lod1Multiplier, foamStrength * 2.5f, (Vector3)offset_lod1, areaPosition1, false);
            var target0 = RenderFluidLod(fluidsData[2], fluidsData[3], flowSpeed * 1.0f, areaSize, foamStrength * 5f, (Vector3)offset, areaPosition0, true, target1.DataRT.rt);

            waterInstance.SharedData.FluidsAreaSize = areaSize;
            waterInstance.SharedData.FluidsSpeed = flowSpeed;
            waterInstance.SharedData.FluidsAreaPositionLod0 = areaPosition0;
            waterInstance.SharedData.FluidsAreaPositionLod1 = areaPosition1;
            waterInstance.SharedData.FluidsRT[0] = target0.DataRT.rt;
            waterInstance.SharedData.FluidsRT[1] = target1.DataRT.rt;
            waterInstance.SharedData.FluidsFoamRT[0] = target0.FoamRT.rt;
            waterInstance.SharedData.FluidsFoamRT[1] = target1.FoamRT.rt;

            lastPosition = areaPosition0;
            lastPosition_lod1 = areaPosition1;

            frameNumber++;

        }

        FluidsData RenderFluidLod(FluidsData data1, FluidsData data2, float flowSpeedMultiplier, float areaSize, float foamTexelOffset, Vector3 offset, Vector3 worldPos,
            bool canUseNextLod, RenderTexture nextLod = null)
        {

            if (canUseNextLod)
            {
                fluidsMaterial.EnableKeyword("CAN_USE_NEXT_LOD");
                fluidsMaterial.SetTexture("_FluidsNextLod", nextLod);
            }
            else fluidsMaterial.DisableKeyword("CAN_USE_NEXT_LOD");

            fluidsMaterial.SetFloat("_FlowSpeed", flowSpeedMultiplier);
            fluidsMaterial.SetFloat("_AreaSize", areaSize);
            fluidsMaterial.SetFloat("_FoamTexelOffset", foamTexelOffset);


            var source = (frameNumber % 2 == 0) ? data1 : data2;
            var target = (frameNumber % 2 == 0) ? data2 : data1;
            fluidsMaterial.SetVector("_CurrentPositionOffset", offset / areaSize);
            fluidsMaterial.SetVector("_CurrentFluidMapWorldPos", worldPos);
            Graphics.SetRenderTarget(target.MRT, target.DataRT.rt.depthBuffer);
            Graphics.Blit(source.DataRT.rt, fluidsMaterial, 0);
            return target;
        }
    }
}