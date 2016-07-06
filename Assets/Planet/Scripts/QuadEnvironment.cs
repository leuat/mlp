using UnityEngine;
using System.Collections;

namespace LemonSpawn

{

   


    public class QuadEnvironment
    {
        private QuadNode quad;
        protected int maxCount = 250;
        public GameObject go;
        public Mesh mesh;
        private Material mat;
        private Vector3 tangent, binormal;

        private void CreateMesh(int N) {
            System.Random r = new System.Random();
            Vector3[] points = new Vector3[N];
            int[] indexes = new int[N];
            Color[] colors = new Color[N];

            Vector3 P = quad.qb.P[0].P;
            Vector3 D1 = (quad.qb.P[3].P - quad.qb.P[0].P);
            Vector3 D2 = (quad.qb.P[1].P - quad.qb.P[0].P);

            tangent = D1.normalized;
            binormal = D2.normalized;

            for(int i=0;i<N;++i) {
                float d1 = (float)r.NextDouble();
                float d2 = (float)r.NextDouble();
                points[i] = (P + D1*d1 + D2*d2).normalized*quad.planetSettings.getPlanetSize();
                indexes[i] = i;
                float fadeColor = (float)r.NextDouble()*0.4f;
                float fadeColorR = (float)r.NextDouble() * 0.4f;
                colors[i] = new Color(1f - fadeColorR,1f -fadeColor,1f - fadeColor*1.5f,1f);
         }
         
         mesh.vertices = points;
         mesh.colors = colors;
         mesh.SetIndices(indexes, MeshTopology.Points,0);
        }

        public void Destroy() {
            if (go==null)
              return;
            GameObject.DestroyImmediate(go);
            quad.planetSettings.atmosphere.removeAffectedMaterial(mat);
        }

        public QuadEnvironment(QuadNode qn, Material mat, int Count)
        {
            quad = qn;

            go = new GameObject("Environment");
            go.transform.parent = qn.planetSettings.transform;
            go.transform.localScale = Vector3.one;
            go.transform.position = Vector3.zero;
            go.transform.localPosition = Vector3.zero;

            MeshRenderer mr = go.AddComponent<MeshRenderer>();
            MeshFilter mf = go.AddComponent<MeshFilter>();
            mesh = new Mesh();
            CreateMesh(Count);
            mf.mesh = mesh;
            Material orgMat = (Material)Resources.Load("TreeTest");
            mat = new Material(orgMat);
            mat.CopyPropertiesFromMaterial(orgMat);
            mr.material = mat;
            mat = mr.material;
            go.tag = "Normal";
            go.layer = 10;
/*            go.tag = "LOD";
            go.layer = 9;*/
            // Set shader properties
            qn.planetSettings.atmosphere.initGroundMaterial(false, mr.material);
            qn.planetSettings.atmosphere.InitAtmosphereMaterial(mr.material);

            qn.planetSettings.atmosphere.addAffectedMaterial(mr.material);



            Quaternion q = Quaternion.FromToRotation(Vector3.up, quad.qb.center.P);
            Matrix4x4 rotMat= Matrix4x4.TRS(Vector3.zero, q, Vector3.one);
            mat.SetMatrix("worldRotMat", rotMat);
            mat.SetVector("b_tangent", tangent);
            mat.SetVector("b_binormal", binormal);
           

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