Shader "Ciconia Studio/CS_Ghost/CS_Advanced Ghost (Always Visible)"
{
	Properties
	{
		[Space(15)][Header(Main Properties)][Space(15)]_Color("Color ", Color) = (0,0,0,1)
		_MainTex("Base Color", 2D) = "white" {}
		[Space(15)][Toggle(_ENABLESPECULARLIGHT_ON)] _EnableSpecularLight("Enable Specular Light", Float) = 0
		_Glossiness("Smoothness", Range( 0 , 1)) = 0.75
		[Space(35)]_BumpMap("Normal Map", 2D) = "bump" {}
		_BumpScale("Scale", Range( 0 , 4)) = 0.3
		[Space(15)][Header(Fresnel Properties)][Space(15)]_FresnelColor("Color", Color) = (0.6933962,1,0.9814353,1)
		_SelfIllumination("Self Illumination", Range( 1 , 10)) = 1
		[Space(15)]_FresnelIntensity("Fresnel Intensity", Float) = 4
		_FresnelPower("Fresnel Power", Float) = 4
		_FresnelBias("Bias", Range( 0 , 1)) = 0
		[Toggle]_Invert("Invert", Float) = 0
		[Space(15)][Header(Animation Properties)][Space(15)]_MinValueAmplitude("Min Value", Float) = 1
		_MaxValueAmplitude("Max Value", Float) = 2
		[TextArea(1)]_AmplitudeSpeed("Speed", Float) = 1
		[Space(15)][Header(Details Properties)][Space(15)]_DetailMap("Detail Map", 2D) = "white" {}
		_ContrastDetailMap("Contrast", Float) = 1
		_SpreadDetailMap("Spread", Float) = 0
		[Space(15)]_DetailScale("Intensity", Float) = 1
		[Toggle(_DUPLICATEDETAILS_ON)] _DuplicateDetails("Duplicate Details", Float) = 1
		[Space(15)]_TranslationSpeed("Translation Speed", Float) = 0
		_RotationSpeed("Rotation Speed", Float) = 0
		_RotationAngle("Rotation Angle", Float) = 0
		[Space(15)][KeywordEnum(UVProjection,ScreenProjection)] _UVScreenProjection("UV/Screen Projection", Float) = 0
		_TexturesScale("Textures Scale", Float) = 1
		[Space(15)][KeywordEnum(None,NormalMap,DetailMap,Both)] _MapContribution("Map Contribution", Float) = 0
		_Refraction("Refraction", Range( 0 , 2)) = 1.1
		[Space(15)][Header(Transparency Properties)][Space(15)]_FillColorBackground("Fill Color Background", Color) = (0,0,0,0)
		_DesaturateBackground("Desaturate Background", Range( 0 , 1)) = 0
		[Space(10)]_Opacity("Opacity", Range( 0 , 1)) = 1
		_ShadowOpacity("Shadow Opacity", Range( 0 , 1)) = 0
		[Space(15)]_FresnelTransIntensity("Fresnel Intensity", Float) = 0
		_FresnelTransPower("Fresnel Power", Float) = 5
		_FresnelTransBias("Bias", Range( 0 , 1)) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Pass
		{
			ColorMask 0
			ZTest Always
			ZWrite On
		}

		Tags{ "RenderType" = "Custom"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Back
		ZWrite On
		Blend SrcAlpha OneMinusSrcAlpha
		
		GrabPass{ "_ScreenGrab0" }
		CGINCLUDE
		#include "UnityPBSLighting.cginc"
		#include "UnityShaderVariables.cginc"
		#include "UnityStandardUtils.cginc"
		#include "UnityCG.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		#pragma shader_feature_local _MAPCONTRIBUTION_NONE _MAPCONTRIBUTION_NORMALMAP _MAPCONTRIBUTION_DETAILMAP _MAPCONTRIBUTION_BOTH
		#pragma shader_feature_local _DUPLICATEDETAILS_ON
		#pragma shader_feature_local _UVSCREENPROJECTION_UVPROJECTION _UVSCREENPROJECTION_SCREENPROJECTION
		#pragma shader_feature_local _ENABLESPECULARLIGHT_ON
		#if defined(UNITY_STEREO_INSTANCING_ENABLED) || defined(UNITY_STEREO_MULTIVIEW_ENABLED)
		#define ASE_DECLARE_SCREENSPACE_TEXTURE(tex) UNITY_DECLARE_SCREENSPACE_TEXTURE(tex);
		#else
		#define ASE_DECLARE_SCREENSPACE_TEXTURE(tex) UNITY_DECLARE_SCREENSPACE_TEXTURE(tex)
		#endif
		#ifdef UNITY_PASS_SHADOWCASTER
			#undef INTERNAL_DATA
			#undef WorldReflectionVector
			#undef WorldNormalVector
			#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
			#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
			#define WorldNormalVector(data,normal) half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))
		#endif
		struct Input
		{
			float4 screenPos;
			float3 worldNormal;
			INTERNAL_DATA
			float2 uv_texcoord;
			float3 worldPos;
		};

		struct SurfaceOutputCustomLightingCustom
		{
			half3 Albedo;
			half3 Normal;
			half3 Emission;
			half Metallic;
			half Smoothness;
			half Occlusion;
			half Alpha;
			Input SurfInput;
			UnityGIInput GIData;
		};

		uniform float _SelfIllumination;
		ASE_DECLARE_SCREENSPACE_TEXTURE( _ScreenGrab0 )
		uniform sampler2D _BumpMap;
		uniform float4 _BumpMap_ST;
		uniform float _BumpScale;
		uniform float _ContrastDetailMap;
		uniform sampler2D _DetailMap;
		uniform float _TranslationSpeed;
		uniform float4 _DetailMap_ST;
		uniform float _TexturesScale;
		uniform float _RotationSpeed;
		uniform float _RotationAngle;
		uniform float _SpreadDetailMap;
		uniform float _Refraction;
		uniform float _DesaturateBackground;
		uniform float4 _FillColorBackground;
		uniform float _Opacity;
		uniform float4 _FresnelColor;
		uniform float _Invert;
		uniform float _FresnelBias;
		uniform float _FresnelIntensity;
		uniform float _FresnelPower;
		uniform float _DetailScale;
		uniform float _MaxValueAmplitude;
		uniform float _MinValueAmplitude;
		uniform float _AmplitudeSpeed;
		uniform float _ShadowOpacity;
		uniform float _FresnelTransBias;
		uniform float _FresnelTransIntensity;
		uniform float _FresnelTransPower;
		uniform float4 _Color;
		uniform sampler2D _MainTex;
		uniform float4 _MainTex_ST;
		uniform float _Glossiness;


		inline float4 ASE_ComputeGrabScreenPos( float4 pos )
		{
			#if UNITY_UV_STARTS_AT_TOP
			float scale = -1.0;
			#else
			float scale = 1.0;
			#endif
			float4 o = pos;
			o.y = pos.w * 0.5f;
			o.y = ( pos.y - o.y ) * _ProjectionParams.x * scale + o.y;
			return o;
		}


		float4 CalculateContrast( float contrastValue, float4 colorTarget )
		{
			float t = 0.5 * ( 1.0 - contrastValue );
			return mul( float4x4( contrastValue,0,0,t, 0,contrastValue,0,t, 0,0,contrastValue,t, 0,0,0,1 ), colorTarget );
		}

		inline half4 LightingStandardCustomLighting( inout SurfaceOutputCustomLightingCustom s, half3 viewDir, UnityGI gi )
		{
			UnityGIInput data = s.GIData;
			Input i = s.SurfInput;
			half4 c = 0;
			#ifdef UNITY_PASS_FORWARDBASE
			float ase_lightAtten = data.atten;
			if( _LightColor0.a == 0)
			ase_lightAtten = 0;
			#else
			float3 ase_lightAttenRGB = gi.light.color / ( ( _LightColor0.rgb ) + 0.000001 );
			float ase_lightAtten = max( max( ase_lightAttenRGB.r, ase_lightAttenRGB.g ), ase_lightAttenRGB.b );
			#endif
			#if defined(HANDLE_SHADOWS_BLENDING_IN_GI)
			half bakedAtten = UnitySampleBakedOcclusion(data.lightmapUV.xy, data.worldPos);
			float zDist = dot(_WorldSpaceCameraPos - data.worldPos, UNITY_MATRIX_V[2].xyz);
			float fadeDist = UnityComputeShadowFadeDistance(data.worldPos, zDist);
			ase_lightAtten = UnityMixRealtimeAndBakedShadows(data.atten, bakedAtten, UnityComputeShadowFade(fadeDist));
			#endif
			float3 ase_worldPos = i.worldPos;
			float3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float3 ase_worldNormal = WorldNormalVector( i, float3( 0, 0, 1 ) );
			float clampResult2_g1 = clamp( _FresnelTransIntensity , 0.0 , 400.0 );
			float fresnelNdotV5_g1 = dot( ase_worldNormal, ase_worldViewDir );
			float fresnelNode5_g1 = ( _FresnelTransBias + clampResult2_g1 * pow( max( 1.0 - fresnelNdotV5_g1 , 0.0001 ), _FresnelTransPower ) );
			float temp_output_7_0_g1 = saturate( ( 1.0 - fresnelNode5_g1 ) );
			float Opacity250 = ( (0.5 + (( 1.0 - _ShadowOpacity ) - 0.0) * (1.0 - 0.5) / (1.0 - 0.0)) * temp_output_7_0_g1 );
			float2 uv_MainTex = i.uv_texcoord * _MainTex_ST.xy + _MainTex_ST.zw;
			float2 uv_BumpMap = i.uv_texcoord * _BumpMap_ST.xy + _BumpMap_ST.zw;
			float3 NormalmapXYZ536 = UnpackScaleNormal( tex2D( _BumpMap, uv_BumpMap ), _BumpScale );
			#if defined(LIGHTMAP_ON) && UNITY_VERSION < 560 //aseld
			float3 ase_worldlightDir = 0;
			#else //aseld
			float3 ase_worldlightDir = normalize( UnityWorldSpaceLightDir( ase_worldPos ) );
			#endif //aseld
			float dotResult540 = dot( (WorldNormalVector( i , NormalmapXYZ536 )) , ase_worldlightDir );
			#if defined(LIGHTMAP_ON) && ( UNITY_VERSION < 560 || ( defined(LIGHTMAP_SHADOW_MIXING) && !defined(SHADOWS_SHADOWMASK) && defined(SHADOWS_SCREEN) ) )//aselc
			float4 ase_lightColor = 0;
			#else //aselc
			float4 ase_lightColor = _LightColor0;
			#endif //aselc
			UnityGI gi550 = gi;
			float3 diffNorm550 = WorldNormalVector( i , NormalmapXYZ536 );
			gi550 = UnityGI_Base( data, 1, diffNorm550 );
			float3 indirectDiffuse550 = gi550.indirect.diffuse + diffNorm550 * 0.0001;
			float3 indirectNormal557 = WorldNormalVector( i , NormalmapXYZ536 );
			Unity_GlossyEnvironmentData g557 = UnityGlossyEnvironmentSetup( _Glossiness, data.worldViewDir, indirectNormal557, float3(0,0,0));
			float3 indirectSpecular557 = UnityGI_IndirectSpecular( data, 1.0, indirectNormal557, g557 );
			float3 ase_worldTangent = WorldNormalVector( i, float3( 1, 0, 0 ) );
			float3 ase_worldBitangent = WorldNormalVector( i, float3( 0, 1, 0 ) );
			float3x3 ase_tangentToWorldFast = float3x3(ase_worldTangent.x,ase_worldBitangent.x,ase_worldNormal.x,ase_worldTangent.y,ase_worldBitangent.y,ase_worldNormal.y,ase_worldTangent.z,ase_worldBitangent.z,ase_worldNormal.z);
			float fresnelNdotV548 = dot( mul(ase_tangentToWorldFast,NormalmapXYZ536), ase_worldViewDir );
			float fresnelNode548 = ( 0.04 + 1.0 * pow( 1.0 - fresnelNdotV548, 1.11 ) );
			float clampResult552 = clamp( fresnelNode548 , 0.0 , 1.0 );
			#ifdef _ENABLESPECULARLIGHT_ON
				float staticSwitch556 = clampResult552;
			#else
				float staticSwitch556 = 0.0;
			#endif
			float4 lerpResult559 = lerp( ( ( _Color * tex2D( _MainTex, uv_MainTex ) ) * float4( ( ( max( dotResult540 , 0.0 ) * ( ase_lightAtten * ase_lightColor.rgb ) ) + indirectDiffuse550 ) , 0.0 ) ) , float4( indirectSpecular557 , 0.0 ) , staticSwitch556);
			float4 CustomLight560 = lerpResult559;
			float OpacityFresnelSub562 = temp_output_7_0_g1;
			c.rgb = ( CustomLight560 * OpacityFresnelSub562 ).rgb;
			c.a = Opacity250;
			return c;
		}

		inline void LightingStandardCustomLighting_GI( inout SurfaceOutputCustomLightingCustom s, UnityGIInput data, inout UnityGI gi )
		{
			s.GIData = data;
		}

		void surf( Input i , inout SurfaceOutputCustomLightingCustom o )
		{
			o.SurfInput = i;
			o.Normal = float3(0,0,1);
			float4 ase_screenPos = float4( i.screenPos.xyz , i.screenPos.w + 0.00000000001 );
			float4 ase_grabScreenPos = ASE_ComputeGrabScreenPos( ase_screenPos );
			float4 ase_grabScreenPosNorm = ase_grabScreenPos / ase_grabScreenPos.w;
			float3 ase_worldNormal = WorldNormalVector( i, float3( 0, 0, 1 ) );
			float3 ase_normWorldNormal = normalize( ase_worldNormal );
			float3 temp_output_271_0 = mul( float4( ase_normWorldNormal , 0.0 ), UNITY_MATRIX_V ).xyz;
			float2 uv_BumpMap = i.uv_texcoord * _BumpMap_ST.xy + _BumpMap_ST.zw;
			float3 NormalmapXYZ536 = UnpackScaleNormal( tex2D( _BumpMap, uv_BumpMap ), _BumpScale );
			float3 temp_output_272_0 = ( temp_output_271_0 + NormalmapXYZ536 );
			float TranslationSpeed329 = _TranslationSpeed;
			float2 temp_cast_12 = (TranslationSpeed329).xx;
			float2 uv_DetailMap = i.uv_texcoord * _DetailMap_ST.xy + _DetailMap_ST.zw;
			float TexturesScale451 = _TexturesScale;
			float3 ase_vertex3Pos = mul( unity_WorldToObject, float4( i.worldPos , 1 ) );
			float4 unityObjectToClipPos420 = UnityObjectToClipPos( ase_vertex3Pos );
			float4 computeScreenPos421 = ComputeScreenPos( unityObjectToClipPos420 );
			float4 unityObjectToClipPos437 = UnityObjectToClipPos( float3(0,0,0) );
			float4 computeScreenPos438 = ComputeScreenPos( unityObjectToClipPos437 );
			float3 ase_worldPos = i.worldPos;
			float4 transform428 = mul(unity_ObjectToWorld,float4( ase_worldPos , 0.0 ));
			float4 WorldProjection345 = ( ( ( ( computeScreenPos421 / (computeScreenPos421).w ) - ( computeScreenPos438 / (computeScreenPos438).w ) ) * _TexturesScale ) * distance( ( float4( _WorldSpaceCameraPos , 0.0 ) - transform428 ) , float4( 0,0,0,0 ) ) );
			#if defined(_UVSCREENPROJECTION_UVPROJECTION)
				float4 staticSwitch394 = float4( ( uv_DetailMap * TexturesScale451 ), 0.0 , 0.0 );
			#elif defined(_UVSCREENPROJECTION_SCREENPROJECTION)
				float4 staticSwitch394 = WorldProjection345;
			#else
				float4 staticSwitch394 = float4( ( uv_DetailMap * TexturesScale451 ), 0.0 , 0.0 );
			#endif
			float RotationSpeed330 = _RotationSpeed;
			float mulTime304 = _Time.y * RotationSpeed330;
			float RotationAngle331 = _RotationAngle;
			float cos301 = cos( ( mulTime304 + radians( RotationAngle331 ) ) );
			float sin301 = sin( ( mulTime304 + radians( RotationAngle331 ) ) );
			float2 rotator301 = mul( staticSwitch394.xy - float2( 0.5,0.5 ) , float2x2( cos301 , -sin301 , sin301 , cos301 )) + float2( 0.5,0.5 );
			float2 panner282 = ( _Time.x * temp_cast_12 + rotator301);
			float4 tex2DNode295 = tex2D( _DetailMap, panner282 );
			float2 temp_cast_18 = (-TranslationSpeed329).xx;
			float mulTime317 = _Time.y * -RotationSpeed330;
			float cos322 = cos( ( mulTime317 + radians( -RotationAngle331 ) ) );
			float sin322 = sin( ( mulTime317 + radians( -RotationAngle331 ) ) );
			float2 rotator322 = mul( staticSwitch394.xy - float2( 0.5,0.5 ) , float2x2( cos322 , -sin322 , sin322 , cos322 )) + float2( 0.5,0.5 );
			float2 panner325 = ( _Time.x * temp_cast_18 + rotator322);
			#ifdef _DUPLICATEDETAILS_ON
				float4 staticSwitch341 = ( CalculateContrast(_ContrastDetailMap,( tex2D( _DetailMap, ( 1.0 - panner325 ) ) + tex2DNode295 )) + _SpreadDetailMap );
			#else
				float4 staticSwitch341 = ( CalculateContrast(( _ContrastDetailMap * 1.25 ),tex2DNode295) + _SpreadDetailMap );
			#endif
			float4 NoiseMapRefraction484 = staticSwitch341;
			float4 temp_output_482_0 = ( float4( temp_output_271_0 , 0.0 ) + NoiseMapRefraction484 );
			#if defined(_MAPCONTRIBUTION_NONE)
				float4 staticSwitch480 = float4( temp_output_271_0 , 0.0 );
			#elif defined(_MAPCONTRIBUTION_NORMALMAP)
				float4 staticSwitch480 = float4( temp_output_272_0 , 0.0 );
			#elif defined(_MAPCONTRIBUTION_DETAILMAP)
				float4 staticSwitch480 = temp_output_482_0;
			#elif defined(_MAPCONTRIBUTION_BOTH)
				float4 staticSwitch480 = ( float4( temp_output_272_0 , 0.0 ) + temp_output_482_0 );
			#else
				float4 staticSwitch480 = float4( temp_output_271_0 , 0.0 );
			#endif
			float4 screenColor15 = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_ScreenGrab0,( (ase_grabScreenPosNorm).xyzw + (( staticSwitch480 * (-1.0 + (_Refraction - 0.0) * (1.0 - -1.0) / (2.0 - 0.0)) )).rgba ).xy);
			float3 desaturateInitialColor278 = screenColor15.rgb;
			float desaturateDot278 = dot( desaturateInitialColor278, float3( 0.299, 0.587, 0.114 ));
			float3 desaturateVar278 = lerp( desaturateInitialColor278, desaturateDot278.xxx, _DesaturateBackground );
			float OpacitySlider208 = ( 1.0 - _Opacity );
			float4 lerpResult258 = lerp( float4( desaturateVar278 , 0.0 ) , _FillColorBackground , OpacitySlider208);
			float4 GrabSreenRefraction16 = lerpResult258;
			float3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float clampResult80 = clamp( _FresnelIntensity , 0.0 , 200.0 );
			float fresnelNdotV72 = dot( ase_worldNormal, ase_worldViewDir );
			float fresnelNode72 = ( _FresnelBias + clampResult80 * pow( 1.0 - fresnelNdotV72, _FresnelPower ) );
			float4 NoiseMap288 = ( staticSwitch341 * _DetailScale );
			float mulTime527 = _Time.y * _AmplitudeSpeed;
			float lerpResult531 = lerp( _MaxValueAmplitude , _MinValueAmplitude , sin( mulTime527 ));
			float4 clampResult35 = clamp( ( (( _Invert )?( ( 1.0 - fresnelNode72 ) ):( fresnelNode72 )) * NoiseMap288 * lerpResult531 ) , float4( 0,0,0,0 ) , float4( 1,1,1,0 ) );
			float4 FresnelMask197 = clampResult35;
			float4 lerpResult225 = lerp( GrabSreenRefraction16 , _FresnelColor , FresnelMask197);
			float4 Emission263 = ( _SelfIllumination * lerpResult225 );
			o.Emission = Emission263.rgb;
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf StandardCustomLighting keepalpha fullforwardshadows 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#pragma multi_compile_shadowcaster
			#pragma multi_compile UNITY_PASS_SHADOWCASTER
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			#include "HLSLSupport.cginc"
			#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
				#define CAN_SKIP_VPOS
			#endif
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			sampler3D _DitherMaskLOD;
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float2 customPack1 : TEXCOORD1;
				float4 screenPos : TEXCOORD2;
				float4 tSpace0 : TEXCOORD3;
				float4 tSpace1 : TEXCOORD4;
				float4 tSpace2 : TEXCOORD5;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				Input customInputData;
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				half3 worldTangent = UnityObjectToWorldDir( v.tangent.xyz );
				half tangentSign = v.tangent.w * unity_WorldTransformParams.w;
				half3 worldBinormal = cross( worldNormal, worldTangent ) * tangentSign;
				o.tSpace0 = float4( worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x );
				o.tSpace1 = float4( worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y );
				o.tSpace2 = float4( worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z );
				o.customPack1.xy = customInputData.uv_texcoord;
				o.customPack1.xy = v.texcoord;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				o.screenPos = ComputeScreenPos( o.pos );
				return o;
			}
			half4 frag( v2f IN
			#if !defined( CAN_SKIP_VPOS )
			, UNITY_VPOS_TYPE vpos : VPOS
			#endif
			) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				Input surfIN;
				UNITY_INITIALIZE_OUTPUT( Input, surfIN );
				surfIN.uv_texcoord = IN.customPack1.xy;
				float3 worldPos = float3( IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w );
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldPos = worldPos;
				surfIN.worldNormal = float3( IN.tSpace0.z, IN.tSpace1.z, IN.tSpace2.z );
				surfIN.internalSurfaceTtoW0 = IN.tSpace0.xyz;
				surfIN.internalSurfaceTtoW1 = IN.tSpace1.xyz;
				surfIN.internalSurfaceTtoW2 = IN.tSpace2.xyz;
				surfIN.screenPos = IN.screenPos;
				SurfaceOutputCustomLightingCustom o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputCustomLightingCustom, o )
				surf( surfIN, o );
				UnityGI gi;
				UNITY_INITIALIZE_OUTPUT( UnityGI, gi );
				o.Alpha = LightingStandardCustomLighting( o, worldViewDir, gi ).a;
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				half alphaRef = tex3D( _DitherMaskLOD, float3( vpos.xy * 0.25, o.Alpha * 0.9375 ) ).a;
				clip( alphaRef - 0.01 );
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
}