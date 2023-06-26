Shader "OccaSoftware/Buto/DepthAwareUpscale"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}
        
        ZWrite Off
        Cull Off
        ZTest Always
        
        Pass
        {
            Name "DepthAwareUpscalePass"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Fragment
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Common.hlsl"
            

            TEXTURE2D_X(_ButoDepth);
			SAMPLER(sampler_ButoDepth);
			
			TEXTURE2D_X(_ButoUpscaleInputTex);
			SAMPLER(sampler_ButoUpscaleInputTex);
			
            float4 Fragment (Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                
                // Setup
				half2 texelSize = rcp(_ScreenParams.xy * 0.5);
				
                half2 texcoords[4] = {
					half2(0, 0),
					half2(texelSize.x, texelSize.y),
					half2(0, texelSize.y),
					half2(texelSize.x, 0)
				};
				
				// Set up the distances array
				half distances[4];
				float depthRaw = SampleSceneDepth(input.uv);
                float depth = GetDepth01(depthRaw);
                
				for (int a = 0; a <= 3; a++)
				{
					float s = SAMPLE_TEXTURE2D_X(_ButoDepth, sampler_ButoDepth, input.uv + texcoords[a]).r;
                    s = GetDepth01(s);
					distances[a] = abs(depth - s);
				}
                
				
				// Evaluate for the Minimum Distance diff. Then sample that UV.
				int minDistId = 0;
				float minDist = distances[0];
				
				for(int c = 1; c <= 3; c++)
				{
					if(distances[c] < minDist)
					{
						minDist = distances[c];
						minDistId = c;
					}
				}
				
				
				// Calculate the nearest UV Coordinate and set the color.
				float2 nearestUV = input.uv + texcoords[minDistId];
                float4 color = SAMPLE_TEXTURE2D_X(_ButoUpscaleInputTex, point_clamp_sampler, nearestUV);
				
				return color;
            }
            ENDHLSL
        }
    }
}