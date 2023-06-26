#ifndef OCCASOFTWARE_BUTO_FOG_INCLUDE
#define OCCASOFTWARE_BUTO_FOG_INCLUDE


#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

#include "Common.hlsl"

half3 rayDirection;
half3 mainLightDirection;
half3 mainLightColor;


///////////////////////////////////////////
//    SHADER VARIABLES                   //
///////////////////////////////////////////
int _SampleCount;
bool _AnimateSamplePosition;
float _MaxDistanceVolumetric;
float _MaxDistanceNonVolumetric;
float _Anisotropy;

// 0 = Early Exit, 1 = Shorten Rays
float _DepthInteractionMode;

// 0 = Constant, 1 = Prefer Nearby
float _RayLengthMode;

float _DepthSofteningDistance;

float _BaseHeight;
float _AttenuationBoundarySize;

float _DistantBaseHeight;
float _DistantFogStartDistance;
float _DistantAttenuationBoundarySize;

float _FogDensity;
float _LightIntensity;
float _ShadowIntensity;

Texture2D _ColorRamp;
float _ColorInfluence;


Texture3D _NoiseTexture;


float _NoiseTiling;
float3 _NoiseWindSpeed;
float _NoiseIntensityMin;
float _NoiseIntensityMax;
float _Octaves;
float _Gain;
float _Lacunarity;

float _DistantFogDensity;




#define MAX_LIGHT_COUNT 8
int _LightCountButo = 0;
half3 _LightPosButo[MAX_LIGHT_COUNT];
half _LightIntensityButo[MAX_LIGHT_COUNT];
half3 _LightColorButo[MAX_LIGHT_COUNT];
half3 _LightDirectionButo[MAX_LIGHT_COUNT];
half2 _LightAngleButo[MAX_LIGHT_COUNT];

#define MAX_VOLUME_COUNT 8
int _VolumeCountButo = 0;
int _VolumeShape[MAX_VOLUME_COUNT]; // 0 => Sphere, 1 => Box
half3 _VolumePosition[MAX_VOLUME_COUNT]; // xyz world pos
half3 _VolumeSize[MAX_VOLUME_COUNT]; // Sphere => x = radius, Box => xyz = size, 
half _VolumeIntensityButo[MAX_VOLUME_COUNT]; // Intensity scalar
int _VolumeBlendMode[MAX_VOLUME_COUNT]; // BlendMode per Volume
half _VolumeBlendDistance[MAX_VOLUME_COUNT]; // Blend Distance per Volume

int _MaximumSelfShadowingOctaves = 1;
half3 _WorldColor = half3(0, 0, 0);

// SimpleColor[0] -> Lit, SimpleColor[1] -> Shadowed, SimpleColor[2] -> Emit
half3 _SimpleColor[3];

half3 _DirectionalLightingForward;
half3 _DirectionalLightingBack;
half _DirectionalLightingRatio;

int _FrameId;

TEXTURE2D_X(_ButoDepth);
SAMPLER(sampler_ButoDepth);

// Returns % between start and stop
half InverseLerp(half start, half stop, half value)
{
	return (value - start) / (stop - start);
}

half Remap(half inStart, half inStop, half outStart, half outStop, half v)
{
	half t = InverseLerp(inStart, inStop, v); 
	return lerp(outStart, outStop, saturate(t));
}


half InverseLerp0N(half stop, half value)
{
	return value / stop;
}


half Remap0N(half inStop, half outStart, half outStop, half v)
{
	half t = v / inStop;
	return lerp(outStart, outStop, saturate(t));
}


half CalculateHorizonFalloff(half3 RayPosition, half3 LightDirection)
{
	half h = max(RayPosition.y, 0);
	half r = 6371000.0; // Earth Radius (m)
	half a = r + h;
	half b = r / a;
	half c = acos(b);
	LightDirection = normalize(LightDirection);
	half angle = LightDirection.y * 1.57079632679;
	half d = angle - c;
	
	return smoothstep(0.0, radians(5.0), d);
}

half GetLightFalloff(half3 RayPosition, half3 LightPosition, half2 LightAngle, half3 LightForward)
{
	half3 dirToRay = normalize(LightPosition - RayPosition);
	half dotDir = dot(dirToRay, LightForward);
	half spotAttenuation = saturate((dotDir * LightAngle.x) + LightAngle.y);
	spotAttenuation *= spotAttenuation;
	
	half d = distance(RayPosition, LightPosition);
	half distanceAttenuation = 1.0 / ((1.0 + (d * d)));
	return distanceAttenuation * spotAttenuation;
}

half3 GetDirectionalLightOverrides(half3 ForwardColor, half3 BackColor, half Ratio)
{
	half3 color = half3(0, 0, 0);
	
	
	half cosAngle = dot(normalize(mainLightDirection), normalize(rayDirection)); // [-1, 1]
	cosAngle = saturate((cosAngle + 1.0) * 0.5); // [0, 1]
	cosAngle = pow(cosAngle, Ratio);
	color = lerp(BackColor, ForwardColor, cosAngle);
	
	
	half lightApexFade = abs(dot(normalize(mainLightDirection), float3(0, 1, 0))); // [0,1]
	color = lerp(color, half3(1.0, 1.0, 1.0), lightApexFade);
	
	
	return color;
}

half3 GetAdditionalLightData(half3 RayPosition)
{
	half3 additionalLightsColor = half3(0,0,0);
	
	uint lightCount = _LightCountButo;
	
	for (int lightIndex = 0; lightIndex < _LightCountButo; lightIndex++)
	{
		half3 lightColor = 
			_LightColorButo[lightIndex]
			* GetLightFalloff(RayPosition, _LightPosButo[lightIndex], _LightAngleButo[lightIndex], _LightDirectionButo[lightIndex])
			* _LightIntensityButo[lightIndex];
			
		additionalLightsColor += lightColor;
	}
	
	return additionalLightsColor;
}

// Unity renamed sampler_MainLightShadowmapTexture to sampler_LinearClampCompare in 2022.2.11. 
// So, we just declare our own sampler.
SAMPLER_CMP(sampler_ButoLinearClampCompare); 

// See https://github.com/Unity-Technologies/Graphics/blob/d84f56b68aea37785813fddfcbcf63384e9640ff/Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl
// WebGL complains about a gradient instruction when sampling the Shadowmap using SampleCmpLevelZero, 
// but this method always samples mip0. Why the complaint?
half GetRealtimeShadows(float4 shadowCoord)
{
	// We just manually sample the shadowmap using unfiltered data.
	
    half4 shadowParams = GetMainLightShadowParams();
    float shadowStrength = shadowParams.x;
    float attenuation = _MainLightShadowmapTexture.SampleCmpLevelZero(sampler_ButoLinearClampCompare, shadowCoord.xy, shadowCoord.z);
    return BEYOND_SHADOW_FAR(shadowCoord) ? 1.0 : attenuation;
}

half GetShadowAttenuation(half3 RayPos)
{
	half4 shadowCoord = TransformWorldToShadowCoord(RayPos);
	return saturate(lerp(GetRealtimeShadows(shadowCoord), 1.0, GetMainLightShadowFade(RayPos)));
}

float rand2dTo1d(float2 vec, float2 dotDir = float2(12.9898, 78.233))
{
	float random = dot(sin(vec.xy), dotDir);
	random = frac(sin(random) * 143758.5453);
	return random;
}

float2 rand2dTo2d(float2 vec, float2 seed = 4605)
{
	return float2(
		rand2dTo1d(vec + seed, float2(12.989, 78.233)),
		rand2dTo1d(vec + seed, float2(39.346, 11.135))
	);
}

float IGN(int pixelX, int pixelY, int frame)
{
    float f = frame % 64; // need to periodically reset frame to avoid numerical issues
    float x = float(pixelX) + 5.588238f * float(f);
    float y = float(pixelY) + 5.588238f * float(f);
    return fmod(52.9829189f * fmod(0.06711056f*float(x) + 0.00583715f*float(y), 1.0f), 1.0f);
}

half GetRandomHalf(half2 Seed)
{
	return saturate(frac(sin(dot(Seed + half2(642.86, 808.10), half2(12.9898, 78.233))) * 43758.5453));
}

half HenyeyGreenstein(half eccentricity)
{
	half cosAngle = dot(normalize(mainLightDirection), normalize(rayDirection));
	half e2 = eccentricity * eccentricity;
	return ((1.0 - e2) / pow(abs(1.0 + e2 - 2.0 * eccentricity * cosAngle), 1.5)) / 4.0 * 3.1416;
}

half GetFogDensityByHeight(half AttenuationBoundarySize, half BaseHeight, half3 RayPosition)
{
	half height = max(RayPosition.y - BaseHeight, 0.0);
	return exp(-height * 1.0 / max(AttenuationBoundarySize, 0.001));
}

half GetFogDensityByNoise(Texture3D NoiseTexture, SamplerState Sampler, half3 RayPosition, half InvNoiseScale, half3 WindVelocity, half NoiseMin, half NoiseMax, int Octaves, int mip)
{
	half3 uvw = RayPosition * InvNoiseScale;
	half3 wind = WindVelocity * InvNoiseScale * _Time.y;
	
	half4 value = 0;
	half c = 0;
	half amp = 1.0;
	
	for (int i = 1; i <= Octaves; i++)
	{
		value += amp * SAMPLE_TEXTURE3D_LOD(NoiseTexture, Sampler, uvw + wind, mip);
		c += amp;
		uvw *= _Lacunarity;
		amp *= _Gain;
	}
	value /= c;
	
	half v = value.r * 0.53 + value.g * 0.27 + value.b * 0.13 + value.a * 0.07;
	v = Remap(NoiseMin, NoiseMax, 0.0, 1.0, v);

	return v;
}

struct Box
{
	float3 position;
	float3 size;
};

struct Sphere
{
	float3 position;
	float radius;
};


half SdBox(float3 rayPosition, Box box)
{
	half3 v = abs(rayPosition - box.position) - box.size * 0.5;
	return length(max(v, 0)) + min(max(v.x, max(v.y,v.z)), 0);
}

half SdSphere(float3 rayPosition, Sphere sphere)
{
	return max(length(rayPosition - sphere.position) - sphere.radius, 0);
}


half GetFogFalloff(float3 RayPosition)
{
	// BlendMode 0 -> Multiplicative
	// BlendMode 1 -> Exclusive
	
	// Shape 0 -> Sphere
	// Shape 1 -> Box
	
	half a = 1.0;
	half x = 0.0;
	int xo = 0;
	
	
	for (int i = 0; i < _VolumeCountButo; i++)
	{
		float d = 0;
		
		if(_VolumeShape[i] == 0)
		{
			Sphere sphere;
			sphere.position = _VolumePosition[i].xyz;
			sphere.radius = _VolumeSize[i].x;
			d = SdSphere(RayPosition, sphere);
		}
		
		if(_VolumeShape[i] == 1)
		{
			Box box;
			box.position = _VolumePosition[i].xyz;
			box.size = _VolumeSize[i].xyz;
			d = SdBox(RayPosition, box);
		}
		
		
		half2 remapVals = half2(_VolumeIntensityButo[i], 1);
		
		if (_VolumeBlendMode[i] == 1)
		{
			xo = 1;
			remapVals = half2(1, 0);
		}
		
		//half v = Remap(0, _VolumeBlendDistance[i], remapVals.x, remapVals.y, d);
		half v = Remap0N(_VolumeBlendDistance[i], remapVals.x, remapVals.y, d);
		
		if (_VolumeBlendMode[i] == 0)
		{
			a *= v;
		}
		else if(_VolumeBlendMode[i] == 1)
		{
			x += v;
		}
	}
	
	if(xo == 1)
		a *= saturate(x);
	
	return a;
}

half CalculateExponentialHeightFog(half extinction, half fogExp, half rayOriginY, half rayLength)
{
	return saturate((extinction / fogExp) * exp(-rayOriginY * fogExp) * (1.0 - exp(-rayLength * rayDirection.y * fogExp)) / rayDirection.y);
}


half2 GetTexCoordSize(float scale)
{
	return rcp(_ScreenParams.xy * scale);
}

float3 GetCameraForward()
{
	return unity_CameraToWorld._m02_m12_m22;
}

float3 GetCameraUp()
{
	// Get the normal direction of the bottom clip plane (which points "camera up" in orthographic view).
	return unity_CameraToWorld._m01_m11_m21;
}

float3 GetCameraRight()
{
	// Get the normal direction of the bottom clip plane (which points "camera up" in orthographic view).
	return unity_CameraToWorld._m00_m10_m20;
}

float3 GetCameraPosition()
{
	return _WorldSpaceCameraPos.xyz;
}

float3 GetOrthographicOrigin(float2 UV)
{
	// Remaps xy to range of [orthoWidth, orthoWidth] on x and [orthoHeight, orthoHeight] on y.
	float2 orthoSize = unity_OrthoParams.xy;
	float2 orthographicUV = (UV - 0.5) * orthoSize * 2.0;
	
	// We could adjust this to account for the near clip plane,
	// However, we would also need to adjust the EyeDepth to account for this offset.
	// In addition, we should likely also adjust the Perspective projection version as well for the same.
	// I don't think this would create any real difference for most use cases, so we can save on performance by skipping it.
	return GetCameraPosition() + GetCameraRight() * orthographicUV.x + GetCameraUp() * orthographicUV.y;
}


float GetLuminance(float3 color)
{
	const float3 _RGB_WEIGHTS = float3(0.2126, 0.7152, 0.0722);
	return dot(color, _RGB_WEIGHTS);
}

half4 SampleCookieTexture(float2 uv)
{
    return SAMPLE_TEXTURE2D_LOD(_MainLightCookieTexture, sampler_MainLightCookieTexture, uv, 0);
}

half4 SampleCookie(float3 samplePositionWS)
{
    if(!IsMainLightCookieEnabled())
        return half4(1,1,1,1);

    float2 uv = ComputeLightCookieUVDirectional(_MainLightWorldToLight, samplePositionWS, float4(1, 1, 0, 0), URP_TEXTURE_WRAP_MODE_NONE);
    half4 color = SampleCookieTexture(uv);

	
	// If RGB Format, treat the cookie as COLORED, ALPHA CHANNEL AFFECTING SHADOW
	// If Alpha or Red format, treat the cookie as COLORLESS, INTENSITY AFFECTS SHADOW
	if(IsMainLightCookieTextureRGBFormat())
	{
		return color;
	}
	else if(IsMainLightCookieTextureAlphaFormat())
	{
		return half4(1,1,1, color.a);
	}
	else
	{
		return half4(1,1,1, color.r);
	}
}


void SampleVolumetricFog(float2 UV, out float3 Color, out float Alpha)
{
	// Out Value Setup
	Color = float3(0.0, 0.0, 0.0);
	Alpha = 1.0;
	
	
	// Initializing and Checking Params
	_AttenuationBoundarySize = max(_AttenuationBoundarySize, 0.001);
	_FogDensity = max(_FogDensity, 0.001);
	
	// Light Setup
	Light mainLight = GetMainLight();
	mainLightColor = mainLight.color;
	mainLightDirection = normalize(mainLight.direction);
	
	half2 jitteredUV = UV;
	// Ray Setup
	if (_AnimateSamplePosition)
	{
		half2 texCoord = GetTexCoordSize(0.5);
		half2 jitter = texCoord * (rand2dTo2d(UV + _Time.x) - 0.5);
		jitteredUV += jitter;
	}
	
	
	float depthRaw = SAMPLE_TEXTURE2D_X(_ButoDepth, sampler_ButoDepth, UV).r;
	float depth01 = GetDepth01(depthRaw);
	float depthEye = GetDepthEye(depthRaw);
	float realDepth = depthEye;
	
	float3 RayOrigin = GetCameraPosition();
	
	if(IsOrthographicPerspective())
	{
		rayDirection = GetCameraForward();
		RayOrigin = GetOrthographicOrigin(jitteredUV);
	}
	else
	{
		float3 viewVector = mul(unity_CameraInvProjection, half4(jitteredUV * 2 - 1, 0.0, -1)).xyz;
		viewVector = mul(unity_CameraToWorld, half4(viewVector, 0.0)).xyz;
		float viewLength = length(viewVector);
		realDepth = depthEye * viewLength;
		rayDirection = viewVector / viewLength;
	}
	
	half targetRayDistance = _MaxDistanceVolumetric;
	
	if(_DepthInteractionMode == 1.0)
		targetRayDistance = min(targetRayDistance, realDepth);
	
	// Lighting
	half extinction = _FogDensity * _FogDensity * 0.001;
	half hg = HenyeyGreenstein(_Anisotropy);
	
	static half points[3] =
	{
		0.165,
		0.495,
		0.825
	};
	
	
	// Ray March
	half2 seed = UV;
	if(_AnimateSamplePosition)
		seed += _Time.xx;
	
	half random = rand2dTo1d(seed);
	
	half invStepCount = rcp(half(_SampleCount));
	half invNoiseScale = rcp(_NoiseTiling);
	half invMaxRayDistance = rcp(targetRayDistance);
	
	half3 rayPosition = RayOrigin;
	float rayDepth_previous = 0;
	float rayDepth_current = 0;
	half3 directionalColor = GetDirectionalLightOverrides(_DirectionalLightingForward, _DirectionalLightingBack, _DirectionalLightingRatio);
	
	#define SELF_SHADOWING_STEP_COUNT 4
	float selfShadowSteps[SELF_SHADOWING_STEP_COUNT] = 
	{
		0.125 * _AttenuationBoundarySize,
		0.25 * _AttenuationBoundarySize,
		0.5 * _AttenuationBoundarySize,
		1.0 * _AttenuationBoundarySize
	};
	
	half stepSize = targetRayDistance * invStepCount;
	
	if(_RayLengthMode == 0.0)
    {
        rayDepth_current += random * stepSize;
    }
	
	float softenDepth = 1.0;
    float invDepthSofteningDistance = rcp(_DepthSofteningDistance);
    bool lastRun = false;
	
    float ratioSquishedCurr = 0.0;
    float ratioSquishedPrev = 0.0;
	
	for (int i = 1; i <= _SampleCount; i++)
	{
		// Positioning
		half ratio = half(i) * invStepCount;
		
        if (_RayLengthMode == 1.0)
        {
            ratioSquishedCurr = pow(ratio, 1.5);
            float rayDepth_current_max = targetRayDistance * ratioSquishedCurr;
            float rayDepth_current_min = targetRayDistance * ratioSquishedPrev;
            rayDepth_current = lerp(rayDepth_current_min, rayDepth_current_max, saturate(random));
            ratioSquishedPrev = ratioSquishedCurr;
			
        }
		
		if(rayDepth_current > targetRayDistance)
		{
			rayDepth_current = targetRayDistance;
			lastRun = true;
		}
		
		if(_DepthSofteningDistance > 1e-3)
		{
			softenDepth = clamp(realDepth - rayDepth_current, 0.0, _DepthSofteningDistance);
            softenDepth *= invDepthSofteningDistance;
        }
		
		
        rayPosition = RayOrigin + rayDirection * rayDepth_current;
		
		half stepLength = rayDepth_current - rayDepth_previous;
		rayDepth_previous = rayDepth_current;
		
		// Transmittance
		half sampleDensity = GetFogDensityByHeight(_AttenuationBoundarySize, _BaseHeight, rayPosition) * GetFogDensityByNoise(_NoiseTexture, linear_repeat_sampler, rayPosition, invNoiseScale, _NoiseWindSpeed, _NoiseIntensityMin, _NoiseIntensityMax, _Octaves, 0) * GetFogFalloff(rayPosition);
		
		half sampleExtinction = extinction * sampleDensity * softenDepth;
		
		if (sampleExtinction > 1e-7)
		{
			// Evaluate Color
			half3 colorSamples[3];
			half3 shadedColor = _WorldColor;
			half4 cookie = SampleCookie(rayPosition);
			
			half3 litColor = _WorldColor + mainLightColor * cookie.rgb;
			half3 emitColor = 0;
			
			if (_ColorInfluence > 0.001)
			{
				for (int i = 0; i <= 2; i++)
				{
					colorSamples[i] = SAMPLE_TEXTURE2D_LOD(_ColorRamp, point_clamp_sampler, half2(saturate(rayDepth_current * invMaxRayDistance), points[i]), 0).rgb;
				}
			
				shadedColor = lerp(shadedColor, colorSamples[0] * _SimpleColor[1], _ColorInfluence);
				litColor = lerp(litColor, colorSamples[1] * _SimpleColor[0], _ColorInfluence);
				emitColor = colorSamples[2] * _SimpleColor[2] * _ColorInfluence;
			}
			
			
			half shadowAttenuation = GetShadowAttenuation(rayPosition) * cookie.a;
			half finalShading = shadowAttenuation;
			
			
			#if _BUTO_SELF_ATTENUATION_ENABLED
				half shadowSelfAttenuation = 1.0;
				
				for (int f = 1; f <= SELF_SHADOWING_STEP_COUNT; f++)
				{
					half selfAttenStepLength = selfShadowSteps[f - 1];
					half3 currRayPos = rayPosition + mainLightDirection * selfAttenStepLength;
					half density = GetFogDensityByHeight(_AttenuationBoundarySize, _BaseHeight, currRayPos) * GetFogDensityByNoise(_NoiseTexture, linear_repeat_sampler, currRayPos, invNoiseScale, _NoiseWindSpeed, _NoiseIntensityMin, _NoiseIntensityMax, _MaximumSelfShadowingOctaves, 1) * GetFogFalloff(currRayPos);
					shadowSelfAttenuation *= exp(-density * extinction * selfAttenStepLength);
				}
				finalShading *= shadowSelfAttenuation;
			#endif
			
			#if _BUTO_HORIZON_SHADOWING_ENABLED
				finalShading *= CalculateHorizonFalloff(rayPosition, mainLightDirection);
			#endif
			
			
			half3 fogColor = lerp(shadedColor, litColor * hg, finalShading) * directionalColor + emitColor;
			
			// Solve Transmittance
			half shadowIntensity = lerp(_ShadowIntensity, _LightIntensity, finalShading);
			half transmittance = exp(-sampleExtinction * stepLength * shadowIntensity);
			half3 additionalLightColor = GetAdditionalLightData(rayPosition);
			
			// Solve Color
			half3 inScatteringData = sampleExtinction * (fogColor + additionalLightColor);
			half3 integratedScattering = (inScatteringData - (inScatteringData * transmittance)) / sampleExtinction;	
			Color += Alpha * integratedScattering;
			
			// Apply Transmittance
			Alpha *= transmittance;
			
		}
		
		if(lastRun)
			break;
			
        if (_RayLengthMode == 0.0)
        {
            rayDepth_current += stepSize;
        }
		
	}
	
	// TO DO:
	// Move to standalone analytic fog renderer.
	
	#if _BUTO_ANALYTIC_FOG_ENABLED
		half sampleDist = _MaxDistanceNonVolumetric;
		if (depth01 < 1.0)
		{
			sampleDist = min(sampleDist, realDepth);
		}
		
		half rayLength = max(sampleDist - _DistantFogStartDistance, 0);
		half rayHeight = max((RayOrigin + rayDirection * _DistantFogStartDistance).y - _DistantBaseHeight, 0.0);
		
		half e = _DistantFogDensity * _DistantFogDensity * 0.001;
		half fogAmount = 1.0 - CalculateExponentialHeightFog(e, 1.0 / max(_DistantAttenuationBoundarySize, 0.001), rayHeight, rayLength);
		
		// Apply Transmittance
		half alpha_previous = Alpha;
		Alpha *= fogAmount;
	
		// Evaluate Color
		half3 colorSamples[3];
		for (int f = 0; f <= 2; f++)
		{
			colorSamples[f] = SAMPLE_TEXTURE2D_LOD(_ColorRamp, point_clamp_sampler, half2(1.0, points[f]), 0).rgb;
		}
		
		half shadowAttenuation = 1.0;
	
		#if _BUTO_HORIZON_SHADOWING_ENABLED
			shadowAttenuation *= CalculateHorizonFalloff(RayOrigin, mainLightDirection);
		#endif
		
		half3 shadedColor = lerp(_WorldColor, colorSamples[0] * _SimpleColor[1], _ColorInfluence);
		half3 litColor = lerp(_WorldColor + mainLightColor, colorSamples[1] * _SimpleColor[0], _ColorInfluence) * _LightIntensity;
		half3 emitColor = colorSamples[2] * _SimpleColor[2] * _ColorInfluence;
		
		half3 fogColor = lerp(shadedColor, litColor * hg, shadowAttenuation) * directionalColor + emitColor;
		
		Color += fogColor * (alpha_previous - Alpha);
	#endif
}


#endif