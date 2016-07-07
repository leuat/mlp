Shader "Leuat/TreeShader"
{
	Properties
	{
		_MainTex("TileTexture", 2D) = "white" {}
		_PointSize("Point Size", Float) = 1.0
	}


		SubShader
		{

			//Blend SrcAlpha OneMinusSrcAlpha    
		/*	Cull Off
		  Lighting on
		   ZWrite on
			ZTest on
		  Tags{ "LightMode" = "ForwardBase" }*/
			//          Tags {"Queue"="Transparent+1000" "IgnoreProjector"="True" "RenderType"="Transparent"}
		//	Tags{ "LightMode" = "ForwardBase" }
		  LOD 400
		  Pass
		  {

	    	Cull Off
			Lighting on



			Tags{ "LightMode" = "ForwardBase" }
			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma multi_compile_fwdbase
			#include "AutoLight.cginc"
			#include "Include/Atmosphere.cginc"
			#include "Include/PlanetSurface.cginc"
#define L_FRAG_PASS

			#include "Include/GeometryShaderExtra.cginc"

			//#pragma only_renderers d3d11
			#pragma target 4.0

#pragma vertex   vert
#pragma geometry geo
#pragma fragment frag


			// ----------------------------------------------------

			[maxvertexcount(TAM)]
			// ----------------------------------------------------
			// Using "point" type as input, not "triangle"
			void geo(point gIn vert[1], inout TriangleStream<v2f> triStream)
			{
				setupCross(vert, triStream);

			}

			// ----------------------------------------------------
		   float4 frag(v2f IN) : COLOR
		   {
			   //return float4(1.0,0.0,0.0,1.0);
			   float4 v = tex2D(_MainTex, IN.uv_MainTex);
			   v.xyz *= IN.col.xyz;
			   float attenuation = clamp(LIGHT_ATTENUATION(IN), 0.1, 1);

			   float3 lightDirection =
				   normalize(_WorldSpaceLightPos0.xyz);
			   float3 light = clamp(dot(IN.n, lightDirection), 0, 1);


			   float realAtt = attenuation;
//			   if (dot(lightDirection, normalize(_WorldSpaceCameraPos - IN.posWorld) > 0))
	//			   realAtt = 1;

			   v.rgb = groundColor(IN.c0, IN.c1, v.xyz*realAtt*light, IN.posWorld, 1.0);

			   float dist = length(_WorldSpaceCameraPos - IN.posWorld);
			  float scale = 1 - clamp(sqrt(dist / fInnerRadius*2.5), 0, 1);

			  v.a *= scale;
			  if (v.a < 0.25)
				discard;
			
				return v;
			 }

		 ENDCG



     }
	 
			pass {
				 Name "ShadowCollector"

					 Tags{ "LightMode" = "ShadowCollector" }

					 Fog{ Mode Off }
					 ZWrite On ZTest LEqual


					 CGPROGRAM
// Upgrade NOTE: excluded shader from DX11 and Xbox360; has structs without semantics (struct v2f2 members vertex)
//#pragma exclude_renderers d3d11 xbox360

#pragma target 4.0
#pragma vertex vert
#pragma geometry geo
#pragma fragment frag
#define SHADOW_COLLECTOR_PASS
#pragma fragmentoption ARB_precision_hint_fastest
#pragma multi_compile_shadowcollector

#include "UnityCG.cginc"



#include "AutoLight.cginc"
#include "Include/Atmosphere.cginc"
#include "Include/PlanetSurface.cginc"

#define L_SC_PASS			

#include "Include/GeometryShaderExtra.cginc"


struct v2f_sc
{
	float2 uv_MainTex : TEXCOORD1;
	V2F_SHADOW_COLLECTOR;
};

				 // ----------------------------------------------------

				 [maxvertexcount(TAM)]
				 // ----------------------------------------------------
				 // Using "point" type as input, not "triangle"
				 void geo(point gIn vert[1], inout TriangleStream<v2f_sc> triStream)
				 {
					 setupCross(vert, triStream);
				 }


				 // ----------------------------------------------------
				 float4 frag(v2f_sc IN) : COLOR
				 {
					 SHADOW_COLLECTOR_FRAGMENT(IN)
				 }
					 ENDCG
			 }
			 
			 


			 pass {
				 Name "ShadowCaster"

					 Tags{ "LightMode" = "ShadowCaster" }
					

					
					
					 Fog{ Mode Off }
					 ZWrite On ZTest Less Cull Off
					 Offset 1, 1

					 CGPROGRAM

#pragma vertex vert
#pragma geometry geo
#pragma fragment frag
#pragma fragmentoption ARB_precision_hint_fastest
//#pragma multi_compile SHADOWS_NATIVE SHADOWS_CUBE
#pragma multi_compile_shadowcaster

#include "UnityCG.cginc"
#include "Include/Atmosphere.cginc"
#include "Include/PlanetSurface.cginc"
#define L_SCAST_PASS	


					 struct v2f_scast // OUT geometry shader, IN fragment shader 
				 {
					 float2 uv_MainTex : TEXCOORD1;
					 float3 posWorld : TEXCOORD2;

					 V2F_SHADOW_CASTER;
				 };

#include "Include/GeometryShaderExtra.cginc"



				 [maxvertexcount(TAM)]
				 void geo(point gIn vert[1], inout TriangleStream<v2f_scast> triStream)
				 {
					 setupCross(vert, triStream);
				 }


				 // ----------------------------------------------------
				 float4 frag(v2f_scast IN) : COLOR
				 {
					 float4 v = tex2D(_MainTex, IN.uv_MainTex);
					 float dist = length(_WorldSpaceCameraPos - IN.posWorld);
					 float scale = 1 - clamp(sqrt(dist / fInnerRadius*2.5), 0, 1);

					 v.a *= scale;
					 if (v.a < 0.25)
						 discard;
					 SHADOW_CASTER_FRAGMENT(IN)
				 }
					 ENDCG
			 }
			
		}
			Fallback "Diffuse"
}