using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace KWS
{
    public class DrawToPosteffectsDepthPass : WaterPass
    {
        DrawToPosteffectsDepthPassCore _pass;


#if UNITY_2022_1_OR_NEWER
        private RTHandle _depthRT;
#else
        RenderTargetIdentifier _depthRT = new RenderTargetIdentifier(Shader.PropertyToID("_CameraDepthTexture"));
#endif


        public DrawToPosteffectsDepthPass(RenderPassEvent renderPassEvent, WaterSystem waterInstance)
        {
            WaterInstance = waterInstance;
            this.renderPassEvent = renderPassEvent;
        }

        void Initialize()
        {
            IsInitialized = true;
            _pass                   =  new DrawToPosteffectsDepthPassCore(WaterInstance);
            _pass.OnSetRenderTarget += OnSetRenderTarget;
        }
    
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (!IsInitialized) Initialize();
            var cam = renderingData.cameraData.camera;
            var cmd = CommandBufferPool.Get(_pass.PassName);
            cmd.Clear();

#if UNITY_2022_1_OR_NEWER
            _depthRT = renderingData.cameraData.renderer.cameraDepthTargetHandle;
#endif

            _pass.Execute(cam, cmd, _depthRT);

            context.ExecuteCommandBuffer(cmd); 
            CommandBufferPool.Release(cmd);
        }

        private void OnSetRenderTarget(CommandBuffer cmd, Camera cam)
        {
            ConfigureTarget(_depthRT);
            CoreUtils.SetRenderTarget(cmd, _depthRT);
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
