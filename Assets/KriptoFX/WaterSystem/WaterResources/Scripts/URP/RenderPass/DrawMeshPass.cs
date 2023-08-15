using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace KWS
{
    public class DrawMeshPass : WaterPass
    {
        DrawMeshPassCore _pass;

        public DrawMeshPass(RenderPassEvent renderPassEvent, WaterSystem waterInstance)
        {
            WaterInstance = waterInstance;
            this.renderPassEvent = renderPassEvent;
        }

        void Initialize()
        {
            IsInitialized = true;
            _pass = new DrawMeshPassCore(WaterInstance);
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

        public override void Release()
        {
            if (_pass != null) _pass.Release();

            IsInitialized = false;
        }
    }
}