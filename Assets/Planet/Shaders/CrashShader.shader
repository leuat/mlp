Shader "LemonSpawn/CrashShader" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}

SubShader {
	    //Tags {"IgnoreProjector"="True"}
        LOD 400
		Tags{ "RenderType" = "Opaque" }



		Lighting Off
        Cull Off
        ZWrite Off
        ZTest off
//        Blend one zero
	   Blend off
       Pass
         {

             CGPROGRAM
		    
             #pragma target 3.0
             #pragma fragmentoption ARB_precision_hint_fastest
             
             #pragma vertex vert
             #pragma fragment frag
//             #pragma multi_compile_fwdbase
             
           //  #include "AutoLight.cginc"
             #include "UnityCG.cginc"
			 #include "Include/IQnoise.cginc" 

			 sampler2D _MainTex;
    		
             struct v2f
             {
                 float4 pos : POSITION;
                 float4 texcoord : TEXCOORD0;
                 float3 normal : TEXCOORD1;
                 float2 uv : TEXCOORD2;
				 float4 color : TEXCOORD3;


             };
              
             v2f vert (appdata_full v)
             {
                 v2f o;

                 o.pos = mul( UNITY_MATRIX_MVP, v.vertex);
                 o.uv = v.texcoord;
                 o.normal = normalize(v.normal).xyz;
                 o.texcoord = v.texcoord;
 	//			 o.worldPosition = mul (unity_ObjectToWorld, v.vertex).xyz;
			     o.color =v.color;
                 return o;
             }

			fixed4 frag(v2f IN) : COLOR {
				float4 c = float4(1,1,1,1)*tex2D(_MainTex, IN.uv*42.2321*_Time);
				c.a = 1;
			c = float4(1,0,0,1);

			return c;// *attenuation;
			
             }
             ENDCG
         }
	}	FallBack "Diffuse"
}
