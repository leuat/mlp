using UnityEngine;
using System.Collections;

namespace LemonSpawn {

	public class CubeSphere {
		public QuadNode[] nodes;
		public PlanetSettings planetSettings;
		
		private bool isEnvironment = false;
		
		
		public void Delete() {
			for (int i = 0; i < 6; i++)
				nodes[i].deleteChildren();
			nodes = null;
		}
		
		public CubeSphere(PlanetSettings ps, bool isEnv) {
			planetSettings = ps;
			isEnvironment=isEnv;
//            Debug.Log(planetSettings);
			nodes = new QuadNode[6];
			
			QuadPoint[] p = new QuadPoint[8];
			float l = 5;
			for (int i = 0; i < p.Length; i++)
				p[i] = new QuadPoint();
			
			p[0].P = new Vector3(-l, -l, -l);
			p[1].P = new Vector3(l, -l, -l);
			p[2].P = new Vector3(l, l, -l);
			p[3].P = new Vector3(-l, l, -l);
			
			p[4].P = new Vector3(-l, -l, l);
			p[5].P = new Vector3(l, -l, l);
			p[6].P = new Vector3(l, l, l);
			p[7].P = new Vector3(-l, l, l);
			
			int back = 0;
			int front = 1;
			
			int left = 2;
			int right = 3;
			
			int top = 4;
			int bottom = 5;
			
			// Bak x
			nodes[back] = new QuadNode(p[1], p[0], p[3], p[2], 0, null, 0,
			                           planetSettings, isEnvironment, null);
			// Foran x
			nodes[front] = new QuadNode(p[4], p[5], p[6], p[7], 0, null, 0,
			                            planetSettings, isEnvironment,null);
			
			// Venstre side
			nodes[left] = new QuadNode(p[0], p[4], p[7], p[3], 0, null, 0,
			                           planetSettings, isEnvironment,null);
			// Høyre side
			nodes[right] = new QuadNode(p[5], p[1], p[2], p[6], 0, null, 0,
			                            planetSettings, isEnvironment,null);
			
			//
			// Topp y
			nodes[top] = new QuadNode(p[0], p[1], p[5], p[4], 0, null, 0,
			                          planetSettings, isEnvironment,null);
			// Bottom y
			nodes[bottom] = new QuadNode(p[7], p[6], p[2], p[3], 0, null, 0,
			                             planetSettings, isEnvironment,null);
			
			nodes[back].up = nodes[top];
			nodes[back].down = nodes[bottom];
			nodes[back].left = nodes[left];
			nodes[back].right = nodes[right];
			
			nodes[front].up = nodes[top];
			nodes[front].down = nodes[bottom];
			nodes[front].left = nodes[right];
			nodes[front].right = nodes[left];
			
			// Venstre side
			nodes[2].up = nodes[4];
			nodes[2].down = nodes[5];
			nodes[2].left = nodes[0];
			nodes[2].right = nodes[1];
			
			// Høyre side
			nodes[3].up = nodes[4];
			nodes[3].down = nodes[5];
			nodes[3].left = nodes[1];
			nodes[3].right = nodes[0];
			
			// Topp y side
			nodes[4].up = nodes[1];
			nodes[4].down = nodes[0];
			nodes[4].left = nodes[3];
			nodes[4].right = nodes[4];
			
			// Bottom y side
			nodes[5].up = nodes[0];
			nodes[5].down = nodes[1];
			nodes[5].left = nodes[3];
			nodes[5].right = nodes[4];
			
		}
		
		public void Setup() {
			
		}
		
		public void Realise() {
			for (int i = 0; i < nodes.Length; i++) {
				nodes[i].Realise();
			}
			
		}
		
/*		public void SetMaterial(Material mat) {
			for (int i = 0; i < nodes.Length; i++)
				nodes[i].SetMaterial(mat);
			
		}
*/		
		public void SubDivide(float gridDivide) {
			
			for (int i = 0; i < nodes.Length; i++)
				nodes[i].Subdivide(0, gridDivide);
			
			for (int i = 0; i < nodes.Length; i++)
				nodes[i].setupNeighbors();
			
		}
		
	}
	

}