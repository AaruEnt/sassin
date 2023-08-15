float4 KWS_VolumeTexSceenSize;

half MaxDistance;
uint KWS_RayMarchSteps;
half4 KWS_LightAnisotropy;

float KWS_VolumeLightMaxDistance;
float KWS_VolumeDepthFade;
float3 KWS_WorldSpaceCameraPosCompillerFixed; //by some reason the _WorldSpaceCameraPos throw error in single pass stereo VR
//D3D11 Internal Compiler Error: Invalid Bytecode: source register relative index temp register component 3 in r7 uninitialized. Opcode #47

static const float ditherPattern[8][8] =
{

	{
		0.012f, 0.753f, 0.200f, 0.937f, 0.059f, 0.800f, 0.243f, 0.984f
	},
	{
		0.506f, 0.259f, 0.690f, 0.443f, 0.553f, 0.306f, 0.737f, 0.490f
	},
	{
		0.137f, 0.875f, 0.075f, 0.812f, 0.184f, 0.922f, 0.122f, 0.859f
	},
	{
		0.627f, 0.384f, 0.569f, 0.322f, 0.675f, 0.427f, 0.612f, 0.369f
	},
	{
		0.043f, 0.784f, 0.227f, 0.969f, 0.027f, 0.769f, 0.212f, 0.953f
	},
	{
		0.537f, 0.290f, 0.722f, 0.475f, 0.522f, 0.275f, 0.706f, 0.459f
	},
	{
		0.169f, 0.906f, 0.106f, 0.843f, 0.153f, 0.890f, 0.090f, 0.827f
	},
	{
		0.659f, 0.412f, 0.600f, 0.353f, 0.643f, 0.400f, 0.584f, 0.337f
	},
};

struct RaymarchData
{
	float2 uv;
	float stepSize;
	float3 step;
	float offset;
	float3 currentPos;
	float3 rayStart;
	float3 rayDir;
	float rayLength;
};

struct vertexInput
{
	uint vertexID : SV_VertexID;
	UNITY_VERTEX_INPUT_INSTANCE_ID
};


struct vertexOutput
{
	float4 vertex : SV_POSITION;
	float2 uv : TEXCOORD0;
	UNITY_VERTEX_OUTPUT_STEREO
};


vertexOutput vert(vertexInput v)
{
	vertexOutput o;
	UNITY_SETUP_INSTANCE_ID(v);
	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
	o.vertex = GetTriangleVertexPosition(v.vertexID);
	o.uv = GetTriangleUVScaled(v.vertexID);
	return o;
}

float3 FrustumRay(float2 uv, float4 frustumRays[4])
{
	float3 ray0 = lerp(frustumRays[0].xyz, frustumRays[1].xyz, uv.x);
	float3 ray1 = lerp(frustumRays[2].xyz, frustumRays[3].xyz, uv.x);
	return lerp(ray0, ray1, uv.y);
}


inline half MieScattering(float cosAngle)
{
	return KWS_LightAnisotropy.w * (KWS_LightAnisotropy.x / (KWS_LightAnisotropy.y - KWS_LightAnisotropy.z * cosAngle));
}



half GetCausticLod_Raymarched(float3 lightForward, float3 currentPos, float offsetLength, float lodDist, Texture2D tex, half lastLodCausticColor)
{
	float2 uv = ((currentPos.xz - GetAbsoluteWorldSpacePos().xz) - offsetLength * lightForward.xz) / lodDist + 0.5 - KW_CausticLodOffset.xz;
	half caustic = tex.SampleLevel(sampler_linear_repeat, uv, 4.0).r;
	uv = 1 - min(1, abs(uv * 2 - 1));
	float lerpLod = uv.x * uv.y;
	lerpLod = min(1, lerpLod * 3);
	return lerp(lastLodCausticColor, caustic, lerpLod);
}

half RaymarchCaustic(float3 rayStart, float3 currentPos, float3 lightForward)
{
	float angle = dot(float3(0, -0.999, 0), lightForward);
	float offsetLength = (rayStart.y - currentPos.y) / angle;

	half caustic = 0.1;
	#if defined(USE_LOD3)
		caustic = GetCausticLod_Raymarched(lightForward, currentPos, offsetLength, KW_CausticLodSettings.w, KW_CausticLod3, caustic);
	#endif
	#if defined(USE_LOD2) || defined(USE_LOD3)
		caustic = GetCausticLod_Raymarched(lightForward, currentPos, offsetLength, KW_CausticLodSettings.z, KW_CausticLod2, caustic);
	#endif
	#if defined(USE_LOD1) || defined(USE_LOD2) || defined(USE_LOD3)
		caustic = GetCausticLod_Raymarched(lightForward, currentPos, offsetLength, KW_CausticLodSettings.y, KW_CausticLod1, caustic);
	#endif
	caustic = GetCausticLod_Raymarched(lightForward, currentPos, offsetLength, KW_CausticLodSettings.x, KW_CausticLod0, caustic);

	float distToCamera = length(currentPos - GetAbsoluteWorldSpacePos());
	float distFade = saturate(distToCamera / KW_DecalScale * 2);
	caustic = lerp(caustic, 0.1, distFade);
	caustic = caustic * 10 - 1;
	return caustic;
}


RaymarchData InitRaymarchData(vertexOutput i, float depthTop, float depthBot, bool isUnderwater)
{
	RaymarchData data;

	float3 topPos = GetWorldSpacePositionFromDepth(i.uv, depthTop);
	float3 botPos = GetWorldSpacePositionFromDepth(i.uv, depthBot);

	UNITY_BRANCH
	if (isUnderwater)
	{
		float3 camPos = GetAbsoluteWorldSpacePos();
		data.rayStart = camPos;
		data.rayDir = normalize(botPos - camPos);
		data.rayLength = min(length(camPos - botPos), KWS_VolumeLightMaxDistance);
		data.rayLength = min(length(camPos - topPos), data.rayLength);
	}
	else
	{
		data.rayDir = normalize(botPos - topPos);
		data.rayLength = min(length(topPos - botPos), KWS_VolumeLightMaxDistance);
		data.rayStart = topPos;
	}
	float2 ditherScreenPos = i.vertex.xy % 8;
	
	data.stepSize = data.rayLength / KWS_RayMarchSteps;
	data.step = data.rayDir * data.stepSize;
	data.offset = ditherPattern[ditherScreenPos.y][ditherScreenPos.x];
	data.currentPos = data.rayStart + data.step * data.offset;
	data.uv = i.uv;

	return data;
}

bool EarlyDiscardUnderwaterPixels(half mask)
{
	return mask < 0.01;
}

bool EarlyDiscardDepthOcclusionPixels(float depthBot, float depthTop, half mask)
{
	return (depthBot > depthTop && !(mask > 0.01));
}