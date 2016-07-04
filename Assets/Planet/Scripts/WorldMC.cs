using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine.EventSystems;
using System.Xml.Serialization;


#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;

namespace LemonSpawn
{

   

    public class WorldMC : World
    {


        private float m_playSpeed = 0;
        protected Texture2D tx_background, tx_load, tx_record;
        protected int load_percent;
        protected GameObject helpPanel = null;
        protected GameObject settingsPanel = null;
        public GameObject debugPanel = null;
        protected MCAstSettings settings = new MCAstSettings();

        protected void PopulateGUISettings()
        {
            GameObject.Find("ScreenshotResolutionCmb").GetComponent<Dropdown>().value = settings.screenShotResolution;
            GameObject.Find("MovieResolutionCmb").GetComponent<Dropdown>().value = settings.movieResolution;
            GameObject.Find("GridSizeCmb").GetComponent<Dropdown>().value = settings.gridSize;
            GameObject.Find("ToggleCameraEffects").GetComponent<Toggle>().isOn = settings.cameraEffects;

        }

        protected void PopulateSettingsFromGUI()
        {
            settings.screenShotResolution = GameObject.Find("ScreenshotResolutionCmb").GetComponent<Dropdown>().value;
            settings.movieResolution = GameObject.Find("MovieResolutionCmb").GetComponent<Dropdown>().value;
            settings.gridSize = GameObject.Find("GridSizeCmb").GetComponent<Dropdown>().value;
            settings.cameraEffects = GameObject.Find("ToggleCameraEffects").GetComponent<Toggle>().isOn;
            int actualGridSize = MCAstSettings.GridSizes[ settings.gridSize ];
            if (actualGridSize != RenderSettings.sizeVBO)
            {
                RenderSettings.sizeVBO = actualGridSize;
                solarSystem.Reset();
                AddMessage("New gridsize: Solar system reset");

            }
            effectCamera.GetComponent<Camera>().enabled = settings.cameraEffects;
        }

        protected void LoadSettings()
        {
            string fname = RenderSettings.path + RenderSettings.MCAstSettingsFile;
            if (File.Exists(fname))
            {
                settings = MCAstSettings.DeSerialize(fname);
                AddMessage("Settings file loaded : " + RenderSettings.MCAstSettingsFile);

            }
            else
            {
                AddMessage("Settings file created : " + RenderSettings.MCAstSettingsFile);
            }
        }

        protected void SaveSettings()
        {
			string fname = RenderSettings.path + RenderSettings.MCAstSettingsFile;
            MCAstSettings.Serialize(settings, fname);
//            AddMessage("Settings saved");
        }

        public void ClickOverview()
        {
            if (RenderSettings.renderType == RenderType.Normal)
                RenderSettings.renderType = RenderType.Overview;
            else
                RenderSettings.renderType = RenderType.Normal;
        }



        public void Slide()
        {
            float v = slider.GetComponent<Slider>().value;
            szWorld.getInterpolatedCamera(v, solarSystem.planets);
        }




        private void setPlaySpeed(float v)
        {
            if (m_playSpeed == v)
            {
                m_playSpeed = 0;
            }
            else
            {
                // Clear on play!
                if (RenderSettings.toggleSaveVideo)
                    ClearMovieDirectory();
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






        public void ClearStarSystem()
        {
            solarSystem.planets.Clear();
            for (int i = 0; i < transform.childCount; i++)
            {
                GameObject go = transform.GetChild(i).gameObject;
                GameObject.Destroy(go);
                //	Debug.Log ("Destroying " + go.name);
            }


        }

        public void ExitSave()
        {
            SaveScreenshot();
            Application.Quit();
        }



        void CreateConfig(string fname)
        {

            SerializedPlanet p = new SerializedPlanet();
            SerializedWorld sz = new SerializedWorld();
            sz.Planets.Add(p);

            SerializedCamera c = new SerializedCamera();
            c.cam_x = 0;
            c.cam_y = 0;
            c.cam_z = -20000;
            /*		c.rot_x = 0;
                    c.rot_y = 0;
                    c.rot_z = 0;*/
            c.fov = 60;

            sz.Cameras.Add(c);


            SerializedWorld.Serialize(sz, fname);
        }

        public void ToggleSaveVideoCommand()
        {
            RenderSettings.toggleSaveVideo = GameObject.Find("ToggleSaveVideo").GetComponent<Toggle>().isOn;
        }
/*#if UNITY_STANDALONE

        public void LoadXmlFile()
        {
            string xml = GameObject.Find("XMLText").GetComponent<Text>().text;
            GameObject.Find("XMLText").GetComponent<Text>().text = " ";
            LoadXmlFile(xml);
        }
#endif

#if UNITY_STANDALONE_WIN
        public void LoadCommandLineXML()
        {

            string[] cmd = System.Environment.GetCommandLineArgs();
            if (cmd.Length > 1)
            {
                if (cmd[1] != "")
                    solarSystem.LoadWorld(Application.dataPath + "/../" + cmd[1], true, true, this);
            }

            //		LoadWorld("Assets/Planet/Resources/system1.xml", true);
            szWorld.IterateCamera();
            solarSystem.space.color = new Color(szWorld.sun_col_r, szWorld.sun_col_g, szWorld.sun_col_b);
            solarSystem.space.hdr = szWorld.sun_intensity;

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
			solarSystem.LoadWorld(Application.dataPath + "/../" + cmd[1], true, true, this);
		}
		
//		LoadWorld("Assets/Planet/Resources/system1.xml", true);
		szWorld.IterateCamera();
			solarSystem.space.color = new Color(szWorld.sun_col_r,szWorld.sun_col_g,szWorld.sun_col_b);
			solarSystem.space.hdr = szWorld.sun_intensity;
			
	}
	
#endif
*/


        protected void GenerateTextures()
        {
            if (tx_background == null)
            {
                tx_background = new Texture2D(1, 1);
                tx_load = new Texture2D(1, 1);
                tx_background.SetPixel(0, 0, new Color(0, 0, 0, 1));
                tx_background.Apply();
                tx_load.SetPixel(0, 0, new Color(0.7f, 0.3f, 0.2f, 1));
                tx_load.Apply();
                // Create a circle


                int N = 512;
                tx_record = new Texture2D(N, N);
                for (int i = 0; i < N; i++)
                    for (int j = 0; j < N; j++)
                    {
                        Vector3 p = new Vector3(i / (float)N, j / (float)N, 0);
                        p -= new Vector3(0.5f, 0.5f);
                        float a = Mathf.Pow(1.8f - 2 * p.magnitude, 10);
                        tx_record.SetPixel(i, j, new Color(1, 0.2f, 0.2f, a));

                    }
                tx_record.Apply();
                //			tx_background = (Texture2D)Resources.Load ("cloudsTexture");

            }

        }

        protected void RenderProgressbar()
        {
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), tx_background);
            float h = 0.08f;
            int hei = (int)(Screen.height * h);
            float border = 0.05f;
            int rectwidth = (int)(Screen.width * (1 - 2 * border));
            GUI.DrawTexture(new Rect(Screen.width * border, Screen.height / 2 - hei, (int)(rectwidth / 100f * load_percent), 2 * hei), tx_load);
            GUI.Label(new Rect(Screen.width / 2 - 40, (int)(Screen.height * (2 / 3f)), 200, 200), RenderSettings.generatingText);

        }

        public void SaveScreenshot()
        {
            string warn = "";
            if (percent != 100)
                warn = " - WARNING: Image not fully processed (" + percent + "%)";

            string file = WriteScreenshot(RenderSettings.screenshotDir, MCAstSettings.Resolution[settings.screenShotResolution,0], MCAstSettings.Resolution[settings.screenShotResolution, 1]);
            AddMessage("Screenshot saved to " + RenderSettings.screenshotDir + file + warn);

        }

        protected void OnGUI()
        {

            GenerateTextures();

            if (RenderSettings.isVideo)
            {
                // Render blinking record sign
                if (RenderSettings.toggleSaveVideo && m_playSpeed>0)
                {
                    int s = Screen.width / 50;
                    int b = 50;
                    int t = (int)(Time.time * 2);
                    if (t % 2==0) {
                        GUI.DrawTexture(new Rect(Screen.width-b-s, Screen.height -b -s, s,s), tx_record);
                    } 
                }
                return;
            }
            // Generate Textures
            if (!hasScene)
                return;

            if (load_percent == 100)
                return;

            if (RenderSettings.toggleProgressbar)
                RenderProgressbar();

        }

        public void displayHelpPanel() {
            if (helpPanel != null)
            {
                helpPanel.SetActive(true);
                helpPanel.transform.SetAsLastSibling();
            }

        }

		public void closeHelpPanel() {
            if (helpPanel!=null)
            	helpPanel.SetActive(false);

        }
        public void displaySettingsPanel()
        {
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(true);
                settingsPanel.transform.SetAsLastSibling();
            }

        }

        public void closeSettingsPanel()
        {
            PopulateSettingsFromGUI();
            if (settingsPanel != null)
                settingsPanel.SetActive(false);

            SaveSettings();

        }


        public void PopulateResolutionCombobox(string box)
        {
            Dropdown cbx = GameObject.Find(box).GetComponent<Dropdown>();
            cbx.ClearOptions();
            List<Dropdown.OptionData> l = new List<Dropdown.OptionData>();

            for (int i=0;i<MCAstSettings.Resolution.GetLength(0);i++)
            {
                string s = MCAstSettings.Resolution[i, 0] + "x" + MCAstSettings.Resolution[i, 1];
                ComboBoxItem ci = new ComboBoxItem();
                l.Add(new Dropdown.OptionData(s));
            }

            cbx.AddOptions(l);

        }

        public void PopulateIndexCombobox(string box, int[] lst)
        {
            Dropdown cbx = GameObject.Find(box).GetComponent<Dropdown>();
            cbx.ClearOptions();
            List<Dropdown.OptionData> l = new List<Dropdown.OptionData>();

            for (int i = 0; i < lst.GetLength(0); i++)
            {
                string s = ""+ lst[i];
                ComboBoxItem ci = new ComboBoxItem();
                l.Add(new Dropdown.OptionData(s));
            }
            cbx.AddOptions(l);

        }


        private void SetupGUI()
        {
        	SetupErrorPanel();
			helpPanel = GameObject.Find("HelpPanel");
            if (helpPanel != null)
                helpPanel.SetActive(false);
			settingsPanel = GameObject.Find("SettingsPanel");
			if (settingsPanel != null)
                settingsPanel.SetActive(false);

			PlanetTypes.Initialize();

			if (settingsPanel != null)
                settingsPanel.SetActive(true); // must be true before populating

            LoadSettings();

            PopulateFileCombobox("ComboBoxLoadFile", "xml");
            PopulateResolutionCombobox("MovieResolutionCmb");
            PopulateResolutionCombobox("ScreenshotResolutionCmb");
            PopulateIndexCombobox("GridSizeCmb", MCAstSettings.GridSizes);
            displaySettingsPanel();
            PopulateGUISettings();
            



            if (settingsPanel != null)
                settingsPanel.SetActive(false);


        }


        public void LoadFileFromMenu()
        {
            int idx = GameObject.Find("ComboBoxLoadFile").GetComponent<Dropdown>().value;
            string name = GameObject.Find("ComboBoxLoadFile").GetComponent<Dropdown>().options[idx].text;
           	if (name=="-")
           		return;
			name =RenderSettings.dataDir + name + ".xml";

            LoadFromXMLFile(name);
            settings.previousFile = name;
            PopulateOverviewList("Overview");
            slider.GetComponent<Slider>().value = 0;

        }

        public void FocusOnPlanetFromMenu()
        {
            int idx = GameObject.Find("Overview").GetComponent<Dropdown>().value;
            string name = GameObject.Find("Overview").GetComponent<Dropdown>().options[idx].text;
            FocusOnPlanet(name);

        }

    public override void Start()
        {
            RenderSettings.path = Application.dataPath + "/../";
            CurrentApp = Verification.MCAstName;

			if (solarSystem == null)
    			solarSystem = new SolarSystem(sun, sphere, transform, (int)szWorld.skybox);

            SetupGUI();
			base.Start();

//            solarSystem.InitializeFromScene();

            GameObject.Find("TextVersion").GetComponent<Text>().text = "Version " + RenderSettings.version.ToString("0.00"); ;
            debugPanel = GameObject.Find("DebugPanel");
            debugPanel.SetActive(false);

            RenderSettings.MoveCam = false;
            //		slider = GameObject.Find ("Slider");
            //          Debug.Log(slider);
            if (slider != null)
                slider.SetActive(true);

            if (settings.previousFile != "")
            {
                LoadFromXMLFile(settings.previousFile);
                szWorld.IterateCamera();
                
                //                szWorld.getInterpolatedCamera(0, solarSystem.planets);

            }



#if UNITY_STANDALONE
            //	LoadCommandLineXML();
#endif
        }
        public override void LoadFromXMLFile(string filename, bool randomizeSeed = false)
        {
            AddMessage("Loading XML file: " + filename);



			if (!Verification.VerifyXML(RenderSettings.path + filename, Verification.MCAstName)) {
				AddMessage("ERROR: File " + filename + " is not a valid MCAst data file. Aborting. ", 2.5f);
				return;
			}


            base.LoadFromXMLFile(filename, randomizeSeed);
            displaySettingsPanel();
            PopulateSettingsFromGUI();
            closeSettingsPanel();
            Slide();

        }

            void setSun()
        {
            if (sun == null)
                return;

            sun.transform.rotation = Quaternion.FromToRotation(Vector3.forward, World.WorldCamera.toVectorf().normalized);
            sun.GetComponent<Light>().color = solarSystem.space.color;
        }

        // Update is called once per frame

        private void UpdateSlider()
        {



        }

        protected void UpdatePlay()
        {
  //          Debug.Log(Time.time + " " + m_playSpeed);
            if (m_playSpeed > 0 && solarSystem.planets.Count!=0)
            {
                canvas.SetActive(true);
                float v = slider.GetComponent<Slider>().value;
                v += m_playSpeed;
                if (v >= 1)
                {
                    m_playSpeed = 0;
                    v = 0;
                }
//                Debug.Log("Playspeed after: " + m_playSpeed + " " + Time.time);
                slider.GetComponent<Slider>().value = v;
                canvas.SetActive(RenderSettings.RenderMenu);

                szWorld.getInterpolatedCamera(v, solarSystem.planets);
                if (RenderSettings.toggleSaveVideo)
                {
                    string f = WriteScreenshot(RenderSettings.movieDir,
                        MCAstSettings.Resolution[settings.movieResolution,0], MCAstSettings.Resolution[settings.movieResolution, 1]);
                    AddMessage("Movie frame saved to : " + RenderSettings.movieDir + f, 0.025f);
                }
                
            }

        }


        public void TogglePlanetTypes()
        {
            RenderSettings.ForceAllPlanetTypes++;
            if (RenderSettings.ForceAllPlanetTypes>=PlanetSettings.planetTypes.planetTypes.Count)
            {
                RenderSettings.ForceAllPlanetTypes = -1;
                AddMessage("Resetting to all planet types");
            }
            else
            {
                //AddMessage("Setting all planets to type : " + RenderSettings.ForceAllPlanetTypes +"  (" + PlanetSettings.planetTypes.planetTypes[RenderSettings.ForceAllPlanetTypes].Name + ")", 4);
            }
            LoadFromXMLFile(settings.previousFile, false);

        }

        public void ToggleFlyCamera() {
			RenderSettings.MoveCam = !RenderSettings.MoveCam;

        }

        public void RandomizeSeeds() {
			if (settings.previousFile!="") {
            		LoadFromXMLFile(settings.previousFile, true);
            		AddMessage("Planet seeds set to random value");
			}

        }

        public override void Update()
        {
            base.Update();
            UpdatePlay();

            if (modifier)
                if (Input.GetKeyUp(KeyCode.P))
                {
                    debugPanel.SetActive(!debugPanel.activeSelf);

                    // Randomize seed

                }

            if (modifier)
                if (Input.GetKeyUp(KeyCode.L))
                {
                    RenderSettings.MoveCam = !RenderSettings.MoveCam;


                }
        }


        private int percent;

        protected override void Log()
        {
            string s = "";
            float val = 1;
            if (ThreadQueue.orgThreads != 0)
                val = (ThreadQueue.threadQueue.Count / (float)ThreadQueue.orgThreads);

            percent = 100 - (int)(100 * val);


            if (percent == 100 && RenderSettings.ExitSaveOnRendered && ThreadQueue.currentThreads.Count == 0)
            {
                if (extraTimer-- == 0)
                    ExitSave();
            }
            load_percent = percent;

            if (RenderSettings.isVideo)
                s += "Progress: " + percent + " %\n";
            //s+="Height: " + stats.Height.ToString("0.00") + " km \n";
            //s+="Velocity: " + stats.Velocity.ToString("0.00") + " km/s\n";
            s += RenderSettings.extraText + "\n";
            foreach (Message m in messages)
                s += m.message + "\n";


            s+=RenderSettings.path;
            GameObject info = GameObject.Find("Logging");
            if (info != null)
                info.GetComponent<Text>().text = s;
        }


    }

}