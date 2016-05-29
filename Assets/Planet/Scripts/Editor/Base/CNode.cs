using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

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

namespace LemonSpawn{

		
		/*
		*
		* Parent node class
		*
		*
		*/
		public class CNode
		{
				public string Name = "CNode";
				// Window ID.
				protected string[] types = null;
				public int ID;
				public int Type;
				public bool counted = false;
				
				public static bool editPresets = false;
				
				public static bool debug = false;
				public int status_after_click = 0;
				List<string> keys;
				// Internal type - which kind of combiner etc
				protected bool displayTypes = true;
				// If changed, update
				public bool changed = true;
				// Correct amount if inputs/outputs
				public bool verified = false;
				public static bool allVerified = false;
				// clickerrormessage displayed if node incorrectly used. Help is generic. 
				public string helpMessage = "";
				public string clickErrorMessage = "";
				// Data for popup selection
				protected PopupData popupData;
				public List<Parameter> displayParameters = null;
				// Alternate names for parameters
				protected List<AlternativeName> alternativeNames = new List<AlternativeName> ();

				// Font for printing
				static public GUIStyle largeFont = new GUIStyle ();
				// Internal tabbing
				public static int TAB = 0;
				// All parameters 
				public Hashtable parameters = new Hashtable ();

				int labelSize;
				int remainingSize;

				// Default color
				public Color color = new Color (1, 1, 0.2f);
				public Color errorColor = new Color (1, 0, 0.2f, 0f);

				// The rect for this node
				public Rect window;
				// Default (x,y) size for this window
				public Vector2 size = new Vector3 (340, 0);
				public Vector2 rightSize = new Vector3 (170, 0);
				// only used for alpha maps. terrible solution, but quick fix. 
				// List of connections
				public ArrayList Inputs = new ArrayList ();
				public ArrayList Outputs = new ArrayList ();
				public ArrayList Bottoms = new ArrayList ();
				public ArrayList Tops = new ArrayList ();
				// Used for counting the number of children of this node
				public static int childrenCount;
				
				
				
				// counds the number of child nodes
				public void getInputsChildren ()
				{
						if (!counted) {
							childrenCount += 1;
							counted = true;
						}
						foreach (CConnection c in Inputs) {
								if (c.pointer != null)
										c.pointer.parent.getInputsChildren ();
						}

				}
				// Sets up progress delta for status bar
				public void calculateProgressDelta (float size)
				{
						childrenCount = 0;
						getInputsChildren ();
						CNodeManager.progressDelta = 100.0f / (childrenCount * (size * size));
						CNodeManager.progress = 0;
				}

				// Returns this nodes connection type
				private int getConnectionType ()
				{
						if (Inputs.Count != 0)
								return ((CConnection)Inputs [0]).Type;
						if (Outputs.Count != 0)
								return ((CConnection)Outputs [0]).Type;
						if (Bottoms.Count != 0)
								return ((CConnection)Bottoms [0]).Type;
						if (Tops.Count != 0)
								return ((CConnection)Tops [0]).Type;

						return -1;
				}
				// creates a list of links for the connections in use
				public void SetupLinks (ArrayList links)
				{

						foreach (CConnection c in Outputs) {
								if (c.pointer != null) {
										CLink l = new CLink ();
										l.from = c;
										l.to = c.pointer;
										links.Add (l);
								}
						}
						foreach (CConnection c in Bottoms) {
								if (c.pointer != null) {
										CLink l = new CLink ();
										l.from = c;
										l.to = c.pointer;
										l.drawType = 1;
										links.Add (l);
								}
						}

				}
				// get a link node
				protected CNode getNode (ArrayList l, int i)
				{
						CConnection c = ((CConnection)l [i]);
						if (c == null)
								return null;
						if (c.pointer == null)
								return null;

						return c.pointer.parent;

				}
				// returns a value of a parameter
				public float getValue (string p)
				{
						if ((Parameter)parameters [p] == null)
								return -1;
						return ((Parameter)(parameters [p])).value;
				}
				// returns a value of a parameter
				public string getString (string p)
				{
						if ((Parameter)parameters [p] == null)
								return "";
						return ((Parameter)(parameters [p])).label;
				}
				public Color getColor (string p)
				{
					if ((Parameter)parameters [p] == null)
						return new Color(0,0,0);
					return new Color(((Parameter)(parameters [p])).value, ((Parameter)(parameters [p])).min, ((Parameter)(parameters [p])).max);
				}
		
				// sets a parameter value
				public void setValue (string p, float v)
				{
						if ((Parameter)parameters [p] == null)
								return;
						((Parameter)(parameters [p])).value = v;
				}
				// sets a new parameter max val
				public void setMax (string p, float v)
				{
						if ((Parameter)parameters [p] == null)
								return;
						((Parameter)(parameters [p])).max = v;
				}

				public void PropagateChange ()
				{
//					if (Event.current.isMouse && Event.current.)
					changed = true;
						foreach (CConnection c in Outputs) {
							if (c.pointer != null)
							if (c.pointer.parent!= null)
									c.pointer.parent.PropagateChange();
						}
						
				}

				// set up alternative names for labels
				protected void setupAlternativeNames ()
				{
						foreach (AlternativeName an in alternativeNames) {
								if (an.type == Type) {
										Parameter p = (Parameter)parameters [an.hashName];
										if (p == null)
												return;
										p.label = an.label;
								}
						}
				}

/*				private static int SortByName (Parameter o1, Parameter o2)
				{
						return o1.label.CompareTo (o2.label);
				}*/
				private static int SortByNameString (string o1, string o2)
				{
						return o1.CompareTo (o2);
				}
				private static int SortByIndex (Parameter o1, Parameter o2)
				{
					return o1.index.CompareTo (o2.index);
				}
				
				public virtual void resetMaps() {
				
				}
				
				
				// displays preset options, including loading and saving 
				protected void renderPresets ()
				{
						popupData = CNodeManager.presets.buildData (popupData, this);
						//GUILayout.Label ("Presets:");
						if (popupData.vals.Length!=0) {
							GUILayout.BeginHorizontal();
							GUILayout.Label(" Preset:", GUILayout.Width(labelSize));
							popupData.index = EditorGUILayout.Popup (popupData.index, popupData.vals,GUILayout.Width(remainingSize));
							GUILayout.EndHorizontal();
							size.y += LStyle.FontSize * 1; //3 for presets
				
						}
							
						if (popupData.index != popupData.newIndex) {
								popupData.newIndex = popupData.index;
								popupData.getCurrentPreset ().CopyTo (parameters);
								popupData.name = popupData.getCurrentPreset ().Name;
								keys = rebuildDisplayParams ();
								PropagateChange();
						}
						if (!editPresets)
							return;
						popupData.name = GUILayout.TextField (popupData.name,GUILayout.Width(rightSize.x));
			
						// Deciding!
						
			if (GUILayout.Button ("Save preset",GUILayout.Width(rightSize.x))) {
								CNodeManager.presets.AddPreset (this, parameters, popupData.name);
								CNodeManager.presets.Save (LStyle.PresetsFilename);
						}
			if (GUILayout.Button ("Delete preset",GUILayout.Width(rightSize.x))) {
								CNodeManager.presets.RemovePreset (popupData.getCurrentPreset ());
								popupData.index = -1;
								CNodeManager.presets.Save (LStyle.PresetsFilename);
						}
						
						size.y += LStyle.FontSize * 3; //3 for presets

				}
			
				protected void InitializeWindow(int windowID, int type, int x, int y, string name) {
					window = new Rect (x, y, size.x, size.y);
					ID = windowID;
					Type = type;
					Name = name;
			
				}

				protected List<string> rebuildDisplayParams ()
				{
						displayParameters.Clear ();
						List<string> s = new List<string>();
						foreach (DictionaryEntry e in parameters) {
								displayParameters.Add ((Parameter)e.Value);
								s.Add((string)e.Key);
							}
		
		
						displayParameters.Sort (SortByIndex);
						s.Sort (SortByNameString);
						return s;
			
				}
				// standard random seed used for various perlin noise methods
				public static float getSeed (float s)
				{
						return s * 152.135f;
				}
				
				public void verifyAll() {
					Verify();
					if (!verified) {
						allVerified = false;
						return;
					}
					foreach (CConnection c in Inputs)
						if (c.pointer!=null)
						c.pointer.parent.verifyAll();
				}

			
				private Color displayColor = new Color(1,1,1);
	
				// automatically builds the node window
				protected void buildGUI (string lbl)
				{
					setColor();
			
			if (SurfaceEditor.mouseUp!=true) {
				if (Event.current.type == EventType.MouseUp)
					SurfaceEditor.mouseUp = true;
			}
			
						largeFont.fontSize = LStyle.LargeFontSize;
						largeFont.normal.textColor = color;
						//setColor ();
						largeFont.alignment = TextAnchor.UpperCenter;
						labelSize = 65;
						remainingSize = (int)(rightSize.x-labelSize) - 5;
						float d = 30; // pos of window
						/*if (!SurfaceEditor.settings.usesUnityDarkTheme) {
							GUI.contentColor = Color.black;
						}*/
						if (GUI.Button (new Rect (d, 15, rightSize.x - 1.5f*d, 32), "")) {
								Verify ();
								if (verified) {
										// only allowed to click when not active
										if (CNodeManager.status == CNodeManager.STATUS_NONE) {
												CNodeManager.status = status_after_click;

												calculateProgressDelta (CNodeManager.size);
												CNodeManager.calculator = this;
										}
								} else 
										CNodeManager.DisplayError (clickErrorMessage);
						}
						size.y = 50;
						// main label
						if (debug)
							lbl = "" + ID;
						GUILayout.Label (lbl, largeFont, GUILayout.Width(rightSize.x));
						GUILayout.Space (5);
						if (displayParameters == null) {
								displayParameters = new List<Parameter> ();
								keys = rebuildDisplayParams ();
						}
						int current = 0;
						foreach (Parameter p in displayParameters) {
								size.y += LStyle.FontSize;
								if (p.label == "")
										continue;
								if (p.label.StartsWith("Color")) {
									GUILayout.BeginHorizontal (); // "Button" is cool!
									displayColor.r = p.value;
									displayColor.g = p.min;
									displayColor.b = p.max;
//									displayColor = (Color)EditorGUILayout.ObjectField(p.label, displayColor, typeof(Color), false);
									GUILayout.Label (p.label.Substring(5), GUILayout.Width (labelSize));
									Color c = GUI.backgroundColor;
									GUI.backgroundColor = Color.white;
									displayColor = EditorGUILayout.ColorField(displayColor, GUILayout.Width(remainingSize));
									GUI.backgroundColor = c;
									if (displayColor.r != p.value || displayColor.g != p.min || displayColor.b != p.max)
										PropagateChange();
									p.value = displayColor.r;
									p.min = displayColor.g;
									p.max = displayColor.b;
					
						
									GUILayout.EndHorizontal ();
								}
								else
								if (p.label.StartsWith("string")) {
									string s2 = p.key;
									if (s2 == null || s2.Length==0)
										s2 = keys[current];
									GUILayout.Label ( s2, GUILayout.Width (remainingSize + labelSize));
					
									GUILayout.BeginHorizontal (); // "Button" is cool!
									string org = p.label;
									string s = p.label.Substring(6);
					
									p.label = "string" + GUILayout.TextField (s, GUILayout.Width (remainingSize + labelSize));
									if (!p.label.Equals(org))
										PropagateChange();
									GUILayout.EndHorizontal ();
								
								}

								else
								{
									GUILayout.BeginHorizontal (); // "Button" is cool!
									GUILayout.Label (" "  +p.label, GUILayout.Width (labelSize));
									float f = p.value;
									p.value = GUILayout.HorizontalSlider (p.value, p.min, p.max, GUILayout.Width(remainingSize-28));
									GUILayout.Label ("" + p.value, GUILayout.Width (28));

									if (p.value != f)  {
										PropagateChange ();
										}

									GUILayout.EndHorizontal ();
							}
								current++;
						}
					if (types.Length>1 && displayTypes) {				
							int t = Type;
							size.y+=LStyle.FontSize;
							Type = EditorGUILayout.Popup (Type, types,GUILayout.Width(rightSize.x));
							if (t != Type) {
								PropagateChange();
								CNodeManager.mouseUp = true;
								//Debug.Log (t + " and " + Type);
								}
						}
			
						// close and help buttons
			if (GUI.Button (new Rect (rightSize.x - 20, 15, 25, 32), "X"))
								CNodeManager.deleteNode (this);
						if (GUI.Button (new Rect (5, 15, 25, 32), "?")) {
								HelpEditor.Create (helpMessage);                         
						}
						window.height = size.y;
				}

				public virtual void Initialize (int windowID, int type, int x, int y)
				{
				}

				public virtual void Draw (int ID)
				{
						Verify ();
				}				

				// Virtual function for calculating the actual node
				public virtual void Calculate ()
				{
				
//					if (CNodeManager.window!=null)
//						CNodeManager.window.Repaint();
				}	

				public void MirrorParameters(CNode other) {
					foreach (string p in parameters.Keys) {
						Parameter org = (Parameter)other.parameters[p];
						Parameter copy = (Parameter)parameters[p];
						//Debug.Log ("Copying :" + p);						
						if (org != null && copy != null) {
							copy.value = org.value;
							if (org.label.StartsWith("Color")) {
								copy.max = org.max;
								copy.min = org.min;
							}
						}
					}
					
				}

				// sets up connection ids for loading/saving
				public void SetupID ()
				{
						foreach (CConnection c in Inputs) {
								c.parent = this;
								c.setupID ();
						}
						foreach (CConnection c in Outputs) {
								c.parent = this;
								c.setupID ();
						}
						foreach (CConnection c in Bottoms) {
								c.parent = this;
								c.setupID ();
						}
						foreach (CConnection c in Tops) {
								c.parent = this;
								c.setupID ();
						}
				}
				// prepare node for deletion
				public void BreakLinks ()
				{
						foreach (CConnection c in Inputs) 
								c.Break ();
						foreach (CConnection c in Outputs) 
								c.Break ();
						foreach (CConnection c in Bottoms) 
								c.Break ();
				}
				// Render connections of a specific type (top/bottom/input/output)
				private void RenderConnectionType (ArrayList list, Vector2 pos, Vector2 deltaScale, int sx, int sy, string str)
				{

						float w = window.width;
						float h = window.height;
						if (debug) { sx*=3; ;}
			
						float dx = w / (float)(list.Count + 1);
						float dy = h / (float)(list.Count + 1);
						float X = dx - sx / 2;
						float Y = dy - sy / 2;
						foreach (CConnection c in list) {
								c.setupID ();
								GUI.color = LStyle.connectionColors [c.Type];
								GUI.backgroundColor = LStyle.connectionColors [c.Type];
				
								c.position.x = pos.x + X * deltaScale.x + sy / 2;
								c.position.y = pos.y + Y * deltaScale.y + sy / 2;
								if (debug) {
									str = "" + c.ID;
									if (GUI.Button (new Rect (pos.x + X * deltaScale.x-sx/3, pos.y + Y * deltaScale.y, sx, sy), str) && CNodeManager.status == CNodeManager.STATUS_NONE) {
										CNodeManager.connection = c.Click (CNodeManager.connection);
										CNodeManager.status = CNodeManager.STATUS_TASK3;
									}
								}
								else
								if (GUI.Button (new Rect (pos.x + X * deltaScale.x, pos.y + Y * deltaScale.y, sx, sy), str) && CNodeManager.status == CNodeManager.STATUS_NONE) {
										CNodeManager.connection = c.Click (CNodeManager.connection);
										CNodeManager.status = CNodeManager.STATUS_TASK3;
								}
			
								X += dx;
								Y += dy;
						}
						setColor();
//						GUI.color = color;

				}

				public void RenderConnections ()
				{
						int sx = LStyle.ConnectionTypeX;
						int sy = LStyle.ConnectionTypeY;
						RenderConnectionType (Inputs, new Vector2 (window.x - sx, window.y), new Vector2 (0, 1), sx, sy, ">");
						RenderConnectionType (Outputs, new Vector2 (window.x + window.width, window.y), new Vector2 (0, 1), sx, sy, ">");
						RenderConnectionType (Bottoms, new Vector2 (window.x, window.y + window.height), new Vector2 (1, 0), sx, sy, "|");
						RenderConnectionType (Tops, new Vector2 (window.x, window.y - sy), new Vector2 (1, 0), sx, sy, "|");
				}
				// Sets default normal + non-verified colors
				public void setColor ()
				{
						float s = 0.75f;
						errorColor.r = s;
						errorColor.g = s;
						errorColor.b = s;
						
						errorColor = color * 0.1f + errorColor*0.9f;
						errorColor.a = 1.0f;
			
						if (verified) {
								GUI.color = color;
								GUI.backgroundColor = color;
								}
						else   {
								GUI.color = errorColor;
								GUI.backgroundColor = errorColor;
								}

//						if (changed)
//							GUI.color = Color.blue;

				}
				// Really not in use right now
				public void MouseMenu (Event evt, Vector2 mousePos)
				{
						if (evt.type != EventType.ContextClick)
								return;

						{
						}

				}
				
				
				
				// Default verify method. Can be overridden.
				public virtual void Verify ()
				{
						verified = true;

						foreach (CConnection c in Inputs)
								if (c.pointer == null)
										verified = false;

						setupAlternativeNames ();
				}



		}
}
