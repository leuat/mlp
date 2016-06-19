using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;


namespace LemonSpawn {

	[System.Serializable]
	public class Frame {
		public int id;
		public double rotation;
		public double pos_x;
		public double pos_y;
		public double pos_z;
		public DVector pos() {
			return new DVector(pos_x, pos_y, pos_z);
		}		
	}	
	
	[System.Serializable]
 	public class SerializedPlanet {
 		public float outerRadiusScale = 1.05f;
 		public float radius = 5000;
 		public int seed = 0;
 		public double pos_x, pos_y, pos_z;
 		public string name;
 		public double rotation = 0;
 		public float temperature = 200;
 		public List<Frame> Frames = new List<Frame>();
 		public float atmosphereDensity = 1;
 		public float atmosphereHeight = 1;
		public PlanetSettings DeSerialize(GameObject g, int count, float radiusScale) {
			PlanetSettings ps = g.AddComponent<PlanetSettings>();
			ps.outerRadiusScale = outerRadiusScale;
			//ps.transform.position.Set (pos_x, pos_y, pos_z);
			ps.properties.pos.x = pos_x;
			ps.properties.pos.y = pos_y;
			ps.properties.pos.z = pos_z;
			ps.rotation = rotation % (2.0*Mathf.PI);
			ps.temperature = temperature;
			ps.seed = seed;
			ps.properties.Frames = Frames;
			ps.radius = radius*radiusScale;
			ps.atmosphereDensity = Mathf.Clamp(atmosphereDensity, 0, 0.95f);
			ps.atmosphereHeight = atmosphereHeight;
            foreach (Frame f in Frames)
                f.rotation = f.rotation% (2.0 * Mathf.PI);
            ps.Randomize(count);

            return ps;
		}
		
		public SerializedPlanet() {
		
		}
		
		public SerializedPlanet(PlanetSettings ps) {
			outerRadiusScale = ps.outerRadiusScale;
			radius = ps.radius;
			pos_x = ps.transform.position.x;
			pos_y = ps.transform.position.y;
			pos_z = ps.transform.position.z;
			temperature = ps.temperature;
			rotation = ps.rotation;
			seed = ps.seed;
			atmosphereHeight = ps.atmosphereHeight;
			atmosphereDensity= ps.atmosphereDensity;
		}
		
		
 	}


/*	public class PlanetType {
		public string Name;
		public string CloudTexture;
	}
*/

	[System.Serializable]
	public class SerializedCamera {
		public double cam_x, cam_y, cam_z;
//		public float rot_x, rot_y, rot_z;
//		public float cam_theta, cam_phi;
		public double dir_x, dir_y, dir_z;
		public double up_x, up_y, up_z;
		public double fov;
		public double time;
		public int frame;
		public DVector getPos() {
			return new DVector(cam_x, cam_y, cam_z);
		}
		public DVector getUp() {
			return new DVector(up_x, up_y, up_z);
		}
		public DVector getDir() {
			return new DVector(dir_x, dir_y, dir_z);
		}
	}	
	
	

	[System.Serializable]
	public class SerializedWorld {
		public List<SerializedPlanet> Planets = new List<SerializedPlanet>();
		public List<SerializedCamera> Cameras = new List<SerializedCamera>();
		public float sun_col_r=1;
		public float sun_col_g=1;
		public float sun_col_b=0.8f;
		public float sun_intensity=0.1f;
		public float resolutionScale = 1.0f;
		public float global_radius_scale = 1;
		private int frame = 0;
		public float skybox = 0;
		public int resolution = 64;
		public float overview_distance = 4;
		public int screenshot_width = 1024;
		public int screenshot_height = 1024;
		public bool isVideo() {
			if (Cameras.Count>1)
				return true;
			return false;
		}
		
		public SerializedCamera getCamera(int i) {
			if (i>=0 && i<Cameras.Count)
				return Cameras[i];
			return null;
		}
		
		
		public SerializedCamera getCamera(double t, int add) {
		
			double ct = 0;
			for (int i=0;i<Cameras.Count;i++) {
				if (t>=ct && t<Cameras[i].time) {
					return getCamera(i+add-1);
				}
				ct = Cameras[i].time;				
			}
			return null;
		}
		
		public void getInterpolatedCamera(double t, List<Planet> planets) {
			// t in [0,1]
			if (Cameras.Count==1)
				return;
			DVector pos, up;
			up = new DVector(Vector3.up);
			
//			float n = t*(Cameras.Count-1);
			
			double maxTime = Cameras[Cameras.Count-1].time;
			double time = t*maxTime;
			
//			SerializedCamera a = getCamera(n-1);
			SerializedCamera b = getCamera((int)time, 0);
			SerializedCamera c = getCamera((int)time, 1);
			if (/*a==null || */c == null)
				return;
			
			double dt = 1.0/(c.time - b.time) *(time - b.time);
			
			pos = b.getPos() + (c.getPos() - b.getPos())*dt;
			up = b.getUp() + (c.getUp() - b.getUp())*dt;
			
			
			DVector dir = b.getDir() + (c.getDir() - b.getDir())*dt;
			
//			float theta = b.cam_theta + (c.cam_theta - b.cam_theta)*dt;
//			float phi = b.cam_phi + (c.cam_phi - b.cam_phi)*dt;
			
			foreach (Planet p in planets) {
				p.InterpolatePositions(b.frame, dt);
			}

            World.MainCamera.GetComponent<SpaceCamera>().SetLookCamera(pos, dir.toVectorf(), up.toVectorf());
			
		}
		
		public void IterateCamera() {

			if (frame>=Cameras.Count)
				return;

			//Debug.Log("JAH");

			SerializedCamera sc = Cameras[frame];
			//gc.GetComponent<SpaceCamera>().SetCamera(new Vector3(sc.cam_x, sc.cam_y, sc.cam_z), Quaternion.Euler (new Vector3(sc.rot_x, sc.rot_y, sc.rot_z)));
			DVector up = new DVector(sc.up_x, sc.up_y, sc.up_z);
			DVector pos = new DVector(sc.cam_x, sc.cam_y, sc.cam_z);
			World.MainCamera.GetComponent<SpaceCamera>().SetLookCamera(pos, sc.getDir().toVectorf(), up.toVectorf());



			//c.fieldOfView = sc.fov;
             
			
			Atmosphere.sunScale = Mathf.Clamp (1.0f/  (float)pos.Length(), 0.0001f, 1);
			frame++;
		}
		
		
		public SerializedWorld() {
		
		}
		
		
		public static SerializedWorld DeSerialize(string filename) {
			XmlSerializer deserializer = new XmlSerializer(typeof(SerializedWorld));
			TextReader textReader = new StreamReader(filename);
			SerializedWorld sz = (SerializedWorld)deserializer.Deserialize(textReader);
			textReader.Close();
			return sz;
		}
		public static SerializedWorld DeSerializeString(string data) {
			XmlSerializer deserializer = new XmlSerializer(typeof(SerializedWorld));
			//TextReader textReader = new StreamReader(filename);
			StringReader sr = new StringReader(data);
			SerializedWorld sz = (SerializedWorld)deserializer.Deserialize(sr);
			sr.Close();
			return sz;
		}
		static public void Serialize(SerializedWorld sz, string filename)
		{
			XmlSerializer serializer = new XmlSerializer(typeof(SerializedWorld));
			TextWriter textWriter = new StreamWriter(filename);
			serializer.Serialize(textWriter, sz);
			textWriter.Close();
		}
		
		
	}

	
	public class PlanetType {
		public string Name;
		public Color color;
		public Color colorVariation;
		public Color basinColor;
		public Color basinColorVariation;
		public string clouds;
		public float atmosphereDensity = 1;
		public int minQuadLevel;
		public Vector2 RadiusRange, TemperatureRange;
		public delegate SurfaceNode InitializeSurface(float a, float scale, PlanetSettings ps);
		
		public InitializeSurface Delegate;
		public PlanetType(InitializeSurface del, string n, Color c, Color cv, Color b, Color bv, string cl, Vector2 rr, Vector2 tr, int mq, float atm) {
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
		}
		public PlanetType(InitializeSurface del, string n, Color c, Color cv, string cl, Vector2 rr, Vector2 tr, int mq, float atm) {
			Delegate = del;
			Name = n;
			color = c;
			colorVariation = cv;
			clouds = cl;
			RadiusRange = rr;
			TemperatureRange = tr;
			minQuadLevel = mq;
			basinColor = 0.5f*c;
			atmosphereDensity = atm;
			basinColorVariation = 0.5f*cv;
		}
		
		public static List<PlanetType> planetTypes = new List<PlanetType>();
		public static void Initialize() {
//			planetTypes.Add (new PlanetType(Surface.InitializeTerra, "Terra", new Color(0.2f, 0.3f, 0.1f), new Color(0.3f, 0.2f, 0.1f), new Color(0.1f, 0.3f, 0.3f), new Color(0.1f, 0.2f, 0.2f),"", new Vector2(3000, 12000), new Vector2(150,400), RenderSettings.minQuadNodeLevel,1));
			planetTypes.Add (new PlanetType(Surface.InitializeNew, "Terra", new Color(0.3f, 0.5f, 0.2f), new Color(0.3f, 0.2f, 0.1f), new Color(0.4f, 0.4f, 0.1f), new Color(0.1f, 0.1f, 0.0f),"", new Vector2(3000, 12000), new Vector2(150,400), RenderSettings.minQuadNodeLevel,1));
			planetTypes.Add (new PlanetType(Surface.InitializeDesolate, "Desolate", new Color(0.4f, 0.4f, 0.4f), new Color(0.3f, 0.3f, 0.3f), "", new Vector2(100, 10000), new Vector2(0,1000), RenderSettings.minQuadNodeLevel,0.2f));
			planetTypes.Add (new PlanetType(Surface.InitializeFlat, "Cold gas giant", new Color(0.2f, 0.5f, 0.7f), new Color(0.1f, 0.2f, 0.2f), "", new Vector2(12000, 1000000), new Vector2(0,200),1,1));
			planetTypes.Add (new PlanetType(Surface.InitializeFlat, "Hot gas giant", new Color(0.6f, 0.4f, 0.3f), new Color(0.2f, 0.2f, 0.1f), "", new Vector2(50000, 5000000), new Vector2(150,1000),1,1));
			planetTypes.Add (new PlanetType(Surface.InitializeNew, "New", new Color(0.6f, 0.4f, 0.3f), new Color(0.2f, 0.2f, 0.1f), "", new Vector2(500, 5000000), new Vector2(150,1000),1,1));
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
        public float atmosphereHeight = 1.025f;
        public float outerRadiusScale = 1.05f;
        public Vector3 m_atmosphereWavelengths = new Vector3(0.65f, 0.57f, 0.475f);
        public float m_hdrExposure = 1.5f;
        public float m_ESun = 10.0f;            // Sun brightness constant
        public float radius = 5000;
        public float temperature = 300f;

        [Space(10)]
        [Header("Ground settings")]
        public float hillyThreshold = 0.99f;
        public float liquidThreshold = 0.001f;
        public float topThreshold = 0.0045f;
        public float basinThreshold = 0.001f;
        public float globalTerrainHeightScale = 1.0f;
        public float globalTerrainScale = 1.0f;
        public Color m_surfaceColor, m_surfaceColor2;
        public Texture2D m_surfaceTexture;
        public Color m_basinColor, m_basinColor2;
        public Texture2D m_basinTexture;
        public Color m_topColor;
        public Texture2D m_topTexture;
        public Color m_hillColor;
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
        public Vector3 cloudColor = new Vector3(1,1,0);
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
			
			float a = 0.4f;
			float b = 0.25f;
/*			m_atmosphereWavelengths.x = a+ (float)r.NextDouble()*b;
			m_atmosphereWavelengths.y = a+ (float)r.NextDouble()*b;
			m_atmosphereWavelengths.z = a+ (float)r.NextDouble()*b;
*/			

			bumpMap = (Texture2D)Resources.Load ("Meaty_Normal");
			
			a = 0.1f;
			b = 0.8f;
			a = 0;
			
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
			
			m_basinColor.r = planetType.basinColor.r + planetType.basinColorVariation.r*(float)r.NextDouble();
			m_basinColor.g = planetType.basinColor.g + planetType.basinColorVariation.g*(float)r.NextDouble();
			m_basinColor.b = planetType.basinColor.b + planetType.basinColorVariation.b*(float)r.NextDouble();
			
			
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
            cloudColor.x = 0.7f;
            cloudColor.y = 0.8f;
            cloudColor.z = 1;


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
            m_hdrExposure = 2;
            m_ESun = 10;
            globalTerrainHeightScale = 1.1f + (float)r.NextDouble() ;
			globalTerrainScale = 2 + (float)(4*r.NextDouble());
//            atmosphereHeight = 1.019f;
  //          outerRadiusScale = 1.025f;

//             < atmosphereHeight > 1.019 </ atmosphereHeight >
  //  < outerRadiusScale > 1.025 </ outerRadiusScale >


            if (radius >= 5000) 
			{
				clouds = (Texture2D)Resources.Load (Constants.Clouds[r.Next()%Constants.Clouds.Length]);
                hasFlatClouds = true;
                hasVolumetricClouds = false;
			}
			if (planetType.Name == "Terra") {
				sea = new Sea();
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