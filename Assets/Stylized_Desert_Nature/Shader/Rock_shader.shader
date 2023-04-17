// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Rock_shader"
{
	Properties
	{
		_Albedo("Albedo", 2D) = "white" {}
		_TextureColor("Texture Color", Color) = (0,0,0,0)
		_color_intensity("color_intensity", Range( 0 , 1)) = 0
		_MSAO("MSAO", 2D) = "white" {}
		_Normal("Normal", 2D) = "bump" {}
		_SmothnessIntensity("SmothnessIntensity", Range( 0 , 1)) = 0
		_AOIntensity("AOIntensity", Range( 0 , 1)) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Back
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform sampler2D _Normal;
		uniform float4 _Normal_ST;
		uniform sampler2D _Albedo;
		uniform float4 _Albedo_ST;
		uniform float4 _TextureColor;
		uniform float _color_intensity;
		uniform sampler2D _MSAO;
		uniform float4 _MSAO_ST;
		uniform float _SmothnessIntensity;
		uniform float _AOIntensity;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_Normal = i.uv_texcoord * _Normal_ST.xy + _Normal_ST.zw;
			o.Normal = UnpackNormal( tex2D( _Normal, uv_Normal ) );
			float2 uv_Albedo = i.uv_texcoord * _Albedo_ST.xy + _Albedo_ST.zw;
			float4 tex2DNode1 = tex2D( _Albedo, uv_Albedo );
			float4 blendOpSrc2 = tex2DNode1;
			float4 blendOpDest2 = _TextureColor;
			float4 lerpBlendMode2 = lerp(blendOpDest2,(( blendOpDest2 > 0.5 ) ? ( 1.0 - 2.0 * ( 1.0 - blendOpDest2 ) * ( 1.0 - blendOpSrc2 ) ) : ( 2.0 * blendOpDest2 * blendOpSrc2 ) ),_color_intensity);
			float4 lerpResult8 = lerp( tex2DNode1 , ( saturate( lerpBlendMode2 )) , _color_intensity);
			o.Albedo = lerpResult8.rgb;
			float2 uv_MSAO = i.uv_texcoord * _MSAO_ST.xy + _MSAO_ST.zw;
			float4 tex2DNode6 = tex2D( _MSAO, uv_MSAO );
			o.Metallic = tex2DNode6.r;
			o.Smoothness = ( tex2DNode6.g * _SmothnessIntensity );
			o.Occlusion = pow( tex2DNode6.b , _AOIntensity );
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=17700
-106;119;1906;822;2122.473;1154.41;1.802147;True;True
Node;AmplifyShaderEditor.RangedFloatNode;5;-967.793,77.88226;Inherit;False;Property;_color_intensity;color_intensity;2;0;Create;True;0;0;False;0;0;0.306;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;1;-1085.924,-693.8547;Inherit;True;Property;_Albedo;Albedo;0;0;Create;True;0;0;False;0;-1;None;d95cd6c9a75c84542a41f6dad36d201a;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;4;-962.9218,-163.2392;Inherit;False;Property;_TextureColor;Texture Color;1;0;Create;True;0;0;False;0;0,0,0,0;1,0.5894524,0.3235294,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.BlendOpsNode;2;-603.6747,-198.555;Inherit;False;Overlay;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;13;-895.049,787.0474;Inherit;False;Property;_AOIntensity;AOIntensity;6;0;Create;True;0;0;False;0;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;6;-1176.202,283.2083;Inherit;True;Property;_MSAO;MSAO;3;0;Create;True;0;0;False;0;-1;None;8cca8264bed01ce44885e05401140204;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;10;-921.1746,672.5947;Inherit;False;Property;_SmothnessIntensity;SmothnessIntensity;5;0;Create;True;0;0;False;0;0;0.306;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;8;-219.6613,-286.9475;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;7;-1201.085,-30.29111;Inherit;True;Property;_Normal;Normal;4;0;Create;True;0;0;False;0;-1;None;71a23383653b360438b9619267e52682;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;9;-541.7408,439.9583;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;14;-209.0219,633.6978;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;144.1716,-105.0224;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;Rock_shader;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;2;0;1;0
WireConnection;2;1;4;0
WireConnection;2;2;5;0
WireConnection;8;0;1;0
WireConnection;8;1;2;0
WireConnection;8;2;5;0
WireConnection;9;0;6;2
WireConnection;9;1;10;0
WireConnection;14;0;6;3
WireConnection;14;1;13;0
WireConnection;0;0;8;0
WireConnection;0;1;7;0
WireConnection;0;3;6;1
WireConnection;0;4;9;0
WireConnection;0;5;14;0
ASEEND*/
//CHKSM=BE586810EFFBE569EE8ED4405709C875A453E632