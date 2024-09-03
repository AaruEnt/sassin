Shader "Custom/FishShader" 
{
	Properties{
		_MainTex("Albedo (BW)", 2D) = "white" {}
		_Color1("Color1",Color) = (0,0,0,0)
		_Color2("Color2",Color) = (0,0,0,0)
	
		_GlimmerTex("Glimmer (BW)", 2D) = "white" {}
		_GlimmerColor("Glimmer",Color) = (0,0,0,0)
		_GlimmerAngle("Glimmer Angle", Vector) = (0,.5,0)
		_GlimmerStrength("Glimmer Strength", Int) = 5
	}
	
    SubShader 
	{
		Tags { "RenderType" = "Opaque" }
		//Cull Off
		LOD 200
		CGPROGRAM
		#pragma surface surf Lambert fullforwardshadows
		
		struct Input {
		  float3 worldPos;
		  float3 worldNormal;
		  float2 uv_MainTex    : TEXCOORD0;
		  float2 uv_GlimmerTex	: TEXCOORD1;
		  float3 viewDir;
		};


		sampler2D _MainTex;
		sampler2D _GlimmerTex;
		fixed4 _Color1;
		fixed4 _Color2;

		fixed4 _GlimmerColor;
		fixed4 _GlimmerAngle;
		fixed _GlimmerStrength;

		fixed4 _Color;
		
		// remap value to 0-1 range
		float remap(float value, float minSource, float maxSource)
		{
			return (value-minSource)/(maxSource-minSource);
		}
		
		void surf (Input IN, inout SurfaceOutput o) 
		{
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
			fixed4 c2 = tex2D(_GlimmerTex, IN.uv_GlimmerTex);
			
			float glimmer = dot(normalize(IN.viewDir + _GlimmerAngle),IN.worldNormal);
			glimmer  = pow(glimmer, _GlimmerStrength);

			_Color = lerp(_Color2,_Color1,(c.r + c.g + c.b)/3);
			_Color = lerp(_Color, _GlimmerColor, clamp((c2.r + c2.g + c2.b) / 3 * _GlimmerColor.a * glimmer,0,10));				
			c = _Color;
			o.Albedo = c;
		}
		ENDCG
    } 
    Fallback "Mobile/VertexLit"
}
