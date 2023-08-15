#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using static KWS.KWS_EditorUtils;
using Debug = UnityEngine.Debug;
using static KWS.WaterSystem;
using static KWS.KWS_Settings;
using Description = KWS.KWS_EditorTextDescription;

namespace KWS
{
    [System.Serializable]
    [CustomEditor(typeof(WaterSystem))]
    public partial class KWS_Editor : Editor
    {
        private WaterSystem _waterSystem;
        private WaterSystemScriptableData _settings;

        private bool _isActive;
        private WaterEditorModeEnum _waterEditorMode;
        private WaterEditorModeEnum _waterEditorModeLast;


        static KWS_EditorProfiles.PerfomanceProfiles.Reflection _reflectionProfile;
        static KWS_EditorProfiles.PerfomanceProfiles.ColorRerfraction _colorRefractionProfile;
        static KWS_EditorProfiles.PerfomanceProfiles.Flowing _flowingProfile;
        static KWS_EditorProfiles.PerfomanceProfiles.DynamicWaves _dynamicWavesProfile;
        static KWS_EditorProfiles.PerfomanceProfiles.Shoreline _shorelineProfile;
        static KWS_EditorProfiles.PerfomanceProfiles.Foam _foamProfile;
        static KWS_EditorProfiles.PerfomanceProfiles.VolumetricLight _volumetricLightProfile;
        static KWS_EditorProfiles.PerfomanceProfiles.Caustic _causticProfile;
        static KWS_EditorProfiles.PerfomanceProfiles.Mesh _meshProfile;
        static KWS_EditorProfiles.PerfomanceProfiles.Rendering _renderingProfile;

        private KWS_EditorShoreline shorelineEditor = new KWS_EditorShoreline();
        private KWS_EditorFlowmap flowmapEditor = new KWS_EditorFlowmap();
        private KWS_EditorCaustic causticEditor = new KWS_EditorCaustic();

        private KWS_EditorSplineMesh _splineMeshEditor;


        private SceneView.SceneViewState _lastSceneView;


        private KWS_EditorSplineMesh SplineMeshEditor
        {
            get
            {
                if (_splineMeshEditor == null) _splineMeshEditor = new KWS_EditorSplineMesh(_waterSystem);
                return _splineMeshEditor;
            }
        }

        enum WaterEditorModeEnum
        {
            Default,
            ShorelineEditor,
            FlowmapEditor,
            FluidsEditor,
            CausticEditor,
            SplineMeshEditor
        }

        string GetProfileName()
        {
            return $"{GetNormalizedSceneName() }.{KWS_Settings.ResourcesPaths.WaterSettingsProfileAssetName}";
        }

        internal WaterSystemScriptableData SaveToProfile()
        {
#if UNITY_EDITOR
            var path = GetProfileName();
            return _waterSystem.Settings.SaveScriptableData(_waterSystem.WaterInstanceID, GetProfileName(), "WaterProfile");

#else
            Debug.LogError("You can't save settings data in runtime");
            return;
#endif
        }


        void OnDestroy()
        {
            KWS_EditorUtils.Release();
        }


        public override void OnInspectorGUI()
        {
            _waterSystem = (WaterSystem)target;


            if (_waterSystem.enabled && _waterSystem.gameObject.activeSelf && _waterSystem.IsEditorAllowed())
            {
                _isActive = true;
                GUI.enabled = true;
            }
            else
            {
                _isActive = false;
                GUI.enabled = false;
            }

            UpdateWaterGUI();

        }


        void OnEnable()
        {
            SceneView.duringSceneGui += OnSceneGUICustom;
        }


        void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUICustom;
        }

        void OnSceneGUICustom(SceneView sceneView)
        {
            DrawWaterEditor();
        }


        void DrawWaterEditor()
        {
            if (!_isActive) return;

            if (_waterSystem.ShorelineInEditMode) _waterEditorMode = WaterEditorModeEnum.ShorelineEditor;
            else if (_waterSystem.FlowMapInEditMode) _waterEditorMode = WaterEditorModeEnum.FlowmapEditor;
            else if (_waterSystem.FluidsSimulationInEditMode()) _waterEditorMode = WaterEditorModeEnum.FluidsEditor;
            else if (_waterSystem.CausticDepthScaleInEditMode) _waterEditorMode = WaterEditorModeEnum.CausticEditor;
            else if (_waterSystem.SplineMeshInEditMode) _waterEditorMode = WaterEditorModeEnum.SplineMeshEditor;
            else _waterEditorMode = WaterEditorModeEnum.Default;

            switch (_waterEditorMode)
            {
                case WaterEditorModeEnum.Default:
                    break;
                case WaterEditorModeEnum.ShorelineEditor:
                    shorelineEditor.DrawShorelineEditor(_waterSystem);
                    _waterSystem.ShowShorelineMap = true;
                    break;
                case WaterEditorModeEnum.FlowmapEditor:
                    flowmapEditor.DrawFlowMapEditor(_waterSystem, this);
                    _waterSystem.ShowFlowMap = true;
                    break;
                case WaterEditorModeEnum.CausticEditor:
                    causticEditor.DrawCausticEditor(_waterSystem);
                    _waterSystem.ShowCausticEffectSettings = true;
                    break;
                case WaterEditorModeEnum.SplineMeshEditor:
                    SplineMeshEditor.DrawSplineMeshEditor(target);
                    _waterSystem.ShowMeshSettings = true;
                    break;
            }

            if (_waterEditorMode != WaterEditorModeEnum.Default || _waterEditorModeLast != _waterEditorMode) Repaint();
            _waterEditorModeLast = _waterEditorMode;
        }

        void UpdateWaterGUI()
        {
            _settings = _waterSystem.Settings;

            Undo.RecordObject(_waterSystem.Settings, "Changed water parameters");
#if KWS_DEBUG
            WaterSystem.Test4 = EditorGUILayout.Vector4Field("Test4", WaterSystem.Test4);
            if (WaterSystem.IsSinglePassStereoEnabled) VRScale = Slider("VR Scale", "", VRScale, 0.5f, 2.5f, "");
#endif
            //waterSystem.TestObj = (GameObject) EditorGUILayout.ObjectField(waterSystem.TestObj, typeof(GameObject), true);

            var lastScene = SceneView.lastActiveSceneView;
            if (lastScene != null)
            {
                lastScene.sceneViewState.SetAllEnabled(true);
                lastScene.sceneViewState.alwaysRefresh = true;
                lastScene.sceneLighting = true;
            }

            EditorGUI.BeginChangeCheck();


            CheckMessages();

            var isActiveTab = _waterEditorMode == WaterEditorModeEnum.Default && _isActive;
            GUI.enabled = isActiveTab;

            bool defaultVal = false;

            var isProfileNonSynchronized = !_waterSystem.Settings.CompareValues(_waterSystem.Profile);
            EditorGUILayout.LabelField("Settings are not synchronized with the profile", isProfileNonSynchronized ? NotesLabelStyleInfo : NotesLabelStyleEmpty);

            _waterSystem.Profile = (WaterSystemScriptableData)EditorGUILayout.ObjectField("Settings Profile", _waterSystem.Profile, typeof(WaterSystemScriptableData), true);
            GUILayout.BeginHorizontal();

            if (SaveButton("Save to Profile", isProfileNonSynchronized, UnityEditor.EditorStyles.miniButtonLeft))
            {
                if (_waterSystem.Profile != null)
                {
                    if (EditorUtility.DisplayDialog("Are you sure you want to overwrite the profile?", Description.Flowing.LoadLatestSaved, "Yes", "Cancel"))
                    {
                        _waterSystem.Profile = SaveToProfile();
                        Debug.Log("Water settings saved to profile file " + GetProfileName());
                    }
                }
                else
                {
                    _waterSystem.Profile = SaveToProfile();
                    Debug.Log("Water settings saved to profile file " + GetProfileName());
                }

            }

            if (SaveButton("Load from Profile", false, UnityEditor.EditorStyles.miniButtonRight))
            {
                if (_waterSystem.Profile != null)
                {
                    if (EditorUtility.DisplayDialog("Are you sure you want to overwrite the current water settings?", Description.Flowing.LoadLatestSaved, "Yes", "Cancel"))
                    {
                        _waterSystem.Settings = ScriptableObject.Instantiate(_waterSystem.Profile);
                        EditorUtility.SetDirty(_waterSystem.Settings);
                        _settings = _waterSystem.Settings;
                        Debug.Log("Water settings loaded from profile file " + GetProfileName());
                        _waterSystem.UpdateState();
                    }
                }
                else
                {
                    KWS_EditorUtils.DisplayMessageNotification("Unable to load settings, profile not selected", false);
                }

            }

            GUILayout.EndHorizontal();



            EditorGUILayout.Space(20);

            KWS_Tab(_waterSystem, ref _waterSystem.ShowColorSettings, false, false, ref defaultVal, null, "Color Settings", ColorSettings, WaterTab.ColorSettings);
            KWS_Tab(_waterSystem, ref _waterSystem.ShowWaves, false, false, ref defaultVal, null, "Waves", WavesSettings, WaterTab.Waves);

            KWS_Tab(_waterSystem, ref _waterSystem.ShowReflectionSettings, true, true, ref _waterSystem.ShowExpertReflectionSettings, _reflectionProfile, "Reflection", ReflectionSettings, WaterTab.Reflection);
            KWS_Tab(_waterSystem, ref _waterSystem.ShowRefractionSettings, true, false, ref defaultVal, _colorRefractionProfile, "Color Refraction", RefractionSetting, WaterTab.ColorRefraction);

            KWS_Tab(_waterSystem, isActiveTab, ref _settings.UseFlowMap, ref _waterSystem.ShowFlowMap, true, ref _waterSystem.ShowExpertFlowmapSettings, _flowingProfile, "Flowing", FlowingSettings, WaterTab.Flowing);
            KWS_Tab(_waterSystem, isActiveTab, ref _settings.UseDynamicWaves, ref _waterSystem.ShowDynamicWaves, false, ref defaultVal, _dynamicWavesProfile, "Dynamic Waves", DynamicWavesSettings, WaterTab.DynamicWaves);
            KWS_Tab(_waterSystem, isActiveTab, ref _settings.UseShorelineRendering, ref _waterSystem.ShowShorelineMap, false, ref defaultVal, _shorelineProfile, "Shoreline", ShorelineSetting, WaterTab.Shoreline);
            KWS_Tab(_waterSystem, isActiveTab, ref _settings.UseFoamRendering, ref _waterSystem.ShowFoamSettings, false, ref defaultVal, _foamProfile, "Foam(beta)", FoamSetting, WaterTab.Foam);
            KWS_Tab(_waterSystem, isActiveTab, ref _settings.UseVolumetricLight, ref _waterSystem.ShowVolumetricLightSettings, false, ref defaultVal, _volumetricLightProfile, "Volumetric Lighting", VolumetricLightingSettings, WaterTab.VolumetricLighting);
            KWS_Tab(_waterSystem, isActiveTab, ref _settings.UseCausticEffect, ref _waterSystem.ShowCausticEffectSettings, true, ref _waterSystem.ShowExpertCausticEffectSettings, _causticProfile, "Caustic", CausticSettings, WaterTab.Caustic);
            KWS_Tab(_waterSystem, isActiveTab, ref _settings.UseUnderwaterEffect, ref _waterSystem.ShowUnderwaterEffectSettings, false, ref defaultVal, null, "Underwater", UnderwaterSettings, WaterTab.Underwater);

            KWS_Tab(_waterSystem, ref _waterSystem.ShowMeshSettings, true, false, ref defaultVal, _meshProfile, "Mesh", MeshSettings, WaterTab.Mesh);
            KWS_Tab(_waterSystem, ref _waterSystem.ShowRendering, true, false, ref defaultVal, _renderingProfile, "Rendering", RenderingSetting, WaterTab.Rendering);


            GUI.enabled = isActiveTab;

            if (!_settings.UseFlowMap || !_isActive) _waterSystem.FlowMapInEditMode = false;
            if (!_settings.UseShorelineRendering || !_isActive) _waterSystem.ShorelineInEditMode = false;

            EditorGUILayout.LabelField("Water unique ID: " + _waterSystem.WaterInstanceID, KWS_EditorUtils.NotesLabelStyleFade);

            if (EditorGUI.EndChangeCheck())
            {
                if (!Application.isPlaying)
                {
                    EditorUtility.SetDirty(_waterSystem.Settings);
                    EditorSceneManager.MarkSceneDirty(_waterSystem.gameObject.scene);
                }
            }

        }

        void CheckMessages()
        {
            CheckPlatformSpecificMessages();
            if (_settings.UseFlowMap && !_waterSystem.IsFlowmapInitialized()) KWS_EditorMessage(Description.Flowing.FlowingNotInitialized, MessageType.Warning);
            if (WaterSystem.SelectedThirdPartyFogMethod > 0) KWS_EditorMessage(Description.Rendering.ThirdPartyFogWarnign, MessageType.Warning);
        }

        void ColorSettings()
        {
            _settings.Transparent = Slider("Transparent", Description.Color.Transparent, _settings.Transparent, 0.1f, 50f, nameof(_settings.Transparent));
            _settings.WaterColor = ColorField("Water Color", Description.Color.WaterColor, _settings.WaterColor, false, false, false, nameof(_settings.WaterColor));
            _settings.Turbidity = Slider("Turbidity", Description.Color.Turbidity, _settings.Turbidity, 0.05f, 1f, nameof(_settings.Turbidity));
            _settings.TurbidityColor = ColorField("Turbidity Color", Description.Color.TurbidityColor, _settings.TurbidityColor, false, false, false, nameof(_settings.TurbidityColor));
        }

        void WavesSettings()
        {
            _settings.FFT_SimulationSize = (FFT_GPU.SizeSetting)EnumPopup("Waves Detailing", Description.Waves.FFT_SimulationSize, _settings.FFT_SimulationSize, nameof(_settings.FFT_SimulationSize));
            _settings.WindSpeed = Slider("Wind Speed", Description.Waves.WindSpeed, _settings.WindSpeed, 0.1f, 15.0f, nameof(_settings.WindSpeed));
            _settings.WindRotation = Slider("Wind Rotation", Description.Waves.WindRotation, _settings.WindRotation, 0.0f, 360.0f, nameof(_settings.WindRotation));
            _settings.WindTurbulence = Slider("Wind Turbulence", Description.Waves.WindTurbulence, _settings.WindTurbulence, 0.0f, 1.0f, nameof(_settings.WindTurbulence));
            _settings.TimeScale = Slider("Time Scale", Description.Waves.TimeScale, _settings.TimeScale, 0.0f, 2.0f, nameof(_settings.TimeScale));
        }

        void ReflectionSettings()
        {
            //KWS_EditorProfiles.PerfomanceProfiles.Reflection.ReadDataFromProfile(_waterSystem);

            _settings.UseScreenSpaceReflection = Toggle("Use Screen Space Reflection", Description.Reflection.UseScreenSpaceReflection, _settings.UseScreenSpaceReflection, nameof(_settings.UseScreenSpaceReflection));

            if (_settings.UseScreenSpaceReflection)
            {

                _settings.ScreenSpaceReflectionResolutionQuality =
                    (ScreenSpaceReflectionResolutionQualityEnum)EnumPopup("Screen Space Resolution Quality", Description.Reflection.ScreenSpaceReflectionResolutionQuality, _settings.ScreenSpaceReflectionResolutionQuality,
                                                                           nameof(_settings.ScreenSpaceReflectionResolutionQuality));


                if (_settings.UsePlanarReflection)
                {
                    _settings.UseScreenSpaceReflectionHolesFilling = false;
                    EditorGUILayout.LabelField($"'Holes Filling' doesn't work with planar reflection", KWS_EditorUtils.NotesLabelStyleFade);
                    GUI.enabled = false;
                    _settings.UseScreenSpaceReflectionHolesFilling = Toggle("Holes Filling", "", _settings.UseScreenSpaceReflectionHolesFilling, nameof(_settings.UseScreenSpaceReflectionHolesFilling));
                    GUI.enabled = _isActive;
                }
                else
                {
                    _settings.UseScreenSpaceReflectionHolesFilling = Toggle("Holes Filling", "", _settings.UseScreenSpaceReflectionHolesFilling, nameof(_settings.UseScreenSpaceReflectionHolesFilling));
                }

                if (_waterSystem.ShowExpertReflectionSettings)
                {
                    _settings.ScreenSpaceBordersStretching = Slider("Borders Stretching", "", _settings.ScreenSpaceBordersStretching, 0f, 0.05f, nameof(_settings.ScreenSpaceBordersStretching));
                }
                Line();
            }

            var layerNames = new List<string>();
            for (int i = 0; i <= 31; i++) layerNames.Add(LayerMask.LayerToName(i));


            _settings.UsePlanarReflection = Toggle("Use Planar Reflection", Description.Reflection.UsePlanarReflection, _settings.UsePlanarReflection, nameof(_settings.UsePlanarReflection));
            if (_settings.UsePlanarReflection)
            {
                _settings.RenderPlanarShadows = Toggle("Planar Shadows", "", _settings.RenderPlanarShadows, nameof(_settings.RenderPlanarShadows));

                if (_waterSystem.ShowExpertReflectionSettings)
                {
                    if (Reflection.IsVolumetricsAndFogAvailable) _settings.RenderPlanarVolumetricsAndFog = Toggle("Planar Volumetrics and Fog", "", _settings.RenderPlanarVolumetricsAndFog, nameof(_settings.RenderPlanarVolumetricsAndFog));
                    if (Reflection.IsCloudRenderingAvailable) _settings.RenderPlanarClouds = Toggle("Planar Clouds", "", _settings.RenderPlanarClouds, nameof(_settings.RenderPlanarClouds));
                }

                _settings.PlanarReflectionResolutionQuality =
                    (PlanarReflectionResolutionQualityEnum)EnumPopup("Planar Resolution Quality", Description.Reflection.PlanarReflectionResolutionQuality, _settings.PlanarReflectionResolutionQuality,
                                                                      nameof(_settings.PlanarReflectionResolutionQuality));

                var planarCullingMask = MaskField("Planar Layers Mask", Description.Reflection.PlanarCullingMask, _settings.PlanarCullingMask, layerNames.ToArray(), nameof(_settings.PlanarCullingMask));
                _settings.PlanarCullingMask = planarCullingMask & ~(1 << Water.WaterLayer);

            }

            if (_waterSystem.ShowExpertReflectionSettings && (_settings.UsePlanarReflection || _settings.UseScreenSpaceReflection))
            {
                _settings.ReflectionClipPlaneOffset = Slider("Clip Plane Offset", Description.Reflection.ReflectionClipPlaneOffset, _settings.ReflectionClipPlaneOffset, 0, 0.07f,
                                                                    nameof(_settings.ReflectionClipPlaneOffset));
            }

            if (_settings.UsePlanarReflection) Line();

            _settings.CubemapReflectionResolutionQuality =
                (CubemapReflectionResolutionQualityEnum)EnumPopup("Cubemap Quality", Description.Reflection.CubemapReflectionResolutionQuality, _settings.CubemapReflectionResolutionQuality, nameof(_settings.CubemapReflectionResolutionQuality));
            if (_waterSystem.ShowExpertReflectionSettings)
            {
                if (_settings.FixCubemapIndoorSkylightReflection)
                {
                    GUI.enabled = false;
                    EditorGUILayout.LabelField("Cubemap layers mask is overridden by \"Fix Indoor Skylight Reflection\"", KWS_EditorUtils.NotesLabelStyleFade);
                    //var newMask = MaskField("Cubemap Layers Mask", Description.Reflection.CubemapCullingMask, _settings.CubemapCullingMaskWithIndoorSkylingReflectionFix, layerNames.ToArray(), nameof(_settings.CubemapCullingMask));
                    GUI.enabled = _isActive;
                    // _settings.CubemapCullingMaskWithIndoorSkylingReflectionFix = newMask;
                }
                else
                {
                    //var newMask = MaskField("Cubemap Layers Mask", Description.Reflection.CubemapCullingMask, _settings.CubemapCullingMask, layerNames.ToArray(), nameof(_settings.CubemapCullingMask));
                    //_settings.CubemapCullingMask = newMask;
                }

                _settings.CubemapUpdateInterval = Slider("Cubemap Update Delay", Description.Reflection.CubemapUpdateInterval, _settings.CubemapUpdateInterval, 1, 600, nameof(_settings.CubemapUpdateInterval));

                Line();
            }
            _settings.FixCubemapIndoorSkylightReflection = Toggle("Fix Indoor Skylight Reflection", Description.Reflection.UsePlanarCubemapReflection, _settings.FixCubemapIndoorSkylightReflection, nameof(_settings.FixCubemapIndoorSkylightReflection));

            _settings.UseAnisotropicReflections = Toggle("Use Anisotropic Reflections", Description.Reflection.UseAnisotropicReflections, _settings.UseAnisotropicReflections, nameof(_settings.UseAnisotropicReflections));
            if (_waterSystem.ShowExpertReflectionSettings && _settings.UseAnisotropicReflections)
            {
                _settings.AnisotropicReflectionsScale = Slider("Anisotropic Reflections Scale", Description.Reflection.AnisotropicReflectionsScale, _settings.AnisotropicReflectionsScale, 0.1f, 1.5f,
                                                                  nameof(_settings.AnisotropicReflectionsScale));
                _settings.AnisotropicReflectionsHighQuality = Toggle("High Quality Anisotropic", Description.Reflection.AnisotropicReflectionsHighQuality, _settings.AnisotropicReflectionsHighQuality,
                                                                        nameof(_settings.AnisotropicReflectionsHighQuality));

                if (_settings.UseScreenSpaceReflection) _settings.UseAnisotropicCubemapSkyForSSR = Toggle("Use Anisotropic Sky For SSR", "", _settings.UseAnisotropicCubemapSkyForSSR, nameof(_settings.UseAnisotropicCubemapSkyForSSR));
                Line();
            }

            Line();
            _settings.ReflectSun = Toggle("Reflect Sunlight", Description.Reflection.ReflectSun, _settings.ReflectSun, nameof(_settings.ReflectSun));
            if (_settings.ReflectSun)
            {
                _settings.ReflectedSunCloudinessStrength = Slider("Sun Cloudiness", Description.Reflection.ReflectedSunCloudinessStrength, _settings.ReflectedSunCloudinessStrength, 0.03f, 0.25f,
                                                                     nameof(_settings.ReflectedSunCloudinessStrength));
                if (_waterSystem.ShowExpertReflectionSettings)
                    _settings.ReflectedSunStrength = Slider("Sun Strength", Description.Reflection.ReflectedSunStrength, _settings.ReflectedSunStrength, 0f, 1f, nameof(_settings.ReflectedSunStrength));
            }

            CheckPlatformSpecificMessages_Reflection();

            //KWS_EditorProfiles.PerfomanceProfiles.Reflection.CheckDataChangesAnsSetCustomProfile(_settings);
        }

        void RefractionSetting()
        {
            _settings.RefractionMode = (RefractionModeEnum)EnumPopup("Refraction Mode", Description.Refraction.RefractionMode, _settings.RefractionMode, nameof(_settings.RefractionMode));

            if (_settings.RefractionMode == RefractionModeEnum.PhysicalAproximationIOR)
            {
                _settings.RefractionAproximatedDepth = Slider("Aproximated Depth", Description.Refraction.RefractionAproximatedDepth, _settings.RefractionAproximatedDepth, 0.25f, 5f, nameof(_settings.RefractionAproximatedDepth));
            }

            if (_settings.RefractionMode == RefractionModeEnum.Simple)
            {
                _settings.RefractionSimpleStrength = Slider("Strength", Description.Refraction.RefractionSimpleStrength, _settings.RefractionSimpleStrength, 0.02f, 1, nameof(_settings.RefractionSimpleStrength));
            }

            _settings.UseRefractionDispersion = Toggle("Use Dispersion", Description.Refraction.UseRefractionDispersion, _settings.UseRefractionDispersion, nameof(_settings.UseRefractionDispersion));
            if (_settings.UseRefractionDispersion)
            {
                _settings.RefractionDispersionStrength = Slider("Dispersion Strength", Description.Refraction.RefractionDispersionStrength, _settings.RefractionDispersionStrength, 0.25f, 1,
                                                                   nameof(_settings.RefractionDispersionStrength));
            }

        }


        void FlowingSettings()
        {
            EditorGUILayout.HelpBox(Description.Flowing.FlowingDescription, MessageType.Info);

            KWS_EditorTab(_waterEditorMode == WaterEditorModeEnum.FlowmapEditor, FlowmapEditModeSettings);

            EditorGUILayout.Space();
            _settings.UseFluidsSimulation = Toggle("Use Fluids Simulation", Description.Flowing.UseFluidsSimulation, _settings.UseFluidsSimulation, nameof(_settings.UseFluidsSimulation));
            if (_settings.UseFluidsSimulation)
            {
                EditorGUILayout.HelpBox(Description.Flowing.FluidSimulationUsage, MessageType.Info);

                var simPercent = _waterSystem.GetBakeSimulationPercent();
                var fluidsInfo = simPercent > 0 ? string.Concat(" (", simPercent, "%)") : string.Empty;
                if (GUILayout.Button("Bake Fluids Obstacles" + fluidsInfo))
                {
                    if (_settings.FlowingScriptableData == null || _settings.FlowingScriptableData.FlowmapTexture == null)
                    {
                        KWS_EditorUtils.DisplayMessageNotification("You haven't drawn a flow map yet. Use 'FlowMap Painter' and save the result.", false, 5);
                    }
                    else if (EditorUtility.DisplayDialog("Warning", "Baking may take about a minute (depending on the settings and power of your PC).", "Ready to wait", "Cancel"))
                    {
                        _waterSystem.FlowMapInEditMode = false;
                        _waterSystem.Editor_SaveFluidsDepth();
                        _waterSystem.BakeFluidSimulation();
                    }
                }

                if (simPercent > 0) DisplayMessageNotification("Fluids baking: " + fluidsInfo, false, 3);

                EditorGUILayout.Space();

                if (_waterSystem.ShowExpertFlowmapSettings)
                {
                    float currentRenderedPixels = _settings.FluidsSimulationIterrations * _settings.FluidsTextureSize * _settings.FluidsTextureSize * 2f; //iterations * width * height * lodLevels
                    currentRenderedPixels = (currentRenderedPixels / 1000000f);
                    EditorGUILayout.LabelField("Current rendered pixels(less is better): " + currentRenderedPixels.ToString("0.0") + " millions", KWS_EditorUtils.NotesLabelStyleFade);
                    _settings.FluidsSimulationIterrations = IntSlider("Simulation iterations", Description.Flowing.FluidsSimulationIterrations, _settings.FluidsSimulationIterrations, 1, 3,
                                                                         nameof(_settings.FluidsSimulationIterrations));
                    _settings.FluidsTextureSize = IntSlider("Fluids Texture Resolution", Description.Flowing.FluidsTextureSize, _settings.FluidsTextureSize, 368, 2048, nameof(_settings.FluidsTextureSize));
                }

                _settings.FluidsAreaSize = IntSlider("Fluids Area Size", Description.Flowing.FluidsAreaSize, _settings.FluidsAreaSize, 10, 80, nameof(_settings.FluidsAreaSize));
                _settings.FluidsSpeed = Slider("Fluids Flow Speed", Description.Flowing.FluidsSpeed, _settings.FluidsSpeed, 0.25f, 1.0f, nameof(_settings.FluidsSpeed));
                _settings.FluidsFoamStrength = Slider("Fluids Foam Strength", Description.Flowing.FluidsFoamStrength, _settings.FluidsFoamStrength, 0.0f, 1.0f, nameof(_settings.FluidsFoamStrength));
            }
        }

        void FlowmapEditModeSettings()
        {
            var isFlowEditMode = GUILayout.Toggle(_waterSystem.FlowMapInEditMode, "Flowmap Painter", "Button");
            if (_waterSystem.FlowMapInEditMode != isFlowEditMode)
            {
                if (isFlowEditMode)
                {
                    SetEditorCameraPosition(new Vector3(_settings.FlowMapAreaPosition.x, _waterSystem.WaterPivotWorldPosition.y + 10, _settings.FlowMapAreaPosition.z));
                    _waterSystem.InitializeFlowMapEditorResources();
                }
                _waterSystem.FlowMapInEditMode = isFlowEditMode;
            }


            if (_waterSystem.FlowMapInEditMode)
            {
                EditorGUILayout.HelpBox(Description.Flowing.FlowingEditorUsage, MessageType.Info);


                _settings.FlowMapAreaPosition = Vector3Field("FlowMap Area Position", Description.Flowing.FlowMapAreaPosition, _settings.FlowMapAreaPosition, nameof(_settings.FlowMapAreaPosition));
                _settings.FlowMapAreaPosition.y = _waterSystem.transform.position.y;

                EditorGUI.BeginChangeCheck();
                _settings.FlowMapAreaSize = IntSlider("Flowmap Area Size", Description.Flowing.FlowMapAreaSize, _settings.FlowMapAreaSize, 10, 16000, nameof(_settings.FlowMapAreaSize));
                if (EditorGUI.EndChangeCheck()) _waterSystem.RedrawFlowMap();


                EditorGUI.BeginChangeCheck();
                _settings.FlowMapTextureResolution = (FlowmapTextureResolutionEnum)EnumPopup("Flowmap resolution", Description.Flowing.FlowMapTextureResolution, _settings.FlowMapTextureResolution, nameof(_settings.FlowMapTextureResolution));
                if (EditorGUI.EndChangeCheck()) _waterSystem.ChangeFlowmapResolution();


                EditorGUILayout.Space();
                _settings.FlowMapSpeed = Slider("Flow Speed", Description.Flowing.FlowMapSpeed, _settings.FlowMapSpeed, 0.1f, 5f, nameof(_settings.FlowMapSpeed));


                _waterSystem.FlowMapBrushStrength = Slider("Brush Strength", Description.Flowing.FlowMapBrushStrength, _waterSystem.FlowMapBrushStrength, 0.01f, 1, nameof(_waterSystem.FlowMapBrushStrength));
                EditorGUILayout.Space();

                if (GUILayout.Button("Load Latest Saved"))
                {
                    if (EditorUtility.DisplayDialog("Load Latest Saved?", Description.Flowing.LoadLatestSaved, "Yes", "Cancel"))
                    {
                        _waterSystem.LoadFlowMap();
                        Debug.Log("Load Latest Saved");
                    }
                }

                if (GUILayout.Button("Delete All"))
                {
                    if (EditorUtility.DisplayDialog("Delete All?", Description.Flowing.DeleteAll, "Yes", "Cancel"))
                    {
                        _waterSystem.ClearFlowMap();
                        Debug.Log("Flowmap data has been deleted");
                    }
                }

                if (GUILayout.Button("Save All"))
                {
                    _waterSystem.SaveFlowMap();
                    Debug.Log("Flowmap texture saved");
                }

                GUILayout.Space(10);
                EditorGUI.BeginChangeCheck();
                _settings.FlowingScriptableData = (FlowingScriptableData)EditorGUILayout.ObjectField("Flowing Data", _settings.FlowingScriptableData, typeof(FlowingScriptableData), true);
                if (EditorGUI.EndChangeCheck())
                {
                    EditorUtility.SetDirty(_settings.FlowingScriptableData);
                    _waterSystem.ReinitializeFlowmap();
                }
                GUILayout.Space(10);

                GUI.enabled = _isActive;
            }
        }

        void DynamicWavesSettings()
        {
            EditorGUILayout.HelpBox(Description.DynamicWaves.Usage, MessageType.Warning);
            var maxTexSize = DynamicWaves.MaxDynamicWavesTexSize;

            int currentRenderedPixels = _settings.DynamicWavesAreaSize * _settings.DynamicWavesResolutionPerMeter;
            currentRenderedPixels = currentRenderedPixels * currentRenderedPixels;
            EditorGUILayout.LabelField($"Simulation rendered pixels (less is better): {KW_Extensions.SpaceBetweenThousand(currentRenderedPixels)}", KWS_EditorUtils.NotesLabelStyleFade);

            _settings.DynamicWavesAreaSize = IntSlider("Waves Area Size", Description.DynamicWaves.DynamicWavesAreaSize, _settings.DynamicWavesAreaSize, 10, 200, nameof(_settings.DynamicWavesAreaSize));
            _settings.DynamicWavesResolutionPerMeter = _settings.DynamicWavesAreaSize * _settings.DynamicWavesResolutionPerMeter > maxTexSize
                ? maxTexSize / _settings.DynamicWavesAreaSize
                : _settings.DynamicWavesResolutionPerMeter;


            _settings.DynamicWavesResolutionPerMeter = IntSlider("Detailing per meter", Description.DynamicWaves.DynamicWavesResolutionPerMeter, _settings.DynamicWavesResolutionPerMeter, 5, 50,
                                                                    nameof(_settings.DynamicWavesResolutionPerMeter));
            _settings.DynamicWavesAreaSize = _settings.DynamicWavesAreaSize * _settings.DynamicWavesResolutionPerMeter > maxTexSize
                ? maxTexSize / _settings.DynamicWavesResolutionPerMeter
                : _settings.DynamicWavesAreaSize;



            _settings.DynamicWavesPropagationSpeed = Slider("Speed", Description.DynamicWaves.DynamicWavesPropagationSpeed, _settings.DynamicWavesPropagationSpeed, 0.1f, 2, nameof(_settings.DynamicWavesPropagationSpeed));

            EditorGUILayout.Space();
            _settings.UseDynamicWavesRainEffect = Toggle("Rain Drops", Description.DynamicWaves.UseDynamicWavesRainEffect, _settings.UseDynamicWavesRainEffect, nameof(_settings.UseDynamicWavesRainEffect));
            if (_settings.UseDynamicWavesRainEffect)
            {
                _settings.DynamicWavesRainStrength = Slider("Rain Strength", Description.DynamicWaves.DynamicWavesRainStrength, _settings.DynamicWavesRainStrength, 0.01f, 1, nameof(_settings.DynamicWavesRainStrength));
            }
        }

        void ShorelineSetting()
        {
            _settings.ShorelineFoamLodQuality = (ShorelineFoamQualityEnum)EnumPopup("Foam Lod Quality", "", _settings.ShorelineFoamLodQuality, nameof(_settings.ShorelineFoamLodQuality));
            _settings.ShorelineColor = ColorField("Shoreline Color", "", _settings.ShorelineColor, false, true, true, nameof(_settings.ShorelineColor));
            if (KWS_CoreUtils.IsAtomicsSupported())
                _settings.UseShorelineFoamFastMode = Toggle("Use Fast Mode", Description.Shoreline.FoamCastShadows, _settings.UseShorelineFoamFastMode, nameof(_settings.UseShorelineFoamFastMode));
            else
            {
                EditorGUILayout.LabelField($"Fast mode is not supported on this platform, it's directX11/12 only feature", KWS_EditorUtils.NotesLabelStyleFade);
                GUI.enabled = false;
                _settings.UseShorelineFoamFastMode = Toggle("Use Fast Mode", Description.Shoreline.FoamCastShadows, _settings.UseShorelineFoamFastMode, nameof(_settings.UseShorelineFoamFastMode));
                GUI.enabled = _isActive;
            }

            _settings.ShorelineFoamReceiveDirShadows = Toggle("Receive Shadows", string.Empty, _settings.ShorelineFoamReceiveDirShadows, nameof(_settings.ShorelineFoamReceiveDirShadows));

            KWS_EditorTab(_waterEditorMode == WaterEditorModeEnum.ShorelineEditor, ShorelineEditModeSettings);
        }

        void FoamSetting()
        {
            _settings.FoamColor          = ColorField("Foam Color", "", _settings.FoamColor, false, true, true, nameof(_settings.FoamColor), false);
            _settings.FoamFadeDistance = Slider("Fade Distance", "", _settings.FoamFadeDistance, 0.01f, 10, nameof(_settings.FoamFadeDistance), false);
            _settings.FoamSize = Slider("Foam Size", "", _settings.FoamSize, 5, 50, nameof(_settings.FoamSize), false);
        }

        void ShorelineEditModeSettings()
        {
            _waterSystem.ShorelineInEditMode = GUILayout.Toggle(_waterSystem.ShorelineInEditMode, "Edit Mode", "Button");

            if (_waterSystem.ShorelineInEditMode)
            {
                _waterSystem.UndoProvider.ShorelineWaves = _waterSystem.GetShorelineWaves();
                if (_waterSystem.UndoProvider != null && _waterSystem.UndoProvider.ShorelineWaves != null)
                {
                    Undo.RecordObject(_waterSystem.UndoProvider, "Changed shoreline");
                }

                GUILayout.Space(10);
                EditorGUILayout.HelpBox(Description.Shoreline.ShorelineEditorUsage, MessageType.Info);
                GUILayout.Space(10);
                if (GUILayout.Button(new GUIContent("Add Wave")))
                {
                    shorelineEditor.AddWave(_waterSystem, shorelineEditor.GetCameraToWorldRay(), true);
                }

                if (GUILayout.Button("Delete All Waves"))
                {
                    if (EditorUtility.DisplayDialog("Delete Shoreline Waves?", Description.Shoreline.DeleteAll, "Yes", "Cancel"))
                    {
                        Debug.Log("Shoreline waves deleted");
                        shorelineEditor.RemoveAllWaves(_waterSystem);
                    }
                }

                if (GUILayout.Button("Save Changes"))
                {
                    _waterSystem.SaveShorelineWavesDataToJson();
                    Debug.Log("Shoreline Saved");
                }
                GUILayout.Space(10);
                EditorGUI.BeginChangeCheck();
                _settings.ShorelineWavesScriptableData = (ShorelineWavesScriptableData)EditorGUILayout.ObjectField("Waves Data", _settings.ShorelineWavesScriptableData, typeof(ShorelineWavesScriptableData), true);
                if (EditorGUI.EndChangeCheck())
                {
                    EditorUtility.SetDirty(_settings.ShorelineWavesScriptableData);
                    _waterSystem.ReinitializeShorelineWaves();
                }
                GUILayout.Space(10);
            }
        }

        void VolumetricLightingSettings()
        {
            CheckPlatformSpecificMessages_VolumeLight();

            _settings.VolumetricLightResolutionQuality =
                (VolumetricLightResolutionQualityEnum)EnumPopup("Resolution Quality", Description.VolumetricLight.ResolutionQuality, _settings.VolumetricLightResolutionQuality, nameof(_settings.VolumetricLightResolutionQuality));
            _settings.VolumetricLightIteration = IntSlider("Iterations", Description.VolumetricLight.Iterations, _settings.VolumetricLightIteration, 2, 8, nameof(_settings.VolumetricLightIteration));
            _settings.VolumetricLightFilter = (VolumetricLightFilterEnum)EnumPopup("Filter Mode", Description.VolumetricLight.Filter, _settings.VolumetricLightFilter, nameof(_settings.VolumetricLightFilter));

            if (_settings.VolumetricLightFilter != VolumetricLightFilterEnum.Bilateral)
                _settings.VolumetricLightBlurRadius = EditorGUILayout.Slider(new GUIContent("Blur Radius", Description.VolumetricLight.BlurRadius), _settings.VolumetricLightBlurRadius, 1, 4);

        }

        void CausticSettings()
        {
            if (_waterSystem.ShowExpertCausticEffectSettings)
            {
                float currentRenderedPixels = _settings.CausticTextureSize * _settings.CausticTextureSize * _settings.CausticActiveLods;
                currentRenderedPixels = (currentRenderedPixels / 1000000f);
                EditorGUILayout.LabelField("Simulation rendered pixels (less is better): " + currentRenderedPixels.ToString("0.0") + " millions", KWS_EditorUtils.NotesLabelStyleFade);

                _settings.UseCausticDispersion = Toggle("Use Dispersion", Description.Caustic.UseCausticDispersion, _settings.UseCausticDispersion, nameof(_settings.UseCausticDispersion));
                _settings.UseCausticBicubicInterpolation = Toggle("Use Bicubic Interpolation", Description.Caustic.UseCausticBicubicInterpolation, _settings.UseCausticBicubicInterpolation,
                                                                     nameof(_settings.UseCausticBicubicInterpolation));
            }

            var texSize = IntSlider("Texture Size", Description.Caustic.CausticTextureSize, _settings.CausticTextureSize, 256, 1024, nameof(_settings.CausticTextureSize));
            texSize = Mathf.RoundToInt(texSize / 64f);
            _settings.CausticTextureSize = (int)texSize * 64;
            if (_waterSystem.ShowExpertCausticEffectSettings)
                _settings.CausticMeshResolution = IntSlider("Mesh Resolution", Description.Caustic.CausticMeshResolution, _settings.CausticMeshResolution, 128, 512, nameof(_settings.CausticMeshResolution));
            _settings.CausticActiveLods = IntSlider("Cascades", Description.Caustic.CausticActiveLods, _settings.CausticActiveLods, 1, 4, nameof(_settings.CausticActiveLods));

            EditorGUILayout.Space();
            _settings.CausticStrength = Slider("Caustic Strength", Description.Caustic.CausticStrength, _settings.CausticStrength, 0, 2, nameof(_settings.CausticStrength));
            _settings.CausticDepthScale = Slider("Caustic Scale", Description.Caustic.CausticDepthScale, _settings.CausticDepthScale, 0.1f, 5, nameof(_settings.CausticDepthScale));
            EditorGUILayout.Space();
            //_settings.UseDepthCausticScale = Toggle("Use Depth Scaling", Description.Caustic.UseDepthCausticScale, _settings.UseDepthCausticScale, nameof(_settings.UseDepthCausticScale));

            //KWS_EditorTab(_waterEditorMode == WaterEditorModeEnum.CausticEditor, CausticEditModeSettings);
        }

        void CausticEditModeSettings()
        {
            if (_settings.UseDepthCausticScale)
            {
                _waterSystem.CausticDepthScaleInEditMode = GUILayout.Toggle(_waterSystem.CausticDepthScaleInEditMode, "Edit Mode", "Button");
                if (_waterSystem.CausticDepthScaleInEditMode)
                {
                    if (_settings.CausticOrthoDepthPosition.x > 10000000f) _settings.CausticOrthoDepthPosition = shorelineEditor.GetSceneCameraPosition();
                    _settings.CausticOrthoDepthPosition = Vector3Field("Depth Area Position", Description.Caustic.CausticOrthoDepthPosition, _settings.CausticOrthoDepthPosition, nameof(_settings.CausticOrthoDepthPosition));
                    _settings.CausticOrthoDepthPosition.y = _waterSystem.transform.position.y;

                    _settings.CausticOrthoDepthAreaSize =
                        IntSlider("Depth Area Size", Description.Caustic.CausticOrthoDepthAreaSize, _settings.CausticOrthoDepthAreaSize, 10, 8000, nameof(_settings.CausticOrthoDepthAreaSize));
                    _settings.CausticOrthoDepthTextureResolution =
                        IntSlider("Depth Texture Size", Description.Caustic.CausticOrthoDepthTextureResolution, _settings.CausticOrthoDepthTextureResolution, 128, 4096, nameof(_settings.CausticOrthoDepthTextureResolution));
                    if (GUILayout.Button("Bake Caustic Depth"))
                    {
                        _waterSystem.Editor_SaveCausticDepth();
                    }
                }
            }
        }

        void UnderwaterSettings()
        {
            _settings.UseUnderwaterBlur = Toggle("Use Blur Effect", Description.Underwater.UseUnderwaterBlur, _settings.UseUnderwaterBlur, nameof(_settings.UseUnderwaterBlur));
            if (_settings.UseUnderwaterBlur)
            {
                _settings.UnderwaterBlurRadius = Slider("Blur Radius", Description.Underwater.UnderwaterBlurRadius, _settings.UnderwaterBlurRadius, 0.1f, 5, nameof(_settings.UnderwaterBlurRadius));
            }
            _settings.UnderwaterQueue = (UnderwaterQueueEnum)EnumPopup("Underwater Queue", "", _settings.UnderwaterQueue, nameof(_settings.UnderwaterQueue));
        }

        void MeshSettings()
        {
            if (!_settings.UseTesselation)
            {
                int vertexCount = 0;
                if (_settings.CustomMesh != null) vertexCount = (int)(_settings.CustomMesh.triangles.Length / 3.0f);
                else if (_settings.WaterMeshType == WaterMeshTypeEnum.FiniteBox || _settings.WaterMeshType == WaterMeshTypeEnum.InfiniteOcean) vertexCount = _waterSystem.GetVisibleMeshTrianglesCount();
                EditorGUILayout.LabelField($"Visible water triangles: {KW_Extensions.SpaceBetweenThousand(vertexCount)}", KWS_EditorUtils.NotesLabelStyleFade);
            }
            _settings.WaterMeshType = (WaterMeshTypeEnum)EnumPopup("Render Mode", Description.Mesh.WaterMeshType, _settings.WaterMeshType, nameof(_settings.WaterMeshType));

            if (_settings.WaterMeshType == WaterMeshTypeEnum.CustomMesh)
            {
                _settings.CustomMesh = (Mesh)EditorGUILayout.ObjectField(_settings.CustomMesh, typeof(Mesh), true);
            }

            if (_settings.WaterMeshType == WaterMeshTypeEnum.River)
            {
                KWS_EditorTab(_waterEditorMode == WaterEditorModeEnum.SplineMeshEditor, SplineEditModeSettings);
            }

            if (_settings.WaterMeshType == WaterMeshTypeEnum.FiniteBox || _settings.WaterMeshType == WaterMeshTypeEnum.CustomMesh)
            {
                _settings.MeshSize = EditorGUILayout.Vector3Field("Water Mesh Size", _settings.MeshSize);
            }


            if (!_settings.UseTesselation)
            {
                switch (_settings.WaterMeshType)
                {
                    case WaterMeshTypeEnum.InfiniteOcean:
                        _settings.WaterMeshQualityInfinite = (WaterMeshQualityEnum)EnumPopup("Mesh Quality", Description.Mesh.MeshQuality, _settings.WaterMeshQualityInfinite, nameof(_settings.WaterMeshQualityInfinite));
                        break;
                    case WaterMeshTypeEnum.FiniteBox:
                        _settings.WaterMeshQualityFinite = (WaterMeshQualityEnum)EnumPopup("Mesh Quality", Description.Mesh.MeshQuality, _settings.WaterMeshQualityFinite, nameof(_settings.WaterMeshQualityFinite));
                        break;
                }
            }

            if (_settings.WaterMeshType == WaterMeshTypeEnum.InfiniteOcean)
            {
                _settings.OceanDetailingFarDistance = IntSlider("Ocean Detailing Far Distance", "", _settings.OceanDetailingFarDistance, 1000, 10000, nameof(_settings.OceanDetailingFarDistance));
            }

            if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Metal &&
                (_settings.WaterMeshType == WaterMeshTypeEnum.InfiniteOcean || _settings.WaterMeshType == WaterMeshTypeEnum.FiniteBox))
            {
                _settings.UseTesselation = false;
                EditorGUILayout.LabelField($"Tesselation on Metal API is only available for custom meshes and rivers", KWS_EditorUtils.NotesLabelStyleFade);
                GUI.enabled                      = false;
                _settings.UseTesselation = Toggle("Use Tesselation", Description.Mesh.UseTesselation, _settings.UseTesselation, nameof(_settings.UseTesselation));
                GUI.enabled                      = true;
            }
            else
            {
                _settings.UseTesselation = Toggle("Use Tesselation", Description.Mesh.UseTesselation, _settings.UseTesselation, nameof(_settings.UseTesselation));
            }
           
            if (_settings.UseTesselation)
            {
                _settings.TesselationFactor = Slider("Tesselation Factor", Description.Mesh.TesselationFactor, _settings.TesselationFactor, 0.15f, 1, nameof(_settings.TesselationFactor));

                if (_settings.WaterMeshType == WaterMeshTypeEnum.InfiniteOcean)
                    _settings.TesselationInfiniteMeshMaxDistance = Slider("Tesselation Max Distance", Description.Mesh.TesselationMaxDistance, _settings.TesselationInfiniteMeshMaxDistance, 10, 10000, "TesselationMaxDistance");
                else _settings.TesselationOtherMeshMaxDistance = Slider("Tesselation Max Distance", Description.Mesh.TesselationMaxDistance, _settings.TesselationOtherMeshMaxDistance, 10, 150, "TesselationMaxDistance");

            }

//#if KWS_DEBUG
            _waterSystem.DebugQuadtree = Toggle("Debug Quadtree", "", _waterSystem.DebugQuadtree, nameof(_waterSystem.DebugQuadtree));
//#endif
        }

        void SplineEditModeSettings()
        {

            EditorGUI.BeginChangeCheck();
            _waterSystem.SplineMeshInEditMode = GUILayout.Toggle(_waterSystem.SplineMeshInEditMode, "River Editor", "Button");
            if (EditorGUI.EndChangeCheck())
            {
                if (_waterSystem.SplineMeshInEditMode)
                {
                    _waterSystem.SplineMeshComponent.LoadOrCreateSpline(_waterSystem);
                }
                _settings.WireframeMode = _waterSystem.SplineMeshInEditMode;
            }

            if (_waterSystem.SplineMeshInEditMode)
            {
                EditorGUILayout.HelpBox(Description.Mesh.RiverUsage, MessageType.Info);
                GUILayout.Space(20);

                _settings.RiverSplineNormalOffset = Slider("Spline Normal Offset", Description.Mesh.RiverSplineNormalOffset, _settings.RiverSplineNormalOffset, 0.1f, 10, nameof(_settings.RiverSplineNormalOffset));

                EditorGUI.BeginChangeCheck();

                _settings.RiverSplineDepth = IntSlider("Selected Spline Depth", "", _settings.RiverSplineDepth, 1, 100, nameof(_settings.RiverSplineDepth));
                var loadedVertexCount = SplineMeshEditor.GetVertexCountBetweenPoints();
                if (loadedVertexCount == -1) loadedVertexCount = _settings.RiverSplineVertexCountBetweenPoints;
                var newVertexCountBetweenPoints = IntSlider("Selected Spline Vertex Count", Description.Mesh.RiverSplineVertexCountBetweenPoints,
                                                            loadedVertexCount, Water.SplineRiverMinVertexCount, Water.SplineRiverMaxVertexCount, nameof(_settings.RiverSplineVertexCountBetweenPoints));
                if (EditorGUI.EndChangeCheck())
                {
                    _settings.RiverSplineVertexCountBetweenPoints = newVertexCountBetweenPoints;
                    SplineMeshEditor.UpdateSplineParams();


                    //if (_settings.WaterMesh != null) //todo add for splineMesh
                    //{
                    //    var vertexCount = (int)(_settings.WaterMesh.triangles.Length / 3.0f);
                    //    DisplayMessageNotification($"Water mesh triangles count: {KW_Extensions.SpaceBetweenThousand(vertexCount)}", false, 1);
                    //}
                }

                if (GUILayout.Button("Add River"))
                {
                    SplineMeshEditor.AddSpline();
                }

                if (GUILayout.Button("Delete Selected River"))
                {
                    if (EditorUtility.DisplayDialog("Delete Selected River?", Description.Mesh.RiverDeleteAll, "Yes", "Cancel"))
                    {
                        SplineMeshEditor.DeleteSpline();
                        Debug.Log("Selected river deleted");
                    }
                }

                if (SaveButton("Save Changes", SplineMeshEditor.IsSplineChanged()))
                {
                    _settings.SplineScriptableData = _waterSystem.SplineMeshComponent.SaveSplineData(_waterSystem);
                    SplineMeshEditor.ResetSplineChangeStatus();
                    Debug.Log("River spline saved");
                }

                GUILayout.Space(10);
                EditorGUI.BeginChangeCheck();
                _settings.SplineScriptableData = (SplineScriptableData)EditorGUILayout.ObjectField("Spline Data", _settings.SplineScriptableData, typeof(SplineScriptableData), true);
                if (EditorGUI.EndChangeCheck())
                {
                    EditorUtility.SetDirty(_settings.SplineScriptableData);
                    _waterSystem.SplineMeshComponent.LoadOrCreateSpline(_waterSystem);
                    _waterSystem.SplineMeshComponent.UpdateAllMeshes(_waterSystem);
                }
                GUILayout.Space(10);

            }
        }

        void RenderingSetting()
        {
            ReadSelectedThirdPartyFog();
            var selectedThirdPartyFogMethod = _waterSystem.ThirdPartyFogAssetsDescription[WaterSystem.SelectedThirdPartyFogMethod];

            _settings.EnabledMeshRendering = Toggle("Enabled Mesh Rendering", "", _settings.EnabledMeshRendering, nameof(_settings.EnabledMeshRendering), false);
            _settings.UseFiltering = Toggle("Use Filtering", Description.Rendering.UseFiltering, _settings.UseFiltering, nameof(_settings.UseFiltering));
            _settings.UseAnisotropicFiltering = Toggle("Use Anisotropic Normals", Description.Rendering.UseAnisotropicFiltering, _settings.UseAnisotropicFiltering, nameof(_settings.UseAnisotropicFiltering));

            if (selectedThirdPartyFogMethod.DrawToDepth)
            {
                EditorGUILayout.LabelField($"Draw To Depth overrated by {selectedThirdPartyFogMethod.EditorName}", KWS_EditorUtils.NotesLabelStyleFade);
                GUI.enabled = false;
                _settings.DrawToPosteffectsDepth = Toggle("Draw To Depth", Description.Rendering.DrawToPosteffectsDepth, true, nameof(_settings.DrawToPosteffectsDepth));
                GUI.enabled = true;
            }
            else
            {
                _settings.DrawToPosteffectsDepth = Toggle("Draw To Depth", Description.Rendering.DrawToPosteffectsDepth, _settings.DrawToPosteffectsDepth, nameof(_settings.DrawToPosteffectsDepth));
            }


            //if (_waterSystem.UseTesselation)
            //{
            //    _waterSystem.WireframeMode = false;
            //    EditorGUILayout.LabelField($"Wireframe mode doesn't work with tesselation (water -> mesh -> use tesselation)", KWS_EditorUtils.NotesLabelStyleFade);
            //    GUI.enabled                           = false;
            //    _waterSystem.WireframeMode = Toggle("Wireframe Mode", "", _waterSystem.WireframeMode, nameof(_waterSystem.WireframeMode));
            //    GUI.enabled = _isActive;
            //}
            //else
            //{
            //    _waterSystem.WireframeMode = Toggle("Wireframe Mode", "", _waterSystem.WireframeMode, nameof(_waterSystem.WireframeMode));
            //}

            var assets = _waterSystem.ThirdPartyFogAssetsDescription;
            var fogDisplayedNames = new string[assets.Count + 1];
            for (var i = 0; i < assets.Count; i++)
            {
                fogDisplayedNames[i] = assets[i].EditorName;
            }
            EditorGUI.BeginChangeCheck();
            WaterSystem.SelectedThirdPartyFogMethod = EditorGUILayout.Popup("Third-Party Fog Support", WaterSystem.SelectedThirdPartyFogMethod, fogDisplayedNames);
            if (EditorGUI.EndChangeCheck())
            {
                UpdateThirdPartyFog();
            }
        }

        void ReadSelectedThirdPartyFog()
        {
            //load enabled third-party asset for all water instances
            if (WaterSystem.SelectedThirdPartyFogMethod == -1)
            {
                var defines = _waterSystem.ThirdPartyFogAssetsDescription.Select(n => n.ShaderDefine).ToList();
                WaterSystem.SelectedThirdPartyFogMethod = KWS_EditorUtils.GetEnabledDefineIndex(ShaderPaths.KWS_PlatformSpecificHelpers, defines);
            }

        }

        void UpdateThirdPartyFog()
        {
            if (SelectedThirdPartyFogMethod > 0)
            {
                var selectedMethod = _waterSystem.ThirdPartyFogAssetsDescription[WaterSystem.SelectedThirdPartyFogMethod];
                if (selectedMethod.IgnoreInclude) return;
                var inlcudeFileName = KW_Extensions.GetAssetsRelativePathToFile(selectedMethod.ShaderInclude, selectedMethod.AssetNameSearchPattern);
                if (inlcudeFileName == String.Empty)
                {
                    Debug.LogError($"Can't find the asset {_waterSystem.ThirdPartyFogAssetsDescription[WaterSystem.SelectedThirdPartyFogMethod].EditorName}");
                    return;
                }
            }

            //replace defines
            for (int i = 1; i < _waterSystem.ThirdPartyFogAssetsDescription.Count; i++)
            {
                var selectedMethod = _waterSystem.ThirdPartyFogAssetsDescription[i];
                SetShaderTextDefine(ShaderPaths.KWS_PlatformSpecificHelpers, selectedMethod.ShaderDefine, WaterSystem.SelectedThirdPartyFogMethod == i);
            }

            //replace paths to assets
            if (WaterSystem.SelectedThirdPartyFogMethod > 0)
            {
                var selectedMethod = _waterSystem.ThirdPartyFogAssetsDescription[WaterSystem.SelectedThirdPartyFogMethod];
                var inlcudeFileName = KW_Extensions.GetAssetsRelativePathToFile(selectedMethod.ShaderInclude, selectedMethod.AssetNameSearchPattern);
                KWS_EditorUtils.ChangeShaderTextIncludePath(KWS_Settings.ShaderPaths.KWS_PlatformSpecificHelpers, selectedMethod.ShaderDefine, inlcudeFileName);
            }

            AssetDatabase.Refresh();
        }
    }
}
#endif