using System;
using System.Collections.Generic;
using UnityEngine;
using static KWS.KW_Extensions;

namespace KWS
{
    public partial class WaterSystem
    {
        int BakedFluidsSimPercentPassed;
        private bool _isFluidsSimBakedMode;

        [Flags]
        internal enum WaterTab
        {
            ColorSettings      = 1,
            Waves              = 2,
            Reflection         = 4,
            ColorRefraction    = 8,
            Flowing            = 16,
            DynamicWaves       = 32,
            Shoreline          = 64,
            Foam               = 128,
            VolumetricLighting = 256,
            Caustic            = 512,
            Underwater         = 1024,
            Mesh               = 2048,
            Rendering          = 4096,
            OrthoDepth         = 8192,
            All                = ~0
        }

        internal int GetVisibleMeshTrianglesCount()
        {

            var data = GetQuadTreeChunksData();
            if (data.Instances.Count == 0) return 0;
            return MeshQuadTreeComponent.VisibleNodes.Count * data.Instances[data.ActiveInstanceIndex].triangles.Length;
        }

        bool IsCanRendererGameOrSceneCameraOnly()
        {
            if (Application.isPlaying)
            {
                return _currentCamera.cameraType == CameraType.Game;
            }
            else
            {
                return _currentCamera.cameraType == CameraType.SceneView;
            }
        }


        internal void BakeFluidSimulation()
        {
            _isFluidsSimBakedMode = true;
            currentBakeFluidsFrames = 0;
        }

        void BakeFluidSimulationFrame()
        {
            if (currentBakeFluidsFrames < BakeFluidsLimitFrames)
            {
                currentBakeFluidsFrames++;
                BakedFluidsSimPercentPassed = (int)((100f / BakeFluidsLimitFrames) * currentBakeFluidsFrames);
                for (int j = 0; j < 10; j++)
                    _fluidsSimulationEditorView.PrebakeSimulation(this, Settings.FlowMapAreaPosition, Settings.FlowMapAreaSize, 4096, Settings.FluidsSpeed, 0.1f);
            }
            else if(_isFluidsSimBakedMode)
            {
                _fluidsSimulationEditorView.SavePrebakedSimulation(this);
                BakedFluidsSimPercentPassed = 0;
                _isFluidsSimBakedMode = false;
                Debug.Log("Fluids obstacles saved!");
            }
        }

        internal int GetBakeSimulationPercent()
        {
            return _isFluidsSimBakedMode ? BakedFluidsSimPercentPassed : -1;
        }

        internal bool FluidsSimulationInEditMode()
        {
            return _isFluidsSimBakedMode;
        }


        #region  Shoreline Methods

        internal bool IsEditorAllowed()
        {
            if (_tempGameObject == null) return false;
            else return true;
        }

        internal bool Editor_IsShorelineRequiredInitialize()
        {
            return _shoreLineInitializingStatus == AsyncInitializingStatusEnum.NonInitialized;
        }

        internal void SaveShorelineWavesDataToJson()
        {
            Settings.ShorelineWavesScriptableData = ShorelineWavesComponent.SaveWavesDataToAsset(WaterInstanceID);
        }

        internal List<ShorelineWavesScriptableData.ShorelineWave> GetShorelineWaves()
        {
            return ShorelineWavesComponent?.GetInitializedWaves();
        }

        internal void ReinitializeShorelineWaves()
        {
            _shoreLineInitializingStatus = AsyncInitializingStatusEnum.NonInitialized;
        }

        #endregion

        #region FlowMap Methods

        internal void ReinitializeFlowmap()
        {
            _flowmapInitializingStatus = AsyncInitializingStatusEnum.NonInitialized;
        }

        internal void InitializeFlowMapEditorResources()
        {
            FlowMapComponent.InitializeFlowMapEditor(this, (int)Settings.FlowMapTextureResolution, Settings.FlowMapAreaSize, Settings.FlowingScriptableData);
            _flowmapInitializingStatus = AsyncInitializingStatusEnum.Initialized;
        }

        internal void DrawOnFlowMap(Vector3 brushPosition, Vector3 brushMoveDirection, float circleRadius, float brushStrength, bool eraseMode = false)
        {
            InitializeFlowMapEditorResources();
            FlowMapComponent.DrawOnFlowMap(this, brushPosition, brushMoveDirection, circleRadius, brushStrength, eraseMode);
        }

        internal void RedrawFlowMap()
        {
            InitializeFlowMapEditorResources();
            FlowMapComponent.RedrawFlowMap(Settings.FlowMapAreaSize);
        }

        internal void ChangeFlowmapResolution()
        {
            InitializeFlowMapEditorResources();
            FlowMapComponent.ChangeFlowmapResolution(this, (int)Settings.FlowMapTextureResolution);
        }

        internal void SaveFlowMap()
        {
            InitializeFlowMapEditorResources();
            Settings.FlowingScriptableData = FlowMapComponent.SaveFlowMap(Settings.FlowMapAreaSize, Settings.FlowMapAreaPosition, (int)Settings.FlowMapTextureResolution, WaterInstanceID);
        }

        internal void LoadFlowMap()
        {
            var data = Settings.FlowingScriptableData;
            if (data == null || data.FlowmapTexture == null) return;
            if (_flowmapInitializingStatus == AsyncInitializingStatusEnum.Initialized) return;

            Settings.FlowMapTextureResolution = (FlowmapTextureResolutionEnum)data.FlowmapResolution;
            Settings.FlowMapAreaSize = data.AreaSize;
            SharedData.Flowmap = data.FlowmapTexture;

            _flowmapInitializingStatus = AsyncInitializingStatusEnum.Initialized;
        }

        internal bool IsFlowmapInitialized()
        {
            if (_flowmapInitializingStatus == AsyncInitializingStatusEnum.Initialized) return true;
            if (_flowmapInitializingStatus == AsyncInitializingStatusEnum.Failed || _flowmapInitializingStatus == AsyncInitializingStatusEnum.NonInitialized) return false;

            return true;
        }

        internal void ClearFlowMap()
        {
            FlowMapComponent.ClearFlowMap(this, WaterInstanceID);
            if (Settings.FlowingScriptableData != null) Settings.FlowingScriptableData.FlowmapTexture = null;
        }

        #endregion


        #region Caustic Methods

        internal void Editor_SaveCausticDepth()
        {
            //todo use native caustic depth
        }

        internal void Editor_SaveFluidsDepth()
        {
            FluidsSimulationComponent.SaveOrthoDepth(this, Settings.FlowMapAreaPosition, Settings.FlowMapAreaSize, (int)Settings.FlowMapTextureResolution);
        }



        #endregion
    }

}