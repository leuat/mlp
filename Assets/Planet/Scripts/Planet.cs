using UnityEngine;
using System.Collections;


namespace LemonSpawn
{




    public class Planet
    {

        public PlanetSettings pSettings;// = new PlanetSettings();
        public Clouds clouds;
        public Rings rings;
        CubeSphere cube;
        public GameObject impostor;
        public TextMesh infoText;
        public GameObject infoTextGO;
        public static Color color = new Color(1f, 1f, 0.8f, 0.6f);

        public Planet(PlanetSettings p, CloudSettings cs)
        {
            pSettings = p;
            if (pSettings != null)
                pSettings.cloudSettings = cs;
        }
        public Planet(PlanetSettings p)
        {
            pSettings = p;
        }


        public void InterpolatePositions(int frame, float dt)
        {
            //		return;
            Frame f0 = pSettings.getFrame(frame);
            Frame f1 = pSettings.getFrame(frame + 1);
            if (f1 == null || f0 == null)
                return;

            Vector3 pos = f0.pos() + (f1.pos() - f0.pos()) * dt;
            float rot = (f0.rotation + (f1.rotation - f0.rotation) * dt);

            pSettings.pos.Set(pos);
            pSettings.rotation = rot;


        }

        public void Initialize(GameObject sun, Material ground, Material sky, Mesh sphere)
        {

            pSettings.atmosphere = new Atmosphere(sun, ground, sky, sphere, pSettings);
            pSettings.Initialize();
            if (pSettings.radius > RenderSettings.RingRadiusRequirement && pSettings.hasRings)
                rings = new Rings(pSettings, sun);
            if (pSettings.cloudSettings != null)
                clouds = new Clouds(sun, sphere, pSettings, pSettings.cloudSettings);
            if (pSettings.sea != null)
                pSettings.sea.Initialize(sun, sphere, pSettings);

        }

        public string getDistance()
        {
            double d = pSettings.localCamera.magnitude;
            if (d > 1E6)
                return (d /= RenderSettings.AU).ToString("F3") + " Au";
            else
                return (int)d + " Km";

        }

        public void UpdateText()
        {
            if (infoTextGO == null)
            {
                infoTextGO = new GameObject();
                //infoTextGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
                infoText = infoTextGO.AddComponent<TextMesh>();

            }

            infoTextGO.SetActive(RenderSettings.RenderText);

            infoText.fontSize = 40;
            infoTextGO.transform.position = pSettings.localCamera.normalized * -250;
            infoTextGO.transform.rotation = World.MainCameraObject.transform.rotation;
            infoText.color = color;
            infoText.text = pSettings.name + "\n" + getDistance() + "\nType:" + pSettings.planetType.Name;


        }



        public void Instantiate()
        {
            cube.SubDivide(RenderSettings.gridDivide);
            cube.Realise();

        }

        private void MaintainPlanet()
        {
            if (pSettings.terrainObject == null)
            {
                pSettings.terrainObject = new GameObject("terrain");
                pSettings.terrainObject.transform.parent = pSettings.gameObject.transform;
                pSettings.terrainObject.transform.localPosition = Vector3.zero;
                pSettings.terrainObject.transform.localScale = Vector3.one;
                cube = new CubeSphere(pSettings, false);
                if (impostor != null)
                    GameObject.Destroy(impostor);
            }
            Instantiate();
            pSettings.Update();
            UpdateText();

        }

        public void ConstrainCameraExterior()
        {
            Vector3 p = pSettings.localCamera.normalized;


            float h = pSettings.getPlanetSize() * (1 + pSettings.surface.GetHeight(p, 0)) + RenderSettings.MinCameraHeight;
            float ch = pSettings.localCamera.magnitude;
            if (ch < h)
            {
                World.MoveCamera(p * (h - ch));
            }

        }




        public void tagAll(GameObject g, string tag, int layer)
        {
            if (g.tag == tag)
                return;

            g.tag = tag;
            g.layer = layer;
            for (int i = 0; i < g.transform.childCount; i++)
            {
                GameObject go = g.transform.GetChild(i).gameObject;
                tagAll(go, tag, layer);
            }
        }


        private void cameraAndPosition()
        {
            DVector cam = new DVector(World.WorldCamera.x, World.WorldCamera.y, World.WorldCamera.z);
            cam.Scale(1.0 / RenderSettings.AU);
            DVector d = cam.Sub(pSettings.pos);
            double dist = d.Length() * RenderSettings.AU;
            //    double dist = pSettings.getHeight();
            d.Scale(-1.0 / d.Length());
            pSettings.currentDistance = dist;

            d.Scale(Mathf.Min((float)dist, (float)RenderSettings.LOD_ProjectionDistance));

            Vector3 pos = d.toVectorf();

            double ds = dist / RenderSettings.LOD_Distance;
            if (ds < 1)
            {
                tagAll(pSettings.parent, "Normal", 10);
                pSettings.currentTag = "Normal";
                pSettings.currentLayer = 10;
            }
            else
            {
                tagAll(pSettings.parent, "LOD", 9);
                pSettings.currentTag = "LOD";
                pSettings.currentLayer = 9;

            }

            double projectionDistance = dist / RenderSettings.LOD_ProjectionDistance;
            d.Scale(Mathf.Min((float)projectionDistance, (float)RenderSettings.LOD_ProjectionDistance));

            if (projectionDistance < 1)
            {
                pSettings.gameObject.transform.localScale = Vector3.one;

            }
            else
            {
                pSettings.gameObject.transform.localScale = Vector3.one * (float)(1.0 / projectionDistance);

            }

            pSettings.gameObject.transform.position = pos;

        }


        public void Update()
        {
            cameraAndPosition();

            if (rings != null)
                rings.Update();

            MaintainPlanet();
            // Rotation test		
            //		pSettings.rotation+=0.05f;


            pSettings.gameObject.transform.rotation = Quaternion.Euler(new Vector3(0, pSettings.rotation / (2 * Mathf.PI) * 360f, 0));
            //			pSettings.gameObject.transform.rotation = Quaternion.Euler(new Vector3(0, 0,pSettings.rotation/(2*Mathf.PI)*360f));
            //			Debug.Log (	pSettings.rotation/(2*Mathf.PI)*360f);

            if (pSettings.atmosphere != null)
                pSettings.atmosphere.Update();
            if (pSettings.cloudSettings != null)
                pSettings.cloudSettings.Update();

            if (clouds != null)
                clouds.Update();
            if (pSettings.sea != null)
                pSettings.sea.Update();

        }

    }

}