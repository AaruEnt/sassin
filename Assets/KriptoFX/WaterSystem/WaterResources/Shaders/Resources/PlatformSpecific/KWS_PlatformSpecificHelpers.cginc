//#define ENVIRO_FOG
//#define ENVIRO_3_FOG
//#define AZURE_FOG
//#define ATMOSPHERIC_HEIGHT_FOG
//#define VOLUMETRIC_FOG_AND_MIST
//#define COZY_FOG

//ATMOSPHERIC_HEIGHT_FOG also need to change the "Queue" = "Transparent-1"      -> "Queue" = "Transparent+2"
//VOLUMETRIC_FOG_AND_MIST also need to enable "Water->Rendering->DrawToDepth"
//enviro works out of the box with depth writing? #include "Assets/Enviro 3 - Sky and Weather/Resources/Shader/Includes/FogIncludeHLSL.hlsl" 
#define _FrustumCameraPlanes unity_CameraWorldClipPlanes

#ifdef KWS_COMPUTE
	#undef ENVIRO_3_FOG
#endif

//------------------  unity includes   ----------------------------------------------------------------

#ifndef UNIVERSAL_PIPELINE_CORE_INCLUDED
	#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#endif

#ifndef UNIVERSAL_LIGHTING_INCLUDED
	#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#endif

//-------------------------------------------------------------------------------------------------------



//------------------  thid party assets  ----------------------------------------------------------------

#if defined(ENVIRO_FOG)
	#include "Assets/Enviro - Sky and Weather/Core/Resources/Shaders/Core/EnviroFogCore.hlsl"
#endif

#if defined(ENVIRO_3_FOG)
	#include "Assets/Third-party assets/Enviro 3 - Sky and Weather/Resources/Shader/Includes/FogIncludeHLSL.hlsl"
#endif

#if defined(AZURE_FOG)
	#include "Assets/Azure[Sky] Dynamic Skybox/Shaders/Transparent/AzureFogCore.cginc"
#endif

#if defined(ATMOSPHERIC_HEIGHT_FOG)
	#include "Assets/Third-party assets/BOXOPHOBIC/Atmospheric Height Fog/Core/Includes/AtmosphericHeightFog.cginc"
#endif

#if defined(VOLUMETRIC_FOG_AND_MIST)
	#include "Assets/Third-party assets/VolumetricFog/Resources/Shaders/VolumetricFogOverlayVF.cginc"
#endif

#if defined(COZY_FOG)
	#include "Assets/Third-party assets/Distant Lands/Cozy Weather/Contents/Materials/Shaders/Includes/StylizedFogIncludes.cginc"
#endif

//-------------------------------------------------------------------------------------------------------

#ifndef KWS_WATER_VARIABLES
	#include "..\Common\KWS_WaterVariables.cginc"
#endif


DECLARE_TEXTURE(_CameraDepthTexture);
DECLARE_TEXTURE(_CameraOpaqueTexture);

SamplerState sampler_CameraDepthTexture;
float4 _CameraDepthTexture_TexelSize;
float4 _CameraDepthTexture_ST;

SamplerState sampler_CameraOpaqueTexture;
float4 _CameraOpaqueTexture_TexelSize;
float4 _CameraOpaqueTexture_RTHandleScale;

float3 KWS_AmbientColor;

inline void OverrideUnityInstanceMatrixes(float3 position, float3 size)
{
	#ifdef unity_ObjectToWorld 
		#undef unity_ObjectToWorld
	#endif
	#ifdef unity_WorldToObject 
		#undef unity_WorldToObject
	#endif

	position.y += 0.001;
	unity_ObjectToWorld._11_21_31_41 = float4(size.x, 0,  0, 0);
	unity_ObjectToWorld._12_22_32_42 = float4(0, size.y, 0, 0);
	unity_ObjectToWorld._13_23_33_43 = float4(0, 0, size.z, 0);
	unity_ObjectToWorld._14_24_34_44 = float4(position.xyz, 1);

	unity_WorldToObject = unity_ObjectToWorld;
	unity_WorldToObject._14_24_34 *= -1;
	unity_WorldToObject._11_22_33 = 1.0f / unity_WorldToObject._11_22_33;
}


inline float4 ObjectToClipPos(float4 vertex)
{
	return TransformObjectToHClip(vertex.xyz);
}

inline float2 GetTriangleUVScaled(uint vertexID)
{
	#if UNITY_UV_STARTS_AT_TOP
		return float2((vertexID << 1) & 2, 1.0 - (vertexID & 2));
	#else
		return float2((vertexID << 1) & 2, vertexID & 2);
	#endif
}

inline float2 GetScreenUV(float2 vertex)
{
	return vertex.xy / _ScreenSize.xy;
}

inline float2 GetNormalizedRTHandleUV(float2 screenUV)
{
	return screenUV;
}


inline float4 GetTriangleVertexPosition(uint vertexID, float z = UNITY_NEAR_CLIP_VALUE)
{
	float2 uv = float2((vertexID << 1) & 2, vertexID & 2);
	return float4(uv * 2.0 - 1.0, z, 1.0);
}

inline float3 LocalToWorldPos(float3 localPos)
{
	float3 worldPos = mul(UNITY_MATRIX_M, float4(localPos, 1)).xyz;
	return GetAbsolutePositionWS(worldPos).xyz;
}

inline float3 WorldToLocalPos(float3 worldPos)
{
	return mul(UNITY_MATRIX_I_M, float4(GetCameraRelativePositionWS(worldPos), 1)).xyz;
}

inline float3 WorldToLocalPosWithoutTranslation(float3 worldPos)
{
	return mul((float3x3)UNITY_MATRIX_I_M, worldPos).xyz;
}

inline float3 GetAbsoluteWorldSpacePos()
{
		return UNITY_MATRIX_I_V._m03_m13_m23;
	//return _WorldSpaceCameraPos.xyz; //cause shader error in 'Hidden/KriptoFX/KWS/VolumetricLighting': Program 'frag', error X8000: D3D11 Internal Compiler Error: Invalid Bytecode:
	//source register relative index temp register component 1 in r7 uninitialized. Opcode #61 (count is 1-based) at line 15 (on vulkan)
}

inline float3 GetCameraRelativePosition(float3 worldPos)
{
	return GetCameraRelativePositionWS(worldPos);
}

inline float3 GetWorldSpaceViewDirNorm(float3 worldPos)
{
	return normalize(_WorldSpaceCameraPos.xyz - worldPos.xyz);
}

inline float3 GetWorldSpaceNormal(float3 normal)
{
	return normalize(mul((float3x3)UNITY_MATRIX_M, normal)).xyz;
}

inline float GetWorldToCameraDistance(float3 worldPos)
{
	return length(_WorldSpaceCameraPos.xyz - worldPos.xyz);
}

float3 GetWorldSpacePositionFromDepth(float2 uv, float deviceDepth)
{
	return GetAbsolutePositionWS(ComputeWorldSpacePosition(uv, deviceDepth, KWS_MATRIX_I_VP)); //UNITY_MATRIX_I_VP have bugs in VR
}

inline float GetSceneDepth(float2 uv)
{  
	float depth = SAMPLE_TEXTURE_LOD(_CameraDepthTexture, sampler_CameraDepthTexture, uv, 0).x;
	#ifndef UNITY_REVERSED_Z
         depth = lerp(UNITY_NEAR_CLIP_VALUE, 1, depth);
    #endif
	return depth;
}

inline float4 GetSceneDepthGather(float2 uv)
{
	float4 depth =  SAMPLE_TEXTURE_GATHER(_CameraDepthTexture, sampler_CameraDepthTexture, uv);
	#ifndef UNITY_REVERSED_Z
         depth = lerp(UNITY_NEAR_CLIP_VALUE, 1, depth);
    #endif
	return depth;
}

inline float3 ScreenPosToWorldPos(float2 uv)
{
	float depth = GetSceneDepth(uv);
	float3 posWS = GetWorldSpacePositionFromDepth(uv, depth);
	return GetAbsolutePositionWS(posWS);
}

inline float4 WorldPosToScreenPos(float3 pos)
{
	pos = GetCameraRelativePositionWS(pos);
	float4 projected = mul(UNITY_MATRIX_VP, float4(pos, 1.0f));
	projected.xy = (projected.xy / projected.w) * 0.5f + 0.5f;
	#ifdef UNITY_UV_STARTS_AT_TOP
		projected.xy.y = 1 - projected.xy.y;
	#endif
	return projected;
}

inline float2 WorldPosToScreenPosReprojectedPrevFrame(float3 pos, float2 texelSize)
{	
	float4 projected = mul(KWS_PREV_MATRIX_VP, float4(pos, 1.0f));
	float2 uv = (projected.xy / projected.w) * 0.5f + 0.5f;
	#ifdef UNITY_UV_STARTS_AT_TOP
		uv.y = 1 - uv.y;
	#endif
	return uv + texelSize * 0.5;
}


inline float3 GetAmbientColor()
{
	return KWS_AmbientColor;
}


inline float3 GetSceneColor(float2 uv)
{
	return SAMPLE_TEXTURE_LOD(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, uv, 0).xyz;
	//return _CameraOpaqueTexture.SampleLevel(sampler_CameraOpaqueTexture, uv, 0).xyz;
}

inline half3 GetSceneColorPoint(float2 uv)
{
	return GetSceneColor(uv);
}

inline half3 GetSceneColorWithDispersion(float2 uv, float dispersionStrength)
{
	half3 refraction;
	refraction.r = GetSceneColor(uv - _CameraOpaqueTexture_TexelSize.xy * dispersionStrength).r;
	refraction.g = GetSceneColor(uv).g;
	refraction.b = GetSceneColor(uv + _CameraOpaqueTexture_TexelSize.xy * dispersionStrength).b;
	return refraction;
}


inline float3 GetMainLightDir()
{
	return _MainLightPosition.xyz;
}

inline float3 GetMainLightColor()
{
	return _MainLightColor.rgb;
}

inline float4 ComputeNonStereoScreenPos(float4 pos)
{
	float4 o = pos * 0.5f;
	o.xy = float2(o.x, o.y * _ProjectionParams.x) + o.w;
	o.zw = pos.zw;
	return o;
}

inline float4 ComputeGrabScreenPos(float4 pos)
{
	#if UNITY_UV_STARTS_AT_TOP
		float scale = -1.0;
	#else
		float scale = 1.0;
	#endif
	float4 o = pos * 0.5f;
	o.xy = float2(o.x, o.y * scale) + o.w;
	#ifdef UNITY_SINGLE_PASS_STEREO
		o.xy = TransformStereoScreenSpaceTex(o.xy, pos.w);
	#endif
	o.zw = pos.zw;
	return o;
}

inline float LinearEyeDepth(float z)
{
	return 1.0 / (_ZBufferParams.z * z + _ZBufferParams.w);
}


inline void GetInternalFogVariables(float4 pos, float3 viewDir, float surfaceDepthZ, float screenPosZ, out half3 fogColor, out half3 fogOpacity)
{
	#if defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2)
		float fogRawFactor = ComputeFogFactor(screenPosZ);
		fogOpacity = 1 - saturate(ComputeFogIntensity(fogRawFactor));
		fogColor = unity_FogColor.rgb;
	#else
		fogOpacity = half3(0.0, 0.0, 0.0);
		fogColor = half3(0.0, 0.0, 0.0);
	#endif
}


inline half3 ComputeInternalFog(half3 sourceColor, half3 fogColor, half3 fogOpacity)
{
	#if defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2)
		return lerp(sourceColor, fogColor, fogOpacity);
	#else
		return sourceColor;
	#endif
}

inline half3 ComputeThirdPartyFog(half3 sourceColor, float3 worldPos, float2 screenUV, float3 screenPosZ)
{
	#if defined(ENVIRO_FOG)
		sourceColor = TransparentFog(half4(sourceColor, 1.0), worldPos.xyz, screenUV, screenPosZ);
	#elif defined(ENVIRO_3_FOG)
		sourceColor = ApplyFogAndVolumetricLights(sourceColor, screenUV, worldPos.xyz, Linear01Depth(screenPosZ, _ZBufferParams));
	#elif defined(AZURE_FOG)
		sourceColor = ApplyAzureFog(half4(sourceColor, 1.0), worldPos.xyz).xyz;
	#elif defined(ATMOSPHERIC_HEIGHT_FOG)
		float4 fogParams = GetAtmosphericHeightFog(worldPos);
		fogParams.a = saturate(fogParams.a * 1.5f); //by some reason max value < 0.75;
		sourceColor = ApplyAtmosphericHeightFog(half4(sourceColor, 1.0), fogParams).xyz;
	#elif defined(VOLUMETRIC_FOG_AND_MIST)
		sourceColor = overlayFog(worldPos, float4(screenUV, screenPosZ, 1), half4(sourceColor, 1.0)).xyz;
	#elif defined(COZY_FOG)
		sourceColor = BlendStylizedFog(worldPos, half4(sourceColor.xyz, 1));
	#endif
	
	return max(0, sourceColor);
}

inline float GetExposure()
{
	return 1.0;
}

float GetSurfaceDepth(float screenPosZ)
{
	#if UNITY_REVERSED_Z
		#if SHADER_API_OPENGL || SHADER_API_GLES || SHADER_API_GLES3
			//GL with reversed z => z clip range is [near, -far] -> should remap in theory but dont do it in practice to save some perf (range is close enough)
			return max(-screenPosZ, 0);
		#else
			//D3d with reversed Z => z clip range is [near, 0] -> remapping to [0, far]
			//max is required to protect ourselves from near plane not being correct/meaningfull in case of oblique matrices.
			return max(((1.0 - screenPosZ / _ProjectionParams.y) * _ProjectionParams.z), 0);
		#endif
	#elif UNITY_UV_STARTS_AT_TOP
		//D3d without reversed z => z clip range is [0, far] -> nothing to do
		return screenPosZ;
	#else
		//Opengl => z clip range is [-near, far] -> should remap in theory but dont do it in practice to save some perf (range is close enough)
		return screenPosZ;
	#endif
}
