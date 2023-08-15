using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
#if UNITY_EDITOR
using System.IO;
using UnityEditor;
#endif

namespace KWS
{
    [ExecuteAlways]
    [Serializable]
    [AddComponentMenu("")]
    public partial class WaterSystem : MonoBehaviour
    {
        //todo add KWS_STANDARD/KWS_HDRP/KWS_URP

        [SerializeField] public WaterSystemScriptableData Profile;
        [SerializeField] public WaterSystemScriptableData Settings;

        #region public enums

        public enum WaterProfileEnum
        {
            Custom,
            Ultra,
            High,
            Medium,
            Low,
            PotatoPC
        }

        public enum QualityEnum
        {
            High = 0,
            Medium = 1,
            Low = 2,
        }

        public enum PlanarReflectionResolutionQualityEnum
        {
            Ultra = 768,
            High = 512,
            Medium = 368,
            Low = 256,
            VeryLow = 128,
        }

        /// <summary>
        /// Resolution quality in percent relative to current screen size. For example Medium quality = 35, it's mean ScreenSize * (35 / 100)
        /// </summary>
        public enum ScreenSpaceReflectionResolutionQualityEnum
        {
            Ultra = 75,
            High = 50,
            Medium = 35,
            Low = 25,
            VeryLow = 20,
        }

        public enum CubemapReflectionResolutionQualityEnum
        {
            High = 512,
            Medium = 256,
            Low = 128,
        }

        public enum ReflectionClearFlagEnum
        {
            Skybox,
            Color,
        }


        public enum RefractionModeEnum
        {
            Simple,
            PhysicalAproximationIOR
        }

        public enum FoamShadowMode
        {
            None,
            CastOnly,
            CastAndReceive
        }

        public enum UnderwaterQueueEnum
        {
            BeforeTransparent,
            AfterTransparent
        }

        public enum WaterMeshTypeEnum
        {
            InfiniteOcean,
            FiniteBox,
            River,
            CustomMesh
        }

        public enum OceanFarDistanceEnum
        {
            _100kilometers = 200000,
            _50kilometers = 100000,
            _25kilometers = 50000,
            _12kilometers = 25000,
            _6kilometers = 12500,
            _3kilometers = 6250,
        }

        public enum WaterMeshQualityEnum
        {
            Ultra,
            High,
            Medium,
            Low,
            VeryLow,
        }

        public enum AntialiasingEnum
        {
            None = 1,
            MSAA2x = 2,
            MSAA4x = 4,
            //MSAA8x = 8
        }

        public enum VolumetricLightFilterEnum
        {
            Bilateral,
            Gaussian
        }

        public enum VolumetricLightResolutionQualityEnum
        {
            Ultra = 75,
            High = 50,
            Medium = 35,
            Low = 25,
            VeryLow = 15,
        }

        public enum FlowmapTextureResolutionEnum
        {
            _512 = 512,
            _1024 = 1024,
            _2048 = 2048,
            _4096 = 4096,
        }

        public enum ShorelineFoamQualityEnum
        {
            High,
            Medium,
            Low,
            VeryLow,
        }


        #endregion

        #region public API methods


        /// <summary>
        /// You must invoke this method every time you change any water parameter.
        /// For example
        /// waterInstance.Settings.Transparent = 5;
        /// waterInstance.Settings.UseVolumetricLighting = false;
        /// waterInstance.ForceUpdateWaterSettings();
        /// </summary>
        public void ForceUpdateWaterSettings()
        {
            UpdateState();
        }

        /// <summary>
        /// You can manually control the water rendering.
        /// For example, you can disable rendering if you go into a cave, etc
        /// </summary>
        public bool IsWaterRenderingActive;

        /// <summary>
        /// Called when visible water is visible and rendered
        /// </summary>
        public Action OnWaterRender;

        /// <summary>
        /// World space bounds of the rendered mesh (the box size relative to the wind speed).
        /// </summary>
        /// <returns></returns>
        public Bounds WorldSpaceBounds
        {
            get
            {
                switch (Settings.WaterMeshType)
                {
                    case WaterMeshTypeEnum.InfiniteOcean:
                        var underwaterOffset = Mathf.Max(0, CurrentMaxWaveHeight - KW_Extensions.GetCameraPositionFast(_currentCamera).y);
                        return new Bounds(WaterRelativeWorldPosition + new Vector3(0, -250 + CurrentMaxWaveHeight - underwaterOffset, 0), new Vector3(1000000, 500 + underwaterOffset * 2, 1000000));
                    case WaterMeshTypeEnum.FiniteBox:
                        return new Bounds(WaterRelativeWorldPosition + new Vector3(0, -Settings.MeshSize.y / 2f + CurrentMaxWaveHeight, 0), new Vector3(Settings.MeshSize.x, Settings.MeshSize.y + CurrentMaxWaveHeight * 2, Settings.MeshSize.z));
                    case WaterMeshTypeEnum.River: return SplineMeshComponent.GetBounds();
                    case WaterMeshTypeEnum.CustomMesh:
                        if (Settings.CustomMesh == null) return new Bounds(WaterPivotWorldPosition, Vector3.one);
                        var size = Settings.CustomMesh.bounds.size;
                        return new Bounds(WaterPivotWorldPosition, Vector3.Scale(size, Settings.MeshSize) + Vector3.up * CurrentMaxWaveHeight);
                }
                return new Bounds(WaterPivotWorldPosition, Vector3.one * 1000);
            }
        }

        /// <summary>
        /// Check if the current camera is under water. This method does not use an accurate calculation of waves, but instead uses approximation, so sometimes with high waves it return true.
        /// </summary>
        public bool IsCameraUnderwater { get; private set; }

        public static bool IsSphereUnderWater(Vector3 centerWorldPos, float radius)
        {

            if (IsPositionUnderWater(centerWorldPos - Vector3.one * radius)) return true;
            if (IsPositionUnderWater(centerWorldPos + Vector3.up * radius)) return true;

            if (IsPositionUnderWater(centerWorldPos + Vector3.back * radius)) return true;
            if (IsPositionUnderWater(centerWorldPos + Vector3.forward * radius)) return true;

            if (IsPositionUnderWater(centerWorldPos + Vector3.left * radius)) return true;
            if (IsPositionUnderWater(centerWorldPos + Vector3.right * radius)) return true;

            return false;
        }

        /// <summary>
        /// Check if the current world space position is under water. For example, you can detect if your character enters the water to like triggering a swimming state.
        /// </summary>
        /// <param name="worldPos"></param>
        /// <returns></returns>
        public static bool IsPositionUnderWater(Vector3 worldPos)
        {
            foreach (var instance in VisibleWaterInstances)
            {
                if (!instance.WorldSpaceBounds.Contains(worldPos)) continue;

                instance._buoyancyPassedFramesAfterRequest = 0;
                return instance.Settings.WaterMeshType == WaterMeshTypeEnum.River ? instance.IsRiverUnderwaterVisible(worldPos) : instance.IsUnderwaterVisible(worldPos);
            }

            return false;
        }

        /// <summary>
        /// Get world space water position/normal at point. Used for water physics. It check all instances
        /// </summary>
        /// <param name="worldPosition"></param>
        /// <returns></returns>
        public static WaterSurfaceData GetWaterSurfaceData(Vector3 worldPosition)
        {
            foreach (var instance in VisibleWaterInstances)
            {
                if (!instance.WorldSpaceBounds.Contains(worldPosition)) continue;

                if (instance.Settings.WaterMeshType == WaterMeshTypeEnum.River)
                {
                    if (instance.SplineMeshComponent.GetSplineSurfaceHeight(instance, worldPosition, out var surfaceWorldPos, out var surfaceNormal))
                    {
                        instance.SurfaceData.IsActualDataReady = true;
                        instance.SurfaceData.Position = surfaceWorldPos;
                        instance.SurfaceData.Normal = surfaceNormal;
                        return instance.SurfaceData;
                    }
                }
                else
                {
                    instance._buoyancyPassedFramesAfterRequest = 0;
                    if (!instance.IsWaterInitialized || !instance._isBuoyancyDataReadCompleted)
                    {
                        DefaultSurfaceData.Position = worldPosition;
                        return DefaultSurfaceData;
                    }
                    return instance.FFTComponentHeightData.GetWaterSurfaceData(worldPosition);
                }
            }

            DefaultSurfaceData.Position = worldPosition;
            return DefaultSurfaceData;
        }

        /// <summary>
        /// Activate this option if you want to manually synchronize the time for all clients over the network
        /// </summary>
        public bool UseNetworkTime;

        public float NetworkTime;


        #endregion

        #region editor variables
        internal static Action<WaterSystem, WaterTab> OnWaterSettingsChanged;

        [SerializeField] internal bool ShowColorSettings = true;
        [SerializeField] internal bool ShowExpertColorSettings = false;

        [SerializeField] internal bool ShowWaves = true;
        [SerializeField] internal bool ShowExpertWavesSettings = false;

        [SerializeField] internal bool ShowReflectionSettings = false;
        [SerializeField] internal bool ShowExpertReflectionSettings = false;

        [SerializeField] internal bool ShowRefractionSettings = false;
        [SerializeField] internal bool ShowExpertRefractionSettings = false;

        [SerializeField] internal bool ShowVolumetricLightSettings = false;
        [SerializeField] internal bool ShowExpertVolumetricLightSettings = false;

        [SerializeField] internal bool ShowFlowMap = false;
        [SerializeField] internal bool ShowExpertFlowmapSettings = false;
        [SerializeField] internal bool FlowMapInEditMode = false;
        [SerializeField] internal float FlowMapBrushStrength = 0.75f;

        [SerializeField] internal bool ShowDynamicWaves = false;
        [SerializeField] internal bool ShowExpertDynamicWavesSettings = false;

        [SerializeField] internal bool ShowShorelineMap = false;
        [SerializeField] internal bool ShorelineInEditMode = false;
        [SerializeField] internal bool ShowExpertShorelineSettings = false;

        [SerializeField] internal bool ShowFoamSettings = false;

        [SerializeField] internal bool ShowCausticEffectSettings = false;
        [SerializeField] internal bool ShowExpertCausticEffectSettings = false;
        [SerializeField] internal bool CausticDepthScaleInEditMode = false;
        [SerializeField] internal bool ShowUnderwaterEffectSettings = false;

        [SerializeField] internal bool ShowMeshSettings = false;
        [SerializeField] internal bool SplineMeshInEditMode = false;
        [SerializeField] internal bool ShowExpertMeshSettings = false;
        [SerializeField] internal bool DebugQuadtree = false;

        [SerializeField] internal bool ShowRendering = false;
        [SerializeField] internal bool ShowExpertRenderingSettings = false;

        [SerializeField] internal static int SelectedThirdPartyFogMethod = -1;

        [Serializable]
        internal class ThirdPartyAssetDescription
        {
            public string EditorName;
            public string ShaderDefine = String.Empty;
            public string ShaderInclude = String.Empty;
            public string AssetNameSearchPattern = String.Empty;
            public bool DrawToDepth;
            public int CustomQueue;
            public bool IgnoreInclude;
        }


        #endregion

        #region internal variables

        internal SharedData SharedData = new SharedData();
        internal Action<WaterSystem, Camera> OnUpdate;
        internal WaterSurfaceData SurfaceData = new WaterSurfaceData();
        internal static WaterSurfaceData DefaultSurfaceData = new WaterSurfaceData(){IsActualDataReady = false, Normal =  Vector3.up, Position = Vector3.zero};

        public string WaterInstanceID
        {
            get
            {
#if UNITY_EDITOR
                if (string.IsNullOrEmpty(_waterGUID)) _waterGUID = CreateWaterInstanceID();
                return _waterGUID;
#else
                if (string.IsNullOrEmpty(_waterGUID)) Debug.LogError("Water GUID is empty, therefore shoreline/flowing/etc will not work!");
                return _waterGUID;
#endif
            }
        }

        string CreateWaterInstanceID()
        {
#if UNITY_EDITOR
            return KWS_EditorUtils.GetNormalizedSceneName() + "." + Path.GetRandomFileName().Substring(0, 8).ToUpper();
#else
            return string.Empty;
#endif
        }

        internal void AddShaderToWaterRendering(Material mat)
        {
            if (mat == null || _waterSharedMaterials.Contains(mat)) return;
            _waterSharedMaterials.Add(mat);
            SetConstantShaderParamsShared(mat);
        }

        internal void AddShaderToWaterRendering(ComputeShader cs, params int[] kernelsForUpdate)
        {
            if (cs == null || _waterSharedComputeShaders.Contains(cs)) return;
            _waterSharedComputeShaders.Add(cs);
            SetConstantShaderParamsShared(cs);
        }

        internal void UpdateAllShadersConstantParams()
        {
            foreach (var waterMaterial in _waterSharedMaterials)
            {
                SetConstantShaderParamsShared(waterMaterial);
            }

            foreach (var cs in _waterSharedComputeShaders)
            {
                SetConstantShaderParamsShared(cs);
            }
        }

        internal Vector3 WaterRelativeWorldPosition
        {
            get
            {
                if (Settings.WaterMeshType == WaterMeshTypeEnum.InfiniteOcean)
                {
                    var pos = KW_Extensions.GetCameraPositionFast(_currentCamera);
                    pos.y = WaterPivotWorldPosition.y;
                    return pos;
                }
                else return WaterPivotWorldPosition;
            }
        }

        internal Quaternion WaterPivotWorldRotation => WaterRootTransform.rotation;
        internal Vector3 WaterPivotWorldPosition => WaterRootTransform.position;


        private Transform _waterTransform;
        internal Transform WaterRootTransform
        {
            get
            {
                if (_waterTransform == null) _waterTransform = transform;
                return _waterTransform;
            }
        }

        internal static RTHandleSystem RTHandles;
        internal GameObject WaterTemporaryObject => _tempGameObject;
        internal static List<WaterSystem> VisibleWaterInstances { get; private set; } = new List<WaterSystem>();
        internal static bool IsSinglePassStereoEnabled;
        internal static Plane[] CurrentCameraFrustumPlanes = new Plane[6];
        internal static Vector3[] CurrentCameraFrustumCorners = new Vector3[8];
        internal bool IsWaterVisible { get; private set; }
        internal bool CanRenderTesselation => Settings.UseTesselation && SystemInfo.graphicsShaderLevel >= 46 && SplineMeshInEditMode == false;
        internal float CurrentMaxWaveHeight => Mathf.Max(0, Settings.WindSpeed * 0.4f - 0.75f);
        internal float NormalizedWind => Mathf.Clamp01(Settings.WindSpeed / 15.0f);
        internal float AnisoScaleRelativeToWind => (NormalizedWind * 0.95f + 0.05f) * Settings.AnisotropicReflectionsScale * 0.15f;
        internal bool UseMultipleSimulations => Settings.WindSpeed > 1.2f;
        public Rect ScreenSpaceBounds;

        public WaterSurfaceData GetCurrentWaterSurfaceData(Vector3 worldPosition)
        {
            if (!IsWaterInitialized || !_isBuoyancyDataReadCompleted)
            {
                SurfaceData.IsActualDataReady = false;
                SurfaceData.Position = worldPosition;
                SurfaceData.Normal = Vector3.up;
                return SurfaceData;
            }
            return FFTComponentHeightData.GetWaterSurfaceData(worldPosition);
        }

        #endregion

        #region private variables

        [SerializeField] string _waterGUID;

#if KWS_DEBUG
        public static Vector4 Test4 = Vector4.zero;
        public static float VRScale = 1;
#endif

        private Camera _currentCamera;
        private bool _isSceneCameraRendered;

        private GameObject _tempGameObject;

        internal Mesh SplineRiverMesh;

        internal bool IsWaterInitialized { get; private set; }
        private bool isWaterPlatformSpecificResourcesInitialized;

        private bool _isBuoyancyDataReadCompleted;
        private int _buoyancyPassedFramesAfterRequest = Int32.MaxValue;

        List<Material> _waterSharedMaterials = new List<Material>();
        private List<ComputeShader> _waterSharedComputeShaders = new List<ComputeShader>();
      
        #endregion

        #region properties

        UndoProvider _undoProvider;
        internal UndoProvider UndoProvider
        {
            get
            {
                if (_undoProvider == null && _tempGameObject != null) _undoProvider = _tempGameObject.AddComponent<UndoProvider>();

                return _undoProvider;
            }
        }

        private MeshQuadTree _meshQuadTreeGameView = new MeshQuadTree();
        private MeshQuadTree _meshQuadTreeEditorView = new MeshQuadTree();
        internal MeshQuadTree MeshQuadTreeComponent => _isSceneCameraRendered ? _meshQuadTreeEditorView : _meshQuadTreeGameView;

        internal FFT_GPU FFTComponentLod0 = new FFT_GPU();
        internal FFT_GPU FFTComponentLod1 = new FFT_GPU();
        internal FFT_GPU FFTComponentLod2 = new FFT_GPU();
        internal KWS_FFT_ToHeightMap FFTComponentHeightData = new KWS_FFT_ToHeightMap();

        internal KW_FlowMap FlowMapComponent = new KW_FlowMap();
        internal KW_ShorelineWaves ShorelineWavesComponent = new KW_ShorelineWaves();

        KW_FluidsSimulation2D _fluidsSimulationGameView = new KW_FluidsSimulation2D();
        KW_FluidsSimulation2D _fluidsSimulationEditorView = new KW_FluidsSimulation2D();
        internal KW_FluidsSimulation2D FluidsSimulationComponent => _isSceneCameraRendered ? _fluidsSimulationEditorView : _fluidsSimulationGameView;

        internal KWS_SplineMesh SplineMeshComponent = new KWS_SplineMesh();
        internal KW_DynamicWaves DynamicWavesComponent = new KW_DynamicWaves();
        internal CubemapReflection CubemapReflectionComponent; //must be initialized later

        #endregion

        private GameObject _infiniteOceanShadowCullingAnchor;
        private Transform _infiniteOceanShadowCullingAnchorTransform;
        KW_Extensions.AsyncInitializingStatusEnum _shoreLineInitializingStatus;
        KW_Extensions.AsyncInitializingStatusEnum _flowmapInitializingStatus;

        const int BakeFluidsLimitFrames = 350;
        int currentBakeFluidsFrames = 0;

        KW_CustomFixedUpdate fixedUpdateFluids;
        KW_CustomFixedUpdate fixedUpdateBakingFluids;
        KW_CustomFixedUpdate fixedUpdateDynamicWaves;
        private bool _lastWaterVisible;

        internal static Dictionary<Camera, CameraData> CameraDatas = new Dictionary<Camera, CameraData>();
        private Camera _lastGameCamera;
        private Camera _lastEditorCamera;
        private Material _editorPropertyCheckerMaterial;

        public void InitializeComponents()
        {
            FFTComponentLod0.Initialize(this, 0);
            FFTComponentLod1.Initialize(this, 1);
            FFTComponentLod2.Initialize(this, 2);
        }

        internal class CameraData
        {
            public Transform CameraTransform;
            public Vector3 Position;
            public Vector3 LastPosition;
            public Vector3 RotationEuler;
            public Vector3 Forward;
            public CameraData(Camera camera)
            {
                CameraTransform = camera.transform;
            }

            public void Update()
            {
                LastPosition = Position;
                Position = CameraTransform.position;
                RotationEuler = CameraTransform.rotation.eulerAngles;
                Forward = CameraTransform.forward;
            }
        }


#if UNITY_EDITOR
        [MenuItem("GameObject/Effects/Water System")]
        static void CreateWaterSystemEditor(MenuCommand menuCommand)
        {
            var go = new GameObject("Water System");
            go.transform.position = SceneView.lastActiveSceneView.camera.transform.TransformPoint(Vector3.forward * 3f);
            go.AddComponent<WaterSystem>();
            go.layer = KWS_Settings.Water.WaterLayer;
            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
            Selection.activeObject = go;
        }
#endif

        void CheckPlatformSpecificFeatures()
        {
            if (!KWS_CoreUtils.IsAtomicsSupported()) Settings.UseShorelineFoamFastMode = false;
        }

        void CheckCopyPastAndReplaceInstanceID()
        {
#if UNITY_EDITOR
            Event e = Event.current;
            if (e != null && (e.commandName == "Duplicate" || e.commandName == "Paste"))
            {
                _waterGUID = CreateWaterInstanceID();
                Settings = ScriptableObject.Instantiate(Settings);
            }
#endif
        }

        internal void LoadOrCreateSettings()
        {
            if (Settings == null)
            {
                Settings = Profile != null ? ScriptableObject.Instantiate(Profile) : ScriptableObject.CreateInstance<WaterSystemScriptableData>();
            }
        }

        private Texture2D _foamTex;
        private void Awake()
        {
            LoadOrCreateSettings();
          
        }

        private void OnEnable()
        {
            WaterRootTransform.root.transform.localScale = Vector3.one;
            
            InitializeComponents();
            if (CubemapReflectionComponent == null) CubemapReflectionComponent = new CubemapReflection(this);
            PlanarReflectionComponent.OnEnable();

            _foamTex = Resources.Load<Texture2D>(KWS_Settings.ResourcesPaths.KW_Foam2);
            Shader.SetGlobalTexture("KW_FoamTex", _foamTex);

            OnWaterSettingsChanged                     += OnAnyWaterSettingsChangedEvent;
            FFTComponentHeightData.IsDataReadCompleted += FFTComponentHeightDataOnIsDataReadCompleted;
#if UNITY_EDITOR
            EditorApplication.update += EditorUpdate;
#endif
            LoadOrCreateSettings();
            CheckCopyPastAndReplaceInstanceID();

            CheckPlatformSpecificFeatures();

            SubscribeBeforeCameraRendering();
            SubscribeAfterCameraRendering();

            SetGlobalShaderParams();

            IsWaterRenderingActive = true;
        }

        void OnDestroy()
        {
            if(IsWaterInitialized) OnDisable();
        }

        void OnDisable()
        {
            OnWaterSettingsChanged                     -= OnAnyWaterSettingsChangedEvent;
            FFTComponentHeightData.IsDataReadCompleted -= FFTComponentHeightDataOnIsDataReadCompleted;
#if UNITY_EDITOR
            EditorApplication.update                                  -= EditorUpdate;
#endif
            Resources.UnloadAsset(_foamTex);
            if (VisibleWaterInstances.Contains(this)) VisibleWaterInstances.Remove(this);
            _lastWaterVisible = false;

            UnsubscribeBeforeCameraRendering();
            UnsubscribeAfterCameraRendering();

            ReleasePlatformSpecificResources();
            Release();
            IsWaterRenderingActive = false;
        }

//#if KWS_DEBUG

        void OnDrawGizmos()
        {
            if (DebugQuadtree == false) return;

            //Debug.Log("Nodes :" + MeshQuadTreeComponent.VisibleNodes.Count);
            foreach (var visibleNode in MeshQuadTreeComponent.VisibleNodes)
            {
                Gizmos.matrix = Matrix4x4.TRS(Vector3.zero, WaterPivotWorldRotation, Vector3.one);
                var heightOffset = Vector3.zero;
                if (Settings.WaterMeshType == WaterMeshTypeEnum.InfiniteOcean) heightOffset = new Vector3(0, visibleNode.CurrentSize.y * 0.5f, 0);

                //if(visibleNode.CurrentLevel == 2 && visibleNode.NeighborRight == null) Gizmos.color = Color.red;
                //else Gizmos.color = Color.white;

                Gizmos.DrawWireCube(visibleNode.CurrentCenter + heightOffset, new Vector3(visibleNode.CurrentSize.x, 0.001f, visibleNode.CurrentSize.z));
            }
        }
//#endif

        void UpdateFrustum(Camera cam)
        {
             KW_Extensions.CalculateFrustumPlanes(ref CurrentCameraFrustumPlanes, cam, Settings.WaterMeshType, CurrentMaxWaveHeight);
             KW_Extensions.CalculateFrustumCorners(ref CurrentCameraFrustumCorners, cam);
        }

        public void Update()
        {
            if (IsWaterInitialized && IsWaterRenderingActive && IsWaterVisible && Application.isPlaying)
            {
                if (_lastGameCamera != null) OnUpdate?.Invoke(this, _lastGameCamera);
            }

        }

        public void EditorUpdate()
        {
#if UNITY_EDITOR
            if (IsWaterInitialized && IsWaterRenderingActive && IsWaterVisible && !Application.isPlaying)
            {
                if (_lastEditorCamera != null) OnUpdate?.Invoke(this, _lastEditorCamera);
            }
#endif
        }

        void UpdatePerWaterInstance(Camera cam)
        {
            //Debug.Log("UpdatePerWaterInstance " + cam.name);
            _isSceneCameraRendered = _currentCamera.cameraType == CameraType.SceneView;
           
            if (IsWaterInitialized)
            {
                IsWaterVisible = IsWaterVisibleForCamera();
                UpdateActiveWaterInstances();
                if (!IsWaterVisible) return;
            }
           
            //ScreenSpaceBounds = KW_Extensions.GetScreenSpaceBounds(cam, WorldSpaceBounds);
            //Debug.Log(ScreenSpaceBounds);
            if (Settings.UseUnderwaterEffect)
            {
              
                IsCameraUnderwater = Settings.WaterMeshType switch
                {
                    WaterMeshTypeEnum.River => IsRiverUnderwaterVisible(_currentCamera),
                    _ => IsUnderwaterVisible(_currentCamera, WorldSpaceBounds)
                };
            }
           

            Profiler.BeginSample("Water.Rendering");
            OnWaterRender?.Invoke();
            CheckAndReinitializeIfRequired();
            RenderWater();
            Profiler.EndSample();
        }

        void UpdatePerCamera(Camera cam)
        {
            //Debug.Log(" UpdatePerCamera " + cam);
#if UNITY_EDITOR
            KW_Extensions.UpdateEditorDeltaTime();
#endif

            if (CameraDatas.Count > 100) CameraDatas.Clear();
            if (!CameraDatas.ContainsKey(cam)) CameraDatas.Add(cam, new CameraData(cam));
            CameraDatas[cam].Update();

#if KWS_DEBUG
            Shader.SetGlobalVector("Test4", Test4);
#if ENABLE_VR
            if (IsSinglePassStereoEnabled) UnityEngine.XR.XRSettings.eyeTextureResolutionScale = VRScale;
#endif
#endif
            SetGlobalCameraShaderParams();

            if (RTHandles == null)
            {
                RTHandles = new RTHandleSystem();
                var screenSize = KWS_CoreUtils.GetScreenSizeLimited(IsSinglePassStereoEnabled);
                RTHandles.Initialize(screenSize.x, screenSize.y);
            }
        }

        void OnBeforeCameraRendering(Camera cam)
        {
            
            if (!KWS_CoreUtils.CanRenderWaterForCurrentCamera(this, cam)) return;
           
            IsSinglePassStereoEnabled = KWS_CoreUtils.IsSinglePassStereoEnabled();
            if (IsSinglePassStereoEnabled && !KWS_CoreUtils.CanRenderSinglePassStereo(cam)) return;

            _currentCamera = cam; 
            UpdateFrustum(cam);
           
            if (cam.cameraType == CameraType.Game) _lastGameCamera = cam;
            else if (cam.cameraType == CameraType.SceneView) _lastEditorCamera = cam;
            
            if (!IsWaterInitialized || VisibleWaterInstances.Count > 0 && VisibleWaterInstances[0] == this)
            {
                UpdatePerCamera(cam);
            }
           
            UpdatePerWaterInstance(cam);
        }

        void OnAfterCameraRendering(Camera cam)
        {
            if (cam.cameraType != CameraType.Game && cam.cameraType != CameraType.SceneView) return;

#if UNITY_EDITOR
            KW_Extensions.SetEditorDeltaTime();
#endif
        }


        public void EnableBuoyancyRendering()
        {
            _isBuoyancyDataReadCompleted = false;
        }

        public void DisableBuoyancyRendering()
        {
            _isBuoyancyDataReadCompleted = false;
        }


        void Release()
        {
            IsWaterInitialized = false;

            _meshQuadTreeGameView.Release();
            _meshQuadTreeEditorView.Release();

            FFTComponentLod0.Release();
            FFTComponentLod1.Release();
            FFTComponentLod2.Release();

            FFTComponentHeightData.Release();

            ShorelineWavesComponent.Release();
            FlowMapComponent.Release();
            SplineMeshComponent.Release();

            _fluidsSimulationEditorView.Release();
            _fluidsSimulationGameView.Release();

            DynamicWavesComponent.Release();
            PlanarReflectionComponent.Release();
            if (CubemapReflectionComponent != null) CubemapReflectionComponent.Release();

            if (_infiniteOceanShadowCullingAnchor != null) KW_Extensions.SafeDestroy(_infiniteOceanShadowCullingAnchor.GetComponent<MeshFilter>().sharedMesh,
                                                                                     _infiniteOceanShadowCullingAnchor.GetComponent<MeshRenderer>().sharedMaterial);

            KW_Extensions.SafeDestroy(_tempGameObject, _infiniteOceanShadowCullingAnchor);
            if (Settings.WaterMeshType != WaterMeshTypeEnum.CustomMesh) KW_Extensions.SafeDestroy(SplineRiverMesh);

            _waterSharedMaterials.Clear();
            _waterSharedComputeShaders.Clear();

            _flowmapInitializingStatus = KW_Extensions.AsyncInitializingStatusEnum.NonInitialized;
            _shoreLineInitializingStatus = KW_Extensions.AsyncInitializingStatusEnum.NonInitialized;
            _isFluidsSimBakedMode = false;

            _isBuoyancyDataReadCompleted = false;
            _buoyancyPassedFramesAfterRequest = Int32.MaxValue;
            isWaterPlatformSpecificResourcesInitialized = false;
            IsSinglePassStereoEnabled = false;
            IsWaterVisible = false;
            _lastWaterVisible = false;
            SharedData.WaterShaderPassID = 0;

            SharedData.Release();
            CameraDatas.Clear();

            Resources.UnloadUnusedAssets();
        }
    }
}