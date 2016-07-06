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
			Cull Off
		  Lighting on
		   ZWrite on
			ZTest on
		  Tags{ "LightMode" = "ForwardBase" }
			//          Tags {"Queue"="Transparent+1000" "IgnoreProjector"="True" "RenderType"="Transparent"}

		  LOD 400
		  Pass
		  {
			Tags{ "LightMode" = "ForwardBase" }
			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma multi_compile_fwdbase
			#include "AutoLight.cginc"
			#include "Include/Atmosphere.cginc"
			#include "Include/PlanetSurface.cginc"
			//#pragma only_renderers d3d11
			#pragma target 4.0

			#include "UnityCG.cginc"


			#pragma vertex   myVertexShader
			#pragma geometry myGeometryShader
			#pragma fragment myFragmentShader

			// 36 for cube
			#define TAM 12

			float4x4 worldRotMat;
			float3 b_tangent;
			float3 b_binormal;


			struct vIn // Into the vertex shader
			{
				float4 vertex : POSITION;
				float4 color  : COLOR0;
			};

			struct gIn // OUT vertex shader, IN geometry shader
			{
				float4 pos : SV_POSITION;
				float4 col : COLOR0;

			};

			 struct v2f // OUT geometry shader, IN fragment shader 
			{
				float4 pos           : SV_POSITION;
				float2 uv_MainTex : TEXCOORD0;
				float4 col : COLOR0;
				float3 c0 : TEXCOORD1;
				float3 c1 : TEXCOORD2;
				float3 posWorld : TEXCOORD3;
				float3 n: NORMAL;
				LIGHTING_COORDS(5, 6)

			};

			float4       _MainTex_ST;
			sampler2D _MainTex;
			float     _PointSize;
			// ----------------------------------------------------
			gIn myVertexShader(vIn v)
			{
				gIn o; // Out here, into geometry shader
				// Passing on color to next shader (using .r/.g there as tile coordinate)
				o.col = v.color;
				// Passing on center vertex (tile to be built by geometry shader from it later)
				o.pos = v.vertex;
				o.pos = getPlanetSurfaceOnly(v.vertex);
				return o;
			}

			// ----------------------------------------------------

			[maxvertexcount(TAM)]
			// ----------------------------------------------------
			// Using "point" type as input, not "triangle"
			void myGeometryShader(point gIn vert[1], inout TriangleStream<v2f> triStream)
			{
				float f = 7;//_PointSize/20.0f; //half size

				float h = f*0.98;

				bool discardThis = false;

				 float4 pos = vert[0].pos;

				float3 pos3 = mul(_Object2World, vert[0].pos);
				pos3 -= v3Translate;
				float scale = 1 + 1.5*noise(normalize(pos3)*142343.23);



				float3 realN = getPlanetSurfaceNormal(pos3, b_tangent, b_binormal, 0.2,4)*-1;

				if (dot(normalize(realN), normalize(pos3)) < 0.96)
					discardThis = true;




				/*                 const float4 vc[TAM] = { float4( -f,  f,  f, 0.0f), float4(  f,  f,  f, 0.0f), float4(  f,  f, -f, 0.0f),    //Top
														  float4(  f,  f, -f, 0.0f), float4( -f,  f, -f, 0.0f), float4( -f,  f,  f, 0.0f),    //Top

														  float4(  f,  f, -f, 0.0f), float4(  f,  f,  f, 0.0f), float4(  f, -f,  f, 0.0f),     //Right
														  float4(  f, -f,  f, 0.0f), float4(  f, -f, -f, 0.0f), float4(  f,  f, -f, 0.0f),     //Right

														  float4( -f,  f, -f, 0.0f), float4(  f,  f, -f, 0.0f), float4(  f, -f, -f, 0.0f),     //Front
														  float4(  f, -f, -f, 0.0f), float4( -f, -f, -f, 0.0f), float4( -f,  f, -f, 0.0f),     //Front

														  float4( -f, -f, -f, 0.0f), float4(  f, -f, -f, 0.0f), float4(  f, -f,  f, 0.0f),    //Bottom
														  float4(  f, -f,  f, 0.0f), float4( -f, -f,  f, 0.0f), float4( -f, -f, -f, 0.0f),     //Bottom

														  float4( -f,  f,  f, 0.0f), float4( -f,  f, -f, 0.0f), float4( -f, -f, -f, 0.0f),    //Left
														  float4( -f, -f, -f, 0.0f), float4( -f, -f,  f, 0.0f), float4( -f,  f,  f, 0.0f),    //Left

														  float4( -f,  f,  f, 0.0f), float4( -f, -f,  f, 0.0f), float4(  f, -f,  f, 0.0f),    //Back
														  float4(  f, -f,  f, 0.0f), float4(  f,  f,  f, 0.0f), float4( -f,  f,  f, 0.0f)     //Back
														  };


								 const float2 UV1[TAM] = { float2( 0.0f,    0.0f ), float2( 1.0f,    0.0f ), float2( 1.0f,    0.0f ),         //Esta em uma ordem
														   float2( 1.0f,    0.0f ), float2( 1.0f,    0.0f ), float2( 1.0f,    0.0f ),         //aleatoria qualquer.

														   float2( 0.0f,    0.0f ), float2( 1.0f,    0.0f ), float2( 1.0f,    0.0f ),
														   float2( 1.0f,    0.0f ), float2( 1.0f,    0.0f ), float2( 1.0f,    0.0f ),

														   float2( 0.0f,    0.0f ), float2( 1.0f,    0.0f ), float2( 1.0f,    0.0f ),
														   float2( 1.0f,    0.0f ), float2( 1.0f,    0.0f ), float2( 1.0f,    0.0f ),

														   float2( 0.0f,    0.0f ), float2( 1.0f,    0.0f ), float2( 1.0f,    0.0f ),
														   float2( 1.0f,    0.0f ), float2( 1.0f,    0.0f ), float2( 1.0f,    0.0f ),

														   float2( 0.0f,    0.0f ), float2( 1.0f,    0.0f ), float2( 1.0f,    0.0f ),
														   float2( 1.0f,    0.0f ), float2( 1.0f,    0.0f ), float2( 1.0f,    0.0f ),

														   float2( 0.0f,    0.0f ), float2( 1.0f,    0.0f ), float2( 1.0f,    0.0f ),
														   float2( 1.0f,    0.0f ), float2( 1.0f,    0.0f ), float2( 1.0f,    0.0f )
															 };


								 const int TRI_STRIP[TAM]  = {  0, 1, 2,  3, 4, 5,
																6, 7, 8,  9,10,11,
															   12,13,14, 15,16,17,
															   18,19,20, 21,22,23,
															   24,25,26, 27,28,29,
															   30,31,32, 33,34,35
															   };
															 */

								 const float4 vc[TAM] = {
														  float4(0,  f, -f, 0.0f), float4(0,  f,  f, 0.0f), float4(0, -f,  f, 0.0f),     //Right
														  float4(0, -f,  f, 0.0f), float4(0, -f, -f, 0.0f), float4(0,  f, -f, 0.0f),     //Right

														  float4(-f,  f, 0, 0.0f), float4(f,  f, 0, 0.0f), float4(f, -f, 0, 0.0f),     //Front
														  float4(f, -f, 0, 0.0f), float4(-f, -f, 0, 0.0f), float4(-f,  f, 0, 0.0f)     //Front


														  };


								   const float2 UV1[TAM] = {
														   float2(1.0f,    0.0f), float2(1.0f,    1.0f), float2(0.0f,    1.0f),
														   float2(0.0f,    1.0f), float2(0.0f,    0.0f), float2(1.0f,    0.0f),

														   float2(1.0f,    0.0f), float2(1.0f,    1.0f), float2(0.0f,    1.0f),
														   float2(0.0f,    1.0f), float2(0.0f,    0.0f), float2(1.0f,    0.0f)


															 };

								 const int TRI_STRIP[TAM] = {  0, 1, 2,  3, 4, 5,
																6, 7, 8,  9,10,11 };

								 const int UV_TRI_STRIP[TAM] = {  2, 1, 0 , 5, 4, 3,
																8, 7, 6,  11,10,9 };

								 v2f v[TAM];
								 int i;
								 // Assign new vertices positions 
								 for (i = 0; i < TAM; i++) {
									v[i].pos = pos + mul(worldRotMat,vc[i] + float3(0,h,0))*scale;
									v[i].col = vert[0].col;
									v[i].posWorld = pos3 + v3Translate;
									float h = (length(v[i].posWorld - v3Translate) / fInnerRadius - 1);
									if (h < liquidThreshold)
										discardThis = true;
									if (h > topThreshold)
										discardThis = true;

									v[i].n = realN;
								 }

								 // Assign UV values
				//                 for (i=0;i<TAM;i++) v[i].uv_MainTex = TRANSFORM_TEX(UV1[i],_MainTex); 
								 for (i = 0; i < TAM; i++) v[i].uv_MainTex = UV1[UV_TRI_STRIP[i]];//TRANSFORM_TEX(UV1[i],_MainTex); 

								 // Position in view space
								 for (i = 0; i < TAM; i++) { v[i].pos = mul(UNITY_MATRIX_MVP, v[i].pos); }

								 float3 c0, c1;

								 getGroundAtmosphere(pos, c0, c1);

								 for (i = 0; i < TAM; i++) {
									v[i].c0 = c0;
									v[i].c1 = c1;
								  }
								 for (i = 0; i < TAM; i++)
									 TRANSFER_VERTEX_TO_FRAGMENT(v[i]);

								 // Build the cube tile by submitting triangle strip vertices
								 if (!discardThis)
								 for (i = 0; i < TAM / 3; i++)
								 {
									 triStream.Append(v[TRI_STRIP[i * 3 + 0]]);
									 triStream.Append(v[TRI_STRIP[i * 3 + 1]]);
									 triStream.Append(v[TRI_STRIP[i * 3 + 2]]);

									 triStream.RestartStrip();
								 }
							  }

			// ----------------------------------------------------
		   float4 myFragmentShader(v2f IN) : COLOR
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
	 
/*			pass {
				 Name "ShadowCollector"

					 Tags{ "LightMode" = "ShadowCollector" }

					 ZWrite On
					 ZTest Less

					 CGPROGRAM
// Upgrade NOTE: excluded shader from DX11 and Xbox360; has structs without semantics (struct v2f2 members vertex)
//#pragma exclude_renderers d3d11 xbox360

#pragma target 5.0
#pragma vertex vert
#pragma geometry geo
#pragma fragment frag
#define SHADOW_COLLECTOR_PASS
#pragma fragmentoption ARB_precision_hint_fastest
#pragma multi_compile_shadowcollector

#include "UnityCG.cginc"





#include "UnityCG.cginc"
#pragma multi_compile_fwdbase
#include "AutoLight.cginc"
#include "Include/Atmosphere.cginc"
#include "Include/PlanetSurface.cginc"
					 //#pragma only_renderers d3d11
#pragma target 4.0

#include "UnityCG.cginc"

#pragma vertex   vert
#pragma geometry geo
#pragma fragment frag

			 // 36 for cube
#define TAM 12

					 float4x4 worldRotMat;
				 float3 b_tangent;
				 float3 b_binormal;


				 struct appdata {

					 float4 vertex : POSITION;
				 };

				 struct gIn2 // OUT vertex shader, IN geometry shader
				 {
					 float4 vertex : SV_POSITION;
					 

				 };

				 struct v2f2 // OUT geometry shader, IN fragment shader 
				 {
					 V2F_SHADOW_COLLECTOR;
				 };


				 gIn2 vert(appdata v)
				 {
					 gIn2 o; // Out here, into geometry shader
							// Passing on color to next shader (using .r/.g there as tile coordinate)
					 // Passing on center vertex (tile to be built by geometry shader from it later)
					 o.vertex = getPlanetSurfaceOnly(v.vertex);
					 return o;
				 }

				 // ----------------------------------------------------

				 [maxvertexcount(TAM)]
				 // ----------------------------------------------------
				 // Using "point" type as input, not "triangle"
				 void geo(point gIn2 vert[1], inout TriangleStream<v2f2> triStream)
				 {
					 float f = 7;//_PointSize/20.0f; //half size

					 float h = f*0.98;


					 float4 pos = vert[0].vertex;

					 float3 pos3 = mul(_Object2World, vert[0].vertex);
					 pos3 -= v3Translate;
					 float scale = 1 + 1.5*noise(normalize(pos3)*142343.23);

					 float3 realN = getPlanetSurfaceNormal(pos3, b_tangent, b_binormal, 0.2, 4)*-1;

					 if (dot(normalize(realN), normalize(pos3)) < 0.99)
						 h -= 1000;


					 const float4 vc[TAM] = {
						 float4(0,  f, -f, 0.0f), float4(0,  f,  f, 0.0f), float4(0, -f,  f, 0.0f),     //Right
						 float4(0, -f,  f, 0.0f), float4(0, -f, -f, 0.0f), float4(0,  f, -f, 0.0f),     //Right

						 float4(-f,  f, 0, 0.0f), float4(f,  f, 0, 0.0f), float4(f, -f, 0, 0.0f),     //Front
						 float4(f, -f, 0, 0.0f), float4(-f, -f, 0, 0.0f), float4(-f,  f, 0, 0.0f)     //Front


					 };


					 const float2 UV1[TAM] = {
						 float2(1.0f,    0.0f), float2(1.0f,    1.0f), float2(0.0f,    1.0f),
						 float2(0.0f,    1.0f), float2(0.0f,    0.0f), float2(1.0f,    0.0f),

						 float2(1.0f,    0.0f), float2(1.0f,    1.0f), float2(0.0f,    1.0f),
						 float2(0.0f,    1.0f), float2(0.0f,    0.0f), float2(1.0f,    0.0f)


					 };

					 const int TRI_STRIP[TAM] = { 0, 1, 2,  3, 4, 5,
						 6, 7, 8,  9,10,11 };

					 const int UV_TRI_STRIP[TAM] = { 2, 1, 0 , 5, 4, 3,
						 8, 7, 6,  11,10,9 };

					 v2f2 outV[TAM];
					 int i;
					 // Assign new vertices positions 
					 for (i = 0; i < TAM; i++) {
						 outV[i].pos = pos + mul(worldRotMat, vc[i] + float3(0, h, 0))*scale;
					 }



																					  // Position in view space
					 for (i = 0; i < TAM; i++) { outV[i].pos = mul(UNITY_MATRIX_MVP, outV[i].pos); }

					 for (i = 0; i < TAM; i++) {
						 gIn2 v = vert[0];
						 TRANSFER_SHADOW_COLLECTOR(outV[i])
					 }

					 // Build the cube tile by submitting triangle strip vertices
					 for (i = 0; i < TAM / 3; i++)
					 {
						 triStream.Append(outV[TRI_STRIP[i * 3 + 0]]);
						 triStream.Append(outV[TRI_STRIP[i * 3 + 1]]);
						 triStream.Append(outV[TRI_STRIP[i * 3 + 2]]);

						 triStream.RestartStrip();
					 }
				 }

				 // ----------------------------------------------------
				 float4 frag(v2f2 IN) : COLOR
				 {
					 SHADOW_COLLECTOR_FRAGMENT(IN)
				 }
					 ENDCG
			 }
			 
			 */
/*

			 pass {
				 Name "ShadowCaster"

					 Tags{ "LightMode" = "ShadowCaster" }
					 Cull off

					 ZWrite On
					 ZTest Less

					 CGPROGRAM

#pragma target 5.0
#pragma vertex vert
#pragma geometry geo
#pragma fragment frag
#define SHADOW_COLLECTOR_PASS
#pragma fragmentoption ARB_precision_hint_fastest
#pragma multi_compile_shadowcaster

#include "UnityCG.cginc"





#include "UnityCG.cginc"
#pragma multi_compile_fwdbase
#include "AutoLight.cginc"
#include "Include/Atmosphere.cginc"
#include "Include/PlanetSurface.cginc"
					 //#pragma only_renderers d3d11
#pragma target 4.0

#include "UnityCG.cginc"

#pragma vertex   vert
#pragma geometry geo
#pragma fragment frag

					 // 36 for cube
#define TAM 12
#if defined(_ALPHABLEND_ON) || defined(_ALPHAPREMULTIPLY_ON)
#if !((SHADER_TARGET < 30) || defined (SHADER_API_MOBILE) || defined(SHADER_API_D3D11_9X) || defined (SHADER_API_PSP2) || defined (SHADER_API_PSM))
#define UNITY_STANDARD_USE_DITHER_MASK 1
#endif
#endif

					 // Need to output UVs in shadow caster, since we need to sample texture and do clip/dithering based on it
#if defined(_ALPHATEST_ON) || defined(_ALPHABLEND_ON) || defined(_ALPHAPREMULTIPLY_ON)
#define UNITY_STANDARD_USE_SHADOW_UVS 1
#endif

					 // Has a non-empty shadow caster output struct (it's an error to have empty structs on some platforms...)
#if !defined(V2F_SHADOW_CASTER_NOPOS_IS_EMPTY) || defined(UNITY_STANDARD_USE_SHADOW_UVS)
#define UNITY_STANDARD_USE_SHADOW_OUTPUT_STRUCT 1
#endif
					 float4x4 worldRotMat;
				 float3 b_tangent;
				 float3 b_binormal;


				 struct gIn2 // OUT vertex shader, IN geometry shader
				 {
					 float4 vertex : SV_POSITION;


				 };

				 struct v2f2 // OUT geometry shader, IN fragment shader 
				 {
					 V2F_SHADOW_CASTER;
				 };


				 gIn2 vert(appdata_base v)
				 {
					 gIn2 o; // Out here, into geometry shader
							 // Passing on color to next shader (using .r/.g there as tile coordinate)
							 // Passing on center vertex (tile to be built by geometry shader from it later)
					 o.vertex = getPlanetSurfaceOnly(v.vertex);
					 return o;
				 }

				 // ----------------------------------------------------

				 [maxvertexcount(TAM)]
				 // ----------------------------------------------------
				 // Using "point" type as input, not "triangle"
				 void geo(point gIn2 vert[1], inout TriangleStream<v2f2> triStream)
				 {
					 float f = 7;//_PointSize/20.0f; //half size

					 float h = f*0.98;


					 float4 pos = vert[0].vertex;

					 float3 pos3 = mul(_Object2World, vert[0].vertex);
					 pos3 -= v3Translate;
					 float scale = 1 + 1.5*noise(normalize(pos3)*142343.23);

					 float3 realN = getPlanetSurfaceNormal(pos3, b_tangent, b_binormal, 0.2, 4)*-1;

					 if (dot(normalize(realN), normalize(pos3)) < 0.99)
						 h -= 1000;


					 const float4 vc[TAM] = {
						 float4(0,  f, -f, 0.0f), float4(0,  f,  f, 0.0f), float4(0, -f,  f, 0.0f),     //Right
						 float4(0, -f,  f, 0.0f), float4(0, -f, -f, 0.0f), float4(0,  f, -f, 0.0f),     //Right

						 float4(-f,  f, 0, 0.0f), float4(f,  f, 0, 0.0f), float4(f, -f, 0, 0.0f),     //Front
						 float4(f, -f, 0, 0.0f), float4(-f, -f, 0, 0.0f), float4(-f,  f, 0, 0.0f)     //Front


					 };


					 const float2 UV1[TAM] = {
						 float2(1.0f,    0.0f), float2(1.0f,    1.0f), float2(0.0f,    1.0f),
						 float2(0.0f,    1.0f), float2(0.0f,    0.0f), float2(1.0f,    0.0f),

						 float2(1.0f,    0.0f), float2(1.0f,    1.0f), float2(0.0f,    1.0f),
						 float2(0.0f,    1.0f), float2(0.0f,    0.0f), float2(1.0f,    0.0f)


					 };

					 const int TRI_STRIP[TAM] = { 0, 1, 2,  3, 4, 5,
						 6, 7, 8,  9,10,11 };

					 const int UV_TRI_STRIP[TAM] = { 2, 1, 0 , 5, 4, 3,
						 8, 7, 6,  11,10,9 };

					 v2f2 outV[TAM];
					 int i;
					 // Assign new vertices positions 
					 for (i = 0; i < TAM; i++) {
						 outV[i].pos = pos + mul(worldRotMat, vc[i] + float3(0, h, 0))*scale;
						// v[i].vertex = v[i].pos;
					 }



					 // Position in view space
					 for (i = 0; i < TAM; i++) {
						 outV[i].pos = mul(UNITY_MATRIX_MVP, outV[i].pos);
				
					 }

					 for (i = 0; i < TAM; i++) {
						 v2f2 ot = outV[i];
						 //TRANSFER_SHADOW_CASTER_NOPOS(vv, opos)
						 gIn2 v = vert[0];
						 TRANSFER_SHADOW_CASTER(ot)
					 }

						 // Build the cube tile by submitting triangle strip vertices
						 for (i = 0; i < TAM / 3; i++)
						 {
							 triStream.Append(outV[TRI_STRIP[i * 3 + 0]]);
							 triStream.Append(outV[TRI_STRIP[i * 3 + 1]]);
							 triStream.Append(outV[TRI_STRIP[i * 3 + 2]]);

							 triStream.RestartStrip();
						 }
				 }

				 // ----------------------------------------------------
				 float4 frag(v2f2 IN) : COLOR
				 {
					 SHADOW_CASTER_FRAGMENT(IN)
				 }
					 ENDCG
			 }
*/			 
		}
			Fallback "Diffuse"
}