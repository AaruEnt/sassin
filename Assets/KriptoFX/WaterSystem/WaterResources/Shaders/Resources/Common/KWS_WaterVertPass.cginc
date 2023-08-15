struct waterInput
{
	float4 vertex : POSITION;
	float3 surfaceMask : COLOR0;
	float3 normal : NORMAL;
	#if defined(USE_WATER_INSTANCING)
		float2 uvData : TEXCOORD0;
	#endif
	uint instanceID : SV_InstanceID;
};

struct v2fDepth
{
	float4 pos : SV_POSITION;
	float3 normal : NORMAL;
	float4 worldPos_LocalHeight : TEXCOORD0;
	float3 surfaceMask : COLOR0;
	float4 screenPos : TEXCOORD1;
	UNITY_VERTEX_OUTPUT_STEREO
};

struct v2fWater
{
	float4 pos : SV_POSITION;
	float3 normal : NORMAL;
	float3 surfaceMask : COLOR0;

	float3 worldPos : TEXCOORD0;
	float3 worldPosRefracted : TEXCOORD1;
	float4 screenPos : TEXCOORD2;

	UNITY_VERTEX_OUTPUT_STEREO
};


float3 ComputeWaterOffset(float3 worldPos)
{
	float2 uv = worldPos.xz / KW_FFTDomainSize;
	float3 offset = 0;
	#if defined(USE_FILTERING) || defined(USE_CAUSTIC_FILTERING)
		float3 disp = Texture2DSampleLevelBicubic(KW_DispTex, sampler_linear_repeat, uv, KW_DispTex_TexelSize, 0).xyz;
	#else
		float3 disp = KW_DispTex.SampleLevel(sampler_linear_repeat, uv, 0).xyz;
	#endif

	float distanceToCamera = GetWorldToCameraDistance(worldPos);
	
	float3 displacementScaleRelativeToDistance = lerp(float3(0.9, 1, 0.9), float3(0.25, 1, 0.25), saturate(distanceToCamera * 0.001));
	//displacementScaleRelativeToDistance = lerp(displacementScaleRelativeToDistance, float3(0.0, Test4.x, 0.0), KWS_UnderwaterVisible * saturate(distanceToCamera * 0.001));
	disp *= displacementScaleRelativeToDistance;
	
	#if defined(KW_FLOW_MAP) || defined(KW_FLOW_MAP_EDIT_MODE)
		float2 flowMapUV = (worldPos.xz - KW_FlowMapOffset.xz) / KW_FlowMapSize + 0.5;
		float2 flowmap = KW_FlowMapTex.SampleLevel(sampler_linear_clamp, flowMapUV, 0) * 2 - 1;
		disp = ComputeDisplaceUsingFlowMap(KW_DispTex, sampler_linear_repeat, flowmap, disp, uv, KW_Time * KW_FlowMapSpeed);
	#endif

	#if KW_DYNAMIC_WAVES
		float2 dynamicWavesUV = (worldPos.xz - KW_DynamicWavesWorldPos.xz) / KW_DynamicWavesAreaSize + 0.5;
		float dynamicWave = KW_DynamicWaves.SampleLevel(sampler_linear_clamp, dynamicWavesUV, 0).x;
		disp.y -= dynamicWave * 0.15;
	#endif

	#if defined(KW_FLOW_MAP_FLUIDS) && !defined(KW_FLOW_MAP_EDIT_MODE)
		float2 fluidsUV_lod0 = (worldPos.xz - KW_FluidsMapWorldPosition_lod0.xz) / KW_FluidsMapAreaSize_lod0 + 0.5;
		float2 fluids_lod0 = KW_Fluids_Lod0.SampleLevel(sampler_linear_clamp, fluidsUV_lod0, 0).xy;

		float2 fluidsUV_lod1 = (worldPos.xz - KW_FluidsMapWorldPosition_lod1.xz) / KW_FluidsMapAreaSize_lod1 + 0.5;
		float2 fluids_lod1 = KW_Fluids_Lod1.SampleLevel(sampler_linear_clamp, fluidsUV_lod1, 0).xy;

		float2 maskUV_lod0 = 1 - saturate(abs(fluidsUV_lod0 * 2 - 1));
		float lodLevelFluidMask_lod0 = saturate((maskUV_lod0.x * maskUV_lod0.y - 0.01) * 3);
		float2 maskUV_lod1 = 1 - saturate(abs(fluidsUV_lod0 * 2 - 1));
		float lodLevelFluidMask_lod1 = saturate((maskUV_lod1.x * maskUV_lod1.y - 0.01) * 3);

		float2 fluids = lerp(fluids_lod1, fluids_lod0, lodLevelFluidMask_lod0);
		fluids *= lodLevelFluidMask_lod1;
		disp = ComputeDisplaceUsingFlowMap(KW_DispTex, sampler_linear_repeat, fluids * KW_FluidsVelocityAreaScale * 0.75, disp, uv, KW_Time * KW_FlowMapSpeed).xyz;
	#endif

	if (KWS_UseMultipleSimulations > 0)
	{
		disp += KW_DispTex_LOD1.SampleLevel(sampler_linear_repeat, worldPos.xz / KW_FFTDomainSize_LOD1, 0).xyz * displacementScaleRelativeToDistance;
		disp += KW_DispTex_LOD2.SampleLevel(sampler_linear_repeat, worldPos.xz / KW_FFTDomainSize_LOD2, 0).xyz * displacementScaleRelativeToDistance;
	}
	

	#if USE_SHORELINE
		disp = ComputeShorelineOffset(worldPos, disp);
	#endif

	offset += disp;

	return offset;
}


float3 GetWaterOffsetRelativeToMask(float surfaceMaskY, float3 worldPos)
{
	if (surfaceMaskY > 0.001)
	{
		float3 waterOffset = ComputeWaterOffset(worldPos);
		if (surfaceMaskY < 0.51) waterOffset.xz *= 0;

		return WorldToLocalPosWithoutTranslation(waterOffset);
	}
	else return float3(0, 0, 0);
}

float3 GetWaterOffsetRelativeToMaskInstanced(float surfaceMaskY, float3 worldPos, float uvDataX, uint instanceID)
{
	InstancedMeshDataStruct meshData = InstancedMeshData[instanceID];
	uint mask = (uint)uvDataX;
	float down = GetFlag(mask, 5) * meshData.downInf;
	float left = GetFlag(mask, 6) * meshData.leftInf;
	float top = GetFlag(mask, 7) * meshData.topInf;
	float right = GetFlag(mask, 8) * meshData.rightInf;
	if (!down && !left && !top && !right) surfaceMaskY = 1;

	#ifndef USE_WATER_TESSELATION
		if (worldPos.y < KW_WaterPosition.y) surfaceMaskY = 0;
	#endif

	return GetWaterOffsetRelativeToMask(surfaceMaskY, worldPos);
}

v2fDepth vertDepth(waterInput v)
{
	v2fDepth o = (v2fDepth)0;
	
	UNITY_SETUP_INSTANCE_ID(v);
	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

	#if defined(USE_WATER_INSTANCING)
		#ifdef USE_WATER_TESSELATION
			UpdateInstanceMatrixes(v.instanceID);
		#else
			UpdateInstanceData(v.instanceID, v.uvData, v.vertex);
		#endif
	#endif

	o.normal = v.normal;
	o.worldPos_LocalHeight.xyz = LocalToWorldPos(v.vertex.xyz);
	o.surfaceMask = v.surfaceMask;
	
	#if defined(USE_WATER_INSTANCING)
		v.vertex.xyz += GetWaterOffsetRelativeToMaskInstanced(o.surfaceMask.y, o.worldPos_LocalHeight.xyz, v.uvData.x, v.instanceID);
	#else
		v.vertex.xyz += GetWaterOffsetRelativeToMask(o.surfaceMask.y, o.worldPos_LocalHeight.xyz);
	#endif
	
	o.worldPos_LocalHeight.w = v.vertex.y + 1.0;
	o.pos = ObjectToClipPos(v.vertex);
	o.screenPos = ComputeScreenPos(o.pos);

	return o;
}


v2fWater vert(waterInput v)
{
	v2fWater o = (v2fWater)0;

	UNITY_SETUP_INSTANCE_ID(v);
	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
	
	#if defined(USE_WATER_INSTANCING)
		#ifdef USE_WATER_TESSELATION
			UpdateInstanceMatrixes(v.instanceID);
		#else
			UpdateInstanceData(v.instanceID, v.uvData, v.vertex);
		#endif
	#endif
	o.normal = v.normal;
	o.worldPos = LocalToWorldPos(v.vertex.xyz);
	o.surfaceMask = lerp(v.surfaceMask, 1, KWS_IsCustomMesh);

	#if defined(USE_WATER_INSTANCING)
		v.vertex.xyz += GetWaterOffsetRelativeToMaskInstanced(o.surfaceMask.y, o.worldPos.xyz, v.uvData.x, v.instanceID);
	#else
		v.vertex.xyz += GetWaterOffsetRelativeToMask(o.surfaceMask.y, o.worldPos.xyz);
	#endif

	#ifndef USE_WATER_TESSELATION
		if (KWS_UseWireframeMode)
		{
			o.surfaceMask = ComputeWireframeInterpolators(v.surfaceMask.z);
		}
	#endif

	o.worldPosRefracted = LocalToWorldPos(v.vertex.xyz);
	o.pos = ObjectToClipPos(v.vertex);
	o.screenPos = ComputeScreenPos(o.pos);

	return o;
}