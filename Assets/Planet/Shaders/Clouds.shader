Shader "LemonSpawn/LazyClouds" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_CloudTex ("Base (RGB)", 2D) = "white" {}
	}
	SubShader {
//	    Tags {"Queue"="Transparent-1" "IgnoreProjector"="True" "RenderType"="Transparent"}
	    Tags {"Queue"="Transparent+1105" "IgnoreProjector"="True" "RenderType"="Transparent"}
        LOD 400



		Lighting On
        Cull off
        ZWrite Off
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
        sampler2D _MainTex, _CloudTex;
		float ls_time;
		float ls_cloudscale;
		float ls_cloudscattering;
		float ls_cloudintensity;
		float ls_cloudsharpness;
		float ls_shadowscale;
		float ls_distScale;
		float ls_cloudthickness;
		float3 ls_cloudcolor;
		
			uniform float3 v3Translate;		// The objects world pos
			uniform float3 v3LightPos;		// The direction vector to the light source
			uniform float3 v3InvWavelength; // 1 / pow(wavelength, 4) for the red, green, and blue channels
			uniform float fOuterRadius;		// The outer (atmosphere) radius
			uniform float fOuterRadius2;	// fOuterRadius^2
			uniform float fInnerRadius;		// The inner (planetary) radius
			uniform float fInnerRadius2;	// fInnerRadius^2
			uniform float fKrESun;			// Kr * ESun
			uniform float fKmESun;			// Km * ESun
			uniform float fKr4PI;			// Kr * 4 * PI
			uniform float fKm4PI;			// Km * 4 * PI
			uniform float fScale;			// 1 / (fOuterRadius - fInnerRadius)
			uniform float fScaleDepth;		// The scale depth (i.e. the altitude at which the atmosphere's average density is found)
			uniform float fScaleOverScaleDepth;	// fScale / fScaleDepth
			uniform float fHdrExposure;		// HDR exposure
			uniform float g;				// The Mie phase asymmetry factor
			uniform float g2;				// The Mie phase asymmetry factor squared
			uniform float cloudHeight;
			uniform float3 lightDir;
			
		float getCloud(float2 uv, float scale, float disp) {
					float y = 0.0f;
					// Perlin octaves
					int NN = 4;
					for(int i=0;i < NN; i++) {
						float k = scale*i  + 0.11934;
						y+= 1.0/pow(k,0.5)*tex2D( _CloudTex, k*uv + float2(0.1234*i*ls_time*0.015 - 0.04234*i*i*ls_time*0.015 + 0.9123559 + 0.23411*k , 0.31342  + 0.5923*i*i + disp) ).x;
						//y+= tex2D( _CloudTex, k*uv + float2(0.1234*i*ls_time*0.015 - 0.04234*i*i*ls_time*0.015 + 0.9123559 + 0.23411*k , 0.31342  + 0.5923*i*i + disp) ).x;
					}
					// Normalize
					y /= 0.5f*NN;
					return clamp( pow(ls_cloudscattering/y, ls_cloudsharpness),0,1.0);
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
              
             float scale(float fCos)
			{
				float x = 1.0 - fCos;
				return 0.25 * exp(-0.00287 + x*(0.459 + x*(3.83 + x*(-6.80 + x*5.25))));
			}
			
			const float fSamples = 3.0;

			
			void SkyFromSpace(float4 vert, out float3 c0, out float3 c1, out float3 t0) {
				float3 v3CameraPos = _WorldSpaceCameraPos - v3Translate;	// The camera's current position
				float fCameraHeight = length(v3CameraPos);					// The camera's current height
				float fCameraHeight2 = fCameraHeight*fCameraHeight;			// fCameraHeight^2
			
				// Get the ray from the camera to the vertex and its length (which is the far point of the ray passing through the atmosphere)
				float3 v3Pos = mul(_Object2World, vert).xyz - v3Translate;
				float3 v3Ray = v3Pos - v3CameraPos;
				float fFar = length(v3Ray);
				v3Ray /= fFar;
				
				// Calculate the closest intersection of the ray with the outer atmosphere (which is the near point of the ray passing through the atmosphere)
				float B = 2.0 * dot(v3CameraPos, v3Ray);
				float C = fCameraHeight2 - fOuterRadius2;
				float fDet = max(0.0, B*B - 4.0 * C);
				float fNear = 0.5 * (-B - sqrt(fDet));
				
				// Calculate the ray's start and end positions in the atmosphere, then calculate its scattering offset
				float3 v3Start = v3CameraPos + v3Ray * fNear;
				fFar -= fNear;
				float fStartAngle = dot(v3Ray, v3Start) / fOuterRadius;
				float fStartDepth = exp(-1.0/fScaleDepth);
				float fStartOffset = fStartDepth*scale(fStartAngle);
				
			
				// Initialize the scattering loop variables
				float fSampleLength = fFar / fSamples;
				float fScaledLength = fSampleLength * fScale;
				float3 v3SampleRay = v3Ray * fSampleLength;
				float3 v3SamplePoint = v3Start + v3SampleRay * 0.5;
			
				// Now loop through the sample rays
				float3 v3FrontColor = float3(0.0, 0.0, 0.0);
				for(int i=0; i<int(fSamples); i++)
				{
					float fHeight = length(v3SamplePoint);
					float fDepth = exp(fScaleOverScaleDepth * (fInnerRadius - fHeight));
					float fLightAngle = dot(v3LightPos, v3SamplePoint) / fHeight;
					float fCameraAngle = dot(v3Ray, v3SamplePoint) / fHeight;
					float fScatter = (fStartOffset + fDepth*(scale(fLightAngle) - scale(fCameraAngle)));
					float3 v3Attenuate = exp(-fScatter * (v3InvWavelength * fKr4PI + fKm4PI));
					v3FrontColor += v3Attenuate * (fDepth * fScaledLength);
					v3SamplePoint += v3SampleRay;
				}
			
				c0 = v3FrontColor * (v3InvWavelength * fKrESun);
				c1 = v3FrontColor * fKmESun;
				t0 = v3CameraPos - v3Pos;

			
			}
			
			
			void SkyFromAtm(float4 vert, out float3 c0, out float3 c1, out float3 t0) {
				float3 v3CameraPos = _WorldSpaceCameraPos - v3Translate; 	// The camera's current position
				float fCameraHeight = length(v3CameraPos);					// The camera's current height
				//float fCameraHeight2 = fCameraHeight*fCameraHeight;		// fCameraHeight^2
			
				// Get the ray from the camera to the vertex and its length (which is the far point of the ray passing through the atmosphere)
				float3 v3Pos = mul(_Object2World, vert).xyz - v3Translate;
				float3 v3Ray = v3Pos - v3CameraPos;
				float fFar = length(v3Ray);
				v3Ray /= fFar;
				
				// Calculate the ray's starting position, then calculate its scattering offset
				float3 v3Start = v3CameraPos;
				float fHeight = length(v3Start);
				float fDepth = exp(fScaleOverScaleDepth * (fInnerRadius - fCameraHeight));
				float fStartAngle = dot(v3Ray, v3Start) / fHeight;
				float fStartOffset = fDepth*scale(fStartAngle);
				
			
				// Initialize the scattering loop variables
				float fSampleLength = fFar / fSamples;
				float fScaledLength = fSampleLength * fScale;
				float3 v3SampleRay = v3Ray * fSampleLength;
				float3 v3SamplePoint = v3Start + v3SampleRay * 0.5;
				
				// Now loop through the sample rays
				float3 v3FrontColor = float3(0.0, 0.0, 0.0);
				for(int i=0; i<int(fSamples); i++)
				{
					float fHeight = length(v3SamplePoint);
					float fDepth = exp(fScaleOverScaleDepth * (fInnerRadius - fHeight));
					float fLightAngle = dot(v3LightPos, v3SamplePoint) / fHeight;
					float fCameraAngle = dot(v3Ray, v3SamplePoint) / fHeight;
					float fScatter = (fStartOffset + fDepth*(scale(fLightAngle) - scale(fCameraAngle)));
					float3 v3Attenuate = exp(-fScatter * (v3InvWavelength * fKr4PI + fKm4PI));
					v3FrontColor += v3Attenuate * (fDepth * fScaledLength);
					v3SamplePoint += v3SampleRay;
				}
				c0 = v3FrontColor * (v3InvWavelength * fKrESun);
				c1 = v3FrontColor * fKmESun;
				t0 = v3CameraPos - v3Pos;
			
			}
			
              
             v2f vert (appdata_base v)
             {
                 v2f o;
                 o.pos = mul( UNITY_MATRIX_MVP, v.vertex);
                 o.uv = v.texcoord;
                 o.normal = normalize(v.normal).xyz;
                 o.texcoord = v.texcoord;
 				 o.worldPosition = v.vertex;//mul (_Object2World, v.vertex).xyz;
 				float3 v3CameraPos = _WorldSpaceCameraPos - v3Translate;	// The camera's current position
				float fCameraHeight = length(v3CameraPos);					// The camera's current height

/*	   			if (fCameraHeight<fOuterRadius)
	    			SkyFromAtm(v.vertex, o.c0, o.c1, o.t0);
	    		else
	    			SkyFromSpace(v.vertex, o.c0, o.c1, o.t0);
*/	    	


                 TRANSFER_VERTEX_TO_FRAGMENT(o);
                 
                 return o;
             }
             			// Calculates the Mie phase function
			float getMiePhase(float fCos, float fCos2, float g, float g2)
			{
				return 1.5 * ((1.0 - g2) / (2.0 + g2)) * (1.0 + fCos2) / pow(1.0 + g2 - 2.0*g*fCos, 1.5);
			}

			// Calculates the Rayleigh phase function
			float getRayleighPhase(float fCos2)
			{
				return 0.75 + 0.75*fCos2;
			}

             
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
			float x = getNormal(newPos, 1.73252*ls_cloudscale*0.03791, 0.005*ls_shadowscale, N, 0.05*ls_shadowscale, worldSpacePosition.y/1381.1234f + ls_time*0.0002);//getCloud(IN.uv, 1.729134);
			float albedoColor = x*ls_cloudcolor;
			float3 norm= normalize(worldSpacePosition);
			N = normalize(N + norm);
			float globalLight = saturate(dot(norm, lightDir));
			//if (IN.normal.y<0) discard;
			float spec = pow(max(0.0, dot(
                  reflect(-lightDir, N), 
                  viewDirection)), 2);
//             spec = 0;
            float  NL = 0.4*ls_cloudintensity*(1 + spec + saturate((pow((dot(-N, lightDir)),1))));
            
			float4 m = tex2D(_MainTex, IN.uv.xy);

			
			float4 c;
			float t = 0.85;
		
//				float fCos = dot(v3LightPos, IN.t0) / length(IN.t0);
//				float fCos2 = fCos*fCos;
//				float3 col = getRayleighPhase(fCos2) * IN.c0*0 + getMiePhase(fCos, fCos2, g, g2) * IN.c1;
				//Adjust color from HDR
//				col = 1.0 - exp(col * -5);


			float3 v3CameraPos = _WorldSpaceCameraPos - v3Translate;	// The camera's current position
			float fCameraHeight = length(v3CameraPos);					// The camera's current height
			float col = 0;
			if (fCameraHeight > cloudHeight) {
				col = 0;
				
			}
			
			c.rgb=  (t*albedoColor + (1-t)*m.rgb*ls_cloudcolor)*NL*globalLight + col;
			t = 0.35;
			c.a = 0.5*clamp(5*ls_cloudthickness*pow(t*x + (1-t)*m.r,2)*dist,0,1);

//			c.rgb = col;
//			c.a = 1;
			return c;
             }
             ENDCG
         }
     }
 Fallback "Diffuse"
 }