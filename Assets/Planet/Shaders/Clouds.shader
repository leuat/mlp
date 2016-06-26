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

        sampler2D _MainTex, _CloudTex,_CloudTex2;
		float ls_time;
		float ls_cloudscale;
		float ls_cloudscattering;
		float ls_cloudintensity;
		float ls_cloudsharpness;
		float ls_shadowscale;
		float ls_distScale;
		float ls_cloudthickness;
		float3 ls_cloudcolor;
		float LS_LargeVortex;
		float LS_SmallVortex;


			uniform float cloudHeight;
			uniform float3 lightDir;
			
		float getCloud(float2 uv, float scale, float disp) {
					float y = 0.0f;
					// Perlin octaves
					int NN = 9;
//					scale = scale*(1 + LS_LargeVortex*tex2D(_CloudTex, uv*0.0441)).x;
					scale = scale*(1 + pow(LS_LargeVortex*tex2D(_CloudTex2, uv*0.423421).x,0.5));
					//scale = scale*(1 + LS_SmallVortex*tex2D(_CloudTex, uv*3.234)).x;

					for(int i=0;i < NN; i++) {
						float k = scale*pow(2,i)  + 0.11934;
						y+= 1.0/pow(k,2)*tex2D( _CloudTex, k*uv + float2(0.1234*i*ls_time*0.015 - 0.04234*i*i*ls_time*0.015 + 0.9123559 + 0.23411*k , 0.31342  + 0.5923*i*i + disp) ).x;
						//y+= tex2D( _CloudTex, k*uv + float2(0.1234*i*ls_time*0.015 - 0.04234*i*i*ls_time*0.015 + 0.9123559 + 0.23411*k , 0.31342  + 0.5923*i*i + disp) ).x;
					}
					// Normalize
				
					y /= 2*NN;
					return clamp( pow(ls_cloudscattering/y, ls_cloudsharpness) - 0.2,0,10);

				}
			
	 	// returns cloud value, outputs normal to N. 
	 	
		float getNormal(float2 uv, float scale, float dst, out float3 n, float nscale, float disp) {
					float height = getCloud(uv, scale, disp);
					int N =4;
					for (int i=0;i<N;i++) {
					
						float2 du1 = float2(dst*cos((i)*2*3.14159 / (N)), dst*sin(i*2*3.14159/(N)));
						float2 du2 = float2(dst*cos((i+1)*2*3.14159 / (N)), dst*sin((i+1)*2*3.14159/(N)));
						
						float hx = getCloud(uv + du1, scale, disp);
						float hy = getCloud(uv + du2, scale, disp);
					
						float3 d2 = float3(0,height*nscale,0) - float3(du1.x,hx*nscale,du1.y);
						float3 d1 = float3(0,height*nscale,0) - float3(du2.x,hy*nscale,du2.y);
					
						n = n + normalize(cross(d1,d2));
					}
					n = normalize(n);
					return height;
					
		}
				
				
             struct v2f
             {
                 float4 pos : POSITION;
                 float4 texcoord : TEXCOORD0;
                 float3 normal : TEXCOORD1;
                 float4 uv : TEXCOORD2;
                 float3 worldPosition : TEXCOORD3;
//                 float3 t0 : TEXCOORD4;
//    			float3 c0 : COLOR0;
//    			float3 c1 : COLOR1;

 
                 LIGHTING_COORDS(4,5)
             };
              
             
             v2f vert (appdata_base v)
             {
                 v2f o;
                 o.pos = mul( UNITY_MATRIX_MVP, v.vertex);
                 o.uv = v.texcoord;

				 o.uv.xy = pos2uv(v.vertex.xyz);

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
//			float3 lightDir = _WorldSpaceLightPos0;			
			float3 viewDirection = normalize(_WorldSpaceCameraPos - worldSpacePosition);
//			float dist = clamp(1.0/pow(length(0.5 + 0.0001*ls_distScale*(_WorldSpaceCameraPos - worldSpacePosition)),1.0),0,1);
			float dist = 1;
 			//float2 newPos = worldSpacePosition.xz*0.00005;
 			float2 newPos = IN.uv*11.91234;
 			//newPos.x = atan2(worldSpacePosition.y, worldSpacePosition.x);
 			//newPos.y = acos(normalize(worldSpacePosition).z);		
 	//		newPos = 0;

			float x = getNormal(newPos, 25.73252*ls_cloudscale*0.03791, 0.005*ls_shadowscale, N, 0.05*ls_shadowscale, worldSpacePosition.y/1381.1234f + ls_time*0.0002);//getCloud(IN.uv, 1.729134);
			float3 albedoColor = x*ls_cloudcolor;
			float3 norm= normalize(worldSpacePosition);
			N = normalize(N + norm);
			float globalLight = saturate(dot(norm, lightDir));
			//if (IN.normal.y<0) discard;
			float spec = pow(max(0.0, dot(
                  reflect(-lightDir, N), 
                  viewDirection)), 2);
//             spec = 0;
            float  NL = 0.4*ls_cloudintensity*(1 + spec + clamp((pow((dot(-N, lightDir)),1)), -0.4, 1));
//			NL = 1;
			float4 m = tex2D(_MainTex, IN.uv.xy);

			
			float4 c;
			float t = 0.85;
		

			float3 v3CameraPos = _WorldSpaceCameraPos - v3Translate;	// The camera's current position
			float fCameraHeight = length(v3CameraPos);					// The camera's current height
			
			/*t = 0.5;
			c.rgb=  (t*albedoColor + (1-t)*m.rgb*ls_cloudcolor)*NL*globalLight;
			c.a = 0.9*clamp(5*ls_cloudthickness*pow(t*x + (1-t)*m.r,2)*dist,0,1);
*/

//			c.rgb = albedoColor * m.rgb * NL*globalLight;
			c.rgb = albedoColor;
			c.a = 0.7*clamp(5 * ls_cloudthickness*pow(c.r, 2), 0, 1);

			c.rgb *= NL*globalLight;

			return c;
             }
             ENDCG
         }
     }
 Fallback "Diffuse"
 }