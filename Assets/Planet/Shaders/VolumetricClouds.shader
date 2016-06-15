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
		ZTest off
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
		float depth : TEXCOORD5;
		float4 projPos : TEXCOORD6;
	};

	sampler2D _BumpMap, _MainTex;
	float _Scale;
	float _Alpha;
	float _Glossiness;
	float4 _Color;
	float4 _SpecColor;
	uniform float sradius;
	uniform sampler2D _CameraDepthTexture;

	v2f vert(vertexInput v)
	{
		v2f o;

		float4x4 modelMatrix = _Object2World;
		float4x4 modelMatrixInverse = _World2Object;
		o.pos = mul(UNITY_MATRIX_MVP, v.vertex);

		o.texcoord = v.texcoord;
		o.normal = v.normal;
//		float3 ground = normalize(v.vertex.xyz)*(fInnerRadius*1.1);
//		float4 ground = normalize(v.vertex)*(fInnerRadius*1.5);
		float4 ground = v.vertex;
		ground.xyz = float3(1,1,1)*(fInnerRadius*1.11);
		//ground = (ground) / sradius*(fInnerRadius*1.05);
		getGroundAtmosphere(ground, o.c0, o.c1);
		//ground.w = v.vertex.w;
		o.worldPosition = mul(modelMatrix, v.vertex);

		//		getGroundAtmosphere(v.vertex*1, o.c0, o.c1);
//		getAtmosphere(v.vertex * 1, o.c0, o.c1,t);

/*		float3 v3CameraPos = _WorldSpaceCameraPos - v3Translate;	// The camera's current position
		float fCameraHeight = length(v3CameraPos);					// The camera's current height
		AtmFromGround(v.vertex, o.c0, o.c1);
		*/
		//o.c0 = float3(1, 0, 0);
		o.projPos = ComputeScreenPos(o.pos);
		UNITY_TRANSFER_DEPTH(o.depth);
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
		float ss = 0.9;
		float n = noise(p * 13);
		n += noise(p*22.324)*0.5;
		n += noise(p*52.324)*0.45;
		n += noise(p*152.324)*0.25;
//		n += noise(p*312.324)*0.15;
//		n += noise(p*552.324)*0.10;
		return clamp(n - ss, 0, 1)*0.02;
	}


	float getHeightFromPosition(float3 p) {
		return (length(p - v3Translate) - fInnerRadius) / fInnerRadius;// - liquidThreshold;
	}

	float getRadiusFromHeight(float h) {
		return (h*fInnerRadius + fInnerRadius);
	
	}

		

	void intersectTwoSpheres(in float3 center, in float3 o, in float3 ray, in float innerRadius, in float outerRadius, out float2 t0, out float2 t1, out bool inner, out bool outer, out bool inside) {
		float outer_t0, outer_t1;
		float inner_t0, inner_t1;
		inside = false;
		t0 = float2(0, 0);
		t1 = float2(0, 0);

		float currentRadius = length(o - center);

		bool outerIntersects = intersectSphere(float4(center, outerRadius), o, ray, 250000, outer_t0, outer_t1);
		bool innerIntersects = intersectSphere(float4(center, innerRadius), o, ray, 250000, inner_t0, inner_t1);

		if (currentRadius < outerRadius) {
			swap(outer_t0, outer_t1);
		}

		if (currentRadius < innerRadius) {
			swap(inner_t0, inner_t1);
			inside = true;

		}


		outer = true;
		inner = true;

		if (!outerIntersects || outer_t0 < 0)
			outer = false;


		if (!innerIntersects || inner_t0 < 0)
			inner = false;

		t0.x = outer_t0;
		t0.y = inner_t0;

		// Only outer edge
		if (outer && !inner) {
			t0.x = outer_t0;
			t0.y = outer_t1;
		}

		if (currentRadius >= innerRadius && currentRadius < outerRadius) {
//			inside = true;
			if (outer && !inner) {
				t0.x = 0;
				t0.y = outer_t0;
			}
			else
			{
				// Intersect with planet
				float p0, p1;
				t0.x = 0;
				t0.y = outer_t0;

				if (intersectSphere(float4(center, getRadiusFromHeight(0.005)), o, ray, 250000, p0, p1))
					t0.y = p0;


//				t1.x = inner_t1;
	//			t1.y = outer_t0;

			}
			
		}

		if (currentRadius < innerRadius) {
			t0.x = inner_t0;
			t0.y = outer_t0;

		}

	}

	float4 rayCast(float3 start, float3 end, float3 direction,  float stepLength, float3 lDir, float2 hSpan, bool inside) {

		bool done = false;
		float3 pos = start;
		float3 dir = normalize(direction);
		float intensity = 0;
		float sl = stepLength;
		int N = length(start - end) / stepLength;

		for (int i=0;i<N;i++)
			{

				float h = getHeightFromPosition(pos);

				if (h > hSpan.x && h < hSpan.y) 
				{
					intensity += (getNoise(pos))*2;
/*					if (intensity > 0.05)
						sl = stepLength*0.5;
					else
						sl = stepLength * 2;
						*/
				}
			if (intensity > 0.75) {
				done = true;
			}
			if (h > hSpan.y && inside)
				done = true;

			if (!done)
				pos = pos + dir*sl;
			else
				break;
		}


//		float3 color = float3(0.0, 0.0, 0.0);
		float3 color = float3(1, 1, 1);
		
		
		if (intensity>0 && 1==1) 
		{
			float clear = 1.05;
			pos += lDir*stepLength * 5;//*0.01*i*i;

			float s = 2;

			for (int i = 0; i < 10*s; i++) {
				pos += lDir*stepLength * 10/s;//*0.01*i*i;
				clear -= getNoise(pos)*1.5/s;
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


//	float2 h = float2(0.025, 0.030 + clamp(noise((IN.worldPosition.xyz - v3Translate )/fInnerRadius*16.23)*0.1 - 0.05,0,1));
	float2 h = float2(0.015, 0.033);


	float startRadius = getRadiusFromHeight(h.x);
	float endRadius = getRadiusFromHeight(h.y*1.5);
	float2 t0 = 0;
	float2 t1=0;
	bool inner, outer, inside;
	float3 v3CameraPos = _WorldSpaceCameraPos - v3Translate;

	float3 normal, worldPos;
	viewDirection *= -1;

	//if (intersectSphere(float4(center, getRadiusFromHeight(0.005)), o, ray, 250000, p0, p1))
	if (intersectSphere(float4(float3(0,0,0), endRadius), v3CameraPos, viewDirection, 250000,t0.x, t0.y)) {
		normal = normalize(v3CameraPos + viewDirection*t0.x);
		worldPos = (v3CameraPos + viewDirection*t0.x);
	}
	float light = clamp(dot(lightDirection, normal), 0, 1);


	c.a = 1;
	intersectTwoSpheres(float3(0, 0, 0), v3CameraPos, viewDirection*1, startRadius, endRadius, t0, t1, inner, outer, inside);
	/*	if (outer)
		c.rgb = float3(0, 0, 1);
	
	if (inner)
		c.rgb += float3(0, 1, 0);
	*/	
	if (inner == false && outer == false)
		discard;

	float depth = Linear01Depth(tex2Dproj(_CameraDepthTexture,
		UNITY_PROJ_COORD(IN.projPos)).r);

	if (inside) {
		if (depth < 0.90)
			discard;
	}

	
	if (outer || inner) {
		float3 startPos = _WorldSpaceCameraPos + t0.x*viewDirection;
		float3 endPos = _WorldSpaceCameraPos + t0.y*viewDirection;
		c = rayCast(startPos, endPos, viewDirection, 10, lightDirection, h, inside);
	}
//	c.rgb = 1*(groundColor(IN.c0, IN.c1, c.rgb*light, worldPos, 1000)) + c.rgb*0.5*light;
	c.rgb = atmColor(IN.c0, IN.c1) + c.rgb*light;

	//c.rgb = c.rgb;
	return c;

	}
		ENDCG
	}
	}
		Fallback "Diffuse"
}