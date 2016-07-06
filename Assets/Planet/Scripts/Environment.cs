using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace LemonSpawn

{

   

    public class EnvironmentMaterialReplace
    {
        public string originalMaterialName;
        public List<Material> materials = new List<Material>();
        public List<string> materialStrings = new List<string>();

        public EnvironmentMaterialReplace(string n, string [] mats)
        {
            originalMaterialName = n;
            foreach (string s in mats)
            {
                materials.Add((Material)Resources.Load(s));
                materialStrings.Add(s);
            }
        }
        public Material getRandomMat()
        {
            return materials[Util.rnd.Next() % materials.Count];
        }
        public Material getRandomInstantiatedMat()
        {
            string m = materialStrings[Util.rnd.Next() % materials.Count];
            Material mat = (Material)Resources.Load(m);
            if (mat == null)
                Debug.Log("Cound not find material " + m);
            return mat;
        }
    }



public class EnvironmentType
    {
        public string name;
        public GameObject prefab;
        public float heightAdd = 0;
        public float heightMul = 1;
        public float normalThreshold = 0.98f;
        public float scale = 0.1f;
        public float maxDist = 500;

        EnvironmentMaterialReplace[] replaceList;

        public EnvironmentType(string pfName, EnvironmentMaterialReplace[] lst)
        {
            name = pfName.Trim();
            prefab = (GameObject)Resources.Load(pfName);
            replaceList = lst;
        }

        public EnvironmentType(string pfName, EnvironmentMaterialReplace[] lst, float hAdd, float hMul, float nThreshold, float s, float mdist)
        {
            name = pfName.Trim();
            prefab = (GameObject)Resources.Load(pfName);
            replaceList = lst;
            heightMul = hMul;
            heightAdd = hAdd;
            normalThreshold = nThreshold;
            scale = s;
            maxDist = mdist;
        }

        public EnvironmentMaterialReplace findReplace(string materialName)
        {
            if (replaceList == null)
                return null;
            foreach (EnvironmentMaterialReplace er in replaceList)
            {
                if (materialName.Contains(er.originalMaterialName))
                    return er;
            }
            return null;
        }


        public Material[] Replace(Material[] materials, PlanetSettings planetSettings)
        {
            for (int i=0;i<materials.Length;i++)
            {
                EnvironmentMaterialReplace replace = findReplace(materials[i].name);
                if (replace!=null)
                {
                    materials[i] = replace.getRandomInstantiatedMat();
                }
                
                planetSettings.atmosphere.InitAtmosphereMaterial(materials[i]);
                //Vector3 c1 = (Vector3.one - Util.randomVector(0.1f, 0.2f, 0.2f)) * 2;
                //materials[i].SetColor("_Color", new Color(c1.x, c1.y, c1.z, 1));
            }
            return materials;

        }


    }



    public class EnvironmentObject
    {
        public GameObject go;
        public EnvironmentType et;
        public List<Material> materials = new List<Material>();
        public EnvironmentObject(GameObject g, EnvironmentType e)
        {
            go = g;
            et = e;
        }

    }

    public class Environment
    {
        protected PlanetSettings planetSettings;

        protected int maxCount = 250;
        protected float maxDist;
        protected List<EnvironmentObject> objects = new List<EnvironmentObject>();
        protected List<EnvironmentObject> removeObjects = new List<EnvironmentObject>();
        protected List<EnvironmentType> environmentTypes = new List<EnvironmentType>();

        public Environment()
        {

        }

        protected void calculateMaxMaxDist()
        {
            maxDist = 0;
            foreach (EnvironmentType et in environmentTypes)
                maxDist = Mathf.Max(et.maxDist, maxDist);

        }

        public Environment(PlanetSettings ps)
        {
            planetSettings = ps;

            //            prefabs.Add((GameObject)Resources.Load("Conifer_Desktop"));
            //          prefabs.Add((GameObject)Resources.Load("Palm_Desktop"));
            //            prefabs.Add((GameObject)Resources.Load("Broadleaf_Desktop"));

            /*            prefabs.Add((GameObject)Resources.Load("baum_pine_m"));
                        prefabs.Add((GameObject)Resources.Load("baum_l1_m"));
                        prefabs.Add((GameObject)Resources.Load("baum_l2_m"));
                        */

            EnvironmentMaterialReplace[] std = new EnvironmentMaterialReplace[] {
                    new EnvironmentMaterialReplace("LBark", new string [] { "LGnarled", "LWood1", "LMeaty", "LSlimySkin", "LStudded" }),
                    new EnvironmentMaterialReplace("LLeaf", new string [] { "LLeaf", "LConiferLeaf" })
                    };

            environmentTypes.Add(new EnvironmentType("LTree1", std));
//            environmentTypes.Add(new EnvironmentType("PSystem", std));

            environmentTypes.Add(new EnvironmentType("MeatTree", std));
            environmentTypes.Add(new EnvironmentType("Horetre", std));
            /*            environmentTypes.Add(new EnvironmentType("baum_pine_m", std));
                        environmentTypes.Add(new EnvironmentType("baum_l1_m", std));
                        environmentTypes.Add(new EnvironmentType("baum_l2_m", std));
                        */
            maxCount = planetSettings.environmentDensity;
            calculateMaxMaxDist();
        }


        public void initializeAllMaterial(Component[] components, EnvironmentType et, List<Material> materials)
        {
            foreach (Component c in components)
            {
                MeshRenderer mr = c.GetComponent<MeshRenderer>();
                if (mr != null)
                {
                    mr.materials = et.Replace(mr.materials, planetSettings);
                    foreach (Material m in mr.materials)
                        materials.Add(m);
                }
            }
        }

        public void UpdateMaterials() { 
            foreach (EnvironmentObject eo in objects) 
                foreach (Material m in eo.materials)
                    planetSettings.atmosphere.InitAtmosphereMaterial(m);

        }



        public void insertRandomObjects(int N, int max)
        {
            Vector3 pos = planetSettings.properties.localCamera.normalized;
            Vector3 camSurface = pos * planetSettings.getPlanetSize() * (1 + planetSettings.surface.GetHeight(pos, 0));


            if ((planetSettings.properties.localCamera - camSurface).magnitude > maxDist)
                return;

            int cnt = 0;
            for (int i = 0; i < N; i++)
            {
                EnvironmentType et = environmentTypes[Util.rnd.Next() % environmentTypes.Count];
                //Debug.Log((planetSettings.properties.localCamera - camSurface).normalized);

                float w = 2 * et.maxDist;

                Vector3 sphere = new Vector3((float)Util.rnd.NextDouble() * w - w / 2, (float)Util.rnd.NextDouble() * w - w / 2, (float)Util.rnd.NextDouble() * w - w / 2);
                sphere = sphere.normalized * w * 0.9f;

                pos = planetSettings.properties.localCamera + sphere;
                pos = pos.normalized;
  //              float height = planetSettings.surface.GetHeight(pos, 0);
//                Vector3 realP = pos * planetSettings.getPlanetSize() * (1 + (1.0f) * height);
                Vector3 realP = planetSettings.properties.gpuSurface.getPlanetSurfaceOnly(pos);
                float height = (realP - pos*planetSettings.getPlanetSize()).magnitude;
                Debug.Log(height);

                
                float dist = (planetSettings.properties.localCamera - realP).magnitude;
                //                if (dist < maxDist)
                {
/*                    Vector3 normal = planetSettings.surface.GetNormal(pos, 0, planetSettings.getPlanetSize());
                    if (Vector3.Dot(normal, pos) < et.normalThreshold)
                        continue;*/


                    float hadd = et.heightAdd / planetSettings.radius;
                    float h = (float)(hadd * 0.75 + Util.rnd.NextDouble() * 0.5 * hadd);

                    //realP = pos * planetSettings.getPlanetSize() * (1+ h + (et.heightMul) * height);


                    GameObject go = (GameObject)GameObject.Instantiate(et.prefab, realP - planetSettings.properties.localCamera, Quaternion.FromToRotation(Vector3.up, pos));
                    //                go.transform.rotation = Quaternion.FromToRotation(Vector3.up, pos) * go.transform.rotation;
                    go.transform.RotateAround(pos, Util.rnd.Next() % 360);
                    GameObject.Destroy(go.GetComponent<Rigidbody>());
                    go.transform.localScale = Vector3.one * (float)(0.8 + (Util.rnd.NextDouble() * 0.4))*et.scale*5;
                    go.transform.parent = planetSettings.transform;
                    Util.tagAll(go, "Normal", 10);
                    Debug.Log("ADDING");

                    EnvironmentObject eo = new EnvironmentObject(go, et);
                    Renderer rr = go.GetComponent<Renderer>();
                    if (rr)
                        eo.materials.Add(rr.material);

                    initializeAllMaterial(go.GetComponents<Component>(), et, eo.materials);


                    objects.Add(eo);
                    cnt++;
                    if (cnt > max)
                        return;

                }
            }
        }
//            Debug.Log(cnt);
        

        public void RemoveObjects()
        {
            
            
            foreach (EnvironmentObject eo in objects)
            {
                if ((eo.go.transform.localPosition - planetSettings.properties.localCamera).magnitude > eo.et.maxDist)
                {
                    removeObjects.Add(eo);
               }
            }

            foreach(EnvironmentObject eo in removeObjects)
            {
                objects.Remove(eo);
                GameObject.DestroyImmediate(eo.go);
            }
            removeObjects.Clear();
        }

        public void Update()
        {
//            if (Util.rnd.NextDouble()>0.98)
            insertRandomObjects(maxCount - objects.Count,50);
            RemoveObjects();
            UpdateMaterials();

//            foreach (GameObject go in objects)
  //              Line(go.transform.position + planetSettings.properties.localCamera, go.transform.position + planetSettings.properties.localCamera + go.transform.forward, Color.green, 0.001f);

        }


   }

}