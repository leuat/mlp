using UnityEngine;
using System.Collections;
using System.IO;

using System.Runtime.Serialization.Formatters.Binary;

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
		* Main class used for loading and saving nodes. Converts nodes + links + parameters to serializable structures.
		*
		*/
		[System.Serializable]
		public class CSerializedNode
		{
				// window ID
				public int ID; 
				// class ID
				public string CNodeType;
				public int Type = 0;
				// link to parameters
				public Hashtable parameters;
				// window position etc
				public float windowx, windowy, width, height;
				public ArrayList Outputs = new ArrayList ();
				public ArrayList Inputs = new ArrayList ();

				public CSerializedNode (CNode cn)
				{
						fromCnode (cn);
				}
				// Converts from a CNode to SerializedNode
				public void fromCnode (CNode cn)
				{
						ID = cn.ID;
						Type = cn.Type;
						CNodeType = cn.GetType ().AssemblyQualifiedName;  //cn.CnodeType;
						//Debug.Log ("Name:" + CNodeType);
						parameters = cn.parameters;
						windowx = cn.window.x;
						windowy = cn.window.y;
						width = cn.window.width;
						height = cn.window.height;

						foreach (CConnection c in cn.Outputs) {
								if (c.pointer != null) {
										Outputs.Add (c.ID);
										Inputs.Add (c.pointer.ID);
	
								}
						}

						foreach (CConnection c in cn.Bottoms) {
								if (c.pointer != null) {
										Outputs.Add (c.ID);
										Inputs.Add (c.pointer.ID);
								}
						}

				}
				// Converts to a new node. Automatic class generation. Whoo!
				public CNode toCnode (int Add)
				{
						CNode c = (CNode)System.Activator.CreateInstance (System.Type.GetType (CNodeType));
						c.Initialize (ID + Add, Type, (int)windowx, (int)windowy);

						c.SetupID ();

						c.parameters = parameters;

						return c;
				}

				public static void Save (string fname, ArrayList nodes)
				{
						ArrayList sn = new ArrayList ();
						foreach (CNode c in nodes)  
								c.SetupID ();

						foreach (CNode c in nodes)  
								sn.Add (new CSerializedNode (c));

						using (FileStream str = File.Create(fname)) {
								BinaryFormatter bf = new BinaryFormatter ();
								bf.Serialize (str, sn);
						}

				}

				private static CNode findNodeWithID (ArrayList nodes, int id)
				{
						foreach (CNode cn in nodes) 
								if (cn.ID == id)
										return cn;


						return null;

				}

				private static CConnection findConnection (int ID, ArrayList nodes)
				{
						foreach (CNode cn in nodes) {
								foreach (CConnection c in cn.Inputs)
										if (c.ID == ID)
												return c;
								foreach (CConnection c in cn.Outputs)
										if (c.ID == ID)
												return c;
								foreach (CConnection c in cn.Tops)
										if (c.ID == ID)
												return c;
								foreach (CConnection c in cn.Bottoms)
										if (c.ID == ID)
												return c;
						}
						return null;
				}


				private static void verifyConnectionID(ArrayList sn, int Add) {
					foreach (CSerializedNode s in sn) {
						for (int i=0; i<s.Outputs.Count; i++) {
						int c1 = (int)s.Outputs[i] + Add*100;
						Debug.Log("Testing output " +c1 + ":");
						foreach (CSerializedNode s2 in sn) {
							if (s==s2) 
								break;
							for (int j=0; j<s.Outputs.Count; j++) {
							
								int c2 = (int)s.Outputs[i];
								Debug.Log("  versus " +c2 + Add*100 );
							
								if (c1 == c2) {
									Debug.Log ("  ERROR! IDs IDENTICAL!" + c1 );
								 } 
							}
						}
						}
					
				}
				}

				private static void createLinks (ArrayList sn, ArrayList nodes, int Add)
				{
						//verifyConnectionID(sn, Add);
						foreach (CSerializedNode s in sn) {
								for (int i=0; i<s.Outputs.Count; i++) {
										int I = (int)s.Inputs [i] + Add * 100;
										int O = (int)s.Outputs [i] + Add * 100;
										CConnection a = findConnection (I, nodes);
										CConnection b = findConnection (O, nodes);
										if (a == null || b == null) {
												// you know. Just in case. 
												Debug.Log ("ERROR NODE NOT FOUND BY LOADING; SHOULD NEVER HAPPEN!");
												return;
												
										}
									if (a!=b) 
											a.Link (b);
								}

						}
				}

				public static void Load (string fname, ArrayList nodes)
				{
						if (!System.IO.File.Exists (fname))
								return;
						nodes.Clear ();
						ArrayList sn;

						using (FileStream str = File.OpenRead(fname)) {
								BinaryFormatter bf = new BinaryFormatter ();
								sn = (ArrayList)bf.Deserialize (str);
						}
						foreach (CSerializedNode cs in sn) {
								CNode n = cs.toCnode (0);
								if (n != null)
										nodes.Add (n);
						}
						createLinks (sn, nodes, 0);

				}

				public static void LoadSnippet (string fname, ArrayList nodes, int Add)
				{
						if (!System.IO.File.Exists (fname))
								return;
						ArrayList sn;
		
						using (FileStream str = File.OpenRead(fname)) {
								BinaryFormatter bf = new BinaryFormatter ();
								sn = (ArrayList)bf.Deserialize (str);
						}
						foreach (CSerializedNode cs in sn) {
								CNode n = cs.toCnode (Add);
								if (n != null)
										nodes.Add (n);
						}
						createLinks (sn, nodes, Add);
		
				}
		}
}
