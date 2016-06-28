using UnityEngine;
using System.Collections;

namespace LemonSpawn {



public class Clouds : Atmosphere {

	private CloudSettings m_cloudSettings;


	public Clouds(GameObject sun, Mesh m, PlanetSettings ps, CloudSettings cs) {
		planetSettings = ps;
		m_sun = sun;
		m_skyMesh = m;
		m_cloudSettings = cs;

		InitializeParameters(planetSettings.radius);
    

            //		m_radius = m_outerRadius;//planetSettings.radius*planetSettings.cloudRadius;	
            		m_radius = planetSettings.radius*planetSettings.cloudRadius;
            //m_radius = m_outerRadius*0.999f;
		cs.Initialize((Material)Resources.Load ("Clouds"), ps, sun, ps.cloudColor);
		cs.addMaterial(ps.atmosphere.m_groundMaterial);
		//InitializeMesh();
//		m_sky = m_cloudObject;
		m_skyMaterial = m_cloudSettings.material;
        InitAtmosphereMaterial(m_skyMaterial);

        InitializeSkyMesh(m_radius);

        cs.cloudRadius = m_radius;



//		m_sky.transform.Rotate(new Vector3(90,0,0));
	}

	public override void Update() {
		//DebugLog();
		base.Update();

	}



		
}




    public class VolumetricClouds : Atmosphere
    {

        private CloudSettings m_cloudSettings;

		public bool toggleClouds = true;
        VolumetricTexture vtexture = new VolumetricTexture();

        public VolumetricClouds(GameObject sun, Mesh m, PlanetSettings ps, CloudSettings cs)
        {
            planetSettings = ps;
            m_sun = sun;
            m_skyMesh = m;
            m_cloudSettings = cs;
            InitializeParameters(planetSettings.radius);

            

            //		m_radius = m_outerRadius;//planetSettings.radius*planetSettings.cloudRadius;	
            m_radius = planetSettings.radius * planetSettings.renderedCloudRadius;

            cs.Initialize((Material)Resources.Load("VolumetricClouds"), ps, sun, ps.cloudColor);

            m_skyMaterial = m_cloudSettings.material;
            InitAtmosphereMaterial(m_skyMaterial);

            //m_outerRadius = planetSettings.radius * planetSettings.outerRadiusScale;
            //m_outerRadius = planetSettings.atmosphereHeight * planetSettings.radius;

            //            InitializeSkyMesh();
            m_sky = GameObject.Find("cloudBackgroundSphere");
            if (m_sky == null)
                return;
//            m_skySphere = new GameObject("Atmosphere Sky");

            m_sky.GetComponent<Renderer>().material = m_skyMaterial;

            m_skyMaterial.renderQueue = 10000;
            m_sky.transform.Rotate(new Vector3(90, 0, 0));

            vtexture.CreateNoise(8, 6.3245f);

            m_skyMaterial.SetTexture("_NoiseTex3D", vtexture.texture);
        }


        public override void Update()
        {
            if (m_skyMaterial != null)
            {
                InitAtmosphereMaterial(m_skyMaterial);
                m_skyMaterial.SetTexture("_NoiseTex3D", vtexture.texture);

            }

            //            m_skyMaterial.SetFloat("sradius", m_radius);

            m_sky.GetComponent<MeshRenderer>().enabled = RenderSettings.toggleClouds;

        }




    }

}