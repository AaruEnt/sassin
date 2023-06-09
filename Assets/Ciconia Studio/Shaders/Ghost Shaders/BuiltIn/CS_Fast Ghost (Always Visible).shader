Shader "Ciconia Studio/CS_Ghost/CS_Fast Ghost (Always Visible)"
{
	Properties
	{
		[Space(15)][Header(Fresnel Properties)][Space(15)]_FresnelColor("Color", Color) = (0.6933962,1,0.9814353,1)
		_SelfIllumination("Self Illumination", Range( 1 , 10)) = 1
		[Space(15)]_FresnelIntensity("Fresnel Intensity", Float) = 4
		_FresnelPower("Fresnel Power", Float) = 4
		_FresnelBias("Bias", Range( 0 , 1)) = 0
		[Toggle]_Invert("Invert", Float) = 0
		[Space(15)][Header(Animation Properties)][Space(15)]_MinValueAmplitude("Min Value", Float) = 1
		_MaxValueAmplitude("Max Value", Float) = 2
		[TextArea(1)]_AmplitudeSpeed("Speed", Float) = 1
		[Space(15)][Header(Refraction)][Space(15)]_Refraction("Refraction", Range( 0 , 2)) = 1.1
		[Space(15)][Header(Transparency Properties)][Space(15)]_FillColorBackground("Fill Color Background", Color) = (0,0,0,0)
		_DesaturateBackground("Desaturate Background", Range( 0 , 1)) = 0
		[Space(10)]_Opacity("Opacity", Range( 0 , 1)) = 1
		_ShadowOpacity("Shadow Opacity", Range( 0 , 1)) = 0
		[Space(15)]_FresnelTransIntensity("Fresnel Intensity", Float) = 0
		_FresnelTransPower("Fresnel Power", Float) = 5
		_FresnelTransBias("Bias", Range( 0 , 1)) = 0
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
		Blend SrcAlpha OneMinusSrcAlpha
		
		GrabPass{ "_ScreenGrab0" }
		CGINCLUDE
		#include "UnityShaderVariables.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		#if defined(UNITY_STEREO_INSTANCING_ENABLED) || defined(UNITY_STEREO_MULTIVIEW_ENABLED)
		#define ASE_DECLARE_SCREENSPACE_TEXTURE(tex) UNITY_DECLARE_SCREENSPACE_TEXTURE(tex);
		#else
		#define ASE_DECLARE_SCREENSPACE_TEXTURE(tex) UNITY_DECLARE_SCREENSPACE_TEXTURE(tex)
		#endif
		struct Input
		{
			float4 screenPos;
			float3 worldNormal;
			float3 worldPos;
		};

		uniform float _SelfIllumination;
		ASE_DECLARE_SCREENSPACE_TEXTURE( _ScreenGrab0 )
		uniform float _Refraction;
		uniform float _DesaturateBackground;
		uniform float4 _FillColorBackground;
		uniform float _Opacity;
		uniform float4 _FresnelColor;
		uniform float _Invert;
		uniform float _FresnelBias;
		uniform float _FresnelIntensity;
		uniform float _FresnelPower;
		uniform float _MaxValueAmplitude;
		uniform float _MinValueAmplitude;
		uniform float _AmplitudeSpeed;
		uniform float _ShadowOpacity;
		uniform float _FresnelTransBias;
		uniform float _FresnelTransIntensity;
		uniform float _FresnelTransPower;


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


		inline half4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return half4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			float4 ase_screenPos = float4( i.screenPos.xyz , i.screenPos.w + 0.00000000001 );
			float4 ase_grabScreenPos = ASE_ComputeGrabScreenPos( ase_screenPos );
			float4 ase_grabScreenPosNorm = ase_grabScreenPos / ase_grabScreenPos.w;
			float3 ase_worldNormal = i.worldNormal;
			float3 ase_normWorldNormal = normalize( ase_worldNormal );
			float4 screenColor15 = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_ScreenGrab0,( (ase_grabScreenPosNorm).xyzw + float4( (( mul( float4( ase_normWorldNormal , 0.0 ), UNITY_MATRIX_V ).xyz * (-1.0 + (_Refraction - 0.0) * (1.0 - -1.0) / (2.0 - 0.0)) )).xyz , 0.0 ) ).xy);
			float3 desaturateInitialColor278 = screenColor15.rgb;
			float desaturateDot278 = dot( desaturateInitialColor278, float3( 0.299, 0.587, 0.114 ));
			float3 desaturateVar278 = lerp( desaturateInitialColor278, desaturateDot278.xxx, _DesaturateBackground );
			float OpacitySlider208 = ( 1.0 - _Opacity );
			float4 lerpResult258 = lerp( float4( desaturateVar278 , 0.0 ) , _FillColorBackground , OpacitySlider208);
			float4 GrabSreenRefraction16 = lerpResult258;
			float3 ase_worldPos = i.worldPos;
			float3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float clampResult80 = clamp( _FresnelIntensity , 0.0 , 400.0 );
			float fresnelNdotV72 = dot( ase_worldNormal, ase_worldViewDir );
			float fresnelNode72 = ( _FresnelBias + clampResult80 * pow( 1.0 - fresnelNdotV72, _FresnelPower ) );
			float mulTime526 = _Time.y * _AmplitudeSpeed;
			float lerpResult530 = lerp( _MaxValueAmplitude , _MinValueAmplitude , sin( mulTime526 ));
			float clampResult35 = clamp( ( (( _Invert )?( ( 1.0 - fresnelNode72 ) ):( fresnelNode72 )) * lerpResult530 ) , 0.0 , 1.0 );
			float FresnelMask197 = clampResult35;
			float4 lerpResult225 = lerp( GrabSreenRefraction16 , _FresnelColor , FresnelMask197);
			float4 Emission263 = ( _SelfIllumination * lerpResult225 );
			o.Emission = Emission263.rgb;
			float clampResult2_g1 = clamp( _FresnelTransIntensity , 0.0 , 400.0 );
			float fresnelNdotV5_g1 = dot( ase_worldNormal, ase_worldViewDir );
			float fresnelNode5_g1 = ( _FresnelTransBias + clampResult2_g1 * pow( max( 1.0 - fresnelNdotV5_g1 , 0.0001 ), _FresnelTransPower ) );
			float temp_output_7_0_g1 = saturate( ( 1.0 - fresnelNode5_g1 ) );
			float Opacity250 = ( (0.5 + (( 1.0 - _ShadowOpacity ) - 0.0) * (1.0 - 0.5) / (1.0 - 0.0)) * temp_output_7_0_g1 );
			o.Alpha = Opacity250;
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Unlit keepalpha fullforwardshadows 

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
				float3 worldPos : TEXCOORD1;
				float4 screenPos : TEXCOORD2;
				float3 worldNormal : TEXCOORD3;
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
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				o.worldNormal = worldNormal;
				o.worldPos = worldPos;
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
				float3 worldPos = IN.worldPos;
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldPos = worldPos;
				surfIN.worldNormal = IN.worldNormal;
				surfIN.screenPos = IN.screenPos;
				SurfaceOutput o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutput, o )
				surf( surfIN, o );
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