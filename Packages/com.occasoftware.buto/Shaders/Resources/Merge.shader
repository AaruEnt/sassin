Shader "OccaSoftware/Buto/Merge"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}
        
        ZWrite Off
        Cull Off
        ZTest Always
        
        Pass
        {
            Name "Merge"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Fragment
            
            #include "Common.hlsl"
            
            Texture2D _ScreenTexture;
            Texture2D _ButoTexture;
            
            float3 Fragment (Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                
                float4 fogColor = _ButoTexture.SampleLevel(point_clamp_sampler, input.uv, 0);
                float3 screenColor = _ScreenTexture.SampleLevel(point_clamp_sampler, input.uv, 0).rgb;
                float3 output = (screenColor * fogColor.a) + fogColor.rgb;
                return output;
            }
            ENDHLSL
        }
    }
}