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

        EnvironmentMaterialReplace[] replaceList;

        public EnvironmentType(string pfName, EnvironmentMaterialReplace[] lst)
        {
            name = pfName.Trim();
            prefab = (GameObject)Resources.Load(pfName);
            replaceList = lst;
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



    public class Environment
    {
        private PlanetSettings planetSettings;

        private int maxCount = 250;
        private float maxDist = 500;

        private List<GameObject> objects = new List<GameObject>();
        private List<GameObject> removeObjects = new List<GameObject>();
        private List<EnvironmentType> environmentTypes = new List<EnvironmentType>();



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
/*            environmentTypes.Add(new EnvironmentType("baum_pine_m", std));
            environmentTypes.Add(new EnvironmentType("baum_l1_m", std));
            environmentTypes.Add(new EnvironmentType("baum_l2_m", std));
            */
            maxCount = planetSettings.environmentDensity;

        }

        public void initializeAllMaterial(Component[] components, EnvironmentType et)
        {
            foreach (Component c in components)
            {
                MeshRenderer mr = c.GetComponent<MeshRenderer>();
                if (mr != null)
                    mr.materials = et.Replace(mr.materials, planetSettings);
            }
        }


        public void insertRandomObjects(int N, int max)
        {
            Vector3 pos = planetSettings.localCamera.normalized;
            Vector3 camSurface = pos * planetSettings.getPlanetSize() * (1 + planetSettings.surface.GetHeight(pos, 0));


            if ((planetSettings.localCamera - camSurface).magnitude > maxDist)
                return;
            int cnt = 0;
            for (int i = 0; i < N; i++)
            {

                float w = 2 * maxDist;

                Vector3 sphere = new Vector3((float)Util.rnd.NextDouble() * w - w / 2, (float)Util.rnd.NextDouble() * w - w / 2, (float)Util.rnd.NextDouble() * w - w / 2);
                sphere = sphere.normalized * w * 0.9f;

                pos = planetSettings.localCamera + sphere;
                pos = pos.normalized;
                Vector3 realP = pos * planetSettings.getPlanetSize() * (1 + planetSettings.surface.GetHeight(pos, 0));


                float dist = (planetSettings.localCamera - realP).magnitude;
                //                if (dist < maxDist)
                {
                    Vector3 normal = planetSettings.surface.GetNormal(pos, 0, planetSettings.getPlanetSize());
                    if (Vector3.Dot(normal, pos) < 0.98)
                        continue;

                    EnvironmentType et = environmentTypes[Util.rnd.Next()%environmentTypes.Count];

                    GameObject go = (GameObject)GameObject.Instantiate(et.prefab, realP - planetSettings.localCamera, Quaternion.FromToRotation(Vector3.up, pos));
                    //                go.transform.rotation = Quaternion.FromToRotation(Vector3.up, pos) * go.transform.rotation;
                    go.transform.RotateAround(pos, Util.rnd.Next() % 360);
                    GameObject.Destroy(go.GetComponent<Rigidbody>());
                    go.transform.localScale = Vector3.one * 0.4f * (float)(0.8 + (Util.rnd.NextDouble() * 0.4))*0.4f;
                    go.transform.parent = planetSettings.transform;
                    Util.tagAll(go, "Normal", 10);

                    initializeAllMaterial(go.GetComponents<Component>(), et);





                    objects.Add(go);
                    cnt++;
                    if (cnt > max)
                        return;

                }
            }
        }
//            Debug.Log(cnt);
        

        public void RemoveObjects()
        {
            

            foreach (GameObject go in objects)
            {
                if ((go.transform.localPosition - planetSettings.localCamera).magnitude > maxDist)
                {
                    removeObjects.Add(go);
               }
            }

            foreach(GameObject go in removeObjects)
            {
                objects.Remove(go);
                GameObject.DestroyImmediate(go);
            }
            removeObjects.Clear();
        }

        

        public void Update()
        {
//            if (Util.rnd.NextDouble()>0.98)
            insertRandomObjects(maxCount - objects.Count,50);
            RemoveObjects();

//            foreach (GameObject go in objects)
  //              Debug.DrawLine(go.transform.position + planetSettings.localCamera, go.transform.position + planetSettings.localCamera + go.transform.forward, Color.green, 0.001f);

        }


   }

}