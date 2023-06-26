Shader "OccaSoftware/Buto/DownsampleDepthTexture"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}
        
        ZWrite Off 
        Cull Off 
        ZTest Always
        
        Pass
        {
            Name "DownsampleDepthTexturePass"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Fragment
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Common.hlsl"
            
            
            float Fragment (Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                
                // Sample TexCoords UV + 00, 01, 10, and 11. Then record the distance diff.
	            half2 texelSize = rcp(_ScreenParams.xy);
	            half2 texcoords[4] = {
		            half2(0, 0),
		            half2(texelSize.x, texelSize.y),
		            half2(0, texelSize.y),
		            half2(texelSize.x, 0)
	            };
	            
                
	            // Sample the four pixels that will be downsampled.
                half distances[4];
	            for (int a = 0; a <= 3; a++)
	            {
		            half depthRaw = SampleSceneDepth(input.uv + texcoords[a]);
                    distances[a] = CheckDepthDirection(depthRaw);
	            }
                
                
	            // Checkerboard the block.
	            float depthToWrite = distances[0];
                int pixelX = int(input.uv.x * _ScreenParams.x * 0.5);
                int pixelY = int(input.uv.y * _ScreenParams.y * 0.5);
                int checkerboard = (pixelX ^ pixelY) & 1;
	            for(int c = 1; c <= 3; c++)
	            {
                    depthToWrite = lerp(min(depthToWrite,distances[c]), max(depthToWrite, distances[c]), checkerboard);
	            }
                
                return CheckDepthDirection(depthToWrite);
            }
            ENDHLSL
        }
    }
}