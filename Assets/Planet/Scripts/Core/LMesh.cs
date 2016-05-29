using UnityEngine;
using System.Collections;

namespace LemonSpawn {

public class LMesh  {

		public Mesh mesh;
		
		public static GameObject parent = null;
		
		public int[] faceList;
		
		public Vector3[] normalList;
		public Vector2[] uvList;
		public Vector3[] vertexList;
		public Vector4[] tangentList;
		
		protected int currentVertex = 0;
		protected int currentFace = 0;
		protected int length = 0;
		
		public bool dynamicUV;
		
		public void InitializeVertices(int cnt, int f) {
			uvList = new Vector2[cnt];
			vertexList = new Vector3[cnt];
			tangentList = new Vector4[cnt];
			normalList = new Vector3[cnt];
			faceList = new int[f*3];
		}
	
	
	
		protected void createUVWHAT(int cnt) {	
			
			Vector3 dx = (vertexList[currentVertex + 1 ] - vertexList[currentVertex + 0 ]);
			Vector3 dy = (vertexList[currentVertex + 2 ] - vertexList[currentVertex + 0 ]);
			Vector3 wNorm = Vector3.Cross(dx,dy).normalized;
			
			Vector3 blending = getBlending(wNorm, 1.0f);			
			
			for (int i=0;i<cnt;i++) {
				//wNorm = vertexList[currentVertex+i].normalized;
				
				Vector3 v = vertexList[currentVertex + i ];
			
				float scale = 0.1f;
				
				Vector2 xaxis = new Vector2(v.z,v.y)*scale;
				Vector2 yaxis = new Vector2(v.x,v.z)*scale;
				Vector2 zaxis = new Vector2(v.x,v.y)*scale;
				
			
				uvList[currentVertex +i] = xaxis * blending.x + yaxis * blending.y + zaxis * blending.z; 
			}
	
		}
	protected void createUV(int[] v) {	
		if (dynamicUV) {		
			Vector3 dx = (vertexList[v[1] ] - vertexList[v[0] ]);
				Vector3 dy = (vertexList[v[2] ] - vertexList[v[1] ]);
			// first pick one of them: the one with lowest y-difference
			Vector3 use = Vector3.Cross(dx,dy).normalized;
			
			// Then, pick either x or z as your values
			float s = 0.1f;
			if (Mathf.Abs(use.y)>0.85f) {
				for (int i=0;i<v.Length;i++)
						uvList[v[i] ] = new Vector2(vertexList[v[i] ].x*s,vertexList[v[i] ].z*s);
				
			}
			else
			if (Mathf.Abs(use.x)<Mathf.Abs(use.z)) {
					for (int i=0;i<v.Length;i++)
						uvList[v[i] ] = new  Vector2(vertexList[currentVertex + i ].x*s,vertexList[v[i] ].y*s);
			}
			else{
					for (int i=0;i<v.Length;i++)
						uvList[v[i] ] = new  Vector2(vertexList[currentVertex + i ].z*s,vertexList[v[i] ].y*s);
			}
		}
		
	}	
	
		protected void smoothNormals() {
			Vector3[] newNormals = new Vector3[normalList.Length];
			for (int i=0;i<vertexList.Length;i++) {
				Vector3 v = vertexList[i];
				Vector3 n = Vector3.zero;
				for (int j=0;j<faceList.Length/3;j++) {
					for (int k=0;k<3;k++)
						if ((v - vertexList[ faceList[3*j + k]]).magnitude<0.0001)
					n+= normalList[faceList[3*j + k]];
				}
				newNormals[i] = n.normalized;
			
			}
			normalList = newNormals;
		
		}
	
		protected Vector3 getBlending(Vector3 n, float pow) {
			Vector3 blending = new Vector3();
			blending.Set (n.x, n.y, n.z);
			blending = blending.normalized;
			
			blending.x = Mathf.Abs (blending.x);
			blending.y = Mathf.Abs (blending.y);
			blending.z = Mathf.Abs (blending.z);
			
			
			
			blending.x = Mathf.Max (blending.x, 0.00001f);	
			blending.y = Mathf.Max (blending.y, 0.00001f);	
			blending.z = Mathf.Max (blending.z, 0.00001f);	
			
			
			blending.x = Mathf.Pow(blending.x, pow);
			blending.y = Mathf.Pow(blending.y, pow);
			blending.z = Mathf.Pow(blending.z, pow);
			
			
			blending = blending.normalized;
			float b = (blending.x + blending.y + blending.z);
			blending = blending / b;
			
			return blending;			
		}
	
	
		protected void createUVSpherical(int cnt) {	
			
			for (int i=0;i<cnt;i++) {
				
				
				Vector3 wNorm = vertexList[currentVertex + i];
				Vector3 blending = getBlending(wNorm, 1);
								
				
				Vector3 v = vertexList[currentVertex + i ];
				
				float scale = 0.1f;
				
				Vector2 xaxis = new Vector2(v.z,v.y)*scale;
				Vector2 yaxis = new Vector2(v.x,v.z)*scale;
				Vector2 zaxis = new Vector2(v.x,v.y)*scale;
				
				uvList[currentVertex +i] = xaxis * blending.x + yaxis * blending.y + zaxis * blending.z; 
			}
			
		}
		
		
						
		public void Invert() {
			for (int i=0;i<faceList.Length/3;i++) {
				int j = faceList[3*i+1];
				faceList[3*i+1]=faceList[3*i+2];
				faceList[3*i+2]=j;
				
			}
		}
		protected void addFace(Vector3 x0, Vector3 x1, Vector3 x2, Vector3 x3) {
			addTriangle(x1,x2,x3);
			addTriangle(x1,x3,x0);
			
		}
		
		protected void addFaceOld(Vector3 x0, Vector3 x1, Vector3 x2, Vector3 x3) {
		vertexList[currentVertex + 0 ] = x0;
		vertexList[currentVertex + 1 ] = x1;
		vertexList[currentVertex + 2 ] = x2;
		vertexList[currentVertex + 3 ] = x3;
		
		Vector3 tan = (x0-x1).normalized;
		Vector4 tan2 = new Vector4(tan.x,tan.y, tan.z,1.0f);
		tangentList[currentVertex +0] = tan2;
		tangentList[currentVertex +1] = tan2;
		tangentList[currentVertex +2] = tan2;
		tangentList[currentVertex +3] = tan2;
		
		
		faceList[currentFace+0] = currentVertex +0;
		faceList[currentFace+1] = currentVertex +1;
		faceList[currentFace+2] = currentVertex +3;
		
		faceList[currentFace+3] = currentVertex +1;
		faceList[currentFace+4] = currentVertex +2;
		faceList[currentFace+5] = currentVertex +3;
		
		createUV(new int[3] {currentVertex, currentVertex+1, currentVertex+2});
		createUV(new int[3] {currentVertex, currentVertex+3, currentVertex+2});
			
		currentVertex+=4;
		currentFace+=6;
		
	}
		protected void addTriangle(Vector3 x0, Vector3 x1, Vector3 x2) {
			addTriangle(x0,x1,x2,false);
		}
			
		protected void addTriangle(Vector3 x0, Vector3 x1, Vector3 x2, bool sphericalNormals) {
		
		vertexList[currentVertex + 0 ] = x0;
		vertexList[currentVertex + 1 ] = x1;
		vertexList[currentVertex + 2 ] = x2;
		
		Vector3 tan = new Vector3(0,1,0);//(x0-x1).normalized;
		Vector4 tan2 = new Vector4(tan.x,tan.y, tan.z,1.0f);
		tangentList[currentVertex +0] = tan2;
		tangentList[currentVertex +1] = tan2;
		tangentList[currentVertex +2] = tan2;
		
		
		faceList[currentFace+0] = currentVertex +0;
		faceList[currentFace+1] = currentVertex +1;
		faceList[currentFace+2] = currentVertex +2;
		
		if (sphericalNormals) {
				normalList[currentVertex + 0 ] = x0.normalized;
				normalList[currentVertex + 1 ] = x1.normalized;
				normalList[currentVertex + 2 ] = x2.normalized;
				
				for (int i=0;i<3;i++) {
					Vector3 t = Vector3.Cross (tan2, vertexList[currentVertex +i]).normalized;
					tangentList[currentVertex +i] = new Vector4(t.x, t.y, t.z, 1.0f);
				}
				
//				createUVSpherical(3);		
		}
//	else		
		//createUV(3);
		createUV(new int[3] {currentVertex, currentVertex+1, currentVertex+2});
			
		currentVertex+=3;
		currentFace+=3;
		
	}
	
	public void addRandomPerturbations(float s) {
		Vector3 r = new Vector3();
		System.Random rnd = new System.Random(0);
		for (int i=0;i<vertexList.Length;i++) {
			r.x = (float)(rnd.NextDouble() - 0.5f)*s;
				r.y = (float)(rnd.NextDouble() - 0.5f)*s;
				r.z = (float)(rnd.NextDouble() - 0.5f)*s;
			vertexList[i] += r;
			}	
	}
	
	public GameObject Realize(string name, Material m) {
		if (m==null)
			return null;
		GameObject go = new GameObject(name);
		if (LMesh.parent != null)
			go.transform.parent = LMesh.parent.transform;
		go.AddComponent<MeshFilter>();   
		go.AddComponent<MeshRenderer>();   
		((MeshFilter)go.GetComponent<MeshFilter>()).mesh = mesh;
		if (m!=null)
			go.GetComponent<Renderer>().material = m;
		return go;
	}
	
	}
	
	
}