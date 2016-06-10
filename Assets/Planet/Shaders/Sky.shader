
Shader "LemonSpawn/Sky"
{
	SubShader
	{
		Tags { "RenderType" = "Transparent"  "Queue" = "Transparent+1000" }
		Pass
		{

			Cull Front
			//			Blend SrcAlpha OneMinusSrcAlpha
						Blend One One
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
							float fCos = dot(v3LightPos, IN.t0) / length(IN.t0);
							float fCos2 = fCos *fCos;
							float3 col = getRayleighPhase(fCos2) * IN.c0 + getMiePhase(fCos, fCos2, g, g2)*IN.c1;
							//Adjust color from HDR
			//				col = IN.c0;
							float d = 0.1;
							col = pow(col, 0.5) - float3(d, d, d);
							col = 1.0 - exp(col * -fHdrExposure);
							float a = pow(col.b,2);

							return float4(col, a +1.0);
							}




										ENDCG

									}
	}
}
