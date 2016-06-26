using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;


namespace LemonSpawn {


	public class AtmosphereType {
		public Vector3 values;
		public string name;
		public AtmosphereType(string n, Vector3 v) {
			name = n;
			values = v;
		}
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


    // Hidden properties
    public class PlanetProperties
    {
        public int currentLayer = 10;
        public string currentTag = "Normal";
        public double currentDistance;
        public DVector pos = new DVector();
        public DVector posInKm;
        public GameObject terrainObject, parent;
        public Vector3 localCamera;
        public List<Frame> Frames = new List<Frame>();
        public Plane[] cameraPlanes;

    }
    // Public settings
    public class PlanetSettings : MonoBehaviour {

        [Header("Planet settings")]
        public double rotation;
        public float Gravity;
        public int maxQuadNodeLevel;
        public int planetTypeIndex;
        public bool castShadows = true;
        public bool hasSea = false;

        // Public stuff to be exposed
        [Header("Atmosphere settings")]
        public int seed;
        public float atmosphereDensity = 1.0f;
//        public float atmosphereHeight = 1.025f;
        public float outerRadiusScale = 1.025f;
        public Vector3 m_atmosphereWavelengths = new Vector3(0.65f, 0.57f, 0.475f);



        public float m_hdrExposure = 1.5f;
        public float m_ESun = 10.0f;            // Sun brightness constant
        public float radius = 5000;
        public float temperature = 300f;

        [Space(10)]
        [Header("Ground settings")]
        public float hillyThreshold = 0.980f;
        public float liquidThreshold = 0.0005f;
        public float topThreshold = 0.006f;
        public float basinThreshold = 0.0015f;
        public float globalTerrainHeightScale = 2.0f;
        public float globalTerrainScale = 4.0f;
        public Color m_surfaceColor, m_surfaceColor2;
        public Texture2D m_surfaceTexture;
        public Color m_basinColor, m_basinColor2;
        public Texture2D m_basinTexture;
		public Color m_topColor = new Color(0.8f, 0.8f, 0.8f, 1.0f);
        public Texture2D m_topTexture;
        public Color m_hillColor = new Color(0.3f, 0.3f, 0.3f, 1.0f);
        public Texture2D m_hillTexture;
        public Color m_waterColor = new Color(0.6f, 0.8f, 0.9f, 1.0f);
        public Color emissionColor;
        public float metallicity = 0;
        public float specularity = 0;
        public Texture2D bumpMap;

        [Space(10)]
        [Header("Environment settings")]
        public bool hasEnvironment = false;
        public int environmentDensity = 0;


        [Space(10)]
        [Header("Ring settings")]
        public bool hasRings;
        public Color ringColor = Color.white;
        public float ringScale = 1;
        public float ringAmplitude = 1;
        public Vector2 ringRadius = new Vector2(0.2f, 0.45f);

        [Space(10)]
        [Header("Cloud settings")]
        public Texture2D clouds;
        public float bumpScale = 1.0f;
		public float cloudRadius = 1.02f;
        public float renderedCloudRadius = 1.03f;
        public Vector3 cloudColor = new Vector3(0.7f,0.8f,1f);
		public CloudSettings cloudSettings = new CloudSettings();
        public bool hasFlatClouds = false;
        public bool hasBillboardClouds = false;
        public bool hasVolumetricClouds = false;

        public PlanetType planetType;
        public Atmosphere atmosphere;
        public Sea sea;
        public PlanetProperties properties = new PlanetProperties();
        public Surface surface;

        public static PlanetTypes planetTypes;


        public void setLayer(int layer, string tag)
        {
            properties.currentTag = tag;
            properties.currentLayer = layer;
        }

        public void tagGameObject(GameObject go)
        {
            //   Util.tagAll(pSettings.parent, "Normal", 10);
            go.tag = properties.currentTag;
            go.layer = properties.currentLayer;

        }

        public void tagGameObjectAll(GameObject go)
        {
            //   Util.tagAll(pSettings.parent, "Normal", 10);
            go.tag = properties.currentTag;
            go.layer = properties.currentLayer;
            foreach (Transform t in go.transform)
                tagGameObjectAll(t.gameObject);

        }

        public void InitializeAtmosphereMaterials(GameObject go)
        {
            MeshRenderer mr = go.GetComponent<MeshRenderer>();
            if (mr != null)
            {
                atmosphere.InitAtmosphereMaterial(mr.material);
            }


            foreach (Transform t in go.transform)
                InitializeAtmosphereMaterials(t.gameObject);

        }





        public Frame getFrame(int i) {
			if (i>=0 && i< properties.Frames.Count)
				return properties.Frames[i];
			return null;
		}
		
		public void Initialize() {
		
			if (hasSea) {
				sea = new Sea();
			}
            maxQuadNodeLevel = RenderSettings.maxQuadNodeLevel;

			
			surface = new Surface(this);
			
		}


		public static void InitializePlanetTypes() {
			if (planetTypes != null)
				return;


			if (!File.Exists(RenderSettings.planetTypesFilename)) {
				World.FatalError("Could not find planet types file: " + RenderSettings.planetTypesFilename);
				return;
			}
	

			planetTypes = PlanetTypes.DeSerialize(RenderSettings.planetTypesFilename);
			planetTypes.setDelegates();
		}


		
		public void Randomize(int count) {
			System.Random r = new System.Random(seed);
            temperature = (float)r.NextDouble()*500f + 100;
			if (count>=2)
				planetType = planetTypes.getRandomPlanetType(r, radius, temperature);
			else
				planetType = planetTypes.getPlanetType("Terra");; // First two are ALWAYS TERRA
				
			if (planetType == null)
				return;

            if (RenderSettings.ForceAllPlanetTypes != -1)
				planetType = planetTypes.planetTypes[RenderSettings.ForceAllPlanetTypes];


            //int atm = r.Next()%AtmosphereWavelengths.Length;
            //Debug.Log("Atmosphere index: " + atm);
            string atm = planetType.atmosphere[r.Next()%planetType.atmosphere.Length];
			m_atmosphereWavelengths = PlanetType.getAtmosphereValue(atm);

			bumpMap = (Texture2D)Resources.Load ("Meaty_Normal");


			m_surfaceColor = Util.VaryColor(planetType.color, planetType.colorVariation, r);
			m_surfaceColor2 = Util.VaryColor(planetType.color, planetType.colorVariation, r);
            m_waterColor = planetType.seaColor;


			m_basinColor = Util.VaryColor(planetType.basinColor, planetType.basinColorVariation, r);
			m_basinColor2 = Util.VaryColor(planetType.basinColor, planetType.basinColorVariation, r);

//			Debug.Log("Surface color:" + m_surfaceColor + " " + m_surfaceColor2);
//			Debug.Log("basin color:" + m_basinColor + " " + m_basinColor2);
	
			m_topColor = planetType.topColor;//m_basinColor*1.2f;
//			Debug.Log("TOPColor: " + m_topColor);									

            //metallicity = 0.01f*(float)r.NextDouble();
            metallicity = 0;
							
			hasRings = false;
			if (radius>RenderSettings.RingRadiusRequirement && r.NextDouble() < RenderSettings.RingProbability) {
				hasRings = true;
				ringColor.r = 0.7f+  0.3f*(float)r.NextDouble();
				ringColor.g = 0.7f+  0.3f*(float)r.NextDouble();
				ringColor.b = 0.7f+  0.3f*(float)r.NextDouble();
				ringScale = 0.6f + 2.5f * (float)r.NextDouble();
				ringRadius.x = 0.15f + 0.15f*(float)r.NextDouble();
				ringRadius.y = 0.25f + 0.20f*(float)r.NextDouble();
				
			}
            m_hdrExposure = 1.5f;
            m_ESun = 10;


            globalTerrainHeightScale = (1.1f + 1.1f * (float)r.NextDouble())*planetType.surfaceHeightModifier;
            globalTerrainScale = (1.0f + (float)(6 * r.NextDouble()))*planetType.surfaceScaleModifier;


//            atmosphereHeight = 1.019f;
  //          outerRadiusScale = 1.025f;

//             < atmosphereHeight > 1.019 </ atmosphereHeight >
  //  < outerRadiusScale > 1.025 </ outerRadiusScale >

  			atmosphereDensity = planetType.atmosphereDensity;
            if (radius >= 1000) 
			{
				clouds = (Texture2D)Resources.Load (Constants.Clouds[r.Next()%Constants.Clouds.Length]);
                hasFlatClouds = true;
                hasVolumetricClouds = false;
                cloudSettings.Randomize(r);
			}
			if (planetType.sealevel>0 ) {
				sea = new Sea();
                liquidThreshold = planetType.sealevel;
			}
			
			surface = new Surface(this);
			
		}
		
		
		
		public float getHeight() {
			return properties.localCamera.magnitude - radius;
		}
		public float getScaledHeight() {
			return (properties.localCamera.magnitude - radius)/radius;
		}
		public float getPlanetSize() {
            return radius;
        }
        public PlanetSettings() {
			surface = new Surface(this);
			InitializePlanetTypes();
			
		}
		
		
		public void Update() {
			if (properties.posInKm == null) {
                properties.posInKm = new DVector();
			}


            properties.posInKm.x = properties.pos.x;
            properties.posInKm.y = properties.pos.y;
            properties.posInKm.z = properties.pos.z;
            properties.posInKm.Scale(RenderSettings.AU);


            properties.localCamera = World.WorldCamera.Sub (properties.posInKm).toVectorf();// - transform.position;
			Quaternion q = 	Quaternion.Euler(new Vector3(0, -(float)(rotation/(2*Mathf.PI)*360f),0));
            properties.localCamera = q* properties.localCamera;

			if (World.CloseCamera != null)
                properties.cameraPlanes = GeometryUtility.CalculateFrustumPlanes(World.CloseCamera);
        }
		
	}
	

}