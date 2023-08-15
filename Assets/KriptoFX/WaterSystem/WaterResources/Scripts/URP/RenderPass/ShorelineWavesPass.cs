using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace KWS
{
    public class ShorelineWavesPass : WaterPass
    {
        ShorelineWavesPassCore _pass;
        RenderTargetIdentifier[] _mrt       = new RenderTargetIdentifier[2];
        RTHandle[]               _mrtHandle = new RTHandle[2];

        public ShorelineWavesPass(RenderPassEvent renderPassEvent, WaterSystem waterInstance)
        {
            WaterInstance = waterInstance;
            this.renderPassEvent = renderPassEvent;
        }
        void Initialize()
        {
            IsInitialized = true;
            _pass                   =  new ShorelineWavesPassCore(WaterInstance);
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
       
        private void OnSetRenderTarget(CommandBuffer cmd, Camera cam, RTHandle rt1, RTHandle rt2)
        {
            _mrt[0] = rt1;
            _mrt[1] = rt2;

#if UNITY_2022_1_OR_NEWER
            _mrtHandle[0] = rt1;
            _mrtHandle[1] = rt2;
            ConfigureTarget(_mrtHandle, rt1); //by some reason, configure target/clear cause flickering in the editor view
#else
            ConfigureTarget(_mrt, rt1.rt.depthBuffer); //by some reason, configure target/clear cause flickering in the editor view
#endif
            CoreUtils.SetRenderTarget(cmd, _mrt, rt1.rt.depthBuffer, ClearFlag.Color, Color.black);
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