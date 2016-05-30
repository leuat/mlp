﻿using UnityEngine;
using System.Collections;
using OceanSurfaceEffects;

namespace LemonSpawn {

	public class Sea : Atmosphere
    {
        GameObject parent, m_go;
 		Mesh m_sphere;
        Ocean ocean;
        CubeSphere cube;
        PlanetSettings psOcean;

        protected void InitializeMesh(GameObject p) {

            parent = p;
//            psOcean = new PlanetSettings();


            m_go = new GameObject("water");
            m_go.transform.parent = parent.transform;
            m_go.transform.localPosition = Vector3.zero;
            m_go.transform.localScale = Vector3.one;


            psOcean = m_go.AddComponent<PlanetSettings>();

            psOcean.atmosphere = new Atmosphere();
            //psOcean.gameObject = psOcean.terrainObject;

            psOcean.atmosphere.m_groundMaterial = m_groundMaterial;
            cube = new CubeSphere(psOcean, false);

            InitializeParameters();
            m_radius = planetSettings.radius * (1 + planetSettings.liquidThreshold);
            ocean = new Ocean();
            m_groundMaterial = (Material)Resources.Load("OceanIndie");
            psOcean.radius = m_radius;
            psOcean.planetType = PlanetType.planetTypes[3];
            psOcean.hasSea = false;
            psOcean.hasClouds = false;
            psOcean.Initialize();
            psOcean.maxQuadNodeLevel = 9; ;
            psOcean.atmosphere.m_groundMaterial = m_groundMaterial;
            psOcean.terrainObject = m_go;
            psOcean.castShadows = false;
            ocean.Start(planetSettings.gameObject.transform, m_radius, psOcean.terrainObject, m_sun, m_groundMaterial);


            m_innerRadius = planetSettings.radius;
            m_outerRadius = planetSettings.radius * planetSettings.outerRadiusScale;

            initFixedMateriarProperties(false);
            InitMaterial(m_groundMaterial);
            
            m_groundMaterial.SetVector("waterColor", planetSettings.m_waterColor);
            m_groundMaterial.SetFloat("_Metallic", 0);

            m_sky = m_go; 

        }

        public void Initialize(GameObject sun, Mesh sphere, PlanetSettings ps) {
			m_sphere = sphere;
			m_sun = sun;
			planetSettings = ps;
			InitializeMesh(ps.gameObject);
		}
		
		public Sea() {
		}

        private void MaintainSea()
        {
            if (psOcean == null)
                return;

            psOcean.localCamera = planetSettings.localCamera;

            cube.SubDivide(RenderSettings.gridDivide);
            cube.Realise();

            psOcean.Update();

        }


        public override void Update()
        {
            base.Update();
            MaintainSea();
                   
            m_groundMaterial.SetFloat("time", Time.time*0.01f);
            ocean.Update();
        }




    }

}