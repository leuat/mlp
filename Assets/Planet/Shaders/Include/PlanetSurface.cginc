#ifndef PlanetSurface
#define PlanetSurface


        uniform float3 surfaceNoiseSettings;
        uniform float3 surfaceNoiseSettings2;
        uniform float3 surfaceNoiseSettings3;



		float getStandardPerlin(float3 pos, float scale, float power, float sub, int N) {
			float n = 0;
			float A = 0;
			float ms = scale;
			float3 shift= float3(0.123, 2.314, 0.6243);

			for (int i = 1; i <= N; i++) {
				float f = pow(2, i)*1.0293;
				float amp = (2 * pow(i,power)); 
				n += noise(pos*f*ms + shift*f) / amp;
				A += 1/amp;
			}

			float v = clamp(n - sub*A, 0, 1);
			return v;	

		}

		float getMultiFractal(in float3 p, float frequency, int octaves, float lacunarity, float offs, float gain, float initialO ) {

            float value = 0.0f;
            float weight = 1.0f;

            float3 vt = p * frequency;
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

                value += (signal * 1);
                vt = vt * lacunarity;
                frequency *= lacunarity;
            }
            return value;
        }




		float getSurfaceHeight(float3 pos, float scale) {


			float val = getMultiFractal(pos, scale, 8, surfaceNoiseSettings.x, surfaceNoiseSettings.y, surfaceNoiseSettings.z, surfaceNoiseSettings2.x);
			return clamp(val-surfaceNoiseSettings3.x, 0, 10000);
			//return getStandardPerlin(pos, scale, 1, 0.5, 8);

		}

		float3 getHeightPosition(in float3 pos, in float scale, float heightScale) {
			return pos*fInnerRadius*(1+getSurfaceHeight(pos, scale)*heightScale); 
		}


		float3 getSurfaceNormal(float3 pos, float scale,  float heightScale, float normalScale) {
			float N = 4.0;
			float3 prev = 0;
//			pos = normalize(pos);
			float hs = heightScale;
			float3 centerPos = getHeightPosition(normalize(pos), scale, hs);
			float3 norm = normalize(centerPos);

			[unroll]
			for (float i=0;i<N;i++) {
				float3 disp = float3(cos(i/(N+1)*2*PI), 0, sin(i/(N+1)*2*PI));
				//float3 rotDisp = mul(tangentToWorld, disp);

				float3 np = normalize(pos + disp*normalScale);

				float3 newPos = getHeightPosition(np, scale, hs);
				if (length(prev)>0.1) {
					norm += normalize(cross(newPos-centerPos, prev - centerPos));
				}
				prev = newPos;

			}
			return normalize(norm);
		}


		inline float4 getPlanetSurface(in float4 v, float scale, float heightScale, out float3 n) {

			float4 p = mul(_Object2World, v);
			p.xyz -=v3Translate;


			n = getSurfaceNormal(p.xyz, scale, heightScale, 0.1);


			p.xyz = normalize(p.xyz);
			p.xyz = getHeightPosition(p.xyz, scale, heightScale) + v3Translate;
			return mul(_World2Object, p) ;
		}

		#endif

