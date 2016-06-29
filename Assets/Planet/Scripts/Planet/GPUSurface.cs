﻿using UnityEngine;
using System.Collections;
using LemonSpawn;

namespace LemonSpawn {


/* Translated from Shader code */
public class GPUSurface {

		PlanetSettings planetSettings;


		float PI = Mathf.PI;

		Vector3 surfaceNoiseSettings;
        Vector3 surfaceNoiseSettings2;
        Vector3 surfaceNoiseSettings3;
        Vector3 v3Translate;
        float fInnerRadius;

        public void Update() {
        	if (planetSettings == null) 
        		return;
	        surfaceNoiseSettings = planetSettings.ExpSurfSettings;
			surfaceNoiseSettings2 = planetSettings.ExpSurfSettings2;
			surfaceNoiseSettings3 = planetSettings.ExpSurfSettings3;
			fInnerRadius = planetSettings.radius;
			v3Translate = planetSettings.transform.position;
        }

		public float clamp(float a, float b, float c) {
			return Mathf.Clamp(a,b,c);
		}

		public float pow(float a, float b) {
			return Mathf.Pow(a,b);
		}

		public float frac(float a) {
			return a - Mathf.Floor(a);
		}

		public Vector3 frac(Vector3 a) {
			return new Vector3(a.x - Mathf.Floor(a.x),a.y - Mathf.Floor(a.y),a.z - Mathf.Floor(a.z));
		}

		public Vector3 normalize(Vector3 a) {
			return a.normalized;
		}

		public Vector3 cross(Vector3 a, Vector3 b) {
			return Vector3.Cross(a,b);
		}

		public float length(Vector3 a) {
			return a.magnitude;
		}

		public float floor(float a) {
			return Mathf.Floor(a);
		}

		public Vector3 floor(Vector3 a) {
			return new Vector3(Mathf.Floor(a.x),Mathf.Floor(a.y),Mathf.Floor(a.z));
		}
		public float sin(float a) {
			return Mathf.Sin(a);
		}

		public float cos(float a) {
			return Mathf.Cos(a);
		}

		public float abs(float a) {
			return Mathf.Abs(a);
		}


		public GPUSurface(PlanetSettings ps) {
			planetSettings = ps;
			Update();
		}


		float iqhash(float n)
		{
			return frac(sin(n)*753.5453123f);
		}

		float lerp(float a, float b, float w) {
			//return Mathf.Lerp(a,b,c);
			  return a + w*(b-a);

		}


float noise(Vector3 x)
{
	// The noise function returns a value in the range -1.0f -> 1.0f
	Vector3 p = floor(x);
	Vector3 f = frac(x);

	f.x = f.x*f.x*(3.0f - 2.0f*f.x);
	f.y = f.y*f.y*(3.0f - 2.0f*f.y);
	f.z = f.z*f.z*(3.0f - 2.0f*f.z);



	float n = p.x + p.y*157.0f + 113.0f*p.z;

	    return lerp(lerp(lerp( iqhash(n+  0.0f), iqhash(n+  1.0f),f.x),
                   lerp( iqhash(n+157.0f), iqhash(n+158.0f),f.x),f.y),
               lerp(lerp( iqhash(n+113.0f), iqhash(n+114.0f),f.x),
                   lerp( iqhash(n+270.0f), iqhash(n+271.0f),f.x),f.y),f.z);


}

		float getStandardPerlin(Vector3 pos, float scale, float power, float sub, int N) {
			float n = 0;
			float A = 0;
			float ms = scale;
			Vector3 shift= new Vector3(0.123f, 2.314f, 0.6243f);

			for (int i = 1; i <= N; i++) {
				float f = pow(2, i)*1.0293f;
				float amp = (2 * pow(i,power)); 
				n += noise(pos*f*ms + shift*f) / amp;
				A += 1/amp;
			}

			float v = clamp(n - sub*A, 0, 1);
			return v;	

		}

		float getMultiFractal(Vector3 p, float frequency, int octaves, float lacunarity, float offs, float gain, float initialO ) {

            float value = 0.0f;
            float weight = 1.0f;

            Vector3 vt = p * frequency;
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



		float getSurfaceHeight(Vector3 pos, float scale) {


			float val = getMultiFractal(pos, scale, 8, surfaceNoiseSettings.x, surfaceNoiseSettings.y, surfaceNoiseSettings.z, surfaceNoiseSettings2.x);
			return clamp(val-surfaceNoiseSettings3.x, 0, 10000);
//			return getStandardPerlin(pos, scale, 1, 0.5f, 8);

		}

		Vector3 getHeightPosition(Vector3 pos,  float scale, float heightScale) {
			return pos*fInnerRadius*(1+getSurfaceHeight(pos, scale)*heightScale); 
		}


		Vector3 getSurfaceNormal(Vector3 pos, float scale,  float heightScale, float normalScale) {
			float N = 4.0f;
			Vector3 prev = Vector3.zero;
//			pos = normalize(pos);
			float hs = heightScale;
			Vector3 centerPos = getHeightPosition(normalize(pos), scale, hs);
			Vector3 norm = normalize(centerPos);

			for (float i=0;i<N;i++) {
				Vector3 disp = new Vector3(cos(i/(N+1)*2*PI), 0, sin(i/(N+1)*2*PI));
				//Vector3 rotDisp = mul(tangentToWorld, disp);

				Vector3 np = normalize(pos + disp*normalScale);

				Vector3 newPos = getHeightPosition(np, scale, hs);
				if (length(prev)>0.1f) {
					norm += normalize(cross(newPos-centerPos, prev - centerPos));
				}
				prev = newPos;

			}
			return normalize(norm);
		}


		public Vector3 getPlanetSurface(Vector3 p, float scale, float heightScale, out Vector3 n) {
			n = Vector3.up;

			//return p.normalized*fInnerRadius;

			//n = getSurfaceNormal(p, scale, heightScale, 0.1f);
	
			p = normalize(p);
			p = getHeightPosition(p, scale, heightScale);
			return p;
		}


		}
}