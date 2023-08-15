using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace KWS
{
    public class VolumetricLightingPass: WaterPass
    {
        VolumetricLightingPassCore _pass;

        public VolumetricLightingPass(RenderPassEvent renderPassEvent, WaterSystem waterInstance)
        {
            WaterInstance = waterInstance;
            this.renderPassEvent = renderPassEvent;
        }
        void Initialize()
        {
            IsInitialized = true;
            _pass                   =  new VolumetricLightingPassCore(WaterInstance);
            _pass.OnSetRenderTarget += OnSetRenderTarget;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (!IsInitialized) Initialize();
            var cam = renderingData.cameraData.camera;
            var cmd = CommandBufferPool.Get(_pass.PassName);
            cmd.Clear();
            _pass.Execute(cam, cmd);
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        private void OnSetRenderTarget(CommandBuffer cmd, Camera cam, RTHandle rt)
        {
            ConfigureTarget(rt);
            CoreUtils.SetRenderTarget(cmd, rt);
        }


        public override void Release()
        {
            if (_pass != null)
            {
                _pass.Release();
                _pass.OnSetRenderTarget += OnSetRenderTarget;
            }
            IsInitialized = false;
        }
    }
}