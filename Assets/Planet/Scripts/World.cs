using UnityEngine;
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
		public static int maxQuadNodeLevel = 11;
		public static int minQuadNodeLevel = 2;
		public static double AU = 1.4960*Mathf.Pow(10,8); // AU in km
		public static float LOD_Distance = 100000;
		public static bool MoveCam = false;
		public static bool RenderText = false;
		public static float RingProbability = 0.5f;
		public static float RingRadiusRequirement = 4000;
		public static int CloudTextureSize = 1024;
		public static bool UseThreading = true;
		public static bool RenderMenu = true;
		public static float version = 0.09f;
		public static float MinCameraHeight = 0.05f;
		public static RenderType renderType = RenderType.Normal;
		public static string extraText = "";
		public static int ScreenshotX = 1680;
		public static int ScreenshotY = 1024;
		public static string ScreenshotName ="Image";
		public static bool ExitSaveOnRendered = false;
		public static float ResolutionScale = 1;
		public static bool isVideo = false;
		public static string generatingText = "Downloading data from satellite...";
	}
	
	public class Constants {
		public static string[] Clouds = new string[] {"earthclouds", "earthclouds2","gasclouds"}; 
		
	}
	
	
	
	public class SpaceAtmosphere {
		Material mat;
		GameObject sun;
		public Color color;
		public float m_g = -0.990f;				// The Mie phase asymmetry factor, must be between 0.999 to -0.999
		public float hdr = 0.1f;
		public SpaceAtmosphere(Material m, GameObject s, Color col, float h) {
			mat = m;
			sun = s;
			color = col;
			hdr = h;
		}
		
		
		public void Update()
		{
			mat.SetVector("v3LightPos",  sun.transform.forward*-1.0f);
			mat.SetColor ("sunColor", color);
			mat.SetFloat("fHdrExposure", hdr*Atmosphere.sunScale);
			mat.SetFloat("g", m_g);
			mat.SetFloat("g2", m_g*m_g);
			
		}
		
		
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
		private SpaceAtmosphere space;
		static public Material spaceMaterial;
		static public Material groundMaterial; 
		public List<Planet> planets = new List<Planet>();
		public static Planet planet;
		public static Stats stats = new Stats();
		// Use this for initialization
		public static GameObject canvas;
		public SerializedWorld szWorld;
		public static GameObject slider;
		
		public void ClickOverview() {
			if (RenderSettings.renderType == RenderType.Normal)
				RenderSettings.renderType = RenderType.Overview;
			else
				RenderSettings.renderType = RenderType.Normal;
		}
		
		
		public void LoadWorld(string data, bool isFile, bool ExitOnSave ) {
			ClearStarSystem();
			SerializedWorld sz;
			if (isFile) {
				//			RenderSettings.extraText = data;
				
				if (!System.IO.File.Exists(data)) {
					RenderSettings.extraText = ("ERROR: Could not find file :'" + data + "'");
					return;
				}
				sz = SerializedWorld.DeSerialize(data);
			}
			else
				sz = SerializedWorld.DeSerializeString(data);	 
				
			szWorld = sz;
			RenderSettings.ExitSaveOnRendered = ExitOnSave;
			RenderSettings.extraText = "";
			SetSkybox((int)sz.skybox);
			RenderSettings.sizeVBO = Mathf.Clamp(sz.resolution, 32, 128);
			RenderSettings.ScreenshotX = sz.screenshot_height;
			RenderSettings.ScreenshotY = sz.screenshot_width;
			RenderSettings.ResolutionScale = sz.resolutionScale;
			int cnt = 0;
			hasScene = true;
			RenderSettings.isVideo = sz.isVideo();
			if (RenderSettings.isVideo == true)
				RenderSettings.ExitSaveOnRendered = false;
			
			
			//		RenderSettings.isVideo = false;	
			slider.SetActive(RenderSettings.isVideo);	
			foreach (SerializedPlanet sp in sz.Planets) {
				//GameObject go = transform.GetChild(i).gameObject;
				GameObject go = new GameObject(sp.name);
				go.transform.parent = transform;
				//				go.transform.position = new Vector3((float)(sp.pos_x*RenderSettings.AU), (float)(sp.pos_y*RenderSettings.AU), (float)(sp.pos_z*RenderSettings.AU));
				//				Planet p = new Planet(sp.DeSerialize(go), go.GetComponent<CloudSettings>());
				Planet p = new Planet(sp.DeSerialize(go, cnt++,sz.global_radius_scale));
				p.Initialize(sun, (Material)Resources.Load("GroundMaterial"), (Material)Resources.Load ("SkyMaterial"), sphere);
				planets.Add (p);
			}
			
			PopulateOverviewList("Overview");
		}
		
		public void Slide() {
			float v = slider.GetComponent<Slider>().value;
			szWorld.getInterpolatedCamera(v, planets);
		}
		
		protected void FocusOnPlanet(string n) {
			GameObject gc = GameObject.Find ("CameraLOD");
			//Camera c = gc.GetComponent<Camera>();
			Planet planet = null;
			foreach (Planet p in planets)
				if (p.pSettings.name == n)
					planet = p; 
			
			if (planet == null)
				return;
			
			Vector3 pos = planet.pSettings.pos.toVectorf();
			float s = (float)(planet.pSettings.radius*szWorld.overview_distance/RenderSettings.AU);
			Vector3 dir = pos.normalized*s;
			Vector3 side = Vector3.Cross(Vector3.up, dir);
			
			pos = pos - dir - side.normalized*s;
			pos.y+=s;
			
			gc.GetComponent<SpaceCamera>().SetLookCamera(pos,  planet.pSettings.gameObject.transform.position, Vector3.up);
			UpdateWorldCamera();			
			Update();
			gc.GetComponent<SpaceCamera>().SetLookCamera(pos,  planet.pSettings.gameObject.transform.position, Vector3.up);
			UpdateWorldCamera();			
			
		}
		
		public static void MoveCamera(Vector3 dp) {
			GameObject gc = GameObject.Find ("CameraLOD");
			gc.GetComponent<SpaceCamera>().MoveCamera(dp);
			
		}
		
		
		protected void PopulateOverviewList(string box) {
			ComboBox cbx = GameObject.Find (box).GetComponent<ComboBox>();
			cbx.ClearItems();
			List<ComboBoxItem> l = new List<ComboBoxItem>();
			foreach (Planet p in planets)  {
				ComboBoxItem ci = new ComboBoxItem();
				ci.Caption = p.pSettings.name;
				string n = p.pSettings.name;
				ci.OnSelect = delegate {
					FocusOnPlanet(n);
				};
				l.Add (ci);
			}
			//		foreach (ComboBoxItem i in l)
			//			Debug.Log (i.Caption);
			
			cbx.AddItems(l.ToArray());
			
		}
		
		
		
		public void ClearStarSystem() {
			planets.Clear();
			for (int i=0;i<transform.childCount;i++) { 
				GameObject go = transform.GetChild(i).gameObject;
				GameObject.Destroy(go);
				//	Debug.Log ("Destroying " + go.name);
			}
			
			
		}
		
		public void InitializeFromScene() {
			
			for (int i=0;i<transform.childCount;i++) {
				GameObject go = transform.GetChild(i).gameObject;
				Planet p = new Planet(go.GetComponent<PlanetSettings>(), null);
				p.pSettings.pos.Set(go.transform.position);
				go.transform.parent = transform;
                p.pSettings.parent = go;
				p.pSettings.planetType = PlanetType.planetTypes[p.pSettings.planetTypeIndex];
//				p.pSettings.planetType = PlanetType.planetTypes[1];
				p.Initialize(sun, (Material)Resources.Load("GroundMaterial"), (Material)Resources.Load ("SkyMaterial"), sphere);
				planets.Add (p);
			}
			
			RenderSettings.ResolutionScale = szWorld.resolutionScale;
			
			space.color = new Color(szWorld.sun_col_r,szWorld.sun_col_g,szWorld.sun_col_b);
			space.hdr = szWorld.sun_intensity;
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
			LoadWorld(xml, false, false);
			szWorld.IterateCamera();
			space.color = new Color(szWorld.sun_col_r,szWorld.sun_col_g,szWorld.sun_col_b);
			space.hdr = szWorld.sun_intensity;
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
		
		public void LoadXmlFile() {
			string xml = GameObject.Find ("XMLText").GetComponent<Text>().text;
			GameObject.Find ("XMLText").GetComponent<Text>().text = " ";
			LoadWorld(xml, false,false);
			szWorld.IterateCamera();
			space.color = new Color(szWorld.sun_col_r,szWorld.sun_col_g,szWorld.sun_col_b);
			space.hdr = szWorld.sun_intensity;
		}
		#endif
		
		#if UNITY_STANDALONE_WIN
		public void LoadCommandLineXML() {
			
			string[] cmd = System.Environment.GetCommandLineArgs ();
			if (cmd.Length>1)  {
				if (cmd[1]!="")
					LoadWorld(Application.dataPath + "/../" + cmd[1], true, true);
			}
			
			//		LoadWorld("Assets/Planet/Resources/system1.xml", true);
			szWorld.IterateCamera();
			space.color = new Color(szWorld.sun_col_r,szWorld.sun_col_g,szWorld.sun_col_b);
			space.hdr = szWorld.sun_intensity;
			
		}
		
		#endif
		
		#if UNITY_STANDALONE_OSX
		public void LoadCommandLineXML() {
			
			
			System.IO.StreamWriter standardOutput = new System.IO.StreamWriter(System.Console.OpenStandardOutput());
			standardOutput.AutoFlush = true;
			System.Console.SetOut(standardOutput);
			
			string[] cmd = Util.GetOSXCommandParams();
			if (cmd.Length>1)  {
				if (cmd[1]!="")
					LoadWorld(Application.dataPath + "/../" + cmd[1], true, true);
			}
			
			//		LoadWorld("Assets/Planet/Resources/system1.xml", true);
			szWorld.IterateCamera();
			space.color = new Color(szWorld.sun_col_r,szWorld.sun_col_g,szWorld.sun_col_b);
			space.hdr = szWorld.sun_intensity;
			
		}
		
		#endif
		
		Texture2D tx_background, tx_load;
		int load_percent;
		bool hasScene = false;
		
		
		void OnGUI () {
			
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
		
		void Start () {
			
			canvas = GameObject.Find ("Canvas");
			spaceMaterial = (Material)Resources.Load("SpaceMaterial");	
			groundMaterial = (Material)Resources.Load("GroundMaterial");
            spaceBackground = GameObject.Find("SunBackgroundSphere");
//            spaceBackground.transform.localScale = Vector3.one*RenderSettings.LOD_Distance * 1.01f;

            RenderSettings.maxQuadNodeLevel = m_maxQuadNodeLevel;
			RenderSettings.sizeVBO = szWorld.resolution;
			RenderSettings.minQuadNodeLevel = m_minQuadNodeLevel;
			RenderSettings.MoveCam = true;
			SetSkybox((int)szWorld.skybox);
			
			slider = GameObject.Find ("Slider");
			if (slider!=null)
				slider.SetActive(false);
		
			
			
			
			
			//		CreateConfig("system1.xml");
			//		LoadWorld("system1.xml", true);
			//		szWorld.IterateCamera();
			PlanetType.Initialize();
			space = new SpaceAtmosphere(spaceMaterial, sun, Color.white, 0.1f);
			InitializeFromScene();
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
		
		void findClosestPlanet() {
			if (planets.Count>0)
				planet = planets[0];
			
			float min = 1E10f;
			foreach (Planet p in planets) {
				float l = (p.pSettings.gameObject.transform.position).magnitude;
				if (l<min) {
					planet = p;
					min = l;
				}
			}		
			
		}
		
		
		
		void setSun() {
			//		if (World.WorldCamera
			if (sun==null)
				return;
			sun.transform.rotation = Quaternion.FromToRotation(Vector3.forward, World.WorldCamera.toVectorf().normalized);
			sun.GetComponent<Light>().color = space.color;
		}
		
		// Update is called once per frame
		
        

		public void UpdateWorldCamera() {
			GameObject cam = GameObject.Find("CameraLOD");
			
			WorldCamera = cam.GetComponent<SpaceCamera>().getPos();//  cam.transform.position;
		}
		private bool modifier = false;
		private bool ctrlModifier = false;
		
		private void UpdateSlider() {
			
			
			
		}	
		
		void Update () {
			setSun();
			if (space!=null)			
				space.Update();
			
			
			
			GameObject cam = GameObject.Find("CameraLOD");
			SpaceCamera sc = cam.GetComponent<SpaceCamera>();

            GameObject lodCam = GameObject.Find("CameraNormal");
            lodCam.transform.rotation = cam.transform.rotation;



			UpdateWorldCamera();		
			//		Debug.Log (WorldCamera.toVectorf());
			//	sc.SetLookCamera(1.5f,Time.time,Vector3.up);
			
			findClosestPlanet();
			Camera c = GameObject.Find ("CameraLOD").GetComponent<Camera>();
			
			//Debug.Log (planet.pSettings.name);
			
			if (planet!=null)
				planet.ConstrainCameraExterior();
			
			if (Input.GetKey(KeyCode.Escape)) {
				Application.Quit();
			}
			float s = 0.35f;
			if (Input.GetKey (KeyCode.Alpha9))
				c.fieldOfView-=1*s;
			if (Input.GetKey (KeyCode.Alpha0))
				c.fieldOfView+=1*s;
			
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
			
			if (Input.GetKeyUp (KeyCode.Space)) {
				RenderSettings.RenderMenu = !RenderSettings.RenderMenu;
				canvas.SetActive(RenderSettings.RenderMenu);
			}
			
			//		ThreadQueue.SortQueue(WorldCamera);
			if (RenderSettings.UseThreading) 
				ThreadQueue.MaintainThreadQueue();
			
			if (RenderSettings.RenderMenu)
				Log();
			
			foreach (Planet p in planets)
				p.Update();	
			
			
//			Debug.Log (space.hdr);
			
			if (planet == null)
				return;		
			if (planet.pSettings.atmosphere!=null)
				planet.pSettings.atmosphere.setClippingPlanes();	
			
			
		}
		
		public void ExitSave() {
			SaveScreenshot();
			Application.Quit();
		}
		
		public static void SetSkybox(int s) {
			string skybox = "Skybox3";
            s = s % 6;

			if (s==1) skybox = "Skybox4";
			if (s==2) skybox = "Skybox5";
			if (s==3) skybox = "Skybox2";
            if (s == 4) skybox = "Skybox7";
            if (s == 5) skybox = "Skybox8";

            UnityEngine.RenderSettings.skybox = (Material)Resources.Load(skybox);
			
		}
		
		private int extraTimer = 10;
		
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