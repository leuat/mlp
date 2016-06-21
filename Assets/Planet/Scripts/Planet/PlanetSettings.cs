using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;


namespace LemonSpawn {



    public class PlanetType {
        public string Name;
        public Color color;
        public Color colorVariation;
        public Color basinColor;
        public Color basinColorVariation;
        public Color seaColor;
        public int[] atmosphere;
        public string clouds;
        public float atmosphereDensity = 1;
        public float sealevel;

        public int minQuadLevel;
        public Vector2 RadiusRange, TemperatureRange;
        public delegate SurfaceNode InitializeSurface(float a, float scale, PlanetSettings ps);

        public InitializeSurface Delegate;
        public PlanetType(InitializeSurface del, string n, Color c, Color cv, Color b, Color bv, string cl, Vector2 rr, Vector2 tr, int mq, float atm,
            float seal, Color seacol, int[] atmidx) {
			Delegate = del;
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

		}
        public static Vector3[] AtmosphereWavelengths = new Vector3[] {
            new Vector3(0.65f, 0.57f, 0.475f), // CONFIRMED NORMAL
            new Vector3(0.6f, 0.6f, 0.6f),    // BLEAK 
            new Vector3(0.5f, 0.62f, 0.625f), // CONFIRMED RED
            new Vector3(0.65f, 0.47f, 0.435f), // CONFIRMED CYAN
            new Vector3(0.60f, 0.5f, 0.60f),  // CONFIRMED GREEN
            new Vector3(0.65f, 0.67f, 0.475f),   //  Confirmed PURPLE 
            new Vector3(0.5f, 0.54f, 0.635f),   // CONFIRMED YELLOW
            new Vector3(0.45f, 0.72f, 0.675f), // CONFIRMED PINK


        };

        public static int ATM_NORMAL = 0;
        public static int ATM_BLEAK = 1;
        public static int ATM_RED = 2;
        public static int ATM_CYAN = 3;
        public static int ATM_GREEN = 4;
        public static int ATM_PURPLE = 5;
        public static int ATM_YELLOW = 6;
        public static int ATM_PINK = 7;



        public static List<PlanetType> planetTypes = new List<PlanetType>();
		public static void Initialize() {
            //			planetTypes.Add (new PlanetType(Surface.InitializeTerra, "Terra", new Color(0.2f, 0.3f, 0.1f), new Color(0.3f, 0.2f, 0.1f), new Color(0.1f, 0.3f, 0.3f), new Color(0.1f, 0.2f, 0.2f),"", new Vector2(3000, 12000), new Vector2(150,400), RenderSettings.minQuadNodeLevel,1));


            float atmDens = 0.8f;

            planetTypes.Add(new PlanetType(
                Surface.InitializeNew, "Terra",
                new Color(0.3f, 0.5f, 0.2f), new Color(0.3f, 0.2f, 0.1f),
                new Color(0.4f, 0.4f, 0.1f), new Color(0.5f, 0.5f, 0.0f), "",
                new Vector2(3000, 12000),
                new Vector2(150, 400),
                RenderSettings.minQuadNodeLevel, atmDens,
                0.001f, new Color(0.3f, 0.4f, 1.0f), new int[] { ATM_NORMAL }
                ));
			planetTypes.Add (new PlanetType(
                Surface.InitializeDesolate, "Desert", 
                new Color(0.9f, 0.5f, 0.2f), new Color(0.1f, 0.4f, 0.1f),
                new Color(0.5f, 0.2f, 0.1f), new Color(0.1f, 0.3f, 0.2f),
                "", new Vector2(100, 15000), new Vector2(100,1000), 
                RenderSettings.minQuadNodeLevel, 0.7f,
                0.000f, Color.black, new int[] { ATM_RED, ATM_YELLOW }
                ));
            planetTypes.Add(new PlanetType(
            Surface.InitializeMountain, "Mountain",
            new Color(0.9f, 0.5f, 0.2f), new Color(0.1f, 0.4f, 0.1f),
            new Color(0.5f, 0.2f, 0.1f), new Color(0.1f, 0.3f, 0.2f),
            "", new Vector2(100, 15000), new Vector2(100, 1000),
            RenderSettings.minQuadNodeLevel, 0.7f,
            0.000f, Color.black, new int[] { ATM_RED, ATM_YELLOW }
            ));
            planetTypes.Add(new PlanetType(
                Surface.InitializeFlat, "Cold gas giant",
                new Color(0.2f, 0.5f, 0.7f), new Color(0.1f, 0.2f, 0.2f),
                new Color(0.2f, 0.5f, 0.7f), new Color(0.1f, 0.2f, 0.2f),
                "",
                new Vector2(12000, 1000000), new Vector2(0, 200),
                1, 1,
                0.000f, Color.black, new int[] { ATM_NORMAL, ATM_BLEAK, ATM_CYAN }

                ));
			planetTypes.Add (new PlanetType(
                Surface.InitializeFlat, "Hot gas giant", 
                new Color(0.6f, 0.4f, 0.3f), new Color(0.2f, 0.2f, 0.1f),
                new Color(0.6f, 0.4f, 0.3f), new Color(0.2f, 0.2f, 0.1f),
                "", 
                new Vector2(50000, 5000000), new Vector2(150,1000),
                1,1,
                0.000f, Color.black, new int[] { ATM_RED, ATM_YELLOW }

                ));
			planetTypes.Add (new PlanetType(
                Surface.InitializeNew, "New", 
                new Color(0.6f, 0.4f, 0.3f), new Color(0.2f, 0.2f, 0.1f),
                new Color(0.6f, 0.4f, 0.4f), new Color(0.2f, 0.2f, 0.1f),
                "", new Vector2(500, 5000000), new Vector2(150,1000),
                1, atmDens,
                0.001f, new Color(0.3f, 0.4f, 1.0f), new int[] { ATM_NORMAL }

                ));
            planetTypes.Add(new PlanetType(
                Surface.InitializeTerra2, 
                "Terra2", 
                new Color(0.6f, 0.4f, 0.3f), new Color(0.3f, 0.3f, 0.3f),
                new Color(0.5f, 0.5f, 0.2f), new Color(0.5f, 0.5f, 0.1f),
                "", 
                new Vector2(500, 5000000), new Vector2(150, 1000), 
                RenderSettings.minQuadNodeLevel, atmDens,
                0.001f, new Color(0.3f, 0.4f, 1.0f), new int[] { ATM_NORMAL }

                ));
        }

        public static PlanetType getRandomPlanetType(System.Random r, float radius, float temperature) {
			List<PlanetType> candidates = new List<PlanetType>();
			foreach (PlanetType pt in planetTypes) {
				if ((radius>=pt.RadiusRange.x && radius<pt.RadiusRange.y) && (temperature>=pt.TemperatureRange.x && temperature<pt.TemperatureRange.y))
					candidates.Add (pt);
			}
			
			if (candidates.Count==0)
				return planetTypes[1];
				
			return candidates[r.Next()%candidates.Count];
			
			
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
        public Color m_hillColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
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
		
		
		public void Randomize(int count) {
			System.Random r = new System.Random(seed);
            temperature = (float)r.NextDouble()*500f + 100;
			if (count>=2)
				planetType = PlanetType.getRandomPlanetType(r, radius, temperature);
			else
				planetType = PlanetType.planetTypes[0]; // First two are ALWAYS TERRA
				
			if (planetType == null)
				return;

            if (RenderSettings.ForceAllPlanetTypes != -1)
                planetType = PlanetType.planetTypes[RenderSettings.ForceAllPlanetTypes];


            //int atm = r.Next()%AtmosphereWavelengths.Length;
            //Debug.Log("Atmosphere index: " + atm);
            int idx = planetType.atmosphere[r.Next()%planetType.atmosphere.Length];
			m_atmosphereWavelengths = PlanetType.AtmosphereWavelengths[idx];

			bumpMap = (Texture2D)Resources.Load ("Meaty_Normal");



			float a = 0.1f;
			float b = 0.8f;

			/*m_surfaceColor.r = a+b*(float)r.NextDouble();
			m_surfaceColor.g = a+b*(float)r.NextDouble();
			m_surfaceColor.b = a+b*(float)r.NextDouble();
			m_basinColor.r = 0.5f*(a+b*(float)r.NextDouble());
			m_basinColor.g = 0.5f*(a+b*(float)r.NextDouble());
			m_basinColor.b = 0.5f*(a+b*(float)r.NextDouble());
*/
			m_surfaceColor.r = planetType.color.r + planetType.colorVariation.r*(float)r.NextDouble();
			m_surfaceColor.g = planetType.color.g + planetType.colorVariation.g*(float)r.NextDouble();
			m_surfaceColor.b = planetType.color.b + planetType.colorVariation.b*(float)r.NextDouble();

			m_surfaceColor2.r = planetType.color.r + planetType.colorVariation.r*(float)r.NextDouble();
			m_surfaceColor2.g = planetType.color.g + planetType.colorVariation.g*(float)r.NextDouble();
			m_surfaceColor2.b = planetType.color.b + planetType.colorVariation.b*(float)r.NextDouble();

            m_waterColor = planetType.seaColor;
									
			m_basinColor.r = planetType.basinColor.r + planetType.basinColorVariation.r*(float)r.NextDouble();
			m_basinColor.g = planetType.basinColor.g + planetType.basinColorVariation.g*(float)r.NextDouble();
			m_basinColor.b = planetType.basinColor.b + planetType.basinColorVariation.b*(float)r.NextDouble();

			m_basinColor2.r = planetType.basinColor.r + planetType.basinColorVariation.r*(float)r.NextDouble();
			m_basinColor2.g = planetType.basinColor.g + planetType.basinColorVariation.g*(float)r.NextDouble();
			m_basinColor2.b = planetType.basinColor.b + planetType.basinColorVariation.b*(float)r.NextDouble();

			
			m_topColor = new Color(0.5f, 0.5f,0.5f);//m_basinColor*1.2f;
									
/*			m_topColor.r = a+b*(float)r.NextDouble();
			m_topColor.g = a+b*(float)r.NextDouble();
			m_topColor.b = a+b*(float)r.NextDouble();
*/			
			a = 1;
			b = -0.6f;
            //cloudColor.x = 1f*(a+b*(float)r.NextDouble());
            //cloudColor.y = 1f*(a+b*(float)r.NextDouble());
            //cloudColor.z = 1f*(a+b*(float)r.NextDouble());
/*            cloudColor.x = 0.7f;
            cloudColor.y = 0.8f;
            cloudColor.z = 1;
            */

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
            globalTerrainHeightScale = 1.1f + (float)r.NextDouble() ;
			globalTerrainScale = 2 + (float)(6*r.NextDouble());



//            atmosphereHeight = 1.019f;
  //          outerRadiusScale = 1.025f;

//             < atmosphereHeight > 1.019 </ atmosphereHeight >
  //  < outerRadiusScale > 1.025 </ outerRadiusScale >

//  			atmosphereDensity = 0;
            if (radius >= 1000) 
			{
				clouds = (Texture2D)Resources.Load (Constants.Clouds[r.Next()%Constants.Clouds.Length]);
                //hasFlatClouds = true;
                hasVolumetricClouds = false;
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