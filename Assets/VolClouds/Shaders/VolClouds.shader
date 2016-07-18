Shader "LemonSpawn/VolumetricClouds2" {

	Properties{
		_H("Height (from, to)", Vector) = (0.01, 0.016,0,0)
		_CloudScale ("Scale", Range (0, 1)) = 1
		_CloudIntensity ("Intensity", Range(0,3)) = 1.6
		_CloudColor("Color", Color) = (0.6, 0.75, 1, 1.0)
		_CloudDistance("Distance", Range(0,1)) = 0.11
		_Detail("Detail (performance speed)", Range(0,1)) = 0.10
		_MaxDetail("Max Detail (performance speed)", Range(0,1)) = 0.5
		_CloudSubtract("Subtract", Range(0,1)) = 0.55
		_CloudScattering("Scattering",Range(0,2)) = 1
		_CloudAlpha("Alpha", Range(0,1)) = 1
		_CloudLOD("Level of Detail", Range(0,12)) = 7
		_CloudShadowStrength("Shadow Strength", Range(0,1)) = 0.35
		_CloudShadowStep("Shadow Step", Range(0.1, 10)) = 5
	}

		SubShader{
		Tags{ "Queue" = "Transparent" "RenderType" = "Transparent" }
		LOD 400


		Lighting Off
		Cull off
		ZWrite on
		ZTest off
		Blend SrcAlpha OneMinusSrcAlpha
		Pass
	{

		Tags{ "LightMode" = "ForwardBase" }

		CGPROGRAM

#pragma target 4.0
#pragma fragmentoption ARB_precision_hint_fastest


#pragma vertex vert
#pragma fragment frag
#pragma multi_compile_fwdbase


#include "UnityCG.cginc"
#include "AutoLight.cginc"


	struct vertexInput {
		float4 vertex : POSITION;
		float4 texcoord : TEXCOORD0;
		float3 normal : NORMAL;
		float4 tangent : TANGENT;
	};

	struct v2f
	{
		float4 pos : SV_POSITION;
		float2 texcoord : TEXCOORD0;
		float3 normal : TEXCOORD1;
		float3 worldPosition : TEXCOORD2;
	};

	float4 _H;
	float4 _CloudColor;
	float _CloudScale;
	float _CloudIntensity;
	float _CloudDistance;
	float _Detail;
	float _MaxDetail;
	float _CloudSubtract;
	float _CloudScattering;
	float _CloudAlpha;
	float _CloudLOD;
	float _CloudShadowStrength;
	float _CloudShadowStep;


	v2f vert(vertexInput v)
	{
		v2f o;

		float4x4 modelMatrix = _Object2World;
		float4x4 modelMatrixInverse = _World2Object;
		o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
		o.normal = v.normal;
		o.worldPosition = mul(modelMatrix, v.vertex);
		return o;
	}

	inline float hh(float n)
	{
		return frac(sin(n)*653.52351823);
	}

	float noise(float3 x)
	{

		float3 p = floor(x);
		float3 f = frac(x);

		f = f*f*(3.0 - 2.0*f);
		float n = p.x + p.y*157.0 + 113.0*p.z;

		return lerp(lerp(lerp(hh(n + 0.0), hh(n + 1.0), f.x),
			lerp(hh(n + 157.0), hh(n + 158.0), f.x), f.y),
			lerp(lerp(hh(n + 113.0), hh(n + 114.0), f.x),
			lerp(hh(n + 270.0), hh(n + 271.0), f.x), f.y), f.z);


}


	inline float getNoise(float3 pos, in int N) {

		float3 p = (pos);
		float n = 0;// noise(p*3.123) * 0.2 - 0.2;;
		float ss = _CloudSubtract;
		float ms = 10;// 
		float3 shift= float3(0.123, 2.314, 0.6243);
		float A = 0;
	
		for (int i = 1; i < N; i++) {
			float f = pow(2, i)*1.0293;
			float amp = 1.0 / (2 * pow(i,_CloudScattering));
			n += noise(p*f*ms + shift*f) *amp;
			A += amp;
		}

		return clamp(n - ss*A, 0, 1);//*0.75;
	}



	float4 rayCast(float3 start, float3 end, float3 direction,  float stepLength, float3 lDir, float2 hSpan, float startIntensity, float3 skyColor, float3 camera, float light) {

		bool done = false;
		float3 pos = start;
		float3 dir = normalize(direction);
		float intensity = startIntensity;

		//int N = clamp(length(start - end) / stepLength,0,1000);
		int N = stepLength;
		float sl = clamp(length(end - start)/N,0, 10);
		int LOD = _CloudLOD;
		float scale = 0.001*_CloudScale;
		for (int i=0;i<N;i++)
			{
				if (pos.y > hSpan.x && pos.y < hSpan.y) 
				{
					intensity += (getNoise(pos*scale, LOD))*_CloudAlpha;// *(1 - ScaleHeight(hSpan, h, 0.1));
						
				}
				if (intensity > 1) {
					done = true;
				}
				if (!done)
					pos = pos + dir*sl;
				else
					break;
		}
		intensity*=1;
		//if (intensity>0.01)
		//	intensity = clamp(intensity*10, 0.9,1);

		float3 color = 1.0*skyColor*(light);
//		intensity = 1;
		
		if (intensity>0) 
		{
			float clear = 1.2;
			pos += lDir*sl*_CloudShadowStep*.5;//*0.01*i*i;
			for (int i = 0; i < 10; i++) {
				pos += lDir*sl * _CloudShadowStep*0.5*(0+1);//*0.01*i*i;
				//if (pos.y > 0.9*hSpan.x && pos.y < 1.1*hSpan.y) 
				{
					//clear *= 1-((getNoise(pos*scale,LOD))*_CloudShadowStrength);
					clear -= ((getNoise(pos*scale,LOD))*_CloudShadowStrength);
				}

			}
			color = clamp(color*clear, 0, 2);
		}
//		color = pow(color,0.5);
		return float4(color, intensity);
	}

	bool intersectPlane(in float3 n, in float3 p0, in float3 l0, in float3 l, out float t) 
	{ 
    	float denom = dot(n, l);
    	 
    	if (denom > 0) 
    	{ 
        	t = dot(p0 - l0, n) / denom;
        	return (t >= 0); 
  		} 
    	return false; 
	} 

	fixed4 frag(v2f IN) : COLOR{

	float4 c;
	

	float2 uv = IN.texcoord.xy;




	float3 lightDirection = normalize(_WorldSpaceLightPos0);

//	return float4(lightDirection, 1);
	float3 viewDirection = normalize(
		_WorldSpaceCameraPos - IN.worldPosition.xyz);


	float2 h = _H.xy;//float2(0.010, 0.016);


	float3 v3CameraPos = _WorldSpaceCameraPos;

	float t0, t1;
	viewDirection *= -1;

	float3 plane = float3(0,1,0);
	float3 plane1Pos = float3(0,1*h.x,0);
	float3 plane2Pos = float3(0,1*h.y,0);
	if (v3CameraPos.y<h.x) { // Below camera height
		if (intersectPlane(plane, plane1Pos, v3CameraPos, viewDirection,t0)) {
			if (intersectPlane(plane, plane2Pos, v3CameraPos, viewDirection,t1)) {

			}
			else discard;
		}
		else discard;
	}
	else
	if (v3CameraPos.y>=h.x && v3CameraPos.y<h.y) { // Inside
		t0 = 0;
		if (!intersectPlane(plane, plane2Pos, v3CameraPos, viewDirection,t1)) {
			if (!intersectPlane(plane, plane1Pos, v3CameraPos, viewDirection,t1));
				//discard;
		}
	}
	else
	if (v3CameraPos.y>=h.y) { // Inside
		if (!intersectPlane(plane, plane2Pos, v3CameraPos, viewDirection,t0)) {
			if (!intersectPlane(plane, plane1Pos, v3CameraPos, viewDirection,t1));
				//discard;
		}
	}


	t0*=0.8;
	t1*=1.2;
//	t1 = clamp(t1,0,20);

	float3 startPos = _WorldSpaceCameraPos + t0*viewDirection;
	float3 endPos = _WorldSpaceCameraPos + t1*viewDirection;
	float light = pow(clamp(dot(lightDirection, normalize(plane))+0.2, 0, 1),1);
//	light = 1;

	float detail = clamp(_Detail*100000.0/length(t0*viewDirection), 5, _MaxDetail*200);
	float dist = length(t0*viewDirection);

	float sub = clamp(_CloudDistance*dist*0.002, 0,1);
	if (sub<0.99)
		c = rayCast(startPos, endPos, viewDirection, detail, lightDirection, h, 0, _CloudColor.xyz*_CloudIntensity, _WorldSpaceCameraPos, light*0.75);
	c.a-=sub;

	return c;

	}
		ENDCG
	}
	}
		Fallback "Diffuse"
}