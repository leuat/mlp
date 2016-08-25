using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace LemonSpawn {
    
	public class SolarSystemViewverZoom : WorldMC {
		private List<DisplayPlanet> dPlanets = new List<DisplayPlanet>();
		private Vector3 mouseAccel = new Vector3();
		private Vector3 focusPoint = Vector3.zero;
		private Vector3 focusPointCur = Vector3.zero;
        private float scrollWheel, scrollWheelAccel;
		private DisplayPlanet selected = null;
        public static GameObject linesObject = null;
        private GameObject pnlInfo = null;
        public int selectPlanet = 2;

		private void UpdateCamera () {
			float s = 1.0f;
			float theta = 0.0f;
			float phi = 0.0f;

			if (Input.GetMouseButton (1)) {
				theta = s * Input.GetAxis ("Mouse X");
				phi = s * Input.GetAxis ("Mouse Y") * -1.0f;
			}
			mouseAccel += new Vector3 (theta, phi, 0);
			focusPointCur += (focusPoint - focusPointCur) * 0.1f;
			mainCamera.transform.RotateAround (focusPointCur, Vector3.up, mouseAccel.x);
			mainCamera.transform.RotateAround (focusPointCur, mainCamera.transform.right, mouseAccel.y);
			mainCamera.transform.LookAt (focusPointCur);
			mouseAccel *= 0.9f;
		}


		private void PopulateWorld() {
		}


        private void CreateLine(Vector3 f, Vector3 t, float c1, float c2, float w) {

            Color c = new Color(0.3f, 0.4f, 1.0f,1.0f);
            GameObject g = new GameObject ();
            LineRenderer lr = g.AddComponent<LineRenderer> ();
            lr.material = new Material(Shader.Find("Particles/Additive"));//(Material)Resources.Load ("LineMaterial");
            lr.SetWidth (w, w);
            lr.SetPosition (0, f);
            lr.SetPosition (1, t);
            Color cc1 = c*c1;
            Color cc2 = c*c2;
            cc1.a = 0.4f;
            cc2.a = 0.4f;
            lr.SetColors(cc1,cc2);
        }

        private void CreateAxis() {
            float w = 10000;

            CreateLine(Vector3.zero, Vector3.up*w, 1, 0.2f, 5);
            CreateLine(Vector3.zero, Vector3.up*w*-1, 1, 0.2f, 5);
            CreateLine(Vector3.zero, Vector3.right*w, 1, 0.2f, 5);
            CreateLine(Vector3.zero, Vector3.right*w*-1, 1, 0.2f, 5);
            CreateLine(Vector3.zero, Vector3.forward*w, 1, 0.2f, 5);
            CreateLine(Vector3.zero, Vector3.forward*w*-1, 1, 0.2f, 5);

        }

        public static GameObject satellite = null;

        private void InitializeSinglePlanet()
        {
            SzWorld = RenderSettings.currentSZWorld; 
            szWorld = SzWorld;
            selectPlanet = 2;
            SerializedPlanet sp = szWorld.Planets[selectPlanet];
            SerializedWorld sz = SzWorld;
            GameObject go = new GameObject(sp.name);
            go.transform.parent = transform;
            PlanetSettings ps = sp.DeSerialize(go, 0, 1);
            //Debug.Log(sp.radius);
            //ps.radius *= SSVSettings.PlanetSizeScale;
            Planet p;
            p = new Planet(ps);
            p.pSettings.properties.parent = go;
            Material groundMaterial = (Material)Resources.Load("GroundMaterialGPU");

            p.Initialize(sun, groundMaterial, (Material)Resources.Load("SkyMaterial"), sphere);

            solarSystem.planets.Add(p);
            //Debug.Log(p.pSettings.radius);

         }

		public override void Start () { 
			CurrentApp = Verification.SolarSystemViewerName;
            RenderSettings.path = Application.dataPath + "/../";
         
            RenderSettings.UseThreading = true;
            RenderSettings.reCalculateQuads = false;
            RenderSettings.GlobalRadiusScale = 1;
            RenderSettings.maxQuadNodeLevel = m_maxQuadNodeLevel;
            RenderSettings.sizeVBO = szWorld.resolution;
            RenderSettings.minQuadNodeLevel = m_minQuadNodeLevel;
            RenderSettings.MoveCam = false;
            RenderSettings.ResolutionScale = szWorld.resolutionScale;
            RenderSettings.usePointLightSource = false;




            pnlInfo = GameObject.Find("pnlInfo");
            pnlInfo.SetActive(false);
            solarSystem = new SolarSystem(sun, sphere, transform, (int)szWorld.skybox);
			PlanetTypes.Initialize ();
            SetupCloseCamera();
			MainCamera = mainCamera.GetComponent<Camera> ();
			//PopulateFileCombobox("ComboBoxLoadFile","xml");
			SzWorld = szWorld;
            slider = GameObject.Find ("Slider");

            setText("TextVersion", "Version: " + RenderSettings.version.ToString("0.00"));


            linesObject = new GameObject("Lines");
            CreateAxis();
            InitializeSinglePlanet();
//			LoadData ();
		}


        private void UpdateZoom() {
            scrollWheelAccel = Input.GetAxis("Mouse ScrollWheel")*0.5f;
            scrollWheel = scrollWheel * 0.9f + scrollWheelAccel*0.1f;
//            Debug.Log(ScrollWheel);

            Vector3 pos = MainCamera.transform.position;
            if (selected!=null) {
                pos-=selected.go.transform.position;
                MainCamera.transform.position = pos*(1+scrollWheel) + selected.go.transform.position;
            }
            else
                MainCamera.transform.position = pos*(1+scrollWheel);

        }


        public void LeaveZoom()
        {
            SolarSystemViewverMain.Reload=  true;
            Application.LoadLevel(2);
        }

		public override void Update () {
			UpdateCamera ();
            UpdateZoom();
            solarSystem.Update();
            if (RenderSettings.UseThreading) 
                ThreadQueue.MaintainThreadQueue();


            UpdatePlay();

            if (Input.GetKey(KeyCode.Escape))
            {
                Application.Quit();
            }
            // Now force update planet position



        }

        protected void OnGUI() {
		}



        public void Slide()
        {
//            if (szWorld.getMaxFrames()<=2)
  //              return;
            float v = slider.GetComponent<Slider>().value;
            SSVSettings.currentFrame = v;
            szWorld.InterpolatePlanetFrames(v, solarSystem.planets);
            foreach (DisplayPlanet dp in dPlanets) {
                dp.UpdatePosition();
            }

        }


        private void setPlaySpeed(float v)
        {
            if (m_playSpeed == v)
            {
                m_playSpeed = 0;
            }
            else
            {
                m_playSpeed = v;
            }

        }

        public void playNormal()
        {
            setPlaySpeed(0.000025f);

        }

        public void playFast()
        {
            setPlaySpeed(0.0001f);
        }


        protected void UpdatePlay()
        {
  //          Debug.Log(Time.time + " " + m_playSpeed);
            if (m_playSpeed > 0 && solarSystem.planets.Count!=0)
            {
                float v = slider.GetComponent<Slider>().value;
                v += m_playSpeed;
                if (v >= 1)
                {
                    m_playSpeed = 0;
                    v = 0;
                }
//                Debug.Log("Playspeed after: " + m_playSpeed + " " + Time.time);
                slider.GetComponent<Slider>().value = v;

                Slide();
            }

        }
	}

}