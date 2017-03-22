Shader "LemonSpawn/Rings" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
	}
	SubShader {
		Tags { "RenderType"="Transparent" "Queue"="Transparent+100" }
		LOD 200
		Blend SrcAlpha OneMinusSrcAlpha
		cull off
		Zwrite off
 Pass {
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma vertex vert
      	#pragma fragment frag
        #include "UnityCG.cginc"

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;
		float4 _Color;
		float amplitude;
		float scale;
		float planetRadius;
		float radius1;
		float radius2;
		float3 lightDir;
     	
      struct v2f {
          float4 pos : POSITION;
        //  fixed4 color : COLOR;
          float2 uv : TEXCOORD0;
                  float4 opos: TEXCOORD1;

      };
      
      bool rayIntersectSphere(float3 d, float3 o, float r) {
		float A  = dot(d,d);	
		float B = 2.0*dot(d,o);
		float C = dot(o,o) - r*r;
		float D = B*B-4.0*A*C;
			if (D<0.0) return false;
	
		float t0 = (-B - sqrt(B*B - 4.0*A*C))/(2.0*A);
		if (t0<0.0)
			return false;
	
		return true;
}

      
      v2f vert (appdata_base v)
      {
          v2f o;
          o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
          o.opos = v.vertex;
        //  o.color.xyz = v.normal * 0.5 + 0.5;
        //  o.color.w = 1.0;
          o.uv = v.texcoord;//TRANSFORM_TEX (v.texcoord, _MainTex);
          return o;
      }
	float sstep(float a, float b, float x)
{
    float t = saturate((x - a)/(b - a));
    return t*t*(3.0 - (2.0*t));
}



      fixed4 frag (v2f i) : COLOR {
      	float sc = scale * 0.54;
//      	float3 lightDir = _WorldSpaceLightPos0;
      	float r = length(i.uv - float2(0.5,0.5));
		float4 c = 1.0;
      	c += tex2D( _MainTex, float2(r*sc*5, 0.9f) );
		c += tex2D(_MainTex, float2(r*sc*1.91, 0.61f))*1.2;
		c+= tex2D( _MainTex, float2(r*sc*2.91, 0.61f) )*1.1;
     	c+= tex2D( _MainTex, float2(r*sc*4.91, 0.912f) )*2.1;
      	c+= tex2D( _MainTex, float2(r*sc*8.91, 0.918f) )*1.2;
		c += tex2D(_MainTex, float2(r*sc*16.91, 0.918f))*1.3;
		c += tex2D(_MainTex, float2(r*sc*32.91, 0.918f))*1.5;
		c/=8;
	//	c = clamp(c, 0, 0.65)*1.15;

	//	float center = (radius1 + radius2) / 2;

//		c *= clamp(exp(1000*(pow((r - center)/center, 2))),0,1);




//		c = clamp(1 - c, 0, 1);

        if (r<radius1 || r>radius2)
        	discard;
	  //     c.a = 1;
	  	float4 col = _Color;
	  	float val = pow(c.x,2)-0.0;
	  	col.a = amplitude*(val*1.5 + 0.3);
	  	
	  	
	  	if (rayIntersectSphere(lightDir, i.opos.xyz*planetRadius, planetRadius*0.7))
	  		col.xyz*=0.25;
//	  	val = 1;
//	  	col = float4(i.opos.x, i.opos.y, i.opos.z,1)*100;
	  	
      	return col*val; 
      }
	 ENDCG

    	}
	}
}
