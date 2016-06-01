Shader "LemonSpawn/Nature" {

	Properties{
		_MainTex("Base (RGB)", 2D) = "white" {}
		_BumpMap("Bump", 2D) = "bump" {}
		_Scale("Scale", Range(0.0, 50.0)) = 1
		_Alpha("Alpha", Range(0.0, 1.0)) = 1
		_Glossiness("Smoothness", Range(0.0, 1.0)) = 0.5
		_Color("Color", Color) = (1,1,1,1)
		_SpecColor("Specular Color", Color) = (1,1,1,1)

	}

		SubShader{
		Tags{ "Queue" = "Transparent+11000" "RenderType" = "Transparent" }
		LOD 400


		Lighting On
		Cull Back
		ZWrite on
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
			float3 T: TEXCOORD5;
			float3 B: TEXCOORD6;
		};

		sampler2D _BumpMap, _MainTex;
		float _Scale;
		float _Alpha;
		float _Glossiness;
		float4 _Color;
		float4 _SpecColor;

		v2f vert(vertexInput v)
		{
			v2f o;

			float4x4 modelMatrix = _Object2World;
			float4x4 modelMatrixInverse = _World2Object;
			o.pos = mul(UNITY_MATRIX_MVP, v.vertex);

			o.texcoord = v.texcoord;
			o.worldPosition = mul(modelMatrix, v.vertex);

			getGroundAtmosphere(v.vertex, o.c0, o.c1);


			o.T = normalize(
				mul(modelMatrix, float4(v.tangent.xyz, 0.0)).xyz);
			o.normal = normalize(
				mul(float4(v.normal, 0.0), modelMatrixInverse).xyz);
			o.B = normalize(
				cross(o.normal, o.T)
				* v.tangent.w); // tangent.w is specific to Unity

			return o;
		}

		fixed4 frag(v2f IN) : COLOR{

		float4 c;


		float2 uv = IN.texcoord.xy;

		float3 lightDirection =
			normalize(_WorldSpaceLightPos0.xyz);

		float3 viewDirection = normalize(
			_WorldSpaceCameraPos - IN.worldPosition.xyz);

		float4 encodedNormal = tex2D(_BumpMap,
			_Scale * uv);
		float3 localCoords = float3(2.0 * encodedNormal.a - 1.0,
			2.0 * encodedNormal.g - 1.0, 0.0);
		localCoords.z = sqrt(1.0 - dot(localCoords, localCoords));
		// approximation without sqrt:  localCoords.z = 
		// 1.0 - 0.5 * dot(localCoords, localCoords);



		float3x3 local2WorldTranspose = float3x3(
			IN.T,
			IN.B,
			IN.normal);

		float3 normalDirection =
			normalize(mul(localCoords, local2WorldTranspose));

		float3 specularReflection = _SpecColor
			* pow(max(0.0, dot(
				reflect(-lightDirection, normalDirection),
				viewDirection)), _Glossiness*100);

		float light = max(-0.0, dot(normalDirection, lightDirection));


		float4 color = tex2D(_MainTex, uv*_Scale)*_Color;
		
		if (color.a < _Alpha)
			discard;


		//			float3 skyColor = texCUBE(_SkyBox, WorldReflectionVector(IN, o.Normal)*float3(-1,1,1)).rgb;//flip x
		c.rgb = groundColor(IN.c0, IN.c1, color*light, IN.worldPosition.xyz);



		return float4(c.rgb
			+ specularReflection, 1);

		}
			ENDCG
		}
		}
			Fallback "Diffuse"
}