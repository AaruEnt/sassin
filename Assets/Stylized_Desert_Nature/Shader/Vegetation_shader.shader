// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Vegetation"
{
	Properties
	{
		_Cutoff( "Mask Clip Value", Float ) = 0.5
		_Albedo_Color("Albedo_Color", Color) = (0,0,0,0)
		_Color_intensity("Color_intensity", Range( 0 , 1)) = 0
		_Albedo("Albedo", 2D) = "white" {}
		_MSAO("MSAO", 2D) = "white" {}
		_Smooth_intensity("Smooth_intensity", Range( 0 , 2)) = 0
		_Normals1("Normals", 2D) = "bump" {}
		_RimColor1("RimColor", Color) = (0,0,0,0)
		_Rim_Power("Rim_Power", Range( 0 , 10)) = 0
		_Rim_Intensity("Rim_Intensity", Range( 0 , 1)) = 0
		_Wind_Directions("Wind_Directions", Range( 0 , 10)) = 0
		_Wind_Blend("Wind_Blend", Range( 0 , 1)) = 0
		_Wind_Speed("Wind_Speed", Range( 0 , 10)) = 0
		_Wind_Frequency("Wind_Frequency", Range( 0 , 10)) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "TransparentCutout"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Off
		CGINCLUDE
		#include "UnityShaderVariables.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
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
			float3 worldPos;
			float2 uv_texcoord;
			float3 viewDir;
			INTERNAL_DATA
		};

		uniform float _Wind_Directions;
		uniform float _Wind_Frequency;
		uniform float _Wind_Speed;
		uniform float _Wind_Blend;
		uniform sampler2D _Normals1;
		uniform float4 _Normals1_ST;
		uniform sampler2D _Albedo;
		uniform float4 _Albedo_ST;
		uniform float4 _Albedo_Color;
		uniform float _Color_intensity;
		uniform float _Rim_Power;
		uniform float4 _RimColor1;
		uniform float _Rim_Intensity;
		uniform sampler2D _MSAO;
		uniform float4 _MSAO_ST;
		uniform float _Smooth_intensity;
		uniform float _Cutoff = 0.5;


		float3 RotateAroundAxis( float3 center, float3 original, float3 u, float angle )
		{
			original -= center;
			float C = cos( angle );
			float S = sin( angle );
			float t = 1 - C;
			float m00 = t * u.x * u.x + C;
			float m01 = t * u.x * u.y - S * u.z;
			float m02 = t * u.x * u.z + S * u.y;
			float m10 = t * u.x * u.y + S * u.z;
			float m11 = t * u.y * u.y + C;
			float m12 = t * u.y * u.z - S * u.x;
			float m20 = t * u.x * u.z - S * u.y;
			float m21 = t * u.y * u.z + S * u.x;
			float m22 = t * u.z * u.z + C;
			float3x3 finalMatrix = float3x3( m00, m01, m02, m10, m11, m12, m20, m21, m22 );
			return mul( finalMatrix, original ) + center;
		}


		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float3 ase_vertex3Pos = v.vertex.xyz;
			float3 ase_worldPos = mul( unity_ObjectToWorld, v.vertex );
			float temp_output_10_0 = ( ( ase_vertex3Pos.y * cos( ( ( ( ase_worldPos.x + ase_worldPos.z ) * _Wind_Frequency ) + ( _Time.y * _Wind_Speed ) ) ) ) * _Wind_Blend );
			float4 appendResult12 = (float4(temp_output_10_0 , 0.0 , temp_output_10_0 , 0.0));
			float4 break15 = mul( appendResult12, unity_ObjectToWorld );
			float4 appendResult16 = (float4(break15.x , 0 , break15.z , 0.0));
			float3 rotatedValue18 = RotateAroundAxis( float3( 0,0,0 ), appendResult16.xyz, float3( 0,0,0 ), _Wind_Directions );
			v.vertex.xyz += rotatedValue18;
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_Normals1 = i.uv_texcoord * _Normals1_ST.xy + _Normals1_ST.zw;
			float3 tex2DNode25 = UnpackNormal( tex2D( _Normals1, uv_Normals1 ) );
			o.Normal = tex2DNode25;
			float2 uv_Albedo = i.uv_texcoord * _Albedo_ST.xy + _Albedo_ST.zw;
			float4 tex2DNode33 = tex2D( _Albedo, uv_Albedo );
			float4 blendOpSrc38 = tex2DNode33;
			float4 blendOpDest38 = _Albedo_Color;
			float4 lerpResult36 = lerp( tex2DNode33 , ( saturate( ( 1.0 - ( ( 1.0 - blendOpDest38) / max( blendOpSrc38, 0.00001) ) ) )) , _Color_intensity);
			o.Albedo = lerpResult36.rgb;
			float3 normalizeResult24 = normalize( i.viewDir );
			float dotResult26 = dot( tex2DNode25 , normalizeResult24 );
			o.Emission = ( ( pow( ( 1.0 - saturate( dotResult26 ) ) , _Rim_Power ) * _RimColor1 ) * _Rim_Intensity ).rgb;
			float2 uv_MSAO = i.uv_texcoord * _MSAO_ST.xy + _MSAO_ST.zw;
			float4 tex2DNode34 = tex2D( _MSAO, uv_MSAO );
			o.Metallic = tex2DNode34.r;
			o.Smoothness = ( tex2DNode34.g * _Smooth_intensity );
			o.Occlusion = tex2DNode34.b;
			o.Alpha = 1;
			clip( tex2DNode33.a - _Cutoff );
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Standard keepalpha fullforwardshadows vertex:vertexDataFunc 

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
			#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
				#define CAN_SKIP_VPOS
			#endif
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float2 customPack1 : TEXCOORD1;
				float4 tSpace0 : TEXCOORD2;
				float4 tSpace1 : TEXCOORD3;
				float4 tSpace2 : TEXCOORD4;
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
				vertexDataFunc( v, customInputData );
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
				surfIN.viewDir = IN.tSpace0.xyz * worldViewDir.x + IN.tSpace1.xyz * worldViewDir.y + IN.tSpace2.xyz * worldViewDir.z;
				surfIN.worldPos = worldPos;
				surfIN.internalSurfaceTtoW0 = IN.tSpace0.xyz;
				surfIN.internalSurfaceTtoW1 = IN.tSpace1.xyz;
				surfIN.internalSurfaceTtoW2 = IN.tSpace2.xyz;
				SurfaceOutputStandard o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputStandard, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=17700
7;187;1906;1014;388.027;-192.5736;1;True;True
Node;AmplifyShaderEditor.WorldPosInputsNode;1;-2249.854,351.64;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;4;-1998.854,522.64;Inherit;False;Property;_Wind_Frequency;Wind_Frequency;13;0;Create;True;0;0;False;0;0;3.41;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;21;-2059.838,885.8079;Inherit;False;Property;_Wind_Speed;Wind_Speed;12;0;Create;True;0;0;False;0;0;3.12;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;2;-1901.854,331.64;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TimeNode;5;-2151.471,685.6924;Inherit;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;22;-1702.538,718.8079;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;3;-1663.854,376.64;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;6;-1445.078,500.0757;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;23;-1007.074,107.15;Float;False;Tangent;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.PosVertexDataNode;8;-1252.516,459.8013;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CosOpNode;7;-1368.304,860.0291;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;11;-1303.934,1031.635;Inherit;False;Property;_Wind_Blend;Wind_Blend;11;0;Create;True;0;0;False;0;0;0.044;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;9;-1148.054,766.8943;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;25;30.80452,1348.82;Inherit;True;Property;_Normals1;Normals;6;0;Create;True;0;0;False;0;-1;None;73de2c25c80bfdc4f8503d207496dc56;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;0,0;False;1;FLOAT2;1,0;False;2;FLOAT;1;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.NormalizeNode;24;-784.3769,104.0501;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.DotProductOpNode;26;-592.6769,25.6497;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;10;-944.3384,718.0341;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;12;-751.7762,744.4641;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SaturateNode;27;-417.478,1.149579;Inherit;False;1;0;FLOAT;1.23;False;1;FLOAT;0
Node;AmplifyShaderEditor.ObjectToWorldMatrixNode;14;-812.1877,1038.971;Inherit;False;0;1;FLOAT4x4;0
Node;AmplifyShaderEditor.OneMinusNode;28;-228.4752,-1.551208;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;13;-541.5934,754.5329;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT4x4;0,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;29;-492.0791,149.449;Float;False;Property;_Rim_Power;Rim_Power;8;0;Create;True;0;0;False;0;0;2.41;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector3Node;17;-657.9343,590.1976;Inherit;False;Constant;_Vector0;Vector 0;2;0;Create;True;0;0;False;0;0,0,0;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.ColorNode;37;333.6448,461.8637;Float;False;Property;_Albedo_Color;Albedo_Color;1;0;Create;True;0;0;False;0;0,0,0,0;0.5753044,0.625,0.1102941,0;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;33;209.757,183.0996;Inherit;True;Property;_Albedo;Albedo;3;0;Create;True;0;0;False;0;-1;None;d1e38600101d3344e9cd714613bee2c2;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.BreakToComponentsNode;15;-341.4796,782.2216;Inherit;False;FLOAT4;1;0;FLOAT4;0,0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.ColorNode;31;-188.8767,325.6488;Float;False;Property;_RimColor1;RimColor;7;0;Create;True;0;0;False;0;0,0,0,0;0.2056922,0.625,0.004595593,0;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PowerNode;30;-53.67723,66.8494;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;16;0.8536377,827.5304;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SamplerNode;34;-53.85229,512.1989;Inherit;True;Property;_MSAO;MSAO;4;0;Create;True;0;0;False;0;-1;None;d9911868be138d24f9589e473ee1fa21;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.BlendOpsNode;38;753.4352,270.2669;Inherit;True;ColorBurn;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;32;229.5533,-79.64605;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;43;1.230142,722.3288;Inherit;False;Property;_Smooth_intensity;Smooth_intensity;5;0;Create;True;0;0;False;0;0;1.05;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;19;-1953.338,1650.38;Inherit;False;Property;_Wind_Directions;Wind_Directions;10;0;Create;True;0;0;False;0;0;0;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;39;688.325,546.5836;Float;False;Property;_Color_intensity;Color_intensity;2;0;Create;True;0;0;False;0;0;0.647;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;41;227.3137,42.01347;Inherit;False;Property;_Rim_Intensity;Rim_Intensity;9;0;Create;True;0;0;False;0;0;0.306;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RotateAboutAxisNode;18;-210.8375,1030.037;Inherit;False;False;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;42;342.7415,809.4584;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;36;1188.645,468.8637;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;40;555.6099,-57.41956;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;745.2943,745.2946;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;Vegetation;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Off;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Custom;0.5;True;True;0;True;TransparentCutout;;Geometry;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;0;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;2;0;1;1
WireConnection;2;1;1;3
WireConnection;22;0;5;2
WireConnection;22;1;21;0
WireConnection;3;0;2;0
WireConnection;3;1;4;0
WireConnection;6;0;3;0
WireConnection;6;1;22;0
WireConnection;7;0;6;0
WireConnection;9;0;8;2
WireConnection;9;1;7;0
WireConnection;24;0;23;0
WireConnection;26;0;25;0
WireConnection;26;1;24;0
WireConnection;10;0;9;0
WireConnection;10;1;11;0
WireConnection;12;0;10;0
WireConnection;12;2;10;0
WireConnection;27;0;26;0
WireConnection;28;0;27;0
WireConnection;13;0;12;0
WireConnection;13;1;14;0
WireConnection;15;0;13;0
WireConnection;30;0;28;0
WireConnection;30;1;29;0
WireConnection;16;0;15;0
WireConnection;16;1;17;2
WireConnection;16;2;15;2
WireConnection;38;0;33;0
WireConnection;38;1;37;0
WireConnection;32;0;30;0
WireConnection;32;1;31;0
WireConnection;18;1;19;0
WireConnection;18;3;16;0
WireConnection;42;0;34;2
WireConnection;42;1;43;0
WireConnection;36;0;33;0
WireConnection;36;1;38;0
WireConnection;36;2;39;0
WireConnection;40;0;32;0
WireConnection;40;1;41;0
WireConnection;0;0;36;0
WireConnection;0;1;25;0
WireConnection;0;2;40;0
WireConnection;0;3;34;1
WireConnection;0;4;42;0
WireConnection;0;5;34;3
WireConnection;0;10;33;4
WireConnection;0;11;18;0
ASEEND*/
//CHKSM=0C935E766AC9DCF5807F6276E4887237170C58ED