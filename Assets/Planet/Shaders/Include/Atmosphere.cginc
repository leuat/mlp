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
	return float2(0.5 + atan2(p.z, p.x) / (2 * 3.14159), 0.5 - asin(p.y)/3.1459);
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
	float3 v3Pos = mul(_Object2World, vert).xyz - v3Translate;
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

	c0 = v3FrontColor *(v3InvWavelength * fKrESun + fKmESun);
	c1 = v3Attenuate;// + v3InvWavelength;


}

void SkyFromSpace(float4 vert, out float3 c0, out float3 c1, out float3 t0) {
	float3 v3CameraPos = _WorldSpaceCameraPos - v3Translate;	// The camera's current position
	float fCameraHeight = length(v3CameraPos);					// The camera's current height
	float fCameraHeight2 = fCameraHeight*fCameraHeight;			// fCameraHeight^2
	float fSamples = 3.0;

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
	return lerp(1.1 * color, atmColor(c0,c1), atmosphereDensity*scale);

}



float iqhash(float n)
{
	return frac(sin(n)*43758.5453);
}

float noise(float3 x)
{
	// The noise function returns a value in the range -1.0f -> 1.0f
	float3 p = floor(x);
	float3 f = frac(x);

	f = f*f*(3.0 - 2.0*f);
	float n = p.x + p.y*57.0 + 113.0*p.z;
	return lerp(lerp(lerp(iqhash(n + 0.0), iqhash(n + 1.0), f.x),
		lerp(iqhash(n + 57.0), iqhash(n + 58.0), f.x), f.y),
		lerp(lerp(iqhash(n + 113.0), iqhash(n + 114.0), f.x),
			lerp(iqhash(n + 170.0), iqhash(n + 171.0), f.x), f.y), f.z);
}

float noiseIQ(in float3 x)
{
	float3 p = floor(x);
	float3 f = frac(x);
	f = f*f*(3.0 - 2.0*f);

	float2 uv = (p.xy + float2(37.0, 17.0)*p.z) + f.xy;
//	float2 rg = tex2D(_IQ, (uv + 0.5) / 256.0, -100.0).yx;
	float2 rg = float2(0, 0);
	return lerp(rg.x, rg.y, f.z);
}


float4 getSkyColor(float3 c0, float3 c1, float3 t) {
	float fCos = dot(v3LightPos, t) / length(t);
	float fCos2 = fCos *fCos;
	float3 col = getRayleighPhase(fCos2) * c0 + getMiePhase(fCos, fCos2, g, g2)*c1;
	//Adjust color from HDR
	//				col = IN.c0;
	float d = 0.1;
	col = pow(col, 0.5) - float3(d, d, d);
	col = 1.0 - exp(col * -fHdrExposure);
	float a = pow(col.b, 2);
	return float4(col, a);
}
