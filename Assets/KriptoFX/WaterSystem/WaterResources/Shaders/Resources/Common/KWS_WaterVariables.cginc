#ifndef KWS_WATER_VARIABLES
#define KWS_WATER_VARIABLES

#if defined(STEREO_INSTANCING_ON)
	#define DECLARE_TEXTURE(tex) Texture2DArray tex
	#define DECLARE_TEXTURE_UINT(tex) Texture2DArray<uint> tex
	#define SAMPLE_TEXTURE(tex, samplertex, uv) tex.Sample(samplertex, float3(uv, (float)unity_StereoEyeIndex))
	#define SAMPLE_TEXTURE_LOD(tex, samplertex, uv, lod) tex.SampleLevel(samplertex, float3(uv, (float)unity_StereoEyeIndex), lod)
	#define SAMPLE_TEXTURE_LOD_OFFSET(tex, samplertex, uv, lod, offset) tex.SampleLevel(samplertex, float3(uv, (float)unity_StereoEyeIndex), lod, offset)
	#define SAMPLE_TEXTURE_GATHER(tex, samplertex, uv) tex.Gather(samplertex, float3(uv, (float)unity_StereoEyeIndex))
	#define LOAD_TEXTURE(tex, uv) tex.Load(uint4(uv, unity_StereoEyeIndex, 0))
	#define LOAD_TEXTURE_OFFSET(tex, uv, offset) tex.Load(uint4(uv, unity_StereoEyeIndex, 0), offset)
#else
	#define DECLARE_TEXTURE(tex) Texture2D tex
	#define DECLARE_TEXTURE_UINT(tex) Texture2D<uint> tex
	#define SAMPLE_TEXTURE(tex, samplertex, uv) tex.Sample(samplertex, uv)
	#define SAMPLE_TEXTURE_LOD(tex, samplertex, uv, lod) tex.SampleLevel(samplertex, uv, lod)
	#define SAMPLE_TEXTURE_LOD_OFFSET(tex, samplertex, uv, lod, offset) tex.SampleLevel(samplertex, uv, lod, offset)
	#define SAMPLE_TEXTURE_GATHER(tex, samplertex, uv) tex.Gather(samplertex, uv)
	#define LOAD_TEXTURE(tex, uv) tex.Load(uint3(uv, 0))
	#define LOAD_TEXTURE_OFFSET(tex, uv, offset) tex.Load(uint3(uv, 0), offset)
#endif

float4x4 KWS_MATRIX_VP_STEREO[2];
float4x4 KWS_PREV_MATRIX_VP_STEREO[2];
float4x4 KWS_MATRIX_I_VP_STEREO[2];
float4x4 KWS_MATRIX_CAMERA_PROJECTION_STEREO[2];

float4x4 KWS_MATRIX_VP;
float4x4 KWS_PREV_MATRIX_VP;
float4x4 KWS_MATRIX_I_VP;
float4x4 KWS_MATRIX_CAMERA_PROJECTION;

float4x4 KW_ViewToWorld;
float4x4 KW_ProjToView;

#if defined(STEREO_INSTANCING_ON)
	#define KWS_MATRIX_VP KWS_MATRIX_VP_STEREO[unity_StereoEyeIndex]
	#define KWS_PREV_MATRIX_VP KWS_PREV_MATRIX_VP_STEREO[unity_StereoEyeIndex]
	#define KWS_MATRIX_I_VP KWS_MATRIX_I_VP_STEREO[unity_StereoEyeIndex]
	#define KWS_MATRIX_CAMERA_PROJECTION KWS_MATRIX_CAMERA_PROJECTION_STEREO[unity_StereoEyeIndex]
#else
	#define KWS_MATRIX_VP KWS_MATRIX_VP
	#define KWS_PREV_MATRIX_VP KWS_PREV_MATRIX_VP
	#define KWS_MATRIX_I_VP KWS_MATRIX_I_VP
	#define KWS_MATRIX_CAMERA_PROJECTION KWS_MATRIX_CAMERA_PROJECTION
#endif

#define MIN_THRESHOLD 0.00001
#define KWS_SHORELINE_OFFSET_MULTIPLIER 34.0
#define KWS_WATER_MASK_DECODING_VALUE 1.0/255.0
#define KWS_WATER_MASK_ENCODING_VALUE 255.0

SamplerState sampler_point_clamp;
SamplerState sampler_linear_repeat;
SamplerState sampler_linear_clamp;
SamplerState sampler_trilinear_clamp;
SamplerState sampler_trilinear_repeat;

float4 KWS_WaterViewPort;

half4 _MainColor;
half4 _SeaColor;
half4 _BottomColor;
half4 _BottomColorDeep;
half4 _SSSColor;
half4 _DiffColor;
half4 _BubblesColor;
half4 _IndirectDiffColor;
half4 _IndirectSpecColor;
half _Metalic;
half _Roughness;

//sampler2D KW_PointLightAttenuation;
//sampler2D _BRDFTex;
//sampler2D _MainTex;
//sampler2D					_FoamTex;
//sampler2D					_BubblesTex;
Texture2D					KW_FlowMapTex;
Texture2D					KW_DispTex;
Texture2D					KW_DispTex_LOD1;
Texture2D					KW_DispTex_LOD2;
Texture2D					KW_NormTex;
SamplerState sampler_KW_NormTex;
Texture2D					KW_NormTex_LOD1;
Texture2D					KW_NormTex_LOD2;

Texture2D KW_FluidsDepthTex;
float KW_FluidsDepthOrthoSize;
float3 KW_FluidsDepth_Near_Far_Dist;
float3 KW_FluidsDepthPos;

sampler2D KW_InteractiveWavesTex;
float4 KW_InteractiveWavesTex_TexelSize;
half KW_InteractiveWavesAreaSize;

sampler2D KW_RipplesTexture;
sampler2D KW_RipplesTexturePrev;
sampler2D KW_RipplesNormalTexture;
sampler2D KW_RipplesNormalTexturePrev;
sampler2D _ReflectionTex;

sampler2D KW_WaterOpaqueTexture;
//sampler2D					_ShadowMapTexture;

sampler2D KW_WaterMaskDepth;

float4 KW_DispTex_TexelSize;
float4 KW_DispTexDetail_TexelSize;
float4 KW_NormTex_TexelSize;
float4 KW_NormTex_LOD1_TexelSize;
float4 KW_NormTex_LOD2_TexelSize;
float4 KW_DispTex_LOD1_TexelSize;
float4 KW_DispTex_LOD2_TexelSize;



float _Distortion;
float _test;
float4 _test2;
float _test3;

half _Turbidity;
half _WaterTimeScale;
half KW_FFTDomainSize;
half KW_FFTDomainSize_LOD1;
half KW_FFTDomainSize_LOD2;
half KW_FFTDomainSize_Detail;


float2 KW_RipplesUVOffset;
half KW_RipplesScale;




half KW_WindSpeed_LOD1;
half KW_WindSpeed_LOD2;
half KW_DistortScale;



sampler2D KW_DitherTexture;
sampler2D KW_DistanceFieldDepthIntersection;
sampler2D KW_DistanceField;
sampler2D KW_TimeRemap;
sampler2D KW_ShoreWaveTex;
sampler2D KW_UpDepth;
float4 KW_DistanceFieldPos;
float4 KW_UpDepthPos;

sampler2D KW_BeachWavesTex;
float4 KW_BeachWavesPos;

uniform float4 _GAmplitude;
uniform float4 _GFrequency;
uniform float4 _GSteepness;
uniform float4 _GSpeed;
uniform float4 _GDirectionAB;
uniform float4 _GDirectionCD;


sampler2D _CameraDepthTextureBeforeWaterZWrite;
sampler2D _CameraDepthTextureBeforeWaterZWrite_Blured;
float4 _CameraDepthTextureBeforeWaterZWrite_TexelSize;


half _DistanceBetweenBeachWaves;
half _MinimalDepthForBeachWaves;

float3 KW_DirLightForward;
float3 KW_DirLightColor;

int KW_PointLightCount;
float4 KW_PointLightPositions[100];
float4 KW_PointLightColors[100];


float3 KW_DynamicWavesWorldPos;
float3 KW_InteractCameraOffset_Last;
sampler2D KW_InteractiveWavesNormalTex;
sampler2D KW_InteractiveWavesNormalTexPrev;
sampler2D KW_ShorelineTex;
sampler2D KW_ShorelineNormalTex;
float4 KW_ShorelineTex_TexelSize;

sampler2D KW_ShorelineTexMap;
float KW_ShorelineSize;
float3 KW_ShorelineOffset;

float3 KW_DistanceFieldDepthPos;
float KW_DistanceFieldDepthArea;
float KW_DistanceFieldDepthFar;

sampler2D _TestTexture;
sampler2D _TestDispTexture;
sampler2D _TestNormalTexture;
float4 FoamAnimUV;

sampler2D KW_ReflectionTex;

float KW_DepthOrthographicSize;
float4 Test4;

float3 KW_FluidsMapWorldPosition_lod0;
float3 KW_FluidsMapWorldPosition_lod1;
float KW_FluidsMapAreaSize_lod0;
float KW_FluidsMapAreaSize_lod1;

Texture2D KW_Fluids_Lod0;
Texture2D KW_FluidsFoam_Lod0;
Texture2D KW_Fluids_Lod1;
Texture2D KW_FluidsFoam_Lod1;
Texture2D KW_FluidsFoamTex;
Texture2D KW_FoamTex;
Texture2D KW_FluidsFoamTexBubbles;
float KW_FluidsVelocityAreaScale;
Texture2D KW_DynamicWaves;
Texture2D KW_DynamicWavesNormal;

float KW_DynamicWavesAreaSize;
float srpBatcherFix;
float3 KWS_CameraForward;

///////////////////////////////////// Dynamic data ////////////////////////////////////////////////////////
float KW_Time;
float4 KW_WaterPosition;
uint KWS_WaterPassID;
uint KWS_UnderwaterVisible;
uint KWS_FogState;
///////////////////////////////////// END Constant data ////////////////////////////////////////////////////



///////////////////////////////////// Constant data ////////////////////////////////////////////////////////
float4 KW_TurbidityColor;
float4 KW_WaterColor;

float KW_GlobalTimeScale;
float KWS_SunMaxValue;
float KW_WaterFarDistance;
float KW_FFT_Size_Normalized;
float KW_WindSpeed;
float KW_Transparent;
float KW_Turbidity;
float KWS_SunCloudiness;
float KWS_SunStrength;
float KWS_RefractionAproximatedDepth;
float KWS_RefractionSimpleStrength;
float KWS_RefractionDispersionStrength;
float _TesselationFactor;
float _TesselationMaxDistance;
float _TesselationMaxDisplace;
float KWS_ReflectionClipOffset;
float KWS_AnisoReflectionsScale;
float KW_FlowMapSize;
float KW_FlowMapSpeed;
float KW_FlowMapFluidsStrength;
float2 KWS_FoamFadeSize;
float4 KWS_FoamColor;
float4 KW_FlowMapOffset;
uint KWS_IsCustomMesh;
uint KWS_UseWireframeMode;
uint KWS_UseMultipleSimulations;
uint KWS_UseRefractionIOR;
uint KWS_UseRefractionDispersion;
///////////////////////////////////// END Constant data ////////////////////////////////////////////////////



/////////////////////////////////////  Instanced data ///////////////////////////////////////////////////
float4x4 KWS_InstancingRotationMatrix;
float3 KWS_InstancingWaterScale;
uint KWS_IsFiniteTypeInstancing;

struct InstancedMeshDataStruct
{
	float4 position;
	float4 size;

	uint downSeam;
	uint leftSeam;
	uint topSeam;
	uint rightSeam;

	uint downInf;
	uint leftInf;
	uint topInf;
	uint rightInf;
};
StructuredBuffer<InstancedMeshDataStruct> InstancedMeshData;

///////////////////////////////////// END   Instanced variables ///////////////////////////////////////////////




/////////////////////////////////////  Shoreline variables ///////////////////////////////////////////////////
Texture2D KWS_ShorelineWavesDisplacement;
Texture2D KWS_ShorelineWavesNormal;

float4 KWS_ShorelineAreaPosSize;
uint KWS_ShorelineAreaWavesCount;

struct ShorelineDataStruct
{
	float4x4 worldMatrix;

	int waveID;
	float3 position;

	float angle;
	float3 size;

	float timeOffset;
	float3 scale;

	int flip;
	float3 pad;
};


StructuredBuffer<ShorelineDataStruct> KWS_ShorelineDataBuffer;

///////////////////////////////////// END   Shoreline variables ///////////////////////////////////////////////



//////////////////////////////////////// Ortho depth variables //////////////////////////////////////////////
Texture2D KWS_WaterOrthoDepthRT;
float3 KWS_OrthoDepthPos;
float3 KWS_OrthoDepthNearFarSize;
//////////////////////////////////// END Ortho depth variables //////////////////////////////////////////////

#endif