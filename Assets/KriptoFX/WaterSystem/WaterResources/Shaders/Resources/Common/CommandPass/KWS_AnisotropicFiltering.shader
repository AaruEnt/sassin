Shader "Hidden/KriptoFX/KWS/AnisotropicFiltering"
{

	HLSLINCLUDE
	#pragma multi_compile _ USE_STEREO_ARRAY

	#include "../../PlatformSpecific/Includes/KWS_HelpersIncludes.cginc"

	float KWS_IsCubemapSide;

	#ifdef USE_STEREO_ARRAY
		DECLARE_TEXTURE (_SourceRT);
	#else
		Texture2D _SourceRT;
	#endif
	
	
	float4 _SourceRTHandleScale;

	uint KWS_ReverseBits32(uint bits)
	{
		#if 0 // Shader model 5
			return reversebits(bits);
		#else
			bits = (bits << 16) | (bits >> 16);
			bits = ((bits & 0x00ff00ff) << 8) | ((bits & 0xff00ff00) >> 8);
			bits = ((bits & 0x0f0f0f0f) << 4) | ((bits & 0xf0f0f0f0) >> 4);
			bits = ((bits & 0x33333333) << 2) | ((bits & 0xcccccccc) >> 2);
			bits = ((bits & 0x55555555) << 1) | ((bits & 0xaaaaaaaa) >> 1);
			return bits;
		#endif
	}
	//-----------------------------------------------------------------------------
	float KWS_RadicalInverse_VdC(uint bits)
	{
		return float(KWS_ReverseBits32(bits)) * 2.3283064365386963e-10; // 0x100000000

	}

	//-----------------------------------------------------------------------------
	float2 KWS_Hammersley2d(uint i, uint maxSampleCount)
	{
		return float2(float(i) / float(maxSampleCount), KWS_RadicalInverse_VdC(i));
	}

	//-----------------------------------------------------------------------------
	float KWS_HashRand(uint s)
	{
		s = s ^ 2747636419u;
		s = s * 2654435769u;
		s = s ^(s >> 16);
		s = s * 2654435769u;
		s = s ^(s >> 16);
		s = s * 2654435769u;
		return float(s) / 4294967295.0f;
	}

	//-----------------------------------------------------------------------------
	float KWS_InitRand(float input)
	{
		return KWS_HashRand(uint(input * 4294967295.0f));
	}

	half4 ReflectionPreFiltering(float2 uv, const uint SAMPLE_COUNT)
	{
		half4 prefilteredColor = 0.0;
		float randNum = KWS_InitRand(uv.x * uv.y);

		float anisoScaleOffset = KWS_AnisoReflectionsScale * 0.75;

		UNITY_LOOP
		for (uint i = 0u; i < SAMPLE_COUNT; ++i)
		{
			float2 u = KWS_Hammersley2d(i, SAMPLE_COUNT);
			u = frac(u + randNum + 0.5f);
			#ifdef USE_STEREO_ARRAY
				float4 color = max(0, SAMPLE_TEXTURE_LOD(_SourceRT, sampler_linear_clamp, uv - float2(0, u.x * KWS_AnisoReflectionsScale - anisoScaleOffset), 0));
			#else
				float4 color = max(0, _SourceRT.SampleLevel(sampler_linear_clamp, uv - float2(0, u.x * KWS_AnisoReflectionsScale - anisoScaleOffset), 0));
			#endif
			prefilteredColor += color;
		}
		prefilteredColor = prefilteredColor / (1 * SAMPLE_COUNT);

		return prefilteredColor;
	}

	float2 RevertHalfUV(float2 uv)
	{
		if (KWS_IsCubemapSide < 0.5) return uv;
		
		if (KWS_IsCubemapReflectionPlanar > 0.5) uv.y = abs(abs(uv.y - 0.5) - 0.5);
		else uv.y = abs(uv.y - 0.5) + 0.5;

		return uv;
	}

	half4 ReflectionPreFilteringCubemap(float2 uv, const uint SAMPLE_COUNT, float offset)
	{
		half4 prefilteredColor = 0.0;
		float randNum = KWS_InitRand(uv.x * uv.y);

		UNITY_LOOP
		for (uint i = 0u; i < SAMPLE_COUNT; ++i)
		{
			float2 u = KWS_Hammersley2d(i, SAMPLE_COUNT);
			u = frac(u + randNum);

			#ifdef USE_STEREO_ARRAY
				float4 color = max(0, SAMPLE_TEXTURE_LOD(_SourceRT, sampler_linear_clamp, RevertHalfUV(uv - float2(0, u.x * offset * KWS_AnisoReflectionsScale * uv.y)), 0));
			#else
				prefilteredColor += _SourceRT.SampleLevel(sampler_linear_clamp, RevertHalfUV(uv - float2(0, u.x * offset * KWS_AnisoReflectionsScale * uv.y)), 0);
			#endif
			
		}
		prefilteredColor = prefilteredColor / (1 * SAMPLE_COUNT);

		return prefilteredColor;
	}

	struct vertexInput
	{
        uint vertexID : SV_VertexID;
		UNITY_VERTEX_INPUT_INSTANCE_ID
	};

	struct vertexOutput
	{
		float4 vertex : SV_POSITION;
		float2 uv : TEXCOORD0;
		UNITY_VERTEX_OUTPUT_STEREO
	};

	vertexOutput vert(vertexInput v)
	{
		vertexOutput o;
		UNITY_SETUP_INSTANCE_ID(v);
		UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
		o.vertex = GetTriangleVertexPosition(v.vertexID);
		o.uv = GetTriangleUVScaled(v.vertexID) * _SourceRTHandleScale.xy;
		return o;
	}


	ENDHLSL


	SubShader
	{
		Pass //low quality

		{
			HLSLPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			#define SAMPLE_COUNT 5

			half4 frag(vertexOutput i) : SV_Target
			{		
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
				return ReflectionPreFiltering(i.uv, SAMPLE_COUNT);
			}

			ENDHLSL
		}


		Pass //high quality

		{
			HLSLPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			#define SAMPLE_COUNT 13

			half4 frag(vertexOutput i) : SV_Target
			{
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
				return ReflectionPreFiltering(i.uv, SAMPLE_COUNT);
			}

			ENDHLSL
		}

		Pass //cubemap pass low pass

		{
			HLSLPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			#define SAMPLE_COUNT 13

			half4 frag(vertexOutput i) : SV_Target
			{
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
				return ReflectionPreFilteringCubemap(i.uv, SAMPLE_COUNT, 0.15);
			}

			ENDHLSL
		}

		Pass //cubemap pass high pass

		{
			HLSLPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			#define SAMPLE_COUNT 23

			half4 frag(vertexOutput i) : SV_Target
			{
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
				return ReflectionPreFilteringCubemap(i.uv, SAMPLE_COUNT, 0.2);
			}

			ENDHLSL
		}
	}
}