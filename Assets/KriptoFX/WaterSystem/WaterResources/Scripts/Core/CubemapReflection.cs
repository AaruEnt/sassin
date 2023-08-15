using UnityEngine;
using UnityEngine.Rendering;

namespace KWS
{
    public class CubemapReflection
    {
        private WaterSystem _waterInstance;
        private WaterSystemScriptableData _waterSettings;

        public Material _filteringMaterial;
        private GameObject _reflCameraGo;
        private Camera _reflectionCamera;
        private Transform _reflectionCameraTransform;

        private readonly PassData _currentPassData = new PassData();
        private readonly PassData _currentPassDataEditor = new PassData();

        private CommandBuffer _cmdAnisoFiltering;

        class PassData
        {
            public RenderTexture _cubemapRT;
            public RenderTexture _cubemapSideRawRT;
            public RenderTexture _cubemapSideFinalRT;

            public float _currentTime = 0;
            public int _sideIdx = 0;
            public bool _isTexturesInitialized;
            public int _cubemapRenderedSides = 0;

            public void InitializeTextures(WaterSystem waterInstance)
            {
                var format = KWS_CoreUtils.GetGraphicsFormatHDR();
                var size   = (int)waterInstance.Settings.CubemapReflectionResolutionQuality;
                _cubemapRT        = new RenderTexture(size, size, 0,  format) { dimension = TextureDimension.Cube, useMipMap = false };
                _cubemapSideRawRT = new RenderTexture(size, size, 24, format) { name      = "_cubemapRT_Side", useMipMap     = false };
                if (waterInstance.Settings.UseAnisotropicReflections) _cubemapSideFinalRT = new RenderTexture(size, size, 24, format) { name = "_cubemapRT_Side", useMipMap = false };

                _cubemapRenderedSides  = 0;
                _isTexturesInitialized = true;
            }

            public void Release()
            {
                KW_Extensions.SafeDestroy(_cubemapRT, _cubemapSideRawRT, _cubemapSideFinalRT);
                _cubemapRT             = _cubemapSideRawRT = _cubemapSideFinalRT = null;
                _currentTime           = 0;
                _sideIdx               = 0;
                _isTexturesInitialized = false;
                _cubemapRenderedSides  = 0;
            }
        }

        public CubemapReflection(WaterSystem waterInstance)
        {
            _waterInstance = waterInstance;
            _waterSettings = waterInstance.Settings;

            _waterInstance.OnUpdate += OnUpdate;
            WaterSystem.OnWaterSettingsChanged += OnWaterSettingsChanged;
        }

        private void OnUpdate(WaterSystem waterInstance, Camera camera)
        {
            _waterInstance = waterInstance;
            _waterSettings = waterInstance.Settings;
            RenderReflection(waterInstance, camera, _currentPassData);
        }

        void RenderReflection(WaterSystem waterInstance, Camera currentCamera, PassData passData)
        {
            if (!_waterSettings.EnabledMeshRendering) return;
            if (!KWS_CoreUtils.CanRenderWaterForCurrentCamera(waterInstance, currentCamera)) return;

            if (!passData._isTexturesInitialized) passData.InitializeTextures(waterInstance);
            if (_reflCameraGo == null)
            {
                _reflCameraGo = ReflectionUtils.CreateReflectionCamera("WaterCubemapReflectionCamera", _waterInstance, out _reflectionCamera, out _reflectionCameraTransform);
                KWS_CoreUtils.SetPlatformSpecificCubemapReflectionParams(_reflectionCamera);
            }
            RenderCubemap(waterInstance, currentCamera, passData);
        }
        private void OnWaterSettingsChanged(WaterSystem waterInstance, WaterSystem.WaterTab changedTab)
        {
            if (!changedTab.HasFlag(WaterSystem.WaterTab.Reflection)) return;

            _currentPassData.Release();
            _currentPassDataEditor.Release();

            _currentPassData.InitializeTextures(waterInstance);
            _currentPassDataEditor.InitializeTextures(waterInstance);

        }
        public void Release()
        {
            WaterSystem.OnWaterSettingsChanged -= OnWaterSettingsChanged;

            _currentPassData.Release();
            _currentPassDataEditor.Release();

            KW_Extensions.SafeDestroy(_reflCameraGo, _filteringMaterial);
            //KW_Extensions.WaterLog(this, "Release", KW_Extensions.WaterLogMessageType.Release);
        }


        void RenderCamera(WaterSystem waterInstance, RenderTexture target, Matrix4x4 cameraMatrix, Matrix4x4 projectionMatrix, Vector3 currentCameraPos, bool useCubemapCameraPosition = false)
        {
            //avoid flickering in frame debugger with cubemap rendering
#if UNITY_EDITOR
            var focusedWindow = UnityEditor.EditorWindow.focusedWindow;
            if (focusedWindow != null && focusedWindow.titleContent.text == "Frame Debug") return;
#endif

            if (useCubemapCameraPosition)
            {
                _reflectionCamera.worldToCameraMatrix = cameraMatrix;
                _reflectionCamera.transform.position = currentCameraPos;
                _reflectionCamera.projectionMatrix = projectionMatrix;
                _reflectionCamera.targetTexture = target;
                _reflectionCamera.enabled = false;
                _reflectionCamera.Render();
            }
            else
            {
                ReflectionUtils.RenderPlanarReflection(waterInstance, _reflectionCamera, _reflectionCameraTransform, target, cameraMatrix, projectionMatrix, currentCameraPos, false, true);
            }
        }


        void RenderAnisotropicBlur(PassData passData, CubemapFace face = CubemapFace.Unknown)
        {
            if (_cmdAnisoFiltering == null) _cmdAnisoFiltering = new CommandBuffer() { name = "Water.AnisotropicCubemapReflectionPass" };
            else _cmdAnisoFiltering.Clear();

            if (_filteringMaterial == null)
            {
                _filteringMaterial = KWS_CoreUtils.CreateMaterial(KWS_ShaderConstants.ShaderNames.ReflectionFiltering);
                _waterInstance.AddShaderToWaterRendering(_filteringMaterial);
            }

            var scale = 0.75f;
            if (face == CubemapFace.PositiveY) scale = 0;
            _filteringMaterial.SetFloat(KWS_ShaderConstants.ConstantWaterParams.KWS_AnisoReflectionsScale, scale);

            float isCubemapSide = (face == CubemapFace.NegativeY || face == CubemapFace.PositiveY) ? 1 : 0;
            _filteringMaterial.SetFloat(KWS_ShaderConstants.ReflectionsID.KWS_IsCubemapSide, isCubemapSide);

            _cmdAnisoFiltering.BlitTriangle(passData._cubemapSideRawRT, Vector4.one, passData._cubemapSideFinalRT, _filteringMaterial, _waterInstance.Settings.AnisotropicReflectionsHighQuality ? 3 : 2);
            Graphics.ExecuteCommandBuffer(_cmdAnisoFiltering);
        }

        void ReinitializeTextures(PassData passData)
        {
            passData.Release();
        }

        private bool _renderCubemapUsingTimeSlicing;

        void RenderCubemap(WaterSystem waterInstance, Camera currentCamera, PassData passData)
        {
            var forceUpdate = passData._cubemapRenderedSides < 6;
           
            if (KW_Extensions.TotalTime() - passData._currentTime > _waterSettings.CubemapUpdateInterval)
            {
                passData._currentTime = KW_Extensions.TotalTime();
                _renderCubemapUsingTimeSlicing = true;
            }

            if (forceUpdate)
            {
                CopyCameraParams(currentCamera, out var projectionMatrix);

                RenderToCubemapFace(waterInstance, currentCamera, passData, CubemapFace.NegativeX, projectionMatrix, _waterSettings.FixCubemapIndoorSkylightReflection);
                RenderToCubemapFace(waterInstance, currentCamera, passData, CubemapFace.NegativeY, projectionMatrix, _waterSettings.FixCubemapIndoorSkylightReflection);
                RenderToCubemapFace(waterInstance, currentCamera, passData, CubemapFace.NegativeZ, projectionMatrix, _waterSettings.FixCubemapIndoorSkylightReflection);
                RenderToCubemapFace(waterInstance, currentCamera, passData, CubemapFace.PositiveX, projectionMatrix, _waterSettings.FixCubemapIndoorSkylightReflection);
                RenderToCubemapFace(waterInstance, currentCamera, passData, CubemapFace.PositiveY, projectionMatrix, _waterSettings.FixCubemapIndoorSkylightReflection);
                RenderToCubemapFace(waterInstance, currentCamera, passData, CubemapFace.PositiveZ, projectionMatrix, _waterSettings.FixCubemapIndoorSkylightReflection);
                passData._cubemapRenderedSides = 6;

                _waterInstance.SharedData.CubemapReflection = passData._cubemapRT;
            }
            else if (_renderCubemapUsingTimeSlicing)
            {
                if (passData._sideIdx == 6)
                {
                    passData._sideIdx = 0;
                    _renderCubemapUsingTimeSlicing = false;
                }
                if (passData._sideIdx == 3) passData._sideIdx++;

                CopyCameraParams(currentCamera, out var projectionMatrix);
                RenderToCubemapFace(waterInstance, currentCamera, passData, (CubemapFace)passData._sideIdx, projectionMatrix, _waterSettings.FixCubemapIndoorSkylightReflection);
                passData._sideIdx++;

                _waterInstance.SharedData.CubemapReflection = passData._cubemapRT;
            }

            //_waterInstance.SharedData.CubemapReflection = passData._cubemapRT;
            //_waterInstance.UpdateAllShaders(WaterSystem.ShaderUpdateState.WaterSharedData, WaterSystem.WaterTab.Reflection);

        }

        private void CopyCameraParams(Camera currentCamera, out Matrix4x4 projectionMatrix)
        {
            var cullingMask = _waterSettings.FixCubemapIndoorSkylightReflection ? _waterSettings.CubemapCullingMaskWithIndoorSkylingReflectionFix : _waterSettings.CubemapCullingMask;
            currentCamera.CopyReflectionParamsFrom(_reflectionCamera, cullingMask, isCubemap: true);
            projectionMatrix = Matrix4x4.Perspective(90, 1, currentCamera.nearClipPlane, currentCamera.farClipPlane);
        }

        void RenderToCubemapFace(WaterSystem waterInstance, Camera currentCamera, PassData passData, CubemapFace face, Matrix4x4 projectionMatrix, bool useCubemapCameraPosition)
        {
            var camPos = currentCamera.transform.position;
            var viewMatrix = Matrix4x4.Inverse(Matrix4x4.TRS(camPos, GetRotationByCubeFace(face), new Vector3(1, 1, -1)));

            RenderCamera(waterInstance, passData._cubemapSideRawRT, viewMatrix, projectionMatrix, camPos, useCubemapCameraPosition);
            if (_waterInstance.Settings.UseAnisotropicReflections) RenderAnisotropicBlur(passData, face);

            var sourceRT = _waterInstance.Settings.UseAnisotropicReflections ? passData._cubemapSideFinalRT : passData._cubemapSideRawRT;
            if (sourceRT == null) return;
            Graphics.CopyTexture(sourceRT, 0, passData._cubemapRT, (int)face);
            //Debug.Log("UpdateCubemap");
        }

        Quaternion GetRotationByCubeFace(CubemapFace face)
        {
            switch (face)
            {
                case CubemapFace.NegativeX: return Quaternion.Euler(0, -90, 0);
                case CubemapFace.PositiveX: return Quaternion.Euler(0, 90, 0);
                case CubemapFace.PositiveY: return Quaternion.Euler(90, 0, 0);
                case CubemapFace.NegativeY: return Quaternion.Euler(-90, 0, 0);
                case CubemapFace.PositiveZ: return Quaternion.Euler(0, 0, 0);
                case CubemapFace.NegativeZ: return Quaternion.Euler(0, -180, 0);
            }
            return Quaternion.identity;
        }
    }
}