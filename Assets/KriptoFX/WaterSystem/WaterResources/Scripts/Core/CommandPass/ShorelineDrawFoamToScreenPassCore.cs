using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace KWS
{
    public class ShorelineDrawFoamToScreenPassCore : WaterPassCore
    {
        public Action<CommandBuffer, Camera, RTHandle> OnSetRenderTarget;

        private Material _finalFoamPassMaterial;


        public ShorelineDrawFoamToScreenPassCore(WaterSystem waterInstance)
        {
            PassName = "Water.ShorelineDrawFoamToScreenPass";
            WaterInstance = waterInstance;
        }

        public override void Release()
        {
            KW_Extensions.SafeDestroy(_finalFoamPassMaterial);
           // KW_Extensions.WaterLog(this, "Release", KW_Extensions.WaterLogMessageType.Release);
        }

        public void Execute(Camera cam, CommandBuffer cmd, RenderTargetIdentifier colorRT)
        {
            if (!WaterInstance.Settings.UseShorelineRendering) return;
            if (!KWS_CoreUtils.CanBeRenderCurrentWaterInstance(WaterInstance)) return;

            var buffer = WaterInstance.SharedData.ShorelineWaveBuffers;
            if (buffer == null || buffer.FoamWavesComputeBuffer == null || buffer.VisibleFoamWaves.Count == 0) return;

            var targetRT = WaterInstance.SharedData.FoamRawRT;
            if (targetRT == null) return;
           
            if (_finalFoamPassMaterial == null) _finalFoamPassMaterial = KWS_CoreUtils.CreateMaterial(KWS_ShaderConstants.ShaderNames.ShorelineFoamDrawToScreenName);

            var rt = targetRT.rt;
            _finalFoamPassMaterial.SetVector(Shader.PropertyToID("_FoamRTSize"), new Vector4(rt.width, rt.height, 1.0f / rt.width, 1.0f / rt.height));
            _finalFoamPassMaterial.SetVector(Shader.PropertyToID("KWS_ShorelineColor"), WaterInstance.Settings.ShorelineColor);
            _finalFoamPassMaterial.SetKeyword(KWS_ShaderConstants.ShorelineKeywords.KWS_FOAM_USE_FAST_PATH, WaterInstance.Settings.UseShorelineFoamFastMode);

            var targetViewPortSize = KWS_CoreUtils.GetCameraRTHandleViewPortSize(cam);
            OnSetRenderTarget?.Invoke(cmd, cam, targetRT);
            cmd.BlitTriangle(targetRT, Vector2.one, colorRT, targetViewPortSize, _finalFoamPassMaterial);
        }
    }
}