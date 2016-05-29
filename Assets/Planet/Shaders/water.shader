Shader "LemonSpawn/Water" {

	Properties{
		_Normal("Albedo", 2D) = "bump" {}
		_Scale("NormalScale", Float) = 100
		_Perlin("Distortion", 2D) = "white" {}
		_SunPow("SunPow", float) = 256
		}
		SubShader{
		//	    Tags {"Queue"="Transparent-1" "IgnoreProjector"="True" "RenderType"="Transparent"}
		//Tags{ "Queue" = "Transparent+11000" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
		Tags{ "Queue" = "Transparent+11000" "RenderType" = "Transparent" }
			LOD 400



		Lighting On
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
		sampler2D _Normal, _Perlin;
		uniform float _Scale, time;
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
	uniform float3 waterColor;

	uniform sampler2D _FresnelLookUp, _Map0, _Map1, _Map2, _Main;
	uniform float4 _GridSizes;
	uniform float3 _SunColor, _SunDir;
	uniform float _MaxLod, _LodFadeDist;


	float _SunPow;
	float3 _SeaColor;
	samplerCUBE _SkyBox;

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
		float4 uv : TEXCOORD2;
		float3 worldPosition : TEXCOORD3;
		    			float3 c0 : TEXCOORD4;
		    			float3 c1 : TEXCOORD5;
						float3 T: TEXCOORD6;
						float3 B: TEXCOORD7;


//		LIGHTING_COORDS(7,8)
	};


#define PI 3.141592653589793

	inline float2 RadialCoords(float3 a_coords)
	{
		float3 a_coords_n = normalize(a_coords);
		float lon = atan2(a_coords_n.z, a_coords_n.x);
		float lat = acos(a_coords_n.y);
		float2 sphereCoords = float2(lon, lat) * (1.0 / PI);
		return float2(sphereCoords.x * 0.5 + 0.5, 1 - sphereCoords.y);

	}

	float getPerturb(float2 uv, float scale, float disp) {
		float y = 0.0f;
		// Perlin octaves
		int NN = 4;
		for (int i = 0; i < NN; i++) {
			float k = scale*i + 0.11934;
			y += 1.0 / pow(k, 0.5)*tex2D(_Perlin, k*uv + float2(0.1234*i*time*0.015 - 0.04234*i*i*time*0.015 + 0.9123559 + 0.23411*k, 0.31342 + 0.5923*i*i + disp)).x;
			//y+= tex2D( _CloudTex, k*uv + float2(0.1234*i*ls_time*0.015 - 0.04234*i*i*ls_time*0.015 + 0.9123559 + 0.23411*k , 0.31342  + 0.5923*i*i + disp) ).x;
		}
		// Normalize
		y /= 0.5f*NN;
		return clamp(y, 0, 1.0);
	}
	float Fresnel(float3 V, float3 N)
	{
		float costhetai = abs(dot(V, N));
		return tex2D(_FresnelLookUp, float2(costhetai, 0.0)).a * 0.7; //looks better scaled down a little?
	}

	float3 Sun(float3 V, float3 N)
	{
		float3 H = normalize(V + _SunDir);
		return _SunColor * pow(abs(dot(H, N)), _SunPow);
	}

	float scale(float fCos)
	{
		float x = 1.0 - fCos;
		return 0.25 * exp(-0.00287 + x*(0.459 + x*(3.83 + x*(-6.80 + x*5.25))));
	}

	const float fSamples = 3.0;
	void AtmFromGround(float4 vert, out float3 c0, out float3 c1) {
		float3 v3CameraPos = _WorldSpaceCameraPos - v3Translate;	// The camera's current position
		float fCameraHeight = clamp(length(v3CameraPos), 0, 100000);					// The camera's current height
																						//float fCameraHeight2 = fCameraHeight*fCameraHeight;		// fCameraHeight^2

																						// Get the ray from the camera to the vertex and its length (which is the far point of the ray passing through the atmosphere)
		float3 v3Pos = mul(_Object2World, vert).xyz - v3Translate;
		float3 v3Ray = v3Pos - v3CameraPos;
		v3Pos = normalize(v3Pos);
		float fFar = length(v3Ray);
		v3Ray /= fFar;

		// Calculate the ray's starting position, then calculate its scattering offset
		float3 v3Start = v3CameraPos;
		float fDepth = exp((fInnerRadius - fCameraHeight) * (1.0 / fScaleDepth));
		float fCameraAngle = dot(-v3Ray, v3Pos);
		float fLightAngle = dot(v3LightPos, v3Pos);
		float fCameraScale = scale(fCameraAngle);
		float fLightScale = scale(fLightAngle);
		float fCameraOffset = fDepth*fCameraScale;
		float fTemp = (fLightScale + fCameraScale);

		float fSamples = 3.0;

		// Initialize the scattering loop variables
		float fSampleLength = fFar / fSamples;
		float fScaledLength = fSampleLength * fScale;
		float3 v3SampleRay = v3Ray * fSampleLength;
		float3 v3SamplePoint = v3Start + v3SampleRay * 0.5;

		// Now loop through the sample rays
		float3 v3FrontColor = float3(0.0, 0.0, 0.0);
		float3 v3Attenuate;
		for (int i = 0; i<int(fSamples); i++)
		{
			float fHeight = length(v3SamplePoint);
			float fDepth = exp(fScaleOverScaleDepth * (fInnerRadius - fHeight));
			float fScatter = fDepth*fTemp - fCameraOffset;
			v3Attenuate = exp(-fScatter * (v3InvWavelength * fKr4PI + fKm4PI));
			v3FrontColor += v3Attenuate * (fDepth * fScaledLength);
			v3SamplePoint += v3SampleRay;
		}


		c0 = v3FrontColor * (v3InvWavelength * fKrESun + fKmESun);
		c1 = v3Attenuate;// + v3InvWavelength;


	}



	void AtmFromSpace(float4 vert, out float3 c0, out float3 c1) {
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

		// Calculate the ray's starting position, then calculate its scattering offset
		float3 v3Start = v3CameraPos + v3Ray * fNear;
		fFar -= fNear;
		float fDepth = exp((fInnerRadius - fOuterRadius) / fScaleDepth);
		float fCameraAngle = dot(-v3Ray, v3Pos) / length(v3Pos);
		float fLightAngle = dot(v3LightPos, v3Pos) / length(v3Pos);
		float fCameraScale = scale(fCameraAngle);
		float fLightScale = scale(fLightAngle);
		float fCameraOffset = fDepth*fCameraScale;
		float fTemp = (fLightScale + fCameraScale);

		float fSamples = 3.0;

		// Initialize the scattering loop variables
		float fSampleLength = fFar / fSamples;
		float fScaledLength = fSampleLength * fScale;
		float3 v3SampleRay = v3Ray * fSampleLength;
		float3 v3SamplePoint = v3Start + v3SampleRay * 0.5;

		// Now loop through the sample rays
		float3 v3FrontColor = float3(0.0, 0.0, 0.0);
		float3 v3Attenuate;
		for (int i = 0; i<int(fSamples); i++)
		{
			float fHeight = length(v3SamplePoint);
			float fDepth = exp(fScaleOverScaleDepth * (fInnerRadius - fHeight));
			float fScatter = fDepth*fTemp - fCameraOffset;
			v3Attenuate = exp(-fScatter * (v3InvWavelength * fKr4PI + fKm4PI));
			v3FrontColor += v3Attenuate * (fDepth * fScaledLength);
			v3SamplePoint += v3SampleRay;
		}

		c0 = v3FrontColor * (v3InvWavelength * fKrESun + fKmESun);
		c1 = v3Attenuate;// + v3InvWavelength;


	}




	v2f vert(vertexInput v)
	{
		v2f o;



         float4x4 modelMatrix = _Object2World;
		float4x4 modelMatrixInverse = _World2Object;
/*
		output.posWorld = mul(modelMatrix, input.vertex);
		output.normalDir = normalize(
			mul(float4(input.normal, 0.0), modelMatrixInverse).xyz);
		output.tex = input.texcoord;
		output.pos = mul(UNITY_MATRIX_MVP, input.vertex);
		return output;
*/
		o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
		o.uv = v.texcoord;
		o.texcoord = v.texcoord;
		o.worldPosition = mul(modelMatrix, v.vertex);
		float3 v3CameraPos = _WorldSpaceCameraPos - v3Translate;	// The camera's current position
		float fCameraHeight = length(v3CameraPos);					// The camera's current height

		if (fCameraHeight<fOuterRadius)
		  AtmFromGround(v.vertex, o.c0, o.c1);
		else
		  AtmFromSpace(v.vertex, o.c0, o.c1);
		  
		o.T = normalize(
			mul(modelMatrix, float4(v.tangent.xyz, 0.0)).xyz);
		o.normal = normalize(
			mul(float4(v.normal, 0.0), modelMatrixInverse).xyz);
		o.B = normalize(
			cross(o.normal, o.T)
			* v.tangent.w); // tangent.w is specific to Unity


		float2 uv = RadialCoords(normalize(v.vertex.xyz)) * 200;
		o.texcoord.xy = uv;

		float dist = clamp(distance(_WorldSpaceCameraPos.xyz, o.pos) / _LodFadeDist, 0.0, 1.0);
		float lod = _MaxLod * dist;
		lod = 0;
		float ht = 0.0;
		ht += tex2Dlod(_Map0, float4(uv, 0, lod)*1).x;
		ht += tex2Dlod(_Map0, float4(uv, 0, lod)*1).y;
//		ht += tex2Dlod(_Map0, o.pos *0.1).x;
//		ht += tex2Dlod(_Map0, o.pos * 0.01).y;
		//	ht += tex2D(_Map0, uv).x;
	//	ht += tex2D(_Map0, uv).y;
		//ht += tex2Dlod(_Map0, float4(worldPos.xz/_GridSizes.z, 0, lod)).z;
		//ht += tex2Dlod(_Map0, float4(worldPos.xz/_GridSizes.w, 0, lod)).w;
//		o.normal = normalize(o.pos);
		o.pos = mul(UNITY_MATRIX_MVP, float4(v.vertex.xyz + o.normal*(ht*10.2), v.vertex.w));
		//o.pos =+ float4(0, 1000, 0,0);
		
//		o.vertex.x = 0;


		//TRANSFER_VERTEX_TO_FRAGMENT(o);

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


	fixed4 frag(v2f IN) : COLOR{

		float4 c;
	float3 specularReflection;

		float3 lightDirection =
			normalize(_WorldSpaceLightPos0.xyz);

		float3 viewDirection = normalize(
			_WorldSpaceCameraPos - IN.worldPosition.xyz);


		float3 w = normalize(IN.normal);

//		float2 uv = RadialCoords(w) * 2500;
		float2 uv = IN.texcoord.xy*10;
		//			float2 uv = float2(0.5 + atan2(w.z, w.x)/(2*3.14159), 0.5 - asin(w.y)/3.14159);
		//uv = float2(w.x, w.y);			//uv = IN.uv_MainTex*100;
		//			float2 uv = IN.



		float2 slope = float2(0, 0);

		slope += tex2D(_Map1, uv).xy;
		slope += tex2D(_Map1, uv).zw;
		slope += tex2D(_Map2, uv).xy;
		slope += tex2D(_Map2, uv).zw;

		float3 N = normalize(float3(-slope.x, 2.0, -slope.y)); //shallow normal
		float3 N2 = normalize(float3(-slope.x, 0.5, -slope.y)); //sharp normal

		float3 V = normalize(_WorldSpaceCameraPos - IN.pos);

		float fresnel = Fresnel(V, N);

		float3 normal = IN.normal;// N.xzy;

		//float light = max(0, dot(normal, lightDirection));


		float3x3 local2WorldTranspose = float3x3(
			IN.T,
			IN.B,
			normal);
		float3 normalDirection =

			normalize(mul(N.xyz, local2WorldTranspose));


		normalDirection = normalize(IN.normal + 0.5*N.xyz);

		specularReflection = float3(1, 1, 1)
			* pow(max(0.0, dot(

				reflect(-lightDirection, normalDirection),
				viewDirection)), 50);

		float light = max(0, dot(normalDirection, lightDirection));


		//			float3 skyColor = texCUBE(_SkyBox, WorldReflectionVector(IN, o.Normal)*float3(-1,1,1)).rgb;//flip x
		float3 skyColor = float3(2, 0.7, 0.4)*0.1;

		float3 wc = lerp(waterColor, skyColor, fresnel) + Sun(V, N2);


		c.rgb = (IN.c0 + wc*light + 0.05*IN.c1);



		return float4(c.rgb
			+ specularReflection, 0.75 + specularReflection.b);
		
	}
		ENDCG
	}
	}
		Fallback "Diffuse"
}