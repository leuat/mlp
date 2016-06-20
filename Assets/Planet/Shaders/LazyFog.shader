// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "LemonSpawn/LazyFog" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Scale ("Scale", Range(0,5)) = 1
		_Intensity ("Intensity", Range(0,1)) = 0.5
		_Alpha ("Alpha", Range(0,2.5)) = 0.75
		_AlphaSub ("AlphaSub", Range(0,1)) = 0.0
		_Pow ("Pow", Range(0,4)) = 1.0
	}

SubShader {
	    Tags {"Queue"="Transparent+10001" "IgnoreProjector"="True" "RenderType"="Transparent"}
        LOD 400


		Lighting On
        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
       Pass
         {

			 Tags{ "LightMode" = "ForwardBase" }
             CGPROGRAM
		    
             #pragma target 3.0
             #pragma fragmentoption ARB_precision_hint_fastest
             
             #pragma vertex vert
             #pragma fragment frag
//             #pragma multi_compile_fwdbase
             
           //  #include "AutoLight.cginc"
             #include "UnityCG.cginc"
			 #include "Include/Atmosphere.cginc" 

             sampler2D _MainTex;
			 float4 _Color;		
			 float _Scale;
			 float _Intensity;
			 float _Alpha;
			 float _AlphaSub;
    		 float _Pow;
			 
			 uniform float3 centerPos;
    		
             struct v2f
             {
                 float4 pos : POSITION;
                 float4 texcoord : TEXCOORD0;
                 float3 normal : TEXCOORD1;
                 float2 uv : TEXCOORD2;
                 float3 worldPosition: TEXCOORD3;
				 float3 lightWorld : TEXCOORD4;
//				 LIGHTING_COORDS(6, 7)
				 float3 c0 : TEXCOORD5;
				 float3 c1 : TEXCOORD6;
				 float4 color : TEXCOORD7;


             };
              
             v2f vert (appdata_full v)
             {
                 v2f o;

                 o.pos = mul( UNITY_MATRIX_MVP, v.vertex);
                 o.uv = v.texcoord;
                 o.normal = normalize(v.normal).xyz;
                 o.texcoord = v.texcoord;
 				 o.worldPosition = mul (_Object2World, v.vertex).xyz;
			     o.color =v.color;
				 o.lightWorld = normalize(ObjSpaceLightDir(v.vertex));
  //               TRANSFER_VERTEX_TO_FRAGMENT(o);
				 float3 t0;
				 //			 getAtmosphere(v.vertex, o.c0, o.c1, t0);
				 getGroundAtmosphere(v.vertex, o.c0, o.c1);

                 return o;
             }

		fixed4 frag(v2f IN) : COLOR {

			float3 worldSpacePosition = IN.worldPosition;
			float3 N;
			float3 distScale = 0.5;		
			float3 viewDirection = normalize(_WorldSpaceCameraPos - worldSpacePosition);
			float dist = clamp(pow(length(0.1*distScale*(_WorldSpaceCameraPos - worldSpacePosition)),1.0),0,1);
			float3 lightDir = IN.lightWorld;		
			float scale = 5;
			float shadowscale = 1;
//			float attenuation = LIGHT_ATTENUATION(i);
			float4 c = tex2D(_MainTex, IN.uv*_Scale);
			float xx = c.r*_Intensity;
			xx = pow(xx,_Pow);
			c.a = c.r;

			float2 uv2 = IN.uv - float2(0.5, 0.5);

//			float light = clamp(dot(lightDir, normalize(float3(uv2.x, uv2.y, uv2.y*0)))+1.0,0,1);
			float light = (0.5 + 0.5*clamp(dot(IN.normal, lightDir),0,1));
			c.rgb = float3(xx*_Color.r, xx*_Color.g, xx*_Color.b);// *light;
			c.a *= IN.color.a -2.5*length(uv2);
			c.a*=_Alpha;
			c.a-=_AlphaSub;
			//c.a = 0.5;
//			distScale = 1;;

//			float d = 0.2;
	//		c.rgb += float3(d, d, d);
		
			float3 r = c.rgb;
			c.rgb = groundColor(IN.c0, IN.c1, c.rgb, IN.worldPosition, 0.45);
//			c.rgb += 0.3*r;
			float h = (length(worldSpacePosition.xyz - v3Translate) - fInnerRadius) / fInnerRadius;// - liquidThreshold;


			return c;// *attenuation;
			
             }
             ENDCG
         }
	}
			Fallback "VertexLit"
 }