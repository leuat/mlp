using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;
using UnityEngine.UI;

namespace LemonSpawn
{
    [System.Serializable]
    public class Frame
    {
        public int id;
        public double rotation;
        public double pos_x;
        public double pos_y;
        public double pos_z;
        public DVector pos()
        {
            return new DVector(pos_x, pos_y, pos_z);
        }
    }

    [System.Serializable]
    public class SerializedPlanet
    {
        public float outerRadiusScale = 1.05f;
        public float radius = 5000;
        public int seed = 0;
        public double pos_x, pos_y, pos_z;
        public double col_x=1, col_y=1, col_z=1;
        public string name;
        public string planetType;

        public double rotation = 0;
        public float temperature = 200;
        public List<Frame> Frames = new List<Frame>();
        public float atmosphereDensity = 1;
        // 		public float atmosphereHeight = 1;


        public PlanetSettings DeSerialize(GameObject g, int count, float radiusScale)
        {
            PlanetSettings ps = g.AddComponent<PlanetSettings>();
            ps.outerRadiusScale = outerRadiusScale;
            //ps.transform.position.Set (pos_x, pos_y, pos_z);
            ps.properties.pos.x = pos_x;
            ps.properties.pos.y = pos_y;
            ps.properties.pos.z = pos_z;
            ps.rotation = rotation % (2.0 * Mathf.PI);
            ps.temperature = temperature;
            ps.seed = seed;
            ps.properties.Frames = Frames;
            ps.radius = radius * radiusScale;
            ps.properties.extraColor.r = (float)col_x;
            ps.properties.extraColor.g = (float)col_y;
            ps.properties.extraColor.b = (float)col_z;

            ps.properties.orgPos.Set(ps.properties.pos);
//            ps.atmosphereDensity = Mathf.Clamp(atmosphereDensity, 0, RenderSettings.maxAtmosphereDensity);
            //	ps.atmosphereHeight = atmosphereHeight;
            foreach (Frame f in Frames)
                f.rotation = f.rotation % (2.0 * Mathf.PI);
            if (planetType!="star")
                ps.Randomize(count, planetType);
            else
                ps.planetTypeName = planetType;

            return ps;
        }

        public SerializedPlanet()
        {

        }

        public SerializedPlanet(PlanetSettings ps)
        {
            outerRadiusScale = ps.outerRadiusScale;
            radius = ps.radius;
            pos_x = ps.properties.pos.x;
            pos_y = ps.properties.pos.y;
            pos_z = ps.properties.pos.z;
            temperature = ps.temperature;
            rotation = ps.rotation;
            seed = ps.seed;
            atmosphereDensity = ps.atmosphereDensity;
            Frames = ps.properties.Frames;
            planetType = ps.planetTypeName;



        }


    }


    /*	public class PlanetType {
            public string Name;
            public string CloudTexture;
        }
    */

    [System.Serializable]
    public class SerializedCamera
    {
        public double cam_x, cam_y, cam_z;
        //		public float rot_x, rot_y, rot_z;
        //		public float cam_theta, cam_phi;
        public double dir_x, dir_y, dir_z;
        public double up_x, up_y, up_z;
        public double fov;
        public double time;
        public int frame;
        public DVector getPos()
        {
            return new DVector(cam_x, cam_y, cam_z);
        }
        public DVector getUp()
        {
            return new DVector(up_x, up_y, up_z);
        }
        public DVector getDir()
        {
            return new DVector(dir_x, dir_y, dir_z);
        }
    }



    [System.Serializable]
    public class SerializedWorld
    {
        public List<SerializedPlanet> Planets = new List<SerializedPlanet>();
        public List<SerializedCamera> Cameras = new List<SerializedCamera>();
        public float sun_col_r = 1;
        public float sun_col_g = 1;
        public float sun_col_b = 0.8f;
        public float sun_intensity = 0.1f;
        public float resolutionScale = 1.0f;
        public float global_radius_scale = 1;
        private int frame = 0;
        public float skybox = 0;
        public string uuid;
        public int EnvQuadLevel = 5;
        public int resolution = 64;
        public bool useSpaceCamera = true;
        public float overview_distance = 4;
        public int screenshot_width = 1024;
        public int screenshot_height = 1024;
        public bool isVideo()
        {
            if (Cameras.Count > 1)
                return true;
            return false;
        }


        public int getMaxFrames() {
            float f = 0;
            foreach (SerializedPlanet p in Planets)
                f = Mathf.Max(f, p.Frames.Count);
            return (int)f;

        }

        public void SaveSerializedWorld(string filename, SolarSystem s, string _uuid)
        {
            Planets.Clear();
            foreach (Planet p in s.planets)
            {
                Planets.Add(new SerializedPlanet(p.pSettings));
            }
            uuid = _uuid;
            Serialize(this, filename);
        }


        public SerializedCamera getCamera(int i)
        {
            if (i >= 0 && i < Cameras.Count)
                return Cameras[i];
            if (i < 0)
                return Cameras[0];
            if (i >= Cameras.Count)
                return Cameras[Cameras.Count - 1];
            return null;
        }


        public SerializedCamera getCamera(double t, int add)
        {

           
            for (int i = 0; i < Cameras.Count-1; i++)
            {
                if (t>=Cameras[i].time && t<Cameras[i+1].time)
                    return getCamera(i + add );
            }
            return null;
        }
        public float getCameraIndex(double t, int add) {
            for (int i = 0; i < Cameras.Count - 1; i++)
            {
                if (t >= Cameras[i].time && t < Cameras[i + 1].time)
                    return i + add;
            }
            return 0;
                }

        public void getInterpolatedCamera(double t, List<Planet> planets)
        {
            // t in [0,1]
            if (Cameras.Count <= 1)
                return;
            DVector pos, up;
            up = new DVector(Vector3.up);

            //			float n = t*(Cameras.Count-1);

            double maxTime = Cameras[Cameras.Count - 1].time;
            double time = t * maxTime;

            //			SerializedCamera a = getCamera(n-1);
            SerializedCamera p0 = getCamera(time, -1);
            SerializedCamera p1 = getCamera(time, 0);
            SerializedCamera p2 = getCamera(time, 1);
            SerializedCamera p3 = getCamera(time, 2);

            if (p2 == null || p1 == null)
                return;

            double dt = 1.0 / (p2.time - p1.time) * (time - p1.time);
           
       

            pos = Util.CatmullRom(dt, p0.getPos(), p1.getPos(), p2.getPos(), p3.getPos());
            up = Util.CatmullRom(dt, p0.getUp(), p1.getUp(), p2.getUp(), p3.getUp());
            DVector dir = Util.CatmullRom(dt, p0.getDir(), p1.getDir(), p2.getDir(), p3.getDir());

          /*  pos = p1.getPos() + (p2.getPos() - p1.getPos()) * dt;
            up = p1.getUp() + (p2.getUp() - p1.getUp()) * dt;
            dir = p1.getDir() + (p2.getDir() - p1.getDir()) * dt;
            */

            foreach (Planet p in planets)
            {
                p.InterpolatePositions(p1.frame, dt);
            }

            World.MainCamera.GetComponent<SpaceCamera>().SetLookCamera(pos, dir.toVectorf(), up.toVectorf());

        }

        public void InterpolatePlanetFrames(double t, List<Planet> pl)
        {

            int totalFrames = getMaxFrames();
            if (totalFrames < 2)
            {
                foreach (Planet p in pl)
                {
                    p.pSettings.properties.pos = p.pSettings.properties.orgPos;
  //                  Debug.Log(p.pSettings.properties.pos.toVectorf().x);
                }
                return;
            }
            int frame = (int)(totalFrames*t);
            double dt = (totalFrames*t - frame);

            foreach (Planet p in pl)
            {
                p.InterpolatePositions(frame, dt);
//                Debug.Log(p.pSettings.properties.pos.toVectorf());
            }


        }



        public void getInterpolatedCameraLerp(double t, List<Planet> planets)
        {
            // t in [0,1]
            if (Cameras.Count <= 1)
                return;
            DVector pos, up;
            up = new DVector(Vector3.up);


            double maxTime = Cameras[Cameras.Count - 1].time;
            double time = t * maxTime;

            SerializedCamera b = getCamera((int)time, 0);
            SerializedCamera c = getCamera((int)time, 1);
            if (c == null)
                return;

            double dt = 1.0 / (c.time - b.time) * (time - b.time);

            pos = b.getPos() + (c.getPos() - b.getPos()) * dt;
            up = b.getUp() + (c.getUp() - b.getUp()) * dt;


            DVector dir = b.getDir() + (c.getDir() - b.getDir()) * dt;


            foreach (Planet p in planets)
            {
                p.InterpolatePositions(b.frame, dt);
            }

            World.MainCamera.GetComponent<SpaceCamera>().SetLookCamera(pos, dir.toVectorf(), up.toVectorf());

        }

        public void IterateCamera()
        {

            if (frame >= Cameras.Count)
                return;

            //Debug.Log("JAH");

            SerializedCamera sc = Cameras[frame];
            //gc.GetComponent<SpaceCamera>().SetCamera(new Vector3(sc.cam_x, sc.cam_y, sc.cam_z), Quaternion.Euler (new Vector3(sc.rot_x, sc.rot_y, sc.rot_z)));
            DVector up = new DVector(sc.up_x, sc.up_y, sc.up_z);
            DVector pos = new DVector(sc.cam_x, sc.cam_y, sc.cam_z);
			SpaceCamera spc = World.MainCamera.GetComponent<SpaceCamera>();
            if (spc!=null)
            	spc.SetLookCamera(pos, sc.getDir().toVectorf(), up.toVectorf());



            //c.fieldOfView = sc.fov;


            //Atmosphere.sunScale = Mathf.Clamp(1.0f / (float)pos.Length(), 0.0001f, 1);
            frame++;
        }


        public SerializedWorld()
        {

        }


        public static SerializedWorld DeSerialize(string filename)
        {
            XmlSerializer deserializer = new XmlSerializer(typeof(SerializedWorld));
            TextReader textReader = new StreamReader(filename);
            SerializedWorld sz = (SerializedWorld)deserializer.Deserialize(textReader);
            textReader.Close();
            return sz;
        }
        static public void Serialize(SerializedWorld sz, string filename)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(SerializedWorld));
            TextWriter textWriter = new StreamWriter(filename);
            serializer.Serialize(textWriter, sz);
            textWriter.Close();
        }

		public static SerializedWorld DeSerializeString(string data)
        {
            XmlSerializer deserializer = new XmlSerializer(typeof(SerializedWorld));
            //TextReader textReader = new StreamReader(filename);
            StringReader sr = new StringReader(data);
            SerializedWorld sz = (SerializedWorld)deserializer.Deserialize(sr);
            sr.Close();
            return sz;
        }

    }


	[System.Serializable]
    public class MCAstSettings
    {
        public static int[,] Resolution = new int[11, 2] { 
            { 320, 200 }, { 640, 480 }, { 800, 600 }, { 1024, 768 }, { 1280, 1024 }, { 1600, 1200 },
            { 800, 480 }, { 1024, 600 }, { 1280, 720 }, { 1680, 1050 }, { 2048, 1080 } };

        public static int[] GridSizes = new int[6] { 16, 32, 48, 64, 80, 96 };


        public int movieResolution = 1;
        public int gridSize = 2;
        public int screenShotResolution = 4 ;
        public bool advancedClouds = false;
        public bool cameraEffects = true;
        public string previousFile = "";


        public static MCAstSettings DeSerialize(string filename)
        {
            XmlSerializer deserializer = new XmlSerializer(typeof(MCAstSettings));
            TextReader textReader = new StreamReader(filename);
            MCAstSettings sz = (MCAstSettings)deserializer.Deserialize(textReader);
            textReader.Close();
            return sz;
        }
        static public void Serialize(MCAstSettings sz, string filename)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(MCAstSettings));
            TextWriter textWriter = new StreamWriter(filename);
            serializer.Serialize(textWriter, sz);
            textWriter.Close();
        }
    }


}