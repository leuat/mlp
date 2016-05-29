using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.Threading;
using System.IO;
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
		* MenuType is used for automatic generation of mouseclick-menu.
		*
		*/
		public class MenuType
		{
				public System.Type type = null;
				public string name;
				public int val = 0;
				public bool constrained = false;
				public string message = "";

				public MenuType (string n, System.Type t, int v, bool c, string msg)
				{
						name = n;
						type = t;
						val = v;
						constrained = c;
						message = msg;
				}

				public MenuType (string n, System.Type t, int v)
				{
						name = n;
						type = t;
						val = v;
						constrained = false;
				}
		}
		
		
		
		
		
		/*
		*
		* Main editorwindow class. Contains list of nodes + links + maintains all windows. 
		*
		*/
		public class CNodeManager : EditorWindow
		{
				// Nodes, links and menu items
				protected ArrayList nodes = new ArrayList ();
				protected ArrayList links = new ArrayList ();
				protected List<MenuType> menuItems = new List<MenuType> ();
				// if true, forces a quick repaint.
				public static bool ForceRepaint = false;
				// current connection and link - for temp rendering etc
				public static CConnection connection = null;
				public static CLink link = null;
				protected bool inRendering = false;
				// set by anyone to increase the progress bar
				public static float progress = 0;
				public static float progressDelta = 0;
				public static EditorWindow window = null;


		
										
														
				// mouse menu
				protected GenericMenu menu = new GenericMenu ();
				
				// main presets class - for loading and setting node presets
				public static Presets presets = new Presets ();
				protected int currentPreset = -1;
				
				// pointer to node clicked
				public static CNode calculator = null;
				
				// are the nodes hidden?
				protected bool renderNodes = true;
				public static bool mouseUp = false;
		
				
				// dynamic value of the size of the nodes
				static public int size;
				
				// internal status possibilities
				static public  int status = STATUS_NONE;
				static public  int STATUS_NONE = 0;
				static public  int STATUS_CHANGED = 1;
				static public  int STATUS_START_THREAD = 2;
				static public  int STATUS_IN_THREAD = 3;
				static public  int STATUS_THREAD_DONE = 4;
				static public  int STATUS_TASK1 = 5;
				static public  int STATUS_TASK2 = 6;
				static public  int STATUS_TASK3 = 7;
				static public  int STATUS_TASK4 = 8;
				static public  int STATUS_TASK5 = 9;
		
				static protected int activeNodes = 0;
				
				// if set, schedules object for deletion
				static private object deleteObject = null;
				// thread. great name!
				private Thread calculatorThread = null;
				
				// initialization
				protected bool initialized = false;
				protected Event evt;
				
				// mouse delta
				protected Vector2 delta = new Vector2 ();
				public static Vector2 mousePos;
				protected  Vector2 mousePosOld;

				// Finds a node list of a specific cnode class 
				public static ArrayList findType (ArrayList list, System.Type t)
				{
						ArrayList rlist = new ArrayList ();
						foreach (CNode n in list)
								if (n.GetType () == t)
										rlist.Add (n);

						return rlist;
				}
				// Sets all to changed
				protected virtual void ResetState ()
				{
						//status = STATUS_CHANGED;
						foreach (CNode cn in nodes) {
								cn.Verify ();
								cn.changed = true;
			
						}
					mouseUp = true;
				}
				// calculates window ID
				protected int findNextN ()
				{
						int i = 0;
						foreach (CNode n in nodes)
								if (n.ID >= i)
										i = n.ID;
						return i + 1;
				}

				public void startThread ()
				{
				
					if (calculatorThread!=null && calculatorThread.IsAlive) {
						return;
						}
					calculatorThread = new Thread (threadedUpdate);
					calculatorThread.Start();
					
					
					
//						Assemble();
				}
	
		
				public void KillThread() {
					if (calculatorThread == null) {
						return;
					}
			//if (calculatorThread.IsAlive) {
						calculatorThread.Abort();
						status = STATUS_NONE;
						// Have to wait for thread to end
						System.Threading.Thread.Sleep(25);					//}
				}
		

				public CNodeManager ()
				{

				}
				// schedules object for deletion
				public static void deleteNode (object obj)
				{
						deleteObject = obj;
				}

				public  void deleteNodeInternal (object obj)
				{
						((CNode)obj).BreakLinks ();
						nodes.Remove ((CNode)obj);
						//ResetState ();
				}

				public virtual void PreAssemble ()
				{
				}

				public void Assemble ()
				{
						status = STATUS_IN_THREAD;
						PreAssemble ();
						
						if (calculator != null)
								calculator.Calculate ();
						
						status = STATUS_THREAD_DONE;
		
				}
	
				public void CountChildNodes(CNode n) {
					CNode.childrenCount = 0;
					foreach (CNode c in nodes)
						c.counted = false;
					n.getInputsChildren();
					float cc = CNode.childrenCount-1;
					if (cc==0)
						cc=1;
					progressDelta = 1.0f/(float)(cc);
				}
	
				private void buildConnections ()
				{
						links.Clear ();
						foreach (CNode n in nodes) {
							n.SetupLinks (links);
						}
				}

				public virtual void SetupGUI ()
				{


				}

				public virtual void Create ()
				{
	
				}
				// called in thread
				void threadedUpdate ()
				{
						Assemble ();
				}

				protected void buildMouseMenu ()
				{
						menu.ShowAsContext ();
				}

				public virtual void setupMenu ()
				{
						menu = new GenericMenu ();
						foreach (MenuType mi in menuItems) {
								if (mi.type == null)
										menu.AddSeparator (mi.name);
								else
										menu.AddItem (new GUIContent (mi.name), false, CreateNode, mi);
						}

						setupSnippets ();
				}

				protected void setupSnippets ()
				{
						DirectoryInfo dir = new DirectoryInfo (LStyle.SnippetDirectory);
						FileInfo[] info = dir.GetFiles ("*.tex");
						foreach (FileInfo f in info) {
								menu.AddItem (new GUIContent ("Snippets/" + f.Name), false, LoadSnippet, f.FullName);
						}
				}

				protected void LoadSnippet (object obj)
				{
						string name = (string)obj;
						CSerializedNode.LoadSnippet (name, nodes, findNextN ());

				}
 
				public virtual void Initialize ()
				{
						presets.Load (LStyle.PresetsFilename);
						setupMenu ();
				}

				public virtual void ResetMaps ()
				{
						foreach (CNode cn in nodes) {
								cn.resetMaps();
								cn.changed = true;
						}

				}

				protected void InternalUpdate ()
				{
						if (status == STATUS_THREAD_DONE) {
								Create ();
						}
		
						if (!initialized) {
								Initialize ();
						}
						if (deleteObject != null) {
								deleteNodeInternal (deleteObject);
								deleteObject = null;
						}
						if (ForceRepaint) {
								Repaint ();
								ForceRepaint = false;
						}
				}
	
				private void moveWindow (float dx, float dy)
				{
						foreach (CNode n in nodes) {
								n.window.x += dx;
								n.window.y += dy;
						}
				}

				private void dragBackground ()
				{
						if (Event.current.type == EventType.mouseDrag && Event.current.modifiers == EventModifiers.Shift) {
								{
										moveWindow (mousePos.x - mousePosOld.x, mousePos.y - mousePosOld.y);
										Repaint ();
								}
						}

				}

				public static void Progress ()
				{
						progress += progressDelta;
				}
				public static void Progress (float d)
				{
						progress += progressDelta*d;
				}

				private void mouseClick ()
				{
						if (Event.current.type == EventType.mouseUp && Event.current.modifiers != EventModifiers.Shift) {
								{
										link = null;
										if (connection != null) {
												connection.Break ();
												connection = null;
										}
								}
						}
		
				}

				public virtual void MouseBackgroundMenu ()
				{
						Rect contextRect = position;
						delta.x = position.x;
						delta.y = position.y;
						foreach (CNode c in nodes) {
								c.MouseMenu (evt, mousePos);
						}
						if (evt.type == EventType.ContextClick) {
								if (contextRect.Contains (mousePos + delta)) {
										buildMouseMenu ();
										evt.Use ();
								}
						}

				}

				protected void Render ()
				{
						inRendering = true;
						dragBackground ();
						evt = Event.current;
						mousePosOld = mousePos;
						mousePos = evt.mousePosition;


						SetupGUI ();
						// renders all windows and connections
						if (renderNodes) {
								Handles.BeginGUI ();
								if (link != null)
									link.Draw ();

								buildConnections ();
								foreach (CLink l in links) {
										l.Draw ();
								}
								Handles.EndGUI ();
		
		
								BeginWindows ();
								foreach (CNode c in nodes) {
										c.RenderConnections ();
										c.window = GUI.Window (c.ID, c.window, c.Draw, c.Name);
								}
								EndWindows ();
	
								MouseBackgroundMenu ();
						}

						mouseClick ();
						inRendering = false;

				}

				public bool VerifyNodes(CNode n) {
					CNode.allVerified = true;
					if (n==null)
						return false;
					n.verifyAll();
					return CNode.allVerified;;
				}
		
				protected void LoadDirect (string name)
				{
		
						CSerializedNode.Load (name, nodes);
						Initialize ();
				}
	
				public static void DisplayError (string message)
				{
						EditorUtility.DisplayDialog ("Warning", message, "OK");
				}
				
				// generic node creation
				protected void CreateNode (object obj)
				{
						MenuType mt = (MenuType)obj;
						System.Type t = (System.Type)mt.type;

						if (mt.constrained) {
								ArrayList l = findType (nodes, t);
								if (l.Count != 0) {
										EditorUtility.DisplayDialog ("Error",
				                            mt.message, "Got it");
										return;
								}
						}
						
						CNode n = (CNode)System.Activator.CreateInstance (t);
						n.Initialize (findNextN (), mt.val, (int)mousePos.x, (int)mousePos.y);
						nodes.Add (n);
						//ResetState ();
				}

				public void RefreshNodesNew() {
					foreach (CNode n in nodes) {
							System.Type t = n.GetType();
							CNode newNode = (CNode)System.Activator.CreateInstance (t);
							newNode.Initialize (findNextN (), n.Type, (int)mousePos.x, (int)mousePos.y);
							newNode.MirrorParameters(n);
							n.parameters = newNode.parameters;
							n.displayParameters = null;	
						}
				
				}

		}
}
