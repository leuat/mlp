﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine.EventSystems;


#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;

namespace LemonSpawn {
	
	
	public class Stats {
		
		public float Velocity;
		public float Height;
		public float scale = 1000;
		
	}
	public enum RenderType { Normal, Overview }
	
	public class RenderSettings {
		public static float gridDivide = 10;
		public static int sizeVBO = 96;
		public static bool assProjection = true;
		public static bool flatShading = false;
		public static int maxQuadNodeLevel = 14;
		public static int minQuadNodeLevel = 2;
		public static bool cullCamera = false;
		public static double AU = 1.4960*Mathf.Pow(10,8); // AU in km
		public static float LOD_Distance = 100000;
        public static float LOD_ProjectionDistance = 10000000;
        public static bool MoveCam = false;
		public static bool RenderText = false;
		public static int waterMaxQuadNodeLever = 3;
		public static float RingProbability = 0.5f;
		public static float RingRadiusRequirement = 4000;
		public static int CloudTextureSize = 1024;
		public static bool UseThreading = true;
		public static bool RenderMenu = true;
		public static float version = 0.1f;
		public static float MinCameraHeight = 1.5f;
		public static RenderType renderType = RenderType.Normal;
		public static string extraText = "";
		public static int ScreenshotX = 1680;
		public static int ScreenshotY = 1024;
		public static string ScreenshotName ="Image";
		public static bool ExitSaveOnRendered = false;
		public static float ResolutionScale = 1;
		public static bool isVideo = false;
		public static string generatingText = "Downloading data from satellite...";
        public static float vehicleFollowHeight = 10;
        public static float vehicleFollowDistance = 10;
        public static bool toggleClouds = false;

    }

    public class Constants {
		public static string[] Clouds = new string[] {"earthclouds", "earthclouds2","gasclouds"}; 
		
	}
	
	
	
	
	#if UNITY_EDITOR
	
	[CustomEditor(typeof(PlanetSettings))]
	public class ObjectBuilderEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();
			
			PlanetSettings ps = (PlanetSettings)target;
			if(GUILayout.Button("Build Object"))
			{
				//myScript.BuildObject();
				Planet p = new Planet(ps, null);
				//p.pSettings.pos.Set(go.transform.position);
				//go.transform.parent = transform;
				PlanetType.Initialize();
//				p.pSettings.planetType = PlanetType.planetTypes[PlanetType.planetTypes.Count-1];
				p.pSettings.planetType = PlanetType.planetTypes[1];
				p.Initialize(GameObject.Find ("Sun"), (Material)Resources.Load("GroundMaterial"), (Material)Resources.Load ("SkyMaterial"), (Mesh)Resources.Load("Sphere01"));
				p.Update();
				
			}
		}
	}
	# endif	
	
	public class World : MonoBehaviour {
		
		
		public static DVector WorldCamera = new DVector();
		public GameObject sun, spaceBackground;
		public Mesh sphere;
//		public int m_gridSize = 96;
		public int m_maxQuadNodeLevel = 11;
		public int m_minQuadNodeLevel = 2;


		public static GameObject canvas;
		public SerializedWorld szWorld;
        public GameObject mainCamera;
        public GameObject effectCamera;
        public bool initializeFromScene;
        private GameObject closeCamera;

        public static Camera MainCamera;
        public static Camera CloseCamera;
        public static Stats stats = new Stats();
        public static GameObject MainCameraObject;
        public static GameObject slider;
        public static SpaceCamera SpaceCamera;
        public static bool hasScene = false;
        public static SerializedWorld SzWorld;


		protected Texture2D tx_background, tx_load;
		protected int load_percent;

        public bool followVehicle = false;
		protected int extraTimer = 10;


        public Vector3 vehiclePos, vehicleDir;

        protected SolarSystem solarSystem;
		private bool modifier = false;
		private bool ctrlModifier = false;
		




        public void ClickOverview() {
			if (RenderSettings.renderType == RenderType.Normal)
				RenderSettings.renderType = RenderType.Overview;
			else
				RenderSettings.renderType = RenderType.Normal;
		}
		
		
		
		public void Slide() {
			float v = slider.GetComponent<Slider>().value;
			szWorld.getInterpolatedCamera(v, solarSystem.planets);
		}
		
		
		
		public static void MoveCamera(Vector3 dp) {
            SpaceCamera.MoveCamera(dp);
			
		}

		
		public static void addBall()
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.transform.parent = SolarSystem.planet.pSettings.transform;
            go.transform.localPosition = SolarSystem.planet.pSettings.localCamera + World.MainCamera.transform.forward * 2;
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
            go.transform.parent = SolarSystem.planet.pSettings.transform;
            go.transform.localPosition = SolarSystem.planet.pSettings.localCamera + World.MainCamera.transform.forward * 2;
            go.transform.rotation = Quaternion.FromToRotation(Vector3.up, SolarSystem.planet.pSettings.transform.position*-1);


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




        private string GetScreenshotFilename() {
			string OutputDir = Application.dataPath + "/../";
			DirectoryInfo info = new DirectoryInfo(OutputDir);
			FileInfo[] fileInfo = info.GetFiles();
			int current = 0;
			foreach (FileInfo f in fileInfo)  {
				if (f.Name.Contains(RenderSettings.ScreenshotName)) {
					string name = f.Name.Remove(f.Name.Length-4, 4);
					Regex rgx = new Regex("[^0-9 -]");
					name = rgx.Replace(name, "");
					int next;			
					if (int.TryParse(name, out next))
						current=next;
				}
			}
			current++;
			return OutputDir  + RenderSettings.ScreenshotName + current.ToString("0000") + ".png";
			
			
			
		}

		public void setFieldOfView(float fov) {
			CloseCamera.fieldOfView = fov;
			MainCamera.fieldOfView = fov;
			effectCamera.GetComponent<Camera>().fieldOfView = fov; 

		}

		
		public void SaveScreenshot() {
			Camera camera = GameObject.Find ("Camera").GetComponent<Camera>();
			int resWidth = RenderSettings.ScreenshotX;
			int resHeight = RenderSettings.ScreenshotY;
			
			RenderTexture rt = new RenderTexture(resWidth, resHeight, 24);
			camera.targetTexture = rt;
			Texture2D screenShot = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);
			camera.Render();
			RenderTexture.active = rt;
			screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
			camera.targetTexture = null;
			RenderTexture.active = null; // JC: added to avoid errors
			Destroy(rt);
			byte[] bytes = screenShot.EncodeToPNG();
			
			string file = GetScreenshotFilename();
			
			File.WriteAllBytes( file , bytes);
			
		}
		
		//	#if UNITY_STANDALONE
		



		public void setWorld(SerializedWorld sz) {
			szWorld = sz;
		}

		public void LoadFromFile() {
			if (ThreadQueue.currentThreads.Count != 0 || ThreadQueue.threadQueue.Count!=0)
				return;
			//string xml =  ((TextAsset)Resources.Load ("system1")).text;// //System.IO.File.ReadAllText("system1.xml");
			string file = Application.dataPath + "/../" + GameObject.Find("InputFile").GetComponent<InputField>().text.Trim();
			//		RenderSettings.extraText = file;
			if (!System.IO.File.Exists(file)) {
				RenderSettings.extraText = ("ERROR: Could not find file :'" + file + "'");
				return;
			}
			
			string xml = System.IO.File.ReadAllText(file);
			//			RenderSettings.extraText += "\n" + xml;
			//		Debug.Log (xml);
			solarSystem.LoadWorld(xml, false, false, this);
			szWorld.IterateCamera();
			solarSystem.space.color = new Color(szWorld.sun_col_r,szWorld.sun_col_g,szWorld.sun_col_b);
            solarSystem.space.hdr = szWorld.sun_intensity;
		}
		
		//	#endif	
		void CreateConfig(string fname) {
			
			SerializedPlanet p = new SerializedPlanet();
			SerializedWorld sz = new SerializedWorld();
			sz.Planets.Add (p);
			
			SerializedCamera c = new SerializedCamera();
			c.cam_x = 0;
			c.cam_y = 0;
			c.cam_z = -20000;
			/*		c.rot_x = 0;
		c.rot_y = 0;
		c.rot_z = 0;*/
			c.fov = 60;
			
			sz.Cameras.Add (c);
			
			
			SerializedWorld.Serialize(sz, fname);		
		}
		
		/*	public void LoadDirectXml() {
		string xml = GameObject.Find ("XMLText").GetComponent<Text>().text;
		
		GameObject.Find ("XMLText").GetComponent<Text>().text = " ";
//		Debug.Log (xml);
		LoadWorld(xml, false);
		szWorld.IterateCamera();
		space.color = new Color(szWorld.sun_col_r,szWorld.sun_col_g,szWorld.sun_col_b);
		space.hdr = szWorld.sun_intensity;
	}
*/	
		
		#if UNITY_STANDALONE
		
		public void LoadXmlFile(string filename) {
            solarSystem.LoadWorld(filename, false,false, this);
			szWorld.IterateCamera();
            solarSystem.space.color = new Color(szWorld.sun_col_r,szWorld.sun_col_g,szWorld.sun_col_b);
            solarSystem.space.hdr = szWorld.sun_intensity;
		}
		#endif
		
		#if UNITY_STANDALONE_WIN
		public void LoadCommandLineXML() {
			
			string[] cmd = System.Environment.GetCommandLineArgs ();
			if (cmd.Length>1)  {
				if (cmd[1]!="")
                    solarSystem.LoadWorld(Application.dataPath + "/../" + cmd[1], true, true, this);
			}
			
			//		LoadWorld("Assets/Planet/Resources/system1.xml", true);
			szWorld.IterateCamera();
			solarSystem.space.color = new Color(szWorld.sun_col_r,szWorld.sun_col_g,szWorld.sun_col_b);
            solarSystem.space.hdr = szWorld.sun_intensity;
			
		}
		
		#endif
		
		#if UNITY_STANDALONE_OSX
/*		public void LoadCommandLineXML() {
			
			
			System.IO.StreamWriter standardOutput = new System.IO.StreamWriter(System.Console.OpenStandardOutput());
			standardOutput.AutoFlush = true;
			System.Console.SetOut(standardOutput);
			
			string--[] cmd = Util.GetOSXCommandParams();
			if (cmd.Length>1)  {
				if (cmd[1]!="")
					LoadWorld(Application.dataPath + "/../" + cmd[1], true, true);
			}
			
			//		LoadWorld("Assets/Planet/Resources/system1.xml", true);
			szWorld.IterateCamera();
			space.color = new Color(szWorld.sun_col_r,szWorld.sun_col_g,szWorld.sun_col_b);
			space.hdr = szWorld.sun_intensity;
			
		}
*/		
		#endif
		

		
		protected virtual void OnGUI () {
			
			//	return;
			
			if (RenderSettings.isVideo)
				return;
			if (tx_background == null) {
				tx_background = new Texture2D(1,1);
				tx_load = new Texture2D(1,1);
				tx_background.SetPixel(0,0,new Color(0,0,0,1));
				tx_background.Apply();
				tx_load.SetPixel(0,0,new Color(0.7f,0.3f,0.2f,1));
				tx_load.Apply();
				//			tx_background = (Texture2D)Resources.Load ("cloudsTexture");
				
			}
			if (!hasScene) 
				return;
			if (load_percent==100)
				return;
			
			//	return;
			GUI.DrawTexture( new Rect(0,0,Screen.width, Screen.height), tx_background);
			float h = 0.08f;
			int hei = (int)(Screen.height*h);
			float border = 0.05f;
			int rectwidth = (int)(Screen.width*(1- 2*border));
			GUI.DrawTexture(new Rect(Screen.width*border,Screen.height/2 - hei,   (int)(rectwidth/100f*load_percent), 2*hei), tx_load);
			GUI.Label(new Rect(Screen.width/2 - 40,(int)(Screen.height*(2/3f)), 200,200 ), RenderSettings.generatingText);	
		}
		

        void SetupCloseCamera()
        {
            MainCamera = mainCamera.GetComponent<Camera>();
            MainCameraObject = mainCamera;
            SpaceCamera = mainCamera.GetComponent<SpaceCamera>();
            closeCamera = new GameObject("CloseCamera");
            CloseCamera = closeCamera.AddComponent<Camera>();
            CloseCamera.clearFlags = CameraClearFlags.Depth;
            CloseCamera.nearClipPlane = 2;
            CloseCamera.farClipPlane = 220000;
            CloseCamera.cullingMask = 1 << LayerMask.NameToLayer("Normal");
			setFieldOfView(MainCamera.fieldOfView);

        }


        public virtual void Start () {

            solarSystem = new SolarSystem(sun, sphere, transform, (int)szWorld.skybox);
            SzWorld = szWorld;

			canvas = GameObject.Find ("Canvas");
            spaceBackground = GameObject.Find("SunBackgroundSphere");
            //            spaceBackground.transform.localScale = Vector3.one*RenderSettings.LOD_Distance * 1.01f;

            //CloseCamera = closeCamera.GetComponent<Camera>();
            SetupCloseCamera();


            RenderSettings.maxQuadNodeLevel = m_maxQuadNodeLevel;
			RenderSettings.sizeVBO = szWorld.resolution;
			RenderSettings.minQuadNodeLevel = m_minQuadNodeLevel;
			RenderSettings.MoveCam = true;
			
			slider = GameObject.Find ("Slider");
			if (slider!=null)
				slider.SetActive(false);

            MainCamera.farClipPlane = RenderSettings.LOD_ProjectionDistance*1.1f;			
            MainCamera.depthTextureMode = DepthTextureMode.Depth;



            //		CreateConfig("system1.xml");
            //		LoadWorld("system1.xml", true);
            //		szWorld.IterateCamera();
            PlanetType.Initialize();
			if (initializeFromScene)
				solarSystem.InitializeFromScene();
			Application.runInBackground = true;
			
		}
		
		#if UNITY_EDITOR
		[MenuItem("GameObject/LemonSpawn/Planet")]		
		static void CreatePlanet () {
			GameObject p = new GameObject("Planet");
			if (Selection.activeGameObject != null)
				p.transform.parent = Selection.activeGameObject.transform;
			p.AddComponent<PlanetSettings>();		
			//		p.AddComponent<CloudSettings>();		
		}
		#endif	
		
		
		
		
		
		// Update is called once per frame
		
        

		public void UpdateWorldCamera() {
			
			WorldCamera = mainCamera.GetComponent<SpaceCamera>().getPos();//  cam.transform.position;
			closeCamera.transform.rotation = mainCamera.transform.rotation;
            effectCamera.transform.rotation = mainCamera.transform.rotation;

		}
		private void UpdateSlider() {
			
			
			
		}	
		
		public virtual void Update () {
			UpdateWorldCamera();		
            solarSystem.Update();			
			

			//		Debug.Log (WorldCamera.toVectorf());
			//	sc.SetLookCamera(1.5f,Time.time,Vector3.up);
			
			
			//Debug.Log (planet.pSettings.name);

			if (Input.GetKey(KeyCode.Escape)) {
				Application.Quit();
			}
			float s = 0.35f;
			if (Input.GetKey (KeyCode.Alpha9)) { 
				//MainCamera.fieldOfView-=1*s;
            }
            if (Input.GetKey(KeyCode.Alpha0))
            {
                //MainCamera.fieldOfView += 1 * s;
            }

            if (Input.GetKeyUp(KeyCode.B))
                addBall();
            if (Input.GetKeyUp(KeyCode.V))
                addWheels();

            if (Input.GetKeyDown (KeyCode.LeftShift)) 
				modifier = true;
			if (Input.GetKeyUp (KeyCode.LeftShift)) 
				modifier = false;
			if (Input.GetKeyDown (KeyCode.LeftControl)) 
				ctrlModifier = true;
			if (Input.GetKeyUp (KeyCode.LeftControl)) 
				ctrlModifier = false;
			if (modifier) // && ctrlModifier)
			{
				if (Input.GetKeyUp(KeyCode.Alpha1))
					RenderSettings.MoveCam = !RenderSettings.MoveCam;
				
				if (Input.GetKeyUp(KeyCode.Alpha2))
					RenderSettings.RenderText = !RenderSettings.RenderText;
			}

            if (Input.GetKeyUp(KeyCode.Tab))
                followVehicle = !followVehicle;


			if (Input.GetKeyUp (KeyCode.Space)) {
				RenderSettings.RenderMenu = !RenderSettings.RenderMenu;
				canvas.SetActive(RenderSettings.RenderMenu);
			}
			if (Input.GetKeyUp (KeyCode.L)) {
				RenderSettings.toggleClouds = !RenderSettings.toggleClouds;
			}
			
            if (SolarSystem.planet!=null)
	    		ThreadQueue.SortQueue(SolarSystem.planet.pSettings.localCamera);
    		if (RenderSettings.UseThreading) 
				ThreadQueue.MaintainThreadQueue();
			
			if (RenderSettings.RenderMenu)
				Log();


            if (followVehicle)
                FollowVehicle("car_root");
			
		}
		
        private void FollowVehicle(string s)
        {
            GameObject go = GameObject.Find(s);
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

        public void ExitSave() {
			SaveScreenshot();
			Application.Quit();
		}
		
		

		void Log() {
			string s = "";
			float val = 1;
			if (ThreadQueue.orgThreads!=0)
				val = (ThreadQueue.threadQueue.Count / (float)ThreadQueue.orgThreads);
			
			int percent = 100-(int)(100 * val);
			
			
			if (percent == 100 && RenderSettings.ExitSaveOnRendered && ThreadQueue.currentThreads.Count==0) {
				if (extraTimer--==0)
					ExitSave();
			}
			load_percent = percent;
			
			s+="Version: " + RenderSettings.version.ToString("0.00")  + " \n";
			//if (RenderSettings.isVideo)
	//			s+="Progress: " + percent + " %\n";
			s+="Progress: " + ThreadQueue.threadQueue.Count + " \n";
			s+="Height: " + stats.Height.ToString("0.00") + " km \n";
			//s+="Velocity: " + stats.Velocity.ToString("0.00") + " km/s\n";
			s+=RenderSettings.extraText;
			GameObject info = GameObject.Find ("Logging");
			if (info!=null)
				info.GetComponent<Text>().text = s;
		}
		
		
	}
	
}