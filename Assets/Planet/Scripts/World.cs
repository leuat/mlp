using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine.EventSystems;
using UnityStandardAssets.ImageEffects;


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
        public static bool UseThreading = true;
        public static bool ignoreXMLResolution = true;
		public static int sizeVBO = 96;
		public static bool assProjection = true;
		public static bool flatShading = false;
		public static int maxQuadNodeLevel = 14;
		public static int minQuadNodeLevel = 3;
        public static bool createTerrainColliders = false;
		public static bool cullCamera = false;
		public static double AU = 1.4960*Mathf.Pow(10,8); // AU in km
		public static float LOD_Distance = 100000;
        public static float LOD_ProjectionDistance = 10000000;
        public static bool MoveCam = false;
		public static bool RenderText = false;
        public static bool logScale = false;
		public static int waterMaxQuadNodeLever = 3;
		public static float RingProbability = 0.5f;
		public static float RingRadiusRequirement = 4000;
		public static int CloudTextureSize = 1024;
		public static bool RenderMenu = true;
		public static bool GPUSurface = true;
		public static float version = 0.15f;
        public static float powScale = 0.75f;
        public static SerializedWorld currentSZWorld;
        public static bool debug = false;
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
        public static bool toggleClouds = true;
        public static bool toggleSaveVideo = false;
        public static bool toggleProgressbar = false;
        public static bool displayDebugLines = false;
        public static bool sortInverse = false;
        public static float GlobalRadiusScale = 1;
        public static string textureLocation = "Textures/EnvironmentBillboards/";

        public static bool usePointLightSource = false;

        public static bool reCalculateQuads = true;

        public static string path; 

        public static string planetTypesFilename = "planettypes.xml";

        public static int ForceAllPlanetTypes = -1;

        public static float maxAtmosphereDensity = 0.9f;

        public static bool RecordingVideo = false;

#if UNITY_STANDALONE_OSX
        public static string fileDelimiter = "/";
#endif 
#if UNITY_STANDALONE_LINUX
        public static string fileDelimiter = "/";
#endif 
#if UNITY_STANDALONE_WIN
        public static string fileDelimiter = "\\";
#endif 
#if UNITY_WEBGL
        public static string fileDelimiter = "\\";
#endif 
        public static string screenshotDir = "screenshots" + fileDelimiter;
        public static string movieDir = "movie" + fileDelimiter;
        public static string dataDir = "data" + fileDelimiter;
        public static string MCAstSettingsFile = "mcast_settings.xml";




    }

    public class Constants {
		public static string[] Clouds = new string[] {"earthclouds", "earthclouds2","gasclouds"}; 
		
	}




#if UNITY_EDITOR
    [CustomEditor(typeof(PlanetSettings))]
    public class SetPlanetType : Editor
    {

        string[] pList;
        int selectedPlanetType = 0;

        public SetPlanetType()
        {
            PlanetTypes.Initialize();
            pList = PlanetTypes.p.getStringList();

        }

        public override void OnInspectorGUI()
        {
            selectedPlanetType = EditorGUILayout.Popup("Set planet type:", selectedPlanetType, pList);
            PlanetSettings ps = (PlanetSettings)target;

            ps.planetType = PlanetTypes.p.planetTypes[selectedPlanetType];

            //          System.Random r = new System.Random(ps.seed);
            //            ps.planetType.setParameters(ps, r);


            if (GUILayout.Button("Randomize from seed"))
            {
                System.Random r = new System.Random(ps.seed);
                ps.planetType.Realize(r);
                ps.planetType.setParameters(ps, r);
                ps.planetTypeName = ps.planetType.name;

            }

            DrawDefaultInspector();


            EditorUtility.SetDirty(target);
        }

        [MenuItem("GameObject/LemonSpawn/Planet")]      
        static void CreatePlanet () {
            GameObject p = new GameObject("Planet");
            if (Selection.activeGameObject != null)
                p.transform.parent = Selection.activeGameObject.transform;
            PlanetSettings ps = p.AddComponent<PlanetSettings>();
            
     //       ps.cloudSettings = p.AddComponent<CloudSettings>();
            //      p.AddComponent<CloudSettings>();        
        }


    }
#endif




    public class World : MonoBehaviour {
		
		
		public static DVector WorldCamera = new DVector();
		public GameObject sun, spaceBackground;
		public Mesh sphere;
//		public int m_gridSize = 96;
		public int m_maxQuadNodeLevel = 11;
		public int m_minQuadNodeLevel = 2;

		public static string CurrentApp;

        public bool GPUSurface = false;

		public SerializedWorld szWorld;
        public GameObject mainCamera;
        public GameObject effectCamera;
        public bool initializeFromScene;
        private GameObject closeCamera;
        protected static GameObject FatalErrorPanel;

        public static Camera MainCamera;
        public static Camera CloseCamera;
        public static Stats stats = new Stats();
        public static GameObject MainCameraObject;
        public static SpaceCamera SpaceCamera;
        public static bool hasScene = false;
        public static SerializedWorld SzWorld;

        public bool followVehicle = false;
		protected int extraTimer = 10;
        
        public Vector3 vehiclePos, vehicleDir;

        protected SolarSystem solarSystem;
		protected bool modifier = false;
		protected bool ctrlModifier = false;

        public static void MoveCamera(Vector3 dp) {
            if (World.SzWorld.useSpaceCamera)
                SpaceCamera.MoveCamera(dp);

			
		}

        public virtual void OnGUI() {
			GUI.Label(new Rect(0, 0, 100, 100), "FPS: "+(int)(1.0f / Time.smoothDeltaTime)); 
        }

        private string GetScreenshotFilename(string dir, out string pureFile) {
			string OutputDir = RenderSettings.path + dir;
			DirectoryInfo info = new DirectoryInfo(OutputDir);
			if (!info.Exists) {
				FatalError("Could not find screen directory :" + dir);
				pureFile = "";
				return "";

			}
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
            string fname = OutputDir + RenderSettings.ScreenshotName + current.ToString("0000") + ".png";
            pureFile = RenderSettings.ScreenshotName + current.ToString("0000") + ".png";
            return fname;
			
			
			
		}

		public void setFieldOfView(float fov) {

            if (CloseCamera!=null)
			    CloseCamera.fieldOfView = fov;
            if (MainCamera!=null)
			    MainCamera.fieldOfView = fov;
            if (effectCamera!=null)
			    effectCamera.GetComponent<Camera>().fieldOfView = fov; 

		}

		
        protected string WriteScreenshot(string directory, int resWidth, int resHeight)
        {
        	
            Camera camera = MainCamera;
            Camera eCamera = effectCamera.GetComponent<Camera>();

            RenderTexture rt = new RenderTexture(resWidth, resHeight, 24);
            camera.targetTexture = rt;
            eCamera.targetTexture = rt;
            CloseCamera.targetTexture = rt;
            Texture2D screenShot = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);
            camera.Render();
            RenderTexture.active = rt;
            CloseCamera.Render();
            eCamera.Render();
            screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
            camera.targetTexture = null;
            eCamera.targetTexture = null;
            CloseCamera.targetTexture = null;
            RenderTexture.active = null; // JC: added to avoid errors

            Destroy(rt);
            byte[] bytes = screenShot.EncodeToPNG();
            string pureFile ;
            string file = GetScreenshotFilename(directory, out pureFile);
            if (file!="")
 	           File.WriteAllBytes(file, bytes);
            return pureFile;
        }

      
		
		//	#if UNITY_STANDALONE
		



		public void setWorld(SerializedWorld sz) {
			szWorld = sz;
		}

		public virtual void LoadFromXMLFile(string filename, bool randomizeSeeds = false) {

            ThreadQueue.AbortAll();
			string file = RenderSettings.path + filename;
			if (!System.IO.File.Exists(file)) {
				FatalError("Could not open file: " + file);
				return;
			}

			string xml = System.IO.File.ReadAllText(file);
			PlanetTypes.Initialize();

			solarSystem.LoadWorld(xml, false, false, this, randomizeSeeds);
			szWorld.IterateCamera();
			solarSystem.space.color = new Color(szWorld.sun_col_r,szWorld.sun_col_g,szWorld.sun_col_b);
            solarSystem.space.hdr = szWorld.sun_intensity;
          

        }

        //	#endif	
/*        void CreateConfig(string fname) {
			
			SerializedPlanet p = new SerializedPlanet();
			SerializedWorld sz = new SerializedWorld();
			sz.Planets.Add (p);
			
			SerializedCamera c = new SerializedCamera();
			c.cam_x = 0;
			c.cam_y = 0;
			c.cam_z = -20000;
			c.fov = 60;
			
			sz.Cameras.Add (c);
			
			
			SerializedWorld.Serialize(sz, fname);		
		}
		*/

		#if UNITY_STANDALONE
		
/*		public void LoadXmlFile(string filename) {
            solarSystem.LoadWorld(filename, false,false, this);
			szWorld.IterateCamera();
            solarSystem.space.color = new Color(szWorld.sun_col_r,szWorld.sun_col_g,szWorld.sun_col_b);
            solarSystem.space.hdr = szWorld.sun_intensity;
		}*/
		#endif
		
		#if UNITY_STANDALONE_WIN
		public void LoadCommandLineXML() {
			
/*			string[] cmd = System.Environment.GetCommandLineArgs ();
			if (cmd.Length>1)  {
				if (cmd[1]!="")
                    solarSystem.LoadWorld(RenderSettings.path + cmd[1], true, true, this);
			}
			
			//		LoadWorld("Assets/Planet/Resources/system1.xml", true);
			szWorld.IterateCamera();
			solarSystem.space.color = new Color(szWorld.sun_col_r,szWorld.sun_col_g,szWorld.sun_col_b);
            solarSystem.space.hdr = szWorld.sun_intensity;
	*/		
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
		

		

        protected void SetupCloseCamera()
        {
            MainCamera = mainCamera.GetComponent<Camera>();
            MainCameraObject = mainCamera;
            setFieldOfView(MainCamera.fieldOfView);

            if (!szWorld.useSpaceCamera)
                return;
            SpaceCamera = mainCamera.GetComponent<SpaceCamera>();
            closeCamera = new GameObject("CloseCamera");
            CloseCamera = closeCamera.AddComponent<Camera>();
            CloseCamera.clearFlags = CameraClearFlags.Depth;
            CloseCamera.nearClipPlane = 2;
            CloseCamera.farClipPlane = 220000;
            CloseCamera.cullingMask = 1 << LayerMask.NameToLayer("Normal") ;
			         
            MainCamera.farClipPlane = RenderSettings.LOD_ProjectionDistance * 1.1f;
            MainCamera.depthTextureMode = DepthTextureMode.Depth;
            CloseCamera.depthTextureMode = DepthTextureMode.Depth;

            setFieldOfView(MainCamera.fieldOfView);

            //   SunShafts ss = closeCamera.AddComponent<SunShafts>();
            //ss.sunShaftsShader = Shader.Find("SunShaftsShader");
            //            ss.CheckResources();
            // ss.sunShaftsShader = Shader.Find("SunShaftsComposite");
        }

        public static void FatalError(string errorMessage) {
            Debug.Log(errorMessage);
            Application.Quit();
        }


        protected void StartBasics() {
            RenderSettings.path = Application.dataPath + "/../";
            CurrentApp = Verification.MCAstName;

            RenderSettings.GPUSurface = GPUSurface;

        if (solarSystem == null)
            solarSystem = new SolarSystem(sun, sphere, transform, (int)szWorld.skybox);

            SzWorld = szWorld;
            PlanetTypes.Initialize();
            spaceBackground = GameObject.Find("SunBackgroundSphere");
            //            spaceBackground.transform.localScale = Vector3.one*RenderSettings.LOD_Distance * 1.01f;

            //CloseCamera = closeCamera.GetComponent<Camera>();
            SetupCloseCamera();


            RenderSettings.maxQuadNodeLevel = m_maxQuadNodeLevel;
            RenderSettings.sizeVBO = szWorld.resolution;
            RenderSettings.minQuadNodeLevel = m_minQuadNodeLevel;
            RenderSettings.MoveCam = true;
            RenderSettings.ResolutionScale = szWorld.resolutionScale;
            



            PlanetTypes.Initialize();
            if (initializeFromScene)
                solarSystem.InitializeFromScene();
            Application.runInBackground = true;
           

        }

        public virtual void Start () {
            StartBasics();
   		}
		


        public void UpdateWorldCamera() {
			
			WorldCamera = mainCamera.GetComponent<SpaceCamera>().getPos();//  cam.transform.position;
			closeCamera.transform.rotation = mainCamera.transform.rotation;
            effectCamera.transform.rotation = mainCamera.transform.rotation;

		}
		private void UpdateSlider() {
			
			
			
		}	


        public void FatalQuit() {
        	Application.Quit();
        }	

        public void Collect()
        {
            if (Time.frameCount % 300 == 299)
            {
                System.GC.Collect();
               // Debug.Log("Collect at frame " + Time.frameCount);
              
                Resources.UnloadUnusedAssets();
             
            }
        }

        public virtual void Update () {

			UpdateWorldCamera();		
            solarSystem.Update();
            Collect();

            if (Input.GetKey(KeyCode.Escape)) {
				FatalQuit();
			}

            if (Input.GetKeyDown (KeyCode.LeftShift)) 
				modifier = true;
			if (Input.GetKeyUp (KeyCode.LeftShift)) 
				modifier = false;
			if (Input.GetKeyDown (KeyCode.LeftControl)) 
				ctrlModifier = true;
			if (Input.GetKeyUp (KeyCode.LeftControl)) 
				ctrlModifier = false;


            if (RenderSettings.debug) {

                if (Input.GetKeyUp(KeyCode.F11))
                    solarSystem.toggleGPUSurface();

				if (Input.GetKeyUp(KeyCode.F10))
					RenderSettings.MoveCam = !RenderSettings.MoveCam;

			}


            if (SolarSystem.planet!=null)
	    		ThreadQueue.SortQueue(SolarSystem.planet.pSettings.properties.localCamera);

    		if (RenderSettings.UseThreading) 
				ThreadQueue.MaintainThreadQueue();
			
			if (RenderSettings.RenderMenu)
				Log();




		}


        protected virtual void Log() {

        }




		
		


		
	}
	
}