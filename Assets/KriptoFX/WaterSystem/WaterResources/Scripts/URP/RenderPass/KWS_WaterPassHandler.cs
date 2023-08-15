using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace KWS
{
    [ExecuteAlways]
    public class KWS_WaterPassHandler : MonoBehaviour
    {
        public WaterSystem WaterInstance;

        OrthoDepthPass _orthoDepthPass;
        ShorelineWavesPass _shorelineWavesPass;
        MaskDepthNormalPass _maskDepthNormalPass;
        CausticPass _causticPass;
        ReflectionFinalPass _reflectionFinalPass;
        ScreenSpaceReflectionPass _ssrPass;
        VolumetricLightingPass _volumetricLightingPass;
        DrawMeshPass _drawMeshPass;
        ShorelineFoamPass _shorelineFoamPass;
        ShorelineDrawFoamToScreenPass _shorelineDrawFoamToScreenPass;
        UnderwaterPass _underwaterPass;
        DrawToPosteffectsDepthPass _drawToDepthPass;

        private List<WaterPass> _waterPasses;

        internal static int UpdatedInstancesPerFrame;

        void OnEnable()
        {
            RenderPipelineManager.beginCameraRendering += OnBeforeCameraRendering;

#if UNITY_EDITOR
            var urpAsset = (UniversalRenderPipelineAsset)GraphicsSettings.renderPipelineAsset;
            urpAsset.supportsCameraOpaqueTexture = true;
            urpAsset.supportsCameraDepthTexture = true;
#endif
        }

       
        void OnDisable()
        {
            RenderPipelineManager.beginCameraRendering -= OnBeforeCameraRendering;

            if (_waterPasses != null)
            {
                foreach (var waterPass in _waterPasses) if (waterPass != null) waterPass.Release();}
        }
        void InitializePasses()
        {
            _orthoDepthPass                = new OrthoDepthPass(RenderPassEvent.BeforeRendering, WaterInstance);
            _shorelineWavesPass            = new ShorelineWavesPass(RenderPassEvent.BeforeRenderingSkybox, WaterInstance);
            _maskDepthNormalPass           = new MaskDepthNormalPass(RenderPassEvent.BeforeRenderingSkybox, WaterInstance);
            _causticPass                   = new CausticPass(RenderPassEvent.BeforeRenderingSkybox, WaterInstance);
            _reflectionFinalPass           = new ReflectionFinalPass(RenderPassEvent.BeforeRenderingSkybox, WaterInstance);
            _ssrPass                       = new ScreenSpaceReflectionPass(RenderPassEvent.BeforeRenderingTransparents, WaterInstance);
            _volumetricLightingPass        = new VolumetricLightingPass(RenderPassEvent.BeforeRenderingTransparents, WaterInstance);
            _drawMeshPass                  = new DrawMeshPass(RenderPassEvent.BeforeRenderingTransparents, WaterInstance);
            _shorelineFoamPass             = new ShorelineFoamPass(RenderPassEvent.BeforeRenderingTransparents, WaterInstance);
            _shorelineDrawFoamToScreenPass = new ShorelineDrawFoamToScreenPass(RenderPassEvent.BeforeRenderingTransparents, WaterInstance);
            _underwaterPass                = new UnderwaterPass(RenderPassEvent.BeforeRenderingTransparents, WaterInstance);
            _drawToDepthPass               = new DrawToPosteffectsDepthPass(RenderPassEvent.BeforeRenderingTransparents, WaterInstance);

            if (_waterPasses == null) _waterPasses = new List<WaterPass>()
            {
                _orthoDepthPass, _shorelineWavesPass, _maskDepthNormalPass, _causticPass, _reflectionFinalPass,
                _ssrPass, _volumetricLightingPass, _drawMeshPass, _shorelineFoamPass, _shorelineDrawFoamToScreenPass, _underwaterPass, _drawToDepthPass
            };
        }


        void ExecutePass(WaterPass pass, UniversalAdditionalCameraData data, bool isUsed)
        {
            if (isUsed)data.scriptableRenderer.EnqueuePass(pass);
            else if(pass.IsInitialized) pass.Release();
        }

        private void OnBeforeCameraRendering(ScriptableRenderContext ctx, Camera cam)
        {
            if (!WaterInstance.IsWaterVisible || !KWS_CoreUtils.CanRenderWaterForCurrentCamera(WaterInstance, cam)) return;
            var data = cam.GetComponent<UniversalAdditionalCameraData>();
            if (data == null) return;
           
            data.requiresColorOption = CameraOverrideOption.On;
            data.requiresDepthOption = CameraOverrideOption.On;
            
            if (_waterPasses == null) InitializePasses();

            var cameraSize = KWS_CoreUtils.GetScreenSizeLimited(WaterSystem.IsSinglePassStereoEnabled);
            WaterSystem.RTHandles.SetReferenceSize(cameraSize.x, cameraSize.y);
            var settings = WaterInstance.Settings;
           
            if (WaterSystem.SelectedThirdPartyFogMethod >=0 && WaterInstance.ThirdPartyFogAssetsDescription[WaterSystem.SelectedThirdPartyFogMethod].CustomQueue > 3000)
            {
                _drawMeshPass.renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
                _shorelineDrawFoamToScreenPass.renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
                _underwaterPass.renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
                _drawToDepthPass.renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
            }
            else
            {
                _drawMeshPass.renderPassEvent                  = RenderPassEvent.BeforeRenderingTransparents;
                _shorelineDrawFoamToScreenPass.renderPassEvent = RenderPassEvent.BeforeRenderingTransparents;
                _underwaterPass.renderPassEvent = WaterInstance.Settings.DrawToPosteffectsDepth ? RenderPassEvent.AfterRenderingTransparents : RenderPassEvent.BeforeRenderingTransparents;
                _drawToDepthPass.renderPassEvent = RenderPassEvent.BeforeRenderingTransparents;
            }

           
            ExecutePass(_orthoDepthPass, data, settings.UseShorelineRendering || settings.UseCausticEffect);
            ExecutePass(_shorelineWavesPass, data, settings.UseShorelineRendering);

            if (UpdatedInstancesPerFrame == 0)
            {
                ExecutePass(_maskDepthNormalPass, data, true);
                ExecutePass(_causticPass, data, true);
            }

            
            ExecutePass(_ssrPass, data, settings.UseScreenSpaceReflection);
            ExecutePass(_reflectionFinalPass, data, settings.UsePlanarReflection);
            ExecutePass(_volumetricLightingPass, data, settings.UseVolumetricLight);
            ExecutePass(_shorelineFoamPass, data, settings.UseShorelineRendering);
            ExecutePass(_drawMeshPass, data, true);
            ExecutePass(_shorelineDrawFoamToScreenPass, data, settings.UseShorelineRendering);
            ExecutePass(_underwaterPass, data, settings.UseUnderwaterEffect);
            if (WaterSystem.SelectedThirdPartyFogMethod >= 0 && WaterInstance.ThirdPartyFogAssetsDescription[WaterSystem.SelectedThirdPartyFogMethod].DrawToDepth == false 
             || WaterInstance.IsCameraUnderwater == false) ExecutePass(_drawToDepthPass, data, settings.DrawToPosteffectsDepth);


            UpdatedInstancesPerFrame++;
            if (UpdatedInstancesPerFrame >= WaterSystem.VisibleWaterInstances.Count) UpdatedInstancesPerFrame = 0;
        }
    }
}
