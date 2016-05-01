#ifndef UNITY_STANDARD_CORE_INCLUDED
#define UNITY_STANDARD_CORE_INCLUDED

#include "UnityCG.cginc"

#include "UnityShaderVariables.cginc"
#include "UnityStandardConfig.cginc"
#include "UnityStandardInput.cginc"
#include "UnityPBSLighting.cginc"
#include "UnityStandardUtils.cginc"
#include "UnityStandardBRDF.cginc"

#include "AutoLight.cginc"

//-------------------------------------------------------------------------------------
// counterpart for NormalizePerPixelNormal
// skips normalization per-vertex and expects normalization to happen per-pixel
half3 NormalizePerVertexNormal (half3 n)
{
	#if (SHADER_TARGET < 30) || UNITY_STANDARD_SIMPLE
		return normalize(n);
	#else
		return n; // will normalize per-pixel instead
	#endif
}

half3 NormalizePerPixelNormal (half3 n)
{
	#if (SHADER_TARGET < 30) || UNITY_STANDARD_SIMPLE
		return n;
	#else
		return normalize(n);
	#endif
}

//-------------------------------------------------------------------------------------
UnityLight MainLight (half3 normalWorld)
{
	UnityLight l;
	#ifdef LIGHTMAP_OFF
		
		l.color = _LightColor0.rgb;
		l.dir = _WorldSpaceLightPos0.xyz;
		l.ndotl = LambertTerm (normalWorld, l.dir);
	#else
		// no light specified by the engine
		// analytical light might be extracted from Lightmap data later on in the shader depending on the Lightmap type
		l.color = half3(0.f, 0.f, 0.f);
		l.ndotl  = 0.f;
		l.dir = half3(0.f, 0.f, 0.f);
	#endif

	return l;
}

UnityLight AdditiveLight (half3 normalWorld, half3 lightDir, half atten)
{
	UnityLight l;

	l.color = _LightColor0.rgb;
	l.dir = lightDir;
	#ifndef USING_DIRECTIONAL_LIGHT
		l.dir = NormalizePerPixelNormal(l.dir);
	#endif
	l.ndotl = LambertTerm (normalWorld, l.dir);

	// shadow the light
	l.color *= atten;
	return l;
}

UnityLight DummyLight (half3 normalWorld)
{
	UnityLight l;
	l.color = 0;
	l.dir = half3 (0,1,0);
	l.ndotl = LambertTerm (normalWorld, l.dir);
	return l;
}

UnityIndirect ZeroIndirect ()
{
	UnityIndirect ind;
	ind.diffuse = 0;
	ind.specular = 0;
	return ind;
}

//-------------------------------------------------------------------------------------
// Common fragment setup

// deprecated
half3 WorldNormal(half4 tan2world[3])
{
	return normalize(tan2world[2].xyz);
}

// deprecated
#ifdef _TANGENT_TO_WORLD
	half3x3 ExtractTangentToWorldPerPixel(half4 tan2world[3])
	{
		half3 t = tan2world[0].xyz;
		half3 b = tan2world[1].xyz;
		half3 n = tan2world[2].xyz;

	#if UNITY_TANGENT_ORTHONORMALIZE
		n = NormalizePerPixelNormal(n);

		// ortho-normalize Tangent
		t = normalize (t - n * dot(t, n));

		// recalculate Binormal
		half3 newB = cross(n, t);
		b = newB * sign (dot (newB, b));
	#endif

		return half3x3(t, b, n);
	}
#else
	half3x3 ExtractTangentToWorldPerPixel(half4 tan2world[3])
	{
		return half3x3(0,0,0,0,0,0,0,0,0);
	}
#endif

half3 PerPixelWorldNormal(float4 i_tex, half4 tangentToWorld[3])
{
#ifdef _NORMALMAP
	half3 tangent = tangentToWorld[0].xyz;
	half3 binormal = tangentToWorld[1].xyz;
	half3 normal = tangentToWorld[2].xyz;

	#if UNITY_TANGENT_ORTHONORMALIZE
		normal = NormalizePerPixelNormal(normal);

		// ortho-normalize Tangent
		tangent = normalize (tangent - normal * dot(tangent, normal));

		// recalculate Binormal
		half3 newB = cross(normal, tangent);
		binormal = newB * sign (dot (newB, binormal));
	#endif

	half3 normalTangent = NormalInTangentSpace(i_tex);
	half3 normalWorld = NormalizePerPixelNormal(tangent * normalTangent.x + binormal * normalTangent.y + normal * normalTangent.z); // @TODO: see if we can squeeze this normalize on SM2.0 as well
#else
	half3 normalWorld = normalize(tangentToWorld[2].xyz);
#endif
	return normalWorld;
}

#ifdef _PARALLAXMAP
	#define IN_VIEWDIR4PARALLAX(i) NormalizePerPixelNormal(half3(i.tangentToWorldAndParallax[0].w,i.tangentToWorldAndParallax[1].w,i.tangentToWorldAndParallax[2].w))
	#define IN_VIEWDIR4PARALLAX_FWDADD(i) NormalizePerPixelNormal(i.viewDirForParallax.xyz)
#else
	#define IN_VIEWDIR4PARALLAX(i) half3(0,0,0)
	#define IN_VIEWDIR4PARALLAX_FWDADD(i) half3(0,0,0)
#endif

#if UNITY_SPECCUBE_BOX_PROJECTION
	#define IN_WORLDPOS(i) i.posWorld
#else
	#define IN_WORLDPOS(i) half3(0,0,0)
#endif

#define IN_LIGHTDIR_FWDADD(i) half3(i.tangentToWorldAndLightDir[0].w, i.tangentToWorldAndLightDir[1].w, i.tangentToWorldAndLightDir[2].w)

#define FRAGMENT_SETUP(x) FragmentCommonData x = \
	FragmentSetup(i.tex, i.eyeVec, IN_VIEWDIR4PARALLAX(i), i.tangentToWorldAndParallax, IN_WORLDPOS(i));

#define FRAGMENT_SETUP_FWDADD(x) FragmentCommonData x = \
	FragmentSetup(i.tex, i.eyeVec, IN_VIEWDIR4PARALLAX_FWDADD(i), i.tangentToWorldAndLightDir, half3(0,0,0));

struct FragmentCommonData
{
	half3 diffColor, specColor;
	// Note: oneMinusRoughness & oneMinusReflectivity for optimization purposes, mostly for DX9 SM2.0 level.
	// Most of the math is being done on these (1-x) values, and that saves a few precious ALU slots.
	half oneMinusReflectivity, oneMinusRoughness;
	half3 normalWorld, eyeVec, posWorld;
	half alpha;

#if UNITY_OPTIMIZE_TEXCUBELOD || UNITY_STANDARD_SIMPLE
	half3 reflUVW;
#endif

#if UNITY_STANDARD_SIMPLE
	half3 tangentSpaceNormal;
#endif
};

#ifndef UNITY_SETUP_BRDF_INPUT
	#define UNITY_SETUP_BRDF_INPUT SpecularSetup
#endif

inline FragmentCommonData SpecularSetup (float4 i_tex)
{
	half4 specGloss = SpecularGloss(i_tex.xy);
	half3 specColor = specGloss.rgb;
	half oneMinusRoughness = specGloss.a;

	half oneMinusReflectivity;
	half3 diffColor = EnergyConservationBetweenDiffuseAndSpecular (Albedo(i_tex), specColor, /*out*/ oneMinusReflectivity);
	
	FragmentCommonData o = (FragmentCommonData)0;
	o.diffColor = diffColor;
	o.specColor = specColor;
	o.oneMinusReflectivity = oneMinusReflectivity;
	o.oneMinusRoughness = oneMinusRoughness;
	return o;
}

inline FragmentCommonData MetallicSetup (float4 i_tex)
{
	half2 metallicGloss = MetallicGloss(i_tex.xy);
	half metallic = metallicGloss.x;
	half oneMinusRoughness = metallicGloss.y;		// this is 1 minus the square root of real roughness m.

	half oneMinusReflectivity;
	half3 specColor;
	half3 diffColor = DiffuseAndSpecularFromMetallic (Albedo(i_tex), metallic, /*out*/ specColor, /*out*/ oneMinusReflectivity);

	FragmentCommonData o = (FragmentCommonData)0;
	o.diffColor = diffColor;
	o.specColor = specColor;
	o.oneMinusReflectivity = oneMinusReflectivity;
	o.oneMinusRoughness = oneMinusRoughness;
	return o;
} 

inline FragmentCommonData FragmentSetup (float4 i_tex, half3 i_eyeVec, half3 i_viewDirForParallax, half4 tangentToWorld[3], half3 i_posWorld)
{
	i_tex = Parallax(i_tex, i_viewDirForParallax);

	half alpha = Alpha(i_tex.xy);
	//#if defined(_ALPHATEST_ON)
	#if defined(_ALPHA_TEST)
		clip (alpha - _Cutoff);
	#endif

	FragmentCommonData o = UNITY_SETUP_BRDF_INPUT (i_tex);
	o.normalWorld = PerPixelWorldNormal(i_tex, tangentToWorld);
	o.eyeVec = NormalizePerPixelNormal(i_eyeVec);
	o.posWorld = i_posWorld;

	// NOTE: shader relies on pre-multiply alpha-blend (_SrcBlend = One, _DstBlend = OneMinusSrcAlpha)
	o.diffColor = PreMultiplyAlpha (o.diffColor, alpha, o.oneMinusReflectivity, /*out*/ o.alpha);
	return o;
}

inline UnityGI FragmentGI (FragmentCommonData s, half occlusion, half4 i_ambientOrLightmapUV, half atten, UnityLight light, bool reflections)
{
	UnityGIInput d;
	d.light = light;
	d.worldPos = s.posWorld;
	d.worldViewDir = -s.eyeVec;
	d.atten = atten;
	#if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
		d.ambient = 0;
		d.lightmapUV = i_ambientOrLightmapUV;
	#else
		d.ambient = i_ambientOrLightmapUV.rgb;
		d.lightmapUV = 0;
	#endif
	d.boxMax[0] = unity_SpecCube0_BoxMax;
	d.boxMin[0] = unity_SpecCube0_BoxMin;
	d.probePosition[0] = unity_SpecCube0_ProbePosition;
	d.probeHDR[0] = unity_SpecCube0_HDR;

	d.boxMax[1] = unity_SpecCube1_BoxMax;
	d.boxMin[1] = unity_SpecCube1_BoxMin;
	d.probePosition[1] = unity_SpecCube1_ProbePosition;
	d.probeHDR[1] = unity_SpecCube1_HDR;

	if(reflections)
	{
		Unity_GlossyEnvironmentData g;
		g.roughness		= 1 - s.oneMinusRoughness;
	#if UNITY_OPTIMIZE_TEXCUBELOD || UNITY_STANDARD_SIMPLE
		g.reflUVW 		= s.reflUVW;
	#else
		g.reflUVW		= reflect(s.eyeVec, s.normalWorld);
	#endif

		return UnityGlobalIllumination (d, occlusion, s.normalWorld, g);
	}
	else
	{
		return UnityGlobalIllumination (d, occlusion, s.normalWorld);
	}
}

inline UnityGI FragmentGI (FragmentCommonData s, half occlusion, half4 i_ambientOrLightmapUV, half atten, UnityLight light)
{
	return FragmentGI(s, occlusion, i_ambientOrLightmapUV, atten, light, true);
}


//-------------------------------------------------------------------------------------
half4 OutputForward (half4 output, half alphaFromSurface)
{
	//#if defined(_ALPHABLEND_ON) || defined(_ALPHAPREMULTIPLY_ON)
	#if defined(_ALPHA_BLEND) || defined(_ALPHA_PREMULTIPLY)
		output.a = alphaFromSurface;
	#endif
	#if defined(_ALPHA_NONE)
		UNITY_OPAQUE_ALPHA(output.a);
	#endif
	return output;
}

inline half4 VertexGIForward(VertexInput v, float3 posWorld, half3 normalWorld)
{
	half4 ambientOrLightmapUV = 0;
	// Static lightmaps
	#ifndef LIGHTMAP_OFF
		ambientOrLightmapUV.xy = v.uv1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
		ambientOrLightmapUV.zw = 0;
	// Sample light probe for Dynamic objects only (no static or dynamic lightmaps)
	#elif UNITY_SHOULD_SAMPLE_SH
		#ifdef VERTEXLIGHT_ON
			// Approximated illumination from non-important point lights
			ambientOrLightmapUV.rgb = Shade4PointLights (
				unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
				unity_LightColor[0].rgb, unity_LightColor[1].rgb, unity_LightColor[2].rgb, unity_LightColor[3].rgb,
				unity_4LightAtten0, posWorld, normalWorld);
		#endif

		ambientOrLightmapUV.rgb = ShadeSHPerVertex (normalWorld, ambientOrLightmapUV.rgb);		
	#endif

	#ifdef DYNAMICLIGHTMAP_ON
		ambientOrLightmapUV.zw = v.uv2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
	#endif

	return ambientOrLightmapUV;
}

// ------------------------------------------------------------------
//  Base forward pass (directional light, emission, lightmaps, ...)

struct VertexOutputForwardBase
{
	float4 pos							: SV_POSITION;
	fixed4 vertexColor                  : COLOR;
	float4 tex							: TEXCOORD0;
	half3 eyeVec 						: TEXCOORD1;
	half4 tangentToWorldAndParallax[3]	: TEXCOORD2;	// [3x3:tangentToWorld | 1x3:viewDirForParallax]
	half4 ambientOrLightmapUV			: TEXCOORD5;	// SH or Lightmap UV
	SHADOW_COORDS(6)
	UNITY_FOG_COORDS(7)

	// next ones would not fit into SM2.0 limits, but they are always for SM3.0+
	#if UNITY_SPECCUBE_BOX_PROJECTION
		float3 posWorld					: TEXCOORD8;
	#endif

	#if UNITY_OPTIMIZE_TEXCUBELOD
		#if UNITY_SPECCUBE_BOX_PROJECTION
			half3 reflUVW				: TEXCOORD9;
		#else
			half3 reflUVW				: TEXCOORD8;
		#endif
	#endif
	float4 projPos : TEXCOORD10;
	float4 screenPos : TEXCOORD11;
};

VertexOutputForwardBase vertForwardBase (VertexInput v)
{
	VertexOutputForwardBase o;
	UNITY_INITIALIZE_OUTPUT(VertexOutputForwardBase, o);
	o.vertexColor = v.vertexColor;
	float4 posWorld = mul(_Object2World, v.vertex);
	#if UNITY_SPECCUBE_BOX_PROJECTION
		o.posWorld = posWorld.xyz;
	#endif
	o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
	o.tex = TexCoords(v);
	o.eyeVec = NormalizePerVertexNormal(posWorld.xyz - _WorldSpaceCameraPos);
	float3 normalWorld = UnityObjectToWorldNormal(v.normal);
	#ifdef _TANGENT_TO_WORLD
		float4 tangentWorld = float4(UnityObjectToWorldDir(v.tangent.xyz), v.tangent.w);

		float3x3 tangentToWorld = CreateTangentToWorldPerVertex(normalWorld, tangentWorld.xyz, tangentWorld.w);
		o.tangentToWorldAndParallax[0].xyz = tangentToWorld[0];
		o.tangentToWorldAndParallax[1].xyz = tangentToWorld[1];
		o.tangentToWorldAndParallax[2].xyz = tangentToWorld[2];
	#else
		o.tangentToWorldAndParallax[0].xyz = 0;
		o.tangentToWorldAndParallax[1].xyz = 0;
		o.tangentToWorldAndParallax[2].xyz = normalWorld;
	#endif
	//We need this for shadow receving
	TRANSFER_SHADOW(o);

	o.ambientOrLightmapUV = VertexGIForward(v, posWorld, normalWorld);
	
	#ifdef _PARALLAXMAP
		TANGENT_SPACE_ROTATION;
		half3 viewDirForParallax = mul (rotation, ObjSpaceViewDir(v.vertex));
		o.tangentToWorldAndParallax[0].w = viewDirForParallax.x;
		o.tangentToWorldAndParallax[1].w = viewDirForParallax.y;
		o.tangentToWorldAndParallax[2].w = viewDirForParallax.z;
	#endif

	#if UNITY_OPTIMIZE_TEXCUBELOD
		o.reflUVW 		= reflect(o.eyeVec, normalWorld);
	#endif

	UNITY_TRANSFER_FOG(o,o.pos);

	o.projPos = ComputeScreenPos (o.pos);
    COMPUTE_EYEDEPTH(o.projPos.z);
    o.screenPos = mul(UNITY_MATRIX_MVP, v.vertex );
	return o;
}














// -------------------------------------------------------------------------------------------------------------------------------------------------------------------------- EIDEREN : MAIN FRAGMENT PASS

uniform fixed4 _TimeEditor;
uniform fixed _TimeMult;
uniform fixed _TimeMultSecondLayer;
uniform fixed _Tiling;
fixed _Normalized;
fixed _IndirectContribution;
uniform fixed _Density;
uniform fixed4 _BaseColor;
uniform fixed _Alpha;
uniform fixed _AlphaCut;
uniform fixed _DepthColor;
uniform sampler2D _PerlinNormalMap;uniform float4 _PerlinNormalMap_ST;
uniform fixed4 _WindDirection;
uniform sampler2D _CameraDepthTexture;
uniform fixed4 _Shading;
fixed4 _ShadowColor;
float _ShadowDrawDistance;
int _MaxSteps;
float _StepSize;
float _StepSkip;
float _StepNearSurface;
float _DrawDistance;
float _LodBase;
float _LodOffset;
float _OpacityGain;
float _DistanceBlend;
// x = pos vertical, y = size vertical, rest offset horizontal
float4 _CloudTransform;
float _Dithering;
int _SkipPixel;
float4x4 _ToWorldMatrix;


half4 BRDF1_Unity_PBSMODIFIED (half3 diffColor, half3 specColor, half oneMinusReflectivity, half oneMinusRoughness,
	half3 normal, half3 viewDir,
	UnityLight light, UnityIndirect gi)
{
	gi.diffuse *= _IndirectContribution;
	//gi.specular *= _IndirectContribution;
	
	half roughness = 1-oneMinusRoughness;
	half3 halfDir = Unity_SafeNormalize (light.dir + viewDir);
	
	//half nl = light.ndotl;
	fixed nl = dot(normal, light.dir);
	nl = saturate(lerp(nl, nl*0.5+0.5, _Normalized));

	//half nh = BlinnTerm (normal, halfDir);
	half nv = DotClamped (normal, viewDir);
	//half lv = DotClamped (light.dir, viewDir);
	half lh = DotClamped (light.dir, halfDir);
	/*
#if UNITY_BRDF_GGX
	half V = SmithJointGGXVisibilityTerm (nl, nv, roughness);
	half D = GGXTerm (nh, roughness);
#else
	half V = SmithBeckmannVisibilityTerm (nl, nv, roughness);
	half D = NDFBlinnPhongNormalizedTerm (nh, RoughnessToSpecPower (roughness));
#endif*/

	half nlPow5 = Pow5 (1-nl);//
	half nvPow5 = Pow5 (1-nv);//
	half Fd90 = 0.5 + 2 * lh * lh * roughness;//
	half disneyDiffuse = (1 + (Fd90-1) * nlPow5) * (1 + (Fd90-1) * nvPow5);//necess
	/*
	// HACK: theoretically we should divide by Pi diffuseTerm and not multiply specularTerm!
	// BUT 1) that will make shader look significantly darker than Legacy ones
	// and 2) on engine side "Non-important" lights have to be divided by Pi to in cases when they are injected into ambient SH
	// NOTE: multiplication by Pi is part of single constant together with 1/4 now
	half specularTerm = (V * D) * (UNITY_PI/4); // Torrance-Sparrow model, Fresnel is applied later (for optimization reasons)
	if (IsGammaSpace())
		specularTerm = sqrt(max(1e-4h, specularTerm));
	specularTerm = max(0, specularTerm * nl);*/

	half diffuseTerm = disneyDiffuse * nl;//necess

	//half grazingTerm = saturate(oneMinusRoughness + (1-oneMinusReflectivity));
	/*
    half3 color =	diffColor * (gi.diffuse + light.color * diffuseTerm)
                    + specularTerm * light.color * FresnelTerm (specColor, lh)
					+ gi.specular * FresnelLerp (specColor, grazingTerm, nv);*/
	half3 color =	diffColor * lerp( _Shading.rgb, _Shading.a, (gi.diffuse + light.color * diffuseTerm));
                    //+ specularTerm * light.color * FresnelTerm (specColor, lh)
					//+ gi.specular * FresnelLerp (specColor, grazingTerm, nv);

	return half4(color, 1);
}

inline float4 SampleClouds(float2 UV, float sampleHeight, float lod) {
	//UV = //tex2Dlod(_PerlinNormalMap, float4(UV, 0, lod)).a;
	float2 baseAnimation = (_Time.g + _TimeEditor.g) * 0.001 * _WindDirection.rb;
	float2 worldUV = (UV+_CloudTransform.zw)/_Tiling;//fixed2 worldUV = _PerlinNormalMap_ST.zw + lerp(UV.xz / (_Tiling * _PerlinNormalMap_ST.xy), uv0 / (_Tiling * _PerlinNormalMap_ST.xy * 0.0005), _ObjectSpaceMapping);
                
	float2 newUV = worldUV + (baseAnimation * _TimeMult);
	float2 newUV2 = worldUV + (baseAnimation * _TimeMultSecondLayer) + float2(0.0, 0.5);
	
	float4 cloudTexture = tex2Dlod(_PerlinNormalMap, float4(newUV, 0, lod));
	float4 cloudTexture2 = tex2Dlod(_PerlinNormalMap, float4(newUV2, 0, lod));
	
	cloudTexture.xyz = cloudTexture.xyz*2-1;
	cloudTexture2.xyz = cloudTexture2.xyz*2-1;

	float4 baseCloud = ((cloudTexture + _Density)*sampleHeight) - cloudTexture2;
	baseCloud.a = saturate(baseCloud.a*_Alpha);
	return baseCloud;
}

inline float3 IntersectionOnPlane(float3 offsetOrthogonalToPlane, float3 rayDirection, out bool oob)
{
	float dotToSurface = dot(normalize(offsetOrthogonalToPlane), rayDirection);
	if(dotToSurface <= 0.0f)
	{
		oob = true;
		return float3(0, 0, 0);
	}
	oob = false;
	return rayDirection * length(offsetOrthogonalToPlane) / dotToSurface;
}

// Offset is the percentage of tile displacement, 1 = a tile, 0.5 = half a tile
inline float CreateGrid(float2 pixelPos, float2 offset, float resolution)
{
	float2 transform = round(frac((offset*0.5f*resolution + pixelPos) / resolution));
	return distance(transform.r+transform.g, 1.0f);
}

inline half4 VertexGIForwardReplacement(float3 posWorld, half3 normalWorld)
{
	half4 ambientOrLightmapUV = 0;
	#if UNITY_SHOULD_SAMPLE_SH
		#ifdef VERTEXLIGHT_ON
			ambientOrLightmapUV.rgb = Shade4PointLights (
				unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
				unity_LightColor[0].rgb, unity_LightColor[1].rgb, unity_LightColor[2].rgb, unity_LightColor[3].rgb,
				unity_4LightAtten0, posWorld, normalWorld);
		#endif

		ambientOrLightmapUV.rgb = ShadeSHPerVertex (normalWorld, ambientOrLightmapUV.rgb);		
	#endif
	return ambientOrLightmapUV;
}

inline float3 NormalLighting(FragmentCommonData s, float3 samplePos, float3 normalDir)
{
	s.normalWorld = normalDir;
	UnityLight mainLight = MainLight (normalDir);
	half atten = 1.0f;

	half occlusion = 1.0f;
	UnityGI gi = FragmentGI (s, occlusion, VertexGIForwardReplacement(samplePos, normalDir), atten, mainLight);

	half3 c = BRDF1_Unity_PBSMODIFIED (s.diffColor, s.specColor, s.oneMinusReflectivity, s.oneMinusRoughness, normalDir, -s.eyeVec, gi.light, gi.indirect);
	//c += UNITY_BRDF_GI (s.diffColor, s.specColor, s.oneMinusReflectivity, s.oneMinusRoughness, normalDir, -s.eyeVec, occlusion, gi);
	return lerp(float3(1, 1, 1), c, _BaseColor.a);
}



// Too slow, optimisation works too well with standard planar mapping, cuts perfs for more than 3/4 so commented out
inline float CloudBaseHeight(float3 samplePos, float3 camPos)
{
	return _CloudTransform.x;//-distance(samplePos.xz, camPos.xz)*0.1f;
}

//Returns World Position of a pixel from clipspace depth map
inline float4 WorldPosFromDepth(float2 uv)
{
	float vz = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv));
    float2 p11_22 = float2(unity_CameraProjection._11, unity_CameraProjection._22);
    float3 vpos = float3((uv * 2 - 1) / p11_22, -1) * vz;
	float4 wpos = mul(_ToWorldMatrix, float4(vpos, 1));
    return wpos;
}


inline float4 RaymarchShadowClouds(float4 scenePixelPos, float scenePixelDepth, float renderedAlpha){
	if(scenePixelDepth > _ShadowDrawDistance || renderedAlpha >= .99f)
		return 0.0f;
	// Init raymarching data
	// Start after clipping planes
	float3 initDir = _WorldSpaceLightPos0;
	float3 initPos = scenePixelPos;
	float3 samplePos = initPos;
	float3 offset = initDir * _StepSize;
	float distanceTravelled = 0.0f;

	float shadow = 0.0f;
	
	// PLACE SAMPLEPOS ON CLOUD BOUNDS
	if(samplePos.y > CloudBaseHeight(samplePos, initPos)+_CloudTransform.y)
	{
		float3 offsetOrthoToBounds = float3(samplePos.x, CloudBaseHeight(samplePos, initPos)+_CloudTransform.y, samplePos.z) - samplePos;
		bool oob = false;
		float3 offsetToBounds = IntersectionOnPlane(offsetOrthoToBounds, initDir, oob);
		if(oob)
			return 0.0f;

		float offsetLength = length(offsetToBounds);
		if(offsetLength > _ShadowDrawDistance)
			return 0.0f;

		samplePos += offsetToBounds;
	}
	else if(samplePos.y < CloudBaseHeight(samplePos, initPos)-_CloudTransform.y)
	{
		float3 offsetOrthoToBounds = float3(samplePos.x, CloudBaseHeight(samplePos, initPos)-_CloudTransform.y, samplePos.z) - samplePos;
		bool oob;
		float3 offsetToBounds = IntersectionOnPlane(offsetOrthoToBounds, initDir, oob);
		if(oob)
			return 0.0f;

		float offsetLength = length(offsetToBounds);
		if(offsetLength > _ShadowDrawDistance)
			return 0.0f;

		samplePos += offsetToBounds;
	}
		
	// RAYMARCH
	for(int i = 0; i < _MaxSteps; i++)
	{
		if(shadow >= 1.0f)
			break;

		float dist = distance(CloudBaseHeight(samplePos, initPos), samplePos.y)/_CloudTransform.y;
		float4 textureSample = SampleClouds(float2(samplePos.x, samplePos.z), 1.0f-saturate(dist), _LodBase+sqrt(sqrt(distanceTravelled) * _LodOffset));

		// If current pos is outside of cloud bounds quit loop
		if(samplePos.y > CloudBaseHeight(samplePos, initPos)+_CloudTransform.y+0.1f || samplePos.y < CloudBaseHeight(samplePos, initPos)-_CloudTransform.y-0.1f)
			break;

		// Is inside cloud ?
		if(textureSample.w > dist)
		{
			// Opacity based on position inside the cloud and ray passthrough
			float opacityGain = _OpacityGain * distance(textureSample.w, dist);
			shadow += opacityGain;
			samplePos += offset * (1.0f+sqrt(distanceTravelled)*_StepSkip) * _StepNearSurface * (1.0f-textureSample.w);
		}
		if(textureSample.w <= dist)
		{
			samplePos += offset * (1.0f+sqrt(distanceTravelled)*_StepSkip);
		}
		distanceTravelled = distance(samplePos, initPos);
		if(distanceTravelled > _ShadowDrawDistance)
			break;
	}
	shadow = saturate(shadow);	
	return float4(_ShadowColor.rgb, shadow*_ShadowColor.a);
}


inline float4 RaymarchClouds(FragmentCommonData s, float depth){
	// Init raymarching data
	// Start after clipping planes
	float3 initDir = s.eyeVec;
	float3 initPos = s.posWorld + initDir * _ProjectionParams.y;
	float3 samplePos = initPos;
	float3 offset = initDir * _StepSize;
	float distanceTravelled = 0.0f;

	float4 colors = 0.0f;

	// PLACE SAMPLEPOS ON CLOUD BOUNDS
	if(samplePos.y > CloudBaseHeight(samplePos, initPos)+_CloudTransform.y)
	{
		float3 offsetOrthoToBounds = float3(samplePos.x, CloudBaseHeight(samplePos, initPos)+_CloudTransform.y, samplePos.z) - samplePos;
		bool oob = false;
		float3 offsetToBounds = IntersectionOnPlane(offsetOrthoToBounds, initDir, oob);
		if(oob)
			return float4(0, 0, 0, 0);

		float offsetLength = length(offsetToBounds);
		if(offsetLength > _DrawDistance || offsetLength > depth)
			return float4(0, 0, 0, 0);

		samplePos += offsetToBounds;
	}
	else if(samplePos.y < CloudBaseHeight(samplePos, initPos)-_CloudTransform.y)
	{
		float3 offsetOrthoToBounds = float3(samplePos.x, CloudBaseHeight(samplePos, initPos)-_CloudTransform.y, samplePos.z) - samplePos;
		bool oob = false;
		float3 offsetToBounds = IntersectionOnPlane(offsetOrthoToBounds, initDir, oob);
		if(oob)
			return float4(0, 0, 0, 0);

		float offsetLength = length(offsetToBounds);
		if(offsetLength > _DrawDistance || offsetLength > depth)
			return float4(0, 0, 0, 0);

		samplePos += offsetToBounds;
	}
		
	// RAYMARCH
	for(int i = 0; i < _MaxSteps; i++)
	{
		if(colors.a >= 1.0f)
			break;

		float dist = distance(CloudBaseHeight(samplePos, initPos), samplePos.y)/_CloudTransform.y;
		float4 textureSample = SampleClouds(float2(samplePos.x, samplePos.z), 1.0f-saturate(dist), _LodBase+sqrt(sqrt(distanceTravelled) * _LodOffset));

		// If current pos is outside of cloud bounds quit loop
		if(samplePos.y > CloudBaseHeight(samplePos, initPos)+_CloudTransform.y+0.1f || samplePos.y < CloudBaseHeight(samplePos, initPos)-_CloudTransform.y-0.1f)
			break;

		// Is inside cloud ?
		if(textureSample.w > dist)
		{
			// Opacity based on position inside the cloud and ray passthrough
			float opacityGain = _OpacityGain * distance(textureSample.w, dist);
			colors.a += opacityGain;
			float heightDiff = -(CloudBaseHeight(samplePos, initPos)-samplePos.y)/_CloudTransform.y;
			float3 baseSampleColor =  lerp(1.0f, heightDiff * 0.5f + 0.5f, _DepthColor) * opacityGain;
		#if _NORMALMAP
			float3 normals = (float3(textureSample.x, -(CloudBaseHeight(samplePos, initPos)-samplePos.y)/_CloudTransform.y, textureSample.y));
			baseSampleColor*= NormalLighting(s, samplePos, normals);
		#endif
			colors.rgb += baseSampleColor;
			samplePos += offset * (1.0f+sqrt(distanceTravelled)*_StepSkip) * _StepNearSurface * (1.0f-textureSample.w);
		}
		if(textureSample.w <= dist)
		{
			samplePos += offset * (1.0f+sqrt(distanceTravelled)*_StepSkip);
		}
		distanceTravelled = distance(samplePos, initPos);
		if(distanceTravelled > _DrawDistance || distanceTravelled > depth)
			break;
	}
	colors.a = saturate(colors.a)*(1-saturate(pow(distanceTravelled*0.001f,1.0f/_DistanceBlend)));
	// Re-equilibrate transparent colors
	colors.rgb += (1.0f-colors.a)*colors.rgb;
	colors.rgb *= saturate(distanceTravelled);

	
	
	return colors;
}



// -- ORIGINAL BACKUP
half4 fragForwardBaseInternal (VertexOutputForwardBase i)
{
	FRAGMENT_SETUP(s)
	s.diffColor = _BaseColor;
#if UNITY_OPTIMIZE_TEXCUBELOD
	s.reflUVW		= i.reflUVW;
#endif

	// INIT scene depth and pos
	/*
	#if UNITY_UV_STARTS_AT_TOP
        float grabSign = -_ProjectionParams.x;
    #else
        float grabSign = _ProjectionParams.x;
    #endif*/
	// Projection params doesn't seem to follow UNITY_UV_STARTS_AT_TOP in some cases, this fixed it
	float grabSign = abs(_ProjectionParams.x);
	float4 screenPos = float4( i.screenPos.xy / i.screenPos.w, 0, 0 );
    screenPos.y *= _ProjectionParams.x;
    float2 sceneUVs = float2(1,grabSign)*screenPos.xy*0.5+0.5;
	float4 sceneDepthPos = WorldPosFromDepth(sceneUVs);
	float sceneDepth =  distance(sceneDepthPos, s.posWorld);

	float4 clouds = 0;
	float4 shadows = 0;
#if _SKIPPIXELONCE || _SKIPPIXELTWICE
	float2 pixelPosition = float2(_ScreenParams.x,_ScreenParams.y)*sceneUVs.xy;
	#if _SKIPPIXELONCE
		float grid = CreateGrid(pixelPosition, float2(.25f, .25f), 2);
	#else
		float grid = CreateGrid(pixelPosition, float2(.5f, .5f), 4);
	#endif
	if(grid != 0)
	{
		clouds = RaymarchClouds(s, sceneDepth);
	#if _RENDERSHADOWS
		shadows = RaymarchShadowClouds(sceneDepthPos, sceneDepth, clouds.a);
	#endif
	}
	if(grid == 0)
		clouds = fwidth(clouds) * 0.5f * (1.0-grid) + clouds;
#else
	clouds = RaymarchClouds(s, sceneDepth);
	#if _RENDERSHADOWS
		shadows = RaymarchShadowClouds(sceneDepthPos, sceneDepth, clouds.a);
	#endif
#endif
	clouds += float4(shadows.r*shadows.a, shadows.g*shadows.a, shadows.b*shadows.a, shadows.a)*(1-clouds.a);
	
	if(clouds.a <= _AlphaCut)
		discard;
	return clouds;//OutputForward (clouds,/*s.alpha*/clouds.a);
}



half4 fragForwardBase (VertexOutputForwardBase i) : SV_Target	// backward compatibility (this used to be the fragment entry function)
{
	return fragForwardBaseInternal(i);
}






















// ------------------------------------------------------------------
//  Additive forward pass (one light per pass)

struct VertexOutputForwardAdd
{
	float4 pos							: SV_POSITION;
	float4 tex							: TEXCOORD0;
	half3 eyeVec 						: TEXCOORD1;
	half4 tangentToWorldAndLightDir[3]	: TEXCOORD2;	// [3x3:tangentToWorld | 1x3:lightDir]
	LIGHTING_COORDS(5,6)
	UNITY_FOG_COORDS(7)

	// next ones would not fit into SM2.0 limits, but they are always for SM3.0+
#if defined(_PARALLAXMAP)
	half3 viewDirForParallax			: TEXCOORD8;
#endif
};

VertexOutputForwardAdd vertForwardAdd (VertexInput v)
{
	VertexOutputForwardAdd o;
	UNITY_INITIALIZE_OUTPUT(VertexOutputForwardAdd, o);

	float4 posWorld = mul(_Object2World, v.vertex);
	o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
	o.tex = TexCoords(v);
	o.eyeVec = NormalizePerVertexNormal(posWorld.xyz - _WorldSpaceCameraPos);
	float3 normalWorld = UnityObjectToWorldNormal(v.normal);
	#ifdef _TANGENT_TO_WORLD
		float4 tangentWorld = float4(UnityObjectToWorldDir(v.tangent.xyz), v.tangent.w);

		float3x3 tangentToWorld = CreateTangentToWorldPerVertex(normalWorld, tangentWorld.xyz, tangentWorld.w);
		o.tangentToWorldAndLightDir[0].xyz = tangentToWorld[0];
		o.tangentToWorldAndLightDir[1].xyz = tangentToWorld[1];
		o.tangentToWorldAndLightDir[2].xyz = tangentToWorld[2];
	#else
		o.tangentToWorldAndLightDir[0].xyz = 0;
		o.tangentToWorldAndLightDir[1].xyz = 0;
		o.tangentToWorldAndLightDir[2].xyz = normalWorld;
	#endif
	//We need this for shadow receiving
	TRANSFER_VERTEX_TO_FRAGMENT(o);

	float3 lightDir = _WorldSpaceLightPos0.xyz - posWorld.xyz * _WorldSpaceLightPos0.w;
	#ifndef USING_DIRECTIONAL_LIGHT
		lightDir = NormalizePerVertexNormal(lightDir);
	#endif
	o.tangentToWorldAndLightDir[0].w = lightDir.x;
	o.tangentToWorldAndLightDir[1].w = lightDir.y;
	o.tangentToWorldAndLightDir[2].w = lightDir.z;

	#ifdef _PARALLAXMAP
		TANGENT_SPACE_ROTATION;
		o.viewDirForParallax = mul (rotation, ObjSpaceViewDir(v.vertex));
	#endif
	
	UNITY_TRANSFER_FOG(o,o.pos);
	return o;
}

half4 fragForwardAddInternal (VertexOutputForwardAdd i)
{
	FRAGMENT_SETUP_FWDADD(s)

	UnityLight light = AdditiveLight (s.normalWorld, IN_LIGHTDIR_FWDADD(i), LIGHT_ATTENUATION(i));
	UnityIndirect noIndirect = ZeroIndirect ();

	half4 c = UNITY_BRDF_PBS (s.diffColor, s.specColor, s.oneMinusReflectivity, s.oneMinusRoughness, s.normalWorld, -s.eyeVec, light, noIndirect);
	
	UNITY_APPLY_FOG_COLOR(i.fogCoord, c.rgb, half4(0,0,0,0)); // fog towards black in additive pass
	return OutputForward (c, s.alpha);
}

half4 fragForwardAdd (VertexOutputForwardAdd i) : SV_Target		// backward compatibility (this used to be the fragment entry function)
{
	return fragForwardAddInternal(i);
}

// ------------------------------------------------------------------
//  Deferred pass

struct VertexOutputDeferred
{
	float4 pos							: SV_POSITION;
	float4 tex							: TEXCOORD0;
	half3 eyeVec 						: TEXCOORD1;
	half4 tangentToWorldAndParallax[3]	: TEXCOORD2;	// [3x3:tangentToWorld | 1x3:viewDirForParallax]
	half4 ambientOrLightmapUV			: TEXCOORD5;	// SH or Lightmap UVs			
	#if UNITY_SPECCUBE_BOX_PROJECTION
		float3 posWorld						: TEXCOORD6;
	#endif
	#if UNITY_OPTIMIZE_TEXCUBELOD
		#if UNITY_SPECCUBE_BOX_PROJECTION
			half3 reflUVW				: TEXCOORD7;
		#else
			half3 reflUVW				: TEXCOORD6;
		#endif
	#endif

};


VertexOutputDeferred vertDeferred (VertexInput v)
{
	VertexOutputDeferred o;
	UNITY_INITIALIZE_OUTPUT(VertexOutputDeferred, o);

	float4 posWorld = mul(_Object2World, v.vertex);
	#if UNITY_SPECCUBE_BOX_PROJECTION
		o.posWorld = posWorld;
	#endif
	o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
	o.tex = TexCoords(v);
	o.eyeVec = NormalizePerVertexNormal(posWorld.xyz - _WorldSpaceCameraPos);
	float3 normalWorld = UnityObjectToWorldNormal(v.normal);
	#ifdef _TANGENT_TO_WORLD
		float4 tangentWorld = float4(UnityObjectToWorldDir(v.tangent.xyz), v.tangent.w);

		float3x3 tangentToWorld = CreateTangentToWorldPerVertex(normalWorld, tangentWorld.xyz, tangentWorld.w);
		o.tangentToWorldAndParallax[0].xyz = tangentToWorld[0];
		o.tangentToWorldAndParallax[1].xyz = tangentToWorld[1];
		o.tangentToWorldAndParallax[2].xyz = tangentToWorld[2];
	#else
		o.tangentToWorldAndParallax[0].xyz = 0;
		o.tangentToWorldAndParallax[1].xyz = 0;
		o.tangentToWorldAndParallax[2].xyz = normalWorld;
	#endif

	o.ambientOrLightmapUV = 0;
	#ifndef LIGHTMAP_OFF
		o.ambientOrLightmapUV.xy = v.uv1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
	#elif UNITY_SHOULD_SAMPLE_SH
		o.ambientOrLightmapUV.rgb = ShadeSHPerVertex (normalWorld, o.ambientOrLightmapUV.rgb);
	#endif
	#ifdef DYNAMICLIGHTMAP_ON
		o.ambientOrLightmapUV.zw = v.uv2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
	#endif
	
	#ifdef _PARALLAXMAP
		TANGENT_SPACE_ROTATION;
		half3 viewDirForParallax = mul (rotation, ObjSpaceViewDir(v.vertex));
		o.tangentToWorldAndParallax[0].w = viewDirForParallax.x;
		o.tangentToWorldAndParallax[1].w = viewDirForParallax.y;
		o.tangentToWorldAndParallax[2].w = viewDirForParallax.z;
	#endif

	#if UNITY_OPTIMIZE_TEXCUBELOD
		o.reflUVW		= reflect(o.eyeVec, normalWorld);
	#endif

	return o;
}

void fragDeferred (
	VertexOutputDeferred i,
	out half4 outDiffuse : SV_Target0,			// RT0: diffuse color (rgb), occlusion (a)
	out half4 outSpecSmoothness : SV_Target1,	// RT1: spec color (rgb), smoothness (a)
	out half4 outNormal : SV_Target2,			// RT2: normal (rgb), --unused, very low precision-- (a) 
	out half4 outEmission : SV_Target3			// RT3: emission (rgb), --unused-- (a)
)
{
	#if (SHADER_TARGET < 30)
		outDiffuse = 1;
		outSpecSmoothness = 1;
		outNormal = 0;
		outEmission = 0;
		return;
	#endif

	FRAGMENT_SETUP(s)
#if UNITY_OPTIMIZE_TEXCUBELOD
	s.reflUVW		= i.reflUVW;
#endif

	// no analytic lights in this pass
	UnityLight dummyLight = DummyLight (s.normalWorld);
	half atten = 1;

	// only GI
	half occlusion = Occlusion(i.tex.xy);
#if UNITY_ENABLE_REFLECTION_BUFFERS
	bool sampleReflectionsInDeferred = false;
#else
	bool sampleReflectionsInDeferred = true;
#endif

	UnityGI gi = FragmentGI (s, occlusion, i.ambientOrLightmapUV, atten, dummyLight, sampleReflectionsInDeferred);

	half3 color = UNITY_BRDF_PBS (s.diffColor, s.specColor, s.oneMinusReflectivity, s.oneMinusRoughness, s.normalWorld, -s.eyeVec, gi.light, gi.indirect).rgb;
	color += UNITY_BRDF_GI (s.diffColor, s.specColor, s.oneMinusReflectivity, s.oneMinusRoughness, s.normalWorld, -s.eyeVec, occlusion, gi);

	#ifdef _EMISSION
		color += Emission (i.tex.xy);
	#endif

	#ifndef UNITY_HDR_ON
		color.rgb = exp2(-color.rgb);
	#endif

	outDiffuse = half4(s.diffColor, occlusion);
	outSpecSmoothness = half4(s.specColor, s.oneMinusRoughness);
	outNormal = half4(s.normalWorld*0.5+0.5,1);
	outEmission = half4(color, 1);
}


//
// Old FragmentGI signature. Kept only for backward compatibility and will be removed soon
//

inline UnityGI FragmentGI(
	float3 posWorld,
	half occlusion, half4 i_ambientOrLightmapUV, half atten, half oneMinusRoughness, half3 normalWorld, half3 eyeVec,
	UnityLight light,
	bool reflections)
{
	// we init only fields actually used
	FragmentCommonData s = (FragmentCommonData)0;
	s.oneMinusRoughness = oneMinusRoughness;
	s.normalWorld = normalWorld;
	s.eyeVec = eyeVec;
	s.posWorld = posWorld;
#if UNITY_OPTIMIZE_TEXCUBELOD
	s.reflUVW = reflect(eyeVec, normalWorld);
#endif
	return FragmentGI(s, occlusion, i_ambientOrLightmapUV, atten, light, reflections);
}
inline UnityGI FragmentGI (
	float3 posWorld,
	half occlusion, half4 i_ambientOrLightmapUV, half atten, half oneMinusRoughness, half3 normalWorld, half3 eyeVec,
	UnityLight light)
{
	return FragmentGI (posWorld, occlusion, i_ambientOrLightmapUV, atten, oneMinusRoughness, normalWorld, eyeVec, light, true);
}

#endif // UNITY_STANDARD_CORE_INCLUDED
