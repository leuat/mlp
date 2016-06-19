using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace LemonSpawn {


public class SurfaceNode {

	protected List<SurfaceNode> Inputs = new List<SurfaceNode>();
//	protected SurfaceNode Output;
	protected int type;
	public virtual float Calculate(Noise4D noise, Vector3 p) {
		return 0;
	}
	
		
}


public class SurfaceCombiner: SurfaceNode {
	
	public const int ADD = 0;
	public const int SUB = 1;
	public const int MUL = 2;
		
	float a, b;
	
	public SurfaceCombiner(int t, float _a, float _b) {
		a = _a;
		b = _b;
		type = t;
	}

		public SurfaceCombiner(int t, float _a, float _b,  SurfaceNode sn1, SurfaceNode sn2) {
		a = _a;
		b = _b;
		type = t;
		Inputs.Add (sn1);
		Inputs.Add (sn2);
	}
	
	
	public override float Calculate(Noise4D noise, Vector3 p) {
		if (Inputs.Count!=2) 
			return 0;
			
		float v1 = Inputs[0].Calculate(noise, p);
		float v2 = Inputs[1].Calculate(noise, p);
			
				
		if (type==ADD)
			return b*(a*v1 + (1-a)*v2);
		if (type==SUB)
			return b*(a*v1 - (1-a)*v2);
		if (type==MUL)
			return a*(v1*v2);
			
		return 0;
	}

}

public class SurfaceFilter : SurfaceNode {

	public const int MINMAX = 0;
	public const int POW = 1;
	public const int INV = 2;
	public const int SUB = 3;
	private SurfaceNode input;
	
	float amp, a,b;
	
	public SurfaceFilter(int _type, float _amp, float _a, float _b, SurfaceNode sn) {
		input = sn;
		type = _type;
		amp = _amp;
		a = _a;
		b = _b;
	}
	
	public override float Calculate(Noise4D noise, Vector3 p) {
		if (input == null)
			return 0;
		
		float v = input.Calculate(noise, p);
		
		if (type == MINMAX)
			return amp*Mathf.Clamp( v, a ,b);
		if (type == POW)
			return amp * Mathf.Pow(v, a);
		if (type == INV)
			return amp*(a-v);
		if (type == SUB)
			return amp*(v-a);
	
		return 0;		
	}
	
}


public class SurfaceGenerator : SurfaceNode {

	public const int PERLIN = 0;
	public const int MULTIRIDGE_MOUNTAIN = 1;
	public const int MULTIRIDGE_RIDGE = 2;
	public const int MULTIRIDGE_RIVER = 6;
	public const int SWISS = 3;
	public const int MULTIRIDGE_RIDGE_LOWRES = 4;
	public const int DOMAIN1 = 5;
	public const int FLAT = 10;
	float amp;
	float scale;
	string name;
	public float warp = 0.5f;
	float scaleAmp = 0;
	float scaleScale = 0;
	public float warpScale = 2.13935f;

	public SurfaceGenerator(int _type, float _amp, float _scale) {
		scale = _scale;
		type = _type;
		amp = _amp;
		name = Util.RandomString(6);
	}
		public SurfaceGenerator(int _type, float _amp, float _scale, float _scalescale, float _scaleamp) {
			scale = _scale;
			type = _type;
			amp = _amp;
			name = Util.RandomString(6);
			scaleScale = _scalescale;
			scaleAmp = _scaleamp;
		}
	
	public static float getMFval(Vector3 p, float s, int mfo, string name) {
//			return Util.getRidgedMf(p, s, mfo, 2.5f, 1.5f, 1.0f,0.6f, 0f, name, 0.2314f);
		return Util.PerlinModes(p,s,mfo);
	}
	
	
	private static Vector3 adde = new Vector3(5.2f,1.3f, 3.251f);
		private static Vector3 adde2 = new Vector3(1.23f,1.923f, 2.251f);
		public static float getDomainVal(Vector3 p, float s) {
	
		string n = "domainTest";
		int m = 4;
	
/*		Vector3 q = new Vector3( getMFval( p  ,s,m, n),
			              		 getMFval( p + new Vector3(5.2f,1.3f, 3.251f),s,m,n), 
			                     getMFval( p + new Vector3(1.239f,2.912f, 2.253f),s,m,n)
			              		 );*/
			              		 
		float s2 = s*0.1f;
			float a = 0.05f*s;
			Vector3 q = new Vector3( getMFval( p  ,s2,m, n),
			                         getMFval( p + adde,s2,m,n), 
			                        getMFval( p + adde2,s2,m,n)
			                        );

			Vector3 q2 = new Vector3( getMFval( p + a*q  ,s2,m, n),
			                        getMFval( p + a*q + 0.3125f*adde2,s2,m,n), 
			                        getMFval( p + a*q + 0.423f*adde,s2,m,n)
			                       
			                        );
			
		return getMFval( p + a*q2,s,m,n);
	
	}
	
				
	public override float Calculate(Noise4D noise, Vector3 p) {
	
		float s = scale;
		float scaleScale2 = scaleScale * 1.92315f;
		if (scaleAmp !=0) {
			float ws = Mathf.Clamp(warpScale*(1 + 0.05f*noise.raw_noise_4d(p.x*scaleScale2, p.y*scaleScale2, p.z*scaleScale2, warpScale*2)),0.01f,1000);
		
			s = Mathf.Clamp(s*(1 + scaleAmp*noise.raw_noise_4d(p.x*scaleScale, p.y*scaleScale, p.z*scaleScale, ws)),0.01f,1000);
		}
		int mfo = 6;
		
		
		if (type==PERLIN) 
			return amp*noise.octave_noise_4d(4, 4, s, p.x,p.y, p.z, 0.1293f, false);
		
		
		// getRidgedMf2(Vector3 p, float frequency, int octaves, float lacunarity, float warp, float offset, float gain, float initialOffset, string name, float seed) {
				
		if (type == MULTIRIDGE_MOUNTAIN) // 2.5, 0.5, 0.99, 1, 0.5
			return amp*Util.getRidgedMf(p, s, mfo, 2.2f, 0.5f, 1f,1.2f, 0.3f, name, noise.seed);
		
		if (type == DOMAIN1) {
			return amp*getDomainVal(p,s);
		}
					
			
		if (type == MULTIRIDGE_RIDGE) 
				return amp*Util.getRidgedMf(p, s, mfo, 2.5f, 1.5f, 1.0f,0.6f, 0f, name, noise.seed);
		if (type == MULTIRIDGE_RIVER) 
				return amp*Util.getRidgedMf(p, s, mfo, 2.3f, 1.5f, 1.0f,0.6f, 0f, name, noise.seed);
			//	return amp*Util.getRidgedMf2(p, s, mfo, 2.0f, 0f, 0.9f,0.7f, 0f, name, noise.seed);
			
			if (type == MULTIRIDGE_RIDGE_LOWRES) 
				return amp*Util.getRidgedMf(p, s, 3, 2.5f, 0.5f, 1.0f,0.5f, 0f, name, noise.seed);
			
		if (type == SWISS) 
			return amp*Util.swissTurbulence(p, scale, scale, 10, 2.0f, warp, 0.71f, 0.75f,1.0f,0);

		if (type== FLAT)
			return 0;

		return 0;			
	}

}

public class Surface {
	protected Noise4D noise = new Noise4D();
	
	
	
	public SurfaceNode surfaceNode;
	protected PlanetSettings planetSettings;
	
		public static SurfaceNode InitializeFlat(float a, float scale, PlanetSettings ps) {
		return new SurfaceGenerator(SurfaceGenerator.FLAT, 0,0,0,0);	
	}
	
		public static SurfaceNode InitializeTerraOld(float a, float scale, PlanetSettings ps) {
	
		float s = scale;
	
		SurfaceGenerator cont = new SurfaceGenerator(SurfaceGenerator.MULTIRIDGE_RIDGE, 40f*a, 0.742f*s, 1.9f, 0.5f);
//		SurfaceFilter p1 = new SurfaceFilter(SurfaceFilter.MINMAX, 1,0, 0.1f, cont);		
				
		
		SurfaceGenerator p2 = new SurfaceGenerator(SurfaceGenerator.MULTIRIDGE_MOUNTAIN, 17f*a, 13.19f*s, 4.9f, 0.09f);
		SurfaceGenerator p3 = new SurfaceGenerator(SurfaceGenerator.MULTIRIDGE_RIDGE, 20f*a, 15.19f*s, 8.9f, 0.09f);
		SurfaceGenerator p4 = new SurfaceGenerator(SurfaceGenerator.MULTIRIDGE_MOUNTAIN, 7f*a, 153.19f*s, 18.9f, 0.01f);

		SurfaceGenerator craters = new SurfaceGenerator(SurfaceGenerator.MULTIRIDGE_MOUNTAIN, 50f*a, 1.19f*s, 18.9f, 0.01f);
			
		SurfaceCombiner c2 = new SurfaceCombiner(SurfaceCombiner.ADD, 0.5f, 1, p2, p3);
		SurfaceCombiner c3 = new SurfaceCombiner(SurfaceCombiner.ADD, 0.75f, 1.5f, c2, p4);
			
		
		
		SurfaceFilter f2 = new SurfaceFilter(SurfaceFilter.SUB, 1, -0.02f, 1, c3);
		
		
				
				
		SurfaceCombiner ccrater = new SurfaceCombiner(SurfaceCombiner.ADD, 0.5f, 2f, craters, f2);
			
		SurfaceCombiner c1 = new SurfaceCombiner(SurfaceCombiner.MUL, 70, 0, cont, ccrater);
		
		SurfaceFilter add = new SurfaceFilter(SurfaceFilter.SUB, 1, 0.005f*a, 1, c1);
		SurfaceFilter f1 = new SurfaceFilter(SurfaceFilter.MINMAX, 1, -0.005f, 10, add);
		SurfaceGenerator small = new SurfaceGenerator(SurfaceGenerator.MULTIRIDGE_RIDGE, 0.5f*a, 522.19f*s, 18.9f, 0.01f);
		SurfaceCombiner end1 = new SurfaceCombiner(SurfaceCombiner.ADD, 0.5f, 4f, small, f1);
			
		
		return end1;
//		surfaceNode = f1;
	}
		public static SurfaceNode InitializeTerra(float hscale, float scale, PlanetSettings ps) {
			
			float s = scale;
			float a = 1/7000f;
			
			SurfaceGenerator cont = new SurfaceGenerator(SurfaceGenerator.MULTIRIDGE_RIDGE, 40f*a, 0.742f*s, 1.9f, 0.5f);
			//		SurfaceFilter p1 = new SurfaceFilter(SurfaceFilter.MINMAX, 1,0, 0.1f, cont);		
			
			
			SurfaceGenerator p2 = new SurfaceGenerator(SurfaceGenerator.MULTIRIDGE_MOUNTAIN, 17f*a, 13.19f*s, 4.9f, 0.09f);
			SurfaceGenerator p3 = new SurfaceGenerator(SurfaceGenerator.MULTIRIDGE_RIDGE, 20f*a, 15.19f*s, 8.9f, 0.09f);
			SurfaceGenerator p4 = new SurfaceGenerator(SurfaceGenerator.MULTIRIDGE_MOUNTAIN, 7f*a, 153.19f*s, 18.9f, 0.01f);
			
			SurfaceGenerator craters = new SurfaceGenerator(SurfaceGenerator.MULTIRIDGE_MOUNTAIN, 50f*a, 1.19f*s, 18.9f, 0.01f);
			
			SurfaceCombiner c2 = new SurfaceCombiner(SurfaceCombiner.ADD, 0.5f, 1, p2, p3);
			SurfaceCombiner c3 = new SurfaceCombiner(SurfaceCombiner.ADD, 0.75f, 1.5f, c2, p4);
			
			
			
			SurfaceFilter f2 = new SurfaceFilter(SurfaceFilter.SUB, 1, -0.02f, 1, c3);
			
			
			
			
			SurfaceCombiner ccrater = new SurfaceCombiner(SurfaceCombiner.ADD, 0.5f, 2f, craters, f2);
			
			SurfaceCombiner c1 = new SurfaceCombiner(SurfaceCombiner.MUL, 70, 0, cont, ccrater);
			
			SurfaceFilter add = new SurfaceFilter(SurfaceFilter.SUB, 1, 40f*a, 1, c1);
			SurfaceFilter f1 = new SurfaceFilter(SurfaceFilter.MINMAX, 1, -0.005f, 10, add);
			SurfaceGenerator small = new SurfaceGenerator(SurfaceGenerator.MULTIRIDGE_RIDGE, 0.1f*a, 522.19f*s, 18.9f, 0.01f);
			SurfaceCombiner end1 = new SurfaceCombiner(SurfaceCombiner.ADD, 0.5f, 4f, small, f1);
			
			
			
			
			return end1;
			//		surfaceNode = f1;
		}
		


		public static SurfaceFilter Continents(float a, float size, float max) {
			SurfaceGenerator cont = new SurfaceGenerator(SurfaceGenerator.MULTIRIDGE_RIDGE, 40*a, size, 1.8f, 0.55f);
			SurfaceGenerator cont2 = new SurfaceGenerator(SurfaceGenerator.MULTIRIDGE_RIDGE, 40*a, 4.5231f*size, 4.8f, 0.15f);
			//cont2.warpScale = 10000.23952f;
			
			SurfaceCombiner sc = new SurfaceCombiner(SurfaceCombiner.ADD, 0.5f, 1,cont, cont2);
			
			SurfaceFilter sub = new SurfaceFilter(SurfaceFilter.SUB, 1, 20*a ,1,sc);
			SurfaceFilter f1 = new SurfaceFilter(SurfaceFilter.MINMAX, 1, 0.000f, max, sub);
			//SurfaceFilter f1 = new SurfaceFilter(SurfaceFilter.POW, 1, 0.2f, max, sub);
			
			return f1;
		}
		
		public static SurfaceNode River(float scale, float amplitude, float steep, float sub) {
			SurfaceGenerator river = new SurfaceGenerator(SurfaceGenerator.MULTIRIDGE_RIVER, 2, scale, 0,0);
			
			SurfaceFilter rf1 = new SurfaceFilter(SurfaceFilter.SUB, 1, sub, 0,river);
			SurfaceFilter rf2 = new SurfaceFilter(SurfaceFilter.POW, 4*amplitude, steep, 0,rf1);
			SurfaceFilter rf3 = new SurfaceFilter(SurfaceFilter.MINMAX, 1, 0, 56*amplitude, rf2);
			
			return rf3;
		}
		
		
		public static SurfaceNode InitializeNew(float hscale, float scale, PlanetSettings ps) {
			
			float s = scale;
			float a = 1/7000f;
			
			SurfaceFilter f1 = Continents(a,0.223f*s, 0.01f);
//			return f1;
						
												
//			SurfaceGenerator mountains = new SurfaceGenerator(SurfaceGenerator.MULTIRIDGE_MOUNTAIN, 1f, 10.242f*s, 1.8f, 0.1f);
			//SurfaceGenerator mountains = new SurfaceGenerator(SurfaceGenerator.MULTIRIDGE_RIDGE, 1f, 103.242f*s, 1.8f, 0.3f);
			SurfaceGenerator mountains2 = new SurfaceGenerator(SurfaceGenerator.MULTIRIDGE_RIDGE, 1f, 50.442f*s, 1.8f, 0.123f);
			SurfaceFilter mountains2f = new SurfaceFilter(SurfaceFilter.INV, 0.6f,0.6f, 0, mountains2);
			
			//SurfaceCombiner cob = new SurfaceCombiner(SurfaceCombiner.ADD, 0.5f, 2, mountains, mountains2f);
			
			
			
			SurfaceGenerator LargeScale = new SurfaceGenerator(SurfaceGenerator.MULTIRIDGE_RIDGE_LOWRES, 1f, 3.242f*s, 1.8f, 0.2312f);
			
			//SurfaceFilter mscale = new SurfaceFilter(SurfaceFilter.INV, 0.3f,7f, 0, Continents(1, 5.621f*s, 10f));
			SurfaceFilter mscale = new SurfaceFilter(SurfaceFilter.POW, 1f,2f, 0, LargeScale);
			
			SurfaceCombiner mEnd = new SurfaceCombiner(SurfaceCombiner.MUL, 0.5f, 2f, mountains2f, mscale);
			
			SurfaceCombiner mEnd2 = new SurfaceCombiner(SurfaceCombiner.ADD, 0.5f, 1, mEnd, LargeScale);
			
									
			
			
			SurfaceCombiner end1 = new SurfaceCombiner(SurfaceCombiner.MUL, 0.5f, 1f, mEnd2, f1);
			
			
			SurfaceCombiner adde = new SurfaceCombiner(SurfaceCombiner.ADD, 0.5f, 2f, end1, f1);
            
//			SurfaceCombiner m2 = new SurfaceCombiner(SurfaceCombiner.MUL, 1f, 2f, Continents(2, 3.69235f, 0.02f), River(96.4321f, a*10.5f, 2, 1.0f));
			SurfaceGenerator lsp = new SurfaceGenerator(SurfaceGenerator.PERLIN,0.8f, 3.2351f,0,0);
			SurfaceFilter lsp2 = new SurfaceFilter(SurfaceFilter.SUB, 1f, 0.05f, 0,lsp);
			SurfaceFilter lsp3 = new SurfaceFilter(SurfaceFilter.MINMAX, 1f, -0.0f, 0.025f,lsp2);
			SurfaceCombiner m2 = new SurfaceCombiner(SurfaceCombiner.MUL, 1f, 2f, lsp3, River(96.4321f, a*5.5f, 2, 1.0f));
			
									
			adde = new SurfaceCombiner(SurfaceCombiner.SUB, 0.5f, 2f, adde, River(2.4321f, a, 4,0.8f));
			adde = new SurfaceCombiner(SurfaceCombiner.ADD, 0.5f, 2f, adde, m2);
			
			
			SurfaceGenerator pebbles = new SurfaceGenerator(SurfaceGenerator.PERLIN, 0.2f*a, 4503.242f*s, 0, 0);
			
			SurfaceFilter pebbleCap = new SurfaceFilter(SurfaceFilter.MINMAX, 1, 0.05f*a, 0.07f*a, pebbles);
			
//			adde = new SurfaceCombiner(SurfaceCombiner.ADD, 0.5f, 2f* ps.globalTerrainHeightScale, adde, pebbleCap);
			
			SurfaceFilter sf = new SurfaceFilter(SurfaceFilter.SUB, 1f, 1f*a, 0, adde);
			
			SurfaceFilter liquidCap2 = new SurfaceFilter(SurfaceFilter.MINMAX, 1,-0.002f, 1000, sf);
//			SurfaceFilter liquidCap = new SurfaceFilter(SurfaceFilter.MINMAX, 1,ps.liquidThreshold, 1000, liquidCap2);
			
			return liquidCap2;
			//		surfaceNode = f1;
		}





        public static SurfaceNode InitializeDesolate(float a, float scale, PlanetSettings ps) {
			
			float s = scale;
			
			
//				SurfaceGenerator cont = new SurfaceGenerator(SurfaceGenerator.MULTIRIDGE_RIDGE, 40f*a, 0.742f*s, 1.9f, 0.5f);
			//		SurfaceFilter p1 = new SurfaceFilter(SurfaceFilter.MINMAX, 1,0, 0.1f, cont);		
			
			
			SurfaceGenerator p2 = new SurfaceGenerator(SurfaceGenerator.MULTIRIDGE_MOUNTAIN, 17f*a, 13.19f*s, 4.9f, 0.09f);
			SurfaceGenerator p3 = new SurfaceGenerator(SurfaceGenerator.MULTIRIDGE_RIDGE, 20f*a, 15.19f*s, 8.9f, 0.09f);
			SurfaceGenerator p4 = new SurfaceGenerator(SurfaceGenerator.MULTIRIDGE_MOUNTAIN, 7f*a, 153.19f*s, 18.9f, 0.01f);
			
			SurfaceGenerator craters = new SurfaceGenerator(SurfaceGenerator.MULTIRIDGE_MOUNTAIN, 50f*a, 3.19f*s, 18.9f, 0.01f);
			
			SurfaceCombiner c2 = new SurfaceCombiner(SurfaceCombiner.ADD, 0.5f, 1, p2, p3);
			SurfaceCombiner c3 = new SurfaceCombiner(SurfaceCombiner.ADD, 0.75f, 1.5f, c2, p4);
			
			
			
			SurfaceFilter f2 = new SurfaceFilter(SurfaceFilter.SUB, 1, -0.02f, 1, c3);
			
			
			SurfaceCombiner ccrater = new SurfaceCombiner(SurfaceCombiner.ADD, 0.9f, 2f, craters, f2);
			
//			SurfaceCombiner c1 = new SurfaceCombiner(SurfaceCombiner.MUL, 60, 0, cont, ccrater);

									
			SurfaceFilter add = new SurfaceFilter(SurfaceFilter.SUB, 1, 0.000f, 1, ccrater);
			SurfaceFilter f1 = new SurfaceFilter(SurfaceFilter.MINMAX, 1, 0.000f, 10, add);
			
			
			return f1;
			//		surfaceNode = f1;
		}
		
	public Surface(PlanetSettings ps) {
		planetSettings = ps;
		if (ps.planetType!=null)
			surfaceNode = ps.planetType.Delegate(1f/planetSettings.radius, 1*ps.globalTerrainScale, planetSettings);
			
		else surfaceNode = InitializeFlat(0,0, planetSettings);
/*		if (planetSettings.planetType == PlanetSettings.PlanetType.Terra)
			surfaceNode = InitializeTerra(1f/planetSettings.radius, 1);
		if (planetSettings.planetType == PlanetSettings.PlanetType.Gas)
				surfaceNode =InitializeFlat(0,0);
*/		
	}
	
	
	public float GetHeight(Vector3 p, int lod) {
		noise.seed = planetSettings.seed / 2352f; 	
			
		if (surfaceNode == null)
			return 0;
		Vector3 p2 = p;
//		p2.z+=planetSettings.seed*0.01f;
		return surfaceNode.Calculate(noise, p2)*planetSettings.globalTerrainHeightScale;
		
	}


        public Vector3 GetNormal(Vector3 p, int lod, float ps)
        {
            noise.seed = planetSettings.seed / 2352f;

            if (surfaceNode == null)
                return Vector3.zero;

            float scale = 0.0001f;
            Vector3 any = Vector3.Cross(p, Vector3.up);
            Vector3 right = Vector3.Cross(any, p).normalized;
            Vector3 left = Vector3.Cross(right, p).normalized;

            float h1 = surfaceNode.Calculate(noise, (p).normalized);
            float h2 = surfaceNode.Calculate(noise, (p + left*scale).normalized);
            float h3 = surfaceNode.Calculate(noise, (p + right*scale).normalized);

            //            Debug.Log(h1 + " , " + h2 + ", " + h3);

            float s = 1000;
            Vector3 p1 = p.normalized * ps*(1+h1)*s;
            Vector3 p2 = (p + left * scale).normalized * ps*(1+h2)*s;
            Vector3 p3 = (p + right * scale).normalized * ps*(1+h3)*s;


            return Vector3.Cross(p1 - p2, p1 - p3).normalized;
            //		p2.z+=planetSettings.seed*0.01f;

        }




    }

}