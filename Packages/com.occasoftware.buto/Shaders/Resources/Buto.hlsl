#ifndef OCCASOFTWARE_BUTO_INPUT_INCLUDED
#define OCCASOFTWARE_BUTO_INPUT_INCLUDED

Texture2D _ButoTexture;
SamplerState point_clamp_sampler;

float4 ButoFog(float2 ScreenPosition)
{
	return _ButoTexture.Sample(point_clamp_sampler, ScreenPosition);
}

float3 ButoFogBlend(float2 ScreenPosition, float3 InputColor)
{
	float4 fog = ButoFog(ScreenPosition);
	return (InputColor.rgb * fog.a) + fog.rgb;
}

void ButoFog_float(float2 ScreenPosition, out float3 Color, out float Density)
{
	Color = 0;
	Density = 1;
	#ifndef SHADERGRAPH_PREVIEW
	float4 fog = ButoFog(ScreenPosition);
	
	// Note: To blend with target, multiply Density by pre-fog frag color, then add fog color.
	// Alternatively, use ButoFogBlend, which handles this blending for you.
	Color = fog.rgb;
	Density = fog.a;
	#endif
}

void ButoFogBlend_float(float2 ScreenPosition, float3 InputColor, out float3 Color)
{
	Color = InputColor;
	#ifndef SHADERGRAPH_PREVIEW
	Color = ButoFogBlend(ScreenPosition, InputColor);
	#endif
}
#endif