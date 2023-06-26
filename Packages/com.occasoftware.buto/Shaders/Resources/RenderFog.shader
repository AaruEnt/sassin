Shader "OccaSoftware/Buto/RenderFog"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}
        
        ZWrite Off
        Cull Off
        ZTest Always
        
        Pass
        {
            Name "RenderFog"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Fragment
            
            #include "Common.hlsl"
            #include "Fog.hlsl"
            
			
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _BUTO_SELF_ATTENUATION_ENABLED
            #pragma multi_compile _ _BUTO_HORIZON_SHADOWING_ENABLED
            #pragma multi_compile _ _BUTO_ANALYTIC_FOG_ENABLED
            #pragma multi_compile _ _LIGHT_COOKIES
            
            float4 Fragment (Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                
                float3 color;
                float alpha;
                SampleVolumetricFog(input.uv, color, alpha);
                
                return float4(color, alpha);
            }
            ENDHLSL
        }
    }
}