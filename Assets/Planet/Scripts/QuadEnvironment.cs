using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace LemonSpawn

{

    [System.Serializable]
    public class QuadEnvironmentType {
        public string[] Textures = new string[3] {"Tree1","Tree2", "Tree3"};
        public Vector3 noiseValues = new Vector3(1,2,3);
        public Vector3 noiseThresholds = new Vector3(0,0,0.2f);
        public Color[] baseColors = new Color[] { new Color(1,1,1),new Color(1,1,1),new Color(1,1,1)};
        public Color[] spreadColors = new Color[] { new Color(0,0,0),new Color(0,0,0),new Color(0,0,0)};

    }




    public class QuadEnvironment : ThreadQueue
    {
        private QuadNode quad;
        protected int maxCount = 250;
        public GameObject go;
        public Mesh mesh;
        private Material mat;
        private Vector3 tangent, binormal;
        TQueue thread = null;
        private int density;
        List<Vector3> points = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<int> indexes = new List<int>();
        List<Color> colors = new List<Color>();
        QuadEnvironmentType qet;
        

        private bool VerifyPosition(Vector3 pos, out Vector3 newPos)
        {

            newPos = quad.planetSettings.properties.gpuSurface.getPlanetSurfaceOnly(pos);

            float h = (newPos.magnitude / quad.planetSettings.radius - 1);
            if (h < quad.planetSettings.liquidThreshold)
                return false;
            if (h > quad.planetSettings.topThreshold)
                return false;

/*            Vector3 norm = quad.planetSettings.properties.gpuSurface.getPlanetSurfaceNormal(pos, tangent, binormal, 0.2f, 3, 4) * -1;
            if (Vector3.Dot(norm.normalized, pos.normalized) < 0.99)
                return false;
  */          
            
            return true;

        }




        private void CreateMesh(int N) {
            System.Random r = new System.Random();
           
            Vector3 P = quad.qb.P[0].P;
            Vector3 D1 = (quad.qb.P[3].P - quad.qb.P[0].P);
            Vector3 D2 = (quad.qb.P[1].P - quad.qb.P[0].P);

            tangent = D1.normalized;
            binormal = D2.normalized;


            int cur = 0;
            points.Clear();
            colors.Clear();
            normals.Clear();
            indexes.Clear();
            Vector3 vals = new Vector3();
            Color c= new Color(0,0,0,1);

            for(int i=0;i<N;++i) {
                float d1 = (float)r.NextDouble();
                float d2 = (float)r.NextDouble();
                bool isOk = true;

                Vector3 proposedPos = (P + D1 * d1 + D2 * d2).normalized * quad.planetSettings.getPlanetSize();




                Vector3 realPos = proposedPos;
                //isOk = VerifyPosition(proposedPos, out realPos);


                              

                if (isOk)
                {
                    // 

                    vals.x = GPUSurface.noiseStatic(realPos.normalized*qet.noiseValues.x) - qet.noiseThresholds.x;
                    vals.y = GPUSurface.noiseStatic(realPos.normalized*qet.noiseValues.y) - qet.noiseThresholds.y;
                    vals.z = GPUSurface.noiseStatic(realPos.normalized*qet.noiseValues.z) - qet.noiseThresholds.z;

                    int val = 0;
                    if (vals.y>vals.x) val = 1;
                    if (vals.z>vals.y) val = 2;

                    // Check that texture is not null

                    if (qet.Textures[val] == "")
                        continue;

                    points.Add(realPos);
                    indexes.Add(cur);


                    c.r = qet.baseColors[val].r + (float)r.NextDouble()*qet.spreadColors[val].r;
                    c.g = qet.baseColors[val].g + (float)r.NextDouble()*qet.spreadColors[val].g;
                    c.b = qet.baseColors[val].b + (float)r.NextDouble()*qet.spreadColors[val].b;

                    normals.Add(new Vector3(val,(float)r.NextDouble()*360f,0));

                    colors.Add(c);
                    cur++;
                }
         }
         
        }

        public void Destroy() {
            if (go==null)
              return;
            GameObject.DestroyImmediate(go);
            quad.planetSettings.atmosphere.removeAffectedMaterial(mat);
        }


        public override void PostThread()
        {
            base.PostThread();
            if (thread == null)
                return;

            // Setup mesh
            
            mesh.vertices = points.ToArray();
            mesh.colors = colors.ToArray();
            mesh.normals = normals.ToArray();
            mesh.SetIndices(indexes.ToArray(), MeshTopology.Points, 0);
            
            QuadNode qn = quad;

            go = new GameObject("Environment");
            go.transform.parent = qn.planetSettings.properties.environmentObject.transform;
            go.transform.localScale = Vector3.one;
            go.transform.position = Vector3.zero;
            go.transform.localPosition = Vector3.zero;
            MeshRenderer mr = go.AddComponent<MeshRenderer>();
            MeshFilter mf = go.AddComponent<MeshFilter>();
            mf.mesh = mesh;
            Material orgMat = (Material)Resources.Load("TreeTest");
            mat = new Material(orgMat);
            mat.CopyPropertiesFromMaterial(orgMat);
            mr.material = mat;
            mat = mr.material;
            go.tag = "Normal";
            go.layer = 10;
            // Set shader properties
            qn.planetSettings.atmosphere.initGroundMaterial(false, mr.material);
            qn.planetSettings.atmosphere.InitAtmosphereMaterial(mr.material);

            qn.planetSettings.atmosphere.addAffectedMaterial(mr.material);

            // mesh.bounds = new Bounds(mesh.bounds.center, mesh.bounds.size * 100);


            Quaternion q = Quaternion.FromToRotation(Vector3.up, quad.qb.center.P);
            Matrix4x4 rotMat = Matrix4x4.TRS(Vector3.zero, q, Vector3.one);
            mat.SetMatrix("worldRotMat", rotMat);
            mat.SetVector("b_tangent", tangent);
            mat.SetVector("b_binormal", binormal);




        }

        private Texture2D[] textures = new Texture2D[3];

        public void SetTextures()
        {
            int i = 0;
            if (mat == null)
                return;
            foreach (string s in qet.Textures)
            {
                if (s == "")
                    continue;
                if (textures[i] == null)
                {
                    textures[i] = (Texture2D)Resources.Load(RenderSettings.textureLocation + s);
 
                }
                if (textures[i] == null)
                    continue;
                if (textures[i].name != s)
                {
                    textures[i] = (Texture2D)Resources.Load(RenderSettings.textureLocation + s);

                }
                if (textures[i]!=null)
                   mat.SetTexture("_MainTex" + (i+1), textures[i]);
                i++;
            }

        }

        public QuadEnvironment(QuadNode qn, Material mat, int Count)
        {
            quad = qn;
            qet = qn.planetSettings.quadEnvironmentType;
            density = Count;
            mesh = new Mesh();
            thread = new TQueue();
            thread.thread = new Thread(new ThreadStart(ThreadedCreateMesh));
            thread.gt = this;
            AddThread(thread);

        }

        public void ThreadedCreateMesh()
        {
            threadDone = false;
            
            CreateMesh(density);
            threadDone = true;

        }


        public void insertRandomObjects(int N)
        {
        }
//            Debug.Log(cnt);
        

        public void RemoveObjects()
        {
            
        }

        public void Update()
        {
            SetTextures();
        }


   }

}