﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;
using System.Reflection;
using System;
using UnityEngine.UI;

namespace LemonSpawn {





	public class AtmosphereType {
		public Vector3 values;
		public string name;
		public AtmosphereType(string n, Vector3 v) {
			name = n;
			values = v;
		}

        public static AtmosphereType[] AtmosphereTypes = new AtmosphereType[] {
           new AtmosphereType("NORMAL", new Vector3(0.65f, 0.57f, 0.475f)), // CONFIRMED NORMAL
			new AtmosphereType("BLEAK", new Vector3(0.6f, 0.6f, 0.6f)),    // BLEAK 
			new AtmosphereType("RED",new Vector3(0.5f, 0.62f, 0.625f)), // CONFIRMED RED
			new AtmosphereType("CYAN", new Vector3(0.65f, 0.47f, 0.435f)), // CONFIRMED CYAN
			new AtmosphereType("GREEN",new Vector3(0.60f, 0.5f, 0.60f)),  // CONFIRMED GREEN
			new AtmosphereType("PURPLE",new Vector3(0.65f, 0.67f, 0.475f)),   //  Confirmed PURPLE 
			new AtmosphereType("AYELLOW",new Vector3(0.5f, 0.54f, 0.635f)),   // CONFIRMED YELLOW
			new AtmosphereType("PINK",new Vector3(0.45f, 0.72f, 0.675f)) // CONFIRMED PINK
        };

        public static Vector3 getAtmosphereValue(string n)
        {
            foreach (AtmosphereType at in AtmosphereTypes)
                if (at.name.ToLower() == n.ToLower())
                    return at.values;

            return Vector3.zero;
        }
    }

	[System.Serializable]
	public class SettingsType {
		public string name;
		public string stringValue;
		public string propName;
		public int index;
		public string info;
        public string group;
		public float lower;
		public float upper;
		public Vector3 minMax = new Vector3(); // Min / max max
		public float realizedValue;
		public int type; 

		public static int STRING = 0;
		public static int NUMBER = 1;
		public static int BOOL = 2;


		public SettingsType() {

		}

		public SettingsType(string _name, int _type, string _propname, string _info, Vector3 _minMax, Vector3 init, string _group, int _index = 0) {
			name = _name;
			type = _type;
			propName = _propname;
			minMax = _minMax;
			info = _info;
			index = _index;
            group = _group;

			lower = init.x;
			upper = init.y;
			realizedValue = init.z;
		}


		public float Realize(System.Random r) {
			float span = upper - lower;
            if (span < 0.0001f)
                return lower;
			realizedValue = (float)r.NextDouble()*span + lower;
			return realizedValue;
		}

		public float getValue() {
			return realizedValue;
		}

		public void setParameter(PlanetSettings ps) {
            if (ps == null)
                return;
			object o = Util.GetPropertyValue(ps, propName);	
			if (o is Color) {
				Color v = (Color)o;
				v[index] = realizedValue;
				Util.SetPropertyValue(ps, propName, v);
				}
			if (o is Vector3) {
				Vector3 v = (Vector3)o;
               // Debug.Log("Index: " + index + " of " + name);
				v[index] = realizedValue;
				Util.SetPropertyValue(ps, propName, v);
				}
			if (o is float) {
				Util.SetPropertyValue(ps, propName, realizedValue);
				}
			if (o is double) {
				Util.SetPropertyValue(ps, propName, realizedValue);
				}
			if (o is int) {
				Util.SetPropertyValue(ps, propName, realizedValue);
				}
            if (o is string)
            {
                Util.SetPropertyValue(ps, propName, stringValue);
            }
        }





    }
	[System.Serializable]
	public class SettingsTypes {
		public string name = "New planet";

		public List<SettingsType> settingsTypes = new List<SettingsType>();
        public List<string> groups = new List<string>();

        public void InitializeNew() {

			settingsTypes.Add(
				new SettingsType("Atmosphere density",
				SettingsType.NUMBER,
				"atmosphereDensity", 
				"1 = thick, 0 = none", 
				new Vector3(0,1,0),
                new Vector3(0.65f, 0.75f, 0.85f),
                "Atmosphere"));

			settingsTypes.Add(
				new SettingsType("Outer radius scale",
				SettingsType.NUMBER,
				"outerRadiusScale", 
				"Best is 1.025", 
				new Vector3(1,1.05f,1.025f),
                new Vector3(1.024f,1.026f, 1.025f),
                "Atmosphere"));

			settingsTypes.Add(
				new SettingsType("HDR Exposure",
				SettingsType.NUMBER,
				"m_hdrExposure", 
				"HDR exposure of atmosphere: More lets in a lot of sun", 
				new Vector3(0,5,1.5f),
                new Vector3(1.4f, 1.6f, 1.5f),
                "Atmosphere"));

            settingsTypes.Add(
                new SettingsType("Atmosphere color",
                SettingsType.STRING,
                "atmosphereString",
                "Variations: normal, cyan, green, yellow, etc. Can be separated by comma, will choose one by random. ",
                new Vector3(0, 5, 1.5f),
                new Vector3(1.4f, 1.6f, 1.5f),
                "Atmosphere"));

            settingsTypes.Add(
				new SettingsType("Lacunarity",
				SettingsType.NUMBER,
				"ExpSurfSettings", 
				"Lacunarity defines the overall shape of the fractal, high lacunarity = lots of \"lakes\"", 
				new Vector3(1.5f,4,2.5f),
                new Vector3(2.45f, 2.55f, 2.5f),

                "Surface", 0));

			settingsTypes.Add(
				new SettingsType("Offset",
				SettingsType.NUMBER,
				"ExpSurfSettings", 
				"Offset", 
				new Vector3(-3,3,0.65f),
                new Vector3(0.60f, 0.7f, 0.65f),

                "Surface", 1));

			settingsTypes.Add(
				new SettingsType("Gain",
				SettingsType.NUMBER,
				"ExpSurfSettings", 
				"Gain", 
				new Vector3(-3,3,1.7f),
                new Vector3(1.65f, 1.75f, 1.7f),

                "Surface", 2));

			settingsTypes.Add(
				new SettingsType("Initial offset",
				SettingsType.NUMBER,
				"ExpSurfSettings2", 
				"Initial offset", 
				new Vector3(-1,1,-0.5f),
                new Vector3(-0.55f,-0.45f, -0.5f),

                "Surface", 0));

			settingsTypes.Add(
				new SettingsType("Surface height",
				SettingsType.NUMBER,
				"ExpSurfSettings2", 
				"Height surface multiplier", 
				new Vector3(0,0.5f,0.01f),
                new Vector3(0.01f, 0.02f, 0.01f),

                "Surface", 1));

			settingsTypes.Add(
				new SettingsType("Surface scale",
				SettingsType.NUMBER,
				"ExpSurfSettings2", 
				"Surface scale", 
				new Vector3(0,50,5),
                new Vector3(3f,10f,5f),
                "Surface", 2));

            settingsTypes.Add(
                new SettingsType("Vortex 1 scale",
                SettingsType.NUMBER,
                "SurfaceVortex1",
                "Vortex 1 scale",
                new Vector3(-50, 50, 5),
                new Vector3(3f, 7f, 5f),
                "Surface", 2));

            settingsTypes.Add(
                new SettingsType("Vortex 1 amplitude",
                SettingsType.NUMBER,
                "SurfaceVortex1",
                "Vortex 1 amplitude",
                new Vector3(0, 10, 1),
                new Vector3(0.9f, 1.1f, 1f),
                "Surface", 2));


             settingsTypes.Add( 
                new SettingsType("Vortex 2 scale",
                SettingsType.NUMBER,
                "SurfaceVortex2",
                "Vortex 2 scale",
                new Vector3(-50, 50, 5),
                new Vector3(3f, 7f, 0f),
                "Surface", 2));

            settingsTypes.Add(
                new SettingsType("Vortex 2 amplitude",
                SettingsType.NUMBER,
                "SurfaceVortex2",
                "Vortex 2 amplitude",
                new Vector3(0, 10, 1),
                new Vector3(0.9f, 1.1f, 0f),
                "Surface", 2));


        }


        public void Realize(System.Random r) {
			foreach (SettingsType st in settingsTypes)
				st.Realize(r);
		}

		public void setParameters(PlanetSettings ps, System.Random r) {
			foreach (SettingsType st in settingsTypes)
				st.setParameter(ps);;

            ps.UpdateParameters(r);	
		}


        public SettingsType findParameter(string name)
        {
            foreach (SettingsType st in settingsTypes)
                if (st.name.ToLower() == name.ToLower())
                    return st;

            return null;
        }


        public SettingsType getSettingsFromDropdown(string box)
        {
            Dropdown d = GameObject.Find(box).GetComponent<Dropdown>();
            int idx = d.value;
            string name = d.options[idx].text;
            return findParameter(name);

        }

        private void buildGroupList()
        {
            groups.Clear();
            foreach (SettingsType st in settingsTypes)
            {
                bool isNew = true;
                foreach (String s in groups)
                    if (s.ToLower() == st.group.ToLower())
                        isNew = false;

                if (isNew)
                    groups.Add(st.group);

            }

        }

        public void PopulateGroupsDrop(string box)
        {
            Dropdown cbx = GameObject.Find(box).GetComponent<Dropdown>();
            buildGroupList();
           
            cbx.ClearOptions();
            List<Dropdown.OptionData> l = new List<Dropdown.OptionData>();

            for (int i = 0; i < groups.Count; i++)
            {
                ComboBoxItem ci = new ComboBoxItem();
                l.Add(new Dropdown.OptionData(groups[i]));
            }

            cbx.AddOptions(l);

            cbx.value = 0;

         }

        public void MaintainUpdatedParameters()
        {
            SettingsTypes set = new SettingsTypes();
            set.InitializeNew();

            // First, add new settings types
            foreach (SettingsType newSt in set.settingsTypes)
            {
                SettingsType found = null;
                foreach (SettingsType st in settingsTypes)
                    if (st.propName == newSt.propName)
                        found = st;

                if (found==null)
                    settingsTypes.Add(newSt);
                else
                {
                    // Update params, minmax etc
                    found.info = newSt.info;
                    found.propName = newSt.propName;
                    found.minMax = newSt.minMax;
                }


            }


        }



        public void PopulateSettingsDrop(string box, string group)
        {
            Dropdown cbx = GameObject.Find(box).GetComponent<Dropdown>();

            cbx.ClearOptions();
            List<Dropdown.OptionData> l = new List<Dropdown.OptionData>();

            foreach (SettingsType st in settingsTypes)
            {
                if (st.group.ToLower() == group.ToLower())
                {
                    ComboBoxItem ci = new ComboBoxItem();
                    l.Add(new Dropdown.OptionData(st.name));
                }
            }

            cbx.AddOptions(l);
            cbx.value = 0;

        }


    }


    [System.Serializable]
	public class PlanetTypes {
		public List<SettingsTypes> planetTypes = new List<SettingsTypes>();


		public SettingsTypes NewPlanetType() {
			SettingsTypes st = new SettingsTypes();
			st.InitializeNew();
			planetTypes.Add(st);
            currentSettings = st;
			return st;
		}

        public void MaintainNewParameters()
        {
            foreach (SettingsTypes pt in planetTypes)
                pt.MaintainUpdatedParameters();
        }

        public static SettingsTypes currentSettings = null;

        public void PopulatePlanetTypesDrop(string box, int type)
        {
            Dropdown cbx = GameObject.Find(box).GetComponent<Dropdown>();

            cbx.ClearOptions();
            List<Dropdown.OptionData> l = new List<Dropdown.OptionData>();

            foreach (SettingsTypes st in planetTypes)
            {
                ComboBoxItem ci = new ComboBoxItem();
                 l.Add(new Dropdown.OptionData(st.name));
            }
            cbx.options = l;
            if (type==0)
                cbx.value = 0;
            if (type == 1)
                cbx.value = l.Count - 1;

        }

        public SettingsTypes FindPlanetType(int idx)
        {
            return planetTypes[idx];
        }

        public static PlanetTypes DeSerialize(string filename)
        {
			XmlSerializer deserializer = new XmlSerializer(typeof(PlanetTypes));
            TextReader textReader = new StreamReader(filename);
			PlanetTypes sz = (PlanetTypes)deserializer.Deserialize(textReader);
            textReader.Close();
            return sz;
        }
		static public void Serialize(PlanetTypes sz, string filename)
        {
			XmlSerializer serializer = new XmlSerializer(typeof(PlanetTypes));
            TextWriter textWriter = new StreamWriter(filename);
            serializer.Serialize(textWriter, sz);
            textWriter.Close();
        }


        public static PlanetTypes p = new PlanetTypes();

		public static void Initialize() {

			if (!File.Exists(RenderSettings.planetTypesFilename)) {
				World.FatalError("Could not find planet types file: " + RenderSettings.planetTypesFilename);
				return;
			}

			p = DeSerialize(RenderSettings.planetTypesFilename);
            p.MaintainNewParameters();
		}

        public static void Save()
        {
            Serialize(p, RenderSettings.planetTypesFilename);
        }


    }

}
/*
}


	[System.Serializable]
    public class PlanetType {
        public string Name;
        public Color color;
        public Color colorVariation;
        public Color basinColor;
        public Color basinColorVariation;
        public Color seaColor;
        public Color topColor;
        public string[] atmosphere;
        public string cloudType = "";
        public Color cloudColor;
        public string clouds;
        public float atmosphereDensity = 1;
        public float sealevel;
		public string delegateString;

		public float surfaceScaleModifier = 1;
		public float surfaceHeightModifier = 1;


        public int minQuadLevel = 2;
        public Vector2 RadiusRange, TemperatureRange;

//		[System.NonSerialized]
		public delegate SurfaceNode InitializeSurface(float a, float scale, PlanetSettings ps);

		[XmlIgnore]		
		public InitializeSurface Delegate;
    	[field: System.NonSerialized] 
		Dictionary<string, InitializeSurface> calls = new Dictionary<string, InitializeSurface>
		{
			{"Surface.InitializeNew",Surface.InitializeNew}, 
			{"Surface.InitializeDesolate",Surface.InitializeDesolate}, 
			{"Surface.InitializeMoon",Surface.InitializeMoon}, 
			{"Surface.InitializeFlat",Surface.InitializeFlat}, 
			{"Surface.InitializeMountain",Surface.InitializeMountain}, 
			{"Surface.InitializeTerra2",Surface.InitializeTerra2}, 
		};


		public void setDelegate() {
			Delegate = calls[delegateString];
		}
        public PlanetType() {
        }
        public PlanetType(string del, string n, Color c, Color cv, Color b, Color bv, Color topc, string cl, Vector2 rr, Vector2 tr, int mq, float atm,
            float seal, Color seacol, string[] atmidx) {
			delegateString = del;
			Name = n;
			color = c;
			colorVariation = cv;
			clouds = cl;
			basinColor = b;
			basinColorVariation = bv;
			RadiusRange = rr;
			atmosphereDensity = atm;
			TemperatureRange = tr;
			minQuadLevel = mq;
            atmosphere = atmidx;
            seaColor = seacol;
            sealevel = seal;
            topColor = topc;
            setDelegate();

		}
        public static AtmosphereType[] AtmosphereTypes = new AtmosphereType[] {
           new AtmosphereType("ATM_NORMAL", new Vector3(0.65f, 0.57f, 0.475f)), // CONFIRMED NORMAL
			new AtmosphereType("ATM_BLEAK", new Vector3(0.6f, 0.6f, 0.6f)),    // BLEAK 
			new AtmosphereType("ATM_RED",new Vector3(0.5f, 0.62f, 0.625f)), // CONFIRMED RED
			new AtmosphereType("ATM_CYAN", new Vector3(0.65f, 0.47f, 0.435f)), // CONFIRMED CYAN
			new AtmosphereType("ATM_GREEN",new Vector3(0.60f, 0.5f, 0.60f)),  // CONFIRMED GREEN
			new AtmosphereType("ATM_PURPLE",new Vector3(0.65f, 0.67f, 0.475f)),   //  Confirmed PURPLE 
			new AtmosphereType("ATM_YELLOW",new Vector3(0.5f, 0.54f, 0.635f)),   // CONFIRMED YELLOW
			new AtmosphereType("ATM_PINK",new Vector3(0.45f, 0.72f, 0.675f)) // CONFIRMED PINK
        };

        public static Vector3 getAtmosphereValue(string n) {
        	foreach (AtmosphereType at in AtmosphereTypes)
        		if (at.name.ToLower() == n.ToLower())
        			return at.values;

        	return Vector3.zero;
        }

        public static int ATM_NORMAL = 0;
        public static int ATM_BLEAK = 1;
        public static int ATM_RED = 2;
        public static int ATM_CYAN = 3;
        public static int ATM_GREEN = 4;
        public static int ATM_PURPLE = 5;
        public static int ATM_YELLOW = 6;
        public static int ATM_PINK = 7;


		
				
	}


	[System.Serializable]
	public class PlanetTypes {
		public List<PlanetType> planetTypes = new List<PlanetType>();


		public PlanetTypes() {
			Initialize();
		}

		public void Initialize() {
        }



        public void setDelegates() {
        	foreach (PlanetType pt in planetTypes)
        		pt.setDelegate();
        }

        public PlanetType getRandomPlanetType(System.Random r, float radius, float temperature) {
			List<PlanetType> candidates = new List<PlanetType>();
			foreach (PlanetType pt in planetTypes) {
				if ((radius>=pt.RadiusRange.x && radius<pt.RadiusRange.y) && (temperature>=pt.TemperatureRange.x && temperature<pt.TemperatureRange.y))
					candidates.Add (pt);
			}
			
			if (candidates.Count==0)
				return planetTypes[1];
				
			return candidates[r.Next()%candidates.Count];
		}

		public PlanetType getPlanetType(string s) {
			foreach (PlanetType pt in planetTypes)
				if (pt.Name.ToLower() == s.ToLower())
					return pt;

			return null;
		}

        public static PlanetTypes DeSerialize(string filename)
        {
			XmlSerializer deserializer = new XmlSerializer(typeof(PlanetTypes));
            TextReader textReader = new StreamReader(filename);
			PlanetTypes sz = (PlanetTypes)deserializer.Deserialize(textReader);
            textReader.Close();
            return sz;
        }
		static public void Serialize(PlanetTypes sz, string filename)
        {
			XmlSerializer serializer = new XmlSerializer(typeof(PlanetTypes));
            TextWriter textWriter = new StreamWriter(filename);
            serializer.Serialize(textWriter, sz);
            textWriter.Close();
        }


	}

	*/