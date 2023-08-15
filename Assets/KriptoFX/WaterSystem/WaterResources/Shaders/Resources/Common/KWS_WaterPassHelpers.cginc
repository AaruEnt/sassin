#ifndef KWS_WATER_PASS_HELPERS
#define KWS_WATER_PASS_HELPERS

#ifndef KWS_WATER_VARIABLES
	#include "KWS_WaterVariables.cginc"
#endif

float CalcMipLevel(float2 uv)
{
    float2 dx = ddx(uv);
    float2 dy = ddy(uv);
    float delta = max(dot(dx, dx), dot(dy, dy));
    return max(0.0, 0.5 * log2(delta));
}

//////////////////////////////////////////////    MaskDepthNormal_Pass    //////////////////////////////////////////////
#define WATER_MASK_PASS_UNDERWATER_THRESHOLD 0.55

DECLARE_TEXTURE (KW_WaterMaskScatterNormals);
DECLARE_TEXTURE (KWS_WaterTexID);
DECLARE_TEXTURE (KW_WaterDepth);

float4 KW_WaterMaskScatterNormals_TexelSize;
float4 KW_WaterDepth_TexelSize;
float4 KWS_WaterTexID_TexelSize;
float4 KWS_WaterMask_RTHandleScale;

inline float2 GetWaterDepthNormalizedUV(float2 uv)
{
	return clamp(uv, 0.01, 0.99) * KWS_WaterMask_RTHandleScale.xy;
}

inline half4 GetWaterMaskScatterNormals(float2 uv)
{
	return SAMPLE_TEXTURE_LOD(KW_WaterMaskScatterNormals, sampler_linear_clamp, GetWaterDepthNormalizedUV(uv), 0);
}

inline half GetWaterMask(float2 uv)
{
	half4 mask = SAMPLE_TEXTURE_GATHER(KW_WaterMaskScatterNormals, sampler_linear_clamp, GetWaterDepthNormalizedUV(uv));
	return 0.25 * (mask.x + mask.y + mask.z + mask.w);
}

inline half IsUnderwaterMask(float mask)
{
	return mask >= WATER_MASK_PASS_UNDERWATER_THRESHOLD;
}

inline uint GetWaterID(float2 uv)
{
	return LOAD_TEXTURE(KWS_WaterTexID, GetWaterDepthNormalizedUV(uv) * KW_WaterDepth_TexelSize.zw).x * KWS_WATER_MASK_ENCODING_VALUE;
}

inline uint GetWaterID(float2 uv, int2 offset)
{
	return LOAD_TEXTURE_OFFSET(KWS_WaterTexID, GetWaterDepthNormalizedUV(uv) * KW_WaterDepth_TexelSize.zw, offset).x * KWS_WATER_MASK_ENCODING_VALUE;
}

inline float GetWaterDepth(float2 uv)
{
	return SAMPLE_TEXTURE_LOD(KW_WaterDepth, sampler_linear_clamp, GetWaterDepthNormalizedUV(uv), 0).x;
}
//////////////////////////////////////////////  END  MaskDepthNormal_Pass    //////////////////////////////////////////////


//////////////////////////////////////////////    VolumetricLighting_Pass    //////////////////////////////////////////////

DECLARE_TEXTURE (KWS_VolumetricLightRT);
float4 KWS_VolumetricLightRT_TexelSize;
float4 KWS_VolumetricLight_RTHandleScale;

inline float2 GetVolumetricLightNormalizedUV(float2 uv)
{
	return clamp(uv, 0.005, 0.995) * KWS_VolumetricLight_RTHandleScale.xy;
}

inline half4 GetVolumetricLight(float2 uv)
{
	return SAMPLE_TEXTURE_LOD(KWS_VolumetricLightRT, sampler_linear_clamp, GetVolumetricLightNormalizedUV(uv), 0);
}

//////////////////////////////////////////////  END    VolumetricLighting_Pass    //////////////////////////////////////////////


//////////////////////////////////////////////    ScreenSpaceReflection_Pass    //////////////////////////////////////////////
DECLARE_TEXTURE(KWS_ScreenSpaceReflectionRT);
float4 KWS_ScreenSpaceReflectionRT_TexelSize;
float4 KWS_ScreenSpaceReflection_RTHandleScale;
#define KWS_ScreenSpaceReflectionMaxMip 5

inline float2 GetScreenSpaceReflectionNormalizedUV(float2 uv)
{
	return clamp(uv, 0.001, 0.999) * KWS_ScreenSpaceReflection_RTHandleScale.xy;
}

inline half4 GetScreenSpaceReflection(float2 uv)
{
	float2 ssrUV = GetScreenSpaceReflectionNormalizedUV(uv);
	float mipLevel = CalcMipLevel(ssrUV * KWS_ScreenSpaceReflectionRT_TexelSize.zw);
	float lod = min(mipLevel, KWS_ScreenSpaceReflectionMaxMip);
	float4 res = SAMPLE_TEXTURE_LOD(KWS_ScreenSpaceReflectionRT, sampler_linear_clamp, ssrUV, lod);
	res.a = saturate(res.a); //I use negative alpha to minimize edge bilinear interpolation artifacts. 
	return res;
}


inline half4 GetScreenSpaceReflectionWithStretchingMask(float2 refl_uv)
{
	#if defined(STEREO_INSTANCING_ON)
		refl_uv -= mul((float2x2)UNITY_MATRIX_V, float2(0, KWS_ReflectionClipOffset)).xy;
	#else
		refl_uv.y -= KWS_ReflectionClipOffset;
	#endif
	

	float stretchingMask = 1 - abs(refl_uv.x * 2 - 1);
	//stretchingMask = min(1, stretchingMask * 3);
	refl_uv.x = lerp(refl_uv.x * 0.98 + 0.01, refl_uv.x, stretchingMask);
	return GetScreenSpaceReflection(refl_uv);

	//reflection.xyz = lerp(reflection.xyz, ssr.xyz, ssr.a);

}

//////////////////////////////////////////////  END  ScreenSpaceReflection_Pass    ///////////////////////////////////////////


//////////////////////////////////////////////    Reflection_Pass    //////////////////////////////////////////////////////////
DECLARE_TEXTURE(KWS_PlanarReflectionRT);
float4 KWS_PlanarReflectionRT_TexelSize;
TextureCube KWS_CubemapReflectionRT;
uint KWS_IsCubemapReflectionPlanar;
#define KWS_PlanarReflectionMaxMip 5

inline half3 GetPlanarReflection(float2 refl_uv)
{
	float mipLevel = CalcMipLevel(refl_uv * KWS_PlanarReflectionRT_TexelSize.zw);
	float lod = min(mipLevel, KWS_PlanarReflectionMaxMip);
	return SAMPLE_TEXTURE_LOD(KWS_PlanarReflectionRT, sampler_linear_clamp, refl_uv, lod).xyz;
}

inline half3 GetPlanarReflectionRaw(float2 refl_uv)
{
	return SAMPLE_TEXTURE_LOD(KWS_PlanarReflectionRT, sampler_point_clamp, refl_uv, 0).xyz;
}

inline half3 GetPlanarReflectionWithClipOffset(float2 refl_uv)
{
	#if defined(STEREO_INSTANCING_ON)
		refl_uv -= mul((float2x2)UNITY_MATRIX_V, float2(0, KWS_ReflectionClipOffset)).xy;
	#else
		refl_uv.y -= KWS_ReflectionClipOffset;
	#endif
	return GetPlanarReflection(refl_uv);
}

inline half3 GetCubemapReflectionLod(float3 reflDir, float lod)
{
	if (KWS_IsCubemapReflectionPlanar < 0.5) reflDir.y = -reflDir.y;
		//#if defined(STEREO_INSTANCING_ON)
		//	return UNITY_SAMPLE_TEXCUBE_LOD(unity_SpecCube0, reflDir, 1.0);
		//#else 
			return KWS_CubemapReflectionRT.SampleLevel(sampler_linear_clamp, reflDir, lod).xyz;
		//#endif
}

inline half3 GetCubemapReflection(float3 reflDir)
{
	if (KWS_IsCubemapReflectionPlanar < 0.5) reflDir.y = -reflDir.y;
	
	//#if defined(STEREO_INSTANCING_ON)
	//		return UNITY_SAMPLE_TEXCUBE_LOD(unity_SpecCube0, reflDir, 1.0);
	//#else 
			return KWS_CubemapReflectionRT.Sample(sampler_linear_clamp, reflDir).xyz;
	//#endif
	
}
//////////////////////////////////////////////  END  Reflection_Pass    ///////////////////////////////////////////////////////



//////////////////////////////////////////////    Caustic_Pass    //////////////////////////////////////////////////////////
Texture2D KW_CausticLod0;
Texture2D KW_CausticLod1;
Texture2D KW_CausticLod2;
Texture2D KW_CausticLod3;

float4 KW_CausticLodSettings;
float3 KW_CausticLodOffset;
float KW_CaustisStrength;
float KW_DecalScale;


/// //////////////////////////////////////////////  END  Caustic_Pass    //////////////////////////////////////////////////////////


#endif