Shader "Custom/Fancy Water"
{
	Properties
	{
		//_Color("Tint", Color) = (1, 1, 1, .5)
		_MainTex("Main Texture", 2D) = "white" {}
		_TextureDistort("Texture Wobble", range(0,1)) = 0.1
		_NoiseTex("Extra Wave Noise", 2D) = "white" {}
		_Speed("Wave Speed", Range(0,1)) = 0.5
		_Amount("Wave Amount", Range(0,1)) = 0.6
		_Scale("Scale", Range(0,1)) = 0.5
		_Height("Wave Height", Range(0,1)) = 0.1
		
		_NearColor("NearColor", Color) = (1, 1, 1, 1)
		_DistanceColor("DistanceColor", Color) = (1, 1, 1, 1)
		_ColorFade("ColorFade", Range(0,1)) = 0.5
		_DistanceFog("DistanceFade", Range(0,1)) = 0.5

	}
		SubShader
		{
			Tags { "RenderType" = "Opaque"  "Queue" = "Transparent" }
			LOD 100
			//Blend DstAlpha One
			Blend OneMinusDstColor One
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
				};

				struct v2f
				{
					float2 uv : TEXCOORD3;
					float4 vertex : SV_POSITION;
					float4 scrPos : TEXCOORD2;//
					float4 worldPos : TEXCOORD4;//
				};
				float _TextureDistort;
				float4 _Color;
				float4 _DistanceColor;
				float4 _NearColor;
				sampler2D _CameraDepthTexture; //Depth Texture
				sampler2D _MainTex, _NoiseTex;//
				float4 _MainTex_ST;
				float _Speed, _Amount, _Height, _Foam, _Scale, _DistanceFog,_ColorFade;//
				//float4 _FoamC;
				float remap(float value, float minSource, float maxSource)
				{
					return (value - minSource) / (maxSource - minSource);
				}
				v2f vert(appdata v)	
				{
					v2f o;
					UNITY_INITIALIZE_OUTPUT(v2f, o);
					float4 tex = tex2Dlod(_NoiseTex, float4(v.uv.xy, 0, 0));//extra noise tex
					v.vertex.y += sin(_Time.z * _Speed + (v.vertex.x * v.vertex.z * _Amount * tex)) * _Height;//movement
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.worldPos = mul(unity_ObjectToWorld, v.vertex);

					o.uv = TRANSFORM_TEX(v.uv, _MainTex);
					o.scrPos = ComputeScreenPos(o.vertex);
					return o;
				}

				fixed4 frag(v2f i) : SV_Target
				{
					// sample the texture
					fixed distortx = tex2D(_NoiseTex, (i.worldPos.xz * _Scale) + (_Time.x * 2)).r;// distortion alpha
					float4 tex = tex2D(_MainTex, (i.worldPos.xz * _Scale) - (distortx * _TextureDistort));// texture times tint;        
					half depth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.scrPos))); // depth
					//col = saturate(col) * col.a;

					
					float camDist = distance(i.worldPos, _WorldSpaceCameraPos);
					float a1 = clamp(camDist *_ColorFade * _ColorFade,0,1);
					float a2 = clamp(camDist * _DistanceFog * _DistanceFog,0,1);
					
					//col *= lerp(_NearColor, _DistanceColor, a1);

					float4 col = lerp(_NearColor,tex , a1);
					col = lerp(col, _DistanceColor * tex, a1);
					
					col -= a2;
					//col *= tex;
					
					return   col;
				}

				
				ENDCG
			}
		}
}