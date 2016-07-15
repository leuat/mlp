using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

namespace LemonSpawn
{

    public class ExampleScene : World
    {

        public GameObject canvas;

        public static void addBall()
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.transform.parent = SolarSystem.planet.pSettings.transform;
            go.transform.localPosition = SolarSystem.planet.pSettings.properties.localCamera + World.MainCamera.transform.forward * 2;
            SolarSystem.planet.pSettings.tagGameObject(go);
            go.AddComponent<Rigidbody>();
            Material ball = (Material)Resources.Load("BallMaterial");
            go.GetComponent<MeshRenderer>().material = ball;
            SolarSystem.planet.pSettings.atmosphere.InitAtmosphereMaterial(ball);
        }

        public static void addWheels()
        {
            
            GameObject go = GameObject.Find("car_root");
            GameObject gorb = GameObject.Find("car_root");
            if (go == null)
                return;
            go.transform.parent = SolarSystem.planet.pSettings.transform;
            go.transform.localPosition = SolarSystem.planet.pSettings.properties.localCamera + World.MainCamera.transform.forward * 2;
            go.transform.rotation = Quaternion.FromToRotation(Vector3.up, SolarSystem.planet.pSettings.transform.position * -1);


            SolarSystem.planet.pSettings.tagGameObjectAll(go);
            SolarSystem.planet.pSettings.InitializeAtmosphereMaterials(go);
            Rigidbody rb = gorb.GetComponent<Rigidbody>();
            rb.velocity.Set(0, 0, 0);
            rb.angularVelocity.Set(0, 0, 0);
            rb.Sleep();
            /*            go.AddComponent<Rigidbody>();
                        Material ball = (Material)Resources.Load("BallMaterial");
                        go.GetComponent<MeshRenderer>().material = ball;
                        SolarSystem.planet.pSettings.atmosphere.InitAtmosphereMaterial(ball);*/
        }

        void CreateTestMesh(int startRange, int endRange, Mesh currMesh, int maxPointsPerMesh)
        {
            Vector3[] points = new Vector3[maxPointsPerMesh];
            int[] indexes = new int[maxPointsPerMesh];
            Color[] colors = new Color[maxPointsPerMesh];
            for (int i = 0; i < points.Length; ++i)
            {
                points[i] = new Vector3(Random.Range(startRange, endRange), Random.Range(startRange, endRange), Random.Range(startRange, endRange));
                indexes[i] = i;
                colors[i] = new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), 1.0f);
            }
         
            currMesh.vertices = points;
            currMesh.colors = colors;
            currMesh.SetIndices(indexes, MeshTopology.Points, 0);

         
        }

        public void CreateTestObject()
        {
/*        GameObject go = new GameObject("Tree test mesh");
        MeshRenderer mr = go.AddComponent<MeshRenderer>();
        MeshFilter mf = go.AddComponent<MeshFilter>();
        Mesh m = new Mesh();
        CreateTestMesh(-100, 100, m, 100);
        mf.mesh = m;
        mr.material = (Material)Resources.Load("TreeTest");
        go.tag = "Normal";
        go.layer = 10;*/
        }


        float currentTime = 0;
        int currentFrame = 0;
        float timeScale = 1;
        float maxFrames = 1;
        float curFrames = 0;

        private void RecordFrames()
        {
            curFrames -= Time.smoothDeltaTime * timeScale * 2f;
            if (curFrames <= 0)
            {
                curFrames = maxFrames;
                Debug.Log("Adding frame:" + currentFrame);
                AddCurrentCameraPos();
            }
        }


        public void AddCurrentCameraPos()
        {
            SerializedCamera sc = SpaceCamera.getSZCamera();
            sc.fov = MainCamera.fieldOfView;
            sc.time = currentTime;
            sc.frame = currentFrame;
            currentTime += currentFrame;// Time.deltaTime * timeScale*20f; 
            szWorld.Cameras.Add(sc);
           
            currentFrame++;

        }

        protected void PopulateOverviewList(string box)
        {
            Dropdown cbx = GameObject.Find(box).GetComponent<Dropdown>();
            cbx.ClearOptions();
            List<Dropdown.OptionData> l = new List<Dropdown.OptionData>();
            l.Add(new Dropdown.OptionData("None"));
            foreach (Planet p in solarSystem.planets)
            {
                Dropdown.OptionData ci = new Dropdown.OptionData();
                ci.text = p.pSettings.name;
                string n = p.pSettings.name;
                l.Add(ci);
            }
            //      foreach (ComboBoxItem i in l)
            //          Debug.Log (i.Caption);

            cbx.AddOptions(l);

        }


        public void SaveWorld()
        {
            string uuid = Verification.IDValues[0].ID;
            szWorld.SaveSerializedWorld(RenderSettings.path + RenderSettings.dataDir + "example.xml", solarSystem, uuid);
        }




        public override void Update()
        {
            base.Update();
            if (Input.GetKeyUp(KeyCode.Space))
            {
                AddCurrentCameraPos();
                Debug.Log("Added current camera pos: " + currentTime);
            }

            if (Input.GetKeyUp(KeyCode.R))
            {
                //                if (!RenderSettings.RecordingVideo)
                //                  szWorld.Cameras.Clear();
                RenderSettings.RecordingVideo = !RenderSettings.RecordingVideo;
                Debug.Log("Rendering Frames: " + RenderSettings.RecordingVideo);
            }

            if (Input.GetKeyUp(KeyCode.B))
                addBall();
            if (Input.GetKeyUp(KeyCode.V))
                addWheels();

            if (RenderSettings.RecordingVideo)
                RecordFrames();

            if (followVehicle)
                FollowVehicle("car_root");

            if (Input.GetKeyUp (KeyCode.F12)) {
                RenderSettings.RenderMenu = !RenderSettings.RenderMenu;
                canvas.SetActive(RenderSettings.RenderMenu);
            }
            if (Input.GetKeyUp (KeyCode.L)) {
                RenderSettings.toggleClouds = !RenderSettings.toggleClouds;
            }
            if (Input.GetKeyUp(KeyCode.K))
            {
                RenderSettings.displayDebugLines = !RenderSettings.displayDebugLines;
            }

 
        }

        protected override void Log()
        {
            string s = "";
            float val = 1;
            if (ThreadQueue.orgThreads != 0)
                val = (ThreadQueue.threadQueue.Count / (float)ThreadQueue.orgThreads);
            
            int percent = 100 - (int)(100 * val);
            
            
//          load_percent = percent;
            
            s += "Version: " + RenderSettings.version.ToString("0.00") + " \n";
            //if (RenderSettings.isVideo)
            //          s+="Progress: " + percent + " %\n";
            s += "Progress: " + ThreadQueue.threadQueue.Count + " \n";
            s += "Height: " + stats.Height.ToString("0.00") + " km \n";
            //s+="Velocity: " + stats.Velocity.ToString("0.00") + " km/s\n";
            s += RenderSettings.extraText;
            GameObject info = GameObject.Find("Logging");
            if (info != null)
                info.GetComponent<Text>().text = s;
        }

        private void FollowVehicle(string s)
        {
            GameObject go = GameObject.Find(s);
            if (go == null)
                return;
            Vector3 t = go.transform.position + go.transform.forward*RenderSettings.vehicleFollowDistance;
            Vector3 c = go.transform.position + go.transform.forward * RenderSettings.vehicleFollowDistance * -1 + go.transform.up * RenderSettings.vehicleFollowHeight;
            Vector3 up = SolarSystem.planet.pSettings.transform.position.normalized * -1;
            float t1 = 0.9f;

            vehicleDir = vehicleDir * (1 - t1) + t * t1;
            vehiclePos = vehiclePos * (1 - t1) + c * t1;


            float t0 = 0.95f;
            SpaceCamera.MoveCamera(vehiclePos*(1-t0));
            SpaceCamera.SetLookCamera(vehicleDir.normalized * (1-t0) + SpaceCamera.curDir * t0, up);

        }

        public override void Start() {
            base.Start();
            canvas = GameObject.Find ("Canvas");

        }

    }




}
