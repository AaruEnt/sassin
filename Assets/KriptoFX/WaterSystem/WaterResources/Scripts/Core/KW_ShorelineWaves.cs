using System;
using System.Collections.Generic;

using UnityEngine;
using static KWS.ShorelineWavesScriptableData;


namespace KWS
{
    [Serializable]
    public class KW_ShorelineWaves
    {
        private const string shorelineWavesDataAssetName = "ShorelineWavesScriptableData";

        [SerializeField] List<ShorelineWave> _currentWaves = new List<ShorelineWave>();
        private WaveBuffers _wavesBuffers = new WaveBuffers();

        internal static readonly Dictionary<int, PrebakedWaveType> WaveTypes = new Dictionary<int, PrebakedWaveType>()
        {
            {0, new PrebakedWaveType() {Size = new Vector3(14f, 4.5f, 16f)}}
        };

        internal class PrebakedWaveType
        {
            public Vector3 Size;
            //public float AnimationDuration;
            //public float MaxFrames;
        }

  

        public void InitializeWaves(ShorelineWavesScriptableData savedData)
        {
            if (savedData == null) _currentWaves.Clear();
            else _currentWaves = savedData.Waves;
        }

        public List<ShorelineWave> GetInitializedWaves()
        {
            return _currentWaves;
        }

        public WaveBuffers UpdateShorelineBuffers(Camera cam, Vector3 camPos, Vector3 waterPos, WaterSystem.ShorelineFoamQualityEnum quality)
        {
            _wavesBuffers.Clear();

            var areaSize = KWS_Settings.Shoreline.ShorelineWavesAreaSize;

            _wavesBuffers.SurfaceWavesAreaPos = KW_Extensions.GetRelativeToCameraAreaPos(cam, camPos, areaSize, waterPos.y);
            var minAreaSize = _wavesBuffers.SurfaceWavesAreaPos - areaSize * 0.5f * Vector3.one;
            var maxAreaSize = _wavesBuffers.SurfaceWavesAreaPos + areaSize * 0.5f * Vector3.one;

            var addedWaves = 0;

            for (var waveIdx = 0; waveIdx < _currentWaves.Count; waveIdx++)
            {
                var wave = _currentWaves[waveIdx];
                if (!wave.IsWaveVisible(ref WaterSystem.CurrentCameraFrustumPlanes, ref WaterSystem.CurrentCameraFrustumCorners)) continue;

                var distanceToWave = wave.GetDistanceToCamera(camPos);
                var lodIdx = GetLodIndexByDistance(distanceToWave, quality);
                _wavesBuffers.VisibleFoamWavesWithLods[lodIdx].Add(addedWaves, wave);
                _wavesBuffers.VisibleFoamWaves.Add(wave);
                addedWaves++;

                if (wave.IsWaveInsideWorldArea(minAreaSize, maxAreaSize)) _wavesBuffers.VisibleSurfaceWaves.Add(wave);
            }

            foreach (var lod in _wavesBuffers.VisibleFoamWavesWithLods)
            {
                foreach (var wave in lod)
                {
                    _wavesBuffers.VisibleFoamWavesLodIndexes.Add(wave.Key);
                }
            }

            MeshUtils.InitializePropertiesBuffer(_wavesBuffers.VisibleSurfaceWaves, ref _wavesBuffers.SurfaceWavesComputeBuffer, false);
            MeshUtils.InitializePropertiesBuffer(_wavesBuffers.VisibleFoamWaves, ref _wavesBuffers.FoamWavesComputeBuffer, false);
           
            return _wavesBuffers;
        }

        public ShorelineWavesScriptableData SaveWavesDataToAsset(string waterInstanceID)
        {
#if UNITY_EDITOR
            var data = ScriptableObject.CreateInstance<ShorelineWavesScriptableData>();
            data.Waves = _currentWaves;
            return data.SaveScriptableData(waterInstanceID, shorelineWavesDataAssetName);
#else
            Debug.LogError("You can't save waves data in runtime");
            return null;
#endif
        }

        public void Release()
        {
            _wavesBuffers.Release();
        }

        public class WaveBuffers
        {
            public Vector3 SurfaceWavesAreaPos { get; internal set; }

            public ComputeBuffer SurfaceWavesComputeBuffer;
            public ComputeBuffer FoamWavesComputeBuffer;

            public List<ShorelineWave> VisibleSurfaceWaves { get; internal set; }
            public List<ShorelineWave> VisibleFoamWaves    { get; internal set; }

            public Dictionary<int, ShorelineWave>[] VisibleFoamWavesWithLods   { get; internal set; }
            public List<int>             VisibleFoamWavesLodIndexes { get; internal set; }

            internal WaveBuffers()
            {
                VisibleSurfaceWaves        = new List<ShorelineWave>();
                VisibleFoamWaves           = new List<ShorelineWave>();
                VisibleFoamWavesLodIndexes = new List<int>();

                var maxLodLevels = KWS_Settings.Shoreline.LodDistances.Length;
                VisibleFoamWavesWithLods = new Dictionary<int, ShorelineWave>[maxLodLevels];
                for (var i = 0; i < maxLodLevels; i++) VisibleFoamWavesWithLods[i] = new Dictionary<int, ShorelineWave>();
            }

            internal void Clear()
            {
                VisibleSurfaceWaves.Clear();
                VisibleFoamWaves.Clear();
                VisibleFoamWavesLodIndexes.Clear();
                foreach (var shorelineWave in VisibleFoamWavesWithLods) shorelineWave.Clear();
            }

            internal void Release()
            {
                if(SurfaceWavesComputeBuffer != null) SurfaceWavesComputeBuffer.Release();
                if(FoamWavesComputeBuffer != null) FoamWavesComputeBuffer.Release();
                SurfaceWavesComputeBuffer = FoamWavesComputeBuffer = null;

                Clear();
            }
        }

        int GetLodIndexByDistance(float distance, WaterSystem.ShorelineFoamQualityEnum quality)
        {
            var lodDistances = KWS_Settings.Shoreline.LodDistances;
            var offset = KWS_Settings.Shoreline.LodOffset[quality];
            for (int i = 0; i < lodDistances.Length; i++)
            {
                if (distance < lodDistances[i] + offset) return i;
            }

            return lodDistances.Length - 1;
        }
    }

}