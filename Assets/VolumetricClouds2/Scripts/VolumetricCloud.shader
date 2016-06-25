Shader "VolumetricCloud"
{
	Properties
	{
	    _PerlinNormalMap ("Perlin Normal Map", 2D) = "white" {}

		[Header(Colors)]
		_BaseColor ("Base Color", Color) = (1,1,1,1)
		_Shading ("Shading Color", Color) = (0, 0, 0, 1)
		[Toggle(_NORMALMAP)] _NormalmapState ("Use Normalmap", Float) = 1
			[ToggleHideDrawer(_NormalmapState)] _IndirectContribution("Indirect Lighting", Float) = 1
			[ToggleHideDrawer(_NormalmapState)] _Normalized ("Normalized", Float ) = 0
        _DepthColor ("Depth Intensity", Float ) = 0
		_DistanceBlend ("Distance Blend", Float ) = 0
        [Toggle(_RENDERSHADOWS)] _SSShadows ("Screen Space Shadows", Float) = 0
			[ToggleHideDrawer(_SSShadows)] _ShadowColor ("Shadow Color", Color) = (0,0,0,.5)
			[ToggleHideDrawer(_SSShadows)] _ShadowDrawDistance ("Draw distance", Float) = 999



        [Header(Shape)]
        _Density ("Density", Float ) = 0
        _Alpha ("Alpha", Float ) = 4
        _AlphaCut ("AlphaCut", Float ) = 0.01
        
		[Header(Animation)]
        _TimeMult ("Speed", Float ) = 0.1
        _TimeMultSecondLayer ("Speed Second Layer", Float ) = 4
        _WindDirection ("Wind Direction", Vector) = (1,0,0,0)

		[Header(Dimensions)]
		_CloudTransform("Cloud Transform", vector) =  (100, 20, 0, 0)
		_Tiling ("Tiling", Float ) = 1500

		[Space(10)]

		[Header(Raymarcher)]
		_DrawDistance("Draw distance", Float) = 500.0
		_MaxSteps("Steps Max", int) = 500.0
		_StepSize("Step Size", Float) = 0.015
		_StepSkip("Step Skip", Float) = 10
		_StepNearSurface("Step near surface", Float) = 0.5
		_LodBase("Lod Base", Float) = 0
		_LodOffset("Lod Offset", Float) = 1.2
		_OpacityGain("Opacity Gain", Float) = 0.1
		[KeywordEnumFull(_, _SKIPPIXELONCE, _SKIPPIXELTWICE)] _SkipPixel ("Skip Pixel", int) = 0
		//_Dithering("Dithering", Float) = 0.0

		[Space(10)]

		[Header(Debug)]
		_RenderQueue("Render Queue", Range(0, 5000)) = 2501
		[Toggle]_ZWrite ("ZWrite", int) = 0
		//[HideInInspector] _Cull ("Culling", Range(0, 2)) = 2.0
		//_Mode ("__mode", Float) = 0.0
		//[Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Source Blend", Float) = 5.0
		//[Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Dist Blend", Float) = 10.0
		
		[HideInInspector][KeywordEnumFull(_, _ALPHATEST_ON, _ALPHABLEND_ON, _ALPHAPREMULTIPLY_ON)] _AlphaMode ("Alpha Mode", int) = 2

		
	}

	CGINCLUDE
		#define UNITY_SETUP_BRDF_INPUT MetallicSetup
	ENDCG

	SubShader
	{
		Tags { "RenderType"="Transparent"/*"PerformanceChecks"="False"*/}
		LOD 300
	

		// ------------------------------------------------------------------
		//  Base forward pass (directional light, emission, lightmaps, ...)
		Pass
		{
			Name "FORWARD" 
			Tags { "LightMode"="ForwardBase" "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True"  }
			Blend SrcAlpha OneMinusSrcAlpha//[_SrcBlend] [_DstBlend]
			ZWrite [_ZWrite]
			Cull Back//[_Cull]
			
			CGPROGRAM
			#pragma target 3.0
			// #pragma exclude_renderers gles
			
			// -------------------------------------
					
			#pragma shader_feature _NORMALMAP
			#pragma shader_feature _RENDERSHADOWS
			#pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
			#pragma shader_feature _ _SKIPPIXELONCE _SKIPPIXELTWICE
			#pragma shader_feature _EMISSION
			#pragma shader_feature _METALLICGLOSSMAP 
			#pragma shader_feature ___ _DETAIL_MULX2
			#pragma shader_feature _PARALLAXMAP
			
			#pragma multi_compile_fwdbase
			#pragma multi_compile_fog

			#pragma vertex vertBase
			#pragma fragment fragBase
			#include "UnityStandardCoreForward.cginc"

			ENDCG
		}
		/*
		// ------------------------------------------------------------------
		//  Additive forward pass (one light per pass)
		Pass
		{
			Name "FORWARD_DELTA"
			Tags { "LightMode" = "ForwardAdd" }
			Blend [_SrcBlend] One
			Fog { Color (0,0,0,0) } // in additive pass fog should be black
			ZWrite Off
			ZTest LEqual

			CGPROGRAM
			#pragma target 3.0
			// GLES2.0 temporarily disabled to prevent errors spam on devices without textureCubeLodEXT
			#pragma exclude_renderers gles

			// -------------------------------------

			
			#pragma shader_feature _NORMALMAP
			#pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
			#pragma shader_feature _METALLICGLOSSMAP
			#pragma shader_feature ___ _DETAIL_MULX2
			#pragma shader_feature _PARALLAXMAP
			
			#pragma multi_compile_fwdadd_fullshadows
			#pragma multi_compile_fog

			#pragma vertex vertAdd
			#pragma fragment fragAdd
			#include "UnityStandardCoreForward.cginc"

			ENDCG
		}
		
		// ------------------------------------------------------------------
		//  Shadow rendering pass
		Pass {
			Name "ShadowCaster"
			Tags { "LightMode" = "ShadowCaster" }
			
			ZWrite On ZTest LEqual

			CGPROGRAM
			#pragma target 3.0
			// TEMPORARY: GLES2.0 temporarily disabled to prevent errors spam on devices without textureCubeLodEXT
			#pragma exclude_renderers gles
			
			// -------------------------------------


			#pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
			#pragma multi_compile_shadowcaster

			#pragma vertex vertShadowCaster
			#pragma fragment fragShadowCaster

			#include "UnityStandardShadow.cginc"

			ENDCG
		}
		
		// ------------------------------------------------------------------
		//  Deferred pass
		Pass
		{
			Name "DEFERRED"
			Tags { "LightMode" = "Deferred" }

			CGPROGRAM
			#pragma target 3.0
			// TEMPORARY: GLES2.0 temporarily disabled to prevent errors spam on devices without textureCubeLodEXT
			#pragma exclude_renderers nomrt gles
			

			// -------------------------------------

			#pragma shader_feature _NORMALMAP
			#pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
			#pragma shader_feature _EMISSION
			#pragma shader_feature _METALLICGLOSSMAP
			#pragma shader_feature ___ _DETAIL_MULX2
			#pragma shader_feature _PARALLAXMAP

			#pragma multi_compile ___ UNITY_HDR_ON
			#pragma multi_compile LIGHTMAP_OFF LIGHTMAP_ON
			#pragma multi_compile DIRLIGHTMAP_OFF DIRLIGHTMAP_COMBINED DIRLIGHTMAP_SEPARATE
			#pragma multi_compile DYNAMICLIGHTMAP_OFF DYNAMICLIGHTMAP_ON
			
			#pragma vertex vertDeferred
			#pragma fragment fragDeferred

			#include "UnityStandardCore.cginc"

			ENDCG
		}

		// ------------------------------------------------------------------
		// Extracts information for lightmapping, GI (emission, albedo, ...)
		// This pass it not used during regular rendering.
		
		Pass
		{
			Name "META" 
			Tags { "LightMode"="Meta" }

			Cull Off

			CGPROGRAM
			#pragma vertex vert_meta
			#pragma fragment frag_meta

			#pragma shader_feature _EMISSION
			#pragma shader_feature _METALLICGLOSSMAP
			#pragma shader_feature ___ _DETAIL_MULX2

			#include "UnityStandardMeta.cginc"
			ENDCG
		}*/
	}
	/*
	SubShader
	{
		Tags { "RenderType"="Opaque" "PerformanceChecks"="False" }
		LOD 150

		// ------------------------------------------------------------------
		//  Base forward pass (directional light, emission, lightmaps, ...)
		Pass
		{
			Name "FORWARD" 
			Tags { "LightMode" = "ForwardBase" }

			Blend [_SrcBlend] [_DstBlend]
			ZWrite [_ZWrite]

			CGPROGRAM
			#pragma target 2.0
			
			#pragma shader_feature _NORMALMAP
			#pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
			#pragma shader_feature _EMISSION 
			#pragma shader_feature _METALLICGLOSSMAP 
			#pragma shader_feature ___ _DETAIL_MULX2
			// SM2.0: NOT SUPPORTED shader_feature _PARALLAXMAP

			#pragma skip_variants SHADOWS_SOFT DIRLIGHTMAP_COMBINED DIRLIGHTMAP_SEPARATE

			#pragma multi_compile_fwdbase
			#pragma multi_compile_fog

			#pragma vertex vertBase
			#pragma fragment fragBase
			#include "UnityStandardCoreForward.cginc"

			ENDCG
		}
		// ------------------------------------------------------------------
		//  Additive forward pass (one light per pass)
		Pass
		{
			Name "FORWARD_DELTA"
			Tags { "LightMode" = "ForwardAdd" }
			Blend [_SrcBlend] One
			Fog { Color (0,0,0,0) } // in additive pass fog should be black
			ZWrite Off
			ZTest LEqual
			
			CGPROGRAM
			#pragma target 2.0

			#pragma shader_feature _NORMALMAP
			#pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
			#pragma shader_feature _METALLICGLOSSMAP
			#pragma shader_feature ___ _DETAIL_MULX2
			// SM2.0: NOT SUPPORTED shader_feature _PARALLAXMAP
			#pragma skip_variants SHADOWS_SOFT
			
			#pragma multi_compile_fwdadd_fullshadows
			#pragma multi_compile_fog
			
			#pragma vertex vertAdd
			#pragma fragment fragAdd
			#include "UnityStandardCoreForward.cginc"

			ENDCG
		}
		// ------------------------------------------------------------------
		//  Shadow rendering pass
		Pass {
			Name "ShadowCaster"
			Tags { "LightMode" = "ShadowCaster" }
			
			ZWrite On ZTest LEqual

			CGPROGRAM
			#pragma target 2.0

			#pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
			#pragma skip_variants SHADOWS_SOFT
			#pragma multi_compile_shadowcaster

			#pragma vertex vertShadowCaster
			#pragma fragment fragShadowCaster

			#include "UnityStandardShadow.cginc"

			ENDCG
		}

		// ------------------------------------------------------------------
		// Extracts information for lightmapping, GI (emission, albedo, ...)
		// This pass it not used during regular rendering.
		Pass
		{
			Name "META" 
			Tags { "LightMode"="Meta" }

			Cull Off

			CGPROGRAM
			#pragma vertex vert_meta
			#pragma fragment frag_meta

			#pragma shader_feature _EMISSION
			#pragma shader_feature _METALLICGLOSSMAP
			#pragma shader_feature ___ _DETAIL_MULX2

			#include "UnityStandardMeta.cginc"
			ENDCG
		}
	}*/


	//FallBack "VertexLit"
	CustomEditor "NPRShaderGUI"
}
