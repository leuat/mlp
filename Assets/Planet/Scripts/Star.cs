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
            MeshRenderer mr = pSettings.gameObject.AddComponent<MeshRenderer>();
            MeshFilter mf = pSettings.gameObject.AddComponent<MeshFilter>();
            starMaterial = new Material(sky.shader);
            mf.mesh = sphere;
            mr.material = starMaterial;
            pSettings.radius*=RenderSettings.GlobalRadiusScale;

            GameObject go = pSettings.gameObject;
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
