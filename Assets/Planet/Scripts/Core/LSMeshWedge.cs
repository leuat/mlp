using UnityEngine;
using System.Collections;

namespace LemonSpawn {

class LSMeshWedge : LSMesh {
	
	
	
	
	public LSMeshWedge(Vector3 x0, Vector3 x1, Vector3 x2, Vector3 x3, Vector3 x4, Vector3 x5, bool duv) {
		create(x0, x1,x2,x3,x4, x5, duv);
	}
	public LSMeshWedge(Vector3 x0, Vector3 x1, Vector3 x2, bool duv) {
			create(x0, x1,x2, duv);
	}
	
	public static void stretchWedge(Vector3[] v, float s) {
		Vector3 d1 = (v[2] - v[0]).normalized;
//		Vector3 d2 = (v[2] - v[0]).normalized;
		v[2] = v[0] + d1*s;
		//v[2] = v[0] + d2*s;
			
	}
			
					
	public void create(Vector3 x0, Vector3 x1, Vector3 x2,   Vector3 x3, Vector3 x4, Vector3 x5 , bool dUV ) {
/*		int N = 3*(2+6);
		faceList = new int[(2+6)*3];
		normalList= new Vector3 [N];
		uvList = new Vector2 [N];
		vertexList = new Vector3 [N];
		tangentList = new Vector4 [N];
			
		currentVertex = 0;
		currentFace = 0;
*/			
		dynamicUV = true;
		//var vertexList = [ x0, x1, x2,x3, x4, x5, x6, x7]; 
		addTriangle(x0, x1, x2);
		addTriangle(x3, x5, x4);
		
		addFace(x0,x3,x4,x1);
		addFace(x1,x4,x5,x2);
		addFace(x2,x5,x3,x0);
		
		createMesh();		
		
	} 
	
		public void create(Vector3 x0, Vector3 x1, Vector3 x2,  bool dUV ) {
			dynamicUV = dUV;
			addTriangle(x0, x1, x2);
			
			createMesh();			
			
		} 
		
	
}


}
