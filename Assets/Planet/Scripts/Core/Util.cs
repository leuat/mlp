using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.IO;


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
using System.Text;
using System;
using System.Diagnostics;

namespace LemonSpawn
{

    public class MaterialList
    {
        public string mat;
        public List<GameObject> gameObjects = new List<GameObject>();
    }

    [System.Serializable]
    public class DVector
    {
        public double x, y, z;


        public void Set(Vector3 o)
        {
            x = o.x;
            y = o.y;
            z = o.z;
        }
        public DVector(DVector o)
        {
            x = o.x;
            y = o.y;
            z = o.z;
        }
        public DVector(Vector3 o)
        {
            x = o.x;
            y = o.y;
            z = o.z;
        }

        public DVector Add(DVector o)
        {
            return new DVector(x + o.x, y + o.y, z + o.z);
        }
        public static DVector operator +(DVector c1, DVector c2)
        {
            return c1.Add(c2);
        }
        public static DVector operator -(DVector c1, DVector c2)
        {
            return c1.Sub(c2);
        }
        public static DVector operator *(DVector c1, double s)
        {
            return new DVector(c1.x * s, c1.y * s, c1.z * s);
        }
        public static DVector operator /(DVector c1, double s)
        {
            return new DVector(c1.x / s, c1.y / s, c1.z / s);
        }


        public DVector Add(Vector3 o)
        {
            return new DVector(x + o.x, y + o.y, z + o.z);
        }
        public DVector Sub(DVector o)
        {
            return new DVector(x - o.x, y - o.y, z - o.z);
        }
        public double Length()
        {
            return Mathf.Sqrt((float)(x * x + y * y + z * z));
        }

        public DVector()
        {

        }
        public void Scale(double d)
        {
            x *= d;
            y *= d;
            z *= d;
        }
        public DVector(double _x, double _y, double _z)
        {
            x = _x;
            y = _y;
            z = _z;
        }
        public Vector3 toVectorf()
        {
            return new Vector3((float)x, (float)y, (float)z);
        }


    }


    /*
    * Saving detail prototypes as serializable objects. Bah. Needs to be done because unity is evil.
    */
    [System.Serializable]
    public class DetailPrototypeSerializable
    {
        public float maxHeight = 2;
        public float minHeight = 1;
        public float minWidth = 1;
        public float maxWidth = 2;
        public float noiseSpread = 1.0f;
        public Color dryColor = new Color();
        public Color healthyColor = new Color();
        public Texture2D prototypeTexture = null;

        public DetailPrototype toDetailPrototype()
        {
            DetailPrototype dt = new DetailPrototype();
            dt.dryColor = dryColor;
            dt.healthyColor = healthyColor;
            dt.maxWidth = maxWidth;
            dt.minWidth = minWidth;
            dt.maxHeight = maxHeight;
            dt.minHeight = minHeight;
            dt.noiseSpread = noiseSpread;
            dt.prototypeTexture = prototypeTexture;
            dt.usePrototypeMesh = false;
            dt.renderMode = DetailRenderMode.GrassBillboard;
            return dt;
        }


    }
    /*
    * General utilities
    *
    */

    public class Message
    {
        public string message;
        public float time = 100;
        public Message(string s)
        {
            message = s;
        }
        public Message(string s, float t)
        {
            message = s;
            time = t*3;
        }
    }

    public class Util : MonoBehaviour
    {

        //				private static float[] weights = null;
        /*				static int oct = -1;
                        static float lac = -1;
                        static float freq = -1;*/
        //				static System.Random rand = new System.Random (0);
        public static Noise4D noise4D = new Noise4D();
        public static System.Random rnd = new System.Random();


        public static Dictionary<string, float[]> weights = new Dictionary<string, float[]>();


        private static float[] generateSpectralWeights(float lacunarity,
                                             int octaves, float h, float frequency, string name)
        {
            if (weights.ContainsKey(name))
                return weights[name];

            //if (oct != octaves) 
            //						{
            float[] w = new float[octaves];
            //						}

            //						if (lac != lacunarity || freq != frequency || oct != octaves)

            for (int octave = 0; octave < octaves; ++octave)
            {
                w[octave] = Mathf.Pow(frequency, h);
                frequency *= lacunarity;
            }

            weights[name] = w;
            return w;

        }
#if UNITY_STANDALONE_OSX
				public static string[] GetOSXCommandParams() {
					Process proc = Process.GetCurrentProcess(); 
					ProcessStartInfo si = new ProcessStartInfo("ps", "-p" + proc.Id + " -xwwocommand="); 
					si.RedirectStandardOutput = true; si.RedirectStandardError = true; si.UseShellExecute = false; si.CreateNoWindow = true;
					Process psProc = new Process();
					psProc.StartInfo = si;
					psProc.Start();
					string result = psProc.StandardOutput.ReadToEnd();
					return result.Split();
				}
#endif


        public static float PerlinModes(Vector3 p, float s, int modes)
        {
            return noise4D.octave_noise_4d(modes, 1, s, p.x, p.y, p.z, 0.23125f, false);
        }


        public static Vector3 randomVector(float x, float y, float z)
        {
            return new Vector3((float)rnd.NextDouble() * x, (float)rnd.NextDouble() * y, (float)rnd.NextDouble() * z);

        }

        public static float getRidgedMf2(Vector3 p, float frequency, int octaves, float lacunarity, float warp, float offset, float gain, float initialOffset, string name, float seed)
        {

            float signal;
            float value;
            float weight;
            int curOctave;

            /*				x *= _frequency;
                            y *= _frequency;
                            z *= _frequency;
            */
            p *= frequency;
            float[] ww = generateSpectralWeights(lacunarity, octaves, 0, frequency, name);

            // Initialize value : 1st octave
            signal = noise4D.raw_noise_4d(p.x, p.y, p.z, seed);

            // get absolute value of signal (this creates the ridges)
            if (signal < 0.0)
            {
                signal = -signal;
            }//end if

            // invert and translate (note that "offset" should be ~= 1.0)
            signal = offset - signal;

            // Square the signal to increase the sharpness of the ridges.
            signal *= signal;

            // Add the signal to the output value.
            value = signal;

            weight = 1.0f;

            for (curOctave = 1; weight > 0.001 && curOctave < octaves; curOctave++)
            {

                p *= lacunarity;
                /*					x *= _lacunarity;
                                    y *= _lacunarity;
                                    z *= _lacunarity;
                */
                // Weight successive contributions by the previous signal.
                weight = Mathf.Clamp01(signal * gain);

                // Get the coherent-noise value.
                signal = noise4D.raw_noise_4d(p.x, p.y, p.z, seed);

                // Make the ridges.
                if (signal < 0.0f)
                {
                    signal = -signal;
                }//end if

                signal = offset - signal;

                // Square the signal to increase the sharpness of the ridges.
                signal *= signal;

                // The weighting from the previous octave is applied to the signal.
                // Larger values have higher weights, producing sharp points along the
                // ridges.
                signal *= weight;

                // Add the signal to the output value.
                value += (signal * ww[curOctave]);

            }//end for

            return value;

        }//end GetValue



        public static float getRidgedMf(Vector3 p, float frequency, int octaves, float lacunarity, float warp, float offset, float gain, float initialOffset, string name, float seed)
        {
            float value = 0.0f;
            float weight = 1.0f;

            float w = -0.05f;

            float[] ww = generateSpectralWeights(lacunarity, octaves, w, frequency, name);


            Vector3 vt = p * frequency;
            for (float octave = 0; octave < octaves; octave++)
            {
 //               float signal = initialOffset + noise4D.raw_noise_3d(vt.x, vt.y, vt.z);//perlinNoise2dSeamlessRaw(frequency, vt.x, vt.z,0,0,0,0);//   Mathf.PerlinNoise(vt.x, vt.z);
                 float signal = initialOffset + noise4D.raw_noise_4d(vt.x, vt.y, vt.z, seed);//perlinNoise2dSeamlessRaw(frequency, vt.x, vt.z,0,0,0,0);//   Mathf.PerlinNoise(vt.x, vt.z);

                // Make the ridges.
                signal = Mathf.Abs(signal);
                signal = offset - signal;


                signal *= signal;

                signal *= weight;
                weight = signal * gain;
                weight = Mathf.Clamp(weight, 0, 1);

                value += (signal * ww[(int)octave]);
                vt = vt * lacunarity;
                frequency *= lacunarity;
            }
            return value;
        }

        public static float getNoise(float x, float y, float z)
        {
            //tmp2.Set (x, y, z);
            //return simplex.noise3D (tmp2);
            //return perlinNoise2dSeamlessRaw(frequency, x, y,0,0,0,0);//   Mathf.PerlinNoise(vt.x, vt.z);
            return noise4D.raw_noise_4d(x, y, z, 0);

        }

        public static float perlinNoiseDeviv(Vector3 p, int i, float sc, float sc2, out Vector3 N)
        {
            if (sc == 0)
                sc = 1.0f;
            float e = 0.09193f;//*sc;
            N = Vector3.zero;
            float F0 = getNoise(p.x, p.y, p.z);
            float Fx = getNoise(p.x + e, p.y, p.z);
            float Fy = getNoise(p.x, p.y + e, p.z);
            float Fz = getNoise(p.x, p.y, p.z + e);

            N.Set((Fx - F0) / e, (Fy - F0) / e, (Fz - F0) / e);

            float s = 0.8f;
            N.Normalize();
            N = N * s;
            return F0;
        }

        //				static Vector3 N = new Vector3 ();

        public static float swissTurbulence(Vector3 p, float scalex, float scaley, int octaves, float lacunarity, float warp, float offset, float gain, float powscale, float background)
        {

            float sum = 0;
            float freq = 1.0f;
            float amp = 1.0f;
            Vector3 dsum = new Vector3();
            Vector3 N = Vector3.zero;
            dsum.Set(0, 0, 0);
            Vector3 t = new Vector3();
            for (int i = 0; i < octaves; i++)
            {
                t.Set(p.x * scalex, p.y * scalex, p.z * scaley);
                t = t + dsum * warp;
                t = t * freq;

                float F = perlinNoiseDeviv(t, i, scalex * freq, scaley * freq, out N);

                float n = (offset - powscale * Mathf.Abs(F));

                n = n * n;
                sum += amp * n;
                t.Set(N.x, N.y, N.z);
                t = t * amp * -F;
                dsum = dsum + t;
                freq *= lacunarity;

                amp *= gain * Mathf.Clamp(sum, 0, 1);
            }

            return sum;
        }

        public static string RandomString(int size)
        {
            StringBuilder builder = new StringBuilder();
            System.Random rnd = new System.Random();
            char ch;
            for (int i = 0; i < size; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * rnd.NextDouble() + 65)));
                builder.Append(ch);
            }

            return builder.ToString();
        }


        public static bool LastFloatValueChanged = false;

        public static float getFloatFromString(string s, float org)
        {

            s = Regex.Replace(s, @"[^0-9.]", "");
            float val;
            if (float.TryParse(s, out val))
            {
                if (val == org)
                    LastFloatValueChanged = false;
                else
                    LastFloatValueChanged = true;
                return val;
            }
            else
                return 0;
        }

        public static int getIntFromString(string s, int org)
        {

            s = Regex.Replace(s, @"[^0-9]", "");
            int val;
            if (int.TryParse(s, out val))
            {
                if (val == org)
                    LastFloatValueChanged = false;
                else
                    LastFloatValueChanged = true;
                return val;
            }
            else
                return 0;
        }

        public static string ColorToHex(Color32 color)
        {
            string hex = "#" + color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2");
            return hex;
        }

        static public float perlinNoise4D(float octaves, float scale, float persistence, float s, float t, float cx, float cy, float cz, float cw, bool inv)
        {
            return noise4D.octave_noise_4d(octaves, persistence, scale, cx, cy, cz, cw, inv);
        }


        static public float perlinNoise2dSeamless(float octaves, float scale, float persistence, float s, float t, float cx, float cy, float cz, float cw, bool inv)
        {
            // Generate the 4d coordinates that wrap around seamlessly
            float r = scale / (2 * Mathf.PI);
            float axy = 2 * Mathf.PI * s;// / scale;        
            float x = r * Mathf.Cos(axy);
            float y = r * Mathf.Sin(axy);

            float azw = 2 * Mathf.PI * t;// / scale;        
            float z = r * Mathf.Cos(azw);
            float w = r * Mathf.Sin(azw);

            return noise4D.octave_noise_4d(octaves, persistence, scale, cx + x, cy + y, cz + z, cw + w, inv);
        }

        static public float perlinNoise2dSeamless_alt(float octaves, float scale, float persistence, float s, float t, float cx, float cy, float cz, float cw, float pow)
        {
            // Generate the 4d coordinates that wrap around seamlessly
            float r = scale / (2 * Mathf.PI);
            float axy = 2 * Mathf.PI * s;// / scale;        
            float x = r * Mathf.Cos(axy);
            float y = r * Mathf.Sin(axy);

            float azw = 2 * Mathf.PI * t;// / scale;        
            float z = r * Mathf.Cos(azw);
            float w = r * Mathf.Sin(azw);

            return noise4D.octave_noise_4d_alt(octaves, persistence, scale, cx + x, cy + y, cz + z, cw + w, pow);
        }

        static public float perlinNoise2dSeamlessRaw(float scale, float s, float t, float cx, float cy, float cz, float cw)
        {
            // Generate the 4d coordinates that wrap around seamlessly
            float r = scale / (2 * Mathf.PI);
            float axy = 2 * Mathf.PI * s / scale;
            float x = r * Mathf.Cos(axy);
            float y = r * Mathf.Sin(axy);

            float azw = 2 * Mathf.PI * t / scale;
            float z = r * Mathf.Cos(azw);
            float w = r * Mathf.Sin(azw);

            return noise4D.raw_noise_4d(cx + x, cy + y, cz + z, cw + w);
        }


        public static Vector2 Rotate2D(Vector2 v, float t)
        {
            return new Vector2(v.x * Mathf.Cos(t) - v.y * Mathf.Sin(t), v.x * Mathf.Sin(t) + v.y * Mathf.Cos(t));
        }

        public static void Rotate2DDirect(Vector2 v, float t)
        {
            v.x = v.x * Mathf.Cos(t) - v.y * Mathf.Sin(t);
            v.y = v.x * Mathf.Sin(t) + v.y * Mathf.Cos(t);
        }

        public static void SaveTextureFile(Texture2D incomingTexture, string incomingDataPath, string incomingFilename)
        {

            FileStream fs = new FileStream(incomingDataPath + incomingFilename + ".png", FileMode.Create);
            BinaryWriter bw = new BinaryWriter(fs);
            bw.Write(incomingTexture.EncodeToPNG());

            bw.Close();
            fs.Close();
        }
        public static void DestroyGameObjects(string name)
        {

            GameObject go = GameObject.Find(name);
            while (go != null)
            {
                GameObject.Destroy(go);
                go = GameObject.Find(name);
            }

        }
        static string trimMaterial(string mat)
        {
            mat = mat.Replace("(Instance)", "");
            mat = mat.Trim();
            return mat;
        }

        static void InsertGameObject(GameObject go, List<MaterialList> list)
        {
            foreach (MaterialList ml in list)
            {
                if (ml.mat == trimMaterial(go.GetComponent<Renderer>().sharedMaterial.name))
                {
                    ml.gameObjects.Add(go);
                    return;
                }
            }

            MaterialList mli = new MaterialList();
            mli.mat = trimMaterial(go.GetComponent<Renderer>().sharedMaterial.name);
            mli.gameObjects.Add(go);
            list.Add(mli);
        }

        static public void MakeRigidBody(List<GameObject> list)
        {
            foreach (GameObject go in list)
            {
                go.AddComponent<Rigidbody>();
                go.AddComponent<BoxCollider>();
            }

        }


        static public void CombineMeshesMaterials(MaterialList ml, GameObject parent, Vector3 add)
        {
            CombineInstance[] combine = new CombineInstance[ml.gameObjects.Count];
            int i = 0;
            foreach (GameObject go in ml.gameObjects)
            {
                combine[i].mesh = go.GetComponent<MeshFilter>().sharedMesh;
                combine[i].transform = go.transform.localToWorldMatrix;
                i++;
                //					go.SetActive(false);
            }

            GameObject main = new GameObject(parent.name + "_" + ml.mat);
            main.AddComponent<MeshFilter>();
            main.AddComponent<MeshRenderer>();
            main.GetComponent<MeshFilter>().mesh = new Mesh();
            Mesh mesh = main.GetComponent<MeshFilter>().sharedMesh;
            mesh.CombineMeshes(combine);
            mesh.RecalculateNormals();
            mesh.Optimize();

            main.GetComponent<Renderer>().material = (Material)Resources.Load(ml.mat);
            main.transform.position += add;

            main.transform.parent = parent.transform;

        }

        public static void CombineMeshes(string name, GameObject parent, Vector3 add)
        {

            List<MaterialList> mList = new List<MaterialList>();
            GameObject[] gos = (GameObject[])FindObjectsOfType(typeof(GameObject));
            List<GameObject> children = new List<GameObject>();

            foreach (GameObject go in gos)
                if (go.transform.parent == parent.transform && go.name == name)
                {
                    children.Add(go);
                }

            foreach (GameObject go in children)
                InsertGameObject(go, mList);

            foreach (MaterialList ml in mList)
                CombineMeshesMaterials(ml, parent, add);


            foreach (GameObject go in children)
                GameObject.DestroyImmediate(go);


        }


        public static void tagAll(GameObject g, string tag, int layer)
        {
            //if (g.tag == tag)
            //    return;

            g.tag = tag;
            g.layer = layer;
            for (int i = 0; i < g.transform.childCount; i++)
            {
                GameObject go = g.transform.GetChild(i).gameObject;
                //UnityEngine.Debug.Log(go.name);
                tagAll(go, tag, layer);
            }
        }


    }

}
