using System;
using UnityEngine;
using static KWS.KW_Extensions;
using static KWS.KWS_ShaderConstants;

namespace KWS
{
    public partial class WaterSystem
    {

        #region Initialization


        //If you press ctrl+z after deleting the water gameobject, unity returns all objects without links and save all objects until you close the editor. Not sure how to fix that =/ 
        void ClearUndoObjects(Transform parent)
        {
            if (parent.childCount > 0)
            {
                KW_Extensions.SafeDestroy(parent.GetChild(0).gameObject);
            }
        }

        internal void UpdateState()
        {
            OnWaterSettingsChanged?.Invoke(this, WaterTab.All);
            UpdateAllShadersConstantParams();
        }

        void CheckAndReinitializeIfRequired()
        {
            if (_editorPropertyCheckerMaterial == null) _editorPropertyCheckerMaterial = KWS_CoreUtils.CreateMaterial(ShaderNames.WaterShaderName);
            if (!_editorPropertyCheckerMaterial.HasProperty(KWS_ShaderConstants.ConstantWaterParams.KW_Transparent))
            {
                _editorPropertyCheckerMaterial.SetFloat(KWS_ShaderConstants.ConstantWaterParams.KW_Transparent, 1);
                UpdateAllShadersConstantParams();
            }
        }

        private void OnAnyWaterSettingsChangedEvent(WaterSystem instance, WaterTab changedTab)
        {
            UpdateAllShadersConstantParams();
            if (changedTab.HasFlag(WaterTab.Mesh)) RebuildMesh();
        }


        GameObject CreateTempGameobject(string name, Transform parent)
        {
            ClearUndoObjects(parent);

            var tempGameObject = KW_Extensions.CreateHiddenGameObject(name);
            tempGameObject.transform.parent = parent;
            tempGameObject.transform.localPosition = Vector3.zero;
            tempGameObject.transform.localRotation = new Quaternion();
            return tempGameObject;
        }

        void CreateInfiniteOceanShadowCullingAnchor()
        {
            _infiniteOceanShadowCullingAnchor = KW_Extensions.CreateHiddenGameObject("InfiniteOceanShadowCullingAnchor");
            var shadowFixInfiniteOceanMaterial = KWS_CoreUtils.CreateMaterial(KWS_ShaderConstants.ShaderNames.InfiniteOceanShadowCullingAnchorName);
            var shadowFixInfiniteOceanMesh = KWS_CoreUtils.CreateQuad();

            _infiniteOceanShadowCullingAnchor.AddComponent<MeshRenderer>().sharedMaterial = shadowFixInfiniteOceanMaterial;
            _infiniteOceanShadowCullingAnchor.AddComponent<MeshFilter>().sharedMesh = shadowFixInfiniteOceanMesh;


            _infiniteOceanShadowCullingAnchor.transform.rotation = Quaternion.Euler(270, 0, 0);
            _infiniteOceanShadowCullingAnchor.transform.localScale = new Vector3(100000, 100000, 1);
            _infiniteOceanShadowCullingAnchor.transform.parent = WaterTemporaryObject.transform;
            _infiniteOceanShadowCullingAnchorTransform = _infiniteOceanShadowCullingAnchor.transform;
        }

        void UpdateInfiniteOceanShadowCullingAnchor()
        {
            var camPos = KW_Extensions.GetCameraPositionFast(_currentCamera);
            _infiniteOceanShadowCullingAnchorTransform.position = new Vector3(camPos.x, WaterRelativeWorldPosition.y - 1000, camPos.z);
            _infiniteOceanShadowCullingAnchorTransform.rotation = Quaternion.Euler(270, 0, 0);
        }


        internal Mesh InitializeRiverMesh()
        {
            SplineMeshComponent.CreateMeshFromSpline(this);
            return SplineMeshComponent.CurrentMesh;
        }

        internal void InitializeOrUpdateMesh()
        {
            switch (Settings.WaterMeshType)
            {
                case WaterMeshTypeEnum.InfiniteOcean:
                    InitializeQuadTreeOcean(_meshQuadTreeGameView, _lastGameCamera);
#if UNITY_EDITOR
                    InitializeQuadTreeOcean(_meshQuadTreeEditorView, _lastEditorCamera);
#endif
                    UpdateQuadTreeMesh(true);
                    break;
                case WaterMeshTypeEnum.FiniteBox:
                    _meshQuadTreeGameView.Initialize(MeshQuadTree.QuadTreeTypeEnum.Finite, WorldSpaceBounds, WaterPivotWorldPosition, WaterPivotWorldRotation.eulerAngles, Settings.WaterMeshQualityFinite, Settings.UseTesselation);
#if UNITY_EDITOR
                    _meshQuadTreeEditorView.Initialize(MeshQuadTree.QuadTreeTypeEnum.Finite, WorldSpaceBounds, WaterPivotWorldPosition, WaterPivotWorldRotation.eulerAngles, Settings.WaterMeshQualityFinite, Settings.UseTesselation);
#endif
                    UpdateQuadTreeMesh(true);
                    break;
                case WaterMeshTypeEnum.River:
                    SplineRiverMesh = InitializeRiverMesh();
                    break;
            }
        }

        void InitializeQuadTreeOcean(MeshQuadTree quadTree, Camera quadTreeCamera)
        {
            var farDist = Settings.OceanDetailingFarDistance;
            if (Settings.UseUnderwaterEffect && quadTreeCamera != null) farDist = (int)Mathf.Min(farDist, quadTreeCamera.farClipPlane);
            var oceanBounds = new Bounds(new Vector3(0, WaterPivotWorldPosition.y - 2500, 0), new Vector3(farDist, 5000, farDist));
            quadTree.Initialize(MeshQuadTree.QuadTreeTypeEnum.Infinite, oceanBounds, WaterPivotWorldPosition, WaterPivotWorldRotation.eulerAngles, Settings.WaterMeshQualityInfinite, Settings.UseTesselation);

            //Debug.Log("rebuild mesh " + farDist);
        }

        internal void RebuildMesh()
        {
            InitializeOrUpdateMesh();
            UpdateAllShadersConstantParams();
        }

        internal void UpdateQuadTreeMesh(bool forceUpdate = false)
        {
            if (!CameraDatas.TryGetValue(_currentCamera, out var camData)) return;

            MeshQuadTreeComponent.UpdateQuadTree(camData.Position, camData.RotationEuler, camData.CameraTransform.forward, this, forceUpdate);
            var lodOffset = Settings.UseDynamicWaves || Settings.UseShorelineRendering ? KWS_Settings.Water.QuadTreeChunkLodOffsetForDynamicWaves : 0;
            MeshQuadTreeComponent.UpdateQuadTreeDetailingRelativeToWind(Settings.WindSpeed, lodOffset);
        }

        internal MeshQuadTree.QuadTreeChunksData GetQuadTreeChunksData()
        {
            return MeshQuadTreeComponent.QuadTreeData;
        }


        void InitializeWaterCommonResources()
        {
            if (_tempGameObject == null) _tempGameObject = CreateTempGameobject("TemporaryWaterResources", WaterRootTransform);

            _waterSharedMaterials.Clear();
            _waterSharedComputeShaders.Clear();

            InitializeOrUpdateMesh();

            UpdateAllShadersConstantParams();
            IsWaterInitialized = true;

        }

        bool IsUnderwaterVisible(Vector3 worldPos)
        {
            var newPos = GetCurrentWaterSurfaceData(worldPos);
            if (newPos.IsActualDataReady)
            {
                if (worldPos.y < newPos.Position.y) return true;
            }

            return false;
        }


        Vector3[] _nearPlaneWorldPoints = new Vector3[4];
        bool IsUnderwaterVisible(Camera cam, Bounds waterBounds)
        {
            if (Settings.WaterMeshType == WaterMeshTypeEnum.River) return false;
            CalculateNearPlaneWorldPoints(ref _nearPlaneWorldPoints, cam);
            var min = waterBounds.min;
            var max = waterBounds.max;

            if (IsPointInsideAABB(_nearPlaneWorldPoints[0], min, max)
             || IsPointInsideAABB(_nearPlaneWorldPoints[1], min, max)
             || IsPointInsideAABB(_nearPlaneWorldPoints[2], min, max)
             || IsPointInsideAABB(_nearPlaneWorldPoints[3], min, max))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        bool IsRiverUnderwaterVisible(Vector3 worldPos)
        {
            if (SplineMeshComponent.GetSplineSurfaceHeight(this, worldPos, out var surfaceWorldPos, out var surfaceNormal))
            {
                if (worldPos.y < surfaceWorldPos.y + CurrentMaxWaveHeight) return true;
            }

            return false;
        }

        bool IsRiverUnderwaterVisible(Camera cam)
        {
            CalculateNearPlaneWorldPoints(ref _nearPlaneWorldPoints, cam);

            for (int i = 0; i < 4; i++)
            {
                var worldPos = _nearPlaneWorldPoints[i];
                if (SplineMeshComponent.GetSplineSurfaceHeight(this, worldPos, out var surfaceWorldPos, out var surfaceNormal))
                {
                    if (worldPos.y < surfaceWorldPos.y + CurrentMaxWaveHeight) return true;
                }
            }

            return false;
        }

        bool IsPointInsideAABB(Vector3 point, Vector3 min, Vector3 max)
        {
            return (point.x >= min.x && point.x <= max.x) &&
                   (point.y >= min.y && point.y <= max.y) &&
                   (point.z >= min.z && point.z <= max.z);
        }

        #endregion

        #region Render Logic 

        bool IsWaterVisibleForCamera()
        {
            var bounds = WorldSpaceBounds;
            return KW_Extensions.IsBoxVisibleAccurate(ref CurrentCameraFrustumPlanes, ref CurrentCameraFrustumCorners, bounds.min, bounds.max);
        }

        void UpdateActiveWaterInstances()
        {
            if (IsWaterVisible == _lastWaterVisible) return;
            _lastWaterVisible = IsWaterVisible;

            if (IsWaterVisible)
            {
                if (!VisibleWaterInstances.Contains(this))
                {
                    VisibleWaterInstances.Add(this);
                    SharedData.WaterShaderPassID = VisibleWaterInstances.Count - 1;
                }
            }
            else
            {
                if (VisibleWaterInstances.Contains(this))
                {
                    VisibleWaterInstances.Remove(this);
                    SharedData.WaterShaderPassID = 0;
                }
            }
        }

        void SetScaleRotationRelativeToMeshType()
        {
            WaterRootTransform.localScale = Vector3.one;
            switch (Settings.WaterMeshType)
            {
                case WaterMeshTypeEnum.InfiniteOcean:
                    WaterRootTransform.rotation = Quaternion.identity;
                    break;
                case WaterMeshTypeEnum.FiniteBox:
                    WaterRootTransform.rotation = Quaternion.Euler(0, WaterPivotWorldRotation.eulerAngles.y, 0);
                    break;
                case WaterMeshTypeEnum.River:
                    WaterRootTransform.rotation = Quaternion.identity;
                    break;
                case WaterMeshTypeEnum.CustomMesh:
                    break;
            }
        }

        void RenderWater()
        {
            if (_currentCamera == null) return;

            SetScaleRotationRelativeToMeshType();

            if (!IsWaterInitialized) InitializeWaterCommonResources();
            if (!isWaterPlatformSpecificResourcesInitialized) InitializeWaterPlatformSpecificResources();

            if (Settings.WaterMeshType == WaterMeshTypeEnum.InfiniteOcean)
            {
                if (_infiniteOceanShadowCullingAnchor == null) CreateInfiniteOceanShadowCullingAnchor();
                UpdateInfiniteOceanShadowCullingAnchor();
            }

            var currentDeltaTime = KW_Extensions.DeltaTime();
            //if (currentDeltaTime < 0.0001f) return;

            RenderPlatformSpecificFeatures(_currentCamera);

            if (Settings.WaterMeshType == WaterMeshTypeEnum.InfiniteOcean || Settings.WaterMeshType == WaterMeshTypeEnum.FiniteBox)
            {
                if (MeshQuadTreeComponent.IsRequireReinitialize(WaterPivotWorldPosition, WaterPivotWorldRotation.eulerAngles, _currentCamera, Settings)) RebuildMesh();
                UpdateQuadTreeMesh(false);
            }

            PlanarReflectionComponent.RenderReflection(this, _currentCamera);

            RenderFFT(); //todo check disabled mesh rendering and foam shader "displacement texture"

            if (_buoyancyPassedFramesAfterRequest < KWS_Settings.Water.BuoyancyRequestLifetimeFrames) UpdateFFT_HeightData();
            else _isBuoyancyDataReadCompleted = false;

            if (Settings.UseShorelineRendering)
            {
                if (_shoreLineInitializingStatus == AsyncInitializingStatusEnum.NonInitialized)
                {
                    _shoreLineInitializingStatus = AsyncInitializingStatusEnum.Initialized;
                    ShorelineWavesComponent.InitializeWaves(Settings.ShorelineWavesScriptableData);
                }
                UpdateShoreline();
            }

            if (Settings.EnabledMeshRendering && Settings.UseFlowMap)
            {
                LoadFlowMap();
            }
            else if (_flowmapInitializingStatus == AsyncInitializingStatusEnum.Initialized)
            {
                FlowMapComponent.Release();
                _flowmapInitializingStatus = AsyncInitializingStatusEnum.NonInitialized;
            }

            if (Settings.EnabledMeshRendering && Settings.UseFlowMap && Settings.UseFluidsSimulation)
            {
                if (fixedUpdateFluids == null) fixedUpdateFluids = new KW_CustomFixedUpdate(RenderFluidsSimulation, 1);
                fixedUpdateFluids.Update(currentDeltaTime, 1.0f / Settings.FluidsSimulationFPS);
            }

            if (Settings.EnabledMeshRendering && _isFluidsSimBakedMode)
            {
                if (fixedUpdateBakingFluids == null) fixedUpdateBakingFluids = new KW_CustomFixedUpdate(BakeFluidSimulationFrame, 2);
                fixedUpdateBakingFluids.Update(currentDeltaTime, 1.0f / 60f);
            }

            if (Settings.EnabledMeshRendering && Settings.UseDynamicWaves)
            {
                if (fixedUpdateDynamicWaves == null) fixedUpdateDynamicWaves = new KW_CustomFixedUpdate(RenderDynamicWaves, 2);
                fixedUpdateDynamicWaves.Update(currentDeltaTime, 1.0f / Settings.DynamicWavesSimulationFPS);
            }
        }

        void RenderFFT()
        {
            var time = UseNetworkTime ? NetworkTime : KW_Extensions.TotalTime();

            time *= Settings.TimeScale;
            //time = 0.001f;
            var windTurbulence = Mathf.Lerp(0.05f, 0.5f, Settings.WindTurbulence);
            int fftSize = (int)Settings.FFT_SimulationSize;
            var timeScaleRelativeToFFTSize = (Mathf.RoundToInt(Mathf.Log(fftSize, 2)) - 5) / 4.0f;

            float lod0_Time = Mathf.Lerp(time, time, Settings.WindTurbulence);
            lod0_Time = Mathf.Lerp(lod0_Time, lod0_Time * 0.7f, timeScaleRelativeToFFTSize);

            FFTComponentLod0.ComputeFFT(windTurbulence, Mathf.Clamp(Settings.WindSpeed, 0, 2), Settings.WindRotation * Mathf.Deg2Rad, lod0_Time * 1.25f);

            if (UseMultipleSimulations)
            {
                FFTComponentLod1.ComputeFFT(windTurbulence, Mathf.Clamp(Settings.WindSpeed, 0, 6), Settings.WindRotation * Mathf.Deg2Rad, time * 0.9f);
                FFTComponentLod2.ComputeFFT(windTurbulence, Mathf.Clamp(Settings.WindSpeed, 0, 40), Settings.WindRotation * Mathf.Deg2Rad, time * 0.4f);
            }
        }

        void UpdateFFT_HeightData()
        {
            var heightDataSize = UseMultipleSimulations ? 256 : 64;
            FFTComponentHeightData.UpdateHeightData(this, heightDataSize, UseMultipleSimulations ? KWS_Settings.Water.FftDomainSize[2] : KWS_Settings.Water.FftDomainSize[0], WaterRelativeWorldPosition);

            _buoyancyPassedFramesAfterRequest++;
        }

        private void FFTComponentHeightDataOnIsDataReadCompleted()
        {
            _isBuoyancyDataReadCompleted = true;
        }

        void RenderFluidsSimulation()
        {
            if (_isFluidsSimBakedMode) return;
            if (!IsCanRendererGameOrSceneCameraOnly()) return;

            for (int i = 0; i < Settings.FluidsSimulationIterrations; i++)
            {
                FluidsSimulationComponent.RenderFluids(this, _currentCamera, WaterRelativeWorldPosition, Settings.FluidsAreaSize, Settings.FluidsTextureSize, Settings.FluidsSpeed, Settings.FluidsFoamStrength);
            }
        }

        void UpdateShoreline()
        {
            if (!CameraDatas.TryGetValue(_currentCamera, out var camData)) return;
            SharedData.ShorelineWaveBuffers = ShorelineWavesComponent.UpdateShorelineBuffers(_currentCamera, camData.Position, WaterRelativeWorldPosition, Settings.ShorelineFoamLodQuality);
        }

        void RenderDynamicWaves()
        {
            if (!IsCanRendererGameOrSceneCameraOnly()) return;
            if (!CameraDatas.TryGetValue(_currentCamera, out var camData)) return;
            DynamicWavesComponent.RenderWaves(this, _currentCamera, camData.Position, WaterRelativeWorldPosition, Settings.DynamicWavesSimulationFPS, Settings.DynamicWavesAreaSize,
                                              Settings.DynamicWavesResolutionPerMeter, Settings.DynamicWavesPropagationSpeed, Settings.UseDynamicWavesRainEffect ? Settings.DynamicWavesRainStrength : 0);
        }

        internal Matrix4x4 CurrentVPMatrix;
        internal Matrix4x4[] CurrentVPMatrixStereo = new Matrix4x4[2];
        //private Matrix4x4 _prevVPMatrix;
        //Matrix4x4[] _prevVPMatrixStereo = new Matrix4x4[2];

        void SetGlobalShaderParams()
        {
            Shader.SetGlobalFloat(ConstantWaterParams.KWS_SunMaxValue, KWS_Settings.Reflection.MaxSunStrength);
            Shader.SetGlobalVector(CausticID.KW_CausticLodSettings, KWS_Settings.Caustic.LodSettings);
        }

        void SetGlobalCameraShaderParams()
        {
            var cameraProjectionMatrix = _currentCamera.projectionMatrix;
            if (IsSinglePassStereoEnabled)
            {
                var arr_matrix_I_VP = new Matrix4x4[2];
                for (uint eyeIndex = 0; eyeIndex < 2; eyeIndex++)
                {
                    var matrix_cameraProjeciton = _currentCamera.GetStereoProjectionMatrix((Camera.StereoscopicEye)eyeIndex);
                    var matrix_V = GL.GetGPUProjectionMatrix(matrix_cameraProjeciton, true);
                    var matrix_P = _currentCamera.GetStereoViewMatrix((Camera.StereoscopicEye)eyeIndex);
                    var matrix_VP = matrix_V * matrix_P;

                    CurrentVPMatrixStereo[eyeIndex] = matrix_VP;
                    arr_matrix_I_VP[eyeIndex] = matrix_VP.inverse;
                }

                Shader.SetGlobalMatrixArray(CameraMatrix.KWS_MATRIX_VP_STEREO, CurrentVPMatrixStereo);
                Shader.SetGlobalMatrixArray(CameraMatrix.KWS_MATRIX_I_VP_STEREO, arr_matrix_I_VP);
            }
            else
            {
                var matrix_V = GL.GetGPUProjectionMatrix(cameraProjectionMatrix, true);
                var maitix_P = _currentCamera.worldToCameraMatrix;
                CurrentVPMatrix = matrix_V * maitix_P;

                Shader.SetGlobalMatrix(CameraMatrix.KWS_MATRIX_VP, CurrentVPMatrix);
                Shader.SetGlobalMatrix(CameraMatrix.KWS_MATRIX_I_VP, CurrentVPMatrix.inverse);
            }


            Shader.SetGlobalVector(DynamicWaterParams.KWS_CameraForward, GetCameraForwardFast(_currentCamera));

            var fogState = 0;
            if (RenderSettings.fog)
            {
                if (RenderSettings.fogMode == FogMode.Linear) fogState = 1;
                else if (RenderSettings.fogMode == FogMode.Exponential) fogState = 2;
                else if (RenderSettings.fogMode == FogMode.ExponentialSquared) fogState = 3;
            }
            Shader.SetGlobalInt(DynamicWaterParams.KWS_FogState, fogState);
        }

        void SetConstantShaderParamsShared(UnityEngine.Object shader)
        {
            if (shader == null || Settings == null) return;

//#if KWS_DEBUG
//            this.WaterLog("SetConstantShaderParamsShared " + shader.name, WaterLogMessageType.StaticUpdate);
//#endif

            var isFlowmapUsed = Settings.UseFlowMap && !Settings.UseFluidsSimulation;
            var isFlowmapFluidsUsed = Settings.UseFlowMap && Settings.UseFluidsSimulation;

            KWS_CoreUtils.SetKeywords(shader,
                                      (WaterKeywords.STEREO_INSTANCING_ON, IsSinglePassStereoEnabled),
                                      (WaterKeywords.USE_WATER_INSTANCING, Settings.WaterMeshType == WaterMeshTypeEnum.InfiniteOcean || Settings.WaterMeshType == WaterMeshTypeEnum.FiniteBox),
                                      (WaterKeywords.USE_FILTERING, Settings.UseFiltering),
                                      (WaterKeywords.USE_MULTIPLE_SIMULATIONS, UseMultipleSimulations),
                                      (WaterKeywords.SSPR_REFLECTION, Settings.UseScreenSpaceReflection),
                                      (WaterKeywords.USE_HOLES_FILLING, Settings.UseScreenSpaceReflectionHolesFilling && !Settings.UsePlanarReflection),
                                      (WaterKeywords.PLANAR_REFLECTION, Settings.UsePlanarReflection),
                                      (WaterKeywords.REFLECT_SUN, Settings.ReflectSun),
                                      (WaterKeywords.USE_REFRACTION_IOR, Settings.RefractionMode == RefractionModeEnum.PhysicalAproximationIOR),
                                      (WaterKeywords.USE_REFRACTION_DISPERSION, Settings.UseRefractionDispersion),
                                      (WaterKeywords.KW_FLOW_MAP_EDIT_MODE, FlowMapInEditMode),
                                      (WaterKeywords.KW_FLOW_MAP, isFlowmapUsed),
                                      (WaterKeywords.KW_FLOW_MAP_FLUIDS, isFlowmapFluidsUsed),
                                      (WaterKeywords.KW_DYNAMIC_WAVES, Settings.UseDynamicWaves),
                                      (WaterKeywords.USE_SHORELINE, Settings.UseShorelineRendering),
                                      (WaterKeywords.USE_VOLUMETRIC_LIGHT, Settings.UseVolumetricLight),
                                      (WaterKeywords.USE_CAUSTIC, Settings.UseCausticEffect),
                                      (WaterKeywords.USE_FOAM, Settings.UseFoamRendering));



            KWS_CoreUtils.SetFloats(shader,
                                    (ConstantWaterParams.KW_Transparent, Settings.Transparent), (ConstantWaterParams.KW_Turbidity, Settings.Turbidity),
                                    (ConstantWaterParams.KW_FFT_Size_Normalized, (Mathf.RoundToInt(Mathf.Log((int)Settings.FFT_SimulationSize, 2)) - 5) / 4.0f),
                                    (ConstantWaterParams.KW_WindSpeed, Settings.WindSpeed),
                                    (ConstantWaterParams.KWS_WindRotation, Settings.WindSpeed),
                                    (ConstantWaterParams.KWS_WindTurbulence, Settings.WindSpeed),
                                    (ConstantWaterParams.KW_GlobalTimeScale, Settings.TimeScale),

                                    (ConstantWaterParams.KWS_ScreenSpaceBordersStretching, Settings.ScreenSpaceBordersStretching),
                                    (ConstantWaterParams.KWS_IgnoreAnisotropicScreenSpaceSky, Settings.UseAnisotropicReflections && Settings.UseAnisotropicCubemapSkyForSSR ? 1 : 0),
                                    (ConstantWaterParams.KWS_AnisoReflectionsScale, AnisoScaleRelativeToWind),
                                    (ConstantWaterParams.KW_ReflectionClipOffset, Settings.ReflectionClipPlaneOffset),
                                    (ConstantWaterParams.KWS_SunCloudiness, Settings.ReflectedSunCloudinessStrength),
                                    (ConstantWaterParams.KWS_SunStrength, Settings.ReflectedSunStrength),

                                    (ConstantWaterParams.KWS_RefractionAproximatedDepth, Settings.RefractionAproximatedDepth),
                                    (ConstantWaterParams.KWS_RefractionSimpleStrength, Settings.RefractionSimpleStrength),
                                    (ConstantWaterParams.KWS_RefractionDispersionStrength, Settings.RefractionDispersionStrength * KWS_Settings.Water.MaxRefractionDispersion),
                                    (ConstantWaterParams.KW_WaterFarDistance, Settings.OceanDetailingFarDistance));

            KWS_CoreUtils.SetInts(shader,
                                  (ReflectionsID.KWS_IsCubemapReflectionPlanar, Settings.FixCubemapIndoorSkylightReflection ? 0 : 1),
                                  (ConstantWaterParams.KWS_IsCustomMesh, Settings.CustomMesh ? 1 : 0),
                                  (ConstantWaterParams.KWS_UseMultipleSimulations, UseMultipleSimulations ? 1 : 0),
                                  (ConstantWaterParams.KWS_UseRefractionIOR, Settings.RefractionMode == RefractionModeEnum.PhysicalAproximationIOR ? 1 : 0),
                                  (ConstantWaterParams.KWS_UseRefractionDispersion, Settings.UseRefractionDispersion ? 1 : 0),
                                  (ConstantWaterParams.KWS_IsFiniteTypeInstancing, Settings.WaterMeshType == WaterMeshTypeEnum.FiniteBox ? 1 : 0),
                                  (ConstantWaterParams.KWS_UseWireframeMode, Settings.WireframeMode ? 1 : 0));


            KWS_CoreUtils.SetVectors(shader,
                                     (ConstantWaterParams.KW_WaterColor, Settings.WaterColor),
                                     (ConstantWaterParams.KW_TurbidityColor, Settings.TurbidityColor),
                                     (ConstantWaterParams.KWS_InstancingWaterScale, Settings.MeshSize));

            KWS_CoreUtils.SetMatrices(shader, (ConstantWaterParams.KWS_InstancingRotationMatrix, Matrix4x4.TRS(Vector3.zero, WaterPivotWorldRotation, Settings.MeshSize)));

            if (Settings.UseFlowMap)
            {
                var fluidsSpeed = Settings.FlowMapSpeed;
                if (Settings.UseFluidsSimulation && !FlowMapInEditMode) fluidsSpeed = Settings.FluidsSpeed * Mathf.Lerp(0.125f, 1.0f, Settings.FluidsSimulationIterrations / 4.0f);

                KWS_CoreUtils.SetVectors(shader, (ConstantWaterParams.KW_FlowMapOffset, Settings.FlowMapAreaPosition));
                KWS_CoreUtils.SetFloats(shader, (ConstantWaterParams.KW_FlowMapSize, Settings.FlowMapAreaSize),
                                        (ConstantWaterParams.KW_FlowMapSpeed, fluidsSpeed),
                                        (ConstantWaterParams.KW_FlowMapFluidsStrength, Settings.FluidsFoamStrength));

            }


            if (Settings.UseDynamicWaves)
            {
                KWS_CoreUtils.SetFloats(shader, (DynamicWaves.KW_DynamicWavesAreaSize, Settings.DynamicWavesAreaSize));
            }

            if (Settings.UseShorelineRendering)
            {
            }

            if (Settings.UseFoamRendering)
            {
                KWS_CoreUtils.SetVectors(shader, (ConstantWaterParams.KWS_FoamFadeSize, new Vector2(1.0f / Settings.FoamFadeDistance, 1.0f / Settings.FoamSize)),
                                         (ConstantWaterParams.KWS_FoamColor, Settings.FoamColor));

            }

            if (Settings.UseVolumetricLight)
            {
                var volumeLightMaxDist = Mathf.Max(0.3f, Settings.Transparent * 3);
                volumeLightMaxDist = Mathf.Min(40, volumeLightMaxDist);
                KWS_CoreUtils.SetFloats(shader, (VolumetricLightConstantsID.KWS_VolumeLightMaxDistance, volumeLightMaxDist),
                                        (VolumetricLightConstantsID.KWS_VolumeLightBlurRadius, Settings.VolumetricLightBlurRadius));
                KWS_CoreUtils.SetInts(shader, (VolumetricLightConstantsID.KWS_RayMarchSteps, Settings.VolumetricLightIteration));
            }

            if (Settings.UseCausticEffect)
            {
                KWS_CoreUtils.SetFloats(shader, (CausticID.KW_DecalScale, KWS_Settings.Caustic.LodSettings[Settings.CausticActiveLods - 1] * 2));
                KWS_CoreUtils.SetKeywords(shader,
                                          (CausticKeywords.USE_LOD1, Settings.CausticActiveLods == 2),
                                          (CausticKeywords.USE_LOD2, Settings.CausticActiveLods == 3),
                                          (CausticKeywords.USE_LOD3, Settings.CausticActiveLods == 4),
                                          (CausticKeywords.USE_DEPTH_SCALE, Settings.UseDepthCausticScale));
            }

            if (Settings.UseUnderwaterEffect)
            {
            }


            if (Settings.UseTesselation)
            {
                var currentTessFactor = Settings.WaterMeshType switch
                {
                    WaterMeshTypeEnum.FiniteBox => KWS_Settings.Water.MaxTesselationFactorFinite,
                    WaterMeshTypeEnum.InfiniteOcean => KWS_Settings.Water.MaxTesselationFactorInfinite,
                    WaterMeshTypeEnum.River => KWS_Settings.Water.MaxTesselationFactorRiver,
                    _ => KWS_Settings.Water.MaxTesselationFactorOther
                };
                KWS_CoreUtils.SetFloats(shader, (ConstantWaterParams._TesselationMaxDisplace, Mathf.Max(Settings.WindSpeed, 2)),
                                        (ConstantWaterParams._TesselationFactor, currentTessFactor),
                                        (ConstantWaterParams._TesselationMaxDistance, Settings.WaterMeshType == WaterMeshTypeEnum.InfiniteOcean
                                             ? Settings.TesselationInfiniteMeshMaxDistance
                                             : Settings.TesselationOtherMeshMaxDistance)
                );
            }
        }

        #endregion

    }

}