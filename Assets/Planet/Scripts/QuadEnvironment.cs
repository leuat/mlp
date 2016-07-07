using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace LemonSpawn

{

   


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
        List<int> indexes = new List<int>();
        List<Color> colors = new List<Color>();


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
            indexes.Clear();
            for(int i=0;i<N;++i) {
                float d1 = (float)r.NextDouble();
                float d2 = (float)r.NextDouble();
                bool isOk = true;

                Vector3 proposedPos = (P + D1 * d1 + D2 * d2).normalized * quad.planetSettings.getPlanetSize();




                Vector3 realPos = proposedPos;
                //isOk = VerifyPosition(proposedPos, out realPos);

              

                if (isOk)
                {
                    points.Add(realPos);
                    indexes.Add(cur);
                    float fadeColor = (float)r.NextDouble() * 0.4f;
                    float fadeColorR = (float)r.NextDouble() * 0.4f;
                    colors.Add(new Color(1f - fadeColorR, 1f - fadeColor, 1f - fadeColor * 1.5f, 1f));
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
            mesh.SetIndices(indexes.ToArray(), MeshTopology.Points, 0);
            
            QuadNode qn = quad;

            go = new GameObject("Environment");
            go.transform.parent = qn.planetSettings.transform;
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

        public QuadEnvironment(QuadNode qn, Material mat, int Count)
        {
            quad = qn;
            
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
        }


   }

}