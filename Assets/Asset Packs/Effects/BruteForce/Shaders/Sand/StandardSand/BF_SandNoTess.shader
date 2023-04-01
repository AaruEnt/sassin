// MADE BY MATTHIEU HOULLIER
// Copyright 2022 BRUTE FORCE, all rights reserved.
// You are authorized to use this work if you have purchased the asset.
// Mail me at bruteforcegamesstudio@gmail.com if you have any questions or improvements you need.
Shader "BruteForce/Standard/SandNoTessellation" {

	Properties{

		[Header(IIIIIIII          Sand Textures          IIIIIIII)]
		[Space]
		_MainTex("Sand Albedo", 2D) = "white" {}
		_MainTexMult("Sand Albedo Multiplier", Range(0,2)) = 0.11
		[MainColor][HDR]_Color("Sand Tint", Color) = (0.77,0.86,0.91,1)
		_OverallScale("Overall Scale", Float) = 1
		[Space]
		[NoScaleOffset]_BumpMap("Sand Bumpmap", 2D) = "white" {}
		_NormalMultiplier("Sand Bumpmap Multiplier", Range(0,5)) = 0.4
		_SandNormalScale("Sand Bumpmap Scale", Range(0,2)) = 1

		[Space(20)]
		[NoScaleOffset]_SpecGlossMap("Sand Specular", 2D) = "black" {}
		_SpecMult("Spec Multiplier", Float) = 0.5
		[NoScaleOffset]_LittleSpec("Sand Little Spec", 2D) = "black" {}
		_LittleSpecForce("Little Spec Multiplier", Float) = 0.5
		_LittleSpecSize("Little Specular Size", Float) = 3

		[NoScaleOffset]_SandHeight("Sand Displacement Texture", 2D) = "white" {}
		_HeightScale("Displacement Scale", Float) = 0.33
		_DisplacementStrength("Displacement Strength", Float) = 0.3
		_DisplacementOffset("Displacement Offset", Float) = 0.1
		_DisplacementColorMult("Displacement Color Multiplier", Float) = 0.95
		_DisplacementShadowMult("Displacement Shadow Multiplier",  Range(0,2)) = 0.56
		_UpVector("Up Vector", Float) = 1
		_NormalVector("Normal Vector", Float) = 0

		[Space(20)]
		[NoScaleOffset]_SandTransition("Sand Transition", 2D) = "black" {}
		_TransitionScale("Transition Scale", Float) = 0.73
		_TransitionPower("Transition Power", Float) = 0.22
		_TransitionColor("Transition Color (additive only)", Color) = (1,1,1,1)


		[Space(10)]
		[Header(IIIIIIII          Sand Values          IIIIIIII)]
		[Space(10)]
		_MountColor("Mount Color", color) = (0.12,0.12,0.121,1)
		_BotColor("Dig Color", color) = (0.71,0.87,0.91,0)
		_NormalRTDepth("Normal Effect Depth", Range(0,3)) = 0.12
		_NormalRTStrength("Normal Effect Strength", Range(0,4)) = 2.2
		_AddSandStrength("Mount Sand Strength", Range(0,3)) = 0.52
		_RemoveSandStrength("Dig Sand Strength", Range(0,3)) = 0.5
		_SandScale("Sand Scale", float) = 1

		[Space(10)]
		[Header(IIIIIIII          Rock Textures          IIIIIIII)]
		[Space(10)]
		[NoScaleOffset]_RockTex("Rock Albedo", 2D) = "white" {}
		_RockSaturation("Rock Saturation", Float) = 1
		[HDR]_RockTint("Rock Texture Tint", Color) = (0.14,0.35,0.49,1)
		[Space]
		[NoScaleOffset]_NormalTex("Rock Normal Texture", 2D) = "black" {}
		_NormalScale("Rock Normal Scale", Range(0,1)) = 0.766
		[NoScaleOffset]_Roughness("Rock Roughness Texture", 2D) = "black" {}
		[NoScaleOffset]_ParallaxMapRock("Height map (R)", 2D) = "white" {}
		_Parallax("Height scale", Range(0, 0.01)) = 0.005
		_ParallaxMinSamples("Parallax min samples", Range(2, 100)) = 4
		_ParallaxMaxSamples("Parallax max samples", Range(2, 100)) = 20

		[Space(10)]
		[Header(IIIIIIII          Rock Values          IIIIIIII)]
		[Space(10)]
		_RockTrail("Rock Trail Color", Color) = (0.40,0.1,0.01,1)
		_TransparencyValue("Rock Transparency", Range(0,1)) = 1
		_RockScale("Rock Scale", float) = 1

		[Space(10)]
		[Header(IIIIIIII          Custom Fog          IIIIIIII)]
		[Space(10)]
		[NoScaleOffset]_FogTex("Fog Texture", 2D) = "black" {}
		[NoScaleOffset]_FlowTex("Flow Texture", 2D) = "black" {}
		_FlowMultiplier("Flow Multiplier", Range(0,1)) = 0.3
		_FogIntensity("Fog Intensity", Range(0,1)) = 0.3
		_FogColor("Fog Color", Color) = (1.0,1.0,1.0,1.0)
		_FogScale("Fog Scale", float) = 1
		_FogDirection("Fog Direction", vector) = (1, 0.3, 2, 0)

		[Space(10)]
		[Header(IIIIIIII          Lighting          IIIIIIII)]
		[Space(10)]
		_ProjectedShadowColor("Projected Shadow Color",Color) = (0.17 ,0.56 ,0.1,1)
		_SpecColor("Specular Color", Color) = (1,1,1,1)
		_SpecForce("Sand Specular Force", Float) = 3
		_ShininessSand("Sand Shininess", Float) = 25
		[Space]
		_RoughnessStrength("Rock Roughness Strength", Float) = 1.75
		_ShininessRock("Rock Shininess", Float) = 10
		[Space]
		_LightOffset("Light Offset", Range(0, 1)) = 0.2
		_LightHardness("Light Hardness", Range(0, 1)) = 0.686
		_RimColor("Rim Sand Color", Color) = (0.03,0.03,0.03,0)
		_LightIntensity("Additional Lights Intensity", Range(0.00, 4)) = 1

		[Header(Procedural Tiling)]
		[Space]
		[Toggle(USE_PR)] _UsePR("Use Procedural Tiling (Reduce performance)", Float) = 0
		_ProceduralDistance("Tile start distance", Float) = 5.5
		_ProceduralStrength("Tile Smoothness", Float) = 1.5
		[Space]

		[Space(10)]
		[Header(IIIIIIII          Pragmas          IIIIIIII)]
		[Space(10)]
		[Toggle(IS_ROCK)] _ISROCK("Is Only Rock", Float) = 0
		[Toggle(IS_SAND)] _ISSAND("Is Only Sand", Float) = 0
		[Toggle(IS_UNLIT)] _ISUNLIT("Is Unlit", Float) = 0
		[Toggle(IS_ADD)] _ISADD("Is Additive Sand", Float) = 0
		[HideInInspector][Toggle(USE_INTER)] _USEINTER("Use Intersection", Float) = 0
		[Toggle(USE_AL)] _UseAmbientLight("Use Ambient Light", Float) = 1
		[Toggle(USE_RT)] _USERT("Use RT", Float) = 1
		[Toggle(IS_T)] _IST("Is Terrain", Float) = 0
		[Toggle(USE_VR)] _UseVR("Use For VR", Float) = 0
		[Toggle(USE_WC)] _USEWC("Use World Displacement", Float) = 1
		[Toggle(USE_WT)] _USEWT("Use World Coordinates", Float) = 0
		[Toggle(USE_FOG)] _USEFOG("Use Custom Fog", Float) = 1
		[Toggle(USE_LOW)] _USELOW("Use Low End", Float) = 0


			// TERRAIN PROPERTIES //
			[HideInInspector] _Control0("Control0 (RGBA)", 2D) = "white" {}
			[HideInInspector] _Control1("Control1 (RGBA)", 2D) = "white" {}
			// Textures
			[HideInInspector] _Splat0("Layer 0 (R)", 2D) = "white" {}
			[HideInInspector] _Splat1("Layer 1 (G)", 2D) = "white" {}
			[HideInInspector] _Splat2("Layer 2 (B)", 2D) = "white" {}
			[HideInInspector] _Splat3("Layer 3 (A)", 2D) = "white" {}
			[HideInInspector] _Splat4("Layer 4 (R)", 2D) = "white" {}
			[HideInInspector] _Splat5("Layer 5 (G)", 2D) = "white" {}
			[HideInInspector] _Splat6("Layer 6 (B)", 2D) = "white" {}
			[HideInInspector] _Splat7("Layer 7 (A)", 2D) = "white" {}

			// Normal Maps
			[HideInInspector] _Normal0("Normal 0 (R)", 2D) = "bump" {}
			[HideInInspector] _Normal1("Normal 1 (G)", 2D) = "bump" {}
			[HideInInspector] _Normal2("Normal 2 (B)", 2D) = "bump" {}
			[HideInInspector] _Normal3("Normal 3 (A)", 2D) = "bump" {}
			[HideInInspector] _Normal4("Normal 4 (R)", 2D) = "bump" {}
			[HideInInspector] _Normal5("Normal 5 (G)", 2D) = "bump" {}
			[HideInInspector] _Normal6("Normal 6 (B)", 2D) = "bump" {}
			[HideInInspector] _Normal7("Normal 7 (A)", 2D) = "bump" {}

			// Normal Scales
			[HideInInspector] _NormalScale0("Normal Scale 0 ", Float) = 1
			[HideInInspector] _NormalScale1("Normal Scale 1 ", Float) = 1
			[HideInInspector] _NormalScale2("Normal Scale 2 ", Float) = 1
			[HideInInspector] _NormalScale3("Normal Scale 3 ", Float) = 1
			[HideInInspector] _NormalScale4("Normal Scale 4 ", Float) = 1
			[HideInInspector] _NormalScale5("Normal Scale 5 ", Float) = 1
			[HideInInspector] _NormalScale6("Normal Scale 6 ", Float) = 1
			[HideInInspector] _NormalScale7("Normal Scale 7 ", Float) = 1

				// Mask Maps
				[HideInInspector] _Mask0("Mask 0 (R)", 2D) = "bump" {}
				[HideInInspector] _Mask1("Mask 1 (G)", 2D) = "bump" {}
				[HideInInspector] _Mask2("Mask 2 (B)", 2D) = "bump" {}
				[HideInInspector] _Mask3("Mask 3 (A)", 2D) = "bump" {}
				[HideInInspector] _Mask4("Mask 4 (R)", 2D) = "bump" {}
				[HideInInspector] _Mask5("Mask 5 (G)", 2D) = "bump" {}
				[HideInInspector] _Mask6("Mask 6 (B)", 2D) = "bump" {}
				[HideInInspector] _Mask7("Mask 7 (A)", 2D) = "bump" {}

				// specs color
				[HideInInspector] _Specular0("Specular 0 (R)", Color) = (1,1,1,1)
				[HideInInspector] _Specular1("Specular 1 (G)", Color) = (1,1,1,1)
				[HideInInspector] _Specular2("Specular 2 (B)", Color) = (1,1,1,1)
				[HideInInspector] _Specular3("Specular 3 (A)", Color) = (1,1,1,1)
				[HideInInspector] _Specular4("Specular 4 (R)", Color) = (1,1,1,1)
				[HideInInspector] _Specular5("Specular 5 (G)", Color) = (1,1,1,1)
				[HideInInspector] _Specular6("Specular 6 (B)", Color) = (1,1,1,1)
				[HideInInspector] _Specular7("Specular 7 (A)", Color) = (1,1,1,1)

					// Metallic
					[HideInInspector] _Metallic0("Metallic0", Float) = 0
					[HideInInspector] _Metallic1("Metallic1", Float) = 0
					[HideInInspector] _Metallic2("Metallic2", Float) = 0
					[HideInInspector] _Metallic3("Metallic3", Float) = 0
					[HideInInspector] _Metallic4("Metallic4", Float) = 0
					[HideInInspector] _Metallic5("Metallic5", Float) = 0
					[HideInInspector] _Metallic6("Metallic6", Float) = 0
					[HideInInspector] _Metallic7("Metallic7", Float) = 0

					[HideInInspector] _Splat0_ST("Size0", Vector) = (1,1,0)
					[HideInInspector] _Splat1_ST("Size1", Vector) = (1,1,0)
					[HideInInspector] _Splat2_ST("Size2", Vector) = (1,1,0)
					[HideInInspector] _Splat3_ST("Size3", Vector) = (1,1,0)
					[HideInInspector] _Splat4_STn("Size4", Vector) = (1,1,0)
					[HideInInspector] _Splat5_STn("Size5", Vector) = (1,1,0)
					[HideInInspector] _Splat6_STn("Size6", Vector) = (1,1,0)
					[HideInInspector] _Splat7_STn("Size7", Vector) = (1,1,0)

					[HideInInspector] _TerrainScale("Terrain Scale", Vector) = (1, 1 ,0)
					// TERRAIN PROPERTIES //
	}

		CGINCLUDE
					// Reused functions //
					

#pragma shader_feature IS_T
#pragma shader_feature IS_ROCK
#pragma shader_feature IS_SAND
#pragma shader_feature USE_VR
#pragma shader_feature USE_COMPLEX_T

#include "UnityCG.cginc"

#ifdef IS_T
				// TERRAIN DATA //
				sampler2D _Control0;
#ifdef USE_COMPLEX_T
				sampler2D _Control1;
#endif
				half4 _Specular0, _Specular1, _Specular2, _Specular3;
				float4 _Splat0_ST, _Splat1_ST, _Splat2_ST, _Splat3_ST;
				half _Metallic0, _Metallic1, _Metallic2, _Metallic3;
				half _NormalScale0, _NormalScale1, _NormalScale2, _NormalScale3;
				Texture2D _Splat0, _Splat1, _Splat2, _Splat3;
				Texture2D _Normal0, _Normal1, _Normal2, _Normal3;

				Texture2D _Mask0, _Mask1, _Mask2, _Mask3;

#ifdef USE_COMPLEX_T
				half4 _Specular4, _Specular5, _Specular6, _Specular7;
				float4 _Splat4_STn, _Splat5_STn, _Splat6_STn, _Splat7_STn;
				half _Metallic4, _Metallic5, _Metallic6, _Metallic7;
				half _NormalScale4, _NormalScale5, _NormalScale6, _NormalScale7;
				Texture2D _Splat4, _Splat5, _Splat6, _Splat7;
				Texture2D _Normal4, _Normal5, _Normal6, _Normal7;
				Texture2D _Mask4, _Mask5, _Mask6, _Mask7;
#endif

				float3 _TerrainScale;
				// TERRAIN DATA //
#endif
				SamplerState my_linear_repeat_sampler;
				SamplerState my_bilinear_repeat_sampler;
				SamplerState my_trilinear_repeat_sampler;
				SamplerState my_linear_clamp_sampler;

				float4 RotateAroundZInDegrees(float4 vertex, float degrees)
				{
					float alpha = degrees * UNITY_PI / 180.0;
					float sina, cosa;
					sincos(alpha, sina, cosa);
					float2x2 m = float2x2(cosa, -sina, sina, cosa);
					return float4(mul(m, vertex.zy), vertex.xw).zyxw;
				}
				float4 RotateAroundXInDegrees(float4 vertex, float degrees)
				{
					float alpha = degrees * UNITY_PI / 180.0;
					float sina, cosa;
					sincos(alpha, sina, cosa);
					float2x2 m = float2x2(cosa, -sina, sina, cosa);
					return float4(mul(m, vertex.xy), vertex.zw).xyzw;
				}

				float4 multQuat(float4 q1, float4 q2) {
					return float4(
						q1.w * q2.x + q1.x * q2.w + q1.z * q2.y - q1.y * q2.z,
						q1.w * q2.y + q1.y * q2.w + q1.x * q2.z - q1.z * q2.x,
						q1.w * q2.z + q1.z * q2.w + q1.y * q2.x - q1.x * q2.y,
						q1.w * q2.w - q1.x * q2.x - q1.y * q2.y - q1.z * q2.z
						);
				}

				float3 rotateVector(float4 quat, float3 vec) {
					// https://twistedpairdevelopment.wordpress.com/2013/02/11/rotating-a-vector-by-a-quaternion-in-glsl/
					float4 qv = multQuat(quat, float4(vec, 0.0));
					return multQuat(qv, float4(-quat.x, -quat.y, -quat.z, quat.w)).xyz;
				}

				void parallax_vert(
					float4 vertex,
					float3 normal,
					float4 tangent,
					out float4 eye
				) {
					float4x4 mW = unity_ObjectToWorld;
					float3 binormal = cross(normal, tangent.xyz) * tangent.w;
					float3 EyePosition = _WorldSpaceCameraPos;

					float4 localCameraPos = mul(unity_WorldToObject, float4(_WorldSpaceCameraPos, 1));
					float3 eyeLocal = vertex - localCameraPos;
					float4 eyeGlobal = mul(float4(eyeLocal, 1), mW);
					float3 E = eyeGlobal.xyz;

					float3x3 tangentToWorldSpace;

					tangentToWorldSpace[0] = mul(normalize(tangent), mW);
					tangentToWorldSpace[1] = mul(normalize(binormal), mW);
					tangentToWorldSpace[2] = mul(normalize(normal), mW);

					float3x3 worldToTangentSpace = transpose(tangentToWorldSpace);

					eye.xyz = mul(E, worldToTangentSpace);
					eye.w = 1 - dot(normalize(E), -normal);
				}

				float2 parallax_offset(
					float fHeightMapScale,
					float4 eye,
					float2 texcoord,
					Texture2D heightMap,
					int nMinSamples,
					int nMaxSamples
				) {

					float fParallaxLimit = -length(eye.xy) / eye.z;
					fParallaxLimit *= fHeightMapScale;

					float2 vOffsetDir = normalize(eye.xy);
					float2 vMaxOffset = vOffsetDir * fParallaxLimit;

					int nNumSamples = (int)lerp(nMinSamples, nMaxSamples, saturate(eye.w));

					float fStepSize = 1.0 / (float)nNumSamples;

					float2 dx = ddx(texcoord);
					float2 dy = ddy(texcoord);

					float fCurrRayHeight = 1.0;
					float2 vCurrOffset = float2(0, 0);
					float2 vLastOffset = float2(0, 0);

					float fLastSampledHeight = 1;
					float fCurrSampledHeight = 1;

					int nCurrSample = 0;

					while (nCurrSample < nNumSamples)
					{
						fCurrSampledHeight = heightMap.SampleGrad(my_linear_repeat_sampler, texcoord + vCurrOffset, dx, dy).r;
						if (fCurrSampledHeight > fCurrRayHeight)
						{
							float delta1 = fCurrSampledHeight - fCurrRayHeight;
							float delta2 = (fCurrRayHeight + fStepSize) - fLastSampledHeight;

							float ratio = delta1 / (delta1 + delta2);

							vCurrOffset = (ratio)*vLastOffset + (1.0 - ratio) * vCurrOffset;

							nCurrSample = nNumSamples + 1;
						}
						else
						{
							nCurrSample++;

							fCurrRayHeight -= fStepSize;

							vLastOffset = vCurrOffset;
							vCurrOffset += fStepSize * vMaxOffset;

							fLastSampledHeight = fCurrSampledHeight;
						}
					}

					return vCurrOffset;
				}

				ENDCG

					SubShader{

						Pass {
							Tags {
								"LightMode" = "ForwardBase"
							}
								Blend SrcAlpha OneMinusSrcAlpha

							CGPROGRAM
					// Upgrade NOTE: excluded shader from OpenGL ES 2.0 because it uses non-square matrices
					#pragma exclude_renderers gles

												#pragma target 4.6

												#pragma multi_compile _ LOD_FADE_CROSSFADE

												#pragma multi_compile_fwdbase
												#pragma multi_compile_fog
												#pragma multi_compile _ LIGHTMAP_ON

												#pragma vertex vert
												#pragma fragment frag

												#define FORWARD_BASE_PASS
												#pragma shader_feature USE_AL
												#pragma shader_feature USE_RT
												#pragma shader_feature IS_ADD
												#pragma shader_feature USE_INTER
												#pragma shader_feature USE_WC
												#pragma shader_feature USE_WT
												#pragma shader_feature USE_LOW
												#pragma shader_feature IS_UNLIT
												#pragma shader_feature USE_PR
												#pragma shader_feature USE_FOG

												#include "UnityPBSLighting.cginc"
												#include "AutoLight.cginc"

										struct VertexData //appdata
										{
											float4 vertex : POSITION;
											float3 normal : NORMAL;
											float4 tangent : TANGENT;
											float2 uv : TEXCOORD0;
											float4 color : COLOR;

					#ifdef SHADOWS_SCREEN
											SHADOW_COORDS(1)
					#endif
											UNITY_FOG_COORDS(2)
					#ifdef USE_VR
											UNITY_VERTEX_INPUT_INSTANCE_ID
					#endif
					#ifdef IS_ADD
					#ifdef USE_INTER
											float2 uv3 : TEXCOORD3;
											float2 uv4 : TEXCOORD4;
											float2 uv6 : TEXCOORD6;
											float2 uv7 : TEXCOORD7;
					#endif
					#endif
											float4 eye: TEXCOORD5;
										};

										struct InterpolatorsVertex
										{
											float4 vertex : SV_POSITION;
											float3 normal : TEXCOORD1;
											float4 tangent : TANGENT;
											float4 uv : TEXCOORD0;
											float4 color : COLOR;
											float3 worldPos : TEXCOORD2;
											float3 viewDir: POSITION1;
											float3 normalDir: TEXCOORD3;

										#ifdef SHADOWS_SCREEN
											SHADOW_COORDS(4)
										#endif
												UNITY_FOG_COORDS(5)
					#ifdef USE_VR
												UNITY_VERTEX_OUTPUT_STEREO
					#endif

												float4 eye: TEXCOORD6;

					#ifdef LIGHTMAP_ON
											float2 lmap : TEXCOORD7;
					#endif
										};

										sampler2D  _DetailTex;
										float4 _MainTex_ST, _DetailTex_ST;

										sampler2D _NormalMap;


										float _BumpScale, _DetailBumpScale;

										half4 _Color;

										// Render Texture Effects //
										uniform sampler2D _GlobalEffectRT;
										uniform float3 _Position;
										uniform float _OrthographicCamSize;
										uniform sampler2D _GlobalEffectRTAdditional;
										uniform float3 _PositionAdd;
										uniform float _OrthographicCamSizeAdditional;

										sampler2D _MainTex;
										sampler2D _SpecGlossMap;
										sampler2D _BumpMap, _LittleSpec;


										float _Parallax;
										uint _ParallaxMinSamples;
										uint _ParallaxMaxSamples;
										Texture2D _ParallaxMapRock;

										half4 _MountColor;
										half4 _BotColor;

										float _SpecForce, _SpecMult, _LittleSpecSize, _LittleSpecForce, _UpVector, _NormalVector, _RockScale, _SandScale, _TransparencyValue;
										float _NormalRTDepth, _NormalRTStrength, _AddSandStrength, _RemoveSandStrength, _DisplacementStrength, _NormalMultiplier;

										//ICE Variables
										sampler2D _RockTex;
										sampler2D _NormalTex;
										sampler2D _Roughness;
										sampler2D _SandHeight;
										Texture2D _SandTransition;
										float _TransitionScale;
										float _TransitionPower;
										float _HeightScale;
										float _LightOffset;
										float _LightHardness;
										float _LightIntensity;
										float _RockSaturation;
										float _DisplacementColorMult, _DisplacementShadowMult;
										float _FogIntensity, _FogScale, _FlowMultiplier;
										float4 _FogColor;

										Texture2D _FogTex;
										Texture2D _FlowTex;

										half _OffsetScale;
										half _OverallScale;
										half _RoughnessStrength;

										half _NormalScale, _DisplacementOffset, _SandNormalScale, _MainTexMult;
										half4 _RockTint;
										half4 _RockTrail;

										float _ShininessRock, _ShininessSand;
										float _HasRT;
										float4 _ProjectedShadowColor, _TransitionColor, _RimColor;
										float _ProceduralDistance, _ProceduralStrength;
										float3 _FogDirection;


										float3 calcNormal(float2 texcoord, sampler2D globalEffect)
										{
											const float3 off = float3(-0.0005 * _NormalRTDepth, 0, 0.0005 * _NormalRTDepth); // texture resolution to sample exact texels
											const float2 size = float2(0.002, 0.0); // size of a single texel in relation to world units

					#ifdef USE_LOW

											float sS = tex2Dlod(globalEffect, float4(texcoord.xy + 1 * off.xy * 10, 0, 0)).y;
											float s01 = sS * 0.245945946 * _NormalRTDepth;
											float s21 = sS * 0.216216216 * _NormalRTDepth;
											float s10 = sS * 0.540540541 * _NormalRTDepth;
											float s12 = sS * 0.162162162 * _NormalRTDepth;

											float gG = tex2Dlod(globalEffect, float4(texcoord.xy + 1 * off.xy, 0, 0)).z;
											float g01 = gG * 1.945945946 * _NormalRTDepth;
											float g21 = gG * 1.216216216 * _NormalRTDepth;
											float g10 = gG * 0.540540541 * _NormalRTDepth;
											float g12 = gG * 0.162162162 * _NormalRTDepth;

											float3 va = normalize(float3(size.xy, 0)) + normalize(float3(size.xy, g21 - g01));
											float3 vb = normalize(float3(size.yx, 0)) + normalize(float3(size.xy, g12 - g10));

											float3 vc = normalize(float3(size.xy, 0)) + normalize(float3(size.xy, s21 - s01));
											float3 vd = normalize(float3(size.yx, 0)) + normalize(float3(size.xy, s12 - s10));

											float3 calculatedNormal = normalize(cross(va, vb));
											calculatedNormal.y = normalize(cross(vc, vd)).x;
											return calculatedNormal;

					#else

											float s01 = tex2Dlod(globalEffect, float4(texcoord.xy + 4 * off.xy * 10, 0, 0)).y * 0.245945946 * _NormalRTDepth;
											float s21 = tex2Dlod(globalEffect, float4(texcoord.xy + 3 * off.zy * 10, 0, 0)).y * 0.216216216 * _NormalRTDepth;
											float s10 = tex2Dlod(globalEffect, float4(texcoord.xy + 2 * off.yx * 10, 0, 0)).y * 0.540540541 * _NormalRTDepth;
											float s12 = tex2Dlod(globalEffect, float4(texcoord.xy + 1 * off.yz * 10, 0, 0)).y * 0.162162162 * _NormalRTDepth;

											float g01 = tex2Dlod(globalEffect, float4(texcoord.xy + 4 * off.xy, 0, 0)).z * 1.945945946 * _NormalRTDepth;
											float g21 = tex2Dlod(globalEffect, float4(texcoord.xy + 3 * off.zy, 0, 0)).z * 1.216216216 * _NormalRTDepth;
											float g10 = tex2Dlod(globalEffect, float4(texcoord.xy + 2 * off.yx, 0, 0)).z * 0.540540541 * _NormalRTDepth;
											float g12 = tex2Dlod(globalEffect, float4(texcoord.xy + 1 * off.yz, 0, 0)).z * 0.162162162 * _NormalRTDepth;

											float3 va = normalize(float3(size.xy, 0)) + normalize(float3(size.xy, g21 - g01));
											float3 vb = normalize(float3(size.yx, 0)) + normalize(float3(size.xy, g12 - g10));

											float3 vc = normalize(float3(size.xy, 0)) + normalize(float3(size.xy, s21 - s01));
											float3 vd = normalize(float3(size.yx, 0)) + normalize(float3(size.xy, s12 - s10));

											float3 calculatedNormal = normalize(cross(va, vb));
											calculatedNormal.y = normalize(cross(vc, vd)).x;
											return calculatedNormal;
					#endif
										}

										float4 blendMultiply(float4 baseTex, float4 blendTex, float opacity)
										{
											float4 baseBlend = baseTex * blendTex;
											float4 ret = lerp(baseTex, baseBlend, opacity);
											return ret;
										}

										float2 hash2D2D(float2 s)
										{
											//magic numbers
											return frac(sin(s) * 4.5453);
										}

										//stochastic sampling
										float4 tex2DStochastic(sampler2D tex, float2 UV)
										{
											float4x3 BW_vx;
											float2 skewUV = mul(float2x2 (1.0, 0.0, -0.57735027, 1.15470054), UV * 3.464);

											//vertex IDs and barycentric coords
											float2 vxID = float2 (floor(skewUV));
											float3 barry = float3 (frac(skewUV), 0);
											barry.z = 1.0 - barry.x - barry.y;

											BW_vx = ((barry.z > 0) ?
												float4x3(float3(vxID, 0), float3(vxID + float2(0, 1), 0), float3(vxID + float2(1, 0), 0), barry.zyx) :
												float4x3(float3(vxID + float2 (1, 1), 0), float3(vxID + float2 (1, 0), 0), float3(vxID + float2 (0, 1), 0), float3(-barry.z, 1.0 - barry.y, 1.0 - barry.x)));

											//calculate derivatives to avoid triangular grid artifacts
											float2 dx = ddx(UV);
											float2 dy = ddy(UV);

											float4 stochasticTex = mul(tex2D(tex, UV + hash2D2D(BW_vx[0].xy), dx, dy), BW_vx[3].x) +
												mul(tex2D(tex, UV + hash2D2D(BW_vx[1].xy), dx, dy), BW_vx[3].y) +
												mul(tex2D(tex, UV + hash2D2D(BW_vx[2].xy), dx, dy), BW_vx[3].z);
											return stochasticTex;
										}

										InterpolatorsVertex vert(VertexData v) {
											InterpolatorsVertex i;

					#ifdef USE_VR
											UNITY_SETUP_INSTANCE_ID(v);
											UNITY_INITIALIZE_OUTPUT(InterpolatorsVertex, i);
											UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(i);
					#endif

											float3 worldPos = mul(unity_ObjectToWorld, v.vertex);
											float3 originalPos = worldPos;

											//RT Cam effects
											float2 uv = worldPos.xz - _Position.xz;
											uv = uv / (_OrthographicCamSize * 2);
											uv += 0.5;

											float2 uvAdd = worldPos.xz - _PositionAdd.xz;
											uvAdd = uvAdd / (_OrthographicCamSizeAdditional * 2);
											uvAdd += 0.5;

											float3 rippleMain = 0;
											float3 rippleMainAdditional = 0;

											float ripples = 0;
											float ripples2 = 0;
											float ripples3 = 0;

											float uvRTValue = 0;


					#ifdef IS_T
											i.uv.xy = v.uv * _OverallScale;
					#else

					#ifdef USE_WT
											i.uv.xy = float2(worldPos.x + _MainTex_ST.z, worldPos.z + _MainTex_ST.w) * _OverallScale * 0.05;
					#else
											i.uv.xy = TRANSFORM_TEX(v.uv, _MainTex) * _OverallScale;
					#endif
					#endif
											i.uv.zw = TRANSFORM_TEX(v.uv, _DetailTex);


					#ifdef LIGHTMAP_ON
											i.lmap = v.uv.xy * unity_LightmapST.xy + unity_LightmapST.zw;
					#endif

										#ifdef USE_RT
											if (_HasRT == 1)
											{
												// .b(lue) = Sand Dig / .r(ed) = Sand To Ice / .g(reen) = Sand Mount
												rippleMain = tex2Dlod(_GlobalEffectRT, float4(uv, 0, 0));
												rippleMainAdditional = tex2Dlod(_GlobalEffectRTAdditional, float4(uvAdd, 0, 0));
											}

										#ifdef IS_ROCK
										#else
											float2 uvGradient = smoothstep(0, 5, length(max(abs(_Position.xz - worldPos.xz) - _OrthographicCamSize + 5, 0.0)));
											uvRTValue = saturate(uvGradient.x);

											if (v.color.b > 0.95 && v.color.g < 0.05)
											{
											}
											else
											{
												ripples = lerp(rippleMain.x, rippleMainAdditional.x, uvRTValue);
												ripples2 = lerp(rippleMain.y, rippleMainAdditional.y, uvRTValue);
												ripples3 = lerp(rippleMain.z, rippleMainAdditional.z, uvRTValue);
											}
										#endif

										#endif

											float slopeValue = 0;
										#ifdef IS_T
											half4 splat_control = tex2Dlod(_Control0, float4(i.uv.xy, 0,0));
					#ifdef USE_COMPLEX_T
											half4 splat_control1 = tex2Dlod(_Control1, float4(i.uv.xy, 0, 0));
					#endif

					#ifdef USE_COMPLEX_T
											float rockValue = saturate(1 - splat_control.r * _Metallic0 - splat_control.g * _Metallic1 - splat_control.b * _Metallic2 - splat_control.a * _Metallic3
												- splat_control1.r * _Metallic4 - splat_control1.g * _Metallic5 - splat_control1.b * _Metallic6 - splat_control1.a * _Metallic7
												- ripples);
					#else
											float rockValue = saturate(1 - splat_control.r * _Metallic0 - splat_control.g * _Metallic1 - splat_control.b * _Metallic2 - splat_control.a * _Metallic3
												- ripples);
					#endif

											float sandHeightNew = _Mask0.SampleLevel(my_linear_repeat_sampler, float4(i.uv.xy, 0, 0) * _HeightScale * _SandScale, 0).r;
											sandHeightNew = lerp(sandHeightNew, _Mask1.SampleLevel(my_linear_repeat_sampler, float4(i.uv.xy, 0, 0) * _HeightScale * _SandScale, 0).r, saturate(splat_control.g * (1 - _Metallic1)));
											sandHeightNew = lerp(sandHeightNew, _Mask2.SampleLevel(my_linear_repeat_sampler, float4(i.uv.xy, 0, 0) * _HeightScale * _SandScale, 0).r, saturate(splat_control.b * (1 - _Metallic2)));
											sandHeightNew = lerp(sandHeightNew, _Mask3.SampleLevel(my_linear_repeat_sampler, float4(i.uv.xy, 0, 0) * _HeightScale * _SandScale, 0).r, saturate(splat_control.a * (1 - _Metallic3)));
					#ifdef USE_COMPLEX_T
											sandHeightNew = lerp(sandHeightNew, _Mask4.SampleLevel(my_linear_repeat_sampler, float4(i.uv.xy, 0, 0) * _HeightScale * _SandScale, 0).r, saturate(splat_control1.r * (1 - _Metallic4)));
											sandHeightNew = lerp(sandHeightNew, _Mask5.SampleLevel(my_linear_repeat_sampler, float4(i.uv.xy, 0, 0) * _HeightScale * _SandScale, 0).r, saturate(splat_control1.g * (1 - _Metallic5)));
											sandHeightNew = lerp(sandHeightNew, _Mask6.SampleLevel(my_linear_repeat_sampler, float4(i.uv.xy, 0, 0) * _HeightScale * _SandScale, 0).r, saturate(splat_control1.b * (1 - _Metallic6)));
											sandHeightNew = lerp(sandHeightNew, _Mask7.SampleLevel(my_linear_repeat_sampler, float4(i.uv.xy, 0, 0) * _HeightScale * _SandScale, 0).r, saturate(splat_control1.a * (1 - _Metallic7)));
					#endif

											float sandHeight = sandHeightNew;
										#else
											float rockValue = 0;
					#ifdef USE_INTER
					#ifdef IS_ADD			
											// custom intersection and slope value //
											float4 midPoint = mul(unity_ObjectToWorld, float4(0.0, 0.0, 0.0, 1.0));

											float4 quaternion = float4(v.uv6.x, -v.uv6.y, -v.uv7.x, -v.uv7.y);
											float3 offsetPoint = worldPos.xyz - midPoint;

											float3 rotatedVert = rotateVector(quaternion, -offsetPoint);
											float manualLerp = 0;

											 manualLerp = v.uv4.x;

											rotatedVert = RotateAroundZInDegrees(float4(rotatedVert, 0), lerp(6,-6, (manualLerp)));
											rotatedVert = RotateAroundXInDegrees(float4(rotatedVert, 0), lerp(-55,55, (manualLerp))) + midPoint;

											slopeValue = ((v.color.a) - (rotatedVert.y - 0.5));

											if (slopeValue > 0.0)
											{
												v.color.g = saturate(v.color.g + saturate(slopeValue * 3));
												v.color.b = saturate(v.color.b + saturate(slopeValue * 3));
											}
					#endif
					#endif
											if (v.color.b > 0.6 && v.color.g < 0.4)
											{
												rockValue = saturate(1 - v.color.b);
											}
											else
											{
												rockValue = saturate((v.color.g + v.color.b) / 2 - ripples);
											}

										#ifdef USE_WC
											float sandHeight = tex2Dlod(_SandHeight, float4(originalPos.xz, 0, 0) * _HeightScale * 0.1 * _SandScale).r;
										#else
											float sandHeight = tex2Dlod(_SandHeight, float4(i.uv.xy, 0, 0) * _HeightScale * _SandScale).r;
										#endif

										#endif

					#ifdef IS_SAND
											rockValue = 1;
					#endif

											i.normalDir = normalize(mul(float4(v.normal, 0.0), unity_WorldToObject));
										#ifdef IS_ROCK
										#else

										#ifdef USE_RT
											if (_HasRT == 1)
											{
												if (v.color.b > 0.95 && v.color.g < 0.05)
												{
													i.normal = normalize(v.normal);
												}
												else
												{
													i.normal = normalize(lerp(v.normal, calcNormal(uv, _GlobalEffectRT).rbb, rockValue));
												}
											}
											else
											{
												i.normal = normalize(v.normal);
											}
										#else
											i.normal = normalize(v.normal);
										#endif

										#ifdef IS_ADD
											float3 newNormal = normalize(i.normalDir);
											worldPos += ((float4(0, -_RemoveSandStrength, 0, 0) * _UpVector - newNormal * _RemoveSandStrength * _NormalVector) * ripples3 + (float4(0, _AddSandStrength * sandHeight, 0, 0) * _UpVector + newNormal * _AddSandStrength * sandHeight * _NormalVector) * ripples2 * saturate(1 - ripples3)) * saturate(rockValue * 3);
											worldPos += (float4(0, _DisplacementOffset, 0, 0) * _UpVector + newNormal * _DisplacementOffset * _NormalVector) * saturate(rockValue * 2.5);
											worldPos += (float4(0, 2 * _DisplacementStrength * sandHeight , 0, 0) * _UpVector) + (newNormal * 2 * _DisplacementStrength * sandHeight * _NormalVector * clamp(slopeValue * 20, 1, 2)) * saturate(saturate(rockValue * 2.5));

											worldPos = lerp(worldPos, mul(unity_ObjectToWorld, v.vertex), saturate(v.color.g - saturate(v.color.r + v.color.b)));

											v.vertex.xyz = lerp(mul(unity_WorldToObject, float4(originalPos, 1)).xyz, mul(unity_WorldToObject, float4(worldPos, 1)).xyz, rockValue);
										#else
											float3 newNormal = normalize(i.normalDir);
											worldPos += ((float4(0, -_RemoveSandStrength, 0, 0) * _UpVector - newNormal * _RemoveSandStrength * _NormalVector) * ripples3 + (float4(0, _AddSandStrength * sandHeight, 0, 0) * _UpVector + newNormal * _AddSandStrength * sandHeight * _NormalVector) * ripples2 * saturate(1 - ripples3)) * saturate(rockValue * 3);
											worldPos += (float4(0, _DisplacementOffset, 0, 0) * _UpVector + newNormal * _DisplacementOffset * _NormalVector) * saturate(rockValue * 2.5);
											worldPos += (float4(0, 2 * _DisplacementStrength * sandHeight, 0, 0) * _UpVector) + (newNormal * 2 * _DisplacementStrength * sandHeight * _NormalVector) * saturate(saturate(rockValue * 2.5));

											worldPos = lerp(worldPos, mul(unity_ObjectToWorld, v.vertex), saturate(v.color.g - saturate(v.color.r + v.color.b)));

											v.vertex.xyz = lerp(mul(unity_WorldToObject, float4(originalPos, 1)).xyz, mul(unity_WorldToObject, float4(worldPos, 1)).xyz, rockValue);

										#endif
										#endif

										#ifdef USE_RT
											if (_HasRT == 1)
											{
												v.color = lerp(v.color, saturate(float4(1, 0, 0, 1)), ripples);
											}
										#endif
											i.vertex = UnityObjectToClipPos(v.vertex);

											float4 objCam = mul(unity_WorldToObject, float4(_WorldSpaceCameraPos, 1.0));
											float3 viewDir = v.vertex.xyz - objCam.xyz;

										#ifdef IS_T
											//float4 finalTangent = v.tangent;
											float4 finalTangent = float4 (1.0, 0.0, 0.0, -1.0);
											finalTangent.xyz = finalTangent.xyz - v.normal * dot(v.normal, finalTangent.xyz); // Orthogonalize tangent to normal.

											float tangentSign = finalTangent.w * unity_WorldTransformParams.w;
											float3 bitangent = cross(v.normal.xyz, finalTangent.xyz) * tangentSign;

											i.viewDir = float3(
												dot(viewDir, finalTangent.xyz),
												dot(viewDir, bitangent.xyz),
												dot(viewDir, v.normal.xyz)
												);

											i.worldPos.xyz = mul(unity_ObjectToWorld, v.vertex);
											i.tangent = finalTangent;

										#else
											float tangentSign = v.tangent.w * unity_WorldTransformParams.w;
											float3 bitangent = cross(v.normal.xyz, v.tangent.xyz) * tangentSign;

											i.viewDir = float3(
												dot(viewDir, v.tangent.xyz),
												dot(viewDir, bitangent.xyz),
												dot(viewDir, v.normal.xyz)
												);

											i.worldPos.xyz = mul(unity_ObjectToWorld, v.vertex);
											i.tangent = v.tangent;
											float4 finalTangent = v.tangent;
										#endif

											i.color = v.color;


										#ifdef SHADOWS_SCREEN
											i._ShadowCoord = ComputeScreenPos(i.vertex);
										#endif

					#ifdef IS_ADD
					#ifdef USE_INTER
											// NORMAL BASED ON HIT NORMAL //
											 i.normalDir = lerp(i.normalDir, normalize(float3(v.uv3.x, v.uv3.y, v.uv4.y)), saturate(slopeValue));
											 i.normal = normalize(float3(v.uv3.x, v.uv3.y, v.uv4.y));
					#endif
					#endif

					#ifndef USE_LOW
											parallax_vert(v.vertex, v.normal, finalTangent, i.eye);
					#endif

					#ifdef SHADER_API_MOBILE
											TRANSFER_VERTEX_TO_FRAGMENT(i);
					#endif
											UNITY_TRANSFER_FOG(i,i.vertex);
											return i;
										}


										float4 frag(InterpolatorsVertex i) : SV_Target
										{

					#ifdef USE_VR
													UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i)
					#endif


													// Linear to Gamma //
										half gamma = 0.454545;

										half4 lightColor = half4(1, 1, 1, 1);
										float4 bakedColorTex = 0;
										float shadowmap = 0;

					#ifdef LIGHTMAP_ON
										bakedColorTex = UNITY_SAMPLE_TEX2D(unity_Lightmap, i.lmap);
										shadowmap = pow(DecodeLightmap(bakedColorTex),2);
					#else

										shadowmap = SHADOW_ATTENUATION(i);
					#endif

					#ifdef IS_UNLIT
										lightColor = half4(1, 1, 1, 1);
					#else

					#ifdef LIGHTMAP_ON
										lightColor = bakedColorTex;
					#else
										lightColor = _LightColor0;
					#endif
					#endif

					#if !UNITY_COLORSPACE_GAMMA
										//_Color = pow(_Color, gamma);
										_TransitionColor = pow(_TransitionColor, gamma);
										_MountColor = pow(_MountColor, gamma);
										_BotColor = pow(_BotColor, gamma);
										_RockTint = pow(_RockTint, 0.55) * 1.5;
										_RockTrail = pow(_RockTrail, gamma);
										//_ProjectedShadowColor = pow(_ProjectedShadowColor, gamma);
										_SpecColor = pow(_SpecColor, gamma);
										_RimColor = pow(_RimColor, gamma);
										lightColor = pow(lightColor, gamma);
										_LittleSpecForce = pow(_LittleSpecForce, gamma) * 1.5;

										#ifdef IS_T
															_Specular0 = pow(_Specular0, gamma);
															_Specular1 = pow(_Specular1, gamma);
															_Specular2 = pow(_Specular2, gamma);
															_Specular3 = pow(_Specular3, gamma);
										#ifdef USE_COMPLEX_T
															_Specular4 = pow(_Specular4, gamma);
															_Specular5 = pow(_Specular5, gamma);
															_Specular6 = pow(_Specular6, gamma);
															_Specular7 = pow(_Specular7, gamma);
										#endif
										#endif

										#endif

															// Terrain Setup //
										#ifdef IS_T
															int maxN = 4;

															Texture2D _MaskArray[8];
															_MaskArray[0] = _Mask0;
															_MaskArray[1] = _Mask1;
															_MaskArray[2] = _Mask2;
															_MaskArray[3] = _Mask3;

															half _MetallicArray[8];
															_MetallicArray[0] = _Metallic0;
															_MetallicArray[1] = _Metallic1;
															_MetallicArray[2] = _Metallic2;
															_MetallicArray[3] = _Metallic3;

															Texture2D _SplatArray[8];
															_SplatArray[0] = _Splat0;
															_SplatArray[1] = _Splat1;
															_SplatArray[2] = _Splat2;
															_SplatArray[3] = _Splat3;

															float4 _SplatSTArray[8];
															_SplatSTArray[0] = _Splat0_ST;
															_SplatSTArray[1] = _Splat1_ST;
															_SplatSTArray[2] = _Splat2_ST;
															_SplatSTArray[3] = _Splat3_ST;

															float4 _SpecularArray[8];
															_SpecularArray[0] = _Specular0;
															_SpecularArray[1] = _Specular1;
															_SpecularArray[2] = _Specular2;
															_SpecularArray[3] = _Specular3;

															Texture2D _NormalArray[8];
															_NormalArray[0] = _Normal0;
															_NormalArray[1] = _Normal1;
															_NormalArray[2] = _Normal2;
															_NormalArray[3] = _Normal3;

															half _NormalScaleArray[8];
															_NormalScaleArray[0] = _NormalScale0;
															_NormalScaleArray[1] = _NormalScale1;
															_NormalScaleArray[2] = _NormalScale2;
															_NormalScaleArray[3] = _NormalScale3;

										#ifdef USE_COMPLEX_T
															maxN = 8;

															_MaskArray[4] = _Mask4;
															_MaskArray[5] = _Mask5;
															_MaskArray[6] = _Mask6;
															_MaskArray[7] = _Mask7;

															_MetallicArray[4] = _Metallic4;
															_MetallicArray[5] = _Metallic5;
															_MetallicArray[6] = _Metallic6;
															_MetallicArray[7] = _Metallic7;

															_SplatArray[4] = _Splat4;
															_SplatArray[5] = _Splat5;
															_SplatArray[6] = _Splat6;
															_SplatArray[7] = _Splat7;

															_SplatSTArray[4] = _Splat4_STn;
															_SplatSTArray[5] = _Splat5_STn;
															_SplatSTArray[6] = _Splat6_STn;
															_SplatSTArray[7] = _Splat7_STn;

															_SpecularArray[4] = _Specular4;
															_SpecularArray[5] = _Specular5;
															_SpecularArray[6] = _Specular6;
															_SpecularArray[7] = _Specular7;

															_NormalArray[4] = _Normal4;
															_NormalArray[5] = _Normal5;
															_NormalArray[6] = _Normal6;
															_NormalArray[7] = _Normal7;

															_NormalScaleArray[4] = _NormalScale4;
															_NormalScaleArray[5] = _NormalScale5;
															_NormalScaleArray[6] = _NormalScale6;
															_NormalScaleArray[7] = _NormalScale7;
										#endif
										#endif
																		float uvRTValue = 0;
																		float2 uv = i.worldPos.xz - _Position.xz;
																		uv = uv / (_OrthographicCamSize * 2);
																		uv += 0.5;

																		float2 uvAdd = i.worldPos.xz - _PositionAdd.xz;
																		uvAdd = uvAdd / (_OrthographicCamSizeAdditional * 2);
																		uvAdd += 0.5;

																		float3 rippleMain = 0;
																		float3 rippleMainAdditional = 0;
																		float3 calculatedNormal = 0;
																		float3 calculatedNormalAdd = 0;

																		float ripples = 0;
																		float ripples2 = 0;
																		float ripples3 = 0;
																		float rockValue = 1;
																		float sandHeight = 0;
																		float sandHeightReal = 0;

										#ifdef USE_PR
																		float dist = clamp(lerp(0, 1, (distance(_WorldSpaceCameraPos, i.worldPos) - _ProceduralDistance) / max(_ProceduralStrength, 0.05)), 0, 1);
										#endif

										#ifdef IS_ROCK
										#else
																		sandHeight = saturate(_SandTransition.Sample(my_linear_repeat_sampler, i.uv * _TransitionScale * _SandScale).r);

										#ifdef USE_WC

										#ifdef USE_PR
																		sandHeightReal = lerp(tex2D(_SandHeight, float2(i.worldPos.x, i.worldPos.z) * _HeightScale * 0.1 * _SandScale).r, tex2DStochastic(_SandHeight, float2(i.worldPos.x, i.worldPos.z) * _HeightScale * 0.1 * _SandScale).r, dist);
										#else
																		sandHeightReal = tex2D(_SandHeight, float2(i.worldPos.x, i.worldPos.z) * _HeightScale * 0.1 * _SandScale).r;
										#endif

										#else

										#ifdef USE_PR
																		sandHeightReal = lerp(tex2D(_SandHeight, i.uv.xy * _HeightScale * _SandScale).r, tex2DStochastic(_SandHeight, i.uv.xy * _HeightScale * _SandScale).r, dist);
										#else
																		sandHeightReal = tex2D(_SandHeight, i.uv.xy * _HeightScale * _SandScale).r;
										#endif
										#endif


										#endif


										#ifdef IS_T
										#else
										#ifdef IS_SAND
																		rockValue = 1;
										#else
																		rockValue = saturate(pow(saturate(i.color.g), 1 + clamp(abs((sandHeight - 0.5) * 20) * -_TransitionPower * (saturate(i.color.g)), -1, 1)) * 1.25);
										#endif
										#endif

										#ifdef IS_T
										#else
										#ifndef USE_LOW
																		// OCCLUSION PARALLAX //
																		float2 offsetParallax = parallax_offset(_Parallax, i.eye, i.uv * _RockScale,
																			_ParallaxMapRock, _ParallaxMinSamples, _ParallaxMaxSamples);
																		i.uv.xy = lerp(i.uv.xy + offsetParallax, i.uv.xy, rockValue);
										#endif
										#endif


															#ifdef IS_ROCK
															#else
															#ifdef USE_RT

																		if (_HasRT == 1)
																		{
																			rippleMain = tex2D(_GlobalEffectRT, uv);
																			rippleMainAdditional = tex2D(_GlobalEffectRTAdditional, uvAdd);

																			float2 uvGradient = smoothstep(0, 5, length(max(abs(_Position.xz - i.worldPos.xz) - _OrthographicCamSize + 5, 0.0)));
																			uvRTValue = saturate(uvGradient.x);
																			ripples = lerp(rippleMain.x, rippleMainAdditional.x, uvRTValue);
																		}
															#endif
															#endif



															#ifdef IS_T
																		half4 splat_control = tex2D(_Control0, i.uv);
										#ifdef USE_COMPLEX_T
																		half4 splat_control1 = tex2D(_Control1, i.uv);
										#endif

																		splat_control.r = lerp(splat_control.r, 0, saturate(ripples));
																		splat_control.g = lerp(splat_control.g, 1, saturate(ripples));
										#ifdef USE_COMPLEX_T
																		splat_control1.r = lerp(splat_control1.r, 0, saturate(ripples));
																		splat_control1.g = lerp(splat_control1.g, 1, saturate(ripples));
										#endif

										#ifdef USE_COMPLEX_T
																		float splatOverall = saturate(1 - splat_control.r * _Metallic0 - splat_control.g * _Metallic1 - splat_control.b * _Metallic2 - splat_control.a * _Metallic3
																			- splat_control1.r * _Metallic4 - splat_control1.g * _Metallic5 - splat_control1.b * _Metallic6 - splat_control1.a * _Metallic7);
										#else								
																		float splatOverall = saturate(1 - splat_control.r * _Metallic0 - splat_control.g * _Metallic1 - splat_control.b * _Metallic2 - splat_control.a * _Metallic3);
										#endif

										#ifdef IS_SAND
																		rockValue = 1;
										#else
																		rockValue = pow(splatOverall, 1 + clamp(abs((sandHeight - 0.5) * 20) * -_TransitionPower * splatOverall, -1, 1));
										#endif

																		float3 originalPos = i.worldPos;

																		// Terrain Setup //
																		float _splatControlArray[8];
																		_splatControlArray[0] = splat_control.r;
																		_splatControlArray[1] = splat_control.g;
																		_splatControlArray[2] = splat_control.b;
																		_splatControlArray[3] = splat_control.a;
										#ifdef USE_COMPLEX_T
																		_splatControlArray[4] = splat_control1.r;
																		_splatControlArray[5] = splat_control1.g;
																		_splatControlArray[6] = splat_control1.b;
																		_splatControlArray[7] = splat_control1.a;
										#endif


																		// OFFSET COORDINATES //

																		float2 terrainOffsetST[8];

																		for (int n = 0; n < maxN; n++)
																		{
																			if (n <= 3)
																			{
																				terrainOffsetST[n] = (_TerrainScale.xz / _SplatSTArray[n].xy);
																			}
																			else
																			{
																				terrainOffsetST[n] = _SplatSTArray[n].xy;
																			}
																		}

										#ifndef USE_LOW

																		// OCCLUSION PARALLAX //
																		float2 offsetParallax = float2(0, 0);

																		for (int n = 0; n < maxN; n++)
																		{
																			if (saturate(_splatControlArray[n] * (_MetallicArray[n])) > 0.5)
																			{
																				offsetParallax += parallax_offset(_Parallax, i.eye, i.uv * _RockScale * terrainOffsetST[n],
																					_MaskArray[n], _ParallaxMinSamples, _ParallaxMaxSamples);
																			}
																		}

																		i.uv.xy = i.uv.xy + offsetParallax;
										#endif

																		// rock Height Map //

																		float3 rockHeight = 1;
																		for (int n = 0; n < maxN; n++)
																		{
																			rockHeight = lerp(rockHeight, _MaskArray[n].Sample(my_linear_repeat_sampler, i.uv * _RockScale * terrainOffsetST[n]).r, saturate(_splatControlArray[n] * _MetallicArray[n]));
																		}

																		// sand Splat //

																		float3 sandSplatArray[8];
																		for (int n = 0; n < maxN; n++)
																		{
																			sandSplatArray[n] = _SplatArray[n].Sample(my_linear_repeat_sampler, i.uv * terrainOffsetST[n] * 0.01);
																			sandSplatArray[n] = lerp(pow(sandSplatArray[n], 0.4) * _SpecularArray[n] * 2, lerp(1, saturate(pow(sandSplatArray[n], 2)), _MainTexMult), 1 - _MetallicArray[n]);
																		}

																		// SAND NORMAL //

																		float3 sandNormalArray[8];
																		for (int n = 0; n < maxN; n++)
																		{
																			sandNormalArray[n] = UnpackScaleNormal(_NormalArray[n].Sample(my_linear_repeat_sampler, i.uv * terrainOffsetST[n] * lerp(_SandScale, _RockScale, _MetallicArray[n])), _NormalScaleArray[n] * 2);
																		}

																		// SAND NORMAL //
																		float3 normal = 0;
																		for (int n = 0; n < maxN; n++)
																		{
																			normal = lerp(normal, sandNormalArray[n], saturate(_splatControlArray[n] * (1 - _MetallicArray[n])));
																		}

																		// ROCK NORMAL //
																		half3 normalTex = UnpackScaleNormal(_NormalArray[0].Sample(my_linear_repeat_sampler, i.uv * _RockScale), _NormalScale);
																		for (int n = 0; n < maxN; n++)
																		{
																			normalTex = lerp(normalTex, sandNormalArray[n] * _NormalScaleArray[n], _splatControlArray[n] * _MetallicArray[n]);
																		}

																		// sandHeightReal //
																		float sandHeightNew = 0;
																		for (int n = 0; n < maxN; n++)
																		{
																			sandHeightNew = lerp(sandHeightNew, _MaskArray[n].Sample(my_linear_repeat_sampler, originalPos.xz * _HeightScale * _SandScale * 0.01).r, saturate(_splatControlArray[n] * (1 - _MetallicArray[n])));
																		}

																		sandHeightReal = sandHeightNew;


															#else
																		float3 normal = UnpackScaleNormal(tex2D(_BumpMap, (i.uv) * _SandNormalScale * _SandScale), _NormalMultiplier * 2).rgb * rockValue - i.normal;
																		half3 normalTex = UnpackScaleNormal(tex2D(_NormalTex, i.uv * _RockScale), _NormalScale);
															#endif

															#ifdef IS_T

																		half4 c = _Specular0;

																		for (int n = 0; n < maxN; n++)
																		{
																			c = lerp(c, _SpecularArray[n], _splatControlArray[n] * (rockValue) * (1 - _MetallicArray[n]));
																		}
															#else
																		half4 c = _Color;
															#endif

															#ifdef IS_ROCK
															#else
															#ifdef USE_RT

																		if (_HasRT == 1)
																		{
																			if (i.color.b > 0.95 && i.color.g < 0.05)
																			{
																			}
																			else
																			{
																				ripples2 = lerp(rippleMain.y, rippleMainAdditional.y, uvRTValue);
																				ripples3 = lerp(rippleMain.z, rippleMainAdditional.z, uvRTValue);
																				calculatedNormal = calcNormal(uv, _GlobalEffectRT);
																				calculatedNormal.y = lerp(calculatedNormal.y, 0, saturate(ripples3 * 5));
																				calculatedNormalAdd = calcNormal(uvAdd, _GlobalEffectRTAdditional);
																				calculatedNormal = lerp(calculatedNormal, 0, uvRTValue);
																			}

																			c = lerp(
																				c,
																				_BotColor * 2 - 1,
																				ripples3);

																			c = lerp(
																				c,
																				c + _MountColor,
																				saturate(saturate(ripples2 - ripples3) * saturate(sandHeight + 0.5) * 1));

																			c.rgb = lerp(c.rgb, c.rgb * _BotColor, clamp(ripples3 * saturate(calculatedNormalAdd.r - 0.15) * _NormalRTStrength * 1, 0, 1));
																		}
															#endif	
																		c.rgb = c.rgb * lightColor;
																		c.rgb = lerp(c.rgb * _Color * _DisplacementColorMult, c.rgb, sandHeightReal);
															#endif	
																		float3 normalEffect = i.normal;
															#ifdef IS_T
																		// SAND LERP //

																		float3 sandColor = sandSplatArray[0];

																		for (int n = 0; n < maxN; n++)
																		{
																			sandColor = lerp(sandColor, sandSplatArray[n], saturate(_splatControlArray[n] * (1 - _MetallicArray[n])));
																		}

																		c.rgb *= sandColor;
															#else
																		c *= lerp(1, saturate(pow(tex2D(_MainTex, i.uv * _SandScale) + _MainTexMult * 0.225, 2)), _MainTexMult);
															#endif
										#ifdef USE_FOG
																		half3 flowVal = (_FlowTex.Sample(my_bilinear_repeat_sampler, i.uv)) * _FlowMultiplier;

																		float dif1 = frac(_Time.y * 0.15 + 0.5);
																		float dif2 = frac(_Time.y * 0.15);

																		half lerpVal = abs((0.5 - dif1) / 0.5);

																		//_FogDirection
																		half3 col1 = _FogTex.Sample(my_bilinear_repeat_sampler, i.uv * _FogScale - flowVal.xy * dif1 + (normalize(_FogDirection.xy) * _Time.y * -0.02 * _FogDirection.z));
																		half3 col2 = _FogTex.Sample(my_bilinear_repeat_sampler, i.uv * _FogScale - flowVal.xy * dif2 + (normalize(_FogDirection.xy) * _Time.y * -0.02 * _FogDirection.z));

																		half3 fogFlow = lerp(col1, col2, lerpVal);
																		fogFlow = abs(pow(fogFlow, 5));
										#endif

																		float3 viewDirTangent = i.viewDir;
															#ifdef IS_T
																		// ROCK LERP //
																		half4 RockTex = half4(sandSplatArray[0],1);

																		for (int n = 0; n < maxN; n++)
																		{
																			RockTex.rgb = lerp(RockTex, sandSplatArray[n], _splatControlArray[n] * _MetallicArray[n]);
																		}
															#else
										#ifdef USE_PR
																		half4 RockTex = half4(lerp(1, pow(lerp(tex2D(_RockTex, i.uv * _RockScale), tex2DStochastic(_RockTex, i.uv * _RockScale), dist), _RockSaturation), _RockTint.a) * _RockTint.rgb * 2, 1);
										#else
																		half4 RockTex = half4(lerp(1, pow(tex2D(_RockTex, i.uv * _RockScale), _RockSaturation), _RockTint.a) * _RockTint.rgb * 2, 1);
										#endif

										#endif
																		RockTex.rgb = RockTex.rgb * lightColor;

																		float3 viewDirection = normalize(_WorldSpaceCameraPos - i.worldPos.xyz);
																		float3 normalDirection = normalize(i.normalDir);
										#ifdef IS_T
																		float3 normalDirectionWithNormal = normalize(i.normalDir) + normalize(i.normalDir) * lerp(normal , normalTex, saturate(1 - rockValue));
										#else
																		float3 normalDirectionWithNormal = normalize(i.normalDir) + normalize(i.normalDir) * (lerp(abs(tex2D(_BumpMap, i.uv * _SandNormalScale * _SandScale)) * _NormalMultiplier, tex2D(_NormalTex, i.uv * _RockScale) * _NormalScale, saturate(1 - rockValue)));
										#endif

																		half fresnelValue = lerp(0, 1, saturate(dot(normalDirection, viewDirection)));
																		_OffsetScale = max(0, _OffsetScale);

																		half parallax = 0;

																		float3 newRoughnessTex = tex2D(_Roughness, i.uv * _RockScale).rgb;
																		float alphaRock = 1;
																		alphaRock = saturate(1 - saturate((newRoughnessTex.r + newRoughnessTex.g + newRoughnessTex.b) / 3));

																		half4 blended = 0;

																		if (i.color.b > 0.95 && i.color.g < 0.05)
																		{
																			blended = RockTex;
																		}
																		else
																		{
																			blended = RockTex - saturate(_RockTrail * ripples3 * _RockTrail.a);
																		}

																		blended.rgb = lerp(blended.rgb + newRoughnessTex * 0.4, blended.rgb, alphaRock);
										#ifdef IS_T
																		blended.rgb = blended.rgb * saturate(rockHeight);
										#else
																		blended.rgb = blended.rgb * saturate(_ParallaxMapRock.Sample(my_linear_repeat_sampler, i.uv * _RockScale) + 0.25);
										#endif

																		float3 albedo = 1;
															#ifdef	IS_ROCK
																		albedo = blended * 0.5;
															#else
															#ifdef IS_ADD
																		albedo = lerp(c.rgb * _TransitionColor, c.rgb, saturate(pow(rockValue, 3)));
															#else
																		albedo = lerp(blended * 0.5 , c.rgb, saturate(pow(rockValue, 3)));
															#endif
															#endif

																		float3 lightDirection;
																		float attenuation;
																		half3 worldNormal;
										#if !UNITY_COLORSPACE_GAMMA
																		shadowmap = pow(shadowmap, gamma);
										#endif
																		// basic lighting from sun pos //
																		float diff = 0;

																		float3 N = normalize(normalDirection);
																		float3 fragmentToLight = _WorldSpaceLightPos0.xyz - i.worldPos.xyz;
																		//float  distanceToLight = length(fragmentToLight);
																		//float  atten = pow(2, -0.1 * distanceToLight * distanceToLight) * _WorldSpaceLightPos0.w + 1 - _WorldSpaceLightPos0.w;
																		float3 L = normalize(fragmentToLight) * _WorldSpaceLightPos0.w + normalize(_WorldSpaceLightPos0.xyz) * (1 - _WorldSpaceLightPos0.w);

																		diff = dot(N, L);

																		worldNormal.x = dot(normalDirection.x, lerp(normalTex, UnpackScaleNormal(tex2D(_BumpMap, i.uv * _SandScale), _NormalMultiplier).rgb * 3 , rockValue));
																		worldNormal.y = dot(normalDirection.y, lerp(normalTex, UnpackScaleNormal(tex2D(_BumpMap, i.uv * _SandScale), _NormalMultiplier).rgb * 3 , rockValue));
																		worldNormal.z = dot(normalDirection.z, lerp(normalTex, UnpackScaleNormal(tex2D(_BumpMap, i.uv * _SandScale), _NormalMultiplier).rgb * 3 , rockValue));

																		_ShininessRock = max(0.1, _ShininessRock);
																		_ShininessSand = max(0.1, _ShininessSand);

																		if (0.0 == _WorldSpaceLightPos0.w) // directional light
																		{
																			attenuation = 1.0; // no attenuation
																			lightDirection = normalize(_WorldSpaceLightPos0.xyz);
																		}
																		else // point or spot light
																		{
																			float3 vertexToLightSource =
																				_WorldSpaceLightPos0.xyz - i.worldPos.xyz;
																			float distance = length(vertexToLightSource);
																			attenuation = 1.0 / distance; // linear attenuation 
																			lightDirection = normalize(vertexToLightSource);
																		}

										#ifdef LIGHTMAP_ON
																		lightDirection = normalize(float3(0,1,0));
										#endif

																		float3 ambientLighting = UNITY_LIGHTMODEL_AMBIENT.rgb;
										#if !UNITY_COLORSPACE_GAMMA
																		ambientLighting = pow(ambientLighting, gamma);
										#endif
																		ambientLighting *= _Color.rgb;

																		float3 diffuseReflection =
																			attenuation * lightColor * _Color.rgb
																			* max(0.0, dot(normalDirection, lightDirection));

																		float3 specularReflection;
																		if (dot(normalDirection, lightDirection) < 0.0)
																			// light source on the wrong side
																		{
																			specularReflection = float3(0.0, 0.0, 0.0);
																			// no specular reflection
																		}
																		else // light source on the right side
																		{
																			specularReflection = attenuation * lightColor
																				* _SpecColor.rgb * pow(max(0.0, dot(
																					reflect(-lightDirection, normalDirection),
																					viewDirection)), lerp(_ShininessRock, _ShininessSand, rockValue));
																		}

																		float NdotL = 1;
										#ifdef LIGHTMAP_ON
																		NdotL = 1;
										#else
																		NdotL = 0.5 * (dot(_WorldSpaceLightPos0.xyz, normalDirectionWithNormal)) + 0.5; // Lambert Normal adjustement
																		NdotL = lerp(NdotL,NdotL + saturate(sandHeightReal) * 0.1 * _DisplacementStrength - saturate(1 - sandHeightReal) * 0.1 * _DisplacementStrength, rockValue * _DisplacementShadowMult);
																		NdotL = saturate(NdotL);
										#endif

																		float lightIntensity = smoothstep(0.1 + _LightOffset * clamp((_LightHardness + 0.5) * 2,1,10), (0.101 + _LightOffset) * clamp((_LightHardness + 0.5) * 2, 1, 10), NdotL * shadowmap);
																		_SpecForce = max(0.1, _SpecForce);

										#ifdef IS_UNLIT
																		lightIntensity = 1;
										#endif

																		half3 shadowmapColor = lerp(_ProjectedShadowColor, 1, saturate(lightIntensity));
										#ifndef LIGHTMAP_ON
																		float zDist = dot(_WorldSpaceCameraPos - i.worldPos, UNITY_MATRIX_V[2].xyz);
																		float fadeDist = saturate(1 - UnityComputeShadowFade(UnityComputeShadowFadeDistance(i.worldPos, zDist)) * diff);
																		shadowmapColor = lerp(1, shadowmapColor, saturate(fadeDist));
										#endif

																		albedo.xyz = albedo.xyz * saturate(shadowmapColor);

															#ifdef IS_T
																		float4 specGloss = pow(tex2D(_SpecGlossMap, i.uv * 2 * (_TerrainScale.xz / _Splat0_ST.xy) * _SandScale), _SpecForce) * _SpecMult;
																		float4 littleSpec = tex2D(_LittleSpec, i.uv * (_TerrainScale.xz / _Splat0_ST.xy) * _LittleSpecSize * _SandScale) * saturate(1 - ripples3) * saturate(lightIntensity);
															#else
																		float4 specGloss = pow(tex2D(_SpecGlossMap, i.uv * 2 * _SandScale), _SpecForce) * _SpecMult;
																		float4 littleSpec = tex2D(_LittleSpec, i.uv * _SandScale * _LittleSpecSize) * saturate(1 - ripples3) * saturate(lightIntensity);

															#endif

															#ifdef	IS_ROCK
																		half rougnessTex = newRoughnessTex.r * 2 * _RoughnessStrength * saturate(1 - ripples3) * 1;
															#else
																		half rougnessTex = newRoughnessTex.r * 2 * _RoughnessStrength * saturate(1 - ripples3) * (1 - rockValue);
															#endif

										#ifdef IS_T
																		rougnessTex = rougnessTex * saturate(normalTex + 0.25);
										#else
																		rougnessTex = rougnessTex * saturate(UnpackScaleNormal(tex2D(_NormalTex, i.uv * _RockScale), _NormalScale) + 0.25);
										#endif

															#ifdef	IS_ROCK
																		specGloss.r = specGloss.r * saturate(normal);
															#else
															#ifdef USE_RT
																		if (_HasRT == 1)
																		{
																			specGloss.r = specGloss.r * saturate(normal) + saturate(ripples3 * 30) * lerp(0, 1, saturate(saturate(1 - ripples3 * 5) * calculatedNormal.x * reflect(-lightDirection, normalDirection)).x * _NormalRTStrength * saturate(shadowmapColor * 2));
																			specGloss.r = specGloss.r + saturate(ripples2 * 30) * lerp(0, 0.1, saturate(saturate(1 - ripples2 * 5) * calculatedNormal.y * reflect(lightDirection, -normalDirection)).x * _NormalRTStrength * saturate(shadowmapColor * 2));
																		}
																		else
																		{
																			specGloss.r = specGloss.r * saturate(normal);
																		}
															#else
																		specGloss.r = specGloss.r * saturate(normal);
															#endif
															#endif
																		_LittleSpecForce = max(0, _LittleSpecForce);

															#ifdef	IS_ROCK
																		specularReflection = parallax + specularReflection;
															#else
																		specularReflection = lerp(parallax + specularReflection,  specularReflection * (specGloss.r + lerp(littleSpec.g * _LittleSpecForce * 0.2 , littleSpec.g * _LittleSpecForce, specularReflection)), saturate(rockValue * 3)); // multiply the *3 for a better sand ice transition
															#endif


															#ifdef	IS_ROCK
																		specularReflection = diffuseReflection * 0.1 + specularReflection * rougnessTex;
															#else
															#ifdef USE_RT
																		if (_HasRT == 1)
																		{
																			specularReflection = specularReflection - lerp(0, saturate(0.075), saturate(saturate(_LightColor0.a * lightIntensity + _LightColor0.a * 0.35) * saturate(1 - ripples3 * 4) * calculatedNormal.x * reflect(lightDirection, normalDirection)).x * _NormalRTStrength);
																			specularReflection = specularReflection + lerp(0, saturate(0.125), saturate(saturate(_LightColor0.a * lightIntensity + _LightColor0.a * 0.35) * saturate(1 - ripples3 * 8) * calculatedNormal.x * reflect(-lightDirection, normalDirection)).x * _NormalRTStrength);
																			specularReflection = specularReflection - lerp(0, saturate(1 - _ProjectedShadowColor) * 0.25, saturate(saturate(1 - ripples2 * 1) * calculatedNormal.y * reflect(-lightDirection, normalDirection)).x * _NormalRTStrength);
																			specularReflection = specularReflection - lerp(0, -0.1, saturate(saturate(1 - ripples2 * 1) * calculatedNormal.y * -reflect(-lightDirection, normalDirection)).x * _NormalRTStrength);
																			specularReflection = specularReflection + lerp(0, saturate(0.2), saturate(saturate(1 - ripples2 * 6) * calculatedNormal.y * reflect(lightDirection, normalDirection)).x * _NormalRTStrength * 0.5);
																		}
															#endif

																		specularReflection = lerp(specularReflection, diffuseReflection * 0.1 + specularReflection * rougnessTex, saturate(pow(1 - rockValue, 2) * 3));
															#endif

															#ifdef USE_AL

																		half3 ambientColor = ShadeSH9(half4(lerp(normalDirection, normalDirection + normalEffect * 2.5, saturate(ripples3 * rockValue)), 1));
										#if !UNITY_COLORSPACE_GAMMA
																		ambientColor = pow(ambientColor, gamma) * 1.0;
										#endif
																		albedo.rgb = saturate(albedo.rgb + (ambientColor - 0.5) * 0.75);
															#endif
																		half fresnelRefl = lerp(1, 0, saturate(dot(normalDirection, viewDirection) * 2.65 * _RimColor.a));

															#ifdef	IS_ROCK

															#else
																		albedo.rgb = lerp(albedo.rgb, albedo.rgb + _RimColor, saturate(rockValue * (fresnelRefl + normal * fresnelRefl * 0.2)));
															#endif
																		albedo += float4(specularReflection.r, specularReflection.g, specularReflection.b, 1.0) * _SpecColor.rgb;


										#ifdef USE_FOG
																		// CUSTOM FOG RENDER //
																		albedo.rgb = lerp(albedo.rgb, albedo.rgb + fogFlow * _FogColor, _FogIntensity);
										#endif

																		half transparency = 1;
															#ifdef	IS_ADD
																		transparency = saturate(lerp(-0.5,2, saturate(pow(rockValue,1))));
																		if (rockValue < 0.30)
																		{
																			discard;
																		}
															#else
																		transparency = saturate(lerp(_TransparencyValue, 1, saturate(pow(rockValue, 2))));
															#endif

																		albedo = max(0, albedo);
																UNITY_APPLY_FOG(i.fogCoord, albedo);
																return float4(albedo, transparency);
															}

																		ENDCG
															}


					// SHADOW CASTER PASS
					Pass{
					Tags {
						"LightMode" = "ShadowCaster"
					}

					CGPROGRAM

					#pragma target 4.6


							#pragma multi_compile _ LOD_FADE_CROSSFADE

							#pragma multi_compile_fwdbase
							#pragma multi_compile_fog

							#pragma vertex vert
							#pragma fragment frag

							#define FORWARD_BASE_PASS
							#pragma shader_feature USE_AL
							#pragma shader_feature USE_RT
							#pragma shader_feature IS_ADD
							#pragma shader_feature USE_INTER
							#pragma shader_feature USE_WC

							#include "UnityPBSLighting.cginc"
							#include "AutoLight.cginc"

					sampler2D  _DetailTex, _DetailMask;
					float4 _MainTex_ST, _DetailTex_ST;

					// Render Texture Effects //
					uniform sampler2D _GlobalEffectRT;
					uniform float3 _Position;
					uniform float _OrthographicCamSize;
					uniform sampler2D _GlobalEffectRTAdditional;
					uniform float3 _PositionAdd;
					uniform float _OrthographicCamSizeAdditional;

					sampler2D _MainTex;
					float _HasRT;

					float _UpVector, _NormalVector;
					float _AddSandStrength, _RemoveSandStrength, _DisplacementStrength;

					sampler2D _SandHeight;
					sampler2D _SandTransition;
					float _TransitionScale;
					float _TransitionPower;
					float _HeightScale, _SandScale;

					half _OverallScale;

					half _DisplacementOffset;

					struct VertexData //appdata
					{
						float4 vertex : POSITION;
						float3 normal : NORMAL;
						float4 tangent : TANGENT;
						float2 uv : TEXCOORD0;
						float4 color : COLOR;

					#ifdef SHADOWS_SCREEN
						SHADOW_COORDS(1)
					#endif
#ifdef USE_VR
							UNITY_VERTEX_INPUT_INSTANCE_ID
#endif

#ifdef IS_ADD
#ifdef USE_INTER
							float2 uv3 : TEXCOORD3;
						float2 uv4 : TEXCOORD4;
						float2 uv6 : TEXCOORD6;
						float2 uv7 : TEXCOORD7;
#endif
#endif
					};

					struct InterpolatorsVertex
					{
						float4 pos : SV_POSITION;
						float3 normal : TEXCOORD1;
						float4 tangent : TANGENT;
						float4 uv : TEXCOORD0;
						float4 color : COLOR;
						float3 worldPos : TEXCOORD2;
						float3 viewDir: POSITION1;
						float3 normalDir: TEXCOORD3;

					#ifdef SHADOWS_SCREEN
						SHADOW_COORDS(4)
					#endif
#ifdef USE_VR
							UNITY_VERTEX_OUTPUT_STEREO
#endif
					};



					InterpolatorsVertex vert(VertexData v) {
						InterpolatorsVertex i;

#ifdef USE_VR
						UNITY_SETUP_INSTANCE_ID(v);
						UNITY_INITIALIZE_OUTPUT(InterpolatorsVertex, i);
						UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(i);
#endif

						float3 worldPos = mul(unity_ObjectToWorld, v.vertex);
						float3 originalPos = worldPos;

						//RT Cam effects
						float2 uv = worldPos.xz - _Position.xz;
						uv = uv / (_OrthographicCamSize * 2);
						uv += 0.5;

						float2 uvAdd = worldPos.xz - _PositionAdd.xz;
						uvAdd = uvAdd / (_OrthographicCamSizeAdditional * 2);
						uvAdd += 0.5;

						float3 rippleMain = 0;
						float3 rippleMainAdditional = 0;

						float ripples = 0;
						float ripples2 = 0;
						float ripples3 = 0;

						float uvRTValue = 0;

#ifdef IS_T
						i.uv.xy = v.uv * _OverallScale;
#else
						i.uv.xy = TRANSFORM_TEX(v.uv, _MainTex) * _OverallScale;
#endif
						i.uv.zw = TRANSFORM_TEX(v.uv, _DetailTex);

					#ifdef USE_RT
						if (_HasRT == 1)
						{
							// .b(lue) = Sand Dig / .r(ed) = Sand To Ice / .g(reen) = Sand Mount
							rippleMain = tex2Dlod(_GlobalEffectRT, float4(uv, 0, 0));
							rippleMainAdditional = tex2Dlod(_GlobalEffectRTAdditional, float4(uvAdd, 0, 0));
						}

					#ifdef IS_ROCK
					#else
						float2 uvGradient = smoothstep(0, 5, length(max(abs(_Position.xz - worldPos.xz) - _OrthographicCamSize + 5, 0.0)));
						uvRTValue = saturate(uvGradient.x);

						ripples = lerp(rippleMain.x, rippleMainAdditional.x, uvRTValue);
						ripples2 = lerp(rippleMain.y, rippleMainAdditional.y, uvRTValue);
						ripples3 = lerp(rippleMain.z, rippleMainAdditional.z, uvRTValue);
					#endif

					#endif
						float slopeValue = 0;
					#ifdef IS_T
						half4 splat_control = tex2Dlod(_Control0, float4(i.uv.xy, 0, 0));
#ifdef USE_COMPLEX_T
						half4 splat_control1 = tex2Dlod(_Control1, float4(i.uv.xy, 0, 0));
#endif

						float rockValue = saturate(1 - splat_control.r * _Metallic0 - splat_control.g * _Metallic1 - splat_control.b * _Metallic2 - splat_control.a * _Metallic3
							- ripples);

						float sandHeightNew = _Mask0.SampleLevel(my_linear_repeat_sampler, float4(originalPos.xz, 0, 0) * _HeightScale * _SandScale * 0.1, 0).r;
						sandHeightNew = lerp(sandHeightNew, _Mask1.SampleLevel(my_linear_repeat_sampler, float4(originalPos.xz, 0, 0) * _HeightScale * _SandScale * 0.1, 0).r, saturate(splat_control.g * (1 - _Metallic1)));
						sandHeightNew = lerp(sandHeightNew, _Mask2.SampleLevel(my_linear_repeat_sampler, float4(originalPos.xz, 0, 0) * _HeightScale * _SandScale * 0.1, 0).r, saturate(splat_control.b * (1 - _Metallic2)));
						sandHeightNew = lerp(sandHeightNew, _Mask3.SampleLevel(my_linear_repeat_sampler, float4(originalPos.xz, 0, 0) * _HeightScale * _SandScale * 0.1, 0).r, saturate(splat_control.a * (1 - _Metallic3)));
#ifdef USE_COMPLEX_T
						sandHeightNew = lerp(sandHeightNew, _Mask4.SampleLevel(my_linear_repeat_sampler, float4(originalPos.xz, 0, 0) * _HeightScale * _SandScale * 0.1, 0).r, saturate(splat_control1.r * (1 - _Metallic4)));
						sandHeightNew = lerp(sandHeightNew, _Mask5.SampleLevel(my_linear_repeat_sampler, float4(originalPos.xz, 0, 0) * _HeightScale * _SandScale * 0.1, 0).r, saturate(splat_control1.g * (1 - _Metallic5)));
						sandHeightNew = lerp(sandHeightNew, _Mask6.SampleLevel(my_linear_repeat_sampler, float4(originalPos.xz, 0, 0) * _HeightScale * _SandScale * 0.1, 0).r, saturate(splat_control1.b * (1 - _Metallic6)));
						sandHeightNew = lerp(sandHeightNew, _Mask7.SampleLevel(my_linear_repeat_sampler, float4(originalPos.xz, 0, 0) * _HeightScale * _SandScale * 0.1, 0).r, saturate(splat_control1.a * (1 - _Metallic7)));
#endif

						float sandHeight = sandHeightNew;
					#else
						float rockValue = saturate((v.color.g + v.color.b) / 2 - ripples);

#ifdef USE_INTER
#ifdef IS_ADD			// custom intersection and slope value //
						float4 midPoint = mul(unity_ObjectToWorld, float4(0.0, 0.0, 0.0, 1.0));

						float4 quaternion = float4(v.uv6.x, -v.uv6.y, -v.uv7.x, -v.uv7.y);
						float3 offsetPoint = worldPos.xyz - midPoint;

						float3 rotatedVert = rotateVector(quaternion, -offsetPoint);
						float manualLerp = 0;

						manualLerp = v.uv4.x;

						rotatedVert = RotateAroundZInDegrees(float4(rotatedVert, 0), lerp(6, -6, (manualLerp)));
						rotatedVert = RotateAroundXInDegrees(float4(rotatedVert, 0), lerp(-55, 55, (manualLerp))) + midPoint;

						slopeValue = ((v.color.a) - (rotatedVert.y - 0.5));

						if (slopeValue > 0.0)
						{
							v.color.g = saturate(v.color.g + saturate(slopeValue * 3));
							v.color.b = saturate(v.color.b + saturate(slopeValue * 3));
						}
#endif
#endif


						if (v.color.b > 0.6 && v.color.g < 0.4)
						{
							rockValue = saturate(1 - v.color.b);
						}
						else
						{
							rockValue = saturate((v.color.g + v.color.b) / 2 - ripples);
						}


#ifdef USE_WC
						float sandHeight = tex2Dlod(_SandHeight, float4(originalPos.xz, 0, 0) * _HeightScale * 0.1 * _SandScale).r;
#else
						float sandHeight = tex2Dlod(_SandHeight, float4(i.uv.xy, 0, 0) * _HeightScale * _SandScale).r;
#endif

#ifdef IS_SAND
						rockValue = 1;
#endif

					#endif

						i.normalDir = normalize(mul(float4(v.normal, 0.0), unity_WorldToObject));
					#ifdef IS_ROCK
					#else


						v.color = lerp(v.color, saturate(float4(1, 0, 0, 0)), ripples);
						i.normal = normalize(v.normal);

#ifdef IS_ADD
						float3 newNormal = normalize(i.normalDir);
						worldPos += ((float4(0, -_RemoveSandStrength, 0, 0) * _UpVector - newNormal * _RemoveSandStrength * _NormalVector) * ripples3 + (float4(0, _AddSandStrength * sandHeight, 0, 0) * _UpVector + newNormal * _AddSandStrength * sandHeight * _NormalVector) * ripples2 * saturate(1 - ripples3)) * saturate(rockValue * 3);
						worldPos += (float4(0, _DisplacementOffset, 0, 0) * _UpVector + newNormal * _DisplacementOffset * _NormalVector) * saturate(rockValue * 2.5);
						worldPos += (float4(0, 2 * _DisplacementStrength * sandHeight, 0, 0) * _UpVector) + (newNormal * 2 * _DisplacementStrength * sandHeight * _NormalVector * clamp(slopeValue * 20, 1, 2)) * saturate(saturate(rockValue * 2.5));

						worldPos = lerp(worldPos, mul(unity_ObjectToWorld, v.vertex), saturate(v.color.g - saturate(v.color.r + v.color.b)));

						v.vertex.xyz = lerp(mul(unity_WorldToObject, float4(originalPos, 1)).xyz, mul(unity_WorldToObject, float4(worldPos, 1)).xyz, rockValue);
#else
						float3 newNormal = normalize(i.normalDir);
						worldPos += ((float4(0, -_RemoveSandStrength, 0, 0) * _UpVector - newNormal * _RemoveSandStrength * _NormalVector) * ripples3 + (float4(0, _AddSandStrength * sandHeight, 0, 0) * _UpVector + newNormal * _AddSandStrength * sandHeight * _NormalVector) * ripples2 * saturate(1 - ripples3)) * saturate(rockValue * 3);
						worldPos += (float4(0, _DisplacementOffset, 0, 0) * _UpVector + newNormal * _DisplacementOffset * _NormalVector) * saturate(rockValue * 2.5);
						worldPos += (float4(0, 2 * _DisplacementStrength * sandHeight, 0, 0) * _UpVector) + (newNormal * 2 * _DisplacementStrength * sandHeight * _NormalVector) * saturate(saturate(rockValue * 2.5));

						worldPos = lerp(worldPos, mul(unity_ObjectToWorld, v.vertex), saturate(v.color.g - saturate(v.color.r + v.color.b)));

						v.vertex.xyz = lerp(mul(unity_WorldToObject, float4(originalPos, 1)).xyz, mul(unity_WorldToObject, float4(worldPos, 1)).xyz, rockValue);

#endif
					#endif

						i.pos = UnityObjectToClipPos(v.vertex);

						float4 objCam = mul(unity_WorldToObject, float4(_WorldSpaceCameraPos, 1.0));
						float3 viewDir = v.vertex.xyz - objCam.xyz;

					#ifdef IS_T
						float4 tangent = float4 (1.0, 0.0, 0.0, -1.0);
						tangent.xyz = tangent.xyz - v.normal * dot(v.normal, tangent.xyz); // Orthogonalize tangent to normal.

						float tangentSign = tangent.w * unity_WorldTransformParams.w;
						float3 bitangent = cross(v.normal.xyz, tangent.xyz) * tangentSign;

						i.viewDir = float3(
							dot(viewDir, tangent.xyz),
							dot(viewDir, bitangent.xyz),
							dot(viewDir, v.normal.xyz)
							);

						i.worldPos.xyz = mul(unity_ObjectToWorld, v.vertex);
						i.tangent = tangent;

					#else
						float tangentSign = v.tangent.w * unity_WorldTransformParams.w;
						float3 bitangent = cross(v.normal.xyz, v.tangent.xyz) * tangentSign;

						i.viewDir = float3(
							dot(viewDir, v.tangent.xyz),
							dot(viewDir, bitangent.xyz),
							dot(viewDir, v.normal.xyz)
							);

						i.worldPos.xyz = mul(unity_ObjectToWorld, v.vertex);
						i.tangent = v.tangent;
					#endif

						i.color = v.color;



						TRANSFER_SHADOW_CASTER_NORMALOFFSET(i)

						return i;
					}

								float4 frag(InterpolatorsVertex i) : SV_Target
								{
#ifdef USE_VR
								UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i)
#endif
					#ifdef	IS_ADD
										float3 worldPos = mul(unity_ObjectToWorld, i.pos);
						float3 originalPos = worldPos;

						float2 uv = worldPos.xz - _Position.xz;
						uv = uv / (_OrthographicCamSize * 2);
						uv += 0.5;

						float2 uvAdd = worldPos.xz - _PositionAdd.xz;
						uvAdd = uvAdd / (_OrthographicCamSizeAdditional * 2);
						uvAdd += 0.5;

						float3 rippleMain = 0;
						float3 rippleMainAdditional = 0;
										rippleMain = tex2Dlod(_GlobalEffectRT, float4(uv, 0, 0));
						rippleMainAdditional = tex2Dlod(_GlobalEffectRTAdditional, float4(uvAdd, 0, 0));

						float sandHeight = tex2D(_SandTransition, (i.uv * _TransitionScale * _SandScale)).r;
						float rockValue = saturate(pow((i.color.g + i.color.b) / 2, 0.35 + clamp((sandHeight - 0.5) * -_TransitionPower * (saturate(i.color.g + i.color.b)), -0.34, 1)));
						if (rockValue < 0.30)
						{
							discard;
						}
					#endif
								SHADOW_CASTER_FRAGMENT(i)
								}


								ENDCG
							}

									// ADDITIONAL LIGHT PASS
							Pass{
									Tags {
										"LightMode" = "ForwardAdd"
									}
										Blend One One // Additive

									CGPROGRAM

									#pragma target 4.6

											#pragma multi_compile _ LOD_FADE_CROSSFADE

									//#pragma multi_compile_fwdbase
									#pragma multi_compile_fog

									#pragma vertex vert
									#pragma fragment frag

									#define FORWARD_BASE_PASS
									#pragma shader_feature USE_AL
									#pragma shader_feature USE_RT
									#pragma shader_feature IS_ADD
									#pragma shader_feature USE_INTER
									#pragma shader_feature USE_WC

								#pragma multi_compile_fwdadd_fullshadows
								#include "Lighting.cginc"
								#include "AutoLight.cginc"

								//uniform float4x4 unity_WorldToLight;
								//uniform sampler2D _LightTexture0;
								uniform float _LightIntensity;
								//uniform float4 _LightColor0;

								sampler2D  _DetailTex, _DetailMask;
								float4 _MainTex_ST, _DetailTex_ST;

								// Render Texture Effects //
								uniform sampler2D _GlobalEffectRT;
								uniform float3 _Position;
								uniform float _OrthographicCamSize;
								uniform sampler2D _GlobalEffectRTAdditional;
								uniform float3 _PositionAdd;
								uniform float _OrthographicCamSizeAdditional;

								float _HasRT;
								sampler2D _MainTex;

								float _UpVector, _NormalVector;
								float _AddSandStrength, _RemoveSandStrength, _DisplacementStrength;

								//ICE Variables
								sampler2D _SandHeight;
								sampler2D _SandTransition;
								float _TransitionScale;
								float _TransitionPower;
								float _HeightScale, _SandScale;

								half _OverallScale;

								half _DisplacementOffset;

							struct VertexData //appdata
							{
								float4 vertex : POSITION;
								float3 normal : NORMAL;
								float4 tangent : TANGENT;
								float2 uv : TEXCOORD0;
								float4 color : COLOR;
								UNITY_FOG_COORDS(1)
								float4 posLight : TEXCOORD2;
#ifdef USE_VR
								UNITY_VERTEX_INPUT_INSTANCE_ID
#endif

#ifdef IS_ADD
#ifdef USE_INTER
									float2 uv3 : TEXCOORD3;
								float2 uv4 : TEXCOORD4;
								float2 uv6 : TEXCOORD6;
								float2 uv7 : TEXCOORD7;
#endif
#endif
							};

							struct InterpolatorsVertex
							{
								float4 pos : SV_POSITION;
								float3 normal : TEXCOORD1;
								float4 tangent : TANGENT;
								float4 uv : TEXCOORD0;
								float4 color : COLOR;
								float3 worldPos : TEXCOORD2;
								float3 viewDir: POSITION1;
								float3 normalDir: TEXCOORD3;
								UNITY_FOG_COORDS(4)
								float4 posLight : TEXCOORD5;
#ifdef USE_VR
								UNITY_VERTEX_OUTPUT_STEREO
#endif
							};

							InterpolatorsVertex vert(VertexData v) {
								InterpolatorsVertex i;

#ifdef USE_VR
								UNITY_SETUP_INSTANCE_ID(v);
								UNITY_INITIALIZE_OUTPUT(InterpolatorsVertex, i);
								UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(i);
#endif

								float3 worldPos = mul(unity_ObjectToWorld, v.vertex);
								float3 originalPos = worldPos;

								//RT Cam effects
								float2 uv = worldPos.xz - _Position.xz;
								uv = uv / (_OrthographicCamSize * 2);
								uv += 0.5;

								float2 uvAdd = worldPos.xz - _PositionAdd.xz;
								uvAdd = uvAdd / (_OrthographicCamSizeAdditional * 2);
								uvAdd += 0.5;

								float3 rippleMain = 0;
								float3 rippleMainAdditional = 0;

								float ripples = 0;
								float ripples2 = 0;
								float ripples3 = 0;

								float uvRTValue = 0;


#ifdef IS_T
								i.uv.xy = v.uv * _OverallScale;
#else
								i.uv.xy = TRANSFORM_TEX(v.uv, _MainTex) * _OverallScale;
#endif
								i.uv.zw = TRANSFORM_TEX(v.uv, _DetailTex);

								//float viewDistance = distance(worldPos, _WorldSpaceCameraPos);
					#ifdef USE_RT
								if (_HasRT == 1)
								{
									// .b(lue) = Sand Dig / .r(ed) = Sand To Ice / .g(reen) = Sand Mount
									rippleMain = tex2Dlod(_GlobalEffectRT, float4(uv, 0, 0));
									rippleMainAdditional = tex2Dlod(_GlobalEffectRTAdditional, float4(uvAdd, 0, 0));
								}

					#ifdef IS_ROCK
					#else
								float2 uvGradient = smoothstep(0, 5, length(max(abs(_Position.xz - worldPos.xz) - _OrthographicCamSize + 5, 0.0)));
								uvRTValue = saturate(uvGradient.x);

								ripples = lerp(rippleMain.x, rippleMainAdditional.x, uvRTValue);
								ripples2 = lerp(rippleMain.y, rippleMainAdditional.y, uvRTValue);
								ripples3 = lerp(rippleMain.z, rippleMainAdditional.z, uvRTValue);
					#endif

					#endif
								float slopeValue = 0;
					#ifdef IS_T
								half4 splat_control = tex2Dlod(_Control0, float4(i.uv.xy, 0, 0));
#ifdef USE_COMPLEX_T
								half4 splat_control1 = tex2Dlod(_Control1, float4(i.uv.xy, 0, 0));
#endif

								float rockValue = saturate(1 - splat_control.r * _Metallic0 - splat_control.g * _Metallic1 - splat_control.b * _Metallic2 - splat_control.a * _Metallic3
									- ripples);

								float sandHeightNew = _Mask0.SampleLevel(my_linear_repeat_sampler, float4(originalPos.xz, 0, 0) * _HeightScale * _SandScale * 0.1, 0).r;
								sandHeightNew = lerp(sandHeightNew, _Mask1.SampleLevel(my_linear_repeat_sampler, float4(originalPos.xz, 0, 0) * _HeightScale * _SandScale * 0.1, 0).r, saturate(splat_control.g * (1 - _Metallic1)));
								sandHeightNew = lerp(sandHeightNew, _Mask2.SampleLevel(my_linear_repeat_sampler, float4(originalPos.xz, 0, 0) * _HeightScale * _SandScale * 0.1, 0).r, saturate(splat_control.b * (1 - _Metallic2)));
								sandHeightNew = lerp(sandHeightNew, _Mask3.SampleLevel(my_linear_repeat_sampler, float4(originalPos.xz, 0, 0) * _HeightScale * _SandScale * 0.1, 0).r, saturate(splat_control.a * (1 - _Metallic3)));
#ifdef USE_COMPLEX_T
								sandHeightNew = lerp(sandHeightNew, _Mask4.SampleLevel(my_linear_repeat_sampler, float4(originalPos.xz, 0, 0) * _HeightScale * _SandScale * 0.1, 0).r, saturate(splat_control1.r * (1 - _Metallic4)));
								sandHeightNew = lerp(sandHeightNew, _Mask5.SampleLevel(my_linear_repeat_sampler, float4(originalPos.xz, 0, 0) * _HeightScale * _SandScale * 0.1, 0).r, saturate(splat_control1.g * (1 - _Metallic5)));
								sandHeightNew = lerp(sandHeightNew, _Mask6.SampleLevel(my_linear_repeat_sampler, float4(originalPos.xz, 0, 0) * _HeightScale * _SandScale * 0.1, 0).r, saturate(splat_control1.b * (1 - _Metallic6)));
								sandHeightNew = lerp(sandHeightNew, _Mask7.SampleLevel(my_linear_repeat_sampler, float4(originalPos.xz, 0, 0) * _HeightScale * _SandScale * 0.1, 0).r, saturate(splat_control1.a * (1 - _Metallic7)));
#endif

								float sandHeight = sandHeightNew;
					#else
								float rockValue = saturate((v.color.g + v.color.b) / 2 - ripples);
#ifdef USE_INTER
#ifdef IS_ADD			// custom intersection and slope value //
								float4 midPoint = mul(unity_ObjectToWorld, float4(0.0, 0.0, 0.0, 1.0));

								float4 quaternion = float4(v.uv6.x, -v.uv6.y, -v.uv7.x, -v.uv7.y);
								float3 offsetPoint = worldPos.xyz - midPoint;

								float3 rotatedVert = rotateVector(quaternion, -offsetPoint);
								float manualLerp = 0;

								manualLerp = v.uv4.x;

								rotatedVert = RotateAroundZInDegrees(float4(rotatedVert, 0), lerp(6, -6, (manualLerp)));
								rotatedVert = RotateAroundXInDegrees(float4(rotatedVert, 0), lerp(-55, 55, (manualLerp))) + midPoint;

								slopeValue = ((v.color.a) - (rotatedVert.y - 0.5));

								if (slopeValue > 0.0)
								{
									v.color.g = saturate(v.color.g + saturate(slopeValue * 3));
									v.color.b = saturate(v.color.b + saturate(slopeValue * 3));
								}
#endif
#endif

								if (v.color.b > 0.6 && v.color.g < 0.4)
								{
									rockValue = saturate(1 - v.color.b);
								}
								else
								{
									rockValue = saturate((v.color.g + v.color.b) / 2 - ripples);
								}


					#ifdef USE_WC
								float sandHeight = tex2Dlod(_SandHeight, float4(originalPos.xz, 0, 0) * _HeightScale * 0.1).r;
					#else
								float sandHeight = tex2Dlod(_SandHeight, float4(i.uv.xy, 0, 0) * _HeightScale).r;
					#endif
					#endif

								i.normalDir = normalize(mul(float4(v.normal, 0.0), unity_WorldToObject));
					#ifdef IS_ROCK
					#else

#ifdef IS_SAND
								rockValue = 1;
#endif

								v.color = lerp(v.color, saturate(float4(1, 0, 0, 0)), ripples);
								i.normal = normalize(v.normal);

					#ifdef IS_ADD
								float3 newNormal = normalize(i.normalDir);
								worldPos += ((float4(0, -_RemoveSandStrength, 0, 0) * _UpVector - newNormal * _RemoveSandStrength * _NormalVector) * ripples3 + (float4(0, _AddSandStrength * sandHeight, 0, 0) * _UpVector + newNormal * _AddSandStrength * sandHeight * _NormalVector) * ripples2 * saturate(1 - ripples3)) * saturate(rockValue * 3);
								worldPos += (float4(0, _DisplacementOffset, 0, 0) * _UpVector + newNormal * _DisplacementOffset * _NormalVector) * saturate(rockValue * 2.5);
								worldPos += (float4(0, 2 * _DisplacementStrength * sandHeight, 0, 0) * _UpVector) + (newNormal * 2 * _DisplacementStrength * sandHeight * _NormalVector * clamp(slopeValue * 20, 1, 2)) * saturate(saturate(rockValue * 2.5));

								worldPos = lerp(worldPos, mul(unity_ObjectToWorld, v.vertex), saturate(v.color.g - saturate(v.color.r + v.color.b)));

								v.vertex.xyz = lerp(mul(unity_WorldToObject, float4(originalPos, 1)).xyz, mul(unity_WorldToObject, float4(worldPos, 1)).xyz, rockValue);
					#else
								float3 newNormal = normalize(i.normalDir);
								worldPos += ((float4(0, -_RemoveSandStrength, 0, 0) * _UpVector - newNormal * _RemoveSandStrength * _NormalVector) * ripples3 + (float4(0, _AddSandStrength * sandHeight, 0, 0) * _UpVector + newNormal * _AddSandStrength * sandHeight * _NormalVector) * ripples2 * saturate(1 - ripples3)) * saturate(rockValue * 3);
								worldPos += (float4(0, _DisplacementOffset, 0, 0) * _UpVector + newNormal * _DisplacementOffset * _NormalVector) * saturate(rockValue * 2.5);
								worldPos += (float4(0, 2 * _DisplacementStrength * sandHeight, 0, 0) * _UpVector) + (newNormal * 2 * _DisplacementStrength * sandHeight * _NormalVector) * saturate(saturate(rockValue * 2.5));

								worldPos = lerp(worldPos, mul(unity_ObjectToWorld, v.vertex), saturate(v.color.g - saturate(v.color.r + v.color.b)));

								v.vertex.xyz = lerp(mul(unity_WorldToObject, float4(originalPos, 1)).xyz, mul(unity_WorldToObject, float4(worldPos, 1)).xyz, rockValue);

					#endif
					#endif

								i.pos = UnityObjectToClipPos(v.vertex);

								float4 objCam = mul(unity_WorldToObject, float4(_WorldSpaceCameraPos, 1.0));
								float3 viewDir = v.vertex.xyz - objCam.xyz;

					#ifdef IS_T
								float4 tangent = float4 (1.0, 0.0, 0.0, -1.0);
								tangent.xyz = tangent.xyz - v.normal * dot(v.normal, tangent.xyz); // Orthogonalize tangent to normal.

								float tangentSign = tangent.w * unity_WorldTransformParams.w;
								float3 bitangent = cross(v.normal.xyz, tangent.xyz) * tangentSign;

								i.viewDir = float3(
									dot(viewDir, tangent.xyz),
									dot(viewDir, bitangent.xyz),
									dot(viewDir, v.normal.xyz)
									);

								i.worldPos.xyz = mul(unity_ObjectToWorld, v.vertex);
								i.tangent = tangent;

					#else
								float tangentSign = v.tangent.w * unity_WorldTransformParams.w;
								float3 bitangent = cross(v.normal.xyz, v.tangent.xyz) * tangentSign;

								i.viewDir = float3(
									dot(viewDir, v.tangent.xyz),
									dot(viewDir, bitangent.xyz),
									dot(viewDir, v.normal.xyz)
									);

								i.worldPos.xyz = mul(unity_ObjectToWorld, v.vertex);
								i.tangent = v.tangent;
					#endif

								i.color = v.color;


					#if defined(SPOT) || defined(POINT)
								i.posLight = mul(unity_WorldToLight, mul(unity_ObjectToWorld, v.vertex));
					#else
								i.posLight = mul(unity_ObjectToWorld, v.vertex);
					#endif


								UNITY_TRANSFER_FOG(i, i.pos);
								return i;
							}

										float4 frag(InterpolatorsVertex i) : SV_Target
										{
#ifdef USE_VR
								UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i)
#endif
								float3 worldPos = i.worldPos;
								float3 originalPos = worldPos;
							#ifdef	IS_ADD

								float2 uv = worldPos.xz - _Position.xz;
								uv = uv / (_OrthographicCamSize * 2);
								uv += 0.5;

								float2 uvAdd = worldPos.xz - _PositionAdd.xz;
								uvAdd = uvAdd / (_OrthographicCamSizeAdditional * 2);
								uvAdd += 0.5;

								float3 rippleMain = 0;
								float3 rippleMainAdditional = 0;

								rippleMain = tex2Dlod(_GlobalEffectRT, float4(uv, 0, 0));
								rippleMainAdditional = tex2Dlod(_GlobalEffectRTAdditional, float4(uvAdd, 0, 0));

								float sandHeight = tex2D(_SandTransition, (i.uv * _TransitionScale * _SandScale)).r;
								float rockValue = saturate(pow((i.color.g + i.color.b) / 2, 0.35 + clamp((sandHeight - 0.5) * -_TransitionPower * (saturate(i.color.g + i.color.b)), -0.34, 1)));
								if (rockValue < 0.30)
								{
									discard;
								}
							#endif

									 float3 normalDirection = normalize(i.normalDir);
									 float3 lightDirection;
									 float attenuation = 1.0;
									 float cookieAttenuation = 1.0;
					#if defined(SPOT) || defined(POINT)
									 if (0.0 == _WorldSpaceLightPos0.w) // directional light
									 {
										attenuation = 1.0; // no attenuation
										lightDirection = normalize(_WorldSpaceLightPos0.xyz);
										cookieAttenuation = tex2D(_LightTexture0, i.posLight.xy).a;
									 }
									 else if (1.0 != unity_WorldToLight[3][3]) // spot light
									 {
										 attenuation = 1.0; // no attenuation
										 UNITY_LIGHT_ATTENUATION(atten, i, worldPos.xyz);
										 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
										 cookieAttenuation = tex2D(_LightTexture0,i.posLight.xy / i.posLight.w + float2(0.5, 0.5)).a;
										 attenuation = atten;
									 }
									 else // point light
									 {
										float3 vertexToLightSource = _WorldSpaceLightPos0.xyz - originalPos.xyz;
										lightDirection = normalize(vertexToLightSource);

										half ndotl = saturate(dot(normalDirection, lightDirection));
										UNITY_LIGHT_ATTENUATION(atten, i, worldPos.xyz);
										attenuation = ndotl * atten;
									 }
					#else
									 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
					#endif
									 float3 diffuseReflection = attenuation * _LightColor0.rgb * max(0.0, dot(normalDirection, lightDirection));
									 float3 finalLightColor = cookieAttenuation * diffuseReflection;
									 finalLightColor *= _LightIntensity;

									 UNITY_APPLY_FOG(i.fogCoord, finalLightColor);
									 return float4(saturate(finalLightColor),1);
										}

										ENDCG
							}
				}
}