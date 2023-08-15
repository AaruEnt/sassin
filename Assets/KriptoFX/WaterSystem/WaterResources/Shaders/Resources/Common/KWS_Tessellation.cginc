#ifndef USE_WATER_TESSELATION
#define USE_WATER_TESSELATION
#endif

struct TesselationWaterInput
{
	float4 vertex : POSITION;
	float3 surfaceMask : COLOR0;
	float3 normal : NORMAL;
	#ifdef USE_WATER_INSTANCING
		float2 uvData : TEXCOORD0;
	#endif
	uint instanceID : SV_InstanceID;
};

struct TessellationFactors
{
	float edge[3] : SV_TessFactor;
	float inside : SV_InsideTessFactor;
};

struct TessellationControlPoint
{
	float4 vertex : INTERNALTESSPOS;
	float3 surfaceMask : COLOR0;
	float3 normal : NORMAL;
	uint instanceID : TEXCOORD0;
};

TessellationControlPoint vertHull(waterInput v)
{
	TessellationControlPoint o;
	#ifdef USE_WATER_INSTANCING
		InstancedMeshDataStruct meshData = InstancedMeshData[v.instanceID];
		UpdateInstanceMatrixes(meshData);
		UpdateInstanceSeamsAndSkirt(meshData, v.uvData, v.vertex);
	#endif

	o.vertex = v.vertex;
	o.surfaceMask = v.surfaceMask;
	o.normal = v.normal;
	o.instanceID = v.instanceID;
	return o;
}


////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
float CalcDistanceTessFactor(float4 vertex, float minDist, float maxDist, float tess)
{
	float3 wpos = LocalToWorldPos(vertex.xyz).xyz;
	float dist = distance(wpos, _WorldSpaceCameraPos);
	float f = clamp(1.0 - (dist - minDist) / (maxDist - minDist), 0.01, 1.0) * tess;
	return f;
}

float4 CalcTriEdgeTessFactors(float3 triVertexFactors)
{
	float4 tess;
	tess.x = 0.5 * (triVertexFactors.y + triVertexFactors.z);
	tess.y = 0.5 * (triVertexFactors.x + triVertexFactors.z);
	tess.z = 0.5 * (triVertexFactors.x + triVertexFactors.y);
	tess.w = (triVertexFactors.x + triVertexFactors.y + triVertexFactors.z) / 3.0f;
	return tess;
}


float DistanceToPlane(float3 pos, float4 plane)
{
	float d = dot(float4(pos, 1.0f), plane);
	return d;
}

bool IsTriangleVisible(float3 wpos0, float3 wpos1, float3 wpos2, float cullEps)
{
	float4 planeTest;
	//unity provide _FrustumCameraPlanes only for left eye, as a temporary solution, I am adding an additional offset
	float vrRightEyeOffset = 0;
	#if defined(UNITY_STEREO_INSTANCING_ENABLED) || defined(UNITY_STEREO_MULTIVIEW_ENABLED)
		vrRightEyeOffset += 30;
	#endif

	// left
	planeTest.x = ((DistanceToPlane(wpos0, _FrustumCameraPlanes[0]) > - cullEps) ?     1.0f : 0.0f) +
	((DistanceToPlane(wpos1, _FrustumCameraPlanes[0]) > - cullEps) ?     1.0f : 0.0f) +
	((DistanceToPlane(wpos2, _FrustumCameraPlanes[0]) > - cullEps) ?     1.0f : 0.0f);
	// right
	planeTest.y = ((DistanceToPlane(wpos0, _FrustumCameraPlanes[1]) > - cullEps - vrRightEyeOffset) ?     1.0f : 0.0f) +
	((DistanceToPlane(wpos1, _FrustumCameraPlanes[1]) > - cullEps - vrRightEyeOffset) ?     1.0f : 0.0f) +
	((DistanceToPlane(wpos2, _FrustumCameraPlanes[1]) > - cullEps - vrRightEyeOffset) ?     1.0f : 0.0f);
	// top
	planeTest.z = ((DistanceToPlane(wpos0, _FrustumCameraPlanes[2]) > - cullEps) ?     1.0f : 0.0f) +
	((DistanceToPlane(wpos1, _FrustumCameraPlanes[2]) > - cullEps) ?     1.0f : 0.0f) +
	((DistanceToPlane(wpos2, _FrustumCameraPlanes[2]) > - cullEps) ?     1.0f : 0.0f);
	// bottom
	planeTest.w = ((DistanceToPlane(wpos0, _FrustumCameraPlanes[3]) > - cullEps) ?     1.0f : 0.0f) +
	((DistanceToPlane(wpos1, _FrustumCameraPlanes[3]) > - cullEps) ?     1.0f : 0.0f) +
	((DistanceToPlane(wpos2, _FrustumCameraPlanes[3]) > - cullEps) ?     1.0f : 0.0f);

	// has to pass all 4 plane tests to be visible
	return !all(planeTest);
}


float4 DistanceBasedTessCull(float4 v0, float4 v1, float4 v2, float minDist, float maxDist, float tessFactor, float maxDisplace)
{

	#ifdef USE_WATER_INSTANCING

		float3 f;
		f.x = CalcDistanceTessFactor(v0, minDist, maxDist, tessFactor);
		f.y = CalcDistanceTessFactor(v1, minDist, maxDist, tessFactor);
		f.z = CalcDistanceTessFactor(v2, minDist, maxDist, tessFactor);
		return CalcTriEdgeTessFactors(f);

	#else

		float3 pos0 = mul(UNITY_MATRIX_M, v0).xyz;
		float3 pos1 = mul(UNITY_MATRIX_M, v1).xyz;
		float3 pos2 = mul(UNITY_MATRIX_M, v2).xyz;
		float4 tess;

	
		if (IsTriangleVisible(pos0, pos1, pos2, maxDisplace))
		{
			tess = 0.0f;
		}
		else
		{
			float3 f;
			f.x = CalcDistanceTessFactor(v0, minDist, maxDist, tessFactor);
			f.y = CalcDistanceTessFactor(v1, minDist, maxDist, tessFactor);
			f.z = CalcDistanceTessFactor(v2, minDist, maxDist, tessFactor);
			tess = CalcTriEdgeTessFactors(f);
		}
		return tess;

	#endif
	
}

TessellationFactors HSConstant(InputPatch <TessellationControlPoint, 3> patch)
{
	TessellationFactors f;
	
	#ifdef USE_WATER_INSTANCING
		UpdateInstanceMatrixes(InstancedMeshData[patch[0].instanceID]);
	#endif

	half4 factor = DistanceBasedTessCull(patch[0].vertex, patch[1].vertex, patch[2].vertex, 1, _TesselationMaxDistance, _TesselationFactor, _TesselationMaxDisplace);

	f.edge[0] = factor.x;
	f.edge[1] = factor.y;
	f.edge[2] = factor.z;
	f.inside = factor.w;
	return f;
}

[domain("tri")]
[partitioning("fractional_odd")]
[outputtopology("triangle_cw")]
[patchconstantfunc("HSConstant")]
[outputcontrolpoints(3)]
TessellationControlPoint HS(InputPatch < TessellationControlPoint, 3 > Input, uint id : SV_OutputControlPointID)
{
	return Input[id];
}

[domain("tri")]
v2fWater DS(TessellationFactors HSConstantData, const OutputPatch < TessellationControlPoint, 3 > Input, float3 BarycentricCoords : SV_DomainLocation)
{
	float fU = BarycentricCoords.x;
	float fV = BarycentricCoords.y;
	float fW = BarycentricCoords.z;

	float3 vertex = Input[0].vertex.xyz * fU + Input[1].vertex.xyz * fV + Input[2].vertex.xyz * fW;
	
	waterInput input = (waterInput)0;

	#if SHADER_API_METAL
		input.vertex = float4(vertex, 1);
		input.normal = float3(0, 1, 0);
		input.surfaceMask = float3(1, 1, 1);
		//input.instanceID = Input[0].instanceID;
		return vert(input);
	#else
		
		input.surfaceMask = Input[0].surfaceMask * fU + Input[1].surfaceMask * fV + Input[2].surfaceMask * fW;
		input.normal = Input[0].normal.xyz * fU + Input[1].normal.xyz * fV + Input[2].normal.xyz * fW;
		input.vertex = float4(vertex, 1);
		input.instanceID = Input[0].instanceID;
		return vert(input);
	#endif
}

[domain("tri")]
v2fDepth DS_Depth(TessellationFactors HSConstantData, const OutputPatch < TessellationControlPoint, 3 > Input, float3 BarycentricCoords : SV_DomainLocation)
{
	float fU = BarycentricCoords.x;
	float fV = BarycentricCoords.y;
	float fW = BarycentricCoords.z;

	float3 vertex = Input[0].vertex.xyz * fU + Input[1].vertex.xyz * fV + Input[2].vertex.xyz * fW;

	waterInput input = (waterInput)0;

	#if SHADER_API_METAL
		input.vertex = float4(vertex, 1);
		input.normal = float3(0, 1, 0);
		input.surfaceMask = float3(1, 1, 1);
		//input.instanceID = Input[0].instanceID;
		return vertDepth(input);
	#else
		input.surfaceMask = Input[0].surfaceMask * fU + Input[1].surfaceMask * fV + Input[2].surfaceMask * fW;
		input.normal = Input[0].normal.xyz * fU + Input[1].normal.xyz * fV + Input[2].normal.xyz * fW;
		input.vertex = float4(vertex, 1);
		input.instanceID = Input[0].instanceID;
		return vertDepth(input);
	#endif
}