using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using static KWS.KWS_CoreUtils;

namespace KWS
{
    public class KWS_FFT_ToHeightMap 
    {
        Material dataHeightMaterial;
        public RenderTexture temp;
        TemporaryRenderTexture HeighDataTexture = new TemporaryRenderTexture();
        public Mesh heightDataMesh;
        ComputeShader computeShader;
        ComputeBuffer computeBuffer;

        private WaterSurfaceData _surfaceData = new WaterSurfaceData();
        Unity.Collections.NativeArray<FFTHeightData> rawHeightData;
        float currentHeightDataDomainScale;
        int lastSize;
        float _waterHeight;
        public struct FFTHeightData
        {
            public float height;
            public Vector3 normal;
        }

        bool isHeightDataUpdated;

        public delegate void HeightDataHandler();
        public event HeightDataHandler IsDataReadCompleted;

        public void Release()
        {
            KW_Extensions.SafeDestroy(dataHeightMaterial, heightDataMesh);
            HeighDataTexture.Release();
            if (computeBuffer != null) computeBuffer.Release();
            lastSize = 0;

            isHeightDataUpdated = false;
        }

        public void InitializeMaterials(WaterSystem water)
        {
            if (dataHeightMaterial == null)
            {
                dataHeightMaterial = KWS_CoreUtils.CreateMaterial("Hidden/KriptoFX/KWS/FFT_ToHeightMap");
                water.AddShaderToWaterRendering(dataHeightMaterial);
            }

        }

        void InitializeResources(WaterSystem waterInstance, int size)
        {
            HeighDataTexture.Alloc("HeighDataRT", size, size, 0, GraphicsFormat.R16_SFloat);

            if (computeShader == null) computeShader = (ComputeShader)Resources.Load("Common/FFT/ComputeFFT_Height");

            var bufferSize = sizeof(float) * 4; //height float + normal float x3
            computeBuffer = new ComputeBuffer(size * size, bufferSize);

            computeShader.SetTexture(0, "RawHeightDataTex", HeighDataTexture);
            computeShader.SetBuffer(0, "RawHeightData", computeBuffer);

            GeneratePlane(size, 1.1f, true);

            InitializeMaterials(waterInstance);

            lastSize = size;
            isHeightDataUpdated = false;
        }

        public WaterSurfaceData GetWaterSurfaceData(Vector3 worldPosition)
        {
            if (!isHeightDataUpdated)
            {
                _surfaceData.IsActualDataReady = false;
                _surfaceData.Position = worldPosition;
                _surfaceData.Normal = Vector3.up;
                return _surfaceData;
            }


            var domainScale = currentHeightDataDomainScale;
            var x = (worldPosition.x + domainScale * 0.5f) % domainScale;
            var z = (worldPosition.z + domainScale * 0.5f) % domainScale;

            if (x < 0) x = domainScale + x;
            if (z < 0) z = domainScale + z;

            x = HeighDataTexture.rt.width * (x / domainScale);
            z = HeighDataTexture.rt.height * (z / domainScale);

            var pixelIdx = (int)x + HeighDataTexture.rt.height * (int)z;
            if (!rawHeightData.IsCreated || pixelIdx > rawHeightData.Length - 1)
            {
                _surfaceData.IsActualDataReady = false;
                _surfaceData.Position          = worldPosition;
                _surfaceData.Normal            = Vector3.up;
                return _surfaceData;
            }

            var rawData = rawHeightData[pixelIdx];
            _surfaceData.IsActualDataReady = true;
            _surfaceData.Position = new Vector3(worldPosition.x, rawData.height + _waterHeight, worldPosition.z);
            _surfaceData.Normal = rawData.normal;
            // Debug.DrawRay(waterPos, rawHeightData[pixelIdx].normal);
            return _surfaceData;
        }

        public void UpdateHeightData(WaterSystem waterInstance, int size, float domainScale, Vector3 waterPos)
        {
            _waterHeight = waterPos.y;
            if (size != lastSize || Math.Abs(currentHeightDataDomainScale - domainScale) > 0.001f) InitializeResources(waterInstance, size);
            currentHeightDataDomainScale = domainScale;
       
            Graphics.SetRenderTarget(HeighDataTexture.rt);
            GL.Clear(false, true, Color.black);
            dataHeightMaterial.SetFloat("KW_FFT_DomainScale", domainScale);
            dataHeightMaterial.SetVector("KW_FFT_CameraPosition", waterPos);
            dataHeightMaterial.SetPass(0);
            Graphics.DrawMeshNow(heightDataMesh, Matrix4x4.identity);
            Graphics.SetRenderTarget(null);

            isHeightDataUpdated = false;
            computeShader.SetFloat("RawHeightDataSize", size);

            computeShader.Dispatch(0, size / 8, size / 8, 1);
            AsyncGPUReadback.Request(computeBuffer, OnCompleteGPUReadback); //todo empty callback cause CG allocation, why?  
        }

        private void OnCompleteGPUReadback(AsyncGPUReadbackRequest request)
        {
            if (request.hasError)
            {
                Debug.Log("FFT HeightData GPU readback error detected.");
                return;
            }
            if (request.done)
            {
                rawHeightData = request.GetData<FFTHeightData>();
                isHeightDataUpdated = true;
                IsDataReadCompleted?.Invoke();
            }
        }

        private void GeneratePlane(int meshResolution, float scale, bool useXZplane = true)
        {
            if (heightDataMesh == null)
            {
                heightDataMesh = new Mesh();
                heightDataMesh.indexFormat = IndexFormat.UInt32;
            }


            var vertices = new Vector3[(meshResolution + 1) * (meshResolution + 1)];
            var uv = new Vector2[vertices.Length];
            var triangles = new int[meshResolution * meshResolution * 6];

            for (int i = 0, y = 0; y <= meshResolution; y++)
                for (var x = 0; x <= meshResolution; x++, i++)
                {
                    if (useXZplane) vertices[i] = new Vector3(x * scale / meshResolution - 0.5f * scale, 0, y * scale / meshResolution - 0.5f * scale);
                    else vertices[i] = new Vector3(x * scale / meshResolution - 0.5f * scale, y * scale / meshResolution - 0.5f * scale, 0);
                    uv[i] = new Vector2(x * scale / meshResolution, y * scale / meshResolution);
                }

            for (int ti = 0, vi = 0, y = 0; y < meshResolution; y++, vi++)
                for (var x = 0; x < meshResolution; x++, ti += 6, vi++)
                {
                    triangles[ti] = vi;
                    triangles[ti + 3] = triangles[ti + 2] = vi + 1;
                    triangles[ti + 4] = triangles[ti + 1] = vi + meshResolution + 1;
                    triangles[ti + 5] = vi + meshResolution + 2;
                }

            heightDataMesh.Clear();
            heightDataMesh.vertices = vertices;
            heightDataMesh.uv = uv;
            heightDataMesh.triangles = triangles;
        }
    }

    public class WaterSurfaceData
    {
        public bool IsActualDataReady;
        public Vector3 Position;
        public Vector3 Normal;
    }
}