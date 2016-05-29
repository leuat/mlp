using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace LemonSpawn {

class LSMeshBox : LSMesh {

	public LSMeshBox(Vector3 x0, Vector3 x1, Vector3 x2, Vector3 x3, Vector3 x4, Vector3 x5, Vector3 x6, Vector3 x7, bool duv) {
		create(x0, x1,x2,x3,x4, x5,x6,x7, duv);
	}
	
	public void create(Vector3 x0, Vector3 x1, Vector3 x2, Vector3 x3, Vector3 x4, Vector3 x5, Vector3 x6, Vector3 x7 , bool dUV ) {
    	dynamicUV = dUV;
    	addFace(x0,x1,x2,x3);
    	addFace(x6,x1,x0,x7);
    	addFace(x2,x5,x4,x3);
    	addFace(x4,x5,x6,x7);

    	addFace(x1,x6,x5,x2);
    	addFace(x0,x3,x4,x7);
		
		createMesh();
		
	} 


}


}