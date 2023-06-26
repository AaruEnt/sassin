#ifndef OCCASOFTWARE_BUTO_TEMPORAL_AA_INCLUDE
#define OCCASOFTWARE_BUTO_TEMPORAL_AA_INCLUDE


bool IsValidUV(half2 UV)
{
	if (any(UV < 0) || any(UV > 1))
	{
		return false;
	}
	
	return true;
}

half random(half2 seed, half2 dotDir = half2(12.9898, 78.233))
{
	return frac(sin(dot(sin(seed), dotDir)) * 43758.5453);
}

half2 GetTexCoordSize(float scale)
{
	return rcp(_ScreenParams.xy * scale);
}

void TAA(Texture2D HistoricData, Texture2D NewFrameData, float2 UV, float BlendFactor, float2 MotionVector, half Depth, out float3 Color, out float Alpha)
{
	Color = float3(0.0, 0.0, 0.0);
	Alpha = 0.0;
	
	float2 texCoord = GetTexCoordSize(1.0);
	
	float4 newFrame = NewFrameData.SampleLevel(linear_clamp_sampler, UV, 0);
	float2 histUV = UV - MotionVector;
	
	bool isValidHistUV = IsValidUV(histUV);
	if (isValidHistUV)
	{
        float4 histSample = HistoricData.SampleLevel(linear_clamp_sampler, histUV, 0);
		
        float4 newSampleUp = NewFrameData.SampleLevel(linear_clamp_sampler, UV + float2(0.0, -texCoord.y), 0);
        float4 newSampleDown = NewFrameData.SampleLevel(linear_clamp_sampler, UV + float2(0.0, texCoord.y), 0);
        float4 newSampleRight = NewFrameData.SampleLevel(linear_clamp_sampler, UV + float2(-texCoord.x, 0.0), 0);
        float4 newSampleLeft = NewFrameData.SampleLevel(linear_clamp_sampler, UV + float2(texCoord.x, 0.0), 0);
	
        float4 newSampleUpRight = NewFrameData.SampleLevel(linear_clamp_sampler, UV + float2(-texCoord.x, -texCoord.y), 0);;
        float4 newSampleUpLeft = NewFrameData.SampleLevel(linear_clamp_sampler, UV + float2(texCoord.x, -texCoord.y), 0);;
        float4 newSampleDownRight = NewFrameData.SampleLevel(linear_clamp_sampler, UV + float2(-texCoord.x, texCoord.y), 0);;
        float4 newSampleDownLeft = NewFrameData.SampleLevel(linear_clamp_sampler, UV + float2(texCoord.x, texCoord.y), 0);;
	
        float4 minCross = min(min(min(newFrame, newSampleUp), min(newSampleDown, newSampleRight)), newSampleLeft);
        float4 maxCross = max(max(max(newFrame, newSampleUp), max(newSampleDown, newSampleRight)), newSampleLeft);
	
        float4 minBox = min(min(newSampleUpRight, newSampleUpLeft), min(newSampleDownRight, newSampleDownLeft));
		minBox = min(minBox, minCross);
	
        float4 maxBox = max(max(newSampleUpRight, newSampleUpLeft), max(newSampleDownRight, newSampleDownLeft));
		maxBox = max(maxBox, maxCross);
	
        float4 minNew = (minBox + minCross) * 0.5;
        float4 maxNew = (maxBox + maxCross) * 0.5;
	
        float4 clampedHist = clamp(histSample, minNew, maxNew);
		
        newFrame = lerp(clampedHist, newFrame, BlendFactor);
    }
	
	
	#if _DEBUG_MOTION_VECTORS
	newFrame = half4(1.0, 1.0, 1.0, 0.0);
	
	if (isValidHistUV)
	{
		newFrame = half4((MotionVector).x, (MotionVector).y, 0, 0) * 10.0;
	}
	#endif
	
	Color = newFrame.rgb;
	Alpha = newFrame.a;
}
#endif