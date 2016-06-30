using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace LemonSpawn {

	public class QuadField {
		public int sizeVBO;
		private int sizeVertices;
		private int sizeFaces;
		private int sizeMPIData;
		
		
		private PlanetSettings planetSettings;
		// private static TerrainQuad terrain;
		public float lod;
		
		private Vector3[] vertexField;
		private Vector3[] normalField;
		private Vector3[] vertexNormalField;
		private Vector3[] vertexHeightNormalField;
		public float[] dataNormals, dataVertices, dataColors, dataMPI, dataTexCoords;
		
		
		public bool OnlyNormals = false;
		
		// Actual amount of vertices!
		public int currentIdx = 0;
		
		private Vector3 Addx = new Vector3();
		private Vector3 Addy = new Vector3();
		
		private Vector3 mtmp = new Vector3();
		
		private Vector3 n1 = new Vector3();
		private Vector3 n2 = new Vector3();
		private Vector3 n3 = new Vector3();
		private Vector3 n4 = new Vector3();
		
		
		private LSMesh mesh = new LSMesh();
		
		
		private float radius;
		private Vector3 planeNormal = Vector3.zero;
		public void Init(int x) {
			
			sizeVertices = x + 3;
			sizeVBO = x;
			sizeFaces = (x + 2);
			
			
		 
			if (vertexField == null) {
				vertexField = new Vector3[sizeVertices * sizeVertices];
				for (int i = 0; i < vertexField.Length; i++)
					vertexField[i] = new Vector3();
			}
			if (vertexNormalField == null) {
				vertexNormalField = new Vector3[sizeVertices * sizeVertices];
				for (int i = 0; i < vertexNormalField.Length; i++)
					vertexNormalField[i] = new Vector3();
				
			}
			if (vertexHeightNormalField == null) {
				vertexHeightNormalField = new Vector3[sizeVertices * sizeVertices];
				for (int i = 0; i < vertexHeightNormalField.Length; i++)
					vertexHeightNormalField[i] = new Vector3();
				
			}
			
			if (normalField == null) {
				normalField = new Vector3[sizeFaces * sizeFaces];
				for (int i = 0; i < normalField.Length; i++)
					normalField[i] = new Vector3();
			}
			
			
/*			if (dataVertices == null)
				dataVertices = new float[8 * 3 * 3 * (sizeVBO) * (sizeVBO) / 4];
			
			if (dataNormals == null)
				dataNormals = new float[8 * 3 * 3 * (sizeVBO) * (sizeVBO) / 4];
			
			if (dataTexCoords == null)
				dataTexCoords = new float[8 * 3 * 2 * (sizeVBO) * (sizeVBO) / 4];
			
			if (dataColors == null)
				dataColors = new float[sizeVBO * sizeVBO * 3 * 4 * 2];
			
			sizeMPIData = dataVertices.Length + dataNormals.Length + dataTexCoords.Length + DATA_HEADER;
			
			if (dataMPI == null)
				dataMPI = new float[sizeMPIData];
*/			
			
			
			
		}

        private Vector3 P = Vector3.zero;			


        	
		public  void Setup( float rad,  Vector3 p1,  Vector3 p2,
		                         Vector3 p3) {
			radius = rad;
			Addx = (p2-p1)/sizeVBO;
			Addy = (p3-p1)/sizeVBO;
			
			P = p1;
			
			n1 = p1.normalized;
			n2 = p2.normalized;
			n3 = p3.normalized;
			n4 = n2 -n1;
			mtmp = n3-n1;
			
			planeNormal = Vector3.Cross(n4, mtmp).normalized;
			
		}
		
		 public void Calculate( float rad,  Vector3 p1,  Vector3 p2,
		                             Vector3 p3,  PlanetSettings ps,  int[] nbh) {
			planetSettings = ps;			
			Setup(rad, p1, p2, p3);
			
			calculateVertices(p1, ps);
			if (!RenderSettings.GPUSurface) {
				calculateNormalField();
				calculateNormalVertex();
			}

            FieldToFloat(nbh);
		}
		
		private void calculateNormalField() {
			for (int i = 0; i < sizeFaces; i++)
			for (int j = 0; j < sizeFaces; j++) {
				n1 = getVertex(i, j) - getVertex(i+1, j);
				n2 = getVertex(i, j) - getVertex(i, j+1);
				n3 = Vector3.Cross(n2, n1).normalized;
				putNormal(n3, i, j);
			}
			
		}
		
		private float minmax( float val,  float min,  float max) {
			if (val > max)
				return max;
			if (val < min)
				return min;
			return val;
			
		}
		
		
		
		private void calculateNormalVertex() {
			for (int i = 1; i < sizeVertices; i++)
			for (int j = 1; j < sizeVertices; j++) {
				n1 = Vector3.zero;
				int n = 0;
				for (int x = 0; x <= 1; x++)
				for (int y = 0; y <= 1; y++) {
					int l = x + i - 1;
					int m = y + j - 1;
					if (l >= 0 && l < sizeFaces && m > 0 && m < sizeFaces) {
						n1 = n1 + getNormal(l, m);
						n++;
					}
				}
				
				// n1.z(n1.z()/5f);
				n1 = n1.normalized;
				
				// n1.set(0,1,0);
				putVertexNormal(n1, i, j);
			}
			
		}
		
		private void calculateVertices( Vector3 p1,  PlanetSettings ps) {
			for (int i = 0; i < sizeVertices; i++)
			for (int j = 0; j < sizeVertices; j++) {
				mtmp = (p1 + Addx*(i-1) + Addy*(j-1)) / 8;//.normalized;
				
				if (RenderSettings.assProjection)
					mtmp = mtmp.normalized;
				
				mtmp = mtmp*radius*(1+ps.surface.GetHeight(mtmp, (int)lod));
				putVertex(mtmp, i, j);
			}
			
		}
		
		private  float getColorGaussian( float val,  float center,
		                                      float width,  float A) {
			return (float) Mathf.Exp(-(val - center) * (val - center) * width) * A;
		}
		
/*		private void mixColor( Vector3 v, float val,  float num) {
			mtmp = Vector3.zero;//.set(0, 0, 0);
			float A = 1;
			float width = 0.0002f;
			float blueCenter = 0;
			float greenCenter = 80;
			float brownCenter = 250;
			float greyCenter = 450;
			float whiteCenter = 500;
			
			if (num > 0.0 && num < 0.8)
				val = greyCenter;
			
			float blue = getColorGaussian(val, blueCenter, 5f * width, A);
			float green = getColorGaussian(val, greenCenter, 0.5f * width, A);
			float brown = getColorGaussian(val, brownCenter, 0.25f * width, A);
			// float white = getColorGaussian(val, whiteCenter, 0.25f*width,A);
			float grey = getColorGaussian(val, greyCenter, 0.15f * width, A);
			float sand = getColorGaussian(val, 0, 30f * width, A);
			
			n1.set(v);
			n1.normDirect();
			float n = (float) (Math.abs(n1.y()) * 4.0);
			colorTmp.set(colorGreen);
			
			// colorTmp.interpolate(colorYellow, colorGreen, n);
			
			if (val > whiteCenter)
				mtmp.set(colorWhite);
			else {
				mtmp.addDirect(colorTmp, green);
				mtmp.addDirect(colorBrown, brown);
				if (val < 0)
					mtmp.addDirect(colorBlue, blue);
				mtmp.addDirect(colorGrey, grey);
				mtmp.addDirect(colorSand, sand);
			}
			n = (float) (Math.abs(n1.y()) + 0.1f + val / 2000f);
			// n=n*n;
			if (n > 0.9 && val > 8 && num > 0.8) {
				// mtmp.addDirect(colorWhite,1f);
				// mtmp.normDirect();
				mtmp.set(colorWhite);
			}
			
		}
		
		private Vector3 getColor( int i,  int j) {
			float scale = 7500 * 3.5f;
			
			Vector3 v2 = getVertex(i, j);
			double val = scale * (v2.len() - radius);
			// val += val - scale*0.0005f*(d*val);
			double d = Math.abs(planeNormal.dot(getVertexNormal(i, j)));
			
			mixColor(v2, (float) val, (float) d);
			
			return mtmp;
		}
*/		
		public void FieldToFloat( int[] nbh) {
			
			currentIdx = 0;
			int s = 2;
			int min = s;
			int max = sizeVBO;
			int up = 0;
			int down = 1;
			int left = 2;
			int right = 3;
			int sss = sizeVBO;
			vertexTable.Clear();
			for (int i = 0; i < sss / 2; i++)
			for (int j = 0; j < sss / 2; j++) {
				//if (Settings.settings.renderSettings.hasVertices) 
				{
					
					int x = i * 2 + s;
					int y = j * 2 + s;
					
					// 8 fans
					
					if (y == min && nbh[up] == 1) {
						// UP NBH
						putVertexData(x, y, 0);
						putVertexData(x - 1, y - 1, 0);
						putVertexData(x + 1, y - 1, 0);
					} else {
						
						// UP
						putVertexData(x, y, 0);
						putVertexData(x - 1, y - 1, 0);
						putVertexData(x, y - 1, 0);
						
						putVertexData(x, y, 1);
						putVertexData(x, y - 1, 1);
						putVertexData(x + 1, y - 1, 1);
					}
					if (x == max && nbh[right] == 1) {
						// Right
						putVertexData(x, y, 2);
						putVertexData(x + 1, y - 1, 2);
						putVertexData(x + 1, y + 1, 2);
					} else {
						// Right
						putVertexData(x, y, 2);
						putVertexData(x + 1, y - 1, 2);
						putVertexData(x + 1, y, 2);
						
						putVertexData(x, y, 3);
						putVertexData(x + 1, y, 3);
						putVertexData(x + 1, y + 1, 3);
					}
					
					if (y == max && nbh[down] == 1) {
						// Down
						putVertexData(x, y, 4);
						putVertexData(x + 1, y + 1, 4);
						putVertexData(x - 1, y + 1, 4);
					} else {
						putVertexData(x, y, 4);
						putVertexData(x + 1, y + 1, 4);
						putVertexData(x, y + 1, 4);
						
						putVertexData(x, y, 5);
						putVertexData(x, y + 1, 5);
						putVertexData(x - 1, y + 1, 5);
					}
					
					if (x == min && nbh[left] == 1) {
						putVertexData(x, y, 6);
						putVertexData(x - 1, y + 1, 6);
						putVertexData(x - 1, y - 1, 6);
						
					} else {
						putVertexData(x, y, 6);
						putVertexData(x - 1, y + 1, 6);
						putVertexData(x - 1, y, 6);
						
						putVertexData(x, y, 7);
						putVertexData(x - 1, y, 7);
						putVertexData(x - 1, y - 1, 7);
					}
				}
			}
		}
		
		/*
	 * private void FieldTofloat_OLD() { int s = 1; int t = 1; for (int
	 * i=0;i<sizeVBO;i++) for (int j=0;j<sizeVBO;j++) {
	 * 
	 * if (Settings.settings.renderSettings.hasVertices) {
	 * 
	 * PexData(getVertex(i+0 +s,j+ 0 +s),i,j,0);
	 * putVertexData(getVertex(i+0+s,j+1+s),i,j,1);
	 * putVertexData(getVertex(i+1+s,j+1+s),i,j,2);
	 * 
	 * putVertexData(getVertex(i+1+s,j+1+s),i,j,3);
	 * putVertexData(getVertex(i+0+s,j+0+s),i,j,4);
	 putVert putVertexData(getVertex(i+1+s,j+0+s),i,j,5); } if
	 * (Settings.settings.renderSettings.hasNormals) {
	 * 
	 * putNormalData(getVertexNormal(i+0 +t,j+ 0 +t),i,j,0);
	 * putNormalData(getVertexNormal(i+0 +t,j+ 1 +t),i,j,1);
	 * putNormalData(getVertexNormal(i+1 +t,j+ 1 +t),i,j,2);
	 * 
	 * putNormalData(getVertexNormal(i+1 +t,j+ 1 +t),i,j,3);
	 * putNormalData(getVertexNormal(i+0 +t,j+ 0 +t),i,j,4);
	 * putNormalData(getVertexNormal(i+1 +t,j+ 0 +t),i,j,5); } if
	 * (Settings.settings.renderSettings.hasColors) {
	 * 
	 * putColorData(getColor(i+0 +s,j+ 0 +s),i,j,0);
	 * putColorData(getColor(i+0+s,j+1+s),i,j,1);
	 * putColorData(getColor(i+1+s,j+1+s),i,j,2);
	 * 
	 * putColorData(getColor(i+1+s,j+1+s),i,j,3);
	 * putColorData(getColor(i+0+s,j+0+s),i,j,4);
	 * putColorData(getColor(i+1+s,j+0+s),i,j,5); } }
	 * 
	 * }
	 */
		private  Vector3 getVertex(int i, int j) {
			return vertexField[i + j * sizeVertices];
		}
		
		private void putVertex( Vector3 p, int idx, int idy) {
			vertexField[idx + idy * sizeVertices] = p;
		}
		
		private  Vector3 getVertexNormal(int i, int j) {
			if (i >= 0 && i < sizeVertices && j >= 0 && j < sizeVertices)
				return vertexNormalField[i + j * sizeVertices];
			return Vector3.zero;
		}
		
		private void putVertexNormal( Vector3 p,  int idx,  int idy) {
			vertexNormalField[idx + idy * sizeVertices] = p;
		}
		
		private void putNormal( Vector3 p, int idx, int idy) {
			normalField[(idx + idy * sizeFaces)] = p;
		}
		
		private  Vector3 getNormal(int idx, int idy) {
			return normalField[(idx + idy * sizeFaces)];
		}
		
		public Vector3 currentPos = new Vector3();
		public float currentScale = 1;
		
		
		private void putVertexData_OLD( Vector3 p, int idx, int idy, int pos) {
			dataVertices[3 * 6 * (idx + idy * sizeVBO) + 0 + 3 * pos] = (float) p
				.x;
			dataVertices[3 * 6 * (idx + idy * sizeVBO) + 1 + 3 * pos] = (float) p
				.y;
			dataVertices[3 * 6 * (idx + idy * sizeVBO) + 2 + 3 * pos] = (float) p
				.z;
		}
		
		public LSMesh ReCalculate(int[] nbh, bool castShadows) {
  			mesh = new LSMesh();
			FieldToFloat(nbh);
			mesh.createMesh();

            return mesh;
//			return mesh.Realize(planetSettings.gameObject.name, planetSettings.atmosphere.m_groundMaterial, planetSettings.currentLayer, planetSettings.currentTag, castShadows);
			
		
		}




		public GameObject Realise(bool castShadows) {
			//mesh.FacesFromVertices();
			mesh.createMesh();

			if (RenderSettings.GPUSurface) { 
            	mesh.mesh.RecalculateNormals();
            	mesh.mesh.bounds = new Bounds(mesh.mesh.bounds.center, mesh.mesh.bounds.size*4);
//            	mesh.mesh.bounds.max = mesh.mesh.bounds.max*3;
            }
            mesh.mesh.Optimize();
            if (planetSettings != null)
                return mesh.Realize(planetSettings.gameObject.name, planetSettings.atmosphere.m_groundMaterial, planetSettings.properties.currentLayer, planetSettings.properties.currentTag, castShadows);
            else
                return null;
			
		}
		
		public Dictionary<int, int> vertexTable = new Dictionary<int, int>();
		
		public int VecToInt(int i, int j) {
			return (i * 1000 + j);
		}
		
		
		private void putVertexData( int x,  int y,  int pos) {
			
			
				Vector3 v = getVertex(x, y);
				Vector3 n = getNormal(x, y)*-1;
//                Vector3 t = (getVertex(x-1,y) - v).normalized;
//            Debug.Log(t);
				int idx = -1;
				
				if (vertexTable.ContainsKey(VecToInt(x,y)))
					idx = vertexTable[VecToInt(x,y)];
				//int idx = mesh.vertexList[v];//   mesh.getVertexIndex(v);//mesh.GetVertexIndex(v);
				if (RenderSettings.flatShading)
						idx=-1;
						
				if (idx==-1) {

					mesh.vertexList.Add (v);
					mesh.normalList.Add (n);
					
					
												
					Vector3 nn = v.normalized;
                float phi = Mathf.Atan2(nn.z, nn.x);
                float theta = Mathf.Acos(nn.y);
					float s = 10;

                mesh.uvList.Add (new Vector2(s*theta / Mathf.PI, s*phi/ (2 * Mathf.PI)));
                    phi += 0.01f;

                    Vector3 n2 = new Vector3(Mathf.Sin(theta)* Mathf.Cos(phi), Mathf.Cos(theta), Mathf.Sin(phi)*Mathf.Sin(theta));
                Vector3 t = (nn - n2).normalized; 

                    mesh.tangentList.Add(t);


                idx = mesh.vertexList.Count-1;
					vertexTable[VecToInt(x,y)] = idx;
					
				}
				mesh.faceList.Add (idx);
		}
		
		private void putNormalData( Vector3 p, int idx, int idy, int pos) {
			dataNormals[3 * 6 * (idx + idy * sizeVBO) + 0 + 3 * pos] = (float) p
				.x;
			dataNormals[3 * 6 * (idx + idy * sizeVBO) + 1 + 3 * pos] = (float) p
				.y;
			dataNormals[3 * 6 * (idx + idy * sizeVBO) + 2 + 3 * pos] = (float) p
				.z;
		}
		
		private void putColorData( Vector3 p, int idx, int idy, int pos) {
			dataColors[4 * 6 * (idx + idy * sizeVBO) + 0 + 4 * pos] = (float) p.x;
			dataColors[4 * 6 * (idx + idy * sizeVBO) + 1 + 4 * pos] = (float) p.y;
			dataColors[4 * 6 * (idx + idy * sizeVBO) + 2 + 4 * pos] = (float) p.z;
			dataColors[4 * 6 * (idx + idy * sizeVBO) + 3 + 4 * pos] = 1;
		}
		
	}
	
}