using UnityEngine;
using System.Collections;



namespace LemonSpawn {

	public class CloudTexture : C2DMap {

		public void RenderCloud() {
		
			C2DMap m1 = new C2DMap();
			Vector2 aspect = new Vector2(1f,0.5f);
			m1.calculatePerlin(1f, 5, 3, 1, 0 , 0.4f, 6.1235f, aspect, true);
			C2DMap m2 = new C2DMap();
			m2.calculatePerlin(1f, 2, 3, 1, 0 , 0.1f, 2.1235f, aspect, true);
			
			this.Add (m1,0.3f);
			this.Add (m2, 0.7f);
			
//			public void calculatePerlin (float amplitude, float scale, float octaves, float kscale, float seed, float perlinSkew, float perlinSkewScale)
			
			Smooth(3,0); 
			Rotate();
			
		}
	}

}