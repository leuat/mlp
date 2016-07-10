#ifndef GeometryShaderExtra
#define GeometryShaderExtra


#pragma vertex   vert
#pragma geometry geo
#pragma fragment frag

// 36 for cube
#define TAM 12


#include "UnityCG.cginc"


float4x4 worldRotMat;
float3 b_tangent;
float3 b_binormal;
float4       _MainTex_ST;
sampler2D _MainTex1;
sampler2D _MainTex2;
sampler2D _MainTex3;
float     _PointSize;

struct gIn // OUT vertex shader, IN geometry shader
{
	float4 vertex : SV_POSITION;
	float3 norm: NORMAL;
	float4 col : COLOR0;

};

struct v2f // OUT geometry shader, IN fragment shader 
{
	float4 pos           : SV_POSITION;
	float2 uv_MainTex : TEXCOORD0;
	float4 col : COLOR0;
	float3 c0 : TEXCOORD1;
	float3 c1 : TEXCOORD2;
	float3 posWorld : TEXCOORD3;
	float3 n: NORMAL;
	float3 params : TEXCOORD4;
#ifdef L_FRAG_PASS
	LIGHTING_COORDS(7, 8)
#endif
	
};

gIn vert(appdata_full v)
{
	gIn o;
//	o.vertex = v.vertex;
	o.vertex = getPlanetSurfaceOnly(v.vertex);
	o.norm = v.normal;
	o.col = v.color;

	return o;
}



#ifdef L_FRAG_PASS
void setupCross(point gIn vert[1], inout TriangleStream<v2f> triStream)
#endif
#ifdef L_SC_PASS
void setupCross(point gIn vert[1], inout TriangleStream<v2f_sc> triStream)
#endif
#ifdef L_SCAST_PASS
void setupCross(point gIn vert[1], inout TriangleStream<v2f_scast> triStream)
#endif
{
	float f = 7;//_PointSize/20.0f; //half size

	float h = f*0.98;

	bool discardThis = false;

	float4 pos = vert[0].vertex;

	float3 pos3 = mul(_Object2World, vert[0].vertex);
	pos3 -= v3Translate;
	float scale = 1 + 1.5*noise(normalize(pos3)*142343.23);

	float3 realN = normalize(pos3);// getPlanetSurfaceNormal(pos3, b_tangent, b_binormal, 0.2, 4)*-1;
	/*float3 realN = getPlanetSurfaceNormalOctaves(pos3, b_tangent, b_binormal, 0.2, 3,4)*-1;

	if (dot(normalize(realN), normalize(pos3)) < 0.99)
		discardThis = true;
		

		/


	/*                 const float4 vc[TAM] = { float4( -f,  f,  f, 0.0f), float4(  f,  f,  f, 0.0f), float4(  f,  f, -f, 0.0f),    //Top
	float4(  f,  f, -f, 0.0f), float4( -f,  f, -f, 0.0f), float4( -f,  f,  f, 0.0f),    //Top

	float4(  f,  f, -f, 0.0f), float4(  f,  f,  f, 0.0f), float4(  f, -f,  f, 0.0f),     //Right
	float4(  f, -f,  f, 0.0f), float4(  f, -f, -f, 0.0f), float4(  f,  f, -f, 0.0f),     //Right

	float4( -f,  f, -f, 0.0f), float4(  f,  f, -f, 0.0f), float4(  f, -f, -f, 0.0f),     //Front
	float4(  f, -f, -f, 0.0f), float4( -f, -f, -f, 0.0f), float4( -f,  f, -f, 0.0f),     //Front

	float4( -f, -f, -f, 0.0f), float4(  f, -f, -f, 0.0f), float4(  f, -f,  f, 0.0f),    //Bottom
	float4(  f, -f,  f, 0.0f), float4( -f, -f,  f, 0.0f), float4( -f, -f, -f, 0.0f),     //Bottom

	float4( -f,  f,  f, 0.0f), float4( -f,  f, -f, 0.0f), float4( -f, -f, -f, 0.0f),    //Left
	float4( -f, -f, -f, 0.0f), float4( -f, -f,  f, 0.0f), float4( -f,  f,  f, 0.0f),    //Left

	float4( -f,  f,  f, 0.0f), float4( -f, -f,  f, 0.0f), float4(  f, -f,  f, 0.0f),    //Back
	float4(  f, -f,  f, 0.0f), float4(  f,  f,  f, 0.0f), float4( -f,  f,  f, 0.0f)     //Back
	};


	const float2 UV1[TAM] = { float2( 0.0f,    0.0f ), float2( 1.0f,    0.0f ), float2( 1.0f,    0.0f ),         //Esta em uma ordem
	float2( 1.0f,    0.0f ), float2( 1.0f,    0.0f ), float2( 1.0f,    0.0f ),         //aleatoria qualquer.

	float2( 0.0f,    0.0f ), float2( 1.0f,    0.0f ), float2( 1.0f,    0.0f ),
	float2( 1.0f,    0.0f ), float2( 1.0f,    0.0f ), float2( 1.0f,    0.0f ),

	float2( 0.0f,    0.0f ), float2( 1.0f,    0.0f ), float2( 1.0f,    0.0f ),
	float2( 1.0f,    0.0f ), float2( 1.0f,    0.0f ), float2( 1.0f,    0.0f ),

	float2( 0.0f,    0.0f ), float2( 1.0f,    0.0f ), float2( 1.0f,    0.0f ),
	float2( 1.0f,    0.0f ), float2( 1.0f,    0.0f ), float2( 1.0f,    0.0f ),

	float2( 0.0f,    0.0f ), float2( 1.0f,    0.0f ), float2( 1.0f,    0.0f ),
	float2( 1.0f,    0.0f ), float2( 1.0f,    0.0f ), float2( 1.0f,    0.0f ),

	float2( 0.0f,    0.0f ), float2( 1.0f,    0.0f ), float2( 1.0f,    0.0f ),
	float2( 1.0f,    0.0f ), float2( 1.0f,    0.0f ), float2( 1.0f,    0.0f )
	};


	const int TRI_STRIP[TAM]  = {  0, 1, 2,  3, 4, 5,
	6, 7, 8,  9,10,11,
	12,13,14, 15,16,17,
	18,19,20, 21,22,23,
	24,25,26, 27,28,29,
	30,31,32, 33,34,35
	};
	*/

	const float4 vc[TAM] = {
		float4(0,  f, -f, 0.0f), float4(0,  f,  f, 0.0f), float4(0, -f,  f, 0.0f),     //Right
		float4(0, -f,  f, 0.0f), float4(0, -f, -f, 0.0f), float4(0,  f, -f, 0.0f),     //Right

		float4(-f,  f, 0, 0.0f), float4(f,  f, 0, 0.0f), float4(f, -f, 0, 0.0f),     //Front
		float4(f, -f, 0, 0.0f), float4(-f, -f, 0, 0.0f), float4(-f,  f, 0, 0.0f)     //Front


	};


	const float2 UV1[TAM] = {
		float2(1.0f,    0.0f), float2(1.0f,    1.0f), float2(0.0f,    1.0f),
		float2(0.0f,    1.0f), float2(0.0f,    0.0f), float2(1.0f,    0.0f),

		float2(1.0f,    0.0f), float2(1.0f,    1.0f), float2(0.0f,    1.0f),
		float2(0.0f,    1.0f), float2(0.0f,    0.0f), float2(1.0f,    0.0f)


	};

	const int TRI_STRIP[TAM] = { 0, 1, 2,  3, 4, 5,
		6, 7, 8,  9,10,11 };

	const int UV_TRI_STRIP[TAM] = { 2, 1, 0 , 5, 4, 3,
		8, 7, 6,  11,10,9 };

#ifdef L_FRAG_PASS
	v2f outV[TAM];
	#endif
#ifdef L_SC_PASS
	v2f_sc outV[TAM];
#endif
#ifdef L_SCAST_PASS
	v2f_scast outV[TAM];
#endif
	int i;

	float3 posWorld = pos3 + v3Translate;

	float tThreshold = topThreshold*0.5;

	float height = (length(posWorld - v3Translate) / fInnerRadius - 1);
	if (height < liquidThreshold)
		discardThis = true;
	if (height > tThreshold)
		discardThis = true;

	scale = scale - (0.8 * height/tThreshold);

	if (discardThis)
		return;

	// Rotate random
	float rot = vert[0].norm.y;
	float sinX = sin(rot);
	float cosX = cos(rot);
	float2x2 rot2 = float2x2(cosX, -sinX, sinX, cosX);




	for (i = 0; i < TAM; i++) {
		float3 disp0 = vc[i] + float3(0, h, 0);
		disp0.xz = mul(rot2, disp0.xz);
		float3 disp = mul(worldRotMat, disp0)*scale;

		outV[i].pos = pos + float4(disp,0);
		
#ifdef L_FRAG_PASS
		outV[i].col = vert[0].col;
		outV[i].n = realN;
		outV[i].posWorld = posWorld;
		// Additional params used from normals
		if (vert[0].norm.x==0)
    		outV[i].params = float3(1,0,0);
		if (vert[0].norm.x==1)
    		outV[i].params = float3(0,1,0);
		if (vert[0].norm.x==2)
    		outV[i].params = float3(0,0,1);
#endif
#ifdef L_SCAST_PASS
		outV[i].posWorld = posWorld;
		if (vert[0].norm.x==0)
    		outV[i].params = float3(1,0,0);
		if (vert[0].norm.x==1)
    		outV[i].params = float3(0,1,0);
		if (vert[0].norm.x==2)
    		outV[i].params = float3(0,0,1);

#endif

		

	}

	// Assign UV values
	//                 for (i=0;i<TAM;i++) v[i].uv_MainTex = TRANSFORM_TEX(UV1[i],_MainTex); 
	for (i = 0; i < TAM; i++) outV[i].uv_MainTex = UV1[UV_TRI_STRIP[i]];//TRANSFORM_TEX(UV1[i],_MainTex); 


	gIn tmpV[TAM];
																	 // Position in view space
	for (i = 0; i < TAM; i++) { 
		tmpV[i].vertex = outV[i].pos;
		outV[i].pos = mul(UNITY_MATRIX_MVP, outV[i].pos); 
	}

#ifdef L_FRAG_PASS
	float3 c0, c1;

	getGroundAtmosphere(pos, c0, c1);

	for (i = 0; i < TAM; i++) {
		outV[i].c0 = c0;
		outV[i].c1 = c1;
	}
	for (i = 0; i < TAM; i++)
		TRANSFER_VERTEX_TO_FRAGMENT(outV[i]);

#endif

#ifdef L_SC_PASS
	for (i = 0; i < TAM; i++) {
		gIn v = tmpV[i];
		TRANSFER_SHADOW_COLLECTOR(outV[i])
	}
#endif
#ifdef L_SCAST_PASS
	for (i = 0; i < TAM; i++) {
		gIn v = tmpV[i];
		TRANSFER_SHADOW_CASTER(outV[i])
	}
#endif 

	// Build the cube tile by submitting triangle strip vertices
	if (!discardThis)
		for (i = 0; i < TAM / 3; i++)
		{
			triStream.Append(outV[TRI_STRIP[i * 3 + 0]]);
			triStream.Append(outV[TRI_STRIP[i * 3 + 1]]);
			triStream.Append(outV[TRI_STRIP[i * 3 + 2]]);

			triStream.RestartStrip();
		}
}



inline float4 getBlendedTexture(float3 scale, float2 uv) {
	return tex2D(_MainTex1, uv)*scale.x + tex2D(_MainTex2, uv)*scale.y + tex2D(_MainTex3, uv)*scale.z;
}



#endif
