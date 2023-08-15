using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace KWS
{
    public class ShorelineFoamPass : WaterPass
    {
        ShorelineFoamPassCore _pass;

        public ShorelineFoamPass(RenderPassEvent renderPassEvent, WaterSystem waterInstance)
        {
            WaterInstance = waterInstance;
            this.renderPassEvent = renderPassEvent;
        }
        void Initialize()
        {
            IsInitialized = true;
            _pass                   =  new ShorelineFoamPassCore(WaterInstance);
            _pass.OnSetRenderTarget += OnSetRenderTarget;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (!IsInitialized) Initialize();
            var cam = renderingData.cameraData.camera;
            var cmd = CommandBufferPool.Get(_pass.PassName);
            cmd.Clear();

#if UNITY_2022_1_OR_NEWER
             var depthColor = renderingData.cameraData.renderer.cameraDepthTargetHandle.rt;
             var cameraColor = renderingData.cameraData.renderer.cameraColorTargetHandle.rt;
#else
            var depthColor  = renderingData.cameraData.renderer.cameraDepthTarget;
            var cameraColor = renderingData.cameraData.renderer.cameraColorTarget;
#endif
            _pass.Execute(cam, cmd, cameraColor, depthColor);
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        private void OnSetRenderTarget(CommandBuffer cmd, Camera cam, RTHandle rt)
        {
            ConfigureTarget(rt);
            CoreUtils.SetRenderTarget(cmd, rt);
            CoreUtils.ClearRenderTarget(cmd, ClearFlag.Color, Color.clear);
        }

        public override void Release()
        {
            if (_pass != null)
            {
                _pass.OnSetRenderTarget -= OnSetRenderTarget;
                _pass.Release();
            }
            IsInitialized = false;
        }
    }
}