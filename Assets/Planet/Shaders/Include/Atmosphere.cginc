// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

uniform float3 v3Translate;		// The objects world pos
uniform float3 v3LightPos;		// The direction vector to the light source
uniform float3 v3InvWavelength; // 1 / pow(wavelength, 4) for the red, green, and blue channels
uniform float fOuterRadius;		// The outer (atmosphere) radius
uniform float fOuterRadius2;	// fOuterRadius^2
uniform float fInnerRadius;		// The inner (planetary) radius
uniform float fInnerRadius2;	// fInnerRadius^2
uniform float fKrESun;			// Kr * ESun
uniform float fKmESun;			// Km * ESun
uniform float g, g2;
uniform float fKr4PI;			// Kr * 4 * PI
uniform float fKm4PI;			// Km * 4 * PI
uniform float fScale;			// 1 / (fOuterRadius - fInnerRadius)
uniform float fScaleDepth;		// The scale depth (i.e. the altitude at which the atmosphere's average density is found)
uniform float fScaleOverScaleDepth;	// fScale / fScaleDepth
uniform float fHdrExposure;		// HDR exposure
uniform float3 basinColor, topColor, middleColor, middleColor2, basinColor2, waterColor, hillColor;
uniform float liquidThreshold, atmosphereDensity, topThreshold, basinThreshold;
uniform float fade = 0.2;
uniform float time;
uniform float metallicity;
uniform float cloudRadius;
uniform float3 lightDir;

#ifndef PI
#define PI 3.14159265358979323846264338327
#endif
sampler2D _IQ;

float scale(float fCos)
{
	float x = 1.0 - fCos;
	return fScaleDepth * exp(-0.00287 + x*(0.459 + x*(3.83 + x*(-6.80 + x*5.25))));
}

// Calculates the Rayleigh phase function
float getRayleighPhase(float fCos2)
{
	return 0.75 + 0.75*fCos2;
}

float2 pos2uv(in float3 p) {
	//p = normalize(p);
	return float2(0.5 + atan2(p.z, p.x) / (2.0 * PI), 0.5 - asin(p.y)/PI);
}


bool intersectSphere(in float4 sp, in float3 ro, inout float3 rd, in float tm, out float t1, out float t2)
{
//	bool flip = false;
	if (length(sp.xyz - ro) < sp.w) {
		//rd *= -1;
	//	flip = true;
	}

	bool  r = false;
	float3  d = ro - sp.xyz;
	float b = dot(rd, d);
	float c = dot(d, d) - sp.w*sp.w;
	float t = b*b - c;

	if (t > 0.0)
	{
			t1 = (-b - sqrt(t));
			t2 = (-b + sqrt(t));
		return true;
	}
	
	return false;



}

inline void swap(inout float a, inout float b) {
	float tt = a;
	a = b;
	b = tt;
}





void AtmFromGround(float4 vert, out float3 c0, out float3 c1, float3 camPos) {
	float3 v3CameraPos = camPos - v3Translate;	// The camera's current position
																					//float fCameraHeight2 = fCameraHeight*fCameraHeight;		// fCameraHeight^2
																					// Get the ray from the camera to the vertex and its length (which is the far point of the ray passing through the atmosphere)
	float3 v3Pos = mul(unity_ObjectToWorld, vert).xyz - v3Translate;
//	float fCameraHeight = clamp(length(v3CameraPos), length(v3Pos) , 1000000);					// The camera's current height
	float fCameraHeight = clamp(length(v3CameraPos), length(v3Pos)*0, 1000000);					// The camera's current height

	float3 v3Ray = v3Pos - v3CameraPos;
	v3Pos = normalize(v3Pos);
	float fFar = length(v3Ray);
	v3Ray /= fFar;

	// Calculate the ray's starting position, then calculate its scattering offset
	float3 v3Start = v3CameraPos;
	float fDepth = exp(clamp(fInnerRadius*1.0 - fCameraHeight,-10,0) * (1.0 / fScaleDepth));

	//float fCameraAngle = clamp(dot(-v3Ray, v3Pos),-1,1);
	float fLightAngle = clamp(dot(v3LightPos, v3Pos),-1,1);

	float fCameraAngle = clamp(dot(-v3Ray, v3Pos), 0.0, 1);

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
//	c0 = float3(1, 1, 0);

}



void AtmFromSpace(float4 vert, out float3 c0, out float3 c1) {
	float3 v3CameraPos = _WorldSpaceCameraPos - v3Translate;	// The camera's current position
	float fCameraHeight = length(v3CameraPos);					// The camera's current height
	float fCameraHeight2 = fCameraHeight*fCameraHeight;			// fCameraHeight^2

																// Get the ray from the camera to the vertex and its length (which is the far point of the ray passing through the atmosphere)
	float3 v3Pos = mul(unity_ObjectToWorld, vert).xyz - v3Translate;
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

	c0 = v3FrontColor *(v3InvWavelength * fKrESun + fKmESun);
	c1 = v3Attenuate;// + v3InvWavelength;


}

void SkyFromSpace(float4 vert, out float3 c0, out float3 c1, out float3 t0) {
	float3 v3CameraPos = _WorldSpaceCameraPos - v3Translate;	// The camera's current position
	float fCameraHeight = length(v3CameraPos);					// The camera's current height
	float fCameraHeight2 = fCameraHeight*fCameraHeight;			// fCameraHeight^2
	float fSamples = 3.0;

	// Get the ray from the camera to the vertex and its length (which is the far point of the ray passing through the atmosphere)
	float3 v3Pos = mul(unity_ObjectToWorld, vert).xyz - v3Translate;
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
	float fStartDepth = exp(-1.0 / fScaleDepth);
	float fStartOffset = fStartDepth*scale(fStartAngle);


	// Initialize the scattering loop variables
	float fSampleLength = fFar / fSamples;
	float fScaledLength = fSampleLength * fScale;
	float3 v3SampleRay = v3Ray * fSampleLength;
	float3 v3SamplePoint = v3Start + v3SampleRay * 0.5;
	// Now loop through the sample rays
	float3 v3FrontColor = float3(0.0, 0.0, 0.0);
	for (int i = 0; i<int(fSamples); i++)
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

	float fSamples = 3.0;
	// Get the ray from the camera to the vertex and its length (which is the far point of the ray passing through the atmosphere)
	float3 v3Pos = mul(unity_ObjectToWorld, vert).xyz - v3Translate;
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
	for (int i = 0; i<int(fSamples); i++)
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



void getGroundAtmosphere(float4 vertex, out float3 c0, out float3 c1) {

	float3 v3CameraPos = _WorldSpaceCameraPos -v3Translate;	// The camera's current position
	float fCameraHeight = length(v3CameraPos);					// The camera's current height
	float3 tmp;
	if (fCameraHeight > fOuterRadius) {
		AtmFromSpace(vertex, c0, c1);
	}
	else {
		AtmFromGround(vertex, c0, c1, _WorldSpaceCameraPos);
	}
	

}


// Calculates the Mie phase function
float getMiePhase(float fCos, float fCos2, float g, float g2)
{
	return 1.5 * ((1.0 - g2) / (2.0 + g2)) * (1.0 + fCos2) / pow(1.0 + g2 - 2.0*g*fCos, 1.5);
}



void getAtmosphere(float4 vertex, out float3 c0, out float3 c1, out float3 t0) {

	float3 v3CameraPos = _WorldSpaceCameraPos - v3Translate;	// The camera's current position
	float fCameraHeight = length(v3CameraPos);					// The camera's current height
	float3 tmp;
	if (fCameraHeight < fOuterRadius)
		SkyFromAtm(vertex, c0, c1, t0);
	else
		SkyFromSpace(vertex, c0, c1, t0);

}


float3 mixHeight(float3 c1, float3 c2, float spread, float center, float val) {
	float a = 0.5 + 0.5*clamp((val - center)*spread, -1, 1);
	return c1*a + c2*(1 - a);
}

float3 atmColor(float3 c0, float3 c1) {

	float3 atm = 2 * c0 + 0.2*c1;

	return 1.2*atm;

	//return (atmosphereDensity*(2 * c0 + 0.2*c1) + (1 - atmosphereDensity)*color);

}


float3 groundColor(float3 c0, float3 c1, float3 color, float3 wp, float distScale = 1) {
	//return  (atmosphereDensity*2*c0 + (1.0*color*clamp(1-atmosphereDensity,0,1) + atmosphereDensity*0.1*c1);
	float dist = length(_WorldSpaceCameraPos - wp);
	float scale = clamp(sqrt(dist/fInnerRadius*35.0*distScale), 0, 1);
	return lerp(1.6 * color, atmColor(c0,c1), atmosphereDensity*scale);

}




inline float iqhash(float n)
{
	return frac(sin(n)*753.5453123);
	
}

float noise(float3 x)
{

	float3 p = floor(x);
	float3 f = frac(x);

	f = f*f*(3.0 - 2.0*f);
	float n = p.x + p.y*157.0 + 113.0*p.z;

	return lerp(lerp(lerp(iqhash(n + 0.0), iqhash(n + 1.0), f.x),
		lerp(iqhash(n + 157.0), iqhash(n + 158.0), f.x), f.y),
		lerp(lerp(iqhash(n + 113.0), iqhash(n + 114.0), f.x),
			lerp(iqhash(n + 270.0), iqhash(n + 271.0), f.x), f.y), f.z);


}
uniform float3 surfaceNoiseSettings4;

inline float iqhashP(float n)
{
	//	n = n % 2 * PI;
	return frac(sin(n)*surfaceNoiseSettings4.x*0.7535453123);

}

float noisePerturbed(float3 x)
{

	float3 p = floor(x);
	float3 f = frac(x);

	f = f*f*(3.0 - 2.0*f);
	float n = p.x + p.y*157.0 + 113.0*p.z;

	return lerp(lerp(lerp(iqhashP(n + 0.0), iqhashP(n + 1.0), f.x),
		lerp(iqhashP(n + 157.0), iqhashP(n + 158.0), f.x), f.y),
		lerp(lerp(iqhashP(n + 113.0), iqhashP(n + 114.0), f.x),
			lerp(iqhashP(n + 270.0), iqhashP(n + 271.0), f.x), f.y), f.z);


}




float4 getSkyColor(float3 c0, float3 c1, float3 t) {
	float fCos = dot(v3LightPos, t) / length(t);
	float fCos2 = fCos *fCos;
	float3 col = getRayleighPhase(fCos2) * c0 + getMiePhase(fCos, fCos2, g, g2)*c1;
	//Adjust color from HDR
	//				col = IN.c0;
	float d = 0.4;

	float3 v3CameraPos = _WorldSpaceCameraPos - v3Translate;	// The camera's current position
	float fCameraHeight = length(v3CameraPos);					// The camera's current height
	float p = 1;
	float dist = fOuterRadius - fInnerRadius;
	if (fCameraHeight < fOuterRadius)
		p = clamp((fCameraHeight - fInnerRadius)/dist, 0.4, 1);//fInnerRadius;



	col = pow(col, p);// - float3(d, d, d);
	col = 1.0 - exp(col * -fHdrExposure*2);
	float alpha = max(col.r, col.g);
	alpha = max(alpha, col.b);
	float a = pow(alpha, p);
	return float4(col, a);
}

// Cloud stuff 

uniform sampler2D _CloudTex;
uniform sampler2D _CloudTex2;
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
		float ls_cloudShadowStrength;
		float ls_cloudSubScale;
	

		int hasCloudShadows = 0;
		uniform float3 stretch;



		float2 getCloudUVPos(float3 p) {
			float3 np = normalize(p);
			return pos2uv(np)*11.342*stretch;
		}


float getMultiFractal2(in float3 p, float frequency, int octaves, float lacunarity, float offs, float gain, float initialO ) {

            float value = 0.0f;
            float weight = 1.0f;
            float w = -0.05;
            float3 vt = p * frequency;
            float f = 1;

            for (float octave = 0; octave < octaves; octave++)
            {
                 float signal = initialO + noise(vt);//perlinNoise2dSeamlessRaw(frequency, vt.x, vt.z,0,0,0,0);//   Mathf.PerlinNoise(vt.x, vt.z);

                // Make the ridges.
                signal = abs(signal);
                signal = offs - signal;


                signal *= signal;

                signal *= weight;
                weight = signal * gain;
                weight = clamp(weight, 0, 1);

                value += (signal * pow(f, w));
//                value += (signal * pow(f, -1));
//                value += (signal * 1);
//                value += (signal * pow(f, 0.05));
//                value += (signal * pow(frequency, -1.0));
                vt = vt * lacunarity;
//                frequency *= lacunarity;
                 f *= lacunarity;
            }
            return value;
//			return ((value*1.25) -1);
        }


inline float getIQClouds(float3 pos, in int N) {

		float3 p = pos*stretch;
		float n = 0;// noise(p*3.123) * 0.2 - 0.2;;
		float ms = 5*ls_cloudscale;// 
		float3 shift= float3(0.123, 2.314, 0.6243);
		float A = 0;
		float pp = ls_cloudscattering;
		ms = ms * (1 + LS_LargeVortex*noise(p*3.2354 + shift) );
		ms = ms * (1 + LS_SmallVortex*noise(p*29.2354 + shift) );

	/*	for (int i = 1; i <= N; i++) {
			float f = pow(2, i)*1.0293;
			float amp = (2 * pow(i,pp)); 
			n += noise(p*f + shift*f) / amp;
			A += 1/amp;
		}
		*/

//		n = getMultiFractal2(p, ms, 12, 2.5, LS_LargeVortex, 4*LS_SmallVortex, -0.5);
		n = getMultiFractal2(p, ms, 12, 2.5, 0.57, 4, -0.5);

		float v = clamp(n + ls_cloudSubScale, 0, 1.5);

		return  0.4*(10-v*10.0);
	}


		float getCloudTextureOld(float2 uv, float scale, float disp) {
					float y = 0.0f;
					// Perlin octaves
					int NN = 8;
//					scale = scale*(1 + LS_LargeVortex*tex2D(_CloudTex, uv*0.0441)).x;
					float useScale = scale*(1 + pow(LS_LargeVortex*tex2D(_CloudTex2, uv*0.423421).x,0.25));
					//scale = scale*(1 + LS_SmallVortex*tex2D(_CloudTex, uv*3.234)).x;
					//useScale = scale;
					float amp = 0;
					for(int i=0;i < NN; i++) {
						float k = useScale*pow(2,i)*1.032394  + 0.11934;
						float a = 1.0 / pow(i + 1, 2);
						y+= a*tex2D( _CloudTex, k*uv + float2(0.1234*i*ls_time*0.015 - 0.04234*i*i*ls_time*0.015 + 0.9123559 + 0.23411*k , 0.31342  + 0.5923*i*i + disp) ).x;
						//y+= tex2D( _CloudTex, k*uv + float2(0.1234*i*ls_time*0.015 - 0.04234*i*i*ls_time*0.015 + 0.9123559 + 0.23411*k , 0.31342  + 0.5923*i*i + disp) ).x;
						amp += a;
						//if (i >= 2)
						//	useScale = scale;
					}
					// Normalize
				
					y /= amp;
	//				return clamp( pow(ls_cloudscattering/y, ls_cloudsharpness) - 1,0,20);
					return clamp(pow(ls_cloudscattering * y*5, ls_cloudsharpness) - 1.0*ls_cloudSubScale, 0, 20);

				}
			
	 	// returns cloud value, outputs normal to N. 

	 	float getCloudIntensity(float c) {
			return clamp(ls_cloudthickness*pow(clamp(c*0.6,0,1), 1), 0, 1);
	 	}

	 	float getGroundShadowFromClouds(float3 ppos) {
	 		if (hasCloudShadows==0)
	 			return 1;
	 		float2 isp = 0;
			float3 lDir = normalize(v3LightPos);
			if (!intersectSphere(float4(float3(0,0,0), cloudRadius), ppos*fInnerRadius , lDir, 250000, isp.x, isp.y)) {
			}
			float modd = 1;

			if (isp.y>isp.x) {
			float3 iPos = normalize(ppos*fInnerRadius+ isp.y*lDir);
//			float2 cloudUV = getCloudUVPos(iPos);

					
			float cloudVal = getIQClouds(iPos, 5);
			cloudVal = getCloudIntensity(cloudVal);
/*			if (cloudVal > 0.5)
				cloudVal = 1;
			else
				cloudVal = 0;
*/			
			modd = clamp(1-cloudVal*ls_cloudShadowStrength,0,1);
		}
												//modd = 1;
			return modd;


	 	}


		float getNormal(float2 uv, float scale, float dst, out float3 n, float nscale, float disp) {
					float height = getCloudTextureOld(uv, scale, disp);
					int N =4;
					for (int i=0;i<N;i++) {
					
						float2 du1 = float2(dst*cos((i)*2*3.14159 / (N)), dst*sin(i*2*3.14159/(N)));
						float2 du2 = float2(dst*cos((i+1)*2*3.14159 / (N)), dst*sin((i+1)*2*3.14159/(N)));
						
						float hx = getCloudTextureOld(uv + du1, scale, disp);
						float hy = getCloudTextureOld(uv + du2, scale, disp);
					
						float3 d2 = float3(0,height*nscale,0) - float3(du1.x,hx*nscale,du1.y);
						float3 d1 = float3(0,height*nscale,0) - float3(du2.x,hy*nscale,du2.y);
					
						n = n + normalize(cross(d1,d2));
					}
					n = normalize(n);
					return height;
//					return clamp(height-0.0,0,100);
					
		}
				





