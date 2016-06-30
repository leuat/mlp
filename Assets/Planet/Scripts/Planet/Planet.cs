
using UnityEngine;
using System.Collections;


namespace LemonSpawn
{




    public class Planet
    {

        public PlanetSettings pSettings;// = new PlanetSettings();
        public Rings rings;
        CubeSphere cube;
        public GameObject impostor;
        public TextMesh infoText;
        public GameObject infoTextGO;
        public static Color color = new Color(1f, 1f, 0.8f, 0.6f);
        public Environment environment;
        public Clouds clouds;
        public BillboardClouds billboardClouds;
        public VolumetricClouds volumetricClouds;

        


        public Planet(PlanetSettings p)
        {
            pSettings = p;
        }


        public void Reset()
        {
            GameObject.Destroy(pSettings.properties.terrainObject);
        }


        public void InterpolatePositions(int frame, double dt)
        {
            //		return;
            Frame f0 = pSettings.getFrame(frame);
            Frame f1 = pSettings.getFrame(frame + 1);
            if (f1 == null || f0 == null)
                return;

            DVector pos = f0.pos() + (f1.pos() - f0.pos()) * dt;
            double rot = (f0.rotation + (f1.rotation - f0.rotation) * dt);

            pSettings.properties.pos = pos;
            pSettings.rotation = rot;
            

        }

        public void Initialize(GameObject sun, Material ground, Material sky, Mesh sphere)
        {
            if (RenderSettings.GPUSurface)
                pSettings.properties.gpuSurface = new GPUSurface(pSettings);

            pSettings.atmosphere = new Atmosphere(sun, ground, sky, sphere, pSettings);

            pSettings.Initialize();
            if (pSettings.radius > RenderSettings.RingRadiusRequirement && pSettings.hasRings)
                rings = new Rings(pSettings, sun);
            if (pSettings.sea != null)
                pSettings.sea.Initialize(sun, sphere, pSettings);

           

            if (pSettings.hasFlatClouds)
                   clouds = new Clouds(sun, sphere, pSettings, pSettings.cloudSettings);


            if (pSettings.hasEnvironment)
                environment = new Environment(pSettings);
            if (pSettings.hasBillboardClouds)
                billboardClouds = new BillboardClouds(pSettings); 


            if (pSettings.hasVolumetricClouds == true)
                volumetricClouds = new VolumetricClouds(sun, sphere, pSettings, pSettings.cloudSettings);


        }

        public string getDistance()
        {
            double d = pSettings.properties.localCamera.magnitude;
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
            infoTextGO.transform.position = pSettings.properties.localCamera.normalized * -250;
            infoTextGO.transform.rotation = World.MainCameraObject.transform.rotation;
            infoText.color = color;
            infoText.text = pSettings.name + "\n" + getDistance() + "\nType:" + pSettings.planetType.Name;


        }



        public void Instantiate()
        {
            cube.SubDivide(RenderSettings.ResolutionScale);
            cube.Realise();

        }

        private void MaintainPlanet()
        {
            if (pSettings.properties.terrainObject == null)
            {
                pSettings.properties.terrainObject = new GameObject("terrain");
                pSettings.properties.terrainObject.transform.parent = pSettings.gameObject.transform;
                pSettings.properties.terrainObject.transform.localPosition = Vector3.zero;
                pSettings.properties.terrainObject.transform.localScale = Vector3.one;
                cube = new CubeSphere(pSettings, false);
                if (impostor != null)
                    GameObject.Destroy(impostor);
            }
            Instantiate();
            pSettings.Update();
            UpdateText();
            if (SolarSystem.planet == this)
            {
                Physics.gravity = pSettings.transform.position.normalized * pSettings.Gravity;
            }

        }


        public void ConstrainCameraExterior()
        {
            Vector3 p = pSettings.properties.localCamera.normalized;


            Vector3 n;
            float h;
            if (RenderSettings.GPUSurface)
                h = pSettings.properties.gpuSurface.getPlanetSurface(p, out n).magnitude;
            else
                h = pSettings.getPlanetSize() * (1 + pSettings.surface.GetHeight(p, 0)) + RenderSettings.MinCameraHeight;
            float ch = pSettings.properties.localCamera.magnitude;
            if (ch < h)
            {
                World.MoveCamera(p * (h - ch));
            }

        }







        private void cameraAndPosition()
        {
            DVector cam = new DVector(World.WorldCamera.x, World.WorldCamera.y, World.WorldCamera.z);
            cam.Scale(1.0 / RenderSettings.AU);
            DVector d = cam.Sub(pSettings.properties.pos);
            double dist = d.Length() * RenderSettings.AU;
            //    double dist = pSettings.getHeight();
            d.Scale(-1.0 / d.Length());
            pSettings.properties.currentDistance = dist;

            d.Scale(Mathf.Min((float)dist, (float)RenderSettings.LOD_ProjectionDistance));

            Vector3 pos = d.toVectorf();
            double ds = dist / RenderSettings.LOD_Distance;
            //			Debug.Log(ds);
            if (ds < 1 && SolarSystem.planet == this)
            {
                Util.tagAll(pSettings.properties.parent, "Normal", 10);
                pSettings.setLayer(10, "Normal");
            }
            else
            {
                Util.tagAll(pSettings.properties.parent, "LOD", 9);
                pSettings.setLayer(9, "LOD");

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
            float rot = (float)(pSettings.rotation / (2 * Mathf.PI) * 360f);


       //     Debug.Log(pSettings.getHeight());

//             rot = 0;
            pSettings.gameObject.transform.rotation = Quaternion.Euler(new Vector3(0, rot, 0));

            if (pSettings.atmosphere != null)
                pSettings.atmosphere.Update();
                
            if (pSettings.cloudSettings != null)
                pSettings.cloudSettings.Update();


            if (pSettings.sea != null)
                pSettings.sea.Update();

            if (environment != null)
                environment.Update();

            if (clouds != null)
                clouds.Update();

            if (volumetricClouds != null)
                volumetricClouds.Update();
                
            if (billboardClouds != null)
                billboardClouds.Update();


            // Fun
            //pSettings.ExpSurfSettings2.y += (Mathf.PerlinNoise(Time.time, 0)-0.5f) * 0.001f;
            pSettings.ExpSurfSettings2.z += (Mathf.PerlinNoise(Time.time*0.02521f, 0) - 0.5f) * 0.005f;
//            pSettings.ExpSurfSettings2.x += (Mathf.PerlinNoise(Time.time*0.63452f, 0) - 0.5f) * 0.01f;

        }

    }

}