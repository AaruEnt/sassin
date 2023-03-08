Shader "Custom/CustomShadingURP"
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
		_TopColor("TopColor", Color) = (1,1,1,1)

		_TopHeight("_TopHeight", Float) = 5
		_FadeHeight("_FadeHeight", Float) = 5
		_TopHeightSand("_TopHeightSand", Float) = 5
		_FadeHeightSand("_FadeHeightSand", Float) = 5
		_BotColor("BotColor", Color) = (1,1,1,1)
		_MainTex("Main Texture", 2D) = "white" {}
		_Noise("Noise", 2D) = "white" {}
		_SandTransition("SandTransition", 2D) = "white" {}

		_ShadowBiasSand("ShadowBiasSand", Float) = 1

		_ColorYUpTop("_ColorYUpTop", Color) = (1,1,1,1)
		// Ambient light is applied uniformly to all surfaces on the object.
		[HDR]
		_AmbientColor("Ambient Color", Color) = (0.4,0.4,0.4,1)
		[HDR]
		_SpecularColor("Specular Color", Color) = (0.9,0.9,0.9,1)
		// Controls the size of the specular reflection.
		_Glossiness("Glossiness", Float) = 32
		[HDR]
		_RimColor("Rim Color", Color) = (1,1,1,1)
		_RimAmount("Rim Amount", Range(0, 1)) = 0.716
		// Control how smoothly the rim blends when approaching unlit
		// parts of the surface.
		_RimThreshold("Rim Threshold", Range(0, 1)) = 0.1
		_TileWorld("TileWorld", Float) = 2
		_MaskColor("_MaskColor", Color) = (1,1,1,1)
		_SandColor("SandColor", Color) = (1,1,1,1)
		_SandShadowColor("SandShadowColor", Color) = (1,1,1,1)
		_RedColor("RedColor", Color) = (1,1,1,1)
		_GreenColor("GreenColor", Color) = (1,1,1,1)
		_BlueColor("BlueColor", Color) = (1,1,1,1)
	}
		SubShader
		{
			Pass
			{
				// Setup our pass to use Forward rendering, and only receive
				// data on the main directional light and ambient light.
				Tags
				{
							"RenderPipeline" = "UniversalRenderPipeline"
				}

				HLSLPROGRAM
				#pragma vertex vert
				#pragma fragment frag
			// Compile multiple versions of this shader depending on lighting settings.
			#pragma multi_compile_fwdbase
		#pragma multi_compile_fog

			/*
			#include "UnityCG.cginc"
			// Files below include macros and functions to assist
			// with lighting and shadows.
			#include "Lighting.cginc"
			#include "AutoLight.cginc"
			*/

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"


			struct appdata
			{
				float4 vertex : POSITION;
				float4 uv : TEXCOORD0;
				float3 normal : NORMAL;
				float fogCoord : TEXCOORD1;
					float4 color : COLOR;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				float3 worldNormal : NORMAL;
				float2 uv : TEXCOORD0;
				float3 viewDir : TEXCOORD1;

				float4 color : COLOR;
				float3 worldPos : TEXCOORD3;
				// Macro found in Autolight.cginc. Declares a vector4
				// into the TEXCOORD2 semantic with varying precision 
				// depending on platform target.
				float4 shadowCoord : TEXCOORD2;
				float fogCoord : TEXCOORD4;
			};

			sampler2D _MainTex;
			sampler2D _Noise;
			sampler2D _SandTransition;
			float4 _MainTex_ST;
			float4 _SandColor;
			float4 _SandShadowColor;
			float4 _RedColor;
			float4 _GreenColor;
			float4 _BlueColor;

			v2f vert(appdata v)
			{
				v2f o;
				VertexPositionInputs vertexInput = GetVertexPositionInputs(v.vertex.xyz);

				//o.pos = UnityObjectToClipPos(v.vertex);
				o.pos = GetVertexPositionInputs(v.vertex).positionCS;
				o.worldNormal = TransformObjectToWorldNormal(v.normal);
				v.normal = float3(0, 1, 0);

				float4 objCam = mul(unity_WorldToObject, float4(_WorldSpaceCameraPos, 1.0));
				float3 viewDir = v.vertex.xyz - objCam.xyz;

				o.viewDir = viewDir;
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex);
				o.color = v.color;
				// Defined in Autolight.cginc. Assigns the above shadow coordinate
				// by transforming the vertex from world space to shadow-map space.
				o.shadowCoord = GetShadowCoord(vertexInput);
				o.fogCoord = ComputeFogFactor(vertexInput.positionCS.z);

				return o;
			}

			float4 _Color;
			float4 _TopColor;
			float4 _BotColor;
			float4 _MaskColor;
			float4 _ColorYUpTop;

			float4 _AmbientColor;

			float4 _SpecularColor;
			float _Glossiness, _TopHeightSand, _FadeHeightSand;
			float _TileWorld, _ShadowBiasSand;
			float _TopHeight;
			float _FadeHeight;

			float4 _RimColor;
			float _RimAmount;
			float _RimThreshold;

			float4 frag(v2f i) : SV_Target
			{
				Light mainLight = GetMainLight(i.shadowCoord);
				half4 lightColor = half4(mainLight.color.rgb, (mainLight.color.r + mainLight.color.g + mainLight.color.b) / 3);

				float3 redColor = lerp(_TopColor.rgb,_RedColor.rgb,i.color.r);
				float3 greenColor = lerp(_TopColor.rgb, _GreenColor.rgb,i.color.g);
				float3 blueColor = lerp(_TopColor.rgb, _BlueColor.rgb,i.color.b);

				float3 allColors = redColor + greenColor + blueColor;

				float3 normal = normalize(i.worldNormal);
				float3 viewDir = normalize(i.viewDir);

				// Calculate illumination from directional light.
				// _WorldSpaceLightPos0 is a vector pointing the OPPOSITE
				// direction of the main directional light.
				float NdotL = dot(_MainLightPosition.xyz, normal);

				// Samples the shadow map, returning a value in the 0...1 range,
				// where 0 is in the shadow, and 1 is not.
				//float shadow = SHADOW_ATTENUATION(i);
				float shadow = mainLight.shadowAttenuation;

				// Partition the intensity into light and dark, smoothly interpolated
				// between the two to avoid a jagged break.
				float lightIntensity = smoothstep(0, 0.01, NdotL * shadow);
				// Multiply by the main directional light's intensity and color.
				float4 light = lightIntensity * lightColor;

				// Calculate specular reflection.
				float3 halfVector = normalize(_MainLightPosition.xyz + viewDir);
				float NdotH = dot(normal, halfVector);
				// Multiply _Glossiness by itself to allow artist to use smaller
				// glossiness values in the inspector.
				float specularIntensity = pow(NdotH * lightIntensity, _Glossiness * _Glossiness);
				float specularIntensitySmooth = smoothstep(0.005, 0.01, specularIntensity);
				float4 specular = specularIntensitySmooth * _SpecularColor;

				// Calculate rim lighting.
				float rimDot = 1 - dot(viewDir, normal);
				// We only want rim to appear on the lit side of the surface,
				// so multiply it by NdotL, raised to a power to smoothly blend it.
				float rimIntensity = rimDot * pow(NdotL, _RimThreshold);
				rimIntensity = smoothstep(_RimAmount - 0.01, _RimAmount + 0.01, rimIntensity);
				float4 rim = rimIntensity * _RimColor;


				float4 col = (light + _AmbientColor + specular + rim);

				col = lerp(col, _MaskColor, saturate(1*pow(tex2D(_Noise , i.uv.xy * _TileWorld),0.5)* (tex2D(_MainTex, i.uv.xy * _TileWorld))));
				light.rgb = lerp(light.rgb - _SandShadowColor.rgb , light.rgb, shadow);

				col.rgb = col.rgb * lerp(_BotColor.rgb, allColors, saturate( i.uv.y* _FadeHeight*0.1 + _TopHeight));



				float4 sandTex = tex2D(_SandTransition, i.uv.xy * _TileWorld*9);
				col.rgb =   lerp(_SandColor.a*2 * _SandColor * saturate(light.rgb+ _ShadowBiasSand), col.rgb, saturate(i.uv.y * _FadeHeightSand * 0.1 + ( sandTex.r* sandTex.a+0.5) * _TopHeightSand));
				col = lerp(col, _ColorYUpTop, _ColorYUpTop.a* saturate(pow(normal.y,2)));
				//col = col+ _ColorYUpTop* saturate(pow(normal.y,0.5));
				col.xyz = MixFog(col, i.fogCoord);
				return col;
			}
			ENDHLSL
		}

			// Shadow casting support.
			UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
		}
}