half4 frag(v2fWater i, float facing : VFACE) : SV_Target
{
	UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

	float2 uv = i.worldPos.xz / KW_FFTDomainSize;
	float2 screenUV = i.screenPos.xy / i.screenPos.w;

	float3 worldMeshNormal = GetWorldSpaceNormal(i.normal);
	float3 viewDir = GetWorldSpaceViewDirNorm(i.worldPosRefracted);
	
	//float surfaceDepthZ = GetSurfaceDepth(i.screenPos.z); //todo check why UNITY_Z_0_FAR_FROM_CLIPSPACE doesn't work with editor camera
	float surfaceDepthZ = LinearEyeDepth(i.screenPos.z / i.screenPos.w);
	half surfaceMask = KWS_UseWireframeMode == 1 || i.surfaceMask.x > 0.999;

	half exposure = GetExposure();
	
	float3 tangentNormal;

	//return float4(KW_WaterViewPort.xxx, 1);
	/////////////////////////////////////////////////////////////  NORMAL  ////////////////////////////////////////////////////////////////////////////////////////////////////////

	#if USE_FILTERING
		float normalFilteringMask;
		tangentNormal = GetFilteredNormal_lod0(uv, surfaceDepthZ, normalFilteringMask);
	#else
		tangentNormal = GetNormal_lod0(uv);
	#endif
	
	if (KWS_UseMultipleSimulations > 0)	tangentNormal = GetNormal_lod1_lod2(i.worldPos, tangentNormal);
	
	tangentNormal = normalize(tangentNormal);
	
	#if defined(KW_FLOW_MAP) || defined(KW_FLOW_MAP_EDIT_MODE)
		tangentNormal = GetFlowmapNormal(i.worldPos, uv, tangentNormal);
	#endif
	#if defined(KW_FLOW_MAP_FLUIDS) && !defined(KW_FLOW_MAP_EDIT_MODE)
		half fluidsFoam;
		tangentNormal = GetFluidsNormal(i.worldPos, uv, tangentNormal, fluidsFoam);
	#endif


	#ifdef KW_FLOW_MAP_EDIT_MODE
		return GetFlowmapEditor(i.worldPos, tangentNormal);
	#endif

	#if USE_SHORELINE
		tangentNormal = ComputeShorelineNormal(tangentNormal, i.worldPos);
		//return float4(tangentNormal.xz, 0, 1);
	#endif

	
	#if KW_DYNAMIC_WAVES
		tangentNormal = GetDynamicWaves(i.worldPos, tangentNormal);
	#endif

	#if USE_FILTERING
		tangentNormal.xz *= normalFilteringMask;
	#endif

	tangentNormal = lerp(float3(0, 1, 0), tangentNormal, surfaceMask);
	float3 worldNormal = KWS_BlendNormals(tangentNormal, worldMeshNormal);

	/////////////////////////////////////////////////////////////  end normal  ////////////////////////////////////////////////////////////////////////////////////////////////////////
	//return float4(worldNormal.xz, 0, 1);

	
	float sceneZ = GetSceneDepth(screenUV);
	half surfaceTensionFade = GetSurfaceTension(sceneZ, i.screenPos.w);
	

	/////////////////////////////////////////////////////////////////////  REFRACTION  ///////////////////////////////////////////////////////////////////
	float2 refractionUV;
	half3 refraction;

	UNITY_BRANCH
	if (KWS_UseRefractionIOR > 0) refractionUV = GetRefractedUV_IOR(viewDir, worldNormal, i.worldPos, surfaceTensionFade);
	else		refractionUV = lerp(screenUV, GetRefractedUV_Simple(screenUV, tangentNormal), surfaceMask);
	
	UNITY_BRANCH
	if(KWS_UseRefractionDispersion > 0)	refraction = GetSceneColorWithDispersion(refractionUV, KWS_RefractionDispersionStrength);
	else refraction = GetSceneColor(refractionUV);
	
	/////////////////////////////////////////////////////////////  end refraction  ////////////////////////////////////////////////////////////////////////////////////////////////////////
	


	/////////////////////////////////////////////////////////////  REFLECTION  ////////////////////////////////////////////////////////////////////////////////////////////////////////
	half3 finalReflection = 0;
	
	
	float3 reflDir = reflect(-viewDir, worldNormal);

	#if defined(PLANAR_REFLECTION) || defined(SSPR_REFLECTION)
		float2 refl_uv = GetScreenSpaceReflectionUV(reflDir);
	#endif

	#if SSPR_REFLECTION
		finalReflection = GetScreenSpaceReflectionWithStretchingMask(refl_uv).xyz;
	#else
		
		half3 cubemapReflection = GetCubemapReflection(reflDir);
		cubemapReflection.xyz *= exposure;
		finalReflection = cubemapReflection.xyz;

		#if PLANAR_REFLECTION
			float3 planarReflection = GetPlanarReflectionWithClipOffset(refl_uv);
			planarReflection.xyz *= exposure;
			float planarAlpha = planarReflection.x + planarReflection.y + planarReflection.z > 0.00001;
			finalReflection = lerp(finalReflection, planarReflection.xyz, planarAlpha);
		#endif
		
		
	#endif
	finalReflection *= surfaceMask;
	
	/////////////////////////////////////////////////////////////  end reflection  ////////////////////////////////////////////////////////////////////////////////////////////////////////
	//return float4(finalReflection, 1);
	
	
	/////////////////////////////////////////////////////////////////////  UNDERWATER  ///////////////////////////////////////////////////////////////////
	#if USE_VOLUMETRIC_LIGHT
		half4 volumeScattering = GetVolumetricLight(refractionUV);
	#else
		half4 volumeScattering = half4(GetAmbientColor(), 1.0);
	#endif
	
	float depthAngleFix = (surfaceMask < 0.5 || KWS_IsCustomMesh == 1) ?        0.25 : saturate(GetWorldSpaceViewDirNorm(i.worldPos - float3(0, KW_WindSpeed * 0.5, 0)).y);
	float refractedSceneZ = GetSceneDepth(refractionUV);
	float fade = GetWaterRawFade(i.worldPos, surfaceDepthZ, refractedSceneZ, surfaceMask, depthAngleFix);
	FixAboveWaterRendering(refractionUV, refractedSceneZ, i.worldPos, sceneZ, surfaceDepthZ, depthAngleFix, screenUV, surfaceMask, fade, refraction, volumeScattering);
	
	half3 underwaterColor = ComputeUnderwaterColor(refraction.xyz, volumeScattering.rgb, fade, KW_Transparent, KW_WaterColor.xyz, KW_Turbidity, KW_TurbidityColor.xyz);
	
	#if defined(KW_FLOW_MAP_FLUIDS) && !defined(KW_FLOW_MAP_EDIT_MODE)
		underwaterColor = GetFluidsColor(underwaterColor, volumeScattering, fluidsFoam);
	#endif
	underwaterColor += ComputeSSS(screenUV, underwaterColor, volumeScattering.a > 0.5, KW_Transparent) * 5;

	#if defined(USE_FOAM)
		underwaterColor = ComputeDepthFoam(underwaterColor, fade, i.worldPos, GetVolumetricLight(screenUV).a, KW_WaterColor.xyz, exposure);
	#endif
	/////////////////////////////////////////////////////////////  end underwater  ////////////////////////////////////////////////////////////////////////////////////////////////////////
	

	#if USE_SHORELINE
		finalReflection = ApplyShorelineWavesReflectionFix(reflDir, finalReflection, underwaterColor);
	#endif
	
	half waterFresnel = ComputeWaterFresnel(worldNormal, viewDir);
	waterFresnel *= surfaceMask;
	half3 finalColor = lerp(underwaterColor, finalReflection, waterFresnel);
	
	#if REFLECT_SUN
		finalColor += ComputeSunlight(worldNormal, viewDir, GetMainLightDir(), GetMainLightColor() * exposure, volumeScattering.a > 0.5, surfaceDepthZ, KW_WaterFarDistance, KW_Transparent);
		//finalColor += sunReflection * (1 - fogOpacity);
	#endif

	
	half3 fogColor;
	half3 fogOpacity;
	GetInternalFogVariables(i.pos, viewDir, surfaceDepthZ, i.screenPos.z, fogColor, fogOpacity);
	finalColor = ComputeInternalFog(finalColor, fogColor, fogOpacity);
	finalColor = ComputeThirdPartyFog(finalColor, i.worldPos, screenUV, i.screenPos.z);

	if (KWS_UseWireframeMode) finalColor = ComputeWireframe(i.surfaceMask.xyz, finalColor);
	
	
	return float4(finalColor, surfaceTensionFade);
}

struct FragmentOutput
{
	half4 dest0 : SV_Target0;
	half dest1 : SV_Target1;
};


FragmentOutput fragDepth(v2fDepth i, float facing : VFACE)
{
	UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
	FragmentOutput o;

	float facingColor = 0.75 - facing * 0.25;
	i.screenPos.xyz /= i.screenPos.w;
	float sceneDepth = GetSceneDepth(i.screenPos.xy);
	if (KWS_UnderwaterVisible == 0 && LinearEyeDepth(sceneDepth) < LinearEyeDepth(i.screenPos.z))
	{
		o.dest0 = float4(facing > 0.5 ?       0.0 : facingColor, 0, 0, 0);
		o.dest1 = facing > 0.5 ?       0 : KWS_WaterPassID * KWS_WATER_MASK_DECODING_VALUE + KWS_WATER_MASK_DECODING_VALUE;
		return o;
	}

	float3 worldPos = i.worldPos_LocalHeight.xyz;
	float waveLocalHeight = i.worldPos_LocalHeight.w;
	float2 uv = worldPos.xz / KW_FFTDomainSize;
	
	half3 norm = KW_NormTex.Sample(sampler_linear_repeat, uv).xyz;
	half3 normScater = KW_NormTex.SampleLevel(sampler_linear_repeat, uv, 3).xyz;
	
	if (KWS_UseMultipleSimulations > 0)
	{
		half3 normScater_lod1 = KW_NormTex_LOD1.SampleLevel(sampler_linear_repeat, worldPos.xz / KW_FFTDomainSize_LOD1, 2).xyz;
		half3 normScater_lod2 = KW_NormTex_LOD2.SampleLevel(sampler_linear_repeat, worldPos.xz / KW_FFTDomainSize_LOD2, 1).xyz;
		normScater = normalize(half3(normScater.xz + normScater_lod1.xz + normScater_lod2.xz, normScater.y * normScater_lod1.y * normScater_lod2.y)).xzy;

		half3 norm_lod1 = KW_NormTex_LOD1.Sample(sampler_linear_repeat, worldPos.xz / KW_FFTDomainSize_LOD1).xyz;
		half3 norm_lod2 = KW_NormTex_LOD2.Sample(sampler_linear_repeat, worldPos.xz / KW_FFTDomainSize_LOD2).xyz;
		norm = normalize(half3(norm.xz + norm_lod1.xz + norm_lod2.xz, norm.y * norm_lod1.y * norm_lod2.y)).xzy;
	}

	int idx;
	half sss = 0;
	half windLimit = clamp((KW_WindSpeed - 1), 0, 1);
	windLimit = lerp(windLimit, windLimit * 0.25, saturate(KW_WindSpeed / 15.0));

	float3 viewDir = GetWorldSpaceViewDirNorm(worldPos);
	float3 lightDir = GetMainLightDir();
	float distanceToCamera = 1 - saturate(GetWorldToCameraDistance(worldPos) * 0.002);
	
	half zeroScattering = saturate(dot(viewDir, - (lightDir + float3(0, 1, 0))));

	float3 H = (lightDir + normScater * float3(-1, 1, -1));
	float scattering = pow(saturate(dot(viewDir, -H)), 3);
	sss += windLimit * (scattering - zeroScattering * 0.95);

	norm.xz *= i.surfaceMask.x > 0.999 ?       1 : 0;

	o.dest0 = half4(0.75 - facing * 0.25, saturate(scattering * waveLocalHeight * distanceToCamera * windLimit), norm.xz * 0.5 + 0.5);
	o.dest1 = (KWS_WaterPassID * KWS_WATER_MASK_DECODING_VALUE + KWS_WATER_MASK_DECODING_VALUE);
	return o;
}