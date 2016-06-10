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



public class WorldMC : World {


//	public static DVector WorldCamera = new DVector();
//	public GameObject sun;
//	public Mesh sphere;
	
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
	
	protected void FocusOnPlanet(string n) {
		GameObject gc = GameObject.Find ("Camera");
		//Camera c = gc.GetComponent<Camera>();
		Planet planet = null;
		foreach (Planet p in solarSystem.planets)
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
	

	
	protected void PopulateOverviewList(string box) {
			ComboBox cbx = GameObject.Find (box).GetComponent<ComboBox>();
			cbx.ClearItems();
			List<ComboBoxItem> l = new List<ComboBoxItem>();
			foreach (Planet p in solarSystem.planets)  {
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
		solarSystem.planets.Clear();
		for (int i=0;i<transform.childCount;i++) { 
			GameObject go = transform.GetChild(i).gameObject;
			GameObject.Destroy(go);
		//	Debug.Log ("Destroying " + go.name);
		}
			
				
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
			LoadXmlFile(xml);
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
			solarSystem.LoadWorld(Application.dataPath + "/../" + cmd[1], true, true, this);
		}
		
//		LoadWorld("Assets/Planet/Resources/system1.xml", true);
		szWorld.IterateCamera();
			solarSystem.space.color = new Color(szWorld.sun_col_r,szWorld.sun_col_g,szWorld.sun_col_b);
			solarSystem.space.hdr = szWorld.sun_intensity;
			
	}
	
	#endif
	

	
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
	
	public override void Start () {

		base.Start();
		slider = GameObject.Find ("Slider");

		if (slider!=null)
			slider.SetActive(false);
		
//		CreateConfig("system1.xml");
//		LoadWorld("system1.xml", true);
//		szWorld.IterateCamera();

		#if UNITY_STANDALONE
			LoadCommandLineXML();
		#endif
/*			LoadWorld("system1.xml", true,false);
			szWorld.IterateCamera();
			space.color = new Color(szWorld.sun_col_r,szWorld.sun_col_g,szWorld.sun_col_b);
			space.hdr = szWorld.sun_intensity;
			
*/		
		// FOCUS som er problemet
	}
	

	void setSun() {
//		if (World.WorldCamera
		if (sun==null)
			return;
		sun.transform.rotation = Quaternion.FromToRotation(Vector3.forward, World.WorldCamera.toVectorf().normalized);
		sun.GetComponent<Light>().color = solarSystem.space.color;
	}
	
	// Update is called once per frame
	
	private void UpdateSlider() {
	
		
	
	}	
		
	public override void Update () {
		base.Update();
			
		if (RenderSettings.RenderMenu)
			Log();

		
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
				
		s+="Mission Control AST1100 Version: " + RenderSettings.version.ToString("0.00")  + " \n";
		if (RenderSettings.isVideo)
			s+="Progress: " + percent + " %\n";
		//s+="Height: " + stats.Height.ToString("0.00") + " km \n";
		//s+="Velocity: " + stats.Velocity.ToString("0.00") + " km/s\n";
		s+=RenderSettings.extraText;
		GameObject info = GameObject.Find ("Logging");
		if (info!=null)
			info.GetComponent<Text>().text = s;
	}
	
	
}

}