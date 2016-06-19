﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace LemonSpawn {

	public class SSVSettings {
		public static float SolarSystemScale = 5.0f;
		public static float PlanetSizeScale = 1.0f / 5000.0f;
		public static int OrbitalLineSegments = 100;
		public static Vector2 OrbitalLineWidth = new Vector2 (0.03f, 0.03f);
	}

	public class DisplayPlanet {
		public Planet planet;
		public GameObject go;
		public List<GameObject> orbitLines = new List<GameObject>();

		private void CreateOrbit() {
			float radius = (float)planet.pSettings.properties.pos.Length () * SSVSettings.SolarSystemScale;
			for (int i = 0; i < SSVSettings.OrbitalLineSegments; i++) {
				float t0 = 2 * Mathf.PI / (float)SSVSettings.OrbitalLineSegments * (float)i;
				float t1 = 2 * Mathf.PI / (float)SSVSettings.OrbitalLineSegments * (float)(i+1);
				Vector3 from = new Vector3 (Mathf.Cos (t0), 0, Mathf.Sin (t0)) * radius;
				Vector3 to = new Vector3 (Mathf.Cos (t1), 0, Mathf.Sin (t1)) * radius;
			
				GameObject g = new GameObject ();
				g.transform.parent = go.transform;
				LineRenderer lr = g.AddComponent<LineRenderer> ();
				lr.material = (Material)Resources.Load ("LineMaterial");
				lr.SetWidth (SSVSettings.OrbitalLineWidth.x, SSVSettings.OrbitalLineWidth.y);
				lr.SetPosition (0, from);
				lr.SetPosition (1, to);
				orbitLines.Add (g);
			}
		}

		public DisplayPlanet(GameObject g, Planet p) {
			go = g;
			planet = p;

			CreateOrbit ();
		}
	}

	public class SolarSystemViewverMain : World {
		private List<DisplayPlanet> dPlanets = new List<DisplayPlanet>();
		private Vector3 mouseAccel = new Vector3();
		private Vector3 focusPoint = Vector3.zero;
		private DisplayPlanet selected = null;

		private void SelectPlanet(DisplayPlanet dp) {
			selected = dp;
			focusPoint = dp.go.transform.position;
		}

		private void UpdateFocus() {
			if (Input.GetMouseButtonDown (0)) {
				RaycastHit hit;
				Debug.Log (MainCamera);
				Ray ray = MainCamera.ScreenPointToRay (Input.mousePosition);
				if (Physics.Raycast (ray, out hit)) {
					foreach (DisplayPlanet dp in dPlanets) {
						if (dp.go == hit.transform.gameObject)
							SelectPlanet(dp);
					}
				}
			}
		}

		private void UpdateCamera () {
			float s = 1.0f;
			float theta = s * Input.GetAxis ("Mouse X");
			float phi = s * Input.GetAxis ("Mouse Y") * -1.0f;
			mouseAccel += new Vector3 (theta, phi, 0);
			mainCamera.transform.RotateAround (Vector3.zero, Vector3.up, mouseAccel.x);
			mainCamera.transform.RotateAround (Vector3.zero, mainCamera.transform.right, mouseAccel.y);
			mouseAccel *= 0.9f;
		}

		private void LoadData() {
			string file = Application.dataPath + "/../" + "system2.xml";
			if (!System.IO.File.Exists(file)) {
				Debug.Log("ERROR: Could not find file :'" + file + "'");
				return;
			}
			string xml = System.IO.File.ReadAllText(file);

			solarSystem.LoadWorld (xml, false, false, this);
		}

		private void PopulateWorld() {
			dPlanets.Clear ();
			foreach (Planet p in solarSystem.planets) {
				GameObject go = GameObject.CreatePrimitive (PrimitiveType.Sphere);
				go.GetComponent<MeshRenderer> ().material = (Material)Resources.Load ("TempPlanetMaterial");
				Vector3 coolpos = new Vector3 ((float)p.pSettings.properties.pos.x, (float)p.pSettings.properties.pos.z, (float)p.pSettings.properties.pos.y);
				go.transform.position = coolpos * SSVSettings.SolarSystemScale;
				go.transform.localScale = Vector3.one * SSVSettings.PlanetSizeScale * p.pSettings.radius;
			
				dPlanets.Add (new DisplayPlanet (go, p));
			}
		}

		public override void Start () { 
			solarSystem = new SolarSystem(sun, sphere, transform, (int)szWorld.skybox);
			PlanetType.Initialize();
			MainCamera = mainCamera.GetComponent<Camera> ();

			LoadData ();
			PopulateWorld ();
		}
	
		public override void Update () {
			UpdateFocus ();
			UpdateCamera ();
		}

		protected void OnGUI() {
		}
	}

}