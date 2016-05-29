using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace LemonSpawn {

class LSMeshGenericBox : LSMesh {

	public LSMeshGenericBox(Vector3[] v, float h) {
		create(v,h);
	}
	
		
	public void create(Vector3[] v, float height) {
   	 	dynamicUV = true;
   	 	int N = (v.Length);
		Vector3 centerD = Vector3.zero;
		for (int i=0;i<v.Length;i++)
			centerD+=v[i];
		centerD/=v.Length;
		Vector3 h = new Vector3(0,height,0);
		Vector3 centerU = centerD + h;
			
			
		for ( int i=0;i<N;i++) {
			addTriangle(centerD, v[(i+1)%N], v[(i)%N]); 
			addTriangle(centerU, v[(i)%N] +h, v[(i+1)%N] +h); 
			addFace(v[(i)%N], v[(i+1)%N ], v[(i+1)%N]+h, v[(i)%N]+h);
		}
			
   	 	createMesh();
		
	} 



}

}