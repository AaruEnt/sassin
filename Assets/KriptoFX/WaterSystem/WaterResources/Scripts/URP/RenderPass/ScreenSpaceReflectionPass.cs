using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace KWS
{
    public class ScreenSpaceReflectionPass: WaterPass
    {
        ScreenSpaceReflectionPassCore _pass;

       
        readonly RenderTargetIdentifier _cameraDepthTextureRT = new RenderTargetIdentifier(Shader.PropertyToID("_CameraDepthTexture"));
       
        public ScreenSpaceReflectionPass(RenderPassEvent renderPassEvent, WaterSystem waterInstance)
        {
            WaterInstance = waterInstance;
            this.renderPassEvent = renderPassEvent;
        }

        void Initialize()
        {
            IsInitialized = true;
            _pass                   =  new ScreenSpaceReflectionPassCore(WaterInstance);
            _pass.OnSetRenderTarget += OnSetRenderTarget;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            ConfigureInput(ScriptableRenderPassInput.Depth);
        }


        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (!IsInitialized) Initialize();
            var cam = renderingData.cameraData.camera;
            var cmd = CommandBufferPool.Get(_pass.PassName);
            cmd.Clear();

#if UNITY_2022_1_OR_NEWER
             var cameraColor = renderingData.cameraData.renderer.cameraColorTargetHandle.rt;
             var cameraDepth = renderingData.cameraData.renderer.cameraDepthTargetHandle.rt;
#else
            var cameraColor = renderingData.cameraData.renderer.cameraColorTarget;
            var cameraDepth = _cameraDepthTextureRT; //  //renderingData.cameraData.renderer.cameraDepthTarget doesn't work with editor camera, this bug fixed in unity 2022
#endif
            _pass.Execute(cam, cmd, cameraDepth, cameraColor);
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        private void OnSetRenderTarget(CommandBuffer cmd, Camera cam, RTHandle rt)
        {
            ConfigureTarget(rt);
            CoreUtils.SetRenderTarget(cmd, rt, ClearFlag.Color, Color.clear);
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