using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Threading;

/*
 * 
 * © 2014 LemonSpawn. All rights reserved. Source code in this project ("LemonSpawn") are not supported under any LemonSpawn standard support program or service. 
 * The scripts are provided AS IS without warranty of any kind. LemonSpawn disclaims all implied warranties including, without limitation, 
 * any implied warranties of merchantability or of fitness for a particular purpose. 
 * 
 * Basically, do what you want with this code, but don't blame us if something blows up. 
 * 
 * 
*/

namespace LemonSpawn
{

		public class SurfaceEditor : CNodeManager
		{
				public GUIStyle largeFont = new GUIStyle ();
				static public string[] statusStrings = {
				"Done",
				"Initializing",
				"Calculating.",
				"Calculating..",
				"Thread done",
				"Generating alpha maps",
				"Generating alpha maps",
				"Setting alpha maps"
		};
				static private string aboutMessage = 
			"<size=24>" + LStyle.hexColors [1] + "LemonSpawn procedural material generator </color></size>\n" +
						"<size=16>" + LStyle.hexColors [2] + "by LemonSpawn </color></size>\n" +
						"<size=12>" + LStyle.hexColors [3] + "http://www.lemonspawn.com </color></size>\n" +
						"\n" +
						"\n" +
						"LemonSpawn is a node-based procedural material generator extension for Unity, developed by LemonSpawn. \n\n" +
						"To get started, right-click anywhere on the canvas to add a node, or load an example file from the 'Data/Textures' folder. Connect nodes with same colors, and " +
						"attach them correctly to the output node. When you are happy with a material, export the result as a unity parallax material. \n\n" +
						"Please visit our home page for instructions and tutorial videos!\n" +
						"\n";
				// Initialize certain things at startup		
				private static bool startInitialize = false;
				// Link to the terrain manager
				// Link to settings
//				public static Settings settings = null;
				// Link to active terrain 
				// LimeStone Logo
				public static Texture2D logo = null, background = null, background2 = null;
				public static Texture2D texRed = null, texGreen = null;
				// Parent object
				private static bool outputVerified = false;
//				public static Output output = null;
				// Define menu item
				[MenuItem ("Window/LemonSpawn/LemonSpawn Generator")]
				static void Init ()
				{
//						window = EditorWindow.GetWindow (typeof(LemonSpawnEditor));
				}

				
				// Renders the logo and sets up the retro background color. 		
/*				private void renderLogo ()
				{
						if (logo == null) {
								logo = (Texture2D)Resources.Load ("LemonSpawnLogo", typeof(Texture2D));
						}
			
						if (background == null) {
								background = new Texture2D (1, 1, TextureFormat.RGBA32, false);
								background.SetPixel (0, 0, new Color(0.2f,0.2f,0.2f));
								background.Apply ();
						}
						if (texRed == null) {
								texRed = new Texture2D (1, 1, TextureFormat.RGBA32, false);
								texGreen = new Texture2D (1, 1, TextureFormat.RGBA32, false);
								texRed.SetPixel (0, 0, new Color (1, 0.1f, 0.1f, 1));
								texRed.Apply ();
								texGreen.SetPixel (0, 0, new Color (0.3f, 1.0f, 0.3f, 1));
								texGreen.Apply ();
						}
				
			
						if (background2 == null) {
								background2 = new Texture2D (1, 1, TextureFormat.RGBA32, false);
								background2.SetPixel (0, 0, LStyle.Colors [0] * 0.75f);
								background2.Apply ();
						}
						if (logo != null)
								GUI.DrawTexture (new Rect (0, 0, logo.width, 1.0f * logo.height), logo);

						GUI.DrawTexture (new Rect (0, 0, Screen.width, Screen.height), background);
			
						if (texRed != null) {
								Texture2D t = texRed;
								if (outputVerified)
										t = texGreen;
								GUI.DrawTexture (new Rect (Screen.width - 300, 5, 50, 20), t);
						}	
				}
				// Buttons and stuff
				
				private void maintainParent ()
				{
						parent = GameObject.Find (LStyle.TextureGameobjectName);
						if (parent == null) {
								parent = new GameObject (LStyle.TextureGameobjectName);
								parent.AddComponent<Settings> ();
						}
			
						settings = parent.GetComponent<Settings> ();
				}
				
				private void renderMenuButtons ()
				{
						float dy = Screen.height / 15f;///34;
						float hy = Screen.height / 15 - 4;
						float y = hy / 2;
						float i = 0;
					
						int bsx = 200;
						int dbsx = (int)(bsx * 1.15f);
						GUI.color = LStyle.Colors [4];
						GUI.DrawTexture (new Rect (Screen.width - dbsx, 0, Screen.width, Screen.height), background2);
						GUI.color = LStyle.Colors [4];
						GUI.backgroundColor = LStyle.Colors [4];
						int db = dbsx - bsx; 
			
						if (GUI.Button (new Rect (Screen.width - bsx, y + dy * i++, bsx - db, hy), "View material"))
								MaterialPreviewEditor.Create (LStyle.MaterialName);
						if (GUI.Button (new Rect (Screen.width - bsx, y + dy * i++, bsx - db, hy), "New")) {
								New ();
						}
						if (GUI.Button (new Rect (Screen.width - bsx, y + dy * i++, bsx - db, hy), "Load"))
								Load ();
						if (GUI.Button (new Rect (Screen.width - bsx, y + dy * i++, bsx - db, hy), "Save"))
								Save ();
						if (GUI.Button (new Rect (Screen.width - bsx, y + dy * i++, bsx - db, hy), "Save as"))
								SaveAs ();
						if (GUI.Button (new Rect (Screen.width - bsx, y + dy * i++, bsx - db, hy), "About"))
								About ();
						if (GUI.Button (new Rect (Screen.width - bsx, y + dy * i++, bsx - db, hy), "Export"))
								Export ();
						i++;
						if (GUI.Button (new Rect (Screen.width - bsx, y + dy * i++, bsx - db, hy), "Abort"))
								KillThread ();
						if (GUI.Button (new Rect (Screen.width - bsx, y + dy * i++, bsx - db, hy), "Force calculate")) {
								output.changed = true;
								mouseUp = true;
								maintainTextures ();
						}
						if (GUI.Button (new Rect (Screen.width - bsx, y + dy * i++, bsx - db, hy), "Refresh nodes"))
								RefreshNodesNew ();
			

						y += hy;

				}
				
				// prepares GUI with settings, buttons, background and whatnot
				public override void SetupGUI ()
				{
						largeFont.fontSize = 16;
						largeFont.normal.textColor = LStyle.Colors [2];//new Color(0.5f, 0.5f, 0.5f);

						renderLogo ();

						//GUI.color = LStyle.Colors[4];
						string s = settings.currentFile;
						if (s == "") 
								s = "[ No file. Be warned: running game or recompiling will discard unsaved work, so remember to save! ]"; 

						GUI.Label (new Rect (40, Screen.height - 50, 700, 50), "Current file: " + s, largeFont);
				
						if (status != STATUS_NONE) 
								EditorUtility.DisplayProgressBar ("Calculating", "Please stand by...", progress);
						else
								EditorUtility.ClearProgressBar ();


				}

				private void maintainOutput ()
				{
						ArrayList l = findType (nodes, typeof(Output));
						if (l.Count != 0) {
								//Debug.Log (l[0]);
								output = (Output)l [0];
								return;		
						}	
			
						output = new Output ();
						output.Initialize (findNextN (), 0, Screen.width / 2, Screen.height / 2);
						nodes.Add (output);
						ResetState ();
				}

				public SurfaceEditor () : base()
				{
						startInitialize = false;
				}

				public void Update ()
				{
						if (!startInitialize)
								return;
			
						maintainTextures ();
						maintainOutput ();
						InternalUpdate ();
						Tasks ();

				}

				public override void PreAssemble ()
				{
				}
				
				public override void Create ()
				{
						output.CalculateTextures ();			
						MaterialPreviewEditor.forceRepaint ();
						Repaint ();
						status = STATUS_NONE;
			
				}

				private void maintainTextures ()
				{
						if (output == null)
								return;
						outputVerified = VerifyNodes (output);
						if (!outputVerified) 
								return;
							
					
						if (output.changed && link == null && mouseUp) {
				    	
								KillThread ();
								status = STATUS_START_THREAD;
				
						
								progress = 0;
								CountChildNodes (output);
								startThread ();
								mouseUp = false;
								//Debug.Log("THREAD STARTED");						
						}
			
				}

				private void Tasks ()
				{
						calculator = output;
						if (status == STATUS_TASK1) {
								if (calculator != null)
										calculator.Calculate ();
								status = STATUS_NONE;
						}


						if (status == STATUS_IN_THREAD) {
								// Needs to update the status bar. bloody hell. 
								ForceRepaint = true;
						}
						if (status == STATUS_TASK3) {
								ResetState ();
								status = STATUS_NONE;
						}
						if (status == STATUS_TASK4) {
								ResetMaps ();
								status = STATUS_NONE;
						}
						if (status == STATUS_TASK5) {
								LoadSafe ();
						}

				}

				public override void setupMenu ()
				{
				
						menuItems.Clear ();
						menuItems.Add (new MenuType ("Heightmaps/Perlin", typeof(HeightMapGenerator), HeightMapGenerator.TYPE_PERLIN));
						menuItems.Add (new MenuType ("Heightmaps/PerlinClouds", typeof(HeightMapGenerator), HeightMapGenerator.TYPE_PERLINCLOUDS));
						menuItems.Add (new MenuType ("Heightmaps/Multiridged", typeof(HeightMapGenerator), HeightMapGenerator.TYPE_MULTIRIDGED));
						menuItems.Add (new MenuType ("Heightmaps/Swiss", typeof(HeightMapGenerator), HeightMapGenerator.TYPE_SWISS));
						menuItems.Add (new MenuType ("Heightmaps/Filter", typeof(HeightFilter), HeightFilter.TYPE_SMOOTH));
						menuItems.Add (new MenuType ("Heightmaps/Curvature Filter", typeof(HeightCountour), 0));
						menuItems.Add (new MenuType ("Heightmaps/Combiner", typeof(HeightCombiner), HeightCombiner.TYPE_BLEND));
						menuItems.Add (new MenuType ("Patterns/Bricks", typeof(PatternGenerator), PatternGenerator.TYPE_BRICKS));
						menuItems.Add (new MenuType ("Patterns/Grid", typeof(PatternGenerator), PatternGenerator.TYPE_GRID));
						menuItems.Add (new MenuType ("Patterns/Circles", typeof(PatternGenerator), PatternGenerator.TYPE_REGULARDOTS));
						//menuItems.Add (new MenuType ("Patterns/L2System", typeof(L2System), 0));
						//menuItems.Add (new MenuType ("Patterns/Snowflake", typeof(Snowflake), 0));
						menuItems.Add (new MenuType ("Patterns/Flower", typeof(FlowerPattern), FlowerPattern.TYPE_FLOWER));
						menuItems.Add (new MenuType ("Patterns/Grass", typeof(FlowerPattern), FlowerPattern.TYPE_GRASS));
						menuItems.Add (new MenuType ("RGB/Height", typeof(RGBHeight), 0));
						menuItems.Add (new MenuType ("RGB/Curvature", typeof(RGBCurvature), 0));
						menuItems.Add (new MenuType ("RGB/Combiner", typeof(RGBCombiner), 0));
						menuItems.Add (new MenuType ("Normal map/Normal map", typeof(BumpMap), 0));
			
//						menuItems.Add (new MenuType ("Output", typeof(COutput), 0, true, "You can only have one output!"));

						base.setupMenu ();
				}
				
				public override void ResetMaps ()
				{
						maintainOutput ();
						if (output == null) {
								Debug.Log ("Output is NULL; cannot reset maps");
								return;
						}
						size = (int)output.getValue ("size");
						
						C2DMap.sizeX = size;
						C2DMap.sizeY = size;
						base.ResetMaps ();
						foreach (CNode c in nodes)
								c.PropagateChange ();
				}
				
				// First run
				void StartInitialize ()
				{

						if (settings == null) {
								return;
						}
						size = 256;
						C2DMap.sizeX = size;
						C2DMap.sizeY = size;
						if (settings.currentFile == "")
								New ();
						else 
								LoadDirect (settings.currentFile);

						MaterialPreviewEditor.Create (LStyle.MaterialName);
			
						startInitialize = true;
						ForceRepaint = true;
						ResetState ();
						maintainTextures ();

				}

				void OnGUI ()
				{
						
						if (settings == null) 
								maintainParent ();
			
						if (!startInitialize) 
								StartInitialize ();
						
						
						Render ();
						
						renderMenuButtons ();
						
						if (Event.current.keyCode == KeyCode.Return)
						if (output != null) {
								output.Calculate ();
						}
			
			
				}
				// Create new node system
				void New ()
				{
						if (status != STATUS_NONE)
								return;
						nodes.Clear ();
						links.Clear ();
						settings.currentFile = "";
						Initialize ();
						ResetState ();
				}
	
				protected override void ResetState ()
				{
						//status = STATUS_CHANGED;
						foreach (CNode cn in nodes) {
								cn.changed = true;
								cn.Verify ();
								((CTextureNode)cn).updateTexture = true;
						}
						mouseUp = true;
				}
		
				public override void Initialize ()
				{
						base.Initialize ();
						initialized = true;
				}
		
				void Load ()
				{
						if (status != STATUS_NONE)
								return;
				
						string fn = EditorUtility.OpenFilePanel (
							"Load texture",
							"",
						LStyle.Filetype);
						if (fn.Length == 0)
								return;
						settings.currentFile = fn;
						status = STATUS_TASK5;
				}
		
				void LoadSafe ()
				{
			
						LoadDirect (settings.currentFile);
						
						ResetMaps ();
						ResetState ();
							
						status = STATUS_NONE;	
						mouseUp = true;
						output.changed = true;
						
						Update ();
						mouseUp = true;
						Repaint ();
				}

				void Save ()
				{
						if (settings.currentFile.Length != 0)
								CSerializedNode.Save (settings.currentFile, nodes);
						else {
								SaveAs ();
						}

				}
				
				void Export ()
				{
						if (output == null)
								return;
						if (status != STATUS_NONE) 
								DisplayError ("You cannot export before all nodes have completed");
					
						
						string file = EditorUtility.SaveFilePanelInProject (
						"Export material & textures as",
				"",
				"",
				"");
						if (file.Length!=0)
							output.Export (file);
					
			
					
				}

				void SaveAs ()
				{
						string file = EditorUtility.SaveFilePanel (
			"Save texture as",
			"",
			"",
				LStyle.Filetype);
			
						if (file.Length != 0) {
								settings.currentFile = file;
								Save ();
						}
				}

				void About ()
				{
						HelpEditor.Create (aboutMessage);                         
				}
				
				*/
		}


}