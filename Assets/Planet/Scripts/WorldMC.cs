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

namespace LemonSpawn
{



    public class WorldMC : World
    {


        private float m_playSpeed = 0;
        protected Texture2D tx_background, tx_load, tx_record;
        protected int load_percent;


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
                m_playSpeed = 0;
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

        protected void FocusOnPlanet(string n)
        {
            GameObject gc = mainCamera;
            //Camera c = gc.GetComponent<Camera>();
            Planet planet = null;
            foreach (Planet p in solarSystem.planets)
                if (p.pSettings.name == n)
                    planet = p;

            if (planet == null)
                return;

            DVector pos = planet.pSettings.pos;
            float s = (float)(planet.pSettings.radius * szWorld.overview_distance / RenderSettings.AU);
            Vector3 dir = pos.toVectorf().normalized * s;
            Vector3 side = Vector3.Cross(Vector3.up, dir);

            pos = pos - new DVector(dir) - new DVector(side.normalized * s);
            pos.y += s;

            gc.GetComponent<SpaceCamera>().SetLookCamera(pos, planet.pSettings.gameObject.transform.position, Vector3.up);
            UpdateWorldCamera();
            Update();
            gc.GetComponent<SpaceCamera>().SetLookCamera(pos, planet.pSettings.gameObject.transform.position, Vector3.up);
            UpdateWorldCamera();

        }



        protected void PopulateOverviewList(string box)
        {
            ComboBox cbx = GameObject.Find(box).GetComponent<ComboBox>();
            cbx.ClearItems();
            List<ComboBoxItem> l = new List<ComboBoxItem>();
            foreach (Planet p in solarSystem.planets)
            {
                ComboBoxItem ci = new ComboBoxItem();
                ci.Caption = p.pSettings.name;
                string n = p.pSettings.name;
                ci.OnSelect = delegate
                {
                    FocusOnPlanet(n);
                };
                l.Add(ci);
            }
            //		foreach (ComboBoxItem i in l)
            //			Debug.Log (i.Caption);

            cbx.AddItems(l.ToArray());

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



        protected void OnGUI()
        {

            //	return;


            if (RenderSettings.isVideo)
            {
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
                for (int i=0;i<N;i++)
                    for (int j = 0; j < N; j++)
                    {
                        Vector3 p = new Vector3(i / (float)N, j / (float)N,0);
                        p -= new Vector3(0.5f, 0.5f);
                        float a = Mathf.Pow(1.8f - 2*p.magnitude, 10);
                        tx_record.SetPixel(i, j, new Color(1, 0.2f, 0.2f, a));

                    }
                tx_record.Apply();
                //			tx_background = (Texture2D)Resources.Load ("cloudsTexture");

            }
            if (!hasScene)
                return;
            if (load_percent == 100)
                return;

            //	return;	
            //	return;
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), tx_background);
            float h = 0.08f;
            int hei = (int)(Screen.height * h);
            float border = 0.05f;
            int rectwidth = (int)(Screen.width * (1 - 2 * border));
            GUI.DrawTexture(new Rect(Screen.width * border, Screen.height / 2 - hei, (int)(rectwidth / 100f * load_percent), 2 * hei), tx_load);
            GUI.Label(new Rect(Screen.width / 2 - 40, (int)(Screen.height * (2 / 3f)), 200, 200), RenderSettings.generatingText);
        }

        public override void Start()
        {

            base.Start();
            solarSystem.InitializeFromScene();

            GameObject.Find("TextVersion").GetComponent<Text>().text = "Version " + RenderSettings.version.ToString("0.00"); ;


            RenderSettings.MoveCam = false;
            //		slider = GameObject.Find ("Slider");
            //          Debug.Log(slider);
            if (slider != null)
                slider.SetActive(true);

            PopulateFileCombobox("ComboBoxLoadFile", "xml");

            //		CreateConfig("system1.xml");
            //		LoadWorld("system1.xml", true);
            //		szWorld.IterateCamera();

#if UNITY_STANDALONE
            //	LoadCommandLineXML();
#endif
            /*			LoadWorld("system1.xml", true,false);
                        szWorld.IterateCamera();
                        space.color = new Color(szWorld.sun_col_r,szWorld.sun_col_g,szWorld.sun_col_b);
                        space.hdr = szWorld.sun_intensity;

            */
            // FOCUS som er problemet
        }


        void setSun()
        {
            //		if (World.WorldCamera
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
            if (m_playSpeed > 0 && solarSystem.planets.Count!=0)
            {
                float v = slider.GetComponent<Slider>().value;
                v += m_playSpeed;
                if (v >= 1)
                {
                    m_playSpeed = 0;
                    v = 0;
                }

                slider.GetComponent<Slider>().value = v;

                szWorld.getInterpolatedCamera(v, solarSystem.planets);
                if (RenderSettings.toggleSaveVideo)
                {
                    WriteScreenshot(RenderSettings.movieDir);
                }
                
            }

        }

        public override void Update()
        {
            base.Update();

            if (RenderSettings.RenderMenu)
                Log();

            UpdatePlay();

        }




        void Log()
        {
            string s = "";
            float val = 1;
            if (ThreadQueue.orgThreads != 0)
                val = (ThreadQueue.threadQueue.Count / (float)ThreadQueue.orgThreads);

            int percent = 100 - (int)(100 * val);


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
            s += RenderSettings.extraText;
            GameObject info = GameObject.Find("Logging");
            if (info != null)
                info.GetComponent<Text>().text = s;
        }


    }

}