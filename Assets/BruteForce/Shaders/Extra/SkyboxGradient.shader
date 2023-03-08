// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Skybox/GradientSkybox"
{
	Properties
	{
		_Color1("TopColor", Color) = (1, 1, 1, 1)
		_Color2("HorizonColor", Color) = (1, 1, 1, 1)
		_Color3("BottomColor", Color) = (1, 1, 1, 1)
		_Intensity1("IntensityTop", Float) = 1.0		
		_Intensity2("IntensityMid", Float) = 1.0
		_Intensity3("IntensityBot", Float) = 1.0
		_Rotation("Rotation", Range(0, 360)) = 0
		[NoScaleOffset] _Tex("Cubemap   (HDR)", Cube) = "black" {}
	}

		CGINCLUDE

#include "UnityCG.cginc"

		struct appdata
	{
		float4 position : POSITION;
		float3 texcoord : TEXCOORD0;
	};

	struct v2f
	{
		float4 position : SV_POSITION;
		float3 texcoord : TEXCOORD0;
	};

	half4 _Color1,_Color3,_Color2;
	half4 _Tex_HDR;
	half _Intensity1, _Intensity2, _Intensity3;
	samplerCUBE _Tex;
	float _Rotation;

	float3 RotateAroundYInDegrees(float3 vertex, float degrees)
	{
		float alpha = degrees * UNITY_PI / 180.0;
		float sina, cosa;
		sincos(alpha, sina, cosa);
		float2x2 m = float2x2(cosa, -sina, sina, cosa);
		return float3(mul(m, vertex.xz), vertex.y).xzy;
	}

	v2f vert(appdata v)
	{
		v2f o;            
		float3 rotated = RotateAroundYInDegrees(v.position, _Rotation);

		o.position = UnityObjectToClipPos(rotated);
		o.texcoord = v.texcoord;
		return o;
	}

	half4 frag(v2f i) : COLOR
	{
		half4 tex = texCUBE(_Tex, i.texcoord);     
		half3 c = DecodeHDR(tex, _Tex_HDR);
		c = c * unity_ColorSpaceDouble.rgb;

		float p = normalize(i.texcoord).y;
		float p1 = 1.0f - pow(min(1.0f, 1.0f - p), _Intensity1);
		float p3 = 1.0f - pow(min(1.0f, 1.0f + p), _Intensity3);
		float p2 = 1.0f - p1 - p3;
		float3 col01 = _Color1 * c;
		float3 col02 = _Color2 * c;
		float3 col03 = _Color3 * c;
		return float4((col01 * p1 + col02 * p2 + col03 * p3) * _Intensity2,1);
	}

		ENDCG

		SubShader
	{
		Tags{ "RenderType" = "Background" "Queue" = "Background" }
			Pass
		{
			ZWrite Off
			Cull Off
			Fog { Mode Off }
			CGPROGRAM
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma vertex vert
			#pragma fragment frag
			ENDCG
		}
	}
}