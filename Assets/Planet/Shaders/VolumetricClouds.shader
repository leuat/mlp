Shader "LemonSpawn/VolumetricClouds" {

	Properties{
		_MainTex("Base (RGB)", 2D) = "white" {}

	}

		SubShader{
		Tags{ "Queue" = "Transparent+11000" "RenderType" = "Transparent" }
		LOD 400


		Lighting Off
		Cull off
		ZWrite off
		ZTest on
		Blend SrcAlpha OneMinusSrcAlpha
		Pass
	{

		Tags{ "LightMode" = "ForwardBase" }

		CGPROGRAM
		// Upgrade NOTE: excluded shader from DX11 and Xbox360; has structs without semantics (struct v2f members worldPosition)

#pragma target 3.0
#pragma fragmentoption ARB_precision_hint_fastest

#pragma enable_d3d11_debug_symbols

#pragma vertex vert
#pragma fragment frag
#pragma multi_compile_fwdbase

#include "UnityCG.cginc"
#include "AutoLight.cginc"
#include "Include/Utility.cginc"
#include "Include/Atmosphere.cginc"


		struct vertexInput {
		float4 vertex : POSITION;
		float4 texcoord : TEXCOORD0;
		float3 normal : NORMAL;
		float4 tangent : TANGENT;
	};

	struct v2f
	{
		//float4 vpos : SV_POSITION;
		float4 pos : SV_POSITION;
		float4 texcoord : TEXCOORD0;
		float3 normal : TEXCOORD1;
		float3 worldPosition : TEXCOORD2;
		float3 c0 : TEXCOORD3;
		float3 c1 : TEXCOORD4;
	};

	sampler2D _BumpMap, _MainTex;
	float _Scale;
	float _Alpha;
	float _Glossiness;
	float4 _Color;
	float4 _SpecColor;
	uniform float sradius;

	v2f vert(vertexInput v)
	{
		v2f o;

		float4x4 modelMatrix = _Object2World;
		float4x4 modelMatrixInverse = _World2Object;
		o.pos = mul(UNITY_MATRIX_MVP, v.vertex);

		o.texcoord = v.texcoord;
		o.worldPosition = mul(modelMatrix, v.vertex);
		o.normal = v.normal;
//		float3 ground = normalize(v.vertex.xyz)*(fInnerRadius*1.1);
//		float4 ground = normalize(v.vertex)*(fInnerRadius*1.5);
		float4 ground = v.vertex;
		ground.xyz = (ground.xyz)/sradius*(fInnerRadius*1.01);
		//ground = (ground) / sradius*(fInnerRadius*1.05);
		getGroundAtmosphere(ground, o.c0, o.c1);
		//		getGroundAtmosphere(v.vertex*1, o.c0, o.c1);
//		getAtmosphere(v.vertex * 1, o.c0, o.c1,t);

/*		float3 v3CameraPos = _WorldSpaceCameraPos - v3Translate;	// The camera's current position
		float fCameraHeight = length(v3CameraPos);					// The camera's current height
		AtmFromGround(v.vertex, o.c0, o.c1);
		*/
		//o.c0 = float3(1, 0, 0);


		return o;
	}


	inline float getNoiseOld(float3 pos) {
		float3 p = (pos-v3Translate) / fInnerRadius;
		float ss = 0.9;
		float n = noise(p*13);
		//if (n - ss < 0)
	//		return 0;
		n += noise(p*22.324)*0.5;
	//	if (n - ss < 0)
//			return 0;
		n += noise(p*52.324)*0.25;
	//	if (n - ss < 0)
	//		return 0;
		n += noise(p*152.324)*0.25;
//		n += noise(p*752.324)*0.05;

//		n /= 2;
		return clamp(n - ss,0,1)*0.02;
	}

	inline float getNoise(float3 pos) {
		float3 p = (pos - v3Translate) / fInnerRadius;
		float ss = 1.2;
		float n = noise(p * 13);
		n += noise(p*22.324)*0.5;
		n += noise(p*52.324)*0.45;
		n += noise(p*152.324)*0.25;
		n += noise(p*312.324)*0.15;
		n += noise(p*552.324)*0.10;
		return clamp(n - ss, 0, 1)*0.02;
	}


	float getHeightFromPosition(float3 p) {
		return (length(p - v3Translate) - fInnerRadius) / fInnerRadius;// - liquidThreshold;
	}

	float getRadiusFromHeight(float h) {
		return (h*fInnerRadius + fInnerRadius);
	
	}


	float4 rayCast(float3 start, float3 direction, float2 hSpan, float stepLength, float3 end, float3 lDir) {

		bool done = false;
		float3 pos = start;
		float3 dir = normalize(direction);
		float intensity = 0;
		float sl = stepLength;
		while (!done) {
			float h = getHeightFromPosition(pos);


			if (h > hSpan.x && h < hSpan.y) {
				intensity += (getNoise(pos));
				if (intensity > 0.05)
					sl = stepLength*0.5;
				else 
					sl = stepLength*2;
			}
			if (h > hSpan.y)
				done = true;

			if (intensity > 0.65) {
				done = true;
			}
			//intensity += 0.01;
			if (!done)
				pos = pos + dir*sl;

		}


//		float3 color = float3(0.0, 0.0, 0.0);
		float3 color = float3(1, 1, 1);
		
		
		if (intensity>0) 
		{
			float clear = 1.05;
			pos += lDir*stepLength * 5;//*0.01*i*i;

			float s = 2;

			for (int i = 0; i < 10*s; i++) {
				pos += lDir*stepLength * 10/s;//*0.01*i*i;
				float h = getHeightFromPosition(pos);
				if (h > hSpan.x && h < hSpan.y) {
					clear -= getNoise(pos)*1.5/s;
				}
				color = clamp(color*clear, 0, 2);
			}
		}
		return float4(color, intensity);
	}

	fixed4 frag(v2f IN) : COLOR{

	float4 c;


	float2 uv = IN.texcoord.xy;



	float3 lightDirection = normalize(v3LightPos);
//		normalize(_WorldSpaceLightPos0.xyz);


	float3 viewDirection = normalize(
		_WorldSpaceCameraPos - IN.worldPosition.xyz);

	float light = clamp(dot(lightDirection, IN.normal),0,1);

//	float2 h = float2(0.025, 0.030 + clamp(noise((IN.worldPosition.xyz - v3Translate )/fInnerRadius*16.23)*0.1 - 0.05,0,1));
	float2 h = float2(0.015, 0.033);


//	float startRadius = fInnerRadius*1.01;//getRadiusFromHeight(h.x);
	float startRadius = getRadiusFromHeight(h.x);
	float t0=0, t1=0;
	float3 v3CameraPos = _WorldSpaceCameraPos - v3Translate;
//	viewDirection *= -1;
	if (intersectSphere(float4(v3Translate * 0, startRadius), v3CameraPos, viewDirection, 250000, t0, t1)) {
		float3 pos = _WorldSpaceCameraPos + t1*viewDirection;
//		c = rayCast(pos, viewDirection, h, clamp(abs(10 * t1*0.005), 2, 250), IN.worldPosition, lightDirection);
		c = rayCast(pos, viewDirection, h, 10, IN.worldPosition, lightDirection);
	}
	else
		discard;



	c.rgb = 1*(groundColor(IN.c0, IN.c1, c.rgb*light, IN.worldPosition, 0.1)) + c.rgb*0.5*light;
	

	return c;

	}
		ENDCG
	}
	}
		Fallback "Diffuse"
}