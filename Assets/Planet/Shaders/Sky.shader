
Shader "LemonSpawn/Sky"
{
	SubShader
	{

		Tags { "RenderType" = "Transparent"  "Queue" = "Transparent+1" }
		Pass
		{

			Cull Front
			Zwrite off
						Blend SrcAlpha OneMinusSrcAlpha
				//		Blend One One
						CGPROGRAM
						#include "UnityCG.cginc"
						#include "Include/Atmosphere.cginc"
						#pragma target 3.0
						#pragma vertex vert
						#pragma fragment frag

						struct v2f
						{
							float4 pos : SV_POSITION;
							float2 uv : TEXCOORD0;
							float3 t0 : TEXCOORD1;
							float3 c0 : TEXCOORD2;
							float3 c1 : TEXCOORD3;
						};

						v2f vert(appdata_base v)
						{
							v2f OUT;
							OUT.pos = mul(UNITY_MATRIX_MVP, v.vertex);
							OUT.uv = v.texcoord.xy;

							getAtmosphere(v.vertex, OUT.c0, OUT.c1, OUT.t0);

							return OUT;
						}

							half4 frag(v2f IN) : COLOR
						{
//							return float4(1,0,0,1);
							return getSkyColor(IN.c0, IN.c1, IN.t0);
							}



										ENDCG

									}
	}
}
