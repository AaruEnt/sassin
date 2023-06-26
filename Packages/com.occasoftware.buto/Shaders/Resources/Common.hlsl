#ifndef OCCASOFTWARE_BUTO_COMMON_INCLUDED
#define OCCASOFTWARE_BUTO_COMMON_INCLUDED
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

struct Attributes
{
    float4 positionHCS   : POSITION;
    float2 uv           : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float4  positionCS  : SV_POSITION;
    float2  uv          : TEXCOORD0;
    UNITY_VERTEX_OUTPUT_STEREO
};
			
			
Varyings Vert(Attributes input)
{
    Varyings output;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
				
    output.positionCS = float4(input.positionHCS.xyz, 1.0);

    #if UNITY_UV_STARTS_AT_TOP
    output.positionCS.y *= -1;
    #endif

    output.uv = input.uv;
    return output;
}

SamplerState linear_repeat_sampler;
SamplerState linear_clamp_sampler;
SamplerState point_clamp_sampler;


bool IsOrthographicPerspective()
{
	return unity_OrthoParams.w == 1.0;
}

float CheckDepthDirection(float depthRaw)
{
    #if defined(UNITY_REVERSED_Z)
        depthRaw = 1.0 - depthRaw;
    #endif
    return depthRaw;
}

float GetDepthEye(float depthRaw)
{
    if(IsOrthographicPerspective())
    {
        depthRaw = CheckDepthDirection(depthRaw);
        return lerp(_ProjectionParams.y, _ProjectionParams.z, depthRaw);
    }
                
    return LinearEyeDepth(depthRaw, _ZBufferParams);
}

float GetDepth01(float depthRaw)
{
    if(IsOrthographicPerspective())
    {
        return CheckDepthDirection(depthRaw);
    }
                
    return Linear01Depth(depthRaw, _ZBufferParams);
}

#endif