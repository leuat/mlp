
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
 * 
 * © 2014 LemonSpawn. All rights reserved. Source code in this project ("LemonSpawn") are not supported under any LemonSpawn standard support program or service. 
 * The scripts are provided AS IS without warranty of any kind. LemonSpawn disclaims all implied warranties including, without limitation, 
 * any implied warranties of merchantability or of fitness for a particular purpose. 
 * 
 * Basically, do what you want with this code, but don't blame us if something blows up. 
 * 
 * 
*/
using System.IO;

namespace LemonSpawn
{

/*
 * 
 * 2D map class - stores all data to be used in the terrain generation process. 
 * 
 * */
		public class LineState
		{
				public Vector2 p1 = new Vector2 (); // Translate
				public Vector2 p2 = new Vector2 (); // Position
				public float dir = 0.0f;
 		
				public void Rotate (float r)
				{
						p2 = Util.Rotate2D (p2 - p1, r) + p1;
			
				}

				public void Translate (Vector2 v)
				{
						p1 = p1 + Util.Rotate2D (v, dir);
				}
		
		}
 
		public class Walker
		{
				public Vector2 prevPos = new Vector2 ();
				public Vector2 position = new Vector2 ();
				public float angle, dAngle, ddAngle;
				public float width, dWidth;
				public int level;
				public float dStep;
				public float childProb;
				public float childLeft;
				public static bool allDone = true;
				public Vector2 step = new Vector2 ();
				public List<Walker> children = new List<Walker> ();
				public C2DMap map;
				int steps = 200 + (int)(Util.rnd.NextDouble () * 10);
				float childVal;
				public float angleScale;

				public Walker (C2DMap m, float x, float y, float a, float da, float dda, float w, float dw, int l, float ds, float cp, float ass)
				{
						position.Set (x, y);
						angle = a;
						dAngle = da;
						ddAngle = dda;
						width = w;
						dWidth = Mathf.Max (dw, 0.20f);
						level = l;
						dStep = ds;
						childProb = cp;
						map = m;
						prevPos.Set (x, y);
						step.Set (0, 1);
						childLeft = childProb;
						angleScale = ass;
				}
 		
				public void Advance ()
				{
						if (width > 0.5 && steps-- > 0) {
								position = position + Util.Rotate2D (step, angle) * dStep;
								map.DrawLine (prevPos.x, prevPos.y, position.x, position.y, 0.1f, false, (int)width);
								width -= Mathf.Abs (dWidth);
								angle += dAngle;
								dAngle += ddAngle;
								if (Util.rnd.NextDouble () > 0.80f && level > 0 && children.Count < level) {
										children.Add (new Walker (map, position.x, position.y, angle + 0.4f * ((float)Util.rnd.NextDouble () - 0.5f), 
					                        0.1f * ((float)Util.rnd.NextDouble () - 0.5f), angleScale * 0.02f * ((float)Util.rnd.NextDouble () - 0.5f), 
					                        width + 3f * ((float)Util.rnd.NextDouble () - 0.2f), dWidth - 0.4f * ((float)Util.rnd.NextDouble () - 0.0f), level - 1, dStep, childProb * 0.8f, angleScale)); 
								}
								prevPos.Set (position.x, position.y);
								allDone = false;					
						}
 			 
						foreach (Walker w in children) 
								w.Advance ();
 		
				}
 		
 		
 	
		}
 
		public class C2DRGBMap
		{
				public Color[,] colors = null;
 		
				public C2DRGBMap ()
				{
						colors = new Color[C2DMap.sizeX, C2DMap.sizeY];
				}

				public void Scale (float v)
				{
						for (int i=0; i<C2DMap.sizeX; i++)
								for (int j=0; j<C2DMap.sizeY; j++)
										colors [i, j] *= v;
				}

				public Color Min(Color a, Color b) {
					Color c = new Color();
					c.a = a.a;
					c.r = Mathf.Min (a.r, b.r);
					c.g = Mathf.Min (a.g, b.g);
					c.b = Mathf.Min (a.b, b.b);
					return c;
			
				}
				public Color Max(Color a, Color b) {
					Color c = new Color();
					c.a = a.a;
					c.r = Mathf.Max (a.r, b.r);
					c.g = Mathf.Max (a.g, b.g);
					c.b = Mathf.Max (a.b, b.b);
					return c;
			
				}


				public Color getColor (int i, int j)
				{
						if (i < 0)
								i += C2DMap.sizeX;
						if (j < 0)
								j += C2DMap.sizeY;
						if (i >= C2DMap.sizeX)
								i -= C2DMap.sizeX;
						if (j >= C2DMap.sizeY)
								j -= C2DMap.sizeY;
			
						return colors [i, j];
				}
		
				public void CopyFrom (C2DRGBMap m)
				{
						for (int i=0; i<C2DMap.sizeX; i++) {
								for (int j=0; j<C2DMap.sizeY; j++) {   
										colors [i, j] = m.colors [i, j];
								}
						}
				}

				public void Smooth (int N)
				{
						C2DRGBMap copy = new C2DRGBMap ();
						Color sum = new Color ();
						int s = 1;
						for (int i=0; i<N; i++) {
								for (int x = 0; x < C2DMap.sizeX; x++) {
										for (int y = 0; y < C2DMap.sizeY; y++) {
												sum.r = 0;
												sum.g = 0;
												sum.b = 0;
												for (int k=-s; k<=s; k++)
														for (int l=-s; l<=s; l++) {
																sum += getColor (x + k, y + l);
														}
												copy.colors [x, y] = sum / ((2 * s + 1) * (2 * s + 1));
										}
								}
								CopyFrom (copy);
						}
			
				}
		
				public void toTexture (Texture2D t)
				{
				
						for (int i=0; i<C2DMap.sizeX; i++) 
								for (int j=0; j<C2DMap.sizeY; j++) {
										colors[i,j].a = 1;
										t.SetPixel (i, j, colors [i, j]);
						}
				}
				
		public void fromC2DMap (C2DMap o, Color c)
		{
			for (int i=0; i<C2DMap.sizeX; i++)
				for (int j=0; j<C2DMap.sizeY; j++)
					colors [i, j] = c*o.map[i,j];
		}
		
		}
 
		public class C2DMap
		{
				public float[,] map = null;
		public static int sizeX = RenderSettings.CloudTextureSize, sizeY = RenderSettings.CloudTextureSize;
		// The current random shift due to terrain position
				public static Vector2 shift = new Vector2 (0, 0);
				public float maxVal, minVal;

				public float this [int i, int j] {
						get {
								return map [i, j];
						}
						set {
								map [i, j] = value;
						}
				}


				public Texture2D ToTexture(Color c) {
					C2DRGBMap rgb = new C2DRGBMap();
					rgb.fromC2DMap(this, c);
					Texture2D t = new Texture2D(C2DMap.sizeX, C2DMap.sizeY);
					rgb.toTexture(t);
					byte[] b = t.EncodeToPNG();
					t.Apply();
					//File.WriteAllBytes(Application.dataPath + "/tex.png", b);
					return t;
				}

				// Make sure Size is always set. 
				public C2DMap ()
				{
						map = new float[sizeX, sizeY];
						zero ();
				}

				public void CopyFrom (C2DMap m)
				{
						for (int i=0; i<sizeX; i++) {
								for (int j=0; j<sizeY; j++) {   
										map [i, j] = m.map [i, j];
								}
						}
				}

		
				public void zero ()
				{
						for (int i=0; i<sizeX; i++)
								for (int j=0; j<sizeY; j++)
										map [i, j] = 0;
				}

				public void Clamp (float min, float max)
				{
						for (int i=0; i<C2DMap.sizeX; i++)
								for (int j=0; j<C2DMap.sizeY; j++) {
										map [i, j] = Mathf.Clamp (map [i, j], min, max);
								}
			
				}
		
				public void calculateStatistics ()
				{
						minVal = 1E16f;
						maxVal = -1E16f;
						for (int i=0; i<sizeX; i++)
								for (int j=0; j<sizeY; j++) {
										minVal = Mathf.Min (map [i, j], minVal);
										maxVal = Mathf.Max (map [i, j], maxVal);
								}
        
				}

				public Vector2 clampVector2 (Vector2 v)
				{
						if (v.x < 0)
								v.x += 1;
						if (v.y < 0)
								v.y += 1;
						if (v.x >= 1)
								v.x -= 1;
						if (v.y >= 1)
								v.y -= 1;
			
/*			if (v.x<0) v.x+=1;
			if (v.y<0) v.y+=1;
			if (v.x>=1) v.x-=1;
			if (v.y>=1) v.y-=1;*/
						return v;
				}

				public static int mod (int x, int m)
				{
						return (x % m + m) % m;
				}

				public float getPixel (int i, int j)
				{
						i = mod (i, sizeX);	
						j = mod (j, sizeY);	
						return map [i, j];
				}

				public void setPixel (int i, int j, float v)
				{
						i = mod (i, sizeX);	
						j = mod (j, sizeY);	
			
						map [i, j] = v;
				}

				private Vector3 getDistanceVector (int i, int j, int di, int dj, float scale)
				{
						return new Vector3 (di, dj, scale * (getPixel (i, j) - getPixel (i + di, j + dj)));
				}
		
				public void Normalize (float s)
				{
						float min = 1E30f;
						float max = -1E30f;
						for (int i=0; i<sizeX; i++)
								for (int j=0; j<sizeY; j++) {
										min = Mathf.Min (map [i, j], min);
										max = Mathf.Max (map [i, j], min);
								}
						if ((max - min) != 0)
								for (int i=0; i<sizeX; i++)
										for (int j=0; j<sizeY; j++) 
												map [i, j] = s * (map [i, j] / (max - min) - min);
						//map[i,j] = map[i,j]/max;
			
				}
                
				public void createNormalMap (C2DRGBMap t, float scale)
				{
						Vector3 v1 = new Vector3 ();
						Vector3 v2 = new Vector3 ();
						Vector3 v3 = new Vector3 ();
						Vector3 v4 = new Vector3 ();
			
						Vector3 n1 = new Vector3 ();
						Vector3 n2 = new Vector3 ();
						Vector3 n3 = new Vector3 ();
						Vector3 n4 = new Vector3 ();
						Vector3 N;
						Color c = new Color (1, 1, 1);
						for (int i=0; i<sizeX; i++) {
								for (int j=0; j<sizeY; j++) {
										v1 = getDistanceVector (i, j, 0, -1, scale);
//					if (j==50) Debug.Log (v1);
										v2 = getDistanceVector (i, j, 1, 0, scale);
										v3 = getDistanceVector (i, j, 0, 1, scale);
										v4 = getDistanceVector (i, j, -1, 0, scale);
					
										n1 = Vector3.Cross (v1, v2);
										n2 = Vector3.Cross (v2, v3);
										n3 = Vector3.Cross (v3, v4);
										n4 = Vector3.Cross (v4, v1);
										N = (n1 + n2 + n3 + n4).normalized;
										/*			c.r = (N.x*0.5f + 0.5f);
					c.g = (N.y*0.5f + 0.5f);
					c.b = (N.z*0.5f + 0.5f);*/
					
										c.r = (N.x * 0.5f + 0.5f);
										c.g = (N.y * 0.5f + 0.5f);
										c.b = (N.z * 0.5f + 0.5f);
										c.a = 1.0f;//(N.z*0.5f + 0.5f);
										t.colors [i, j] = c;
								}
						}
			
						//t.Apply();
				}
		
				Vector3 avg = new Vector3 ();
		
				public float getCurvature (int i, int j, float scale, int curvtype, Vector3[,] normals)
				{
						float h = map [i, j];
						float f = getNormalChangePeriodic (i, j, normals);
						f *= scale;
						float avgH = getAveragePeriodic (i, j, map);
						//if (f < tilt)// && h < avgH)
						float val = 0;
						if (curvtype == 1 && h > avgH) 
								val = Mathf.Clamp (1 - f, 0, 1);
						if (curvtype == 2 && h < avgH) 
								val = Mathf.Clamp (1 - f, 0, 1);
						if (curvtype == 3) 
								val = Mathf.Clamp (1 - f, 0, 1);
						if (curvtype == 4 && h > avgH) 
								val = Mathf.Clamp (f, 0, 1);
						if (curvtype == 5 && h < avgH) 
								val = Mathf.Clamp (f, 0, 1);
						if (curvtype == 6) 
								val = Mathf.Clamp (f, 0, 1);
						return val;
			
				}
		
				private float getNormalChange (int i, int j, Vector3[,] n)
				{
						avg.Set (n [i, j].x, n [i, j].y, n [i, j].z);
						float val = 1;
						int s = 1;
						for (int x =-1*s; x<=1*s; x++)
								for (int y =-1*s; y<=1*s; y++) {
										int xx = x + i;
										int yy = y + j;
										if (xx >= C2DMap.sizeX)
												xx -= C2DMap.sizeX;
										if (xx < 0)
												xx += C2DMap.sizeX;
										if (yy >= C2DMap.sizeY)
												yy -= C2DMap.sizeY;
										if (yy < 0)
												yy += C2DMap.sizeY;
				
										val *= Vector3.Dot (avg, n [xx, yy]);
				
								}
			
						return val;
			
				}
		
				private float getAverage (int i, int j, float[,] h)
				{
						float avg = 0;
						int s = 1;
						for (int x =-1*s; x<=1*s; x++)
								for (int y =-1*s; y<=1*s; y++) {
										int xx = x + i;
										int yy = y + j;
										if (xx >= C2DMap.sizeX)
												xx -= C2DMap.sizeX;
										if (xx < 0)
												xx += C2DMap.sizeX;
										if (yy >= C2DMap.sizeY)
												yy -= C2DMap.sizeY;
										if (yy < 0)
												yy += C2DMap.sizeY;
										avg += h [xx, yy];
				
				
								}
						avg /= (2f * s + 1.0f) * (2f * s + 1.0f);
						return avg;
						// Find if it is top or bottom
			
				}
		
				private float getNormalChangePeriodic (int i, int j, Vector3[,] n)
				{
						avg.Set (n [i, j].x, n [i, j].y, n [i, j].z);
						float val = 1;
						int s = 1;
						for (int x =-1*s; x<=1*s; x++)
								for (int y =-1*s; y<=1*s; y++) {
										int xx = x + i;
										int yy = y + j;
										if (xx >= C2DMap.sizeX)
												xx -= C2DMap.sizeX;
										if (xx < 0)
												xx += C2DMap.sizeX;
										if (yy >= C2DMap.sizeY)
												yy -= C2DMap.sizeY;
										if (yy < 0)
												yy += C2DMap.sizeY;
				
										val *= Vector3.Dot (avg, n [xx, yy]);
				
								}
			
						return val;
			
				}
		
				private float getAveragePeriodic (int i, int j, float[,] h)
				{
						float avg = 0;
						int s = 1;
						for (int x =-1*s; x<=1*s; x++)
								for (int y =-1*s; y<=1*s; y++) {
										int xx = x + i;
										int yy = y + j;
										if (xx >= C2DMap.sizeX)
												xx -= C2DMap.sizeX;
										if (xx < 0)
												xx += C2DMap.sizeX;
										if (yy >= C2DMap.sizeY)
												yy -= C2DMap.sizeY;
										if (yy < 0)
												yy += C2DMap.sizeY;
										avg += h [xx, yy];
				
				
								}
						avg /= (2f * s + 1.0f) * (2f * s + 1.0f);
						return avg;
						// Find if it is top or bottom
			
				}
        
				public void calculateNormals (float scale, Vector3[,] n)
				{
						Vector3 v1 = new Vector3 ();
						Vector3 v2 = new Vector3 ();
						Vector3 v3 = new Vector3 ();
						Vector3 v4 = new Vector3 ();
			
						Vector3 n1 = new Vector3 ();
						Vector3 n2 = new Vector3 ();
						Vector3 n3 = new Vector3 ();
						Vector3 n4 = new Vector3 ();
						Vector3 N;
						for (int i=0; i<sizeX; i++) {
								for (int j=0; j<sizeY; j++) {
										v1 = getDistanceVector (i, j, 0, -1, scale);
										//					if (j==50) Debug.Log (v1);
										v2 = getDistanceVector (i, j, 1, 0, scale);
										v3 = getDistanceVector (i, j, 0, 1, scale);
										v4 = getDistanceVector (i, j, -1, 0, scale);
					
										n1 = Vector3.Cross (v1, v2);
										n2 = Vector3.Cross (v2, v3);
										n3 = Vector3.Cross (v3, v4);
										n4 = Vector3.Cross (v4, v1);
										N = (n1 + n2 + n3 + n4).normalized;
										N.z = 0.5f;
										n [i, j] = N;
								}
						}
			
						//t.Apply();
				}
		
				public void DrawL2 (Vector2 p1, Vector2 p2, Vector2 direction, float angle, string s, int index, float dx, float amplitude, float thickness)
				{
						for (int i=index; i<s.Length; i++) {
								if (s [i] == ']')
										return;
								if (s [i] == '[') {
					
										DrawL2 (p1, p2, Util.Rotate2D (direction, angle), angle, s, i + 1, dx, amplitude, thickness);
										direction = Util.Rotate2D (direction, -angle);
					
								}
					
								if (s [i] == 'l') {
										p2 = p2 + direction * dx;
										DrawLine (p1.x, p1.y, p2.x, p2.y, amplitude, false, (int)thickness);
										p1.Set (p2.x, p2.y);
								}
								if (s [i] == 'r') {
										//direction = Util.Rotate2D(direction, -angle);
										p2 = p2 + direction * dx;
										DrawLine (p1.x, p1.y, p2.x, p2.y, amplitude, false, (int)thickness);
										p1.Set (p2.x, p2.y);
								}

								if (p2.x >= sizeX) {
										p2.x -= sizeX;
										p1.x -= sizeX;
								}
								if (p2.y >= sizeY) {
										p2.y -= sizeY;
										p1.y -= sizeY;
								}
								if (p2.x < 0) {
										p2.x += sizeX;
										p1.x += sizeX;
								}
								if (p2.y < 0) {
										p2.y += sizeY;
										p1.y += sizeY;
								}
				
						}
				
				}
			
				public void L2System (string s, string case1, string rule1, string case2, string rule2, int levels, float amplitude, float thickness, float angle, float size)
				{
						Vector2 p1 = new Vector2 (sizeX / 2, sizeY);			
						Vector2 p2 = new Vector2 (sizeX / 2, sizeY);
						Vector2 dir = new Vector2 (0, 1);			
			
						levels *= 2;
						s = s.Substring (6);
						rule1 = rule1.Substring (6);
						case1 = case1.Substring (6);
						rule2 = rule2.Substring (6);
						case2 = case2.Substring (6);
						if (s.Length == 0)
								return;			
						for (int i = 0; i<levels; i++) {
								if (case2.Length != 0 && rule2.Length != 0)
										s = s.Replace (case2, rule2);
								if (s.Length > 5000)
										break;
				
								if (case1.Length != 0 && rule1.Length != 0)
										s = s.Replace (case1, rule1);
								if (s.Length > 5000)
										break;
				
						}
						Debug.Log ("String: " + s);
						zero ();

						DrawL2 (p1, p2, dir, angle, s, 0, size * sizeX, amplitude, thickness);			
				}
		
				float[] fractions;
				float[] angles;
		
				public void Snowflake (int depth, float size)
				{
						fractions = new float[depth];
						angles = new float[depth];
						for (int i = 0; i<depth; i++) {
								fractions [i] = 0.5f + 0.25f * (Random.value - Random.value);
								angles [i] = (60 + Random.value * 40 - Random.value * 40) / 1.0f;
						}
						zero ();
			
			
						LineState ls = new LineState ();
						lineState = ls;
						ls.p1 = new Vector2 (0, 0);
						ls.p2 = new Vector2 (0.1f * sizeX, 0);
						ls.dir = 0;
						stack.Push (ls);
						int N = 8;
						lineState.Translate (new Vector2 (0.3f * sizeX, 0));
						for (int i=0; i<N; i++) {
								DrawLine ((int)lineState.p1.x + sizeX / 2, 
				         	(int)lineState.p1.y + sizeY / 2, 
				         	(int)lineState.p1.x + sizeX / 2 + (int)lineState.p2.x, 
				        	(int)lineState.p1.y + sizeY / 2 + (int)lineState.p2.y, 0.5f + (i + 1) / (float)N, false, 3);
				
								//lineState.Translate(new Vector2(0.1f*sizeX, 0));
								lineState.Rotate (6.28f / (N - 1));	
				
						}
						for (int i = 0; i<6; i++) {
								//BranchSnowflake(size*sizeX,  0, depth);
						}
		
				}
		
				Stack<LineState> stack = new Stack<LineState> ();
				LineState lineState;

				public void push ()
				{
						LineState ls = new LineState ();
						ls.p1 = new Vector2 (stack.Peek ().p1.x, stack.Peek ().p1.y);
						ls.p2 = new Vector2 (stack.Peek ().p2.x, stack.Peek ().p2.y);
						ls.dir = stack.Peek ().dir;
						stack.Push (ls);
						lineState = ls;
				}

				public void pop ()
				{
						stack.Pop ();
						lineState = stack.Peek ();
				}

				void BranchSnowflake (float size, int depth, int maxDepth)
				{
			
						if (depth < maxDepth) {
			
								DrawLine ((int)lineState.p1.x + sizeX / 2, 
				         (int)lineState.p1.y + sizeY / 2, 
				         (int)lineState.p1.x + sizeX / 2 + (int)lineState.p2.x + sizeX / 2, 
				         (int)lineState.p1.y + sizeY / 2 + (int)lineState.p2.y + sizeX / 2, 1, false, 3);
								//lineState.p1.Set(lineState.p2.x, lineState.p2.y);
								BranchSnowflake ((int)(fractions [depth] * size), depth + 1, maxDepth);
								push ();
								lineState.Translate (new Vector2 (fractions [depth] * size, 0));
				
								BranchSnowflake ((int)(fractions [depth] * size), depth + 1, maxDepth);
								push ();

								lineState.Rotate (-angles [depth]);				
								BranchSnowflake ((int)(fractions [depth] * size), depth + 1, maxDepth);
								pop ();
								push ();
								lineState.Rotate (angles [depth]);				
				
								BranchSnowflake ((int)(fractions [depth] * size), depth + 1, maxDepth);
								pop ();
								pop ();				
				
						} 
				}
				
				public static float getAltLength (Vector2 v, float l)
				{
						return Mathf.Pow (Mathf.Pow (Mathf.Abs (v.x), l) + Mathf.Pow (Mathf.Abs (v.y), l), 1.0f / l);
				}

				public static float getDistanceToCorner (Vector2 v2)
				{
						int[] x = new int[4]  {0,1,0,1};
						int[] y = new int[4]  {0,0,1,1};
						Vector2 v1 = new Vector2 ();
						float dist = 1E10f;
						for (int l=0; l<4; l++) {
								v1.Set (x [l], y [l]);
								Vector2 v = (v2 - v1);
								float d = v.magnitude;
								//float d = getAltLength(v,0.5f);
								dist = Mathf.Min (dist, d);
						}
				
						return Mathf.Clamp (dist, 0, 1);
			
				}

				public static Vector2 getShortestRepeatPosition (Vector2 a, Vector2 b)
				{
						Vector2 winner = new Vector2 ();
						winner.Set (b.x, b.y);
						Vector2 v2 = new Vector2 ();
						float dist = 1E10f;
						int s = 2;
						Vector2 diff = new Vector2 ();
						for (int i=-1*s; i<2*s; i++)
								for (int j=-1*s; j<2*s; j++)
										if (i != 0 && j != 0) {
												diff.Set (i, j);
												v2 = b + diff;
												float d = (a - v2).magnitude;
												//float d = getAltLength(v,0.5f);
												if (d < dist) {
														dist = d;
														winner.Set (v2.x, v2.y);
												}
										}
			
						return winner;
				}

				private float F (float x, float y)
				{
						S_p.Set (x, 0, y);
						return Util.swissTurbulence (S_p, S_freqx, S_freqy, S_octaves, S_lacunarity, S_warp, S_offset, S_gain, (S_power), 0f);
				}

				static Vector3 S_p = new Vector3 ();
				static float S_freqx;
				static float S_freqy;
				static int S_octaves;
				static float S_lacunarity;
				static float S_warp;
				static float S_offset;
				static float S_gain;
				static float S_power;
		
				private float F_tileSwiss (float x, float y, float w, float h, float frequencyx, float frequencyy, int octaves, float lacunary, float warp, float offset, float gain, float power)
				{
				
						S_freqx = frequencyx;
						S_freqy = frequencyy;
						S_octaves = octaves;
						S_lacunarity = lacunary;
						S_warp = warp;
						S_offset = offset;
						S_gain = gain;
						S_power = power;
						return (F (x, y) * (w - x) * (h - y) +
								F (x - w, y) * (x) * (h - y) +
								F (x - w, y - h) * (x) * (y) +
								F (x, y - h) * (w - x) * (y)
			        ) / (w * h);
				}
		
				public void MakeSeamless (float scale)
				{
/*			C2DMap tmp, tmp2;
			return;	
			tmp = new C2DMap();
			tmp2  = new C2DMap();
			int Width = sizeX;
			int Height = sizeY;
			for (int i=0;i<Width;i++)
			for (int j=0;j<Height;j++) {
				tmp[i,j] = getPixel(i+Width/2, j+Height/2);
				tmp2[i,j] = getPixel(i,j);
			}
			int[] x = new int[4]  {0,1,0,1};
			int[] y = new int[4]  {0,0,1,1};
			int[] x = new int[4]  {-1,1,-1 ,1};
			int[] y = new int[4]  {1,-1,-1, 1};
			
			int[] xa = new int[4]  {+1,0,+1 ,0};
			int[] ya = new int[4]  {0,+1,+1, 0};
			
			Vector2 v1 = new Vector2();
			Vector2 v2 = new Vector2();
			Vector2[] newPos = new Vector2[4];
			Vector2[] newPosOrg = new Vector2[4];
			float[] weights = new float[4];
			Vector2 shading = new Vector2();		
			float x1=-1; float y1=-1; float x2=1.0f; float y2=1;;
				
			for (int i=0;i<Width;i++)
			for (int j=0;j<Height;j++) {
*/		
		
				
								
/*				float dist = 0;
				float val = 0;
				v2.Set ((float)i/(float)Width,(float)j/(float)Height);
				
				for (int l=0;l<3;l++) {
					newPos[l] = new Vector2(v2.x + x[l]/2 , v2.y + y[l]/2);
					//newPos[l] = getShortestRepeatPosition(v2, newPos[l]);
					}
				newPos[3] = new Vector2(v2.x, v2.y);
				
				float sx = Mathf.Abs((newPos[3].x-newPos[1].x)* (newPos[3]-newPos[2]).y);
				for (int l=0;l<3;l++) 
					weights[l] = (v2 - newPos[l]).magnitude*getDistanceToCorner(v2);
					
				
				weights[3] = Mathf.Clamp(1- weights[0] - weights[1] - weights[2],0,1);
				weights[2] = 0;
				weights[1] = 0;
				if (i==Width/4 && j == Height/4) {
					Debug.Log ("weightsBefore:" + sx + "   : " +  weights[0] + " " + weights[1] + " " + weights[2] + " " + weights[3]);
				}
				
				float ll = 0;
				for (int k=0;k<4;k++)
					ll+=weights[k]*weights[k];	
				if (ll!=0)
				for (int k=0;k<4;k++)
					weights[k]/=Mathf.Sqrt(ll);				
				
				if (i==Width/4 && j == Height/4) {
					Debug.Log ("weights:" + weights[0] + " " + weights[1] + " " + weights[2] + " " + weights[3]);
				}
				
				for (int k=0;k<4;k++)
					dist+=(1-weights[k])*tmp2.getPixel((int)(newPos[k].x*Width),(int)(newPos[k].y*Height));
				
				//dist = Mathf.Clamp (dist,0,1);
				map[i,j] =weights[0];//weights[3];// + Mathf.Clamp(1-dist,0,1)*tmp2.getPixel (i,j);
				//map[i,j] = dist*tmp2.getPixel(i,j) + tmp.getPixel(i,j)*(1-dist);
				*/
						//}
						//CopyFrom(tmp);
			
				}
		
				public void Inv(float s) {
					for (int i=0; i<sizeX; i++) 
						for (int j=0; j<sizeY; j++) 
							map[i,j] = s/Mathf.Abs (map[i,j] + 0.1f);
					
				}
		
				public void calculatePerlin (float amplitude, float scale, float octaves, float kscale, float seed, float perlinSkew, float perlinSkewScale, Vector2 aspect, bool inv)
				{
						seed = 3.119f * seed;

						float rx = seed * 7.72423f;
						float ry = seed * 5.12352f;
						for (int i=0; i<sizeX; i++) {
								for (int j=0; j<sizeY; j++) {
										float x = i / (float)(sizeX - 0)*aspect.x + rx + shift.y;
										float y = j / (float)(sizeY - 0)*aspect.y + ry + shift.x;
										float p = Util.perlinNoise2dSeamless (1, 1, 1.0f, x * perlinSkewScale, y * perlinSkewScale, 0, 0, 0, 0,false);
										float t = perlinSkew;
										float xx = x + t * p;
										float yy = y + t * p;
					
										map [i, j] = amplitude * (0.5f + Util.perlinNoise2dSeamless (octaves, scale, kscale, xx, yy, 0, 0, 0, 0, inv));// / Mathf.Pow(k, kscale);///(float)kk;
								}
						}
				}

				public void calculatePerlinClouds (float amplitude, float scale, float octaves, float kscale, float seed, float perlinSkew, float perlinSkewScale, float pow)
				{
						seed = 3.119f * seed;
			
						float rx = seed * 7.72423f;
						float ry = seed * 5.12352f;
						for (int i=0; i<sizeX; i++) {
								for (int j=0; j<sizeY; j++) {
										float x = i / (float)(sizeX - 0) + rx + shift.y;
										float y = j / (float)(sizeY - 0) + ry + shift.x;
										float p = Util.perlinNoise2dSeamless (1, 1, 1.0f, x * perlinSkewScale, y * perlinSkewScale, 0, 0, 0, 0, false);
										float t = perlinSkew;
										float xx = x + t * p;
										float yy = y + t * p;
					
										map [i, j] = amplitude * (0.0f + Util.perlinNoise2dSeamless_alt (octaves, scale, kscale, xx, yy, 0, 0, 0, 0, pow));// / Mathf.Pow(k, kscale);///(float)kk;
								}
						}
				}
		
				public void calculateMultiridged (float seed, float heightScale, float frequency, float lacunarity, float gain, float offset, float ioffset)
				{
						float octaves = 12;
        
						float rx = seed * 7.72423f;
						float ry = seed * 5.12352f;
        
						float nx = sizeX;
						float ny = sizeY;

						float sx = shift.x * 0;
						float sy = shift.y * 0;

						for (int i=0; i<nx; i++) {
								for (int j=0; j<ny; j++) {
										//Vector3 p = new Vector3((i / (nx - 1) + sy) * frequency + rx, 0, (j / (ny - 1) + sx) * frequency + ry);
										Vector3 p = new Vector3 ((i / (nx - 0) + sy) + rx, 0, (j / (ny - 0) + sx) + ry);
										float v = 0;//Util.getRidgedMf (p, frequency, (int)octaves, lacunarity, 0, offset, gain, ioffset);
										map [i, j] = v * heightScale;
					
								}
						}   
			
        
        
				}

				public void ScaleMap (float scale, float offset)
				{
						for (int i=0; i<sizeX; i++)
								for (int j=0; j<sizeY; j++)
										map [i, j] = scale * map [i, j] + offset;
				}

				public void Contour (float contour, float offset)
				{
						C2DRGBMap rgb = new C2DRGBMap ();
						createNormalMap (rgb, contour);
						for (int i=0; i<sizeX; i++)
								for (int j=0; j<sizeY; j++) {
										map [i, j] = rgb.colors [i, j].b + offset;
								}
				}
		
				public void Pow (float pow, float offset)
				{
						for (int i=0; i<sizeX; i++)
								for (int j=0; j<sizeY; j++)
										map [i, j] = Mathf.Pow (map [i, j], pow) + offset;
				}
		
				public void calculateSwiss (float seed, float heightScale, float frequencyx, float frequencyy, float lacunarity, float gain, float offset, float warp, float power)
				{
        
						float octaves = 6;
        
						float rx = 0;//seed * 0.072423f;
						float ry = 0;//seed * 0.012352f;
        
						float nx = sizeX;
						float ny = sizeY;
        
						float sx = shift.x;
						float sy = shift.y;

						/*lacunarity = 2.0f;
        gain = 1.25f + 0.5f;
        offset = 0.5f;
*/
						for (int i=0; i<nx; i++) {
								for (int j=0; j<ny; j++) {
										Vector3 p = new Vector3 ((i / (nx - 0) + sy) + rx, 0, (j / (ny - 0) + sx) + ry);
										//(tmp, 240*s2, m_seed, (int)Math.min(14, level), 2.0f, twist, 1.25f + h2 , 0.45f,fff, (double)f)-0.0f))
										//float v = Util.swissTurbulence(p, frequency, (int)octaves, lacunarity, warp - 1.0f, offset, gain, (power), 0f);
										float v = F_tileSwiss (p.x, p.z, 1.0f, 1.0f, frequencyx, frequencyy, (int)octaves, lacunarity, warp - 1.0f, offset, gain, (power));
					
										map [i, j] = v * heightScale;
								}
						}   
				}
        
				public void Add(C2DMap m, float scale) {
					for (int i=0; i<sizeX; i++) 
						for (int j=0; j<sizeY; j++) 
							map[i,j]+=m.map[i,j]*scale;
				
				}
				// Pixelate 
				public void Pixelate (float val)
				{	
						val = Mathf.Clamp (val, 0.001f, 1);
						int pixelSize = (int)(val * sizeX * 0.2f);		
						C2DMap copy = new C2DMap ();
						copy.CopyFrom (this);
						for (int i=0; i<sizeX; i++) 
								for (int j=0; j<sizeY; j++) {
										int x = (Mathf.FloorToInt (i / pixelSize)) * pixelSize;
										int y = (Mathf.FloorToInt (j / pixelSize)) * pixelSize;
										map [i, j] = copy.map [x, y];
								}
				}
			
			
				public void Rotate() {
					C2DMap copy = new C2DMap ();
					copy.CopyFrom (this);
					for (int i=0; i<sizeX; i++) 
						for (int j=0; j<sizeY; j++) 
							map[i,j] = copy.map[j,i];
				
		}
        
				// Simple smoothing
				public void Smooth (int N, int type)
				{
						C2DMap copy = new C2DMap ();
						int s = 6 * N;
						int D = 2;
						int S = 0;
						float sigma = N;
						int ks = 2 * s + 1;
						// Set up gaussian kernel
						float[] kernel = new float[ks];
						for (int i=0; i<ks; i++) {
								float len = Mathf.Clamp (Mathf.Sqrt (Mathf.Pow (i - ks / 2, 2)), 1, 10000);
								kernel [i] = 1.0f / (Mathf.Sqrt (2 * Mathf.PI) * sigma) * Mathf.Exp (-0.5f * len * len / (sigma * sigma));
						}
						if (type == 2) 
								D = 1; 
						if (type == 3) {
								D = 2;
								S = 1;
						}
						for (int i=S; i<D; i++) {
								for (int x = 0; x < sizeX; x++) {
										for (int y = 0; y < sizeY; y++) {
												float sum = 0;
												float d1 = 1;
												float d2 = 0;
                        
												if (i == 0) {
														d1 = 0;
														d2 = 1;
												}
                        
												for (float k=-s*d1; k<=s*d1; k+=1)
														for (float l=-s*d2; l<=s*d2; l+=1) {
																int ker = (int)Mathf.Max (k + s * d1, l + s * d2);
																sum += getPixel (x + (int)k, y + (int)l) * kernel [ker];
														}
												copy.setPixel (x, y, sum);
												//	copy.setPixel(x,y, sum / 9f);
										}
								}
								CopyFrom (copy);
						}
			
				}
		
				public void putSafePixel (int x, int y, float v)
				{
						if (x >= 0 && x < sizeX && y >= 0 && y < sizeY)
								map [x, y] = v;
				}

				public void putSafePixelMax (int x, int y, float v)
				{
						if (x >= 0 && x < sizeX && y >= 0 && y < sizeY)
								map [x, y] = Mathf.Max (v, map [x, y]);
				}
		
				public bool dotRepeat = false;
		
				public void Dot (int x, int y, float h, int size)
				{
						for (int i=x-size; i<=x+size; i++) 
								for (int j=y-size; j<=y+size; j++) 
										if (!dotRepeat)
												putSafePixel (i, j, h);
										else
												setPixel (i, j, getPixel (i, j) + h);
				}

				public void SmoothDot (int x, int y, float h, float scaledSize, float filling)
				{
						int size = (int)(sizeX * scaledSize);		
						for (int i=x-size; i<=x+size; i++) 
								for (int j=y-size; j<=y+size; j++) {
										float l = Mathf.Sqrt (2 * size * size);
										float dist = Mathf.Clamp (Mathf.Sqrt (Mathf.Pow (i - x, 2) + Mathf.Pow (j - y, 2)) / l, 0, 1);
										//if (dist>=filling)
										//	dist = dist - filling;
										dist = Mathf.Exp (-200f * Mathf.Abs (Mathf.Pow ((dist - filling), 2))) / filling - 0.2f;	
										putSafePixelMax (i, j, Mathf.Clamp (h * dist * 5f, 0, 1000));
								}
			
				}
						
				public void RenderDots (float amplitude, float size, float spacingx, float spacingy, float filling)
				{
						float dx = sizeX * spacingx;
						float dy = sizeY * spacingy;
						float nx = 1.0f / spacingx;
						float ny = 1.0f / spacingy;
			
						// Demand whole
						nx = (int)nx;
						ny = (int)ny;
						dx = sizeX / nx;
						dy = sizeY / ny;
						zero ();
						for (int i=0; i<nx; i++) 
								for (int j=0; j<ny; j++) {
										SmoothDot ((int)(i * dx + dx / 2f), (int)(j * dy + dy / 2f), amplitude, size, filling);
								}
						for (int i=-1; i<nx+1; i++) 
								for (int j=0; j<ny+1; j++) {
										SmoothDot ((int)(i * dx), (int)(j * dy), amplitude, size, filling);
								}
			
				}
		
				public void DrawBox (float x1, float y1, float w, float h, float a, int t)
				{
						DrawLine (x1, y1, x1 + w, y1, a, false, t);
						DrawLine (x1 + w, y1, x1 + w, y1 + h, a, false, t);
						DrawLine (x1 + w, y1 + h, x1, y1 + h, a, false, t);
						DrawLine (x1, y1 + h, x1, y1, a, false, t);
			
				}
		
				//map.calculateBricks (getValue ("amplitude"), getValue ("thickness"), getValue ("spacingx"), getValue ("spacingy"), getValue ("rotation"));
				public void RenderBricks (float amplitude, float thickness, float spacingx, float spacingy, int type)
				{
						float dx = sizeX * spacingx;
						float dy = sizeY * spacingy;
						float nx = 1.0f / spacingx;
						float ny = 1.0f / spacingy;
			
						// Demand whole
						nx = (int)nx;
						ny = (int)ny;
						dx = sizeX / (nx + 1);
						dy = sizeY / (ny + 1);
						zero ();
						float i = 0;
						float j = 0;
						dotRepeat = false;
						if (type == 2)
								for (float u = -2*nx; u< 2*nx; u++) {
										i = u * 1f;
										j = u * 1f - nx;
				
										for (float k=-2*ny; k<2*ny; k++) {
												DrawBox (i * dx, j * dy, dx, 0.5f * dy, amplitude, (int)thickness);
												DrawBox ((i + 0.5f) * dx, (j + 0.5f) * dy, dx * 0.5f, dy, amplitude, (int)thickness);
												//DrawBox( i*dx, j*dy, (i+1)*dx, (j+0.5f)*dy,amplitude, (int)thickness);
				
												i += 0.5f;
												j += 1.5f;
										}
								}
						if (type == 1) {
								dx = sizeX / (nx);
								dy = sizeY / (ny);
				
								for (i=0; i<nx+1; i++) {
										for (j=0; j<ny+1; j++) {
												DrawLine (i * dx, j * dy, (i + 1) * dx, j * dy, amplitude, false, (int)thickness);
												if (j % 2 == 0) 
														DrawLine (i * dx, j * dy, i * dx, (j + 1) * dy, amplitude, false, (int)thickness);
												else
														DrawLine (i * dx + dx / 2f, j * dy, i * dx + dx / 2f, (j + 1) * dy, amplitude, false, (int)thickness);
										}
					
								}
						}
			
		
				}

				public void RenderGrid (float amplitude, float thickness, float spacingx, float spacingy, float rotation)
				{
						List<Vector2> orgPointsA = new List<Vector2> ();
						List<Vector2> orgPointsB = new List<Vector2> ();
						List<Vector2> rotPointsA = new List<Vector2> ();
						List<Vector2> rotPointsB = new List<Vector2> ();
						float dx = sizeX * spacingx;
						float dy = sizeY * spacingy;
						float nx = 1.0f / spacingx;
						float ny = 1.0f / spacingy;
			
						// Demand whole
						nx = (int)nx;
						ny = (int)ny;
						dx = sizeX / nx;
						dy = sizeY / ny;
			
						float start = 50;
						int dn = 4;
						for (int i=-dn; i<nx+dn; i++) {
								orgPointsA.Add (new Vector2 (i * dx, -start));
								orgPointsB.Add (new Vector2 (i * dx, sizeY + start));
						}
						for (int i=-dn; i<ny+dn; i++) {
								orgPointsA.Add (new Vector2 (-start, i * dy));
								orgPointsB.Add (new Vector2 (sizeX + start, i * dy));
						}
						float t = rotation / 360f;
						Vector2 center = new Vector2 (sizeX / 2, sizeY / 2);
						for (int i=0; i<orgPointsA.Count; i++) {
								Vector2 A = Util.Rotate2D (orgPointsA [i] - center, t) + center;
								Vector2 B = Util.Rotate2D (orgPointsB [i] - center, t) + center;
								rotPointsA.Add (A);
								rotPointsB.Add (B);
						}
			
						zero ();
						for (int i=0; i<orgPointsA.Count; i++) {
								DrawLine ((int)rotPointsA [i].x, (int)rotPointsA [i].y, (int)rotPointsB [i].x, (int)rotPointsB [i].y, amplitude, false, (int)thickness);
						}
			
			
			
				}
            
				public void DrawLine (float x00, float y00, float x11, float y11, float color, bool erase, int size)
				{
						int x0 = (int)x00;
						int x1 = (int)x11;
						int y0 = (int)y00;		
						int y1 = (int)y11;
						int dy = (int)(y1 - y0);
						int dx = (int)(x1 - x0);
						float t = 0.5f;                      // offset for rounding
						size = (int)(((float)size / 512f) * sizeX);
						//raster.setPixel(pix, x0, y0);
//		if (!erase)
						Dot ((int)x0, (int)y0, color, size);
//		else
///			Erase(x0,y0);
						if (Mathf.Abs (dx) > Mathf.Abs (dy)) {          // slope < 1
								float m = (float)dy / (float)dx;      // compute slope
								t += y0;
								dx = (dx < 0) ? -1 : 1;
								m *= dx;
								while (x0 != x1) {
										x0 += dx;                           // step to next x value
										t += m;                             // add slope to y value
//				if (!erase)
										Dot ((int)x0, (int)t, color, size);
//				else
//					Erase(x0,t);
								}
						} else {                                    // slope >= 1
								float m = (float)dx / (float)dy;      // compute slope
								t += x0;
								dy = (dy < 0) ? -1 : 1;
								m *= dy;
								while (y0 != y1) {
										y0 += dy;                           // step to next y value
										t += m;                             // add slope to x value
//				if (!erase)
										Dot ((int)t, (int)y0, color, size);
//				else Erase(t,y0);
								}
						}
				}
	
				public void FlowerPattern (int depth, float size, float angle, float seed, float thickness, float angleScale)
				{
						dotRepeat = true;
			
						zero ();
						Util.rnd = new System.Random ((int)(seed * 119321.231f));
						float r1 = 0.02f * ((float)Util.rnd.NextDouble () - 0.5f);
						for (int k=0; k<2; k++) {
								Walker w = new Walker (this, sizeX / 2, sizeY / 4.1f, 
				                k * Mathf.PI / 1f, angle * 0.1f, r1,  
				                      8 * thickness, -0.15f * thickness, 
				                      depth, size / 512f * sizeX * 10f, 4, angleScale);
								Walker.allDone = false;
								while (Walker.allDone != true) {
										Walker.allDone = true;
										w.Advance ();
								}
			
						}
						dotRepeat = false;
			
				}
				
				public void GrassPattern (float N, float size, float angle, float seed, float thickness, float angleScale)
				{
						dotRepeat = true;
			
						zero ();
						Util.rnd = new System.Random ((int)(seed * 119321.231f));
						for (int i=0; i<N*5; i++) {
								Walker w = new Walker (this, Util.rnd.Next () % sizeX, Util.rnd.Next () % sizeY, 
				                      (float)((Util.rnd.NextDouble () - 0.5) * angle), (float)((Util.rnd.NextDouble () - 0.5) * angleScale), 0,  
				                      8 * thickness, -0.15f * thickness, 
				                      0, size / 512f * sizeX * 10f, 4, angleScale);
								Walker.allDone = false;
								while (Walker.allDone != true) {
										Walker.allDone = true;
										w.Advance ();
								}
				
						}
						dotRepeat = false;
			
				}	
		
		}

	
		
				
}
