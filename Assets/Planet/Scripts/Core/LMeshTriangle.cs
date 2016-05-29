using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace LemonSpawn {
	
	class LSMeshTriangle : LSMesh {
		
		public LSMeshTriangle(Vector3 x0, Vector3 x1, Vector3 x2, bool duv) {
			create(x0, x1,x2, duv);
		}
		
		public void create(Vector3 x0, Vector3 x1, Vector3 x2, bool dUV ) {
			dynamicUV = dUV;
			addTriangle(x0,x1,x2);
			createMesh();
			
		} 
		
		public void SetupVoronoi() {
			
		}
		
		public void AddVoronoiVertex(Vector3 p) {
		
		
		}
		
	}
	
	
}