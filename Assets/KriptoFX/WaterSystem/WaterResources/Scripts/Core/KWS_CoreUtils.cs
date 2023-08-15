using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.XR;

namespace KWS
{
    public static partial class KWS_CoreUtils
    {

        private static int _sourceRT_id            = Shader.PropertyToID("_SourceRT"); //by some reason _MainTex don't work
        private static int _sourceRTHandleScale_id = Shader.PropertyToID("_SourceRTHandleScale");
        private static int KWS_WaterViewPort_id    = Shader.PropertyToID("KWS_WaterViewPort");
        const          int MaxHeight               = 1080;
        const          int MaxHeightVR             = 2000;

        public static GraphicsFormat GetGraphicsFormatHDR()
        {
            if (SystemInfo.IsFormatSupported(GraphicsFormat.B10G11R11_UFloatPack32, FormatUsage.Render)) return GraphicsFormat.B10G11R11_UFloatPack32;
            else return GraphicsFormat.R16G16B16A16_SFloat;
        }


        [Serializable]
        public struct Uint2
        {
            public uint X;
            public uint Y;
        }

        [Serializable]
        public struct Uint3
        {
            public uint X;
            public uint Y;
            public uint Z;
        }

        [Serializable]
        public struct Uint4
        {
            public uint X;
            public uint Y;
            public uint Z;
            public uint W;
        }


        static RenderTexture _defaultGrayTexture;

        public static RenderTexture DefaultGrayTexture
        {
            get
            {
                if (_defaultGrayTexture == null)
                {
                    _defaultGrayTexture = new RenderTexture(1, 1, 0);
                    _defaultGrayTexture.Create();
                    ClearRenderTexture(_defaultGrayTexture, ClearFlag.Color, new Color(0.5f, 0.5f, 0.5f, 0.5f));
                }

                return _defaultGrayTexture;
            }
        }


        static RenderTexture _defaultBlackTexture;

        public static RenderTexture DefaultBlackTexture
        {
            get
            {
                if (_defaultBlackTexture == null)
                {
                    _defaultBlackTexture = new RenderTexture(1, 1, 0);
                    _defaultBlackTexture.Create();
                }

                return _defaultBlackTexture;
            }
        }

        static RenderTexture _defaultBlackCubeTexture;

        public static RenderTexture DefaultBlackCubeTexture
        {
            get
            {
                if (_defaultBlackCubeTexture == null)
                {
                    _defaultBlackCubeTexture = new RenderTexture(1, 1, 0) {dimension = TextureDimension.Cube};
                    _defaultBlackCubeTexture.Create();
                }

                return _defaultBlackCubeTexture;
            }
        }

        static RenderTexture _defaultBlack3DTexture;

        public static RenderTexture DefaultBlack3DTexture
        {
            get
            {
                if (_defaultBlack3DTexture == null)
                {
                    _defaultBlack3DTexture = new RenderTexture(1, 1, 0) {dimension = TextureDimension.Tex3D};
                    _defaultBlack3DTexture.Create();
                }

                return _defaultBlack3DTexture;
            }
        }


        public static bool IsAtomicsSupported()
        {
            var api = SystemInfo.graphicsDeviceType;
            return api == GraphicsDeviceType.Direct3D11 || api == GraphicsDeviceType.Direct3D12;
        }

        public static bool CanBeRenderCurrentWaterInstance(WaterSystem waterInstance)
        {
            if (!waterInstance.IsWaterInitialized) return false;
            if (!waterInstance.IsWaterVisible) return false;
            if (!waterInstance.IsWaterRenderingActive) return false;

            return true;
        }

        public static bool CanRenderWaterForCurrentCamera(WaterSystem waterInstance, Camera cam)
        {
            if (cam == null) return false;


#if UNITY_EDITOR
#if UNITY_2021_3_OR_NEWER
                    if (UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage() != null) return false;
#else
            if (UnityEditor.Experimental.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage() != null) return false;
#endif
            if (Application.isPlaying && Time.frameCount <= 2) return false;
#else
            if(Time.frameCount <= 2) return false;
#endif
            if (!waterInstance.IsWaterRenderingActive) return false;
            if (cam.cameraType != CameraType.Game && cam.cameraType != CameraType.SceneView) return false;
            if (!cam.IsLayerRendered(KWS_Settings.Water.WaterLayer)) return false;
            if (!CanRenderWaterForCurrentCamera_PlatformSpecific(cam)) return false;
            if (WaterSystem.IsSinglePassStereoEnabled && cam.cameraType == CameraType.SceneView) return false;
            //if (cam.name == "TopViewDepthCamera" || cam.name.Contains("reflect")) return false; //todo check GC
            return true;
        }

        public static void ConcatAndSetKeywords(string key1, bool value1, string key2, bool value2, string key3, bool value3, string key1_key2_key3)
        {
            if (!value1)
            {
                SetKeyword(key1,           false);
                SetKeyword(key2,           false);
                SetKeyword(key3,           false);
                SetKeyword(key1_key2_key3, false);
            }
            else
            {
                if (!value2 && !value3)
                {
                    SetKeyword(key1,           true);
                    SetKeyword(key2,           false);
                    SetKeyword(key3,           false);
                    SetKeyword(key1_key2_key3, false);
                }
                else if (value2 && !value3)
                {
                    SetKeyword(key1,           false);
                    SetKeyword(key2,           true);
                    SetKeyword(key3,           false);
                    SetKeyword(key1_key2_key3, false);
                }
                else if (!value2 && value3)
                {
                    SetKeyword(key1,           false);
                    SetKeyword(key2,           false);
                    SetKeyword(key3,           true);
                    SetKeyword(key1_key2_key3, false);
                }
                else
                {
                    SetKeyword(key1,           false);
                    SetKeyword(key2,           false);
                    SetKeyword(key3,           false);
                    SetKeyword(key1_key2_key3, true);
                }
            }


        }

        public static void SetKeyword(string keyword, bool state)
        {
            if (state)
                Shader.EnableKeyword(keyword);
            else
                Shader.DisableKeyword(keyword);
        }

        public static void SetKeyword(this Material mat, string keyword, bool state)
        {
            if (state)
                mat.EnableKeyword(keyword);
            else
                mat.DisableKeyword(keyword);
        }

        public static void SetKeyword(this CommandBuffer buffer, string keyword, bool state)
        {
            if (state)
                buffer.EnableShaderKeyword(keyword);
            else
                buffer.DisableShaderKeyword(keyword);
        }

        public static void SetKeyword(this ComputeShader cs, string keyword, bool state)
        {
            if (state)
                cs.EnableKeyword(keyword);
            else
                cs.DisableKeyword(keyword);
        }

        public static void SetMatrices<T>(T obj, params (int id, Matrix4x4 value)[] datas) where T : class
        {
            if (obj is Material mat)
            {
                foreach (var data in datas)
                {
                    mat.SetMatrix(data.id, data.value);
                }
            }
            else if (obj is ComputeShader cs)
            {
                foreach (var data in datas)
                {
                    cs.SetMatrix(data.id, data.value);
                }
            }
        }

        public static void SetFloats<T>(T obj, params (int id, float value)[] datas) where T : class
        {
            if (obj is Material mat)
            {
                foreach (var data in datas)
                {
                    mat.SetFloat(data.id, data.value);
                }
            }
            else if (obj is ComputeShader cs)
            {
                foreach (var data in datas)
                {
                    cs.SetFloat(data.id, data.value);
                }
            }
        }

        public static void SetInts<T>(T obj, params (int id, int value)[] datas) where T : class
        {
            if (obj is Material mat)
            {
                foreach (var data in datas)
                {
                    mat.SetInt(data.id, data.value);
                }
            }
            else if (obj is ComputeShader cs)
            {
                foreach (var data in datas)
                {
                    cs.SetInt(data.id, data.value);
                }
            }
        }

        public static void SetVectors<T>(T obj, params (int id, Vector4 value)[] datas) where T : class
        {
            if (obj is Material mat)
            {
                foreach (var data in datas)
                {
                    mat.SetVector(data.id, data.value);
                }
            }
            else if (obj is ComputeShader cs)
            {
                foreach (var data in datas)
                {
                    cs.SetVector(data.id, data.value);
                }
            }
        }

        public static void SetKeywords<T>(T obj, params (string key, bool value)[] datas) where T : class
        {
            if (obj is Material mat)
            {
                foreach (var data in datas)
                {
                    mat.SetKeyword(data.key, data.value);
                }
            }
            else if (obj is ComputeShader cs)
            {
                foreach (var data in datas)
                {
                    cs.SetKeyword(data.key, data.value);
                }
            }
        }

        public static Texture GetSafeTexture(this Texture rt, Color color = default)
        {
            if (rt != null) return rt;
            return color == Color.gray ? DefaultGrayTexture : DefaultBlackTexture;
        }

        public static RenderTexture GetSafeTexture(this RenderTexture rt, Color color = default)
        {
            if (rt != null) return rt;
            return color == Color.gray ? DefaultGrayTexture : DefaultBlackTexture;
        }

        public static RenderTexture GetSafeCubeTexture(this RenderTexture rt, Color color = default)
        {
            if (rt != null) return rt;
            return DefaultBlackCubeTexture;
        }

        public static RenderTexture GetSafeTexture(this RTHandle rtHandle, Color color = default)
        {
            if (rtHandle != null && rtHandle.rt != null) return rtHandle.rt;
            return color == Color.gray ? DefaultGrayTexture : DefaultBlackTexture;
        }

        public class TemporaryRenderTexture
        {
            public RenderTextureDescriptor descriptor;
            public RenderTexture           rt;
            string                         _name;
            public bool                    isInitialized;

            public TemporaryRenderTexture()
            {

            }

            public TemporaryRenderTexture(string name, TemporaryRenderTexture source)
            {
                descriptor = source.descriptor;
                rt         = RenderTexture.GetTemporary(descriptor);
                rt.name    = name;
                _name      = name;
                if (!rt.IsCreated()) rt.Create();
                isInitialized = true;
            }

            public void Alloc(string name, int width, int height, int depth, GraphicsFormat format)
            {
                if (rt == null)
                {
                    descriptor                  = new RenderTextureDescriptor(width, height, format, depth);
                    descriptor.sRGB             = false;
                    descriptor.useMipMap        = false;
                    descriptor.autoGenerateMips = false;

                    rt      = RenderTexture.GetTemporary(descriptor);
                    rt.name = name;
                    _name   = name;
                    if (!rt.IsCreated()) rt.Create();
                    isInitialized = true;

                }
                else if (rt.width != width || rt.height != height || !isInitialized || _name != name)
                {
                    if (isInitialized) Release();

                    descriptor.width  = width;
                    descriptor.height = height;

                    rt      = RenderTexture.GetTemporary(descriptor);
                    rt.name = name;
                    _name   = name;
                    if (!rt.IsCreated()) rt.Create();
                    isInitialized = true;

                }
            }

            public void Alloc(string     name,                                   int                width, int height, int depth, GraphicsFormat format, bool useMipMap,
                              bool       useRandomWrite   = false,               TextureDimension   dimension          = TextureDimension.Tex2D,
                              bool       autoGenerateMips = false,               int                mipMapCount        = 0, int msaaSamples = 1, VRTextureUsage vrUsage = VRTextureUsage.None,
                              FilterMode filterMode       = FilterMode.Bilinear, ShadowSamplingMode shadowSamplingMode = ShadowSamplingMode.None)
            {
                if (rt == null)
                {
#if UNITY_2019_2_OR_NEWER
                    descriptor = new RenderTextureDescriptor(width, height, format, depth);
#else
                    descriptor = new RenderTextureDescriptor(width, height, GraphicsFormatUtility.GetRenderTextureFormat(format), depth);
#endif

                    descriptor.sRGB               = false;
                    descriptor.enableRandomWrite  = useRandomWrite;
                    descriptor.dimension          = dimension;
                    descriptor.useMipMap          = useMipMap;
                    descriptor.autoGenerateMips   = autoGenerateMips;
                    descriptor.shadowSamplingMode = shadowSamplingMode;
#if UNITY_2019_2_OR_NEWER
                    descriptor.mipCount = mipMapCount;
#endif
                    descriptor.msaaSamples = msaaSamples;
                    descriptor.vrUsage     = vrUsage;

                    rt      = RenderTexture.GetTemporary(descriptor);
                    rt.name = name;
                    _name   = name;
                    if (!rt.IsCreated()) rt.Create();
                    isInitialized = true;

                }
                else if (rt.width != width || rt.height != height || rt.dimension != dimension || rt.useMipMap != useMipMap || !isInitialized || _name != name)
                {
                    if (isInitialized) Release();

                    descriptor.width     = width;
                    descriptor.height    = height;
                    descriptor.dimension = dimension;
                    descriptor.useMipMap = useMipMap;

                    rt      = RenderTexture.GetTemporary(descriptor);
                    rt.name = name;
                    _name   = name;
                    if (!rt.IsCreated()) rt.Create();
                    isInitialized = true;

                }
            }

            public void Alloc(string     name,                                   int                width,                                       int  height, int depth, GraphicsFormat format, ClearFlag clearFlag, Color clearColor,
                              bool       useRandomWrite   = false,               TextureDimension   dimension          = TextureDimension.Tex2D, bool useMipMap   = false,
                              bool       autoGenerateMips = false,               int                mipMapCount        = 0,                      int  msaaSamples = 1, VRTextureUsage vrUsage = VRTextureUsage.None,
                              FilterMode filterMode       = FilterMode.Bilinear, ShadowSamplingMode shadowSamplingMode = ShadowSamplingMode.None)
            {
                if (rt == null)
                {
#if UNITY_2019_2_OR_NEWER
                    descriptor = new RenderTextureDescriptor(width, height, format, depth);
#else
                    descriptor = new RenderTextureDescriptor(width, height, GraphicsFormatUtility.GetRenderTextureFormat(format), depth);
#endif

                    descriptor.sRGB               = false;
                    descriptor.enableRandomWrite  = useRandomWrite;
                    descriptor.dimension          = dimension;
                    descriptor.useMipMap          = useMipMap;
                    descriptor.autoGenerateMips   = autoGenerateMips;
                    descriptor.shadowSamplingMode = shadowSamplingMode;
#if UNITY_2019_2_OR_NEWER
                    descriptor.mipCount = mipMapCount;
#endif
                    descriptor.msaaSamples = msaaSamples;
                    descriptor.vrUsage     = vrUsage;

                    rt      = RenderTexture.GetTemporary(descriptor);
                    rt.name = name;
                    _name   = name;
                    if (!rt.IsCreated()) rt.Create();
                    ClearRenderTexture(rt, clearFlag, clearColor);
                    isInitialized = true;

                }
                else if (rt.width != width || rt.height != height || rt.dimension != dimension || rt.useMipMap != useMipMap || !isInitialized || _name != name)
                {
                    if (isInitialized) Release();

                    descriptor.width     = width;
                    descriptor.height    = height;
                    descriptor.dimension = dimension;
                    descriptor.useMipMap = useMipMap;

                    rt      = RenderTexture.GetTemporary(descriptor);
                    rt.name = name;
                    _name   = name;
                    if (!rt.IsCreated()) rt.Create();
                    ClearRenderTexture(rt, clearFlag, clearColor);
                    isInitialized = true;

                }
            }

            public static implicit operator RenderTexture(TemporaryRenderTexture temporaryRT)
            {
                return temporaryRT?.rt;
            }

            public static implicit operator RenderTargetIdentifier(TemporaryRenderTexture temporaryRT)
            {
                return temporaryRT?.rt;
            }

            public void Release(bool unlink = false)
            {
                if (rt != null)
                {
                    RenderTexture.ReleaseTemporary(rt);
                    isInitialized = false;
                    if (unlink) rt = null;
                }
            }
        }


        public static Mesh CreateQuad()
        {
            var mesh = new Mesh();
            mesh.hideFlags = HideFlags.DontSave;

            Vector3[] vertices = new Vector3[4]
            {
                new Vector3(-0.5f, -0.5f, 0),
                new Vector3(0.5f,  -0.5f, 0),
                new Vector3(-0.5f, 0.5f,  0),
                new Vector3(0.5f,  0.5f,  0)
            };
            mesh.vertices = vertices;

            int[] tris = new int[6]
            {
                0, 2, 1,
                2, 3, 1
            };
            mesh.triangles = tris;

            Vector3[] normals = new Vector3[4]
            {
                -Vector3.forward,
                -Vector3.forward,
                -Vector3.forward,
                -Vector3.forward
            };
            mesh.normals = normals;

            Vector2[] uv = new Vector2[4]
            {
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(1, 1)
            };
            mesh.uv = uv;

            return mesh;
        }

        public static Material CreateMaterial(string shaderName, string prefix)
        {
            return CreateMaterial(string.Format("{0}_{1}", shaderName, prefix));
        }

        public static Material CreateMaterial(string shaderName)
        {
            var waterShader = Shader.Find(shaderName);
            if (waterShader == null)
            {
                Debug.LogError("Can't find the shader '" + shaderName + "' in the resources folder. Try to reimport the package.");
                return null;
            }

            var waterMaterial = new Material(waterShader);
            waterMaterial.hideFlags = HideFlags.HideAndDontSave;
            return waterMaterial;
        }

        public static ComputeBuffer GetOrUpdateBuffer<T>(ref ComputeBuffer buffer, int size) where T : struct
        {
            if (buffer == null)
            {
                buffer = new ComputeBuffer(size, System.Runtime.InteropServices.Marshal.SizeOf<T>());
            }
            else if (size > buffer.count)
            {
                buffer.Dispose();
                buffer = new ComputeBuffer(size, System.Runtime.InteropServices.Marshal.SizeOf<T>());
                // Debug.Log("ReInitializeHashBuffer");
            }

            return buffer;
        }

        public static void BlitTriangle(RenderTexture dest, Material mat, int pass = 0)
        {
            Graphics.SetRenderTarget(dest, 0, CubemapFace.Unknown, -1);
            mat.SetPass(pass);
            Graphics.DrawProcedural(mat, new Bounds(Vector3.zero, Vector3.one), MeshTopology.Triangles, 3, 1);
            RenderTexture.active = null;
        }

        public static void BlitTriangle(this CommandBuffer cmd, RenderTargetIdentifier dest, Material mat, int pass = 0)
        {
            cmd.SetRenderTarget(dest, 0, CubemapFace.Unknown, -1);
            cmd.DrawProcedural(Matrix4x4.identity, mat, pass, MeshTopology.Triangles, 3);
        }

        public static void BlitTriangle(this CommandBuffer cmd, RenderTargetIdentifier source, RenderTargetIdentifier dest, Material mat, int pass = 0)
        {
            cmd.SetGlobalTexture(_sourceRT_id, source);
            cmd.BlitTriangle(dest, mat, pass);
        }

        public static void BlitTriangle(this CommandBuffer cmd, RenderTargetIdentifier source, Vector4 sourceRTHandleScale, RenderTargetIdentifier dest, Material mat, int pass = 0)
        {
            cmd.SetGlobalVector(_sourceRTHandleScale_id, sourceRTHandleScale);
            cmd.SetGlobalTexture(_sourceRT_id, source);
            cmd.BlitTriangle(dest, mat, pass);
        }

        public static void BlitTriangle(this CommandBuffer cmd, RenderTargetIdentifier target, Vector2Int viewPortSize, Material mat, ClearFlag clearFlag, Color clearColor, int pass = 0)
        {
            cmd.SetRenderTarget(target, 0, CubemapFace.Unknown, -1);
            SetViewportAndClear(cmd, viewPortSize, clearFlag, clearColor);
            cmd.DrawProcedural(Matrix4x4.identity, mat, pass, MeshTopology.Triangles, 3);
        }

        public static void BlitTriangle(this CommandBuffer cmd, RenderTargetIdentifier source, Vector4 sourceRTHandleScale, RenderTargetIdentifier dest, Vector2Int viewPortSize, Material mat, int pass = 0)
        {
            cmd.SetGlobalVector(_sourceRTHandleScale_id, sourceRTHandleScale);
            cmd.SetGlobalTexture(_sourceRT_id, source);
            cmd.BlitTriangle(dest, viewPortSize, mat, ClearFlag.None, Color.clear, pass);
        }

        public static void SetViewport(this CommandBuffer cmd, RTHandle target)
        {
            if (target.useScaling)
            {
                var scaledViewportSize = target.GetScaledSize(target.rtHandleProperties.currentViewportSize);
                cmd.SetViewport(new Rect(0.0f, 0.0f, scaledViewportSize.x, scaledViewportSize.y));
            }
        }

        public static void SetViewport(this CommandBuffer cmd, Rect viewPort, RTHandle target)
        {
            if (target.useScaling)
            {
                var scaledViewportSize = target.GetScaledSize(new Vector2Int((int) viewPort.width, (int) viewPort.height));
                var scaledViewportPos  = target.GetScaledSize(new Vector2Int((int) viewPort.x,     (int) viewPort.y));
                cmd.SetViewport(new Rect(scaledViewportPos.x, scaledViewportPos.y, scaledViewportSize.x, scaledViewportSize.y));
            }
        }

        public static void SetViewport(this CommandBuffer cmd, Vector2Int viewPortSize)
        {
            cmd.SetViewport(new Rect(0.0f, 0.0f, viewPortSize.x, viewPortSize.y));
        }

        public static void SetViewportAndClear(this CommandBuffer cmd, Rect viewPort, RTHandle target, ClearFlag clearFlag, Color clearColor)
        {
#if !UNITY_EDITOR
            SetViewport(cmd, viewPort, target);
#endif
            CoreUtils.ClearRenderTarget(cmd, clearFlag, clearColor);
#if UNITY_EDITOR
            SetViewport(cmd, viewPort, target);
#endif
        }

        public static void SetViewportAndClear(this CommandBuffer cmd, RTHandle target, ClearFlag clearFlag, Color clearColor)
        {
#if !UNITY_EDITOR
            SetViewport(cmd, target);
#endif
            CoreUtils.ClearRenderTarget(cmd, clearFlag, clearColor);
#if UNITY_EDITOR
            SetViewport(cmd, target);
#endif
        }

        public static void SetViewportAndClear(this CommandBuffer cmd, Vector2Int viewPortSize, ClearFlag clearFlag, Color clearColor)
        {
#if !UNITY_EDITOR
            SetViewport(cmd, viewPortSize);
#endif
            CoreUtils.ClearRenderTarget(cmd, clearFlag, clearColor);
#if UNITY_EDITOR
            SetViewport(cmd, viewPortSize);
#endif
        }

        public static void BlitTriangleRTHandle(this CommandBuffer cmd, Rect viewPort, RTHandle target, Material mat, ClearFlag clearFlag, Color clearColor, int pass = 0)
        {
            var waterViewportSize  = target.GetScaledSize(new Vector2Int((int) viewPort.width, (int) viewPort.height));
            var waterViewportPos   = target.GetScaledSize(new Vector2Int((int) viewPort.x,     (int) viewPort.y));
            var targetViewportSize = target.GetScaledSize(target.rtHandleProperties.currentViewportSize);

            var waterViewPort = new Vector4((float) waterViewportSize.x / targetViewportSize.x, (float) waterViewportSize.y / targetViewportSize.y,
                                            (float) waterViewportPos.x  / targetViewportSize.x, (float) waterViewportPos.y  / targetViewportSize.y);
            cmd.SetGlobalVector(KWS_WaterViewPort_id, waterViewPort);

            cmd.SetRenderTarget(target, 0, CubemapFace.Unknown, -1);
            SetViewportAndClear(cmd, viewPort, target, clearFlag, clearColor);
            cmd.DrawProcedural(Matrix4x4.identity, mat, pass, MeshTopology.Triangles, 3);
            cmd.SetGlobalVector(KWS_WaterViewPort_id, Vector4.zero);
        }

        public static void BlitTriangleRTHandle(this CommandBuffer cmd, RTHandle target, Material mat, ClearFlag clearFlag, Color clearColor, int pass = 0)
        {
            cmd.SetRenderTarget(target, 0, CubemapFace.Unknown, -1);
            SetViewportAndClear(cmd, target, clearFlag, clearColor);
            cmd.DrawProcedural(Matrix4x4.identity, mat, pass, MeshTopology.Triangles, 3);
        }

        public static void BlitTriangleRTHandle(this CommandBuffer cmd, RenderTargetIdentifier source, Vector4 sourceRTHandleScale, RTHandle target, Material mat, ClearFlag clearFlag, Color clearColor, int pass = 0)
        {
            cmd.SetGlobalVector(_sourceRTHandleScale_id, sourceRTHandleScale);
            cmd.SetGlobalTexture(_sourceRT_id, source);
            cmd.SetRenderTarget(target, 0, CubemapFace.Unknown, -1);
            SetViewportAndClear(cmd, target, clearFlag, clearColor);
            cmd.DrawProcedural(Matrix4x4.identity, mat, pass, MeshTopology.Triangles, 3, 1);
        }

        public static void BlitTriangleRTHandle(this CommandBuffer cmd, RTHandle source, RTHandle target, Material mat, ClearFlag clearFlag, Color clearColor, int pass = 0)
        {
            cmd.SetGlobalVector(_sourceRTHandleScale_id, source.rtHandleProperties.rtHandleScale);
            cmd.SetGlobalTexture(_sourceRT_id, source);
            cmd.SetRenderTarget(target, 0, CubemapFace.Unknown, -1);
            SetViewportAndClear(cmd, target, clearFlag, clearColor);
            cmd.DrawProcedural(Matrix4x4.identity, mat, pass, MeshTopology.Triangles, 3, 1);
        }


        public static Vector2Int GetScreenSizeLimited(bool isStereoEnabled)
        {
            int width;
            int height;

            if (isStereoEnabled)
            {
                width  = XRSettings.eyeTextureWidth;
                height = XRSettings.eyeTextureHeight;
            }
            else
            {
                width  = Screen.width;
                height = Screen.height;
            }

            if (height > MaxHeightVR)
            {
                width  = (int) (MaxHeight * width / (float) height);
                height = MaxHeight;
            }

            return new Vector2Int(width, height);
        }


        public static void ReleaseTemporaryRenderTextures(bool unlink = false, params TemporaryRenderTexture[] tempTenderTextures)
        {
            for (var i = 0; i < tempTenderTextures.Length; i++)
            {
                if (tempTenderTextures[i] == null) continue;
                tempTenderTextures[i].Release(unlink);
            }
        }

        public static void ReleaseComputeBuffers(params ComputeBuffer[] computeBuffers)
        {
            for (var i = 0; i < computeBuffers.Length; i++)
            {
                if (computeBuffers[i] == null) continue;
                computeBuffers[i].Release();
            }
        }

        public static void ReleaseRenderTextures(params RenderTexture[] renderTextures)
        {
            for (var i = 0; i < renderTextures.Length; i++)
            {
                if (renderTextures[i] == null) continue;
                renderTextures[i].Release();
            }
        }

        public static void ClearRenderTexture(RenderTexture rt, ClearFlag clearFlag, Color clearColor)
        {
            var activeRT = RenderTexture.active;
            RenderTexture.active = rt;
            GL.Clear((clearFlag & ClearFlag.Depth) != 0, (clearFlag & ClearFlag.Color) != 0, clearColor);
            RenderTexture.active = activeRT;
        }

        public static RTHandle RTHandleAllocVR(
            int                     width,
            int                     height,
            DepthBits               depthBufferBits   = DepthBits.None,
            GraphicsFormat          colorFormat       = GraphicsFormat.R8G8B8A8_SRGB,
            FilterMode              filterMode        = FilterMode.Point,
            TextureWrapMode         wrapMode          = TextureWrapMode.Repeat,
            bool                    enableRandomWrite = false,
            bool                    useMipMap         = false,
            bool                    autoGenerateMips  = true,
            int                     mipMapCount       = 0,
            bool                    isShadowMap       = false,
            int                     anisoLevel        = 1,
            float                   mipMapBias        = 0,
            MSAASamples             msaaSamples       = MSAASamples.None,
            bool                    bindTextureMS     = false,
            bool                    useDynamicScale   = false,
            RenderTextureMemoryless memoryless        = RenderTextureMemoryless.None,
            string                  name              = ""
        )
        {

            var dimension = WaterSystem.IsSinglePassStereoEnabled ? TextureDimension.Tex2DArray : TextureDimension.Tex2D;
            var slices    = WaterSystem.IsSinglePassStereoEnabled ? 2 : 1;

            return WaterSystem.RTHandles.Alloc(width, height, slices: slices, depthBufferBits: depthBufferBits, colorFormat: colorFormat, filterMode: filterMode, wrapMode: wrapMode, dimension: dimension,
                                               enableRandomWrite: enableRandomWrite, useMipMap: useMipMap, autoGenerateMips: autoGenerateMips, isShadowMap: isShadowMap, anisoLevel: anisoLevel,
                                               mipMapBias: mipMapBias, msaaSamples: msaaSamples, bindTextureMS: bindTextureMS, useDynamicScale: useDynamicScale, memoryless: memoryless, name: name);
        }


        public static RTHandle RTHandleAllocVR(
            Vector2                 scaleFactor,
            DepthBits               depthBufferBits   = DepthBits.None,
            GraphicsFormat          colorFormat       = GraphicsFormat.R8G8B8A8_SRGB,
            FilterMode              filterMode        = FilterMode.Point,
            TextureWrapMode         wrapMode          = TextureWrapMode.Repeat,
            bool                    enableRandomWrite = false,
            bool                    useMipMap         = false,
            bool                    autoGenerateMips  = true,
            int                     mipMapCount       = 0,
            bool                    isShadowMap       = false,
            int                     anisoLevel        = 1,
            float                   mipMapBias        = 0,
            MSAASamples             msaaSamples       = MSAASamples.None,
            bool                    bindTextureMS     = false,
            bool                    useDynamicScale   = false,
            RenderTextureMemoryless memoryless        = RenderTextureMemoryless.None,
            string                  name              = ""
        )
        {
            var dimension = WaterSystem.IsSinglePassStereoEnabled ? TextureDimension.Tex2DArray : TextureDimension.Tex2D;
            var slices    = WaterSystem.IsSinglePassStereoEnabled ? 2 : 1;

            return WaterSystem.RTHandles.Alloc(scaleFactor, slices: slices, depthBufferBits: depthBufferBits, colorFormat: colorFormat, filterMode: filterMode, wrapMode: wrapMode, dimension: dimension,
                                               enableRandomWrite: enableRandomWrite, useMipMap: useMipMap, autoGenerateMips: autoGenerateMips, isShadowMap: isShadowMap, anisoLevel: anisoLevel,
                                               mipMapBias: mipMapBias, msaaSamples: msaaSamples, bindTextureMS: bindTextureMS, useDynamicScale: useDynamicScale, memoryless: memoryless, name: name);
        }

        public static RTHandle RTHandleAllocVR(
            ScaleFunc               scaleFunc,
            DepthBits               depthBufferBits   = DepthBits.None,
            GraphicsFormat          colorFormat       = GraphicsFormat.R8G8B8A8_SRGB,
            FilterMode              filterMode        = FilterMode.Point,
            TextureWrapMode         wrapMode          = TextureWrapMode.Repeat,
            bool                    enableRandomWrite = false,
            bool                    useMipMap         = false,
            bool                    autoGenerateMips  = true,
            bool                    isShadowMap       = false,
            int                     anisoLevel        = 1,
            float                   mipMapBias        = 0,
            int                     mipMapCount       = 0,
            MSAASamples             msaaSamples       = MSAASamples.None,
            bool                    bindTextureMS     = false,
            bool                    useDynamicScale   = false,
            RenderTextureMemoryless memoryless        = RenderTextureMemoryless.None,
            string                  name              = ""
        )
        {
            var dimension = WaterSystem.IsSinglePassStereoEnabled ? TextureDimension.Tex2DArray : TextureDimension.Tex2D;
            var slices    = WaterSystem.IsSinglePassStereoEnabled ? 2 : 1;

            return WaterSystem.RTHandles.Alloc(scaleFunc, slices: slices, depthBufferBits: depthBufferBits, colorFormat: colorFormat, filterMode: filterMode, wrapMode: wrapMode, dimension: dimension,
                                               enableRandomWrite: enableRandomWrite, useMipMap: useMipMap, autoGenerateMips: autoGenerateMips, isShadowMap: isShadowMap, anisoLevel: anisoLevel,
                                               mipMapBias: mipMapBias, msaaSamples: msaaSamples, bindTextureMS: bindTextureMS, useDynamicScale: useDynamicScale, memoryless: memoryless, name: name);
        }
    }
}