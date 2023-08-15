#ifndef KWS_WATER_VARIABLES
	#include "..\Common\KWS_WaterVariables.cginc"
#endif

inline void UpdateInstanceMatrixes(InstancedMeshDataStruct meshData)
{
	float3 position = meshData.position.xyz;
	float3 size = meshData.size.xyz;

	if (KWS_IsFiniteTypeInstancing == 1) size.xyz /= KWS_InstancingWaterScale;

	OverrideUnityInstanceMatrixes(position, size);
}

inline float GetFlag(uint value, uint bit)
{
	return (value >> bit) & 0x01;
}

inline void UpdateInstanceSeamsAndSkirt(InstancedMeshDataStruct meshData, float2 uvData, inout float4 vertex)
{
	float quadOffset = uvData.y;
	uint mask = (uint)uvData.x;

	vertex.x -= quadOffset * GetFlag(mask, 1) * meshData.downSeam;
	vertex.z -= quadOffset * GetFlag(mask, 2) * meshData.leftSeam;
	vertex.x += quadOffset * GetFlag(mask, 3) * meshData.topSeam;
	vertex.z += quadOffset * GetFlag(mask, 4) * meshData.rightSeam;

	float down = GetFlag(mask, 5) * meshData.downInf;
	float left = GetFlag(mask, 6) * meshData.leftInf;
	float top = GetFlag(mask, 7) * meshData.topInf;
	float right = GetFlag(mask, 8) * meshData.rightInf;

	if (KWS_IsFiniteTypeInstancing == 1)
	{
		vertex.y -= down;
		vertex.y -= left;
		vertex.y -= top;
		vertex.y -= right;
	}
	else
	{
		vertex.zy += 1000 * down * lerp(float2(-1, 0), float2(0, -1), KWS_UnderwaterVisible);
		vertex.xy += 1000 * left * lerp(float2(-1, 0), float2(0, -1), KWS_UnderwaterVisible);
		vertex.zy += 1000 * top * lerp(float2(1, 0), float2(0, -1), KWS_UnderwaterVisible);
		vertex.xy += 1000 * right * lerp(float2(1, 0), float2(0, -1), KWS_UnderwaterVisible);
	}
}

inline void UpdateInstaceRotation(inout float4 vertex)
{
	if (KWS_IsFiniteTypeInstancing == 1) vertex.xyz = mul((float3x3)KWS_InstancingRotationMatrix, vertex.xyz);
}

inline void UpdateInstanceData(uint instanceID, float2 uvData, inout float4 vertex)
{
	InstancedMeshDataStruct meshData = InstancedMeshData[instanceID];
	UpdateInstanceMatrixes(meshData);
	UpdateInstanceSeamsAndSkirt(meshData, uvData, vertex);
	UpdateInstaceRotation(vertex);
}
