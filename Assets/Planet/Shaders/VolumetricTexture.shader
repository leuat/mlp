// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "LemonSpawn/VolumetricTexture" {

	Properties{
		_MainTex("Base (RGB)", 3D) = "white" {}
	}

		SubShader{
		Tags{ "Queue" = "Transparent" "RenderType-1000" = "Transparent-1000" }
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
//#include "Include/Atmosphere.cginc"


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
		float3 texcoord : TEXCOORD0;
		float3 normal : TEXCOORD1;
		float3 worldPosition : TEXCOORD2;
		float3 c0 : TEXCOORD3;
		float3 c1 : TEXCOORD4;
		float depth : TEXCOORD5;
		float4 projPos : TEXCOORD6;
		float3 t  : TEXCOORD7;
	};

	sampler3D _MainTex;
	float _Scale;
	float _Alpha;
	float _Glossiness;
	float4 _Color;
	float4 _SpecColor;
	uniform float sradius;
	uniform sampler2D _CameraDepthTexture;


	float4 _H;



	v2f vert(vertexInput v)
	{
		v2f o;

		float4x4 modelMatrix = unity_ObjectToWorld;
		float4x4 modelMatrixInverse = unity_WorldToObject;
		o.pos = mul(UNITY_MATRIX_MVP, v.vertex);

		o.texcoord = v.texcoord;
		o.normal = v.normal;
		o.worldPosition = mul(modelMatrix, v.vertex);

		o.projPos = ComputeScreenPos(o.pos);


		float3 v3CameraPos = _WorldSpaceCameraPos;
		float3 viewDirection = normalize(
			_WorldSpaceCameraPos - o.worldPosition.xyz)*-1;



//		o.texcoord = pos2uv(normalize(hitPos));

		UNITY_TRANSFER_DEPTH(o.depth);
		return o;
	}


	float4 rayCast(float3 start, float3 end, float3 direction, float stepLength) {


//		int N = clamp(length(start - end) / stepLength,0,100);
		int N = 100;
		//int N = 50;
//		return float4(N, N, N, 100)*0.01;
		float4 val = float4(0,0,0,0);
		direction = normalize(direction);
		float3 p = start;
		for (int i = 0; i < N; i++) {



			val += tex3D(_MainTex, p)*1;
			if (val.a >=0.9)
				return val;
			if (length(p - end) < 2 * stepLength)
				return float4(0,0,0,0);

			p = p + stepLength*direction;
		}
		return val;

	}

	fixed4 frag(v2f IN) : COLOR{

		float4 c;



	float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);


	float3 viewDirection = normalize(
		_WorldSpaceCameraPos - IN.worldPosition.xyz)*-1;


	float3 uv = IN.texcoord;

	float dist = length(_WorldSpaceCameraPos - IN.worldPosition.xyz);

	float3 startLength = clamp(dist*0.5,1, 10);
	
	float3 pos = IN.worldPosition.xyz;

	c = rayCast(pos - viewDirection*startLength, pos, viewDirection, 0.001*dist);
	//c.rgb *= 0.5;

	return c;

	}
		ENDCG
	}
	}
		Fallback "Diffuse"
}