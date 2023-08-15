using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace KWS
{
    public class OrthoDepthPass : WaterPass
    {
        OrthoDepthPassCore _pass;

        public OrthoDepthPass(RenderPassEvent renderPassEvent, WaterSystem waterInstance)
        {
            WaterInstance = waterInstance;
            this.renderPassEvent = renderPassEvent; 
        }

        void Initialize()
        {
            IsInitialized = true;
            
            _pass                  =  new OrthoDepthPassCore(WaterInstance);
            _pass.OnRender         += OnRender;
            WaterInstance.OnUpdate += OnUpdate;
        }

        private void OnUpdate(WaterSystem waterSystem, Camera camera)
        {
            if (!WaterInstance.IsWaterVisible || !KWS_CoreUtils.CanRenderWaterForCurrentCamera(WaterInstance, camera)) return;
            _pass.Execute(camera);
        }

        private void OnRender(OrthoDepthPassCore.PassData passData, Camera depthCamera)
        {
            var currentShadowDistance = QualitySettings.shadowDistance;
            QualitySettings.shadowDistance = 0;

            var terrains   = Terrain.activeTerrains;
            var pixelError = new float[terrains.Length];
            for (var i = 0; i < terrains.Length; i++)
            {
                pixelError[i]                   = terrains[i].heightmapPixelError;
                terrains[i].heightmapPixelError = 1;
            }

            depthCamera.targetTexture = passData.DepthRT;
            depthCamera.Render();
            //UniversalRenderPipeline.RenderSingleCamera(_context, depthCamera);
            // Debug.Log("render ortho depth");
            for (var i = 0; i < terrains.Length; i++)
            {
                terrains[i].heightmapPixelError = terrains[i].heightmapPixelError;
            }

            QualitySettings.shadowDistance = currentShadowDistance;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (!IsInitialized) Initialize();
        }


        public override void Release()
        {
            if (_pass != null)
            {
                _pass.OnRender -= OnRender;
                WaterInstance.OnUpdate -= OnUpdate;
                _pass.Release();
            }

            IsInitialized = false;
        }
    }
}