using UnityEngine;
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
        Vector3 surfaceNoiseSettings4;
        Vector3 surfaceNoiseSettings5;
        Vector3 v3Translate;
        Vector3 surfaceVortex1;
        Vector3 surfaceVortex2;
        Matrix4x4 rotMatrix;


        float fInnerRadius;

        public void Update() {
        	if (planetSettings == null) 
        		return;
	        surfaceNoiseSettings = planetSettings.ExpSurfSettings;
			surfaceNoiseSettings2 = planetSettings.ExpSurfSettings2;
			surfaceNoiseSettings3 = planetSettings.ExpSurfSettings3;
            surfaceNoiseSettings4 = planetSettings.ExpSurfSettings4;
            surfaceNoiseSettings5 = planetSettings.ExpSurfSettings5;
            surfaceVortex1 = planetSettings.SurfaceVortex1;
            surfaceVortex2 = planetSettings.SurfaceVortex2;
			fInnerRadius = planetSettings.radius;
            if (planetSettings.atmosphere!=null)
            rotMatrix = planetSettings.atmosphere.rotMat;
			//v3Translate = planetSettings.transform.position;
        }

        static public float clamp(float a, float b, float c) {
			return Mathf.Clamp(a,b,c);
		}

        static public float pow(float a, float b) {
			return Mathf.Pow(a,b);
		}

        static public float frac(float a) {
			return a - Mathf.Floor(a);
		}

      /*  static public double frac(double a)
        {
            return a - System.Math.Floor(a);
        }
        */
        static public Vector3 frac(Vector3 a) {
			return new Vector3(a.x - Mathf.Floor(a.x),a.y - Mathf.Floor(a.y), a.z - Mathf.Floor(a.z));
		}

        static public Vector3 normalize(Vector3 a) {
			return a.normalized;
		}

        static public Vector3 cross(Vector3 a, Vector3 b) {
			return Vector3.Cross(a,b);
		}

        static public float length(Vector3 a) {
			return a.magnitude;
		}

        static public float floor(float a) {
			return Mathf.Floor(a);
		}

        static public Vector3 floor(Vector3 a) {
			return new Vector3(Mathf.Floor(a.x),Mathf.Floor(a.y),Mathf.Floor(a.z));
		}
        static public float sin(float a) {
			return Mathf.Sin(a);
		}

        static public float cos(float a) {
			return Mathf.Cos(a);
		}

        static public float abs(float a) {
			return Mathf.Abs(a);
		}


		public GPUSurface(PlanetSettings ps) {
			planetSettings = ps;
			//Update();
		}


        float iqhash(float n)
		{
//			return (float)frac((double)System.Math.Sin(n)*753.5453123);
            return frac(sin(n) * surfaceNoiseSettings4.x * 0.7535453123f);
        }

        static float iqhashStatic(float n)
        {
          return (float)frac(Mathf.Sin(n)*753.5453123f); 
        }


        static float lerp(float a, float b, float w) {
			//return Mathf.Lerp(a,b,c);
			  return a + w*(b-a);

		}


public float noise(Vector3 x)
{
	// The noise function returns a value in the range -1.0f -> 1.0f
	Vector3 p = floor(x);
	Vector3 f = frac(x);

	f.x = f.x*f.x*(3.0f - 2.0f*f.x);
	f.y = f.y*f.y*(3.0f - 2.0f*f.y);
	f.z = f.z*f.z*(3.0f - 2.0f*f.z);


            float n = (p.x + p.y * 157.0f + 113.0f * p.z);

	    return lerp(lerp(lerp( iqhash(n+  0.0f), iqhash(n+  1.0f),f.x),
                   lerp( iqhash(n+157.0f), iqhash(n+158.0f),f.x),f.y),
               lerp(lerp( iqhash(n+113.0f), iqhash(n+114.0f),f.x),
                   lerp( iqhash(n+270.0f), iqhash(n+271.0f),f.x),f.y),f.z);


}

public static float noiseStatic(Vector3 x)
{
    // The noise function returns a value in the range -1.0f -> 1.0f
    Vector3 p = floor(x);
    Vector3 f = frac(x);

    f.x = f.x*f.x*(3.0f - 2.0f*f.x);
    f.y = f.y*f.y*(3.0f - 2.0f*f.y);
    f.z = f.z*f.z*(3.0f - 2.0f*f.z);


            float n = (p.x + p.y * 157.0f + 113.0f * p.z);

            return lerp(lerp(lerp( iqhashStatic(n+  0.0f), iqhashStatic(n+  1.0f),f.x),
                lerp( iqhashStatic(n+157.0f), iqhashStatic(n+158.0f),f.x),f.y),
                lerp(lerp( iqhashStatic(n+113.0f), iqhashStatic(n+114.0f),f.x),
                    lerp( iqhashStatic(n+270.0f), iqhashStatic(n+271.0f),f.x),f.y),f.z);


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



        float getSurfaceHeight(Vector3 pos, float scale, float octaves) {

           // return noise(pos * 10) * 5;
            scale = scale*(1 + surfaceVortex1.y*noise(pos*surfaceVortex1.x));
            scale = scale*(1 + surfaceVortex2.y*noise(pos*surfaceVortex2.x));
            float val = getMultiFractal(pos, scale, (int)octaves, surfaceNoiseSettings.x, surfaceNoiseSettings.y, surfaceNoiseSettings.z, surfaceNoiseSettings2.x);
            val = pow(val, surfaceNoiseSettings3.z);
            return clamp(val-surfaceNoiseSettings3.x, 0, 10);
            //return getStandardPerlin(pos, scale, 1, 0.5, 8);

        }

        Vector3 getHeightPosition(Vector3 pos, float scale, float heightScale, float octaves) {
            return pos*fInnerRadius*(1 + getSurfaceHeight(pos, scale, octaves)*heightScale);
//          return pos*fInnerRadius*(1+getSurfaceHeight(mul(rotMatrix, pos) , scale, octaves)*heightScale);
            
        }



        float LodSurface(Vector3 p) {
            return surfaceNoiseSettings3.y;
//          return clamp(5000.0 / (length(p.xyz +v3Translate - _WorldSpaceCameraPos.xyz)), 4, surfaceNoiseSettings3.y);

        }


        public Vector3 getPlanetSurfaceOnly(Vector3 v) {

/*            float4 p = mul(_Object2World, v);
            p.xyz -= v3Translate;
            */
            float octaves = surfaceNoiseSettings3.y;

            float scale = surfaceNoiseSettings2.z;
            float heightScale = surfaceNoiseSettings2.y;

            Vector3 p = v;

            p = normalize(p);
            p = getHeightPosition(p, scale, heightScale, octaves);// + v3Translate;
            return p;
        }

        Vector3 getSurfaceNormal(Vector3 pos, float scale, float heightScale, float normalScale, Vector3 tangent, Vector3 bn, float octaves, int N)
        {
            //			Vector3 getSurfaceNormal(Vector3 pos, float scale, float heightScale, float normalScale) {
            Vector3 prev = Vector3.zero;
            //			pos = normalize(pos);
            float hs = heightScale;
            Vector3 centerPos = getHeightPosition(normalize(pos), scale, hs, octaves);
            Vector3 norm = Vector3.zero;
            //			[unroll]
            for (float i = 0; i < N; i++)
            {
                Vector3 disp = new Vector3(cos(i / (N + 0f) * 2.0f * PI), 0, sin(i / (N + 0) * 2.0f * PI));
                //Vector3 rotDisp = mul(tangentToWorld, disp);
                //Vector3 np = normalize(pos + mul(tangentToWorld, disp)*normalScale);
                //Vector3 np = normalize(pos + disp*normalScale);
                Vector3 np = normalize(pos + (disp.x * tangent + disp.z * bn) * normalScale);

                Vector3 newPos = getHeightPosition(np, scale, hs, octaves);


                if (length(prev) > 0.1)
                {
                    Vector3 n = normalize(cross(newPos - centerPos, prev - centerPos));
                    Vector3 nn = n;
                    //					if (dot(nn, normalize(pos)) < 0.0)
                    //					nn *= -1;

                    norm += nn;

                }
                prev = newPos;

            }


            return normalize(norm) * -1;
        }


 
        //	inline Vector3 getPlanetSurfaceNormal(in float4 v) {
        public Vector3 getPlanetSurfaceNormal(Vector3 v, Vector3 t, Vector3 bn, float nscale, int N, int octaves)
        {
            float scale = surfaceNoiseSettings2.z;
            float heightScale = surfaceNoiseSettings2.y;

//            float octaves = LodSurface(v);

            return getSurfaceNormal(v, scale, heightScale, nscale, t, bn, octaves, N);
        }



    }
}