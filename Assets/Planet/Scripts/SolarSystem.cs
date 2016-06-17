﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace LemonSpawn
{


    public class SpaceAtmosphere
    {
        Material mat;
        GameObject sun;
        public Color color;
        public float m_g = -0.990f;             // The Mie phase asymmetry factor, must be between 0.999 to -0.999
        public float hdr = 0.1f;
        public SpaceAtmosphere(Material m, GameObject s, Color col, float h)
        {
            mat = m;
            sun = s;
            color = col;
            hdr = h;
        }


        public void Update()
        {
            mat.SetVector("v3LightPos", sun.transform.forward * -1.0f);
            mat.SetColor("sunColor", color);
            mat.SetFloat("fHdrExposure", hdr * Atmosphere.sunScale);
            mat.SetFloat("g", m_g);
            mat.SetFloat("g2", m_g * m_g);

        }


    }


    public class SolarSystem
    {

        static public Material spaceMaterial;
        static public Material groundMaterial;
        public Transform transform;
        private GameObject sun;
        private Mesh sphere;
        public SpaceAtmosphere space;
        public List<Planet> planets = new List<Planet>();

        // Closest active planet
        public static Planet planet = null;

        // Use this for initialization

        public SolarSystem(GameObject pSun, Mesh s, Transform t, int skybox)
        {
            sun = pSun;
            sphere = s;
            transform = t;
            spaceMaterial = (Material)Resources.Load("SpaceMaterial");
            groundMaterial = (Material)Resources.Load("GroundMaterial");
            space = new SpaceAtmosphere(spaceMaterial, sun, Color.white, 0.1f);


            SetSkybox(skybox);

        }

        void setSun()
        {
            //		if (World.WorldCamera
            if (sun == null)
                return;
            sun.transform.rotation = Quaternion.FromToRotation(Vector3.forward, World.WorldCamera.toVectorf().normalized);
            sun.GetComponent<Light>().color = space.color;
        }

        public void findClosestPlanet()
        {
            if (planets.Count > 0)
                planet = planets[0];

            float min = 1E10f;
            foreach (Planet p in planets)
            {
                float l = (p.pSettings.gameObject.transform.position).magnitude - p.pSettings.radius;
                if (l < min)
                {
                    planet = p;
                    min = l;
                }
            }

        }


        public void InitializeFromScene()
        {

            for (int i = 0; i < transform.childCount; i++)
            {
                GameObject go = transform.GetChild(i).gameObject;
                if (go.activeSelf)
                {
                    Planet p = new Planet(go.GetComponent<PlanetSettings>(), null);
                    p.pSettings.pos.Set(go.transform.position);
                    go.transform.parent = transform;
                    p.pSettings.parent = go;
                    p.pSettings.planetType = PlanetType.planetTypes[p.pSettings.planetTypeIndex];
                    //				p.pSettings.planetType = PlanetType.planetTypes[1];
                    p.Initialize(sun, (Material)Resources.Load("GroundMaterial"), (Material)Resources.Load("SkyMaterial"), sphere);
                    planets.Add(p);
                }
            }

            RenderSettings.ResolutionScale = World.SzWorld.resolutionScale;

            space.color = new Color(World.SzWorld.sun_col_r, World.SzWorld.sun_col_g, World.SzWorld.sun_col_b);
            space.hdr = World.SzWorld.sun_intensity;
        }



        protected void FocusOnPlanet(string n)
        {

            //Camera c = gc.GetComponent<Camera>();
            Planet planet = null;
            foreach (Planet p in planets)
                if (p.pSettings.name == n)
                    planet = p;

            if (planet == null)
                return;

            Vector3 pos = planet.pSettings.pos.toVectorf();
            float s = (float)(planet.pSettings.radius * World.SzWorld.overview_distance / RenderSettings.AU);
            Vector3 dir = pos.normalized * s;
            Vector3 side = Vector3.Cross(Vector3.up, dir);

            pos = pos - dir - side.normalized * s;
            pos.y += s;

            /*            SpaceCamera.SetLookCamera(pos, planet.pSettings.gameObject.transform.position, Vector3.up);
                        UpdateWorldCamera();
                        Update();
                        SpaceCamera.SetLookCamera(pos, planet.pSettings.gameObject.transform.position, Vector3.up);
                        UpdateWorldCamera();*/


        }

        public void Update()
        {
            setSun();
            if (space != null)
                space.Update();

            findClosestPlanet();

            if (planet != null)
                planet.ConstrainCameraExterior();

            foreach (Planet p in planets)
                p.Update();

            // Set closest clippping plane
            if (planet != null)
            {
                if (planet.pSettings.atmosphere != null)
                    planet.pSettings.atmosphere.setClippingPlanes();
            }

        }

        public void LoadWorld(string data, bool isFile, bool ExitOnSave, World world)
        {
            ClearStarSystem();
            SerializedWorld sz;
            if (isFile)
            {
                //			RenderSettings.extraText = data;

                if (!System.IO.File.Exists(data))
                {
                    RenderSettings.extraText = ("ERROR: Could not find file :'" + data + "'");
                    return;
                }
                sz = SerializedWorld.DeSerialize(data);
            }
            else
                sz = SerializedWorld.DeSerializeString(data);

            World.SzWorld = sz;
            RenderSettings.ExitSaveOnRendered = ExitOnSave;
            RenderSettings.extraText = "";
            SetSkybox((int)sz.skybox);
            RenderSettings.sizeVBO = Mathf.Clamp(sz.resolution, 32, 128);
            RenderSettings.ScreenshotX = sz.screenshot_height;
            RenderSettings.ScreenshotY = sz.screenshot_width;
            RenderSettings.ResolutionScale = sz.resolutionScale;
            int cnt = 0;
            World.hasScene = true;
            RenderSettings.isVideo = sz.isVideo();
            if (RenderSettings.isVideo == true)
                RenderSettings.ExitSaveOnRendered = false;


            //		RenderSettings.isVideo = false;
            if (World.slider!=null)	
	            World.slider.SetActive(RenderSettings.isVideo);
            foreach (SerializedPlanet sp in sz.Planets)
            {
                //GameObject go = transform.GetChild(i).gameObject
                Debug.Log("adding: " + sp.name); 
                GameObject go = new GameObject(sp.name);
                go.transform.parent = transform;

                //				go.transform.position = new Vector3((float)(sp.pos_x*RenderSettings.AU), (float)(sp.pos_y*RenderSettings.AU), (float)(sp.pos_z*RenderSettings.AU));
                //				Planet p = new Planet(sp.DeSerialize(go), go.GetComponent<CloudSettings>());
                Planet p = new Planet(sp.DeSerialize(go, cnt++, sz.global_radius_scale));
				p.pSettings.parent = go;

                p.Initialize(sun, (Material)Resources.Load("GroundMaterial"), (Material)Resources.Load("SkyMaterial"), sphere);
                planets.Add(p);
            }
            PopulateOverviewList("Overview");
            //sz.IterateCamera();
			//World.SzWorld = sz;
			world.setWorld(sz);
			//Debug.Log("Before leaving: " + World.SzWorld.Cameras.Count);
        }
        public void ClearStarSystem()
        {
            planets.Clear();
            for (int i = 0; i < transform.childCount; i++)
            {
                GameObject go = transform.GetChild(i).gameObject;
                Debug.Log("Destroying: " + go.name);
                GameObject.Destroy(go);
                //	Debug.Log ("Destroying " + go.name);
            }



        }
        protected void PopulateOverviewList(string box)
        {
			GameObject b = GameObject.Find(box);
			if (b == null)
				return;
			ComboBox cbx = b.GetComponent<ComboBox>();
			cbx.ClearItems();
            List<ComboBoxItem> l = new List<ComboBoxItem>();
            foreach (Planet p in planets)
            {
                ComboBoxItem ci = new ComboBoxItem();
                ci.Caption = p.pSettings.name;
                string n = p.pSettings.name;
                ci.OnSelect = delegate
                {
                    FocusOnPlanet(n);
                };
                l.Add(ci);
            }
            //		foreach (ComboBoxItem i in l)
            //			Debug.Log (i.Caption);

            cbx.AddItems(l.ToArray());

        }
        public static void SetSkybox(int s)
        {
            string skybox = "Skybox3";
            s = s % 7;

            if (s == 1) skybox = "Skybox4";
            if (s == 2) skybox = "Skybox5";
            if (s == 3) skybox = "Skybox2";
            if (s == 4) skybox = "Skybox7";
            if (s == 5) skybox = "Skybox8";
            if (s == 6) skybox = "Skybox9";

            UnityEngine.RenderSettings.skybox = (Material)Resources.Load(skybox);

        }


    }
}