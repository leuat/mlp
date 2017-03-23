// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "LemonSpawn/CloudID" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_CloudTex ("Base (RGB)", 2D) = "white" {}
		_CloudTex2("Base (RGB)", 2D) = "white" {}
	}
	SubShader {
//	    Tags {"Queue"="Transparent-1" "IgnoreProjector"="True" "RenderType"="Transparent"}
	    Tags {"Queue"="Transparent+1105" "IgnoreProjector"="True" "RenderType"="Transparent"}
        LOD 400


		Lighting On
        Cull off
        ZWrite off
        ZTest on
        Blend SrcAlpha OneMinusSrcAlpha
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
			 	 float3 c0 : TEXCOORD4;
				 float3 c1 : TEXCOORD5;
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

 				 float3 vv = mul(unity_ObjectToWorld, v.vertex).xyz;
 				 vv = normalize(vv-v3Translate)*fInnerRadius*1.01;

//  			   	  getGroundAtmosphere(v.vertex, o.c0, o.c1);
  			   	  getGroundAtmosphere(mul(unity_WorldToObject,vv + v3Translate), o.c0, o.c1);
  				

                 TRANSFER_VERTEX_TO_FRAGMENT(o);
                 
                 return o;
             }
             			// Calculates the Mie phase function

             				



		float getIQCloudShadow(float3 start, float3 direction, float stepLength, int CloudLOD) {

			int N = 4;

			float3 p = start;

			float val = 0;
//			[uroll]
			for (int i=0;i<N;i++){
				float v = getIQClouds(p, CloudLOD);
				float h = abs(length(p)-1);
				v = exp(-(h*200))*v;

				val += v;
				p = p + direction*stepLength;
			}
			
			return val/(float)(N);

		}

                            
		fixed4 frag(v2f i) : COLOR {

			float3 worldSpacePosition = i.worldPosition - v3Translate*0;
			float3 viewDirection = normalize(_WorldSpaceCameraPos - worldSpacePosition);

			float globalLight = clamp(dot(i.normal, normalize(lightDir))+0.25,0,1);


//			globalLight = pow(globalLight, 1);


			float3 shift = float3(1.2314, 0.6342, 0.96123)*23.4;

			float3 pos = normalize(worldSpacePosition);
			float4 c;

			int N = 10;

			float val = getIQClouds(pos,N);

			c.rgb = ls_cloudcolor*ls_cloudintensity;
			c.a = val;

			c.a*=ls_cloudthickness;
			if (c.a<0.02)
				discard;


			float shadow = getIQCloudShadow(pos, normalize(v3LightPos), ls_shadowscale, 4);

			shadow = clamp(shadow*0.4,0.0,1.0);

			c.rgb*=(1.0-shadow)*globalLight;

//			c.rgb = groundColor(i.c0, i.c1, c.rgb*0);
			c.rgb = atmColor(i.c0, i.c1)+c.rgb;


			return c;
             }
             ENDCG
         }
     }
 Fallback "Diffuse"
 }