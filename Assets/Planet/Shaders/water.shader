// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "LemonSpawn/Water" {

	Properties{
		_Normal("Albedo", 2D) = "bump" {}
		_Scale("NormalScale", Float) = 100
		_Perlin("Distortion", 2D) = "white" {}
		_SunPow("SunPow", float) = 256
	}


		SubShader{
//			Tags{ "LightMode" = "ForwardBase" }
	//		Tags{ "Queue" = "Transparent+11000" "RenderType" = "Transparent" }
			LOD 400

			Lighting on
			Cull back
			ZWrite on
			ZTest on
		//	Blend SrcAlpha OneMinusSrcAlpha
			Pass
		{

			Tags{ "LightMode" = "ForwardBase" }
			CGPROGRAM
			// Upgrade NOTE: excluded shader from DX11 and Xbox360; has structs without semantics (struct v2f members worldPosition)

	#pragma target 3.0
//#pragma fragmentoption ARB_precision_hint_fastest

//	#pragma enable_d3d11_debug_symbols


	#pragma vertex vert
	#pragma fragment frag

	#include "UnityCG.cginc"
#pragma multi_compile_fwdbase
#include "AutoLight.cginc"
	#include "Include/Utility.cginc"
	#include "Include/Atmosphere.cginc"



		uniform sampler2D _FresnelLookUp, _Map0, _Map1, _Map2, _Main;
		uniform float4 _GridSizes;
		uniform float3 _SunColor, _SunDir;
		uniform float _MaxLod, _LodFadeDist;

		float _SunPow;
		float3 _SeaColor;
		samplerCUBE _SkyBox;


		struct v2f
		{
			//float4 vpos : SV_POSITION;
			float4 pos : SV_POSITION;
			float4 texcoord : TEXCOORD2;
			float3 normal : TEXCOORD3;
//			float4 uv : TEXCOORD2;
			float3 worldPosition : TEXCOORD4;
			float3 c0 : TEXCOORD5;
			float3 c1 : TEXCOORD6;
			float3 T: TEXCOORD7;
			float3 B: TEXCOORD8;
			LIGHTING_COORDS(1, 2)
			float3 vvertex: TEXCOORD9;
		};


								float Fresnel(float3 V, float3 N)
								{
									float costhetai = abs(dot(V, N));
									return tex2D(_FresnelLookUp, float2(costhetai, 0.0)).a * 0.7; //looks better scaled down a little?
								}

								float3 Sun(float3 V, float3 N)
								{
									float3 H = normalize(V + _SunDir);
									return _SunColor * pow(abs(dot(H, N)), _SunPow);
								}


								v2f vert(appdata_full v)
								{
									v2f o;
									
									 float4x4 modelMatrix = _Object2World;
									float4x4 modelMatrixInverse = _World2Object;

									float4 newVertex = mul(_Object2World, v.vertex);
									newVertex.xyz -= v3Translate;
									newVertex.xyz = normalize(newVertex.xyz)*fInnerRadius*(1 + liquidThreshold) + v3Translate;
									v.vertex = mul(_World2Object, newVertex);

									o.worldPosition = newVertex.xyz;

									o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
									TRANSFER_VERTEX_TO_FRAGMENT(o);
								//	o.uv = v.texcoord;
									o.texcoord = v.texcoord;
//									o.worldPosition = mul(modelMatrix, v.vertex);
									o.vvertex = v.vertex;

									getGroundAtmosphere(v.vertex, o.c0, o.c1);


									o.T = normalize(
										mul(modelMatrix, float4(v.tangent.xyz, 0.0)).xyz);
									o.normal = normalize(
										mul(float4(v.normal, 0.0), modelMatrixInverse).xyz);
									o.B = normalize(
										cross(o.normal, o.T)
										* 1); // tangent.w is specific to Unity

									float2 uv = RadialCoords(normalize(v.vertex.xyz)) * 200/(fOuterRadius/50000.0);
									//uv += RadialCoords(normalize(v.vertex.xyz)) * 1223;
									o.texcoord.xy = uv;

									float dist = clamp(distance(_WorldSpaceCameraPos.xyz, o.pos) / _LodFadeDist*0.01, 0.0, 1.0);
									float lod = _MaxLod * dist;
									//lod = 0;

									float ht = 0.0;
									float wscale = 0.5;
									ht += tex2Dlod(_Map0, float4(uv*wscale, 0, lod)*1).x;
									ht += tex2Dlod(_Map0, float4(uv*wscale*0.9123, 0, lod)*1).y;
//									o.pos = mul(UNITY_MATRIX_MVP, float4(v.vertex.xyz + o.normal*(ht*15.2*(fInnerRadius/50000)), v.vertex.w));
//									o.pos = mul(UNITY_MATRIX_MVP, float4(v.vertex.xyz + o.normal*(ht*7.2), v.vertex.w));
									
								
											return o;
										}
										
						
									
										fixed4 frag(v2f IN) : COLOR{
									float attenuation = clamp(LIGHT_ATTENUATION(IN),0.4,1);

											float4 c;
										float3 specularReflection;

											float3 lightDirection =
												normalize(_WorldSpaceLightPos0.xyz);

											float3 viewDirection = normalize(
												_WorldSpaceCameraPos - IN.worldPosition.xyz);


											float3 w = normalize(IN.normal);

											//		float2 uv = RadialCoords(w) * 2500;
													float2 uv = IN.texcoord.xy * 50;
													//			float2 uv = float2(0.5 + atan2(w.z, w.x)/(2*3.14159), 0.5 - asin(w.y)/3.14159);
													//uv = float2(w.x, w.y);			//uv = IN.uv_MainTex*100;
													//			float2 uv = IN.



													float2 slope = float2(0, 0);

													slope += tex2D(_Map1, uv).xy;
													slope += tex2D(_Map1, uv).zw;
													slope += tex2D(_Map2, uv).xy;
													slope += tex2D(_Map2, uv).zw;

													slope += tex2D(_Map2, 0.212*uv).xy;
													slope += tex2D(_Map2, 0.212*uv).zw;

													slope += tex2D(_Map1, 0.143*uv).xy;
													slope += tex2D(_Map1, 0.143*uv).zw;
													slope -= tex2D(_Map2, 2.1121*uv).xy;
													slope -= tex2D(_Map1, 1.612*uv).zw;
													float3 N1 = normalize(float3(-slope.x, 2.0, -slope.y)); //shallow normal
													float3 N2 = normalize(float3(-slope.x, 1, -slope.y)); //sharp normal

													float3 V = normalize(_WorldSpaceCameraPos - IN.worldPosition);


													float3 normal = IN.normal;// N.xzy;
													//N1 = float3(0.5, 1, 0);
													//float light = max(0, dot(normal, lightDirection));


													float3x3 local2WorldTranspose = float3x3(
														IN.T,
														IN.B,
														IN.normal);
													//N1 = float3(0, 1, 0);
													float3 N = (N1);
													//												N = float3(0, 0, 1);
							
													float3 nP =

														normalize(mul(N.xzy, local2WorldTranspose));

													N2 =

														normalize(mul(N2.xzy, local2WorldTranspose));

													//													nP = mul(N1.xzy, local2WorldTranspose);

													float3 normalDirection = normalize(normal*1 + nP);
													//normalDirection = normalize(IN.normal + normalDirectopm);
													float fresnel = Fresnel(V, normalDirection);

													specularReflection = float3(1, 1, 1)*0.6
														* pow(max(0.0, dot(

															reflect(-lightDirection, normalDirection),
															viewDirection)), 50);

													float light = max(0.18*0, dot(normalDirection, lightDirection));
													//			float3 skyColor = texCUBE(_SkyBox, WorldReflectionVector(IN, o.Normal)*float3(-1,1,1)).rgb;//flip x
													float3 skyColor = (3 * IN.c0 + 0.2*IN.c1) * 3;// float3(2, 0.7, 0.4) * 1;

													float3 wc = lerp(waterColor, skyColor, fresnel) + Sun(V, N2);
													//wc = float4(1,0,0,1);

//													atmosphereDensity = 1;
//													return  float4(1.4*(2 * IN.c0 + 0.2*IN.c1) ,1);
													float clShadow= getGroundShadowFromClouds(normalize(IN.vvertex));

													c.rgb = groundColor(IN.c0, IN.c1, wc*light*attenuation, IN.worldPosition,1)*clShadow;
//													return  float4(wc*atmosphereDensity, 1);
//													c.rgb = IN.B;
												//	c.rgb = atmColor(IN.c0, IN.c1);
													//c.rgb = waterColor;



													return float4(c.rgb
														+ specularReflection*attenuation, +0.85 + specularReflection.b);

												}
													ENDCG
												}
		}
			Fallback  "VertexLit"
}