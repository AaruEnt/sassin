using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace KWS
{
    public class MaskDepthNormalPass : WaterPass
    {
        MaskDepthNormalPassCore _pass;
        RenderTargetIdentifier[] _mrt = new RenderTargetIdentifier[2];
        RTHandle[] _mrtHandle = new RTHandle[2];

        public MaskDepthNormalPass(RenderPassEvent renderPassEvent, WaterSystem waterInstance)
        {
            WaterInstance = waterInstance;
            this.renderPassEvent = renderPassEvent;
        }

        public void Initialize()
        {
            IsInitialized = true;

            _pass = new MaskDepthNormalPassCore(WaterInstance);
            _pass.OnInitializedRenderTarget += OnInitializedRenderTarget;
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
        private void OnInitializedRenderTarget(CommandBuffer cmd, RTHandle rt1, RTHandle rt2, RTHandle rt3)
        {
            _mrt[0] = rt1;
            _mrt[1] = rt2;

#if UNITY_2022_1_OR_NEWER
            _mrtHandle[0] = rt1;
            _mrtHandle[1] = rt2;
            ConfigureTarget(_mrtHandle, rt3); //by some reason, configure target/clear cause flickering in the editor view
#else
            ConfigureTarget(_mrt, rt3.rt); //by some reason, configure target/clear cause flickering in the editor view
#endif
            CoreUtils.SetRenderTarget(cmd, _mrt, rt3, ClearFlag.All, Color.black);
        }

        public override void Release()
        {
            if (_pass != null)
            {
                _pass.OnInitializedRenderTarget -= OnInitializedRenderTarget;
                _pass.Release();
            }
            IsInitialized = false;
        }
    }
}