using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UnityEngine;
using Application = UnityEngine.Application;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace KWS
{
    public static class KW_Extensions
    {

        static float prevRealTime;
        static float lastDeltaTime;

     
        public enum AsyncInitializingStatusEnum
        {
            NonInitialized,
            StartedInitialize,
            Initialized,
            Failed,
            BakingStarted
        }

        public static Vector3 GetCameraPositionFast(Camera cam)
        {
            return WaterSystem.CameraDatas.TryGetValue(cam, out var camData) ? camData.Position : cam.transform.position;
        }

        public static Quaternion GetCameraRotationFast(Camera cam)
        {
            return WaterSystem.CameraDatas.TryGetValue(cam, out var camData) ? camData.CameraTransform.rotation : cam.transform.rotation;
        }

        public static Vector3 GetCameraForwardFast(Camera cam)
        {
            return WaterSystem.CameraDatas.TryGetValue(cam, out var camData) ? camData.Forward : cam.transform.forward;
        }


        public static Vector3 GetRelativeToCameraAreaPos(Camera cam, Vector3 camPos, float areaSize, float waterLevel)
        {
            /*var pos1 = cam.ViewportToWorldPoint(new Vector3(0.5f, 0.0f, cam.nearClipPlane));
             var pos2 = cam.ViewportToWorldPoint(new Vector3(0.5f, 0.0f, cam.farClipPlane));
             var ray = new Ray(pos1, (pos2 - pos1).normalized);

             var plane = new Plane(Vector3.down, waterLevel);
             if (plane.Raycast(ray, out var distanceToPlane))
             {
                 var camDir = cam.transform.forward;
                 var areaOffset = 0.45f;
                 return ray.GetPoint(distanceToPlane) + new Vector3(Mathf.Max(0.001f, camDir.x) * areaSize * areaOffset, 0, Mathf.Max(0.001f, camDir.z) * areaSize * areaOffset);
             }
             return cam.transform.position;*/
            var bottomCornerViewWorldPos = cam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, cam.nearClipPlane));
            var cornerDir = (bottomCornerViewWorldPos - camPos).normalized;
            var cornerDotX = Vector3.Dot(cornerDir, Vector3.right) / 2.0f + 0.5f;
            var cornerDotZ = Vector3.Dot(cornerDir, Vector3.forward) / 2.0f + 0.5f;

            float offsetX = Mathf.Lerp(-areaSize, areaSize, cornerDotX) * 0.5f;
            float offsetZ = Mathf.Lerp(-areaSize, areaSize, cornerDotZ) * 0.5f;
            return new Vector3(camPos.x + offsetX, waterLevel, camPos.z + offsetZ);
        }

        public static GameObject CreateHiddenGameObject(string name)
        {
            var go = new GameObject(name);
#if KWS_DEBUG
            go.hideFlags = HideFlags.DontSave;
#else
            go.hideFlags = HideFlags.HideAndDontSave;
#endif
            return go;

        }

        public static void SafeDestroy(params UnityEngine.Object[] components)
        {
            if (!Application.isPlaying)
            {
                foreach (var component in components)
                {
                    if (component != null) UnityEngine.Object.DestroyImmediate(component);
                }

            }
            else
            {
                foreach (var component in components)
                {
                    if (component != null) UnityEngine.Object.Destroy(component);
                }
            }
        }

        public static void SafeRelease(params RenderTexture[] renderTextures)
        {
            for (var i = 0; i < renderTextures.Length; i++)
            {
                if (renderTextures[i] != null) renderTextures[i].Release();
                renderTextures[i] = null;
            }
        }


        public static float TotalTime()
        {
            return (Application.isPlaying ? UnityEngine.Time.time : UnityEngine.Time.realtimeSinceStartup);
        }

        public static float DeltaTime()
        {
            if (Application.isPlaying)
            {
                return UnityEngine.Time.deltaTime;
            }
            else
            {
                return Mathf.Max(0.0001f, lastDeltaTime);
            }
        }

        private static bool _isEditorTimeUpdated;
        public static void UpdateEditorDeltaTime()
        {
            if (Application.isPlaying) return;

            if (_isEditorTimeUpdated) return;

            _isEditorTimeUpdated = true;
            lastDeltaTime = UnityEngine.Time.realtimeSinceStartup - prevRealTime;
            prevRealTime = UnityEngine.Time.realtimeSinceStartup;
        }

        public static void SetEditorDeltaTime()
        {
            _isEditorTimeUpdated = false;
        }

        static string GetPathToKriptoWaterSystemFolder()
        {
            var dirs = Directory.GetDirectories(Application.dataPath, "KriptoFX", SearchOption.AllDirectories);
            if (dirs.Length != 0)
            {
                var pathToShadersFolder = Path.Combine(dirs[0], "WaterSystem");
                if (Directory.Exists(pathToShadersFolder)) return pathToShadersFolder;
            }

            Debug.LogError("Can't find 'KriptoFX/WaterSystem' folder");
            return string.Empty;
        }

        static string GetAbsolutePathRelativeToAssets(string path)
        {
            var startAssetIndex = path.IndexOf("Assets", StringComparison.Ordinal);
            var absolutePath = path.Substring(startAssetIndex);
            absolutePath = absolutePath.Replace(@"\", "/");
            return absolutePath;
        }

        public static string GetPathToSavedDataFolderAbsolute()
        {
            var waterFolder = GetPathToKriptoWaterSystemFolder();
            if (waterFolder == String.Empty) return string.Empty;

            var pathToSavedDataFolder = Path.Combine(waterFolder, "WaterResources", "Resources", "SavedData");
            if (Directory.Exists(pathToSavedDataFolder)) return pathToSavedDataFolder;

            Debug.LogError("Can't find 'KriptoFX/WaterSystem/WaterResources/Resources/SavedData' folder");
            return string.Empty;
        }

        public static string GetPathToSavedDataFolderRelative()
        {
            var path = GetPathToSavedDataFolderAbsolute();
            return GetAbsolutePathRelativeToAssets(path);
        }

        public static string GetPathToWaterInstanceFolder(string waterInstanceID)
        {
            return Path.Combine(GetPathToSavedDataFolderRelative(), waterInstanceID);
        }

        public static string GetPathToStreamingAssetsFolder()
        {

            var streamingAssetData = Path.Combine(Application.streamingAssetsPath, "WaterSystemData");
            if (Directory.Exists(streamingAssetData)) return streamingAssetData;

            var dirs = Directory.GetDirectories(Application.dataPath, "WaterSystemData", SearchOption.AllDirectories);
            if (dirs.Length != 0)
            {
                if (Directory.Exists(dirs[0])) return dirs[0];
            }

            Debug.LogError("Can't find 'Assets/StreamingAssets/WaterSystemData' data folder");
            return string.Empty;
        }

        public static string GetPathToWaterShadersFolder()
        {
            var waterFolder = GetPathToKriptoWaterSystemFolder();
            if (waterFolder == String.Empty) return string.Empty;

            var pathToShadersFolder = Path.Combine(waterFolder, "WaterResources", "Shaders");
            if (Directory.Exists(pathToShadersFolder)) return pathToShadersFolder;

            Debug.LogError("Can't find 'KriptoFX/WaterSystem/WaterResources/Shaders' data folder");
            return string.Empty;
        }

        public static string GetAssetsRelativePathToFile(string fileName, string assetName)
        {
            var files = Directory.GetFiles(Application.dataPath, fileName, SearchOption.AllDirectories);
            foreach (var file in files)
            {
                if (!file.ToLower().Contains(assetName.ToLower())) continue;

                return GetAbsolutePathRelativeToAssets(file);
            }

            return string.Empty;
        }

        public enum UsedChannels
        {
            _R = 0,
            _RG = 1,
            _RGB = 2,
            _RGBA = 3
        }


        public static void SaveTexture(this Texture2D tex, string pathToFileWithoutExtension, bool useAutomaticCompressionFormat, UsedChannels usedChannels = default, bool isHDR = false, bool mipChain = false)
        {
#if UNITY_EDITOR
            TextureImporter.Write(tex, pathToFileWithoutExtension, useAutomaticCompressionFormat, usedChannels, isHDR, mipChain);
            UnityEditor.AssetDatabase.Refresh();
#endif
        }

        public static void SaveRenderTexture(this RenderTexture rt, string pathToFileWithoutExtension, bool useAutomaticCompressionFormat, UsedChannels usedChannels = default, bool isHDR = false, bool mipChain = false)
        {
#if UNITY_EDITOR
            if (rt == null) return;

            var currentRT = RenderTexture.active;
            RenderTexture.active = rt;

            var targetFormat = isHDR ? TextureFormat.RGBAHalf : TextureFormat.RGBA32;
            var tex = new Texture2D(rt.width, rt.height, targetFormat, false, linear: true);
            tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            tex.Apply();

            TextureImporter.Write(tex, pathToFileWithoutExtension, useAutomaticCompressionFormat, usedChannels, isHDR, mipChain);
            RenderTexture.active = currentRT;
            SafeDestroy(tex);
#endif
        }

        public static async Task ReadAsync(this ResourceRequest controller)
        {
            var tcs = new TaskCompletionSource<object>();

            void Handler(AsyncOperation n)
            {
                tcs.TrySetResult(null);
            }

            try
            {
                controller.completed += Handler;
                await tcs.Task;
            }
            finally
            {
                controller.completed -= Handler;
            }
        }

        public static async Task<T[]> LoadBinaryDataAsync<T>(string relativePathToFile)
        {
            try
            {
                var request = Resources.LoadAsync<TextAsset>(relativePathToFile);
                await request.ReadAsync();
                var bytes = ((TextAsset)request.asset).bytes;
                Resources.UnloadUnusedAssets();

                var dataRaw = new T[bytes.Length / Marshal.SizeOf(default(T))];
                Buffer.BlockCopy(bytes, 0, dataRaw, 0, bytes.Length);

                return dataRaw;
            }
            catch (Exception e)
            {
                Debug.LogError("ReadTextureFromFileAsync error: " + e.Message);
                return default;
            }
        }

        static bool CheckAndCreateDirectory(string path)
        {
            var directory = Path.GetDirectoryName(path);
            if (directory == null)
            {
                Debug.LogError("Can't find directory: " + path);
                return false;
            }

            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
            return true;
        }

        public static T SaveScriptableData<T>(this T data, string waterInstanceID, string assetNameWithoutExtenstion, string name = default) where T : ScriptableObject
        {
#if UNITY_EDITOR
            var newData = Object.Instantiate(data);
            newData.name = name == default ? assetNameWithoutExtenstion : name;

            var pathToInstanceFolder = GetPathToWaterInstanceFolder(waterInstanceID);
            if (!Directory.Exists(pathToInstanceFolder)) Directory.CreateDirectory(pathToInstanceFolder);
            UnityEditor.AssetDatabase.CreateAsset(newData, Path.Combine(pathToInstanceFolder, assetNameWithoutExtenstion + ".asset"));
            UnityEditor.AssetDatabase.SaveAssets();

            return newData;
#else
            Debug.LogError("You can't save waves data in runtime");
            return default;
#endif
        }

        static void CheckAndRemoveOldFile(string pathWithExtension)
        {
            if (File.Exists(pathWithExtension)) File.Delete(pathWithExtension);
        }

        public static void SaveBinaryData<T>(T[] data, string pathToFileWithoutExtension) where T : struct
        {
            if (!CheckAndCreateDirectory(pathToFileWithoutExtension)) return;
            var fullPath = pathToFileWithoutExtension + ".bytes";
            CheckAndRemoveOldFile(fullPath);
            try
            {
                var byteArray = new byte[data.Length * Marshal.SizeOf(default(T))];
                Buffer.BlockCopy(data, 0, byteArray, 0, byteArray.Length);
                File.WriteAllBytes(fullPath, byteArray);
            }
            catch (Exception e)
            {
                Debug.LogError("SaveDataToFile error: " + e.Message);
                return;
            }
        }

        public static bool IsLayerRendered(this Camera cam, int layer)
        {
            return (cam.cullingMask & (1 << layer)) != 0;
        }

        public static void WeldVertices(ref List<Vector3> verts, ref List<int> tris)
        {
            WeldVertices(ref verts, ref tris, null);
        }

        public static void WeldVertices(ref List<Vector3> verts, ref List<int> tris, ref List<Color> colors, ref List<Vector3> normals)
        {
            var data = new WeldData {colors = colors, normals = normals};
            WeldVertices(ref verts, ref tris, data);
            colors = data.colors;
            normals = data.normals;
        }

        public static void WeldVertices(ref List<Vector3> verts, ref List<int> tris, ref List<Color> colors, ref List<Vector3> normals, ref List<float> uv)
        {
            var data = new WeldData { colors = colors, normals = normals, uvFloat = uv};
            WeldVertices(ref verts, ref tris, data);
            colors  = data.colors;
            normals = data.normals;
            uv = data.uvFloat;
        }

        public static void WeldVertices(ref List<Vector3> verts, ref List<int> tris, ref List<Color> colors, ref List<Vector3> normals, ref List<Vector2> uv)
        {
            var data = new WeldData { colors = colors, normals = normals, uvVector2 = uv };
            WeldVertices(ref verts, ref tris, data);
            colors  = data.colors;
            normals = data.normals;
            uv      = data.uvVector2;
        }

        class WeldData
        {
            public List<Color> colors; 
            public List<Vector3> normals;
            public List<float> uvFloat;
            public List<Vector2> uvVector2;
            public List<Vector3> uvVector3;
            public List<Vector4> uvVector4;
        }

        static void WeldVertices(ref List<Vector3> verts, ref List<int> tris, WeldData weldData)
        {
            var vertsCount = verts.Count;

            var duplicateHashTable = new Dictionary<Vector3, int>();
            var map = new int[vertsCount];
            List<int> hashVerts = new List<int>();

            for (int i = 0; i < vertsCount; i++)
            {
                if (!duplicateHashTable.ContainsKey(verts[i]))
                {
                    duplicateHashTable.Add(verts[i], hashVerts.Count);
                    map[i] = hashVerts.Count;
                    hashVerts.Add(i);
                }
                else
                {
                    map[i] = duplicateHashTable[verts[i]];
                }
            }

            // create new vertices
            var newVerts = new List<Vector3>();
            var newColors = new List<Color>();
            var newNormals = new List<Vector3>();
            var newUvFloats = new List<float>();
            var newUvVector2 = new List<Vector2>();
            var newUvVector3 = new List<Vector3>();
            var newUvVector4 = new List<Vector4>();

            for (int i = 0; i < hashVerts.Count; i++)
            {
                newVerts.Add(verts[hashVerts[i]]);
            }

            if (weldData.colors != null)
            {
                for (int i = 0; i < hashVerts.Count; i++) newColors.Add(weldData.colors[hashVerts[i]]);
            }
            if (weldData.normals != null)
            {
                for (int i = 0; i < hashVerts.Count; i++) newNormals.Add(weldData.normals[hashVerts[i]]);
            }

            if (weldData.uvFloat != null)
            {
                for (int i = 0; i < hashVerts.Count; i++) newUvFloats.Add(weldData.uvFloat[hashVerts[i]]);
            }

            if (weldData.uvVector2 != null)
            {
                for (int i = 0; i < hashVerts.Count; i++) newUvVector2.Add(weldData.uvVector2[hashVerts[i]]);
            }

            if (weldData.uvVector3 != null)
            {
                for (int i = 0; i < hashVerts.Count; i++) newUvVector3.Add(weldData.uvVector3[hashVerts[i]]);
            }


            if (weldData.uvVector4 != null)
            {
                for (int i = 0; i < hashVerts.Count; i++) newUvVector4.Add(weldData.uvVector4[hashVerts[i]]);
            }


            var trisCount = tris.Count;
            for (int i = 0; i < trisCount; i++)
            {
                tris[i] = map[tris[i]];
            }

            verts = newVerts;

            weldData.colors = newColors;
            weldData.normals = newNormals;
            weldData.uvFloat = newUvFloats;
            weldData.uvVector2 = newUvVector2;
            weldData.uvVector3 = newUvVector3;
            weldData.uvVector4 = newUvVector4;
        }

        public static void CalculateNearPlaneWorldPoints(ref Vector3[] points, Camera cam)
        {
            points[0] = cam.ViewportToWorldPoint(new Vector3(0, 0, cam.nearClipPlane));
            points[1] = cam.ViewportToWorldPoint(new Vector3(1, 0, cam.nearClipPlane));
            points[2] = cam.ViewportToWorldPoint(new Vector3(0, 1, cam.nearClipPlane));
            points[3] = cam.ViewportToWorldPoint(new Vector3(1, 1, cam.nearClipPlane));
        }

        public static Plane[] CalculateFrustumPlanes(ref Plane[] planes, Camera cam, WaterSystem.WaterMeshTypeEnum meshType, float maxWavesHeight)
        {
            //var planes = GeometryUtility.CalculateFrustumPlanes(cam);
            GeometryUtility.CalculateFrustumPlanes(cam, planes);
            if (meshType == WaterSystem.WaterMeshTypeEnum.InfiniteOcean)
            {
                planes[0].distance += KWS_Settings.Water.UpdateQuadtreeEveryMetersForward;
                planes[1].distance += KWS_Settings.Water.UpdateQuadtreeEveryMetersForward;
                planes[2].distance += KWS_Settings.Water.UpdateQuadtreeEveryMetersForward;
            }
            else if (meshType == WaterSystem.WaterMeshTypeEnum.FiniteBox)
            {
                planes[0].distance += maxWavesHeight;
                planes[1].distance += maxWavesHeight;
            }

            return planes;
        }

        public static void CalculateFrustumCorners(ref Vector3[] corners, Camera cam)
        {
            if (cam.orthographic) CalculateFrustumCornersOrthographic(ref corners, cam);
            else CalculateFrustumCornersPerspective(ref corners, cam);
        }

        static void CalculateFrustumCornersOrthographic(ref Vector3[] corners, Camera cam)
        {
            var camTransform = cam.transform;
            var position = camTransform.position;
            var orientation = camTransform.rotation;
            var farClipPlane = cam.farClipPlane;
            var nearClipPlane = cam.nearClipPlane;

            var forward = orientation * Vector3.forward;
            var right = orientation * Vector3.right * cam.orthographicSize * cam.aspect;
            var up = orientation * Vector3.up * cam.orthographicSize;

            // CORNERS:
            // [0] = Far Bottom Left,  [1] = Far Top Left,  [2] = Far Top Right,  [3] = Far Bottom Right, 
            // [4] = Near Bottom Left, [5] = Near Top Left, [6] = Near Top Right, [7] = Near Bottom Right

            corners[0] = position + forward * farClipPlane - up - right;
            corners[1] = position + forward * farClipPlane + up - right;
            corners[2] = position + forward * farClipPlane + up + right;
            corners[3] = position + forward * farClipPlane - up + right;
            corners[4] = position + forward * nearClipPlane - up - right;
            corners[5] = position + forward * nearClipPlane + up - right;
            corners[6] = position + forward * nearClipPlane + up + right;
            corners[7] = position + forward * nearClipPlane - up + right;
        }

        static void CalculateFrustumCornersPerspective(ref Vector3[] corners, Camera cam)
        {
            var position = cam.transform.position;
            var rotation = cam.transform.rotation;
            
            float fovWHalf = cam.fieldOfView * 0.5f;

            Vector3 toRight = Vector3.right * Mathf.Tan(fovWHalf * Mathf.Deg2Rad) * cam.aspect;
            Vector3 toTop = Vector3.up * Mathf.Tan(fovWHalf * Mathf.Deg2Rad);
            var forward = Vector3.forward;

            Vector3 topLeft = (forward - toRight + toTop);
            float camScale = topLeft.magnitude * cam.farClipPlane;

            topLeft.Normalize();
            topLeft *= camScale;

            Vector3 topRight = (forward + toRight + toTop);
            topRight.Normalize();
            topRight *= camScale;

            Vector3 bottomRight = (forward + toRight - toTop);
            bottomRight.Normalize();
            bottomRight *= camScale;

            Vector3 bottomLeft = (forward - toRight - toTop);
            bottomLeft.Normalize();
            bottomLeft *= camScale;

            corners[0] = position + rotation * bottomLeft;
            corners[1] = position + rotation * topLeft;
            corners[2] = position + rotation * topRight;
            corners[3] = position + rotation * bottomRight;

            topLeft = (forward - toRight + toTop);
            camScale = topLeft.magnitude * cam.nearClipPlane;

            topLeft.Normalize();
            topLeft *= camScale;

            topRight = (forward + toRight + toTop);
            topRight.Normalize();
            topRight *= camScale;

            bottomRight = (forward + toRight - toTop);
            bottomRight.Normalize();
            bottomRight *= camScale;

            bottomLeft = (forward - toRight - toTop);
            bottomLeft.Normalize();
            bottomLeft *= camScale;

            corners[4] = position + rotation * bottomLeft;
            corners[5] = position + rotation * topLeft;
            corners[6] = position + rotation * topRight;
            corners[7] = position + rotation * bottomRight;
        }

        public static bool IsBoxVisibleApproximated(ref Plane[] planes, Vector3 min, Vector3 max)
        {
            Vector3 vmin, vmax;

            for (int planeIndex = 0; planeIndex < planes.Length; planeIndex++)
            {
                var normal = planes[planeIndex].normal;
                var planeDistance = planes[planeIndex].distance;

                // X axis
                if (normal.x < 0)
                {
                    vmin.x = min.x;
                    vmax.x = max.x;
                }
                else
                {
                    vmin.x = max.x;
                    vmax.x = min.x;
                }

                // Y axis
                if (normal.y < 0)
                {
                    vmin.y = min.y;
                    vmax.y = max.y;
                }
                else
                {
                    vmin.y = max.y;
                    vmax.y = min.y;
                }

                if (normal.z < 0)
                {
                    vmin.z = min.z;
                    vmax.z = max.z;
                }
                else
                {
                    vmin.z = max.z;
                    vmax.z = min.z;
                }

                var dot1 = normal.x * vmin.x + normal.y * vmin.y + normal.z * vmin.z;
                if (dot1 + planeDistance < 0)
                    return false;

            }

            return true;
        }

        public static bool IsBoxVisibleAccurate(ref Plane[] planes, ref Vector3[] corners, Vector3 min, Vector3 max)
        {
            if (!IsBoxVisibleApproximated(ref planes, min, max)) return false;

            int isOutFrustum = 0;
            for (int i = 0; i < 8; i++) isOutFrustum += ((corners[i].x > max.x) ? 1 : 0);
            if (isOutFrustum == 8) return false;
            isOutFrustum = 0;
            for (int i = 0; i < 8; i++) isOutFrustum += ((corners[i].x < min.x) ? 1 : 0);
            if (isOutFrustum == 8) return false;
            isOutFrustum = 0;
            for (int i = 0; i < 8; i++) isOutFrustum += ((corners[i].y > max.y) ? 1 : 0);
            if (isOutFrustum == 8) return false;
            isOutFrustum = 0;
            for (int i = 0; i < 8; i++) isOutFrustum += ((corners[i].y < min.y) ? 1 : 0);
            if (isOutFrustum == 8) return false;
            isOutFrustum = 0;
            for (int i = 0; i < 8; i++) isOutFrustum += ((corners[i].z > max.z) ? 1 : 0);
            if (isOutFrustum == 8) return false;
            isOutFrustum = 0;
            for (int i = 0; i < 8; i++) isOutFrustum += ((corners[i].z < min.z) ? 1 : 0);
            if (isOutFrustum == 8) return false;

            return true;
        }

        static Vector2[] _corners = new Vector2[8];
        public static Rect GetScreenSpaceBounds(Camera cam, Bounds bounds)
        {
            var posX = bounds.center.x + bounds.extents.x;
            var negX = bounds.center.x - bounds.extents.x;
            var posY = bounds.center.y + bounds.extents.y;
            var negY = bounds.center.y - bounds.extents.y;
            var posZ = bounds.center.z + bounds.extents.z;
            var negZ = bounds.center.z - bounds.extents.z;

            _corners[0] = cam.WorldToScreenPoint(new Vector3(posX, posY, posZ));
            _corners[1] = cam.WorldToScreenPoint(new Vector3(posX, posY, negZ));
            _corners[2] = cam.WorldToScreenPoint(new Vector3(posX, negY, posZ));
            _corners[3] = cam.WorldToScreenPoint(new Vector3(posX, negY, negZ));
            _corners[4] = cam.WorldToScreenPoint(new Vector3(negX, posY, posZ));
            _corners[5] = cam.WorldToScreenPoint(new Vector3(negX, posY, negZ));
            _corners[6] = cam.WorldToScreenPoint(new Vector3(negX, negY, posZ));
            _corners[7] = cam.WorldToScreenPoint(new Vector3(negX, negY, negZ));

            var screenSize = new Vector2(cam.scaledPixelWidth, cam.scaledPixelHeight);
            for (int i = 0; i < 8; i++)
            {
                _corners[i].y = screenSize.y - _corners[i].y;
                _corners[i] = ClampVector2(_corners[i], Vector2.zero, screenSize);
            }

            float min_x = _corners[0].x;
            float min_y = _corners[0].y;
            float max_x = _corners[0].x;
            float max_y = _corners[0].y;

            for (int i = 1; i < 8; i++)
            {
                if (_corners[i].x < min_x)
                {
                    min_x = _corners[i].x;
                }
                if (_corners[i].y < min_y)
                {
                    min_y = _corners[i].y;
                }
                if (_corners[i].x > max_x)
                {
                    max_x = _corners[i].x;
                }
                if (_corners[i].y > max_y)
                {
                    max_y = _corners[i].y;
                }
            }

            return new Rect(min_x, screenSize.y - max_y, max_x - min_x, max_y - min_y);

        }
      
        public static Vector3 ClampVector3(Vector3 current, Vector3 min, Vector3 max)
        {
            current.x = Mathf.Clamp(current.x, min.x, max.x);
            current.y = Mathf.Clamp(current.y, min.y, max.y);
            current.z = Mathf.Clamp(current.z, min.z, max.z);
            return current;
        }
        public static Vector2 ClampVector2(Vector2 current, Vector2 min, Vector2 max)
        {
            current.x = Mathf.Clamp(current.x, min.x, max.x);
            current.y = Mathf.Clamp(current.y, min.y, max.y);
            return current;
        }

        public static List<T> InitializeListWithDefaultValues<T>(int count, T defaultValue)
        {
            var list = new List<T>(count);
            for (int i = 0; i < count; i++)
            {
                list.Add(defaultValue);
            }

            return list;
        }


        [Flags]
        public enum WaterLogMessageType
        {
            Info = 1,
            Release = 2,
            Initialize = 4,
            InitializeRT = 8,
            Error = 16,
            DynamicUpdate = 32,
            StaticUpdate = 64
        }

//#if KWS_DEBUG
//        private static WaterLogMessageType _debugFlags = WaterLogMessageType.Info | WaterLogMessageType.Initialize | WaterLogMessageType.InitializeRT | WaterLogMessageType.Error | WaterLogMessageType.StaticUpdate;
//        //private static WaterLogMessageType _debugFlags = WaterLogMessageType.Error;
//#endif

//        public static void WaterLog(this object sender, string message, WaterLogMessageType logType = WaterLogMessageType.Info)
//        {
//#if KWS_DEBUG
//            switch (logType)
//            {
//                case WaterLogMessageType.Info:
//                    if (_debugFlags.HasFlag(WaterLogMessageType.Info))
//                        Debug.Log($"<color=#bababa>{sender} </color> : <color=#ebebeb>{message}</color>");
//                    break;
//                case WaterLogMessageType.Release:
//                    if (_debugFlags.HasFlag(WaterLogMessageType.Release))
//                        Debug.Log($"<color=#bababa>{sender} </color> : <color=#86c6db>{message}</color>");
//                    break;
//                case WaterLogMessageType.Initialize:
//                    if (_debugFlags.HasFlag(WaterLogMessageType.Initialize))
//                        Debug.Log($"<color=#bababa>{sender} </color> : <color=#70fa75>{message}</color>");
//                    break;
//                case WaterLogMessageType.InitializeRT:
//                    if (_debugFlags.HasFlag(WaterLogMessageType.InitializeRT))
//                        Debug.Log($"<color=#bababa>{sender} </color> : <color=#86eb90>{message}</color>");
//                    break;
//                case WaterLogMessageType.Error:
//                    if (_debugFlags.HasFlag(WaterLogMessageType.Error))
//                        Debug.Log($"<color=#bababa>{sender} </color> : <color=#ff0000>{message}</color>");
//                    break;
//                case WaterLogMessageType.DynamicUpdate:
//                    if (_debugFlags.HasFlag(WaterLogMessageType.DynamicUpdate))
//                        Debug.Log($"<color=#bababa>{sender} </color> : <color=#ffca69>{message}</color>");
//                    break;
//                case WaterLogMessageType.StaticUpdate:
//                    if (_debugFlags.HasFlag(WaterLogMessageType.StaticUpdate))
//                        Debug.Log($"<color=#bababa>{sender} </color> : <color=#36d0ff>{message}</color>");
//                    break;
//                default:
//                    Debug.Log($"<color=#bababa>{sender} </color> : <color=#ebebeb>{message}</color>");
//                    break;
//            }

//#endif
//        }

//        public static void WaterLog(this object sender, params RenderTexture[] renderTextures)
//        {
//#if KWS_DEBUG
//            foreach (var rt in renderTextures)
//            {
//                if (rt != null) WaterLog(sender, $"Initialized texture {rt.name} [{rt.width}] [{rt.height}] {rt.format} mip:{rt.useMipMap} slices:{rt.volumeDepth}", WaterLogMessageType.InitializeRT);
//            }
//#endif
//        }

//        public static void WaterLog(this object sender, params RTHandle[] renderTextures)
//        {
//#if KWS_DEBUG
//            foreach (var handle in renderTextures)
//            {
//                if (handle != null && handle.rt != null) WaterLog(sender, $"Initialized texture {handle.name} [{handle.rt.width}] [{handle.rt.height}] {handle.rt.format} mip:{handle.rt.useMipMap} slices:{handle.rt.volumeDepth}", WaterLogMessageType.InitializeRT);
//            }
//#endif
//        }


        public static string SpaceBetweenThousand(int number)
        {
            return string.Format("{0:#,0}", number);
        }
    }
}