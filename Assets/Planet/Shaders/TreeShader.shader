Shader "Leuat/TreeShader" 
 {        
     Properties 
     {
         _MainTex ("TileTexture", 2D) = "white" {}
         _PointSize("Point Size", Float) = 1.0
     }


     SubShader 
     {
         LOD 200

//         ZTest off
         Blend SrcAlpha OneMinusSrcAlpha    
         Cull Off

          Tags {"Queue"="Transparent+1105" "IgnoreProjector"="True" "RenderType"="Transparent"}
        LOD 400
         Pass 
         {

             CGPROGRAM
 
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
  
                 return o;
             }
             
             // ----------------------------------------------------
             
             [maxvertexcount(TAM)] 
             // ----------------------------------------------------
             // Using "point" type as input, not "triangle"
             void myGeometryShader(point gIn vert[1], inout TriangleStream<v2f> triStream)
             {                            
                 float f = 10;//_PointSize/20.0f; //half size

                 float h = f*0.98;


                 float4 pos = getPlanetSurfaceOnly(vert[0].pos);

                 float3 pos3 = mul(_Object2World, vert[0].pos);
                 pos3 -= v3Translate;


                

                 float3 realN = getPlanetSurfaceNormal(pos3, b_tangent, b_binormal, 0.2,4)*-1;

                 if (dot(normalize(realN), normalize(pos3))<0.99)
                   h-=1000;



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
                                          float4(  0,  f, -f, 0.0f), float4(  0,  f,  f, 0.0f), float4(  0, -f,  f, 0.0f),     //Right
                                          float4(  0, -f,  f, 0.0f), float4(  0, -f, -f, 0.0f), float4(  0,  f, -f, 0.0f),     //Right
                                          
                                          float4( -f,  f, 0, 0.0f), float4(  f,  f, 0, 0.0f), float4(  f, -f, 0, 0.0f),     //Front
                                          float4(  f, -f, 0, 0.0f), float4( -f, -f, 0, 0.0f), float4( -f,  f, 0, 0.0f)     //Front
            
                                        
                                          };
                                          
                 // WTF
/*                 const float2 UV1[TAM] = { 
                                           float2( 1.0f,    1.0f ), float2( 1.0f,    0.0f ), float2( 0.0f,    0.0f ), 
                                           float2( 1.0f,    1.0f ), float2( 0.0f,    1.0f ), float2( 0.0f,    0.0f ),
                                           
                                           float2( 1.0f,    1.0f ), float2( 1.0f,    0.0f ), float2( 0.0f,    0.0f ), 
                                           float2( 1.0f,    1.0f ), float2( 0.0f,    1.0f ), float2( 0.0f,    0.0f )


                                             };
                                             */
                                                                                                  
                   const float2 UV1[TAM] = { 
                                           float2( 1.0f,    0.0f ), float2( 1.0f,    1.0f ), float2( 0.0f,    1.0f ), 
                                           float2( 0.0f,    1.0f ), float2( 0.0f,    0.0f ), float2( 1.0f,    0.0f ),
                                           
                                           float2( 1.0f,    0.0f ), float2( 1.0f,    1.0f ), float2( 0.0f,    1.0f ), 
                                           float2( 0.0f,    1.0f ), float2( 0.0f,    0.0f ), float2( 1.0f,    0.0f )


                                             };
                                                             
                 const int TRI_STRIP[TAM]  = {  0, 1, 2,  3, 4, 5,
                                                6, 7, 8,  9,10,11 };

                 const int UV_TRI_STRIP[TAM]  = {  2, 1, 0 , 5, 4, 3,
                                                8, 7, 6,  11,10,9 };

                 v2f v[TAM];
                 int i;

                 // Assign new vertices positions 
                 for (i=0;i<TAM;i++) { 
                 	v[i].pos = pos + mul(worldRotMat,vc[i] + float3(0,h,0)); 
                 	v[i].col = vert[0].col; 
                 	v[i].posWorld = pos3 + v3Translate; 
                 }
 
                 // Assign UV values
//                 for (i=0;i<TAM;i++) v[i].uv_MainTex = TRANSFORM_TEX(UV1[i],_MainTex); 
                 for (i=0;i<TAM;i++) v[i].uv_MainTex = UV1[UV_TRI_STRIP[i]];//TRANSFORM_TEX(UV1[i],_MainTex); 
                 
                 // Position in view space
                 for (i=0;i<TAM;i++) { v[i].pos = mul(UNITY_MATRIX_MVP, v[i].pos); }

                 float3 c0, c1;

				 getGroundAtmosphere(pos, c0, c1);

				 for (i=0;i<TAM;i++) { 
				 	v[i].c0 = c0;
				 	v[i].c1 = c1;
				  }

                 // Build the cube tile by submitting triangle strip vertices
                 for (i=0;i<TAM/3;i++)
                 { 
                     triStream.Append(v[TRI_STRIP[i*3+0]]);
                     triStream.Append(v[TRI_STRIP[i*3+1]]);
                     triStream.Append(v[TRI_STRIP[i*3+2]]);    
                                     
                     triStream.RestartStrip();
                 }
              }
              
              // ----------------------------------------------------
             float4 myFragmentShader(v2f IN) : COLOR
             {
                 //return float4(1.0,0.0,0.0,1.0);
                 float4 v = tex2D(_MainTex, IN.uv_MainTex);
                 v.xyz *= IN.col.xyz;

                 v.rgb = groundColor(IN.c0, IN.c1, v.xyz, IN.posWorld, 10.0);



/*                 float dist = length(_WorldSpaceCameraPos - IN.posWorld);
				float scale = 1-clamp(sqrt(dist/fInnerRadius*3.0), 0, 1);

				v.a*=scale;
*/
//				 v.a = 1;
//				 v.rgb = float3(1,1,1)*scale;
                 if (v.a<0.25)
                 	discard;
                 v.a =clamp(v.a,0.9,1);
//                 v.a = 1;

                 return v;
             }
 
             ENDCG
         }
     } 
 }