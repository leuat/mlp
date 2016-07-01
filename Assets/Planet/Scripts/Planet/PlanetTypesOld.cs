using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;


namespace LemonSpawn {






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
	public class OldPlanetTypes {
		public List<PlanetType> planetTypes = new List<PlanetType>();


        public OldPlanetTypes() {
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
/*
        public static PlanetTypes DeSerialize(string filename)
        {
			XmlSerializer deserializer = new XmlSerializer(typeof(OldPlanetTypes));
            TextReader textReader = new StreamReader(filename);
			OldPlanetTypes sz = (PlanetTypes)deserializer.Deserialize(textReader);
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
        */

	}


	}