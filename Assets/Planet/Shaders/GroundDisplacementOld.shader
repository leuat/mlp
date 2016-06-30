// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'


Shader "LemonSpawn/GroundDisplacementOld"
{
	Properties
	{

		_Color("Color", Color) = (1,1,1,1)

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
			ZWrite On


			CGPROGRAM
			#pragma target 3.0
		// TEMPORARY: GLES2.0 temporarily disabled to prevent errors spam on devices without textureCubeLodEXT


		#pragma vertex LvertForwardBase
		#pragma fragment LfragForwardBase
#include "UnityCG.cginc"
#pragma multi_compile_fwdbase
#include "AutoLight.cginc"
		#include "Include/Atmosphere.cginc"
		#include "Include/PlanetSurface.cginc"


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
	
		float3 posWorld					: TEXCOORD11;
	
		float3 posWorld2 				: TEXCOORD12;
		float3 tangent : TEXCOORD13;
	};


			sampler2D _Mountain, _Basin, _Top, _Surface;



			VertexOutputForwardBase2 LvertForwardBase(appdata_full v)
			{
				VertexOutputForwardBase2 o;

				float4 ov = v.vertex;
				float3 normalWorld = normalize(mul(_Object2World, v.vertex).xyz - v3Translate);
				//float3 normalWorld = getPlanetSurfaceNormal(v.vertex);
				float4 groundVertex = getPlanetSurfaceOnly(v.vertex);
				v.vertex = groundVertex;
				//				UNITY_INITIALIZE_OUTPUT(VertexOutputForwardBase2, o);



				float4 capV = groundVertex;
				float4 posWorld = mul(_Object2World, ov);
				o.posWorld2 = groundVertex;

				float wh = (length(o.posWorld.xyz - v3Translate) - fInnerRadius);
				o.pos = mul(UNITY_MATRIX_MVP, capV);
				o.tex = v.texcoord;

				o.n1 = normalWorld;
				float3 t = v.tangent.xyz;
				float3 b = normalize(cross(normalWorld, t));
				o.tangent = v.tangent.xyz;
				

				float scale = surfaceNoiseSettings2.z;
				float heightScale = surfaceNoiseSettings2.y;



							
				normalWorld = getPlanetSurfaceNormal(posWorld - v3Translate,t, b,10.1);
								o.n1 = normalWorld;




																				TRANSFER_SHADOW(o);


																						getGroundAtmosphere(groundVertex, o.c0, o.c1);


																						return o;
																					}


																					uniform float hillyThreshold;


																						inline float3 getTex(sampler2D t, in float2 uv) {
																							//								return float3(1,1,1);
																															float3 c = tex2D(t, uv)*0.25;
																															//								c += tex2D(t, 0.5323*uv);
																																							c += tex2D(t, 0.2213*uv)*0.75;

																																							c /= 1;
																																						return c;
																				}

								half4 LfragForwardBase(VertexOutputForwardBase2 i) : SV_Target
							{
							//return float4(i.n1.x, i.n1.y, i.n1.z,1);
							half atten = SHADOW_ATTENUATION(i);


																																						float dd = dot(normalize(i.posWorld2.xyz), normalize(i.n1));

																																						float tt = clamp(noise(normalize(i.posWorld2.xyz)*3.1032) + 0.2,0,1);
																																						float3 mColor = ((1 - tt)*middleColor + middleColor2*tt);
																																						//	float3 bColor = ((1-tt)*basinColor + basinColor2*tt*r_noise(normalize(i.vpos.xyz),2.1032,3));

																																							float3 hColor = mColor*getTex(_Surface, i.tex.xy);//float3(1,1,1);//s.diffColor;
																																							//	float3 hillColor = s.diffColor;
																																								//if (dd < 0.98 )
																																								//	hColor = float3(0.2, 0.2 ,0.2);
																																								float3 v3CameraPos = _WorldSpaceCameraPos - v3Translate;	// The camera's current position


																																								float fCameraHeight = length(v3CameraPos);
																																								float camH = clamp(fCameraHeight - fInnerRadius, 0, 1);
																																								float h = (length(i.posWorld.xyz - v3Translate) - fInnerRadius) / fInnerRadius;// - liquidThreshold;
																																								float wh = (length(i.posWorld.xyz - v3Translate) - fInnerRadius);

																																								//									float modulatedHillyThreshold = hillyThreshold* atan2(i.posWorld.z , i.posWorld.y);
																																																	float3 ppos = normalize(i.posWorld2.xyz);
																																																	//									float modulatedHillyThreshold = atan2(ppos.z, ppos.y);
																																																										float posY = (clamp(2 * abs(asin(ppos.y) / 3.14159), 0, 1));
																																																										float modulatedTopThreshold = topThreshold*(1 - posY*1.1);
																																																										float modulatedHillyThreshold = hillyThreshold;// clamp(hillyThreshold - 1 * posY, 0, 1);


																																																										hColor = mixHeight(hColor, basinColor*getTex(_Basin, i.tex.xy), 500, basinThreshold	, h);

																																																										hColor = mixHeight(hColor, basinColor2*getTex(_Basin, i.tex.xy), 3000, liquidThreshold, h);
																																																										hColor = mixHeight(topColor*getTex(_Top, i.tex.xy), hColor, 1000, modulatedTopThreshold, h);
																																																										hColor = mixHeight(hColor, hillColor*getTex(_Mountain, i.tex.xy), 250, modulatedHillyThreshold, dd);
																																																										//									hColor = mixHeight(topColor, hColor, 4000, topThreshold, h);




																																																										//	float3 diff = hColor*(i.c0*3 + i.c1)*1;//0.35*(hColor*1 + 1.25*cc + hColor*cc);
																																																											//float3 diff = hColor*dot(v3LightPos, i.n1);
																																																										float3 diff = hColor*clamp(dot(v3LightPos, i.n1), 0, 1);
																																																											//float3 diff = 0.35*(hColor*0 + 1.00*i.c0 + i.c1);
																																																											float d = 0.05;
																																																											//	diff -=float3(d,d,d);









																																																											//float4 spc =_Color;// float4(1, 1, 1, 1);// *specularity * 1;
																																																											float4 spc = float4(1,1,1,1)*0.25;// float4(1, 1, 1, 1);// *metallicity;// *specularity * 1;

																																																												//	diff = groundColor(i.c0, i.c1, diff);
																																																											float4 c = 0;
																																																											c.rgb = diff;

//																																																													float groundClouds = getGroundShadowFromClouds(ppos);
																																																													float groundClouds = 1;
																																																													//c.rgb = i.tangent;

																																																													c.rgb = groundColor(i.c0, i.c1, c.rgb, i.posWorld, 1.0)*groundClouds;

																																																													//											c.rgb = modulatedHillyThreshold;
																																																												//												c.rgb = float3(1,0,0)*modd;

																																																												//return float4(ppos.xyz,1);
																																																													c.a = 1;
																																																													return c;
																																																																							}


				ENDCG
														}
	}
		Fallback  "VertexLit"
}