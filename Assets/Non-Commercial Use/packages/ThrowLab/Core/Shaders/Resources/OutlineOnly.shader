Shader "Custom/OutlineOnly" {

	Properties{
		_Color("Color", Color) = (0,0,0,1)
		_Outline("Outline width", Range(0.0, 0.03)) = .03
	}

		CGINCLUDE
#include "UnityCG.cginc"

	struct appdata {
		float4 vertex : POSITION;
		float3 normal : NORMAL;

		UNITY_VERTEX_INPUT_INSTANCE_ID //Insert
	};

	struct v2f {
		float4 pos : POSITION;
		float4 color : COLOR;

		UNITY_VERTEX_OUTPUT_STEREO //Insert
	};

	uniform float _Outline;
	uniform float4 _Color;

	v2f vert(appdata v) {
		
		v2f o;

		UNITY_SETUP_INSTANCE_ID(v); //Insert
		UNITY_INITIALIZE_OUTPUT(v2f, o); //Insert
		UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o); //Insert

		o.pos = v.vertex;
		o.pos.xyz += v.normal.xyz *_Outline;
		o.pos = UnityObjectToClipPos(o.pos);

		o.color = _Color;
		return o;
	}
	ENDCG

		SubShader{
			Tags { "Queue" = "Transparent" }

			Pass {
				Name "BASE"
				Cull Back
				Blend Zero One


		SetTexture[_OutlineColor] {
			ConstantColor(0,0,0,0)
			Combine constant
		}
	}

		// note that a vertex shader is specified here but its using the one above
		Pass {
			Name "OUTLINE"
			Tags { "LightMode" = "Always" }
			Cull Front

		Blend One OneMinusDstColor // Soft Additive


CGPROGRAM
#pragma vertex vert
#pragma fragment frag

half4 frag(v2f i) :COLOR {
	return i.color;
}
ENDCG
		}


	}

		Fallback "Diffuse"
}