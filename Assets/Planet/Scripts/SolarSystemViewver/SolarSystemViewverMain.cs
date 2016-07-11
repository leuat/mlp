using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace LemonSpawn {

	public class SSVSettings {
		public static float SolarSystemScale = 500.0f;
		public static float PlanetSizeScale = 1.0f / 100.0f;
		public static int OrbitalLineSegments = 100;
		public static Vector2 OrbitalLineWidth = new Vector2 (3.03f, 3.03f);
        public static float currentFrame = 0;
	}

	public class DisplayPlanet {
		public Planet planet;
        public SerializedPlanet serializedPlanet;
		public GameObject go;
		public List<GameObject> orbitLines = new List<GameObject>();

/*		private void CreateOrbitCircles() {
			float radius = (float)planet.pSettings.properties.pos.Length () * SSVSettings.SolarSystemScale;
			for (int i = 0; i < SSVSettings.OrbitalLineSegments; i++) {
				float t0 = 2 * Mathf.PI / (float)SSVSettings.OrbitalLineSegments * (float)i;
				float t1 = 2 * Mathf.PI / (float)SSVSettings.OrbitalLineSegments * (float)(i+1);
				Vector3 from = new Vector3 (Mathf.Cos (t0), 0, Mathf.Sin (t0)) * radius;
				Vector3 to = new Vector3 (Mathf.Cos (t1), 0, Mathf.Sin (t1)) * radius;
			
				GameObject g = new GameObject ();
				g.transform.parent = SolarSystemViewverMain.linesObject.transform;
				LineRenderer lr = g.AddComponent<LineRenderer> ();
				lr.material = (Material)Resources.Load ("LineMaterial");
				lr.SetWidth (SSVSettings.OrbitalLineWidth.x, SSVSettings.OrbitalLineWidth.y);
				lr.SetPosition (0, from);
				lr.SetPosition (1, to);
				orbitLines.Add (g);
			}
		}
*/

        public void MaintainOrbits() {
            int maxFrames = serializedPlanet.Frames.Count;
            int currentFrame = (int)(SSVSettings.currentFrame*maxFrames);
            Color c = new Color(0.3f, 0.7f, 1.0f,1.0f);
            for (int i=0;i<orbitLines.Count;i++) {
                int f = Mathf.Clamp(i - orbitLines.Count/2 + currentFrame,0,maxFrames);
                LineRenderer lr = orbitLines[i].GetComponent<LineRenderer>();
                Frame sp = serializedPlanet.Frames[f];
                Frame sp2 = serializedPlanet.Frames[f+1];
                DVector from = new DVector (sp.pos_x, sp.pos_y,sp.pos_z) * SSVSettings.SolarSystemScale;
                DVector to = new DVector (sp2.pos_x, sp2.pos_y,sp2.pos_z) * SSVSettings.SolarSystemScale;


                lr.SetPosition (0, from.toVectorf());
                lr.SetPosition (1, to.toVectorf());

                float colorScale = Mathf.Abs(i-orbitLines.Count/2)/(float)orbitLines.Count*2;
                Color col = c*(1-colorScale);
                col.a = 1;
                lr.SetColors(col,col);
            }
        }

        public void CreateOrbitFromFrames(int maxLines) {
            for (int i = 0; i < maxLines; i++) {
                Frame sp = serializedPlanet.Frames[i];
                Frame sp2 = serializedPlanet.Frames[i+1];
                DVector from = new DVector (sp.pos_x, sp.pos_y,sp.pos_z) * SSVSettings.SolarSystemScale;
                DVector to = new DVector (sp2.pos_x, sp2.pos_y,sp2.pos_z) * SSVSettings.SolarSystemScale;
            
                GameObject g = new GameObject ();
                g.transform.parent = SolarSystemViewverMain.linesObject.transform;
                LineRenderer lr = g.AddComponent<LineRenderer> ();
                lr.material = new Material(Shader.Find("Particles/Additive"));//(Material)Resources.Load ("LineMaterial");
                lr.SetWidth (SSVSettings.OrbitalLineWidth.x, SSVSettings.OrbitalLineWidth.y);
                lr.SetPosition (0, from.toVectorf());
                lr.SetPosition (1, to.toVectorf());
                orbitLines.Add (g);
            }
        }

		public DisplayPlanet(GameObject g, Planet p, SerializedPlanet sp) {
			go = g;
			planet = p;
            serializedPlanet = sp;

			//CreateOrbitFromFrames ();
		}

        public void UpdatePosition() {
            planet.pSettings.properties.pos*=SSVSettings.SolarSystemScale;
            planet.pSettings.transform.position = planet.pSettings.properties.pos.toVectorf();
            go.transform.position = planet.pSettings.properties.pos.toVectorf();
            MaintainOrbits();
        }

	}

	public class SolarSystemViewverMain : World {
		private List<DisplayPlanet> dPlanets = new List<DisplayPlanet>();
		private Vector3 mouseAccel = new Vector3();
		private Vector3 focusPoint = Vector3.zero;
		private Vector3 focusPointCur = Vector3.zero;
		private DisplayPlanet selected = null;
        public static GameObject linesObject = null;
        private float m_playSpeed = 0;

		private void SelectPlanet(DisplayPlanet dp) {
			selected = dp;
			focusPoint = dp.go.transform.position;
		}

		private void UpdateFocus() {
			if (Input.GetMouseButtonDown (0)) {
				RaycastHit hit;
				Ray ray = MainCamera.ScreenPointToRay (Input.mousePosition);
				if (Physics.Raycast (ray, out hit)) {
					foreach (DisplayPlanet dp in dPlanets) {
						if (dp.go == hit.transform.gameObject)
							SelectPlanet(dp);
					}
				}
                else {
                    //selected = null;
                    //focusPoint = Vector3.zero;
                }
			}
		}

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
			DestroyAllGameObjects();
			dPlanets.Clear ();

            //solarSystem.InitializeFromScene();


            int i=0;
			foreach (Planet p in solarSystem.planets) {

                GameObject go = p.pSettings.gameObject;

				Vector3 coolpos = new Vector3 ((float)p.pSettings.properties.pos.x, (float)p.pSettings.properties.pos.y, (float)p.pSettings.properties.pos.z);
				go.transform.position = coolpos * SSVSettings.SolarSystemScale;
                p.pSettings.properties.pos = new DVector(coolpos);
				//go.transform.localScale = Vector3.one * SSVSettings.PlanetSizeScale * p.pSettings.radius;
               //p.pSettings.atmosphereDensity = 0;

                GameObject hidden = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                hidden.transform.position = coolpos * SSVSettings.SolarSystemScale;
                hidden.transform.localScale = Vector3.one * p.pSettings.radius;
                //Destroy(hidden.GetComponent<MeshRenderer>());

				dPlanets.Add (new DisplayPlanet (hidden, p,szWorld.Planets[i++]));
			}
		}


        public void CreateFakeOrbits(int steps, float stepLength) {
            foreach (SerializedPlanet sp in szWorld.Planets) {
                int frame = 0;

                float t0 = Random.value*2*Mathf.PI;
                float radius = new Vector3((float)sp.pos_x,(float)sp.pos_y, (float)sp.pos_z).magnitude;
                float modifiedStepLength = stepLength / Mathf.Sqrt(radius);
                float rot = Random.value*30f + 10f;
                    for (int i = 0;i<steps;i++) {
                        float perturb = Mathf.Cos(i/(float)steps*30.234f);
                        float rad = radius*(0.2f*perturb +1);
                        Vector3 pos = new Vector3 (Mathf.Cos (t0), 0, Mathf.Sin (t0)) * rad;
                        Frame f = new Frame();
                        f.pos_x = pos.x;
                        f.pos_y = pos.y;
                        f.pos_z = pos.z;
                        f.rotation = frame/rot;
                        f.id = frame;
                        sp.Frames.Add(f);
                        frame++;
                        t0+=modifiedStepLength;
                   }
            }
        }


		public override void Start () { 
			CurrentApp = Verification.MCAstName;
            RenderSettings.UseThreading = true;
            RenderSettings.reCalculateQuads = false;
            RenderSettings.GlobalRadiusScale = SSVSettings.PlanetSizeScale;
            RenderSettings.maxQuadNodeLevel = m_maxQuadNodeLevel;
            RenderSettings.sizeVBO = szWorld.resolution;
            RenderSettings.minQuadNodeLevel = m_minQuadNodeLevel;
            RenderSettings.MoveCam = false;
            RenderSettings.ResolutionScale = szWorld.resolutionScale;
            RenderSettings.usePointLightSource = true;
			solarSystem = new SolarSystem(sun, sphere, transform, (int)szWorld.skybox);
			PlanetTypes.Initialize ();
            SetupCloseCamera();
			MainCamera = mainCamera.GetComponent<Camera> ();
			PopulateFileCombobox("ComboBoxLoadFile","xml");
			SzWorld = szWorld;
            slider = GameObject.Find ("Slider");

            linesObject = new GameObject("Lines");

//			LoadData ();
		}
	
		public override void Update () {
			UpdateFocus ();
			UpdateCamera ();
            solarSystem.Update();
            if (RenderSettings.UseThreading) 
                ThreadQueue.MaintainThreadQueue();

            // Always point to selected planet
            if (selected!=null)
                SelectPlanet(selected);

            UpdatePlay();
            
		}

		protected void OnGUI() {
		}

		public void LoadFileFromMenu()
        {
            int idx = GameObject.Find("ComboBoxLoadFile").GetComponent<UnityEngine.UI.Dropdown>().value;
            string name = GameObject.Find("ComboBoxLoadFile").GetComponent<Dropdown>().options[idx].text;
           	if (name=="-")
           		return;
			name =RenderSettings.dataDir + name + ".xml";
            
            LoadFromXMLFile(name);
            CreateFakeOrbits(2000, 0.05f);


            szWorld.useSpaceCamera = false;
	        PopulateOverviewList("Overview");
			PopulateWorld ();
            foreach (DisplayPlanet dp in dPlanets)
                dp.CreateOrbitFromFrames(100);

            Slide();
        }

        private void DestroyAllGameObjects() {
        	foreach (DisplayPlanet dp in dPlanets)
        		GameObject.Destroy(dp.go);
        }


        public void Slide()
        {
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