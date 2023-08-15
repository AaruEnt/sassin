Shader "Hidden/KriptoFX/KWS/VolumetricLighting"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" { }
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			HLSLPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#pragma target 4.6

			#pragma multi_compile _ USE_CAUSTIC
			#pragma multi_compile _ USE_LOD1 USE_LOD2 USE_LOD3

			#pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
			#pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
			#pragma multi_compile _ _SHADOWS_SOFT
			#pragma multi_compile _ _ADDITIONAL_LIGHTS
			#pragma multi_compile _ KW_POINT_SHADOWS_SUPPORTED
			
			#include "../PlatformSpecific/Includes/KWS_HelpersIncludes.cginc"
			#include "../Common/CommandPass/KWS_VolumetricLight_Common.cginc"


			half4 RayMarchDirLight(RaymarchData raymarchData, uint rayMarchSteps, half isUnderwater)
			{
				half4 result = 0;

				Light mainLight = GetMainLight();
				float3 currentPos = raymarchData.currentPos;

				[loop]
				for (uint i = 0; i < KWS_RayMarchSteps; ++i)
				{
					float atten = MainLightRealtimeShadow(TransformWorldToShadowCoord(currentPos));
					float3 scattering = raymarchData.stepSize;
					#if defined(USE_CAUSTIC)
						float underwaterStrength = lerp(saturate((KW_Transparent - 1) / 5) * 0.5, 1, isUnderwater);
						scattering += scattering * RaymarchCaustic(raymarchData.rayStart, currentPos, mainLight.direction) * underwaterStrength;

					#endif
					float3 light = atten * scattering * mainLight.color;
					result.rgb += light;
					currentPos += raymarchData.step;
				}
				float cosAngle = dot(mainLight.direction.xyz, -raymarchData.rayDir);
				result.rgb *= MieScattering(cosAngle);
				if(!isUnderwater) result.a = MainLightRealtimeShadow(TransformWorldToShadowCoord(raymarchData.rayStart));
				
				return result;
			}

			half4 RayMarchAdditionalLights(RaymarchData raymarchData, uint rayMarchSteps)
			{
				half4 result = 0;

				#ifdef _ADDITIONAL_LIGHTS
					//  uint pixelLightCount = GetAdditionalLightsCount(); //bug, unity does not update light count after removal
					uint pixelLightCount = _AdditionalLightsCount.x;
					//uint pixelLightCount = KW_lightsCount;
					for (uint lightIndex = 0u; lightIndex < pixelLightCount; ++lightIndex)
					{
						float3 currentPos = raymarchData.currentPos;
						[loop]
						for (uint i = 0; i < KWS_RayMarchSteps; ++i)
						{

							Light addLight = GetAdditionalPerObjectLight(lightIndex, currentPos);
							#if KW_POINT_SHADOWS_SUPPORTED
								float atten = AdditionalLightRealtimeShadow(lightIndex, currentPos, addLight.direction);
							#else
								float atten = AdditionalLightRealtimeShadow(lightIndex, currentPos);
							#endif

							float3 scattering = raymarchData.stepSize * addLight.color.rgb * 5;
							float3 light = atten * scattering * addLight.distanceAttenuation;

							float cosAngle = dot(-raymarchData.rayDir, normalize(currentPos - addLight.direction.xyz));
							light *= MieScattering(cosAngle);

							result.rgb += light;
							currentPos += raymarchData.step;
						}
					}
				#endif

				return result;
			}

			inline float4 RayMarch(RaymarchData raymarchData, half isUnderwater)
			{

				float4 result = 0;
				
				float extinction = 0;
				
				result += RayMarchDirLight(raymarchData, KWS_RayMarchSteps, isUnderwater);
				result += RayMarchAdditionalLights(raymarchData, KWS_RayMarchSteps);

				result.rgb /= KW_Transparent;
				result.rgb *= KWS_VolumeDepthFade;
				//result.rgb *= 4;
				result.rgb  = max(MIN_THRESHOLD * 2, result.rgb);
				return result;
			}

			half4 frag(vertexOutput i) : SV_Target
			{
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
				half mask = GetWaterMask(i.uv);
				
				UNITY_BRANCH
				if (EarlyDiscardUnderwaterPixels(mask)) return 0;

				float depthTop = GetWaterDepth(i.uv);
				float depthBot = GetSceneDepth(i.uv);

				//UNITY_BRANCH
				//if (EarlyDiscardDepthOcclusionPixels(depthBot, depthTop, mask)) return 0; //todo blur pass wil have black color leaking in this case. Maybe I need to add the same method for blur pass?

				bool isUnderwater = IsUnderwaterMask(mask);
				RaymarchData raymarchData = InitRaymarchData(i, depthTop, depthBot, isUnderwater);
				half4 finalColor = RayMarch(raymarchData, isUnderwater);
				
				return finalColor;
			}

			ENDHLSL
		}
	}
}