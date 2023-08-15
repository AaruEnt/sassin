Shader "Hidden/KriptoFX/KWS/BlurBilateral"
{
	SubShader
	{
		Cull Off
		ZWrite Off
		ZTest Always

		HLSLINCLUDE

		#include "../../PlatformSpecific/Includes/KWS_HelpersIncludes.cginc"

		#define BLUR_KERNEL_SIZE 6
		#define BLUR_KERNEL_SIZE_FAST_MODE 3
		#define UPSAMPLE_DEPTH_THRESHOLD 1.5f
		#define GAUSSIAN_BLUR_DEVIATION 2.5
		#define BLUR_DEPTH_FACTOR 0.5
		#define DEPTH_MIN_THRESHOLD 0.01

		DECLARE_TEXTURE(_SourceRT);
		float4 _SourceRTHandleScale;

		DECLARE_TEXTURE(KWS_CameraDepthTextureLowRes);
		float4 KWS_CameraDepthTextureLowRes_TexelSize;
		float4 KWS_CameraDepthTextureLowRes_RTHandleScale;

		struct appdata
		{
			uint vertexID : SV_VertexID;
			UNITY_VERTEX_INPUT_INSTANCE_ID
		};

		struct v2f
		{
			float2 uv : TEXCOORD0;
			float4 vertex : SV_POSITION;
			UNITY_VERTEX_OUTPUT_STEREO
		};

		struct v2fUpsample
		{
			float2 uv : TEXCOORD0;
			float2 uv00 : TEXCOORD1;
			float2 uv01 : TEXCOORD2;
			float2 uv10 : TEXCOORD3;
			float2 uv11 : TEXCOORD4;
			float4 vertex : SV_POSITION;
			UNITY_VERTEX_OUTPUT_STEREO
		};

		v2f vert(appdata v)
		{
			v2f o;
			UNITY_SETUP_INSTANCE_ID(v);
			UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
			o.vertex = GetTriangleVertexPosition(v.vertexID);
			o.uv = GetTriangleUVScaled(v.vertexID);
			return o;
		}
		v2fUpsample vertUpsample(appdata v)
		{
			v2fUpsample o;
			UNITY_SETUP_INSTANCE_ID(v);
			UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
			o.vertex = GetTriangleVertexPosition(v.vertexID);
			o.uv = GetTriangleUVScaled(v.vertexID);

			o.uv00 = o.uv - 0.5 * KWS_CameraDepthTextureLowRes_TexelSize.xy;
			o.uv10 = o.uv00 + float2(KWS_CameraDepthTextureLowRes_TexelSize.x, 0);
			o.uv01 = o.uv00 + float2(0, KWS_CameraDepthTextureLowRes_TexelSize.y);
			o.uv11 = o.uv00 + KWS_CameraDepthTextureLowRes_TexelSize.xy;
			return o;
		}

		float4 DownsampleDepth(v2f input) : SV_Target
		{
			UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
			float4 depth = GetSceneDepthGather(input.uv);
			float minDepth = min(min(depth.x, depth.y), min(depth.z, depth.w));
			float maxDepth = max(max(depth.x, depth.y), max(depth.z, depth.w));
			int2 position = input.vertex.xy % 2;
			int index = position.x + position.y;
			return index == 1 ?     minDepth : maxDepth;
		}

		float4 DownsampleDepthFastMode(v2f input) : SV_Target
		{
			UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
			float4 depth = GetSceneDepthGather(input.uv);
			return min(min(depth.x, depth.y), min(depth.z, depth.w));
		}

		float GaussianWeight(float offset, float deviation)
		{
			float weight = 1.0f / sqrt(2.0f * 3.1415927f * deviation * deviation);
			weight *= exp( - (offset * offset) / (2.0f * deviation * deviation));
			return weight;
		}


		float4 BilateralBlur(float2 uv, const int2 direction, const int kernelRadius)
		{
			const float deviation = kernelRadius / GAUSSIAN_BLUR_DEVIATION;
			float2 scaledUV = uv * _SourceRTHandleScale.xy;

			float4 centerColor = SAMPLE_TEXTURE_LOD(_SourceRT, sampler_linear_clamp, scaledUV, 0);
			float3 color = centerColor.xyz;

			UNITY_BRANCH
			if (centerColor.r + centerColor.g + centerColor.b < MIN_THRESHOLD) return centerColor;
			else
			{
				float rawZCenter = SAMPLE_TEXTURE_LOD(KWS_CameraDepthTextureLowRes, sampler_linear_clamp, scaledUV, 0).r;
				float centerDepth = LinearEyeDepth(max(rawZCenter, DEPTH_MIN_THRESHOLD));

				float weightSum = 0;

				float weight = GaussianWeight(0, deviation);
				color *= weight;
				weightSum += weight;
				{
					UNITY_UNROLL
					for (int idx = -kernelRadius; idx < 0; idx += 1)
					{
						float2 offset = (direction * idx);
						float3 sampleColor = SAMPLE_TEXTURE_LOD_OFFSET(_SourceRT, sampler_linear_clamp, scaledUV, 0, offset).xyz;
					
						float rawZ = SAMPLE_TEXTURE_LOD_OFFSET(KWS_CameraDepthTextureLowRes, sampler_linear_clamp, scaledUV, 0, offset).x;
						float depth = LinearEyeDepth(max(rawZ, DEPTH_MIN_THRESHOLD));

						float depthDiff = abs(centerDepth - depth);
						float dFactor = depthDiff * BLUR_DEPTH_FACTOR;
						float w = exp( - (dFactor * dFactor));

						weight = GaussianWeight(idx, deviation) * w;

						color += weight * sampleColor;
						weightSum += weight;
					}
				}
				{
					UNITY_UNROLL
					for (int idx = 1; idx <= kernelRadius; idx += 1)
					{
						float2 offset = (direction * idx);
						float3 sampleColor = SAMPLE_TEXTURE_LOD_OFFSET(_SourceRT, sampler_linear_clamp, scaledUV, 0, offset).xyz;
					
						float rawZ = SAMPLE_TEXTURE_LOD_OFFSET(KWS_CameraDepthTextureLowRes, sampler_linear_clamp, scaledUV, 0, offset).x;
						float depth = LinearEyeDepth(max(rawZ, DEPTH_MIN_THRESHOLD));

						float depthDiff = abs(centerDepth - depth);
						float dFactor = depthDiff * BLUR_DEPTH_FACTOR;
						float w = exp( - (dFactor * dFactor));

						weight = GaussianWeight(idx, deviation) * w;

						color += weight * sampleColor;
						weightSum += weight;
					}
				}
				color /= weightSum;
				return float4(color, centerColor.w);
			}
		}

		inline float4 LinearEyeDepth4(float4 z)
		{
			return 1.0 / (_ZBufferParams.z * z + _ZBufferParams.w);
		}

		float4 BilateralUpsample(v2fUpsample input) : SV_Target
		{
			UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
			const float threshold = UPSAMPLE_DEPTH_THRESHOLD;
			float4 highResDepth = LinearEyeDepth(GetSceneDepth(input.uv));
			float4 lowResDepth;
			
			lowResDepth[0] = SAMPLE_TEXTURE_LOD(KWS_CameraDepthTextureLowRes, sampler_linear_clamp, input.uv00 *  _SourceRTHandleScale.xy, 0).r;
			lowResDepth[1] = SAMPLE_TEXTURE_LOD(KWS_CameraDepthTextureLowRes, sampler_linear_clamp, input.uv10 *  _SourceRTHandleScale.xy, 0).r;
			lowResDepth[2] = SAMPLE_TEXTURE_LOD(KWS_CameraDepthTextureLowRes, sampler_linear_clamp, input.uv01 *  _SourceRTHandleScale.xy, 0).r;
			lowResDepth[3] = SAMPLE_TEXTURE_LOD(KWS_CameraDepthTextureLowRes, sampler_linear_clamp, input.uv11 *  _SourceRTHandleScale.xy, 0).r;

			lowResDepth = LinearEyeDepth4(lowResDepth);

			float4 depthDiff = abs(lowResDepth - highResDepth);

			float accumDiff = dot(depthDiff, float4(1, 1, 1, 1));

			UNITY_BRANCH
			if (accumDiff < threshold)
			{
				return SAMPLE_TEXTURE_LOD(_SourceRT, sampler_linear_clamp, input.uv * _SourceRTHandleScale.xy, 0);
			}

			float minDepthDiff = depthDiff[0];
			float2 nearestUv = input.uv00;

			if (depthDiff[1] < minDepthDiff)
			{
				nearestUv = input.uv10;
				minDepthDiff = depthDiff[1];
			}

			if (depthDiff[2] < minDepthDiff)
			{
				nearestUv = input.uv01;
				minDepthDiff = depthDiff[2];
			}

			if (depthDiff[3] < minDepthDiff)
			{
				nearestUv = input.uv11;
				minDepthDiff = depthDiff[3];
			}

			return SAMPLE_TEXTURE_LOD(_SourceRT, sampler_linear_clamp, nearestUv * _SourceRTHandleScale.xy, 0);
		}

		ENDHLSL

		Pass // pass 0 - downsample depth

		{
			HLSLPROGRAM

			#pragma target 4.6
			#pragma vertex vert
			#pragma fragment DownsampleDepth

			ENDHLSL
		}

		Pass // pass 1 - downsample depth fast mode

		{
			HLSLPROGRAM

			#pragma target 4.6
			#pragma vertex vert
			#pragma fragment DownsampleDepthFastMode

			ENDHLSL
		}

		// pass 2 - horizontal blur
		Pass
		{
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment horizontalFrag
			#pragma target 4.0

			half4 horizontalFrag(v2f i) : SV_Target
			{
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
				return BilateralBlur(i.uv, int2(1, 0), BLUR_KERNEL_SIZE);
			}

			ENDHLSL
		}

		Pass // pass 3 - horizontal blur  Fast mode

		{
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment horizontalFrag
			#pragma target 4.0

			half4 horizontalFrag(v2f i) : SV_Target
			{
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
				return BilateralBlur(i.uv, int2(1, 0), BLUR_KERNEL_SIZE_FAST_MODE);
			}

			ENDHLSL
		}

		Pass	// pass 4 - vertical blur

		{
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment verticalFrag
			#pragma target 4.0

			half4 verticalFrag(v2f i) : SV_Target
			{
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
				return BilateralBlur(i.uv, int2(0, 1), BLUR_KERNEL_SIZE);
			}

			ENDHLSL
		}

		Pass // pass 5 - vertical blur  Fast mode

		{
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment verticalFrag
			#pragma target 4.0

			half4 verticalFrag(v2f i) : SV_Target
			{
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
				return BilateralBlur(i.uv, int2(0, 1), BLUR_KERNEL_SIZE_FAST_MODE);
			}

			ENDHLSL
		}


		Pass // pass 6 - bilateral upsample

		{
			Blend One Zero

			HLSLPROGRAM
			#pragma vertex vertUpsample
			#pragma fragment BilateralUpsample
			#pragma target 4.6

			ENDHLSL
		}
	}
}
