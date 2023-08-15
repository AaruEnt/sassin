using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace KWS
{
    public class ReflectionFinalPass : WaterPass
    {
        ReflectionFinalPassCore _pass;

        public ReflectionFinalPass(RenderPassEvent renderPassEvent, WaterSystem waterInstance)
        {
            WaterInstance = waterInstance;
            this.renderPassEvent = renderPassEvent;
        }

        void Initialize()
        {
            IsInitialized = true; 
            _pass     =  new ReflectionFinalPassCore(WaterInstance);
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

        private void OnInitializedRenderTarget(CommandBuffer cmd, Camera cam, RTHandle rt)
        {
            ConfigureTarget(rt);
            CoreUtils.SetRenderTarget(cmd, rt, ClearFlag.Color, Color.black);
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