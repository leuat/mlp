Shader "LemonSpawn/Ground"
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Albedo", 2D) = "white" {}
		_Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5

		_Glossiness("Smoothness", Range(0.0, 1.0)) = 0.5
		[Gamma] _Metallic("Metallic", Range(0.0, 1.0)) = 0.0
		_MetallicGlossMap("Metallic", 2D) = "white" {}

		_BumpScale("Scale", Float) = 1.0
		_BumpMap("Normal Map", 2D) = "bump" {}

		_Parallax("Height Scale", Range(0.005, 0.08)) = 0.02
		_ParallaxMap("Height Map", 2D) = "black" {}


		_OcclusionStrength("Strength", Range(0.0, 1.0)) = 1.0
		_OcclusionMap("Occlusion", 2D) = "white" {}

		_EmissionColor("Color", Color) = (0,0,0)
		_EmissionMap("Emission", 2D) = "white" {}

		_DetailMask("Detail Mask", 2D) = "white" {}

		_DetailAlbedoMap("Detail Albedo x2", 2D) = "grey" {}
		_DetailNormalMapScale("Scale", Float) = 1.0
		_DetailNormalMap("Normal Map", 2D) = "bump" {}

		[Enum(UV0,0,UV1,1)] _UVSec("UV Set for secondary textures", Float) = 0

			// UI-only data
			[HideInInspector] _EmissionScaleUI("Scale", Float) = 0.0
			[HideInInspector] _EmissionColorUI("Color", Color) = (1,1,1)

			// Blending state
			[HideInInspector] _Mode("__mode", Float) = 0.0
			[HideInInspector] _SrcBlend("__src", Float) = 1.0
			[HideInInspector] _DstBlend("__dst", Float) = 0.0
			[HideInInspector] _ZWrite("__zw", Float) = 1.0
	}

		CGINCLUDE
#define UNITY_SETUP_BRDF_INPUT MetallicSetup
			ENDCG



			SubShader
		{
			Tags { "RenderType" = "Opaque" "PerformanceChecks" = "False" }
			LOD 150


			// ------------------------------------------------------------------
			//  Base forward pass (directional light, emission, lightmaps, ...)
			Pass
			{
				Name "FORWARD"
				Tags { "LightMode" = "ForwardBase" }
				Blend[_SrcBlend][_DstBlend]
				ZWrite[_ZWrite]



				CGPROGRAM
				#pragma target 3.0
			// TEMPORARY: GLES2.0 temporarily disabled to prevent errors spam on devices without textureCubeLodEXT
			#pragma exclude_renderers gles

			// -------------------------------------

			#pragma shader_feature _NORMALMAP
			#pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
			#pragma shader_feature _EMISSION
			#pragma shader_feature _METALLICGLOSSMAP 
			#pragma shader_feature ___ _DETAIL_MULX2
			#pragma shader_feature _PARALLAXMAP
			//#pragma skip_variants SHADOWS_SOFT DIRLIGHTMAP_COMBINED DIRLIGHTMAP_SEPARATE

			#pragma multi_compile_fwdbase
			#pragma multi_compile_fog

			#pragma vertex LvertForwardBase
			#pragma fragment LfragForwardBase

			#include "UnityStandardCore.cginc"
			#include "Include/IQnoise.cginc"
			#include "Include/Atmosphere.cginc"


struct VertexOutputForwardBase2
{
	float4 pos							: SV_POSITION;
	float4 tex							: TEXCOORD0;
	half3 eyeVec 						: TEXCOORD1;
	half4 tangentToWorldAndParallax[3]	: TEXCOORD2;	// [3x3:tangentToWorld | 1x3:viewDirForParallax]
	half4 ambientOrLightmapUV			: TEXCOORD5;	// SH or Lightmap UV
	SHADOW_COORDS(6)
	UNITY_FOG_COORDS(7)
	float3 c0 : TEXCOORD8;
	float3 c1 : TEXCOORD9;
	float3 n1 : TEXCOORD10;
//	float4 vpos  : TEXCOORD11;
	// next ones would not fit into SM2.0 limits, but they are always for SM3.0+
#if UNITY_SPECCUBE_BOX_PROJECTION
	float3 posWorld					: TEXCOORD11;
	#endif
};


VertexOutputForwardBase vertForwardBaseORG(VertexInput v)
{
	VertexOutputForwardBase o;
	UNITY_INITIALIZE_OUTPUT(VertexOutputForwardBase, o);

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
			half3 viewDirForParallax = mul(rotation, ObjSpaceViewDir(v.vertex));
			o.tangentToWorldAndParallax[0].w = viewDirForParallax.x;
			o.tangentToWorldAndParallax[1].w = viewDirForParallax.y;
			o.tangentToWorldAndParallax[2].w = viewDirForParallax.z;
		#endif

		#if UNITY_OPTIMIZE_TEXCUBELOD
			o.reflUVW = reflect(o.eyeVec, normalWorld);
		#endif

		UNITY_TRANSFER_FOG(o,o.pos);
		return o;
	}


	float calculateWave(float2 pos, float t) {
		//	return cos(time + pos.x);
			int N = 8;
			float v = 0;
			float w = 0.5;
			float theta_i = 1;
			for (int i = 1; i < N; i++) {
				float A = 1.0 / i;
				float k_i = i;
				float kernel = pos.x*cos(theta_i) + pos.y*sin(theta_i) - w*t;
				v += A*cos(k_i*kernel);
			}
			return v;

		}

		float Gerstner(float2 Position, float time, out float3 N) {
			float h = 0;
			h += 4 * (sin(-0.05*Position.x + 0.5*time)*0.5 + 0.5);
		   float first = -0.100*cos(-0.05*Position.x + 0.5*time);

		   h += 2 * (sin((-0.07*Position.x + 0.07*Position.y) + time*1.3)*0.5 + 0.5);
		   float second1 = -0.070*cos(-0.07*Position.x + 0.07*Position.y + 1.3*time);
		   float second2 = 0.070*cos(-0.07*Position.x + 0.07*Position.y + 1.3*time);

			float3 prem = float3(-first,1,-first);
		   float3 sec = normalize(float3(-second1,1,-second2));
		   N = normalize((sec + prem)* float3(1,0.5,1));

			return h;
		}


		/*float3 Gerstner2(float3 P, float DeltaT, out float3 N) {

			float A = 20.0;	// amplitude
			float L = 50;	// wavelength
			float w = 2*3.1416/L;
			float Q = 0.5;

			float3 P0 = P;
			float2 D = normalize(float2(0.5, 5));
			float dotD = dot(P0.xz, D);
			float C = cos(w*dotD + DeltaT/1.0);
			float S = sin(w*dotD + DeltaT/1.0);

			float3 val = float3( Q*A*C*D.x, A * S,  Q*A*C*D.y);


			return val;

		//	output.position = mul(WorldViewProj, float4(P,1));

		}
		*/

		VertexOutputForwardBase2 LvertForwardBase(VertexInput v)
		{
			VertexOutputForwardBase2 o;
			UNITY_INITIALIZE_OUTPUT(VertexOutputForwardBase2, o);

			float4 capV = v.vertex;

			float4 posWorld = mul(_Object2World, capV);
			#if UNITY_SPECCUBE_BOX_PROJECTION
				o.posWorld = posWorld.xyz;
			#endif

			float wh = (length(o.posWorld.xyz - v3Translate) - fInnerRadius);


			o.pos = mul(UNITY_MATRIX_MVP, capV);


//			o.vpos = capV;

			o.tex = TexCoords(v);
			o.eyeVec = NormalizePerVertexNormal(posWorld.xyz - _WorldSpaceCameraPos);
			float3 normalWorld = UnityObjectToWorldNormal(v.normal);
			/*	if (wh<liquidThreshold) {


			//		float h = Gerstner(o.tex.xy*100.0, time*1.0, normalWorld)*0.4;
					//Gerstner2(normalize(capV.xyz)*100000.0, time*1.0, normalWorld);
					float3 P = 0.2*Gerstner2(normalize(capV.xyz)*1000000.0, time*1.0, normalWorld);
			//		capV.xyz = normalize(capV.xyz)*(fInnerRadius + liquidThreshold + h);
					capV.xyz = normalize(capV.xyz)*(fInnerRadius + liquidThreshold) +P;
					normalWorld = normalize(normalize(capV)+normalWorld);

					posWorld = mul(_Object2World, capV);
					o.pos = mul(UNITY_MATRIX_MVP, capV);

					normalWorld = normalize(capV.xyz);
				}
				*/
				o.n1 = normalWorld;
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

					// Static lightmaps
					#ifndef LIGHTMAP_OFF
						o.ambientOrLightmapUV.xy = v.uv1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
						o.ambientOrLightmapUV.zw = 0;
						// Sample light probe for Dynamic objects only (no static or dynamic lightmaps)
						#elif UNITY_SHOULD_SAMPLE_SH
							#if UNITY_SAMPLE_FULL_SH_PER_PIXEL
								o.ambientOrLightmapUV.rgb = 0;
							#elif (SHADER_TARGET < 30)
								o.ambientOrLightmapUV.rgb = ShadeSH9(half4(normalWorld, 1.0));
							#else
								// Optimization: L2 per-vertex, L0..L1 per-pixel
								o.ambientOrLightmapUV.rgb = ShadeSH3Order(half4(normalWorld, 1.0));
							#endif
								// Add approximated illumination from non-important point lights
								#ifdef VERTEXLIGHT_ON
									o.ambientOrLightmapUV.rgb += Shade4PointLights(
										unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
										unity_LightColor[0].rgb, unity_LightColor[1].rgb, unity_LightColor[2].rgb, unity_LightColor[3].rgb,
										unity_4LightAtten0, posWorld, normalWorld);
								#endif
							#endif

							#ifdef DYNAMICLIGHTMAP_ON
								o.ambientOrLightmapUV.zw = v.uv2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
							#endif

							#ifdef _PARALLAXMAP
								TANGENT_SPACE_ROTATION;
								half3 viewDirForParallax = mul(rotation, ObjSpaceViewDir(v.vertex));
								o.tangentToWorldAndParallax[0].w = viewDirForParallax.x;
								o.tangentToWorldAndParallax[1].w = viewDirForParallax.y;
								o.tangentToWorldAndParallax[2].w = viewDirForParallax.z;
							#endif

							UNITY_TRANSFER_FOG(o,o.pos);

							getGroundAtmosphere(v.vertex, o.c0, o.c1);

							return o;
						}


						uniform float hillyThreshold;




						half4 fragForwardBaseORG(VertexOutputForwardBase i) : SV_Target
						{
							FRAGMENT_SETUP(s)
						#if UNITY_OPTIMIZE_TEXCUBELOD
							s.reflUVW = i.reflUVW;
						#endif

							UnityLight mainLight = MainLight(s.normalWorld);
							half atten = SHADOW_ATTENUATION(i);


							half occlusion = Occlusion(i.tex.xy);
							UnityGI gi = FragmentGI(s, occlusion, i.ambientOrLightmapUV, atten, mainLight);

							half4 c = UNITY_BRDF_PBS(s.diffColor, s.specColor, s.oneMinusReflectivity, s.oneMinusRoughness, s.normalWorld, -s.eyeVec, gi.light, gi.indirect);
							c.rgb += UNITY_BRDF_GI(s.diffColor, s.specColor, s.oneMinusReflectivity, s.oneMinusRoughness, s.normalWorld, -s.eyeVec, occlusion, gi);
							c.rgb += Emission(i.tex.xy);

							UNITY_APPLY_FOG(i.fogCoord, c.rgb);
							return OutputForward(c, s.alpha);
						}


						half4 LfragForwardBase(VertexOutputForwardBase2 i) : SV_Target
						{
							FRAGMENT_SETUP(s)
							UnityLight mainLight = MainLight(s.normalWorld);
							half atten = SHADOW_ATTENUATION(i);

							half occlusion = Occlusion(i.tex.xy);
							UnityGI gi = FragmentGI(
								s.posWorld, occlusion, i.ambientOrLightmapUV, atten, s.oneMinusRoughness, s.normalWorld, s.eyeVec, mainLight);


							float dd = dot(normalize(i.posWorld.xyz - v3Translate), normalize(s.normalWorld * 1 + i.n1 * 0));

							float tt = 0.1;//r_noise(normalize(i.vpos.xyz), 3.1032, 3);
							float3 mColor = ((1 - tt)*middleColor + middleColor2*tt);
							//	float3 bColor = ((1-tt)*basinColor + basinColor2*tt*r_noise(normalize(i.vpos.xyz),2.1032,3));

								float3 hColor = mColor;//float3(1,1,1);//s.diffColor;
								float3 hillColor = float3(0.2, 0.2, 0.2)*0.5;
								//	float3 hillColor = s.diffColor;
									//if (dd < 0.98 )
									//	hColor = float3(0.2, 0.2 ,0.2);
									float3 v3CameraPos = _WorldSpaceCameraPos - v3Translate;	// The camera's current position

									float fCameraHeight = length(v3CameraPos);
									float camH = clamp(fCameraHeight - fInnerRadius, 0, 1);
									float h = (length(i.posWorld.xyz - v3Translate) - fInnerRadius) / fInnerRadius;// - liquidThreshold;
									float wh = (length(i.posWorld.xyz - v3Translate) - fInnerRadius);


									hColor = mixHeight(topColor, hColor, 1000, 0.0035	, h);
									hColor = mixHeight(hColor, basinColor, 500, 0.001	, h);
									hColor = mixHeight(hColor, hillColor, 100, hillyThreshold, dd);
									hColor = mixHeight(hColor, basinColor2, 1000, liquidThreshold, h);




									//	float3 diff = hColor*(i.c0*3 + i.c1)*1;//0.35*(hColor*1 + 1.25*cc + hColor*cc);
										float3 diff = hColor;
										//float3 diff = 0.35*(hColor*0 + 1.00*i.c0 + i.c1);
										float d = 0.05;
										//	diff -=float3(d,d,d);

											float4 spc = _Color;// float4(1, 1, 1, 1);// *specularity * 1;
											float omr = s.oneMinusReflectivity;
											float omr2 = s.oneMinusRoughness;

											//	diff = groundColor(i.c0, i.c1, diff);

												half4 c = UNITY_BRDF_PBS(diff, spc, omr, omr2, s.normalWorld, -s.eyeVec, gi.light, gi.indirect);


												c.rgb += UNITY_BRDF_GI(diff, spc, omr, omr2, s.normalWorld, -s.eyeVec, occlusion, gi);
												c.rgb += Emission(i.tex.xy);

												c.rgb = groundColor(i.c0, i.c1, c.rgb);

												return OutputForward(c, s.alpha);
											}

											ENDCG
													}
			// ------------------------------------------------------------------

			Pass
			{
				Name "FORWARD_DELTA"
				Tags { "LightMode" = "ForwardAdd" }
				Blend[_SrcBlend] One
				Fog { Color(0,0,0,0) } // in additive pass fog should be black
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

												#pragma vertex vertForwardAdd
												#pragma fragment fragForwardAdd

												#include "UnityStandardCore.cginc"

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
													Tags { "LightMode" = "Meta" }

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
		}

			SubShader
											{
												Tags { "RenderType" = "Opaque" "PerformanceChecks" = "False" }
												LOD 150

												// ------------------------------------------------------------------
												//  Base forward pass (directional light, emission, lightmaps, ...)
												Pass
												{
													Name "FORWARD"
													Tags { "LightMode" = "ForwardBase" }

													Blend[_SrcBlend][_DstBlend]
													ZWrite[_ZWrite]

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

												#pragma vertex vertForwardBase
												#pragma fragment fragForwardBase

												#include "UnityStandardCore.cginc"

												ENDCG
											}
												// ------------------------------------------------------------------
												//  Additive forward pass (one light per pass)
												Pass
												{
													Name "FORWARD_DELTA"
													Tags { "LightMode" = "ForwardAdd" }
													Blend[_SrcBlend] One
													Fog { Color(0,0,0,0) } // in additive pass fog should be black
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

												#pragma vertex vertForwardAdd
												#pragma fragment fragForwardAdd

												#include "UnityStandardCore.cginc"

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
													Tags { "LightMode" = "Meta" }

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


											}


												//	FallBack "VertexLit"
												CustomEditor "StandardShaderGUI"
}

