using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace KWS
{
    public class CausticPass : WaterPass
    {
        CausticPassCore _pass;
#if UNITY_2022_1_OR_NEWER
        private RTHandle _cameraColor;
#else
        private RenderTargetIdentifier _cameraColor;
#endif

        public CausticPass(RenderPassEvent renderPassEvent, WaterSystem waterInstance)
        {
            WaterInstance = waterInstance;
            this.renderPassEvent = renderPassEvent;
        }

        public void Initialize()
        {
            IsInitialized = true;

            _pass                         =  new CausticPassCore(WaterInstance);
            _pass.OnRenderToCausticTarget += OnRenderToCausticTarget;
            _pass.OnRenderToCameraTarget  += OnRenderToCameraTarget;

          
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTargetDescriptor)
        {
            //ConfigureTarget(pass.GetTargetColorBuffer());
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (!IsInitialized) Initialize();
            var cam = renderingData.cameraData.camera;
            var cmd = CommandBufferPool.Get(_pass.PassName);
            cmd.Clear();

#if UNITY_2022_1_OR_NEWER
            _cameraColor = renderingData.cameraData.renderer.cameraColorTargetHandle;
#else
            _cameraColor = renderingData.cameraData.renderer.cameraColorTarget;
#endif
            _pass.Execute(cam, cmd);
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        private void OnRenderToCausticTarget(CommandBuffer cmd, RTHandle rt)
        {
            ConfigureTarget(rt);
            CoreUtils.SetRenderTarget(cmd, rt, ClearFlag.Color, Color.black);
        }

        private void OnRenderToCameraTarget(CommandBuffer cmd)
        {
            ConfigureTarget(_cameraColor);
            CoreUtils.SetRenderTarget(cmd, _cameraColor);
        }

        public override void Release()
        {
            if (_pass != null)
            {
                _pass.OnRenderToCausticTarget -= OnRenderToCausticTarget;
                _pass.OnRenderToCameraTarget  -= OnRenderToCameraTarget;
                _pass.Release();
            }
          
            IsInitialized = false;
        }
    }
}