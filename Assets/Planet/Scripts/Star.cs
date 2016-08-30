using UnityEngine;
using System.Collections;


namespace LemonSpawn {

    public class Star : Planet {

        private Material starMaterial;
                                
        public Star(PlanetSettings p)
        {
            pSettings = p;
        }


        public override void Initialize(GameObject sun, Material ground, Material sky, Mesh sphere)
        {
            GameObject obj = new GameObject();
            obj.transform.parent = pSettings.gameObject.transform;

            MeshRenderer mr = obj.AddComponent<MeshRenderer>();
            MeshFilter mf = obj.AddComponent<MeshFilter>();
            starMaterial = new Material(sky.shader);
            starMaterial.CopyPropertiesFromMaterial(sky);
            mf.mesh = sphere;
            mr.material = starMaterial;
            //pSettings.radius*=RenderSettings.GlobalRadiusScale;

            GameObject go = obj;
            go.name = "star";
            go.transform.localScale = Vector3.one * pSettings.radius;

            //Debug.Log("Heisann");

            starMaterial.SetColor("_Color", pSettings.properties.extraColor);

        }

        public override void Update() {
            //cameraAndPosition();
        }



    }

}
