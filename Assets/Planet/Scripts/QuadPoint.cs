using UnityEngine;
using System.Collections;


namespace LemonSpawn {

	public class QuadBlock {
		
		public int lod = 0;
		public QuadPoint[] P = new QuadPoint[4];
		public QuadPoint[] PReal = new QuadPoint[4];
		
		public QuadPoint center = new QuadPoint();
		public QuadPoint centerReal = new QuadPoint();

/*		public Matrix3D rotmat = new Matrix3D();
		public Matrix3D rotmatInv = new Matrix3D();
*/		
		public Vector3 binormal = new Vector3();
		public Vector3 tangent = new Vector3();
		public Vector3 normal = new Vector3();
		
		private PlanetSettings planetSettings = null;
		
		// for culling
		float[] dots = new float[4];
		
		
/*		public void setupShader() {
			planetSettings.activeShader.SetUniformParameter3f("binormal",
			                                                  binormal);
			planetSettings.activeShader.SetUniformParameter3f("tangent",
			                                                  tangent);
			planetSettings.activeShader.SetUniformParameter1f("lod", lod);
			
			
			planetSettings.activeShader.SetUniformParameterMatrix3fv("rotmatInv",
			                                                         rotmatInv.floatBuffer);
			
			planetSettings.activeShader.SetUniformParameterMatrix3fv("rotmat",
			                                                         rotmat.floatBuffer);
			
		}
*/		
		
		public QuadBlock( QuadPoint p1,  QuadPoint p2,  QuadPoint p3,
		                  QuadPoint p4, int level, PlanetSettings ps) {
			
			lod = level;
			
			for (int i = 0; i < P.Length; i++) {
				P[i] = new QuadPoint();
				PReal[i] = new QuadPoint();
			}
			planetSettings = ps;
			
			P[0].set(p1);
			P[1].set(p2);
			P[2].set(p3);
			P[3].set(p4);
			center.findCenter(p1, p2, p3, p4);
			
			for (int i = 0; i < 4; i++) {
				PReal[i].set(P[i]);
				PReal[i].P = PReal[i].P.normalized;
			}
			tangent = (PReal[1].P - PReal[0].P).normalized;
			binormal = (PReal[3].P - PReal[0].P).normalized;
			normal = center.P.normalized;
			
			centerReal.set(center);
			//mtmp = mtmp*(radius *(1+ps.surface.GetHeight(mtmp, (int)lod)));
			
			centerReal.P = centerReal.P.normalized*planetSettings.getPlanetSize()*(1+ps.surface.GetHeight(centerReal.P.normalized, 0));
//			centerReal.P.mulDirect(planetSettings.getPlanetSize() + planetSettings.perlin.getHeight(centerReal.P, lod));
			
			
						
			/*rotmat.ToTangentSpace(tangent, binormal, normal);
			rotmat.invert(rotmatInv);
			
			rotmatInv.toFloatBuffer3();
			rotmat.toFloatBuffer3();
			*/
			
			for (int i = 0; i < 4; i++) {
				PReal[i].P = PReal[i].P.normalized * planetSettings.getPlanetSize();
/*				PReal[i].P.mulDirect(planetSettings.getPlanetSize()
				                     + planetSettings.perlin.getHeight(PReal[i].P, lod));*/
			}
			
			
		}	
	}


	public class QuadPoint {
		public Vector3 P = new Vector3();
		
		public void set(QuadPoint o) {
			P = o.P;//.Set(o.P);
		}
		
		public void findCenter(QuadPoint p1, QuadPoint p2,
		                       QuadPoint p3, QuadPoint p4) {
			P = (p1.P + p2.P + p3.P + p4.P)/4f;
			
			// height = (p1.height + p2.height + p3.height+ p4.height)/4f;
		}
	}
	

}