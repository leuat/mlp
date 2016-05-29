using UnityEngine;
using System.Collections;
using System.Runtime.Serialization.Formatters.Binary;
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
		* Class used for alternative labels within a cnode
		*
		* */ 
		public class AlternativeName
		{
				public string hashName;
				public string label;
				public int type;
				
				public AlternativeName (string hn, string l, int i)
				{
						label = l;
						hashName = hn;
						type = i;
				}

		}
		/*
		* Main parameter classed. Used for all parameters by cnodes. 
		*
		*/
		[System.Serializable]
		public class Parameter : ParameterOld
		{
				public string key;
				public int index;
				public Parameter (string l, float v, float mmin, float mmax, int idx, string k)
				{
						label = l;
						value = v;
						min = mmin;
						max = mmax;
						index = idx;
						key = k;
				}
				public Parameter (string l, float v, float mmin, float mmax)
				{
						label = l;
						value = v;
						min = mmin;
						max = mmax;
				}



				public new Parameter copy ()
				{
						return new Parameter (label, value, min, max, index, key);
				}
	
		}
		
	[System.Serializable]
	public class ParameterOld
	{
		public string label;
		public float value, min, max;
		public ParameterOld (){
		
		}
		public ParameterOld (string l, float v, float mmin, float mmax)
		{
			label = l;
			value = v;
			min = mmin;
			max = mmax;
		}
		
		public ParameterOld copy ()
		{
			return new ParameterOld (label, value, min, max);
		}
		
	}
	
		/*
		* Presets parameter values.
		*
		*/
		[System.Serializable]
		public class Preset
		{
				public Hashtable parameters = null;
				public int ID;
				public string NodeID;
				public int NodeType;
				public string Name;

				public Preset (CNode n, Hashtable par, string name)
				{
						Name = name;
						ID = Random.Range (0, 10000000);
						NodeID = n.GetType ().AssemblyQualifiedName;
						NodeType = n.Type;
						parameters = new Hashtable ();
						CopyFrom (par);
				}

				public void CopyFrom (Hashtable par)
				{
						parameters.Clear ();
						foreach (DictionaryEntry entry in par) {
								Parameter p = (Parameter)entry.Value;
								parameters.Add (entry.Key, p.copy ());
						}
				}

				public void CopyTo (Hashtable par)
				{
						par.Clear ();
						foreach (DictionaryEntry entry in parameters) {
								Parameter p = (Parameter)entry.Value;
								par.Add (entry.Key, p.copy ());
						}
				}

/*	public void CopyFrom(Hashtable ht) {
		foreach (

	}*/
		}

		public class PopupData
		{
				public string name = "";
				public string[] vals = null;
				public Preset[] presets = null;
				public int index = -1, newIndex = -1;

				public Preset getCurrentPreset ()
				{
						if (index >= 0 && index < presets.Length)
								return (presets [index]);
						return null;
				}
	

		}

		public class Presets
		{

				public ArrayList presets = new ArrayList ();

				public void AddPreset (CNode node, Hashtable p, string name)
				{
						presets.Add (new Preset (node, p, name));
				}

				public void RemovePreset (Preset p)
				{
						presets.Remove (p);
				}

				public void RemovePreset (int id)
				{
						Preset rem = null;
						foreach (Preset p in presets)
								if (p.ID == id)
										rem = p;
						if (rem != null)
								presets.Remove (rem);
				}

				public PopupData buildData (PopupData pd, CNode n)
				{

						if (pd == null)
								pd = new PopupData ();
						int cnt = 0;
						foreach (Preset p in presets)
								if (n.GetType ().AssemblyQualifiedName == p.NodeID && n.Type == p.NodeType)
										cnt++;


						if (pd.presets == null || pd.presets.Length != cnt) {

								pd.presets = new Preset[cnt];
								pd.vals = new string[cnt];
						}
						cnt = 0;
						foreach (Preset p in presets)
								if (n.GetType ().AssemblyQualifiedName == p.NodeID && n.Type == p.NodeType) {
										pd.presets [cnt] = p;
										pd.vals [cnt] = p.Name;
										cnt++;
								}
						return pd;

				}

				public void Load (string fname)
				{
						if (!System.IO.File.Exists (fname))
								return;
						presets.Clear ();


						using (FileStream str = File.OpenRead(fname)) {
								BinaryFormatter bf = new BinaryFormatter ();
								presets = (ArrayList)bf.Deserialize (str);
						}

				}

				public void Save (string fname)
				{
						using (FileStream str = File.Create(fname)) {
								BinaryFormatter bf = new BinaryFormatter ();
								bf.Serialize (str, presets);
						}
		
				}
		}
}