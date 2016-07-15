Shader "LemonSpawn/Star" {
	Properties {
	}
	SubShader {
//	    Tags {"Queue"="Transparent-1" "IgnoreProjector"="True" "RenderType"="Transparent"}
	    Tags {"Queue"="Transparent+1105" "IgnoreProjector"="True" "RenderType"="Transparent"}
        LOD 400


		Lighting On
        Cull Back
        ZWrite on
        ZTest on
        Blend One One//SrcAlpha OneMinusSrcAlpha
       Pass
         {

	Tags { "LightMode" = "ForwardBase" }
         
             CGPROGRAM
// Upgrade NOTE: excluded shader from DX11 and Xbox360; has structs without semantics (struct v2f members worldPosition)
             
             #pragma target 3.0
             #pragma fragmentoption ARB_precision_hint_fastest
             
             #pragma vertex vert
             #pragma fragment frag
             #pragma multi_compile_fwdbase
                         
             #include "UnityCG.cginc"
             #include "AutoLight.cginc"
#include "Include/Utility.cginc"
#include "Include/Atmosphere.cginc"

        sampler2D _MainTex;

             struct v2f
             {
                 float4 pos : POSITION;
                 float4 texcoord : TEXCOORD0;
                 float3 normal : TEXCOORD1;
                 float4 uv : TEXCOORD2;
                 float3 worldPosition : TEXCOORD3;
                 LIGHTING_COORDS(6,7)
             };
              
             
             v2f vert (appdata_base v)
             {
                 v2f o;
                 o.pos = mul( UNITY_MATRIX_MVP, v.vertex);
                 o.uv = v.texcoord;

				 o.uv.xy = pos2uv(v.vertex.xyz);
				 o.uv.xy *= stretch;

                 o.normal = normalize(v.normal).xyz;
                 o.texcoord = v.texcoord;
 				 o.worldPosition = v.vertex;//mul (_Object2World, v.vertex).xyz;
 				 //o.worldPosition = mul (_Object2World, v.vertex).xyz;

 				 float3 vv = mul(_Object2World, v.vertex).xyz;
 				 vv = normalize(vv-v3Translate)*fInnerRadius*1.01;

//  			   	  getGroundAtmosphere(v.vertex, o.c0, o.c1);
  				

                 TRANSFER_VERTEX_TO_FRAGMENT(o);
                 
                 return o;
             }
             			// Calculates the Mie phase function

             				



		float getSun(float3 pos, int N, float scale) {

			
			float3 p = pos;

			float val = 0;
//			[uroll]
			float A = 0;
			for (int i=0;i<N;i++){
				float k = (i+1)*scale;
				float amp = 1/((i+1)*k);
				float v = noise(p*k);
				val += amp*v;
				A+=amp;
			}
			
			return val/A;

			}

                            
		fixed4 frag(v2f i) : COLOR {

			float3 worldSpacePosition = i.worldPosition - v3Translate*0;
			float3 viewDirection = normalize(_WorldSpaceCameraPos - worldSpacePosition);

			float globalLight = clamp(dot(i.normal, normalize(lightDir))+0.25,0,1);


//			globalLight = pow(globalLight, 1);


			float3 shift = float3(1.2314, 0.6342, 0.96123)*23.4;

			float3 pos = normalize(worldSpacePosition);
			float4 c;



			float val = pow(getSun(pos, 4, 5.23)+0.6,2);

			c.a = 1;
			c.rgb = float3(1, 0.9, 0.5)*val;


			return c;
             }
             ENDCG
         }
     }
 Fallback "Diffuse"
 }