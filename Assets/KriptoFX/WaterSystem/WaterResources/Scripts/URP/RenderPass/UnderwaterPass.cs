using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace KWS
{
    public class UnderwaterPass: WaterPass
    {
        UnderwaterPassCore _pass;
#if UNITY_2022_1_OR_NEWER
        private RTHandle _cameraColor;
#else
        private RenderTargetIdentifier _cameraColor;
#endif

        public UnderwaterPass(RenderPassEvent renderPassEvent, WaterSystem waterInstance)
        {
            WaterInstance = waterInstance;
            this.renderPassEvent = renderPassEvent;
        }
        void Initialize()
        {
            IsInitialized = true;
            _pass                   =  new UnderwaterPassCore(WaterInstance);
            _pass.OnSetRenderTarget += OnSetRenderTarget;
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
            _pass.Execute(cam, cmd, _cameraColor);
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
      
        private void OnSetRenderTarget(CommandBuffer cmd, RTHandle rt)
        {
            if (rt == null)
            {
                ConfigureTarget(_cameraColor);
                CoreUtils.SetRenderTarget(cmd, _cameraColor);
            }
            else
            {
                ConfigureTarget(rt);
                CoreUtils.SetRenderTarget(cmd, rt);
            }
        }

        public override void Release()
        {
            if (_pass != null)
            {
                _pass.OnSetRenderTarget            -= OnSetRenderTarget;
                _pass.Release();
            }

          
            IsInitialized = false;
        }
    }
}