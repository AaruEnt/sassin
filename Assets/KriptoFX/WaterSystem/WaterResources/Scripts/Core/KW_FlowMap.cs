using System.IO;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using static KWS.KWS_CoreUtils;

namespace KWS
{
    public class KW_FlowMap 
    {

        private Material    _flowMaterial;

        public RenderTexture _flowmapRT;
        public RenderTexture _tempRT;
        //public  Texture2D     _flowmapTex;
        private Texture2D     _grayTex;

        private int _currentAreaSize;


        private Material FlowMaterial
        {
            get
            {
                if (_flowMaterial == null) _flowMaterial = CreateMaterial(KWS_ShaderConstants.ShaderNames.FlowMapShaderName);
                return _flowMaterial;
            }
        }

        public void Release()
        {
            KW_Extensions.SafeDestroy(_flowmapRT, _tempRT);
            _flowmapRT = null;
            _tempRT = null;
            KW_Extensions.SafeDestroy(_flowMaterial, _grayTex);
        }

        public void ClearFlowMap(WaterSystem waterInstance, string waterInstanceID)
        {
#if UNITY_EDITOR
            ClearRenderTexture(_flowmapRT, ClearFlag.Color, new Color(0.5f, 0.5f, 0.5f, 0.5f));
            waterInstance.SharedData.Flowmap = _flowmapRT;

            var pathToInstanceFolder = KW_Extensions.GetPathToWaterInstanceFolder(waterInstanceID);
            var pathToFile = Path.Combine(pathToInstanceFolder, KWS_Settings.DataPaths.FlowmapTexture);
            pathToFile = pathToFile + ".png";
            if (!File.Exists(pathToFile)) return;

            UnityEditor.AssetDatabase.DeleteAsset(pathToFile);
#endif
        }

      
        private void InitializeFlowmapRT(int size)
        {
            _flowmapRT = new RenderTexture(size, size, 0, GraphicsFormat.R16G16_SFloat) { name = "_FlowmapRT" };
            ClearRenderTexture(_flowmapRT, ClearFlag.Color, new Color(0.5f, 0.5f, 0.5f, 0.5f));
        }


        public void InitializeFlowMapEditor(WaterSystem waterInstance, int size, int areaSize, FlowingScriptableData savedData)
        {
            if (_flowmapRT == null)
            {
                InitializeFlowmapRT(size);

                if (_tempRT == null) _tempRT = new RenderTexture(size, size, 0, _flowmapRT.graphicsFormat) { name = "_tempFlowmapRT" };

                var flowTex = waterInstance.Settings.FlowingScriptableData != null && waterInstance.Settings.FlowingScriptableData.FlowmapTexture != null
                    ? waterInstance.Settings.FlowingScriptableData.FlowmapTexture
                    : null;
                if (flowTex != null)
                {
                    var activeRT = RenderTexture.active;
                    Graphics.Blit(flowTex, _flowmapRT);
                    RenderTexture.active = activeRT;
                }

                waterInstance.SharedData.Flowmap = _flowmapRT;
                _currentAreaSize = areaSize;
            }
            
        }


        public void DrawOnFlowMap(WaterSystem waterInstance, Vector3 brushPosition, Vector3 brushMoveDirection, float circleRadius, float brushStrength, bool eraseMode = false)
        {
            var brushSize = _currentAreaSize / circleRadius;
            var uv        = new Vector2(brushPosition.x / _currentAreaSize + 0.5f, brushPosition.z / _currentAreaSize + 0.5f);
            if (brushMoveDirection.magnitude < 0.001f) brushMoveDirection = Vector3.zero;

            FlowMaterial.SetVector("_MousePos",  uv);
            FlowMaterial.SetVector("_Direction", new Vector2(brushMoveDirection.x, brushMoveDirection.z));
            FlowMaterial.SetFloat("_Size",          brushSize     * 0.75f);
            FlowMaterial.SetFloat("_BrushStrength", brushStrength / (circleRadius * 3));
            FlowMaterial.SetInt("isErase",        eraseMode ? 1 : 0);
            
            var activeRT = RenderTexture.active; 
            Graphics.Blit(_flowmapRT, _tempRT, FlowMaterial, 0);
            Graphics.Blit(_tempRT,    _flowmapRT);
            RenderTexture.active = activeRT;
        }

        public FlowingScriptableData SaveFlowMap(int areaSize, Vector3 areaPos, int resolution, string waterInstanceID)
        {
#if UNITY_EDITOR
            var pathToInstanceFolder = KW_Extensions.GetPathToWaterInstanceFolder(waterInstanceID);
            var pathToFile = Path.Combine(pathToInstanceFolder, KWS_Settings.DataPaths.FlowmapTexture);
            _flowmapRT.SaveRenderTexture(pathToFile, useAutomaticCompressionFormat: true, KW_Extensions.UsedChannels._RG, isHDR: false, mipChain: false);

            var data = ScriptableObject.CreateInstance<FlowingScriptableData>();
            data.FlowmapTexture = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(pathToFile +".kwsTexture");
            data.AreaSize = areaSize;
            data.AreaPosition = areaPos;
            data.FlowmapResolution = resolution;
            return data.SaveScriptableData(waterInstanceID, "FlowingData");
#else
            Debug.LogError("You can't save waves data in runtime");
            return null;
#endif
        }

        public void ChangeFlowmapResolution(WaterSystem waterInstance, int newResolution)
        {
            KW_Extensions.SafeDestroy(_flowmapRT);
            InitializeFlowmapRT(newResolution);
           
            var activeRT = RenderTexture.active;
            Graphics.Blit(_tempRT,    _flowmapRT);
            RenderTexture.active = activeRT;

            waterInstance.SharedData.Flowmap = _flowmapRT;
        }

        public void RedrawFlowMap(int newAreaSize)
        {
            var uvScale = (float) newAreaSize / _currentAreaSize;
            _currentAreaSize = newAreaSize;
            FlowMaterial.SetFloat("_UvScale", uvScale);
            var activeRT = RenderTexture.active;
            Graphics.Blit(_flowmapRT, _tempRT, FlowMaterial, 1);
            Graphics.Blit(_tempRT, _flowmapRT);
            RenderTexture.active = activeRT;
        }
    }
}