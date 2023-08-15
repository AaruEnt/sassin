Shader "KriptoFX/KWS/Water"
{
	Properties
	{
		srpBatcherFix ("srpBatcherFix", Float) = 0
	}

	SubShader
	{

		Tags { "Queue" = "Transparent-1" "IgnoreProjector" = "True" "RenderType" = "Transparent" "DisableBatching" = "true" }
		
		Blend SrcAlpha OneMinusSrcAlpha

		Stencil
		{
			Ref 230
			Comp Greater
			Pass keep
		}


		//0 water pass quadtree
		Pass
		{
			ZWrite On

			Cull Back
			HLSLPROGRAM

			#define USE_WATER_INSTANCING
		
			#pragma shader_feature  KW_FLOW_MAP_EDIT_MODE

			#pragma multi_compile _ KW_FLOW_MAP KW_FLOW_MAP_FLUIDS
			#pragma multi_compile _ KW_DYNAMIC_WAVES
			#pragma multi_compile _ SSPR_REFLECTION
			#pragma multi_compile _ PLANAR_REFLECTION
			#pragma multi_compile _ USE_SHORELINE
			#pragma multi_compile _ USE_FOAM
 			#pragma multi_compile _ REFLECT_SUN
			#pragma multi_compile _ USE_VOLUMETRIC_LIGHT
			#pragma multi_compile _ USE_FILTERING

			#pragma multi_compile_fog

			#include "Includes/KWS_VertFragIncludes.cginc"
			#pragma vertex vert
			#pragma fragment frag
			#pragma editor_sync_compilation
			#pragma target 4.6

			ENDHLSL
		}

		
		//1 water pass quadtree tesselated
		Pass
		{
			ZWrite On
			Cull Back

			HLSLPROGRAM
			
			#define USE_WATER_INSTANCING

			#pragma shader_feature  KW_FLOW_MAP_EDIT_MODE
		
			#pragma multi_compile _ KW_FLOW_MAP KW_FLOW_MAP_FLUIDS
			#pragma multi_compile _ KW_DYNAMIC_WAVES
			#pragma multi_compile _ SSPR_REFLECTION
			#pragma multi_compile _ PLANAR_REFLECTION
			#pragma multi_compile _ USE_SHORELINE			
			#pragma multi_compile _ USE_FOAM
			#pragma multi_compile _ REFLECT_SUN
			#pragma multi_compile _ USE_VOLUMETRIC_LIGHT
			#pragma multi_compile _ USE_FILTERING
		
			#include "Includes/KWS_VertFragIncludes.cginc"
			#include "../Common/KWS_Tessellation.cginc"
			
			#pragma vertex vertHull
			#pragma fragment frag
			#pragma hull HS
			#pragma domain DS
			#pragma editor_sync_compilation
			#pragma target 4.6

			ENDHLSL
		}


		//2 water pass custom/river
		Pass
		{
			ZWrite On

			Cull Back
			HLSLPROGRAM
			
			#include "Includes/KWS_VertFragIncludes.cginc"

			#pragma shader_feature  KW_FLOW_MAP_EDIT_MODE

			#pragma multi_compile _ KW_FLOW_MAP KW_FLOW_MAP_FLUIDS
			#pragma multi_compile _ KW_DYNAMIC_WAVES
			#pragma multi_compile _ SSPR_REFLECTION
			#pragma multi_compile _ PLANAR_REFLECTION
			#pragma multi_compile _ USE_SHORELINE			
			#pragma multi_compile _ USE_FOAM
			#pragma multi_compile _ REFLECT_SUN
			#pragma multi_compile _ USE_VOLUMETRIC_LIGHT
			#pragma multi_compile _ USE_FILTERING

			#pragma vertex vert
			#pragma fragment frag
			#pragma editor_sync_compilation
			#pragma target 4.6

			ENDHLSL
		}


		//3 water pass custom/river tesselated
		Pass
		{
			ZWrite On
			Cull Back

			HLSLPROGRAM

			#include "Includes/KWS_VertFragIncludes.cginc"
			#include "../Common/KWS_Tessellation.cginc"

			#pragma shader_feature  KW_FLOW_MAP_EDIT_MODE

			#pragma multi_compile _ KW_FLOW_MAP KW_FLOW_MAP_FLUIDS
			#pragma multi_compile _ KW_DYNAMIC_WAVES
			#pragma multi_compile _ SSPR_REFLECTION
			#pragma multi_compile _ PLANAR_REFLECTION
			#pragma multi_compile _ USE_SHORELINE			
			#pragma multi_compile _ USE_FOAM
			#pragma multi_compile _ REFLECT_SUN
			#pragma multi_compile _ USE_VOLUMETRIC_LIGHT
			#pragma multi_compile _ USE_FILTERING
			
			#pragma editor_sync_compilation
			#pragma target 4.6

			#pragma vertex vertHull
			#pragma fragment frag
			#pragma hull HS
			#pragma domain DS

			ENDHLSL
		}

		
		//4 water side pass
		Pass
		{
			ZWrite On

			Cull Back
			HLSLPROGRAM
			
			#include "Includes/KWS_VertFragIncludes.cginc"

			#pragma multi_compile _ KW_FLOW_MAP KW_FLOW_MAP_FLUIDS
			#pragma multi_compile _ KW_DYNAMIC_WAVES
			#pragma multi_compile _ SSPR_REFLECTION
			#pragma multi_compile _ PLANAR_REFLECTION
			#pragma multi_compile _ USE_VOLUMETRIC_LIGHT

			#pragma vertex vert
			#pragma fragment frag
			#pragma editor_sync_compilation
			#pragma target 4.6

			ENDHLSL
		}


		//5 MaskDepthNormal quadtree
		Pass
		{
			Blend One Zero
			ZWrite On
			Cull Off

			HLSLPROGRAM

			#define USE_WATER_INSTANCING

			#pragma multi_compile _ KW_FLOW_MAP KW_FLOW_MAP_FLUIDS
			#pragma multi_compile _ KW_DYNAMIC_WAVES
			#pragma multi_compile _ USE_SHORELINE
			#pragma multi_compile _ USE_FILTERING

			#include "Includes/KWS_VertFragIncludes.cginc"

			#pragma vertex vertDepth
			#pragma fragment fragDepth
			#pragma editor_sync_compilation


			ENDHLSL
		}


		//6 MaskDepthNormalPass quadtree tesselated
		Pass
		{
			Blend One Zero
			ZWrite On
			Cull Off

			HLSLPROGRAM
			#define USE_WATER_INSTANCING

			#pragma multi_compile _ KW_FLOW_MAP KW_FLOW_MAP_FLUIDS
			#pragma multi_compile _ KW_DYNAMIC_WAVES
			#pragma multi_compile _ USE_SHORELINE
			#pragma multi_compile _ USE_FILTERING

			#include "Includes/KWS_VertFragIncludes.cginc"
			#include "../Common/KWS_Tessellation.cginc"

			

			#pragma vertex vertHull
			#pragma fragment fragDepth
			#pragma hull HS
			#pragma domain DS_Depth
			#pragma editor_sync_compilation
			#pragma target 4.6

			ENDHLSL
		}

		//7 MaskDepthNormal custom/river
		Pass
		{
			Blend One Zero
			ZWrite On
			Cull Off

			HLSLPROGRAM

			#include "Includes/KWS_VertFragIncludes.cginc"

			#pragma multi_compile _ KW_FLOW_MAP KW_FLOW_MAP_FLUIDS
			#pragma multi_compile _ KW_DYNAMIC_WAVES
			#pragma multi_compile _ USE_SHORELINE
			#pragma multi_compile _ USE_FILTERING

			#pragma vertex vertDepth
			#pragma fragment fragDepth
			#pragma editor_sync_compilation


			ENDHLSL
		}


		//8 MaskDepthNormalPass custom/river tesselated
		Pass
		{
			Blend One Zero
			ZWrite On
			Cull Off

			HLSLPROGRAM
			
			#include "Includes/KWS_VertFragIncludes.cginc"
			#include "../Common/KWS_Tessellation.cginc"

			#pragma multi_compile _ KW_FLOW_MAP KW_FLOW_MAP_FLUIDS
			#pragma multi_compile _ KW_DYNAMIC_WAVES
			#pragma multi_compile _ USE_SHORELINE
			#pragma multi_compile _ USE_FILTERING

			#pragma target 4.6
			#pragma editor_sync_compilation

			#pragma vertex vertHull
			#pragma fragment fragDepth
			#pragma hull HS
			#pragma domain DS_Depth

			ENDHLSL
		}

		//9 MaskDepthNormalPass side
		Pass
		{
			Blend One Zero
			ZWrite On
			Cull Off

			HLSLPROGRAM

			#include "Includes/KWS_VertFragIncludes.cginc"

			#pragma multi_compile _ KW_FLOW_MAP KW_FLOW_MAP_FLUIDS
			#pragma multi_compile _ KW_DYNAMIC_WAVES
			#pragma multi_compile _ USE_SHORELINE
			#pragma multi_compile _ USE_FILTERING

			#pragma vertex vertDepth
			#pragma fragment fragDepth
			#pragma editor_sync_compilation


			ENDHLSL
		}
	}
}