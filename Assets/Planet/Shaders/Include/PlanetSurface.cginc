#ifndef PlanetSurface
#define PlanetSurface


        uniform float3 surfaceNoiseSettings;
        uniform float3 surfaceNoiseSettings2;
        uniform float3 surfaceNoiseSettings3;
		uniform float3 surfaceVortex1;
		uniform float3 surfaceVortex2;
		float4x4 rotMatrix;
	

		float getStandardPerlin(float3 pos, float scale, float power, float sub, int N) {
			float n = 0;
			float A = 0;
			float ms = scale;
			float3 shift= float3(0.123, 2.314, 0.6243);

			for (int i = 1; i <= N; i++) {
				float f = pow(2, i)*1.0293;
				float amp = (2 * pow(i,power)); 
				n += noisePerturbed(pos*f*ms + shift*f) / amp;
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
                 float signal = initialO + noisePerturbed(vt);//perlinNoise2dSeamlessRaw(frequency, vt.x, vt.z,0,0,0,0);//   Mathf.PerlinNoise(vt.x, vt.z);

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



		float getSurfaceHeight(float3 pos, float scale, float octaves) {

			//return noise(pos * 10)*5;


			scale = scale*(1 + surfaceVortex1.y*noisePerturbed(pos*surfaceVortex1.x));
			scale = scale*(1 + surfaceVortex2.y*noisePerturbed(pos*surfaceVortex2.x));
			float val = getMultiFractal(pos, scale, octaves, surfaceNoiseSettings.x, surfaceNoiseSettings.y, surfaceNoiseSettings.z, surfaceNoiseSettings2.x);
		//	if (surfaceNoiseSettings4.y>0)
	   // 		val+= surfaceNoiseSettings4.y*getMultiFractal(pos*surfaceNoiseSettings4.z, scale, octaves, surfaceNoiseSettings.x, surfaceNoiseSettings.y, surfaceNoiseSettings.z, surfaceNoiseSettings2.x);
			val = pow(val, surfaceNoiseSettings3.z);
			return clamp(val-surfaceNoiseSettings3.x, 0, 10);
			//return getStandardPerlin(pos, scale, 1, 0.5, 8);

		}

		float3 getHeightPosition(in float3 pos, in float scale, float heightScale, float octaves) {
			return pos*fInnerRadius*(1 + getSurfaceHeight(mul(rotMatrix, pos), scale, octaves)*heightScale);
//			return pos*fInnerRadius*(1+getSurfaceHeight(mul(rotMatrix, pos) , scale, octaves)*heightScale);
			
		}



		float3 getSurfaceNormal(float3 pos, float scale,  float heightScale, float normalScale, float3 tangent, float3 bn, float octaves, int N) {
//			float3 getSurfaceNormal(float3 pos, float scale, float heightScale, float normalScale) {
			float3 prev = 0;
//			pos = normalize(pos);
			float hs = heightScale;
			float3 centerPos = getHeightPosition(normalize(pos), scale, hs, octaves);
			float3 norm = normalize(centerPos) * 0;
			//			[unroll]
						for (float i=0;i<N;i++) {
							float3 disp = float3(cos(i/(N+0)*2.0*PI), 0, sin(i/(N+0)*2.0*PI));
							//float3 rotDisp = mul(tangentToWorld, disp);
							//float3 np = normalize(pos + mul(tangentToWorld, disp)*normalScale);
							//float3 np = normalize(pos + disp*normalScale);
							float3 np = normalize(pos + (disp.x*tangent + disp.z*bn) *normalScale);

							float3 newPos = getHeightPosition(np, scale, hs, octaves);


							if (length(prev)>0.1) {
								float3 n = normalize(cross(newPos - centerPos, prev - centerPos));
								float3 nn = n;
			//					if (dot(nn, normalize(pos)) < 0.0)
				//					nn *= -1;

								norm += nn;

							}
							prev = newPos;

						}
						

			return normalize(norm)*-1;
		}


		inline float LodSurface(in float3 p) {
			return surfaceNoiseSettings3.y;
//			return clamp(5000.0 / (length(p.xyz +v3Translate - _WorldSpaceCameraPos.xyz)), 4, surfaceNoiseSettings3.y);

		}

	//	inline float3 getPlanetSurfaceNormal(in float4 v) {
		float3 getPlanetSurfaceNormal(in float3 v, float3 t, float3 bn, float nscale, int N) {
			float scale = surfaceNoiseSettings2.z;
			float heightScale = surfaceNoiseSettings2.y;

			float octaves = LodSurface(v.xyz);

			return getSurfaceNormal(v, scale, heightScale, nscale, t,bn, octaves, N);
		}


		float4 getPlanetSurfaceOnly(in float4 v) {

			float4 p = mul(_Object2World, v);
			p.xyz -= v3Translate;

			float octaves = surfaceNoiseSettings3.y;

			float scale = surfaceNoiseSettings2.z;
			float heightScale = surfaceNoiseSettings2.y;

			p.xyz = normalize(p.xyz);
			p.xyz = getHeightPosition(p.xyz, scale, heightScale, octaves) + v3Translate;
			return mul(_World2Object, p);
		}

		float4 getPlanetSurfaceOnlyNoTranslate(in float4 v) {

			float4 p = mul(_Object2World, v);

			float octaves = surfaceNoiseSettings3.y;

			float scale = surfaceNoiseSettings2.z;
			float heightScale = surfaceNoiseSettings2.y;

			p.xyz = normalize(p.xyz);
			p.xyz = getHeightPosition(p.xyz, scale, heightScale, octaves);
			return mul(_World2Object, p);
		}

		#endif

