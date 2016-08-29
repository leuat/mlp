using UnityEngine;
using System.Collections;


namespace LemonSpawn
{

    public class Satellite : Planet
    {

        private Material starMaterial;

        public Satellite(PlanetSettings p)
        {
            pSettings = p;
        }

        

        public override void Initialize(GameObject sun, Material ground, Material sky, Mesh sphere)
        {

            GameObject main = new GameObject("Satellite");

            pSettings.properties.autoOrient = true;

            GameObject obj = (GameObject)GameObject.Instantiate(Resources.Load("SatelliteMain"), new Vector3(0, 0, 0), Quaternion.Euler(new Vector3(0,90,0)));
            main.transform.parent = pSettings.gameObject.transform;
            obj.transform.parent = main.transform;
            GameObject go = obj;
            obj.transform.localScale = Vector3.one * 0.01f;
            go.name = "spacecraft";
//            go.transform.localScale = Vector3.one * pSettings.radius;
            

//            starMaterial.SetColor("_Color", pSettings.properties.extraColor);

        }

        public override void Update()
        {

            //cameraAndPosition();
        }



    }

}