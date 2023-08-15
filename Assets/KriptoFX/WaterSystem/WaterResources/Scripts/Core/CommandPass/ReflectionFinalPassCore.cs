using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using static KWS.KWS_CoreUtils;

namespace KWS
{
    public class ReflectionFinalPassCore : WaterPassCore
    {
        public Action<CommandBuffer, Camera, RTHandle> OnInitializedRenderTarget;
        RTHandle _planarFinalRT;
        private Material _anisoMaterial;

        public ReflectionFinalPassCore(WaterSystem waterInstance)
        {
            PassName = "Water.ReflectionFinalPass";
            WaterInstance = waterInstance;
            WaterSystem.OnWaterSettingsChanged += OnWaterSettingsChanged;
            OnWaterSettingsChanged(waterInstance, WaterSystem.WaterTab.Reflection);
        }

        public override void Release()
        {
            WaterSystem.OnWaterSettingsChanged -= OnWaterSettingsChanged;

            KW_Extensions.SafeDestroy(_planarFinalRT, _anisoMaterial);
            //KW_Extensions.WaterLog(this, "Release", KW_Extensions.WaterLogMessageType.Release);
        }

        private void OnWaterSettingsChanged(WaterSystem waterInstance, WaterSystem.WaterTab changedTab)
        {
            if (!changedTab.HasFlag(WaterSystem.WaterTab.Reflection)) return;

            var sourceRT = waterInstance.SharedData.PlanarReflectionRaw;
            if (sourceRT == null || sourceRT.width <= 1) return;
            ReinitializeTextures(sourceRT.width, sourceRT.graphicsFormat);
        }


        void ReinitializeTextures(int size, GraphicsFormat graphicsFormat)
        {
            if(_planarFinalRT != null) _planarFinalRT.Release();

            _planarFinalRT = WaterSystem.RTHandles.Alloc(size, size, colorFormat: graphicsFormat, name: "_planarFilteredRT", useMipMap: true, autoGenerateMips: true);
            WaterInstance.SharedData.PlanarReflectionFinal = _planarFinalRT;
            //KW_Extensions.WaterLog(this, _planarFinalRT);
        }

        public void Execute(Camera cam, CommandBuffer cmd)
        {
            if (!WaterInstance.Settings.UsePlanarReflection) return;
            if (!WaterInstance.Settings.EnabledMeshRendering) return;
            if (!CanBeRenderCurrentWaterInstance(WaterInstance)) return;

            var sourceRT = WaterInstance.SharedData.PlanarReflectionRaw;
            if (sourceRT == null || sourceRT.width <= 1) return;

            if (_planarFinalRT == null) ReinitializeTextures(sourceRT.width, sourceRT.graphicsFormat);
            OnInitializedRenderTarget?.Invoke(cmd, cam, _planarFinalRT);

            if (WaterInstance.Settings.UseAnisotropicReflections)
            {
                if (_anisoMaterial == null) 
                {
                    _anisoMaterial = KWS_CoreUtils.CreateMaterial(KWS_ShaderConstants.ShaderNames.ReflectionFiltering);
                    WaterInstance.AddShaderToWaterRendering(_anisoMaterial);
                }
                cmd.BlitTriangle(sourceRT, Vector4.one, _planarFinalRT, _anisoMaterial, WaterInstance.Settings.AnisotropicReflectionsHighQuality ? 1 : 0);
            }
            else
            {
                cmd.Blit(sourceRT, _planarFinalRT);
            }

        }


    }
}