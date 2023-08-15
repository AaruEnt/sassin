using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR;


namespace KWS
{
    public class PlanarReflection : ReflectionPass
    {
        private WaterSystem _waterInstance;
        private WaterSystemScriptableData _waterSettings;

        private GameObject _reflCameraGo;
        private Camera _reflectionCamera;
        private Transform _reflectionCameraTransform;

        RenderTexture _planarRT;
        RenderTexture _planarLeftEyeRT;
        RenderTexture _planarRightEyeRT;
       
        private Material _stereoBlitter;

        private bool _isTexturesInitialized;

        public override void RenderReflection(WaterSystem waterInstance, Camera currentCamera)
        {
            _waterInstance = waterInstance;
            _waterSettings = waterInstance.Settings;

            if (!_waterSettings.UsePlanarReflection) return;

            if (!_waterSettings.EnabledMeshRendering) return;
            if (!KWS_CoreUtils.CanRenderWaterForCurrentCamera(waterInstance, currentCamera)) return;

            if (!_isTexturesInitialized) ReinitializeTextures(waterInstance);

            RenderPlanar(waterInstance, currentCamera);
        }
        private void OnWaterSettingsChanged(WaterSystem waterInstance, WaterSystem.WaterTab changedTab)
        {
            if (!changedTab.HasFlag(WaterSystem.WaterTab.Reflection)) return;

            ReinitializeTextures(waterInstance);
        }

        public override void OnEnable()
        {
            WaterSystem.OnWaterSettingsChanged += OnWaterSettingsChanged;
        }

  
        public override void Release()
        {
            WaterSystem.OnWaterSettingsChanged -= OnWaterSettingsChanged;

            if (_reflectionCamera != null) _reflectionCamera.targetTexture = null;
            KW_Extensions.SafeDestroy(_reflCameraGo, _stereoBlitter);
            ReleaseTextures();
           
           // KW_Extensions.WaterLog(this, "Release", KW_Extensions.WaterLogMessageType.Release);
        }


        void ReleaseTextures()
        {
            KW_Extensions.SafeDestroy(_planarRT, _planarLeftEyeRT, _planarRightEyeRT);
            _planarRT = _planarRightEyeRT = _planarLeftEyeRT = null;
            _isTexturesInitialized = false;

        }

           // KW_Extensions.WaterLog(this, "Release", KW_Extensions.WaterLogMessageType.Release);
        void ReinitializeTextures(WaterSystem waterInstance)
        {
            ReleaseTextures();

            var format = KWS_CoreUtils.GetGraphicsFormatHDR();
            var height = (int)waterInstance.Settings.PlanarReflectionResolutionQuality;
            var width  = height * 2; // typical resolution ratio is 16x9 (or 2x1), for better pixel filling we use [2 * width] x [height], instead of square [width] * [height].

            var isSinglePassStereo = WaterSystem.IsSinglePassStereoEnabled;
            var vrUsage = VRTextureUsage.None;
#if ENABLE_VR
            if (WaterSystem.IsSinglePassStereoEnabled) vrUsage = UnityEngine.XR.XRSettings.eyeTextureDesc.vrUsage;
#endif
            var dimension          = isSinglePassStereo ? TextureDimension.Tex2DArray : TextureDimension.Tex2D;
            var slices             = isSinglePassStereo ? 2 : 1;

            _planarRT = new RenderTexture(width, height, 24, format)
            {
                name = "_planarRT", useMipMap = false, hideFlags = HideFlags.HideAndDontSave, vrUsage = vrUsage, dimension = dimension, volumeDepth = slices
            };

            if (isSinglePassStereo)
            {
                _planarLeftEyeRT  = new RenderTexture(width, height, 24, format) {name = "_planarRT_leftEye", useMipMap  = false, hideFlags = HideFlags.HideAndDontSave};
                _planarRightEyeRT = new RenderTexture(width, height, 24, format) {name = "_planarRT_rightEye", useMipMap = false, hideFlags = HideFlags.HideAndDontSave};
            }

            _isTexturesInitialized = true;

            //KW_Extensions.WaterLog(this, _planarRT);
        }


        void CreateCamera()
        {
            _reflCameraGo = ReflectionUtils.CreateReflectionCamera("WaterPlanarReflectionCamera", _waterInstance, out _reflectionCamera, out _reflectionCameraTransform);
            KWS_CoreUtils.SetPlatformSpecificPlanarReflectionParams(_reflectionCamera);
        }

        void RenderPlanar(WaterSystem waterInstance, Camera currentCamera)
        {
           
            var isSinglePassStereo = WaterSystem.IsSinglePassStereoEnabled;
            if (isSinglePassStereo && currentCamera.cameraType == CameraType.SceneView) return;

            if (_reflCameraGo == null) CreateCamera();

            var occlusionMeshDefault = false;
            if (isSinglePassStereo)
            {
                occlusionMeshDefault = XRSettings.useOcclusionMesh;
                XRSettings.useOcclusionMesh = false;
            }

            currentCamera.CopyReflectionParamsFrom(_reflectionCamera, _waterSettings.PlanarCullingMask, isCubemap: false);
            KWS_CoreUtils.UpdatePlatformSpecificPlanarReflectionParams(_reflectionCamera, _waterInstance);

            var currentCamPos = KW_Extensions.GetCameraPositionFast(currentCamera);
            var worldToCameraMatrix = currentCamera.worldToCameraMatrix;
          
            if (isSinglePassStereo)
            {
                ReflectionUtils.RenderPlanarReflection(waterInstance, _reflectionCamera, _reflectionCameraTransform, _planarLeftEyeRT, worldToCameraMatrix, currentCamera.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left), currentCamPos, true, false);
                ReflectionUtils.RenderPlanarReflection(waterInstance, _reflectionCamera, _reflectionCameraTransform, _planarRightEyeRT, worldToCameraMatrix, currentCamera.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right), currentCamPos, true, false);

                if (_stereoBlitter == null) _stereoBlitter = KWS_CoreUtils.CreateMaterial(KWS_ShaderConstants.ShaderNames.CombineSeparatedTexturesToArrayVR);
                _stereoBlitter.SetTexture("KWS_PlanarLeftEye", _planarLeftEyeRT);
                _stereoBlitter.SetTexture("KWS_PlanarRightEye", _planarRightEyeRT);
                Graphics.Blit(null, _planarRT, _stereoBlitter, 0, -1);
            }
            else ReflectionUtils.RenderPlanarReflection(waterInstance, _reflectionCamera, _reflectionCameraTransform, _planarRT, worldToCameraMatrix, currentCamera.projectionMatrix, currentCamPos, true, false);
           
            if(_waterInstance.Settings.UseAnisotropicReflections) _waterInstance.SharedData.PlanarReflectionRaw = _planarRT;
            else _waterInstance.SharedData.PlanarReflectionFinal = _planarRT;

            if (isSinglePassStereo) XRSettings.useOcclusionMesh = occlusionMeshDefault;
        }
    }
}