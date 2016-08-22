
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

        
        public Planet() {

        }

        public Planet(PlanetSettings p)
        {
            pSettings = p;
        }


        public void Reset()
        {
            GameObject.Destroy(pSettings.properties.terrainObject);
            GameObject.Destroy(pSettings.properties.environmentObject);
        }


        public void InterpolatePositions(int frame, double dt)
        {
            //		return;
        //    Debug.Log("Frame:" + frame + " / " + pSettings.properties.Frames.Count);
            if (frame>=pSettings.properties.Frames.Count) {
                pSettings.properties.pos.Set( pSettings.properties.orgPos);
                return;    
                }
            Frame f0 = pSettings.getFrame(frame);
            Frame f1 = pSettings.getFrame(frame + 1);
            if (f1 == null || f0 == null)
            {
                //pSettings.properties.pos.Set(pSettings.properties.orgPos);
                return;
            }

            DVector pos = f0.pos() + (f1.pos() - f0.pos()) * dt;

//            double rot = (f0.rotation + (f1.rotation - f0.rotation) * dt);
            double rot = Util.LerpDegrees(f0.rotation, f1.rotation, dt);


            pSettings.properties.pos = pos;
            pSettings.rotation = rot;
            

        }

        public virtual void Initialize(GameObject sun, Material ground, Material sky, Mesh sphere)
        {
//            if (RenderSettings.GPUSurface)
  //              pSettings.properties.gpuSurface = new GPUSurface(pSettings);


            pSettings.atmosphere = new Atmosphere(sun, ground, sky, sphere, pSettings);

            pSettings.Initialize();
//            if (pSettings.radius > RenderSettings.RingRadiusRequirement && pSettings.hasRings)

            rings = new Rings(pSettings, sun);

            if (pSettings.sea != null)
                pSettings.sea.Initialize(sun, sphere, pSettings);

           

            if (pSettings.hasFlatClouds)
                   clouds = new Clouds(sun, sphere, pSettings, pSettings.cloudSettings);

            // Ignore old environment type
//            if (pSettings.hasEnvironment)
  //              environment = new Environment(pSettings);
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
        /* Earth psettings 
          * 2.5 0.65 1.7
         *  -0.5 0.01 5
         *  0.3 11 1
         * 
        */ 
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
            if (World.SzWorld.useSpaceCamera) {
                infoTextGO.transform.position = pSettings.properties.localCamera.normalized * -250;
                infoTextGO.transform.rotation = World.MainCameraObject.transform.rotation;
            infoText.color = color;
            if (pSettings == null)
                return;
            if (pSettings.planetType == null)
                return;
            infoText.text = pSettings.name + "\n" + getDistance() + "\nType:" + pSettings.planetType.name;

            }

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
                pSettings.properties.terrainObject.transform.localRotation = Quaternion.identity;

                pSettings.properties.environmentObject = new GameObject("environment");
                pSettings.properties.environmentObject.transform.parent = pSettings.gameObject.transform;
                pSettings.properties.environmentObject.transform.localPosition = Vector3.zero;
                pSettings.properties.environmentObject.transform.localScale = Vector3.one;
                pSettings.properties.environmentObject.transform.localRotation = Quaternion.identity;
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

            float dh = 0.00025f*pSettings.radius;
            float h;
//            if (RenderSettings.GPUSurface && pSettings.properties.gpuSurface!=null)
                h = pSettings.properties.gpuSurface.getPlanetSurfaceOnly(p).magnitude + dh;
  //          else
    //            h = pSettings.getPlanetSize() * (1 + pSettings.surface.GetHeight(p, 0)) + RenderSettings.MinCameraHeight;
            float ch = pSettings.properties.localCamera.magnitude;
            if (ch < h)
            {
                World.MoveCamera(p * (h - ch ));
            }

        }




        private void UpdateSpaceCameraPlanetPosition() {
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
            //          Debug.Log(ds);
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




        protected void cameraAndPosition()
        {
            if (World.SzWorld.useSpaceCamera)
                UpdateSpaceCameraPlanetPosition();
            else {
                //pSettings.gameObject.transform.position

            }            



        }


        public virtual void Update()
        {
            cameraAndPosition();


            if (this == SolarSystem.planet) {
                float ch = (World.MainCameraObject.transform.position - pSettings.transform.position).magnitude;
//                Debug.Log(ch);
            }

            if (rings != null)
                rings.Update();

            MaintainPlanet();
            float rot = (float)(pSettings.rotation / (2 * Mathf.PI) * 360f);

//            Debug.Log(pSettings.radius);

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

 //          pSettings.ExpSurfSettings2.z += (Mathf.PerlinNoise(Time.time*0.0521f, 0) - 0.5f) * 0.04f;
//            pSettings.ExpSurfSettings2.x += (Mathf.PerlinNoise(Time.time*0.63452f, 0) - 0.5f) * 0.01f;

        }

    }

}