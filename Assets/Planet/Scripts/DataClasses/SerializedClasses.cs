using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;

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
        public string name;
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
            ps.atmosphereDensity = Mathf.Clamp(atmosphereDensity, 0, RenderSettings.maxAtmosphereDensity);
            //	ps.atmosphereHeight = atmosphereHeight;
            foreach (Frame f in Frames)
                f.rotation = f.rotation % (2.0 * Mathf.PI);
            ps.Randomize(count);

            return ps;
        }

        public SerializedPlanet()
        {

        }

        public SerializedPlanet(PlanetSettings ps)
        {
            outerRadiusScale = ps.outerRadiusScale;
            radius = ps.radius;
            pos_x = ps.transform.position.x;
            pos_y = ps.transform.position.y;
            pos_z = ps.transform.position.z;
            temperature = ps.temperature;
            rotation = ps.rotation;
            seed = ps.seed;
            //			atmosphereHeight = ps.atmosphereHeight;
            atmosphereDensity = ps.atmosphereDensity;
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
        public int resolution = 64;
        public float overview_distance = 4;
        public int screenshot_width = 1024;
        public int screenshot_height = 1024;
        public bool isVideo()
        {
            if (Cameras.Count > 1)
                return true;
            return false;
        }

        public SerializedCamera getCamera(int i)
        {
            if (i >= 0 && i < Cameras.Count)
                return Cameras[i];
            return null;
        }


        public SerializedCamera getCamera(double t, int add)
        {

            double ct = 0;
            for (int i = 0; i < Cameras.Count; i++)
            {
                if (t >= ct && t < Cameras[i].time)
                {
                    return getCamera(i + add - 1);
                }
                ct = Cameras[i].time;
            }
            return null;
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
            SerializedCamera b = getCamera((int)time, 0);
            SerializedCamera c = getCamera((int)time, 1);
            if (/*a==null || */c == null)
                return;

            double dt = 1.0 / (c.time - b.time) * (time - b.time);

            pos = b.getPos() + (c.getPos() - b.getPos()) * dt;
            up = b.getUp() + (c.getUp() - b.getUp()) * dt;


            DVector dir = b.getDir() + (c.getDir() - b.getDir()) * dt;

            //			float theta = b.cam_theta + (c.cam_theta - b.cam_theta)*dt;
            //			float phi = b.cam_phi + (c.cam_phi - b.cam_phi)*dt;

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
            World.MainCamera.GetComponent<SpaceCamera>().SetLookCamera(pos, sc.getDir().toVectorf(), up.toVectorf());



            //c.fieldOfView = sc.fov;


            Atmosphere.sunScale = Mathf.Clamp(1.0f / (float)pos.Length(), 0.0001f, 1);
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
        public static SerializedWorld DeSerializeString(string data)
        {
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


}