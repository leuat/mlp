using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace LemonSpawn {
	
	public class LSMesh  {
		
		public Mesh mesh;
		
		public static GameObject parent = null;
		
		public List<int> faceList = new List<int>();
		
		public List<Vector3> normalList = new List<Vector3>();
		public List<Vector2> uvList = new List<Vector2>();
		public List<Vector3> vertexList = new List<Vector3>();
		public List<Vector4> tangentList = new List<Vector4>();
		
		protected int length = 0;
		
		public bool dynamicUV;
		
/*		public void CollapseFrom(LSMesh o) {
			LSMesh m = new LSMesh();
			foreach (int i in o.faceList)
				faceList.Add (i);
			
			
		}
*/		
	
		public void FacesFromVertices() {
			for (int i=0;i<vertexList.Count;i++) {
				faceList.Add (i);
			}
		}
	
		
		
		
		/*
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
			
		}*/
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
						uvList.Add (new Vector2(vertexList[v[i] ].x*s,vertexList[v[i] ].z*s));
					
				}
				else
				if (Mathf.Abs(use.x)<Mathf.Abs(use.z)) {
					for (int i=0;i<v.Length;i++)
						uvList.Add (new Vector2(vertexList[v[i] ].x*s,vertexList[v[i] ].y*s));
				}
				else{
					for (int i=0;i<v.Length;i++)
						uvList.Add (new Vector2(vertexList[v[i] ].z*s,vertexList[v[i] ].y*s));
				}
			}
				
		}	
		
		protected void smoothNormals() {
			List<Vector3> newNormals = new List<Vector3>();
			for (int i=0;i<vertexList.Count;i++) {
				Vector3 v = vertexList[i];
				Vector3 n = Vector3.zero;
				for (int j=0;j<faceList.Count/3;j++) {
					for (int k=0;k<3;k++)
						if ((v - vertexList[ faceList[3*j + k]]).magnitude<0.0001)
							n+= normalList[faceList[3*j + k]];
				}
				newNormals.Add (n.normalized);
				
			}
			normalList = newNormals;
			
		}
		
/*		protected Vector3 getBlending(Vector3 n, float pow) {
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
		
*/		
		
		public void Invert() {
			for (int i=0;i<faceList.Count/3;i++) {
				int j = faceList[3*i+1];
				faceList[3*i+1]=faceList[3*i+2];
				faceList[3*i+2]=j;
				
			}
		}
		protected void addFace(Vector3 x0, Vector3 x1, Vector3 x2, Vector3 x3) {
			addTriangle(x1,x2,x3);
			addTriangle(x1,x3,x0);
			
		}
		
		protected void addTriangle(Vector3 x0, Vector3 x1, Vector3 x2) {
			addTriangle(x0,x1,x2,false);
		}
		
		protected void addTriangle(Vector3 x0, Vector3 x1, Vector3 x2, bool sphericalNormals) {

			int currentVertex = vertexList.Count;
			vertexList.Add(x0);
			vertexList.Add(x1);
			vertexList.Add(x2);
			
			Vector3 tan = new Vector3(0,1,0);
//			Vector3 tan = (x0-x1).normalized;
			Vector4 tan2 = new Vector4(tan.x,tan.y, tan.z,1.0f);
			tangentList.Add (tan2);
			tangentList.Add (tan2);
			tangentList.Add (tan2);
			
			
			faceList.Add(currentVertex +0);
			faceList.Add(currentVertex +1);
			faceList.Add(currentVertex +2);
			
			/*if (sphericalNormals) {
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
			//createUV(3);*/
			createUV(new int[3] {currentVertex, currentVertex+1, currentVertex+2});
			
			
		}
		
		public void createMesh() {
			mesh = new Mesh();
			mesh.vertices = vertexList.ToArray(); 
			mesh.triangles = faceList.ToArray();
			mesh.uv = uvList.ToArray();
			mesh.normals = normalList.ToArray();
			mesh.tangents = tangentList.ToArray();
			
//			mesh.RecalculateNormals();
			mesh.Optimize();
			
		}
		
		public void addRandomPerturbations(float s) {
			Vector3 r = new Vector3();
			System.Random rnd = new System.Random(0);
			for (int i=0;i<vertexList.Count;i++) {
				r.x = (float)(rnd.NextDouble() - 0.5f)*s;
				r.y = (float)(rnd.NextDouble() - 0.5f)*s;
				r.z = (float)(rnd.NextDouble() - 0.5f)*s;
				vertexList[i] += r;
			}	
		}
		
		public GameObject Realize(string name, Material m, int layer, string tag, bool castShadows) {
			if (m==null)
				return null;
			GameObject go = new GameObject(name);
            go.tag = tag;
            go.layer = layer;
			if (LMesh.parent != null)
				go.transform.parent = LMesh.parent.transform;
			go.AddComponent<MeshFilter>();   
			go.AddComponent<MeshRenderer>();   
            if (!castShadows)
            {
                go.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                go.GetComponent<MeshRenderer>().receiveShadows = false;

            }
            ((MeshFilter)go.GetComponent<MeshFilter>()).mesh = mesh;
			if (m!=null)
				go.GetComponent<Renderer>().material = m;
			return go;
		}
		
		public void CapY(float maxY) {
			for (int i=0;i<vertexList.Count;i++) {
				
				vertexList[i] = new Vector3(vertexList[i].x, Mathf.Min (vertexList[i].y, maxY), vertexList[i].z);
				}
			createMesh();
		}
		
	}
	
	
}