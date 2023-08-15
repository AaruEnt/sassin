using UnityEngine;

namespace KWS
{
    public static class ReflectionUtils
    {
        private static PlanarReflectionSettingData _settingsData = new PlanarReflectionSettingData();

        public static GameObject CreateReflectionCamera(string name, WaterSystem waterInstance, out Camera reflectionCamera, out Transform reflectionCamTransform)
        {
            var reflCameraGo = KW_Extensions.CreateHiddenGameObject(name);

            reflCameraGo.transform.parent = waterInstance.WaterTemporaryObject.transform;
            reflectionCamera = reflCameraGo.AddComponent<Camera>();
            reflectionCamTransform = reflCameraGo.transform;
            reflectionCamera.cameraType = CameraType.Reflection;
            reflectionCamera.allowMSAA = false;
            reflectionCamera.enabled = false;
            reflectionCamera.allowHDR = true;

            return reflCameraGo;
        }

        public static void RenderPlanarReflection(WaterSystem waterInstance, Camera reflectionCamera, Transform reflectionCameraTransform, RenderTexture target,
                                                  Matrix4x4 cameraMatrix, Matrix4x4 projectionMatrix, Vector3 currentCameraPos, bool isPlanarReflection, bool isCubemapReflection)
        {
            float clipPlaneOffset = 0.01f;
            float planeOffset = -0.01f;

            var pos = waterInstance.WaterRelativeWorldPosition + Vector3.up * planeOffset;
            var reflectedCameraPos = currentCameraPos - new Vector3(0, pos.y * 2, 0);
            reflectedCameraPos.y = -reflectedCameraPos.y;
            var normal = Vector3.up;

            var d = -Vector3.Dot(normal, pos) - clipPlaneOffset;
            var reflectionPlane = new Vector4(normal.x, normal.y, normal.z, d);

            CalculateReflectionMatrix(out var reflection, reflectionPlane);
            FixOutOfViewCameraFrustumError(reflectionCameraTransform, reflectedCameraPos);

            reflectionCamera.worldToCameraMatrix = cameraMatrix * reflection;
            var clipPlane = CameraSpacePlane(reflectionCamera, pos + normal * 0.01f, normal, 1.0f, planeOffset);
            CalculateObliqueMatrix(ref projectionMatrix, clipPlane);
            reflectionCamera.projectionMatrix = projectionMatrix;

            try
            {
                _settingsData.Set(waterInstance, isPlanarReflection);
                reflectionCamera.targetTexture = target;
                if (isCubemapReflection) reflectionCamera.Render();
                else KWS_CoreUtils.UniversalCameraRendering(waterInstance, reflectionCamera);
            }
            finally
            {
                _settingsData.Restore();
            }
        }

        class PlanarReflectionSettingData
        {
            private bool _fog;
            private int _maxLod;
            private float _lodBias;
            private ShadowQuality _shadowQuality;

          
            public void Set(WaterSystem waterInstance, bool isPlanarReflection)
            {
                _fog           = RenderSettings.fog;
                _shadowQuality = QualitySettings.shadows;
                _maxLod        = QualitySettings.maximumLODLevel;
                _lodBias       = QualitySettings.lodBias;

                if (!waterInstance.Settings.FixCubemapIndoorSkylightReflection) GL.invertCulling = true;
                RenderSettings.fog = false;
                QualitySettings.maximumLODLevel = 1;
                QualitySettings.lodBias = _lodBias * 0.5f;
                if (isPlanarReflection && waterInstance.Settings.RenderPlanarShadows == false && !waterInstance.Settings.FixCubemapIndoorSkylightReflection)
                    QualitySettings.shadows = ShadowQuality.Disable;
            }

            public void Restore()
            {
                GL.invertCulling = false;
                RenderSettings.fog = _fog;
                QualitySettings.shadows = _shadowQuality;
                QualitySettings.maximumLODLevel = _maxLod;
                QualitySettings.lodBias = _lodBias;
            }
        }

        public static void CopyReflectionParamsFrom(this Camera currentCamera, Camera reflectionCamera, int cullingMask, bool isCubemap)
        {
            //_reflectionCamera.CopyFrom(currentCamera); //this method have 100500 bugs

            reflectionCamera.orthographic = currentCamera.orthographic;
            reflectionCamera.fieldOfView = currentCamera.fieldOfView;
            reflectionCamera.farClipPlane = currentCamera.farClipPlane;
            reflectionCamera.nearClipPlane = currentCamera.nearClipPlane;
            reflectionCamera.rect = currentCamera.rect;
            reflectionCamera.renderingPath = currentCamera.renderingPath;

            if (currentCamera.usePhysicalProperties)
            {
                reflectionCamera.usePhysicalProperties = true;
                reflectionCamera.focalLength = currentCamera.focalLength;
                reflectionCamera.sensorSize = currentCamera.sensorSize;
                reflectionCamera.lensShift = currentCamera.lensShift;
                reflectionCamera.gateFit = currentCamera.gateFit;
            }

            reflectionCamera.cullingMask = cullingMask;
            reflectionCamera.depth = currentCamera.depth - 100;


            reflectionCamera.fieldOfView = isCubemap ? 90 : currentCamera.fieldOfView;
            reflectionCamera.aspect = isCubemap ? 1 : currentCamera.aspect;
        }

        public static void FixOutOfViewCameraFrustumError(Transform reflectionCameraTransform, Vector3 camPos)
        {
            //var newPos = reflection.MultiplyPoint(currentCameraPos);
            //var xPos   = Mathf.Clamp(newPos.x, -float.MaxValue + 100, float.MaxValue - 100) + float.Epsilon; //avoiding error "Screen position out of view frustum"
            //var yPos   = Mathf.Clamp(newPos.y, -float.MaxValue + 100, float.MaxValue - 100) + float.Epsilon;
            //var zPos   = Mathf.Clamp(newPos.z, -float.MaxValue + 100, float.MaxValue - 100) + float.Epsilon;

            reflectionCameraTransform.position = Vector3.ClampMagnitude(camPos, float.MaxValue - 100) + Vector3.one * float.Epsilon;
            if (reflectionCameraTransform.rotation.eulerAngles == Vector3.zero) reflectionCameraTransform.rotation = Quaternion.Euler(0.000001f, 0.000001f, 0.000001f);
        }

        public static void CalculateObliqueMatrix(ref Matrix4x4 matrix, Vector4 clipPlane)
        {
            Vector4 q;

            q.x = (Sgn(clipPlane.x) + matrix[8]) / matrix[0];
            q.y = (Sgn(clipPlane.y) + matrix[9]) / matrix[5];
            q.z = -1.0F;
            q.w = (1.0F + matrix[10]) / matrix[14];

            var c = clipPlane * (2.0F / Vector3.Dot(clipPlane, q));

            matrix[2] = c.x;
            matrix[6] = c.y;
            matrix[10] = c.z + 1.0F;
            matrix[14] = c.w;
        }

        private static float Sgn(float a)
        {
            if (a > 0.0f) return 1.0f;
            if (a < 0.0f) return -1.0f;
            return 0.0f;
        }

        public static Vector4 CameraSpacePlane(Camera cam, Vector3 pos, Vector3 normal, float sideSign, float clipOffset)
        {
            var offsetPos = pos + normal * clipOffset;
            var m = cam.worldToCameraMatrix;
            var cameraPosition = m.MultiplyPoint(offsetPos);
            var cameraNormal = m.MultiplyVector(normal).normalized * sideSign;
            return new Vector4(cameraNormal.x, cameraNormal.y, cameraNormal.z, -Vector3.Dot(cameraPosition, cameraNormal));
        }

        public static void CalculateReflectionMatrix(out Matrix4x4 reflectionMat, Vector4 plane)
        {
            reflectionMat.m00 = (1F - 2F * plane[0] * plane[0]);
            reflectionMat.m01 = (-2F * plane[0] * plane[1]);
            reflectionMat.m02 = (-2F * plane[0] * plane[2]);
            reflectionMat.m03 = (-2F * plane[3] * plane[0]);

            reflectionMat.m10 = (-2F * plane[1] * plane[0]);
            reflectionMat.m11 = (1F - 2F * plane[1] * plane[1]);
            reflectionMat.m12 = (-2F * plane[1] * plane[2]);
            reflectionMat.m13 = (-2F * plane[3] * plane[1]);

            reflectionMat.m20 = (-2F * plane[2] * plane[0]);
            reflectionMat.m21 = (-2F * plane[2] * plane[1]);
            reflectionMat.m22 = (1F - 2F * plane[2] * plane[2]);
            reflectionMat.m23 = (-2F * plane[3] * plane[2]);

            reflectionMat.m30 = 0F;
            reflectionMat.m31 = 0F;
            reflectionMat.m32 = 0F;
            reflectionMat.m33 = 1F;
        }
    }
}