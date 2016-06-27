Shader "LemonSpawn/LazyClouds" {
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


			uniform float cloudHeight;
			uniform float3 lightDir;
			
				
             struct v2f
             {
                 float4 pos : POSITION;
                 float4 texcoord : TEXCOORD0;
                 float3 normal : TEXCOORD1;
                 float4 uv : TEXCOORD2;
                 float3 worldPosition : TEXCOORD3;
 
                 LIGHTING_COORDS(4,5)
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
 				float3 v3CameraPos = _WorldSpaceCameraPos - v3Translate;	// The camera's current position
				float fCameraHeight = length(v3CameraPos);					// The camera's current height


                 TRANSFER_VERTEX_TO_FRAGMENT(o);
                 
                 return o;
             }
             			// Calculates the Mie phase function

             				
             
		fixed4 frag(v2f IN) : COLOR {
			float3 worldSpacePosition = IN.worldPosition;
			float3 N = float3(0,0,0);
			float3 viewDirection = normalize(_WorldSpaceCameraPos - worldSpacePosition);
			float dist = 1;
			float2 newPos = getCloudUVPos(worldSpacePosition.xyz);


			float x = getNormal(newPos, ls_cloudscale, 0.005*ls_shadowscale, N, 0.05*ls_shadowscale, worldSpacePosition.y/1381.1234f + ls_time*0.0002);//getCloud(IN.uv, 1.729134);
			float3 albedoColor = x*ls_cloudcolor;
			float3 norm= normalize(worldSpacePosition);
			N = normalize(N + norm);
			float globalLight = clamp(dot(norm, lightDir)+0.25,0,1);
			//if (IN.normal.y<0) discard;
			float spec = pow(max(0.0, dot(
                  reflect(-lightDir, N), 
                  viewDirection*-1)), 50);
//             spec = 0;
            float  NL = clamp(0.4*ls_cloudintensity*(1 + spec + 0.45*clamp((pow((dot(-N, lightDir)),1)), -0.5, 1)),-1,1);
//			NL = 1;
			float4 m = tex2D(_MainTex, IN.uv.xy);

			
			float4 c;
			float t = 0.85;
		

			float3 v3CameraPos = _WorldSpaceCameraPos - v3Translate;	// The camera's current position
			float fCameraHeight = length(v3CameraPos);					// The camera's current height
			
			c.rgb = albedoColor;
			c.a = getCloudIntensity(x);
			c.rgb *= NL*globalLight;

//						c.rgb = c.a*float3(1,1,1);

			return c;
             }
             ENDCG
         }
     }
 Fallback "Diffuse"
 }