Shader "Hidden/KriptoFX/KWS/KWS_CombineSeparatedTexturesToArrayVR"
{
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				UNITY_VERTEX_OUTPUT_STEREO
			};

			v2f vert(appdata v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}

			Texture2D KWS_PlanarLeftEye;
			Texture2D KWS_PlanarRightEye;
			SamplerState sampler_linear_clamp;

			float4 frag(v2f i) : SV_Target
			{
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

				if (unity_StereoEyeIndex == 0) return KWS_PlanarLeftEye.Sample(sampler_linear_clamp, i.uv);
				else return KWS_PlanarRightEye.Sample(sampler_linear_clamp, i.uv);
			}
			ENDCG
		}
	}

	//SubShader
	//{
	//    Cull Off
	//    ZWrite Off
	//    ZTest Always

	//    Pass
	//    {
	//        HLSLPROGRAM

	//        #pragma vertex vert
	//        #pragma fragment frag

	//        #include "..\..\PlatformSpecific\KWS_PlatformSpecificHelpers.cginc"

	//        struct appdata_t
	//        {
	//            uint vertexID : SV_VertexID;
	//            UNITY_VERTEX_INPUT_INSTANCE_ID
	//        };

	//        struct v2f
	//        {
	//            float2 uv : TEXCOORD0;
	//            float4 vertex : SV_POSITION;
	//            UNITY_VERTEX_OUTPUT_STEREO
	//        };

	//        v2f vert(appdata_t v)
	//        {
	//            v2f o;
	//            UNITY_SETUP_INSTANCE_ID(v);
	//            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
	//            o.vertex = GetTriangleVertexPosition(v.vertexID);
	//            o.uv = GetTriangleUV(v.vertexID);
	//            return o;
	//        }
	
	//        Texture2D KWS_PlanarLeftEye;
	//        Texture2D KWS_PlanarRightEye;

	//        float4 frag(v2f i) : SV_Target
	//        {
	//            UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
	//            if(unity_StereoEyeIndex == 0) return KWS_PlanarLeftEye.Sample(sampler_linear_clamp, i.uv);
	//            else return KWS_PlanarRightEye.Sample(sampler_linear_clamp, i.uv);
	//        }
	//        ENDHLSL
	//    }
	//}

}