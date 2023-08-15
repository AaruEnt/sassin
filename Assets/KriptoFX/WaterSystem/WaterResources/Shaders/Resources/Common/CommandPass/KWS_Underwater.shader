Shader "Hidden/KriptoFX/KWS/Underwater"
{
	HLSLINCLUDE
	
	#include "../../PlatformSpecific/Includes/KWS_HelpersIncludes.cginc"
	
	DECLARE_TEXTURE(_SourceRT);
	float4 KWS_Underwater_RTHandleScale;
	float4 _SourceRTHandleScale;

	inline half4 GetUnderwaterColor(float2 uv)
	{
		half mask = GetWaterMask(uv);
		
		if (!IsUnderwaterMask(mask)) return 0;

		float waterDepth = GetWaterDepth(uv - KW_WaterDepth_TexelSize.y * 3);
		float z = GetSceneDepth(uv);
		float linearZ = LinearEyeDepth(z);
		float depthSurface = LinearEyeDepth(waterDepth);
		half waterSurfaceMask = (depthSurface - linearZ) > 0;

		#if USE_VOLUMETRIC_LIGHT
			half halfMask = 1 - saturate(mask * 2 - 1);
			half3 volumeScattering = GetVolumetricLight(uv.xy - float2(0, halfMask * 0.02)).xyz;
		#else
			half4 volumeScattering = half4(GetAmbientColor(), 1.0);
		#endif

		half2 normals = GetWaterMaskScatterNormals(uv.xy).zw * 2 - 1;
		half3 waterColorUnder = GetSceneColor(lerp(uv.xy, uv.xy * 1.75, 0));
		half3 waterColorBellow = GetSceneColor(uv.xy + normals * mask);
		half3 refraction = lerp(waterColorBellow, waterColorUnder, waterSurfaceMask);

		float fade = max(0, min(depthSurface, linearZ)) * 0.25;
		half3 underwaterColor = ComputeUnderwaterColor(refraction, volumeScattering.rgb, fade, KW_Transparent, KW_WaterColor.xyz, KW_Turbidity, KW_TurbidityColor.xyz, 0, 0);
		return half4(underwaterColor, 1);
	}

	inline half4 GetUnderwaterBluredColor(float2 uv)
	{
		half mask = GetWaterMask(uv);
		
		if (mask < WATER_MASK_PASS_UNDERWATER_THRESHOLD) return 0;

		half3 underwaterColor = SAMPLE_TEXTURE_LOD(_SourceRT, sampler_linear_clamp, uv * _SourceRTHandleScale.xy, 0).xyz;
		return half4(underwaterColor, 1);
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
		o.uv = GetTriangleUVScaled(v.vertexID);
		return o;
	}

	half4 frag(vertexOutput i) : SV_Target
	{
		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
		half4 underwaterColor = GetUnderwaterColor(i.uv);
		
		return underwaterColor;
	}

	half4 fragPostFX(vertexOutput i) : SV_Target
	{
		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
		half4 underwaterColor = GetUnderwaterBluredColor(i.uv);
		return underwaterColor;
	}

	ENDHLSL

	SubShader
	{
		Tags { "Queue" = "Transparent+1" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha
		ZTest Always
		ZWrite Off

		Stencil
		{
			Ref 230
			Comp Greater
			Pass keep
		}

		Pass
		{
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 4.6

			#pragma multi_compile _ USE_VOLUMETRIC_LIGHT

			ENDHLSL
		}

		Pass
		{
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment fragPostFX
			#pragma target 4.6

			ENDHLSL
		}
	}
}