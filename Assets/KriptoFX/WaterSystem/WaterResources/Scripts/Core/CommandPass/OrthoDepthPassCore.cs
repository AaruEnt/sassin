using System;
using UnityEngine;

namespace KWS
{
    class OrthoDepthPassCore : WaterPassCore
    {
        public Action<PassData, Camera> OnRender;
        public Action<Camera> OnInitializedDepthCamera;

        private GameObject _currentCamGO;
        private Camera _depthCamera;
        private Transform _camTransform;

        readonly PassData _currentPassData = new PassData();
        readonly PassData _currentPassDataEditor = new PassData();

        public OrthoDepthPassCore(WaterSystem waterInstance)
        {
            PassName = "Water.OrthoDepthPass";
            WaterInstance = waterInstance;
        }

        public override void Release()
        {
            KW_Extensions.SafeDestroy(_currentCamGO);
            _currentPassData.Release();
            _currentPassDataEditor.Release();
        }
        internal class PassData
        {
            public RenderTexture DepthRT;
            public Vector4 Position;
            public Vector4 NearFarSize = new Vector4(-KWS_Settings.Water.OrthoDepthAreaNearOffset, KWS_Settings.Water.OrthoDepthAreaFarOffset, KWS_Settings.Water.OrthoDepthAreaSize, 0);
            public bool IsTexturesInitialized;

            public void InitializeTexture()
            {
                if (DepthRT == null) DepthRT = new RenderTexture(KWS_Settings.Water.OrthoDepthResolution, KWS_Settings.Water.OrthoDepthResolution, 32, RenderTextureFormat.Depth);
                IsTexturesInitialized = true;
            }

            public void Release()
            {
                if (DepthRT != null)
                {
                    DepthRT.Release();
                    DepthRT = null;
                }

                IsTexturesInitialized = false;
            }
        }

        void InitializeCamera()
        {
            _currentCamGO = KW_Extensions.CreateHiddenGameObject("Ortho depth camera");

            _camTransform = _currentCamGO.transform;
            _camTransform.parent = WaterInstance.WaterTemporaryObject.transform;

            _depthCamera = _currentCamGO.AddComponent<Camera>();
            _depthCamera.cameraType = CameraType.Reflection;
            _depthCamera.depthTextureMode = DepthTextureMode.None;
            _depthCamera.backgroundColor = Color.black;
            _depthCamera.clearFlags = CameraClearFlags.Color;
            _depthCamera.aspect = 1.0f;
            _depthCamera.depth = -30000;

            _depthCamera.orthographic = true;
            _depthCamera.allowHDR = false;
            _depthCamera.enabled = false;

            _depthCamera.orthographicSize = KWS_Settings.Water.OrthoDepthAreaSize * 0.5f;
            _depthCamera.farClipPlane = KWS_Settings.Water.OrthoDepthAreaFarOffset;
            _depthCamera.nearClipPlane = -KWS_Settings.Water.OrthoDepthAreaNearOffset;
            _depthCamera.cullingMask = ~(1 << KWS_Settings.Water.WaterLayer);

            OnInitializedDepthCamera?.Invoke(_depthCamera);
        }

        bool CheckIfCanUpdateDepth(Vector3 waterPos, Vector3 camPos, PassData passData, bool forceUpdate = false)
        {
            var lastPos = passData.Position;
            var currentCamersPos = camPos;
            lastPos.y = currentCamersPos.y = waterPos.y;
            if (forceUpdate || Vector3.Distance(lastPos, currentCamersPos) >= KWS_Settings.Water.OrthoDepthAreaSize * 0.2f)
            {
                passData.Position = currentCamersPos;

                return true;
            }

            return false;
        }

        public void Execute(Camera cam)
        {
            if ((WaterInstance.Settings.UseCausticEffect == false || WaterInstance.Settings.UseDepthCausticScale == false) && WaterInstance.Settings.UseShorelineRendering == false) return;
            if (!WaterInstance.Settings.EnabledMeshRendering) return;
            if (!KWS_CoreUtils.CanBeRenderCurrentWaterInstance(WaterInstance)) return;

            if (_currentCamGO == null) InitializeCamera();
            var camPos = KW_Extensions.GetCameraPositionFast(cam);
            camPos.y = WaterInstance.WaterPivotWorldPosition.y;

            var isEditorCamera = cam.cameraType == CameraType.SceneView;
            var passData = isEditorCamera ? _currentPassDataEditor : _currentPassData;

            if (!CheckIfCanUpdateDepth(WaterInstance.WaterPivotWorldPosition, camPos, passData, forceUpdate: false)) return;
            if (!passData.IsTexturesInitialized) passData.InitializeTexture();

            _camTransform.position = camPos;
            _camTransform.rotation = Quaternion.Euler(90, 0, 0);

            OnRender?.Invoke(passData, _depthCamera);

            WaterInstance.SharedData.OrthoDepth            = passData.DepthRT;
            WaterInstance.SharedData.OrthoDepthPosition    = passData.Position;
            WaterInstance.SharedData.OrthoDepthNearFarSize = passData.NearFarSize;
        }
    }
}