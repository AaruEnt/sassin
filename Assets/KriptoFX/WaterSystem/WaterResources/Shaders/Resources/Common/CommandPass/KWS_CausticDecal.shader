Shader "Hidden/KriptoFX/KWS/CausticDecal"
{
	Subshader
	{
		ZWrite Off
		Cull Front

		ZTest Always
		Blend DstColor Zero
		//Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			HLSLPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#pragma target 4.6

			#pragma multi_compile _ USE_DISPERSION
			#pragma multi_compile _ USE_LOD1 USE_LOD2 USE_LOD3
			#pragma multi_compile _ KW_DYNAMIC_WAVES
			#pragma multi_compile _ USE_DEPTH_SCALE
			#pragma multi_compile_fog

			#include "../../PlatformSpecific/Includes/KWS_HelpersIncludes.cginc"
			

			Texture2D KW_CausticDepthTex;

			float4 KW_CausticLod0_TexelSize;
			float4 KW_CausticLod1_TexelSize;

			float KW_CausticDispersionStrength;

			float KW_CausticDepthOrthoSize;
			float3 KW_CausticDepth_Near_Far_Dist;
			float3 KW_CausticDepthPos;

			half3 GetCausticLod(float2 decalUV, float lodDist, Texture2D tex, half3 lastLodCausticColor)
			{
				float2 uv = lodDist * decalUV + 0.5 - KW_CausticLodOffset.xz;
				float caustic = tex.Sample(sampler_linear_repeat, uv, 0).x;
				uv = 1 - min(1, abs(uv * 2 - 1));
				float lerpLod = uv.x * uv.y;
				lerpLod = min(1, lerpLod * 3);
				return lerp(lastLodCausticColor, caustic, lerpLod);
			}

			half3 GetCausticLodWithDynamicWaves(float2 decalUV, float lodDist, Texture2D tex, half3 lastLodCausticColor, float2 offsetUV1, float2 offsetUV2, float flowLerpMask)
			{
				float2 uv = lodDist * decalUV + 0.5 - KW_CausticLodOffset.xz;
				float caustic1 = tex.Sample(sampler_linear_repeat, uv - offsetUV1).x;
				float caustic2 = tex.Sample(sampler_linear_repeat, uv - offsetUV2).x;
				float caustic = lerp(caustic1, caustic2, flowLerpMask);
				uv = 1 - min(1, abs(uv * 2 - 1));
				float lerpLod = uv.x * uv.y;
				lerpLod = min(1, lerpLod * 3);
				return lerp(lastLodCausticColor, caustic, lerpLod);
			}

			half3 GetCausticLodWithDispersion(float2 decalUV, float lodDist, Texture2D tex, half3 lastLodCausticColor, float texelSize, float dispersionStr)
			{
				float2 uv = lodDist * decalUV + 0.5 - KW_CausticLodOffset.xz;
				float3 caustic;
				caustic.r = tex.Sample(sampler_linear_repeat, uv).x;
				caustic.g = tex.Sample(sampler_linear_repeat, uv + texelSize * dispersionStr * 2).x;
				caustic.b = tex.Sample(sampler_linear_repeat, uv + texelSize * dispersionStr * 4).x;

				uv = 1 - min(1, abs(uv * 2 - 1));
				float lerpLod = uv.x * uv.y;
				lerpLod = min(1, lerpLod * 3);
				return lerp(lastLodCausticColor, caustic, lerpLod);
			}

			float ComputeCausticOrthoDepth(float3 worldPos)
			{
				float2 depthUV = (worldPos.xz - KW_CausticDepthPos.xz - KW_WaterPosition.xz * 0) / KW_CausticDepthOrthoSize + 0.5;
				float terrainDepth = KW_CausticDepthTex.SampleLevel(sampler_linear_clamp, depthUV, 0).r * KW_CausticDepth_Near_Far_Dist.z - KW_CausticDepth_Near_Far_Dist.y;
				return terrainDepth;
			}

			half GetTerrainDepth(float3 worldPos)
			{
				#if USE_DEPTH_SCALE
					half terrainDepth = ComputeCausticOrthoDepth(worldPos);
				#else
					half terrainDepth = 1;

				#endif

				half depthTransparent = max(1, KW_Transparent * 2);
				terrainDepth = clamp(-terrainDepth, 0, depthTransparent) / (depthTransparent);
				return terrainDepth;
			}

			half3 GetCaustic(float3 worldPos, float3 localPos)
			{
				half3 caustic = 0.1;

				#if KW_DYNAMIC_WAVES
					float2 dynamicWavesUV = (worldPos.xz - KW_DynamicWavesWorldPos.xz) / KW_DynamicWavesAreaSize + 0.5;
					half2 dynamicWavesNormals = KW_DynamicWavesNormal.SampleLevel(sampler_linear_clamp, dynamicWavesUV, 0) * 2 - 1;


					half time1 = frac(_Time.x + 0.5);
					half time2 = frac(_Time.x);
					half flowLerpMask = abs((0.5 - time1) / 0.5);

					float2 uvOffset1 = 0.25 * dynamicWavesNormals * time1;
					float2 uvOffset2 = 0.25 * dynamicWavesNormals * time2;

					#if defined(USE_LOD3)
						caustic = GetCausticLodWithDynamicWaves(localPos.xz, KW_DecalScale / KW_CausticLodSettings.w, KW_CausticLod3, caustic, uvOffset1, uvOffset2, flowLerpMask);
					#endif
					#if defined(USE_LOD2) || defined(USE_LOD3)
						caustic = GetCausticLodWithDynamicWaves(localPos.xz, KW_DecalScale / KW_CausticLodSettings.z, KW_CausticLod2, caustic, uvOffset1, uvOffset2, flowLerpMask);
					#endif


					#if defined(USE_LOD1) || defined(USE_LOD2) || defined(USE_LOD3)
						caustic = GetCausticLodWithDynamicWaves(localPos.xz, KW_DecalScale / KW_CausticLodSettings.y, KW_CausticLod1, caustic, uvOffset1, uvOffset2, flowLerpMask);
					#endif

					caustic = GetCausticLodWithDynamicWaves(localPos.xz, KW_DecalScale / KW_CausticLodSettings.x, KW_CausticLod0, caustic, uvOffset1, uvOffset2, flowLerpMask);

					half dynamicWaves = KW_DynamicWaves.SampleLevel(sampler_linear_clamp, dynamicWavesUV, 0);
					caustic += clamp(dynamicWaves, -0.025, 1);
				#else


					#if defined(USE_LOD3)
						caustic = GetCausticLod(localPos.xz, KW_DecalScale / KW_CausticLodSettings.w, KW_CausticLod3, caustic);
					#endif
					#if defined(USE_LOD2) || defined(USE_LOD3)
						caustic = GetCausticLod(localPos.xz, KW_DecalScale / KW_CausticLodSettings.z, KW_CausticLod2, caustic);
					#endif

					#if USE_DISPERSION
						#if defined(USE_LOD1) || defined(USE_LOD2) || defined(USE_LOD3)
							caustic = GetCausticLodWithDispersion(localPos.xz, KW_DecalScale / KW_CausticLodSettings.y, KW_CausticLod1, caustic, KW_CausticLod0_TexelSize.x, KW_CausticDispersionStrength);
						#endif
						caustic = GetCausticLodWithDispersion(localPos.xz, KW_DecalScale / KW_CausticLodSettings.x, KW_CausticLod0, caustic, KW_CausticLod0_TexelSize.x, KW_CausticDispersionStrength);

					#else
						#if defined(USE_LOD1) || defined(USE_LOD2) || defined(USE_LOD3)
							caustic = GetCausticLod(localPos.xz, KW_DecalScale / KW_CausticLodSettings.y, KW_CausticLod1, caustic);
						#endif
						caustic = GetCausticLod(localPos.xz, KW_DecalScale / KW_CausticLodSettings.x, KW_CausticLod0, caustic);
					#endif
				#endif
				return caustic;
			}

			half3 ApplyCausticFade(half3 caustic, half3 worldPos, float terrainDepth)
			{
				caustic = lerp(half3(1, 1, 1), caustic * 10, saturate(KW_CaustisStrength));
				caustic += caustic * caustic * caustic * saturate(KW_CaustisStrength - 1);
				float dist = length(worldPos - _WorldSpaceCameraPos);
				float distFade = 1 - saturate(dist / KW_DecalScale * 2);
				caustic = lerp(half3(1, 1, 1), caustic, distFade);

				float fade = saturate((KW_WaterPosition.y - worldPos.y) * 2);
				caustic = lerp(half3(1, 1, 1), caustic, fade);
				return lerp(caustic, half3(1, 1, 1), terrainDepth);
			}

			half4 GetCaustic(float2 uv, out float depth, out float3 worldPos)
			{
				//float waterDepth = GetWaterDepth(uv);
				//bool isUnderwater = GetWaterMask(uv).x > WaterMask_UnderwaterThreshold;
				//bool causticMask = isUnderwater ?   waterDepth < depth : waterDepth > depth;

				//if (causticMask < 0.0001) discard;

				if (GetWaterID(uv) - 1 != KWS_WaterPassID) discard;
				//if (GetWaterID(uv) - 1 == (int)Test4.x) discard;
				//if (KWS_WaterID == (int)Test4.y) discard;
				
				depth = GetSceneDepth(uv);
				worldPos = GetWorldSpacePositionFromDepth(uv, depth);
				float3 localPos = WorldToLocalPos(worldPos);

				half terrainDepth = GetTerrainDepth(worldPos);
				half3 caustic = GetCaustic(worldPos, localPos);
				caustic = ApplyCausticFade(caustic, worldPos, terrainDepth);

				return float4(caustic, 1);
			}

			struct vertexInput
			{
				float4 vertex : POSITION;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct vertexOutput
			{
				float4 vertex : SV_POSITION;
				float4 screenUV : TEXCOORD0;
				UNITY_VERTEX_OUTPUT_STEREO
			};


			vertexOutput vert(vertexInput v)
			{
				vertexOutput o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				o.vertex = ObjectToClipPos(v.vertex);
				o.screenUV = ComputeScreenPos(o.vertex);
				return o;
			}

			half4 frag(vertexOutput i) : SV_Target
			{
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
				float2 screenUV = GetNormalizedRTHandleUV(i.screenUV.xy / i.screenUV.w);
				float depth;
				float3 worldPos;
				half4 caustic = GetCaustic(screenUV, depth, worldPos);

				half3 fogColor;
				half3 fogOpacity;
				
				GetInternalFogVariables(i.vertex, 0, 0, LinearEyeDepth(depth), fogColor, fogOpacity);
				
				caustic.rgb = lerp(caustic.rgb, 1, fogOpacity);
				return caustic;
			}

			ENDHLSL
		}
	}
}