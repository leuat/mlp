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
		m_innerRadius = planetSettings.radius;
		m_outerRadius = planetSettings.radius*planetSettings.outerRadiusScale;

            //		m_radius = m_outerRadius;//planetSettings.radius*planetSettings.cloudRadius;	
            //		m_radius = planetSettings.radius*planetSettings.cloudRadius;	
            m_radius = m_outerRadius*0.999f;
			
		cs.Initialize((Material)Resources.Load ("Clouds"), ps, sun);
		//InitializeMesh();
//		m_sky = m_cloudObject;
		m_skyMaterial = m_cloudSettings.material;
        InitAtmosphereMaterial(m_skyMaterial);

        InitializeSkyMesh();
		m_sky.transform.Rotate(new Vector3(90,0,0));
	}
	
		
}




    public class RenderedClouds : Atmosphere
    {

        private CloudSettings m_cloudSettings;

		public bool toggleClouds = false;


        public RenderedClouds(GameObject sun, Mesh m, PlanetSettings ps, CloudSettings cs)
        {
            planetSettings = ps;
            m_sun = sun;
            m_skyMesh = m;
            m_cloudSettings = cs;
            InitializeParameters();

            m_innerRadius = planetSettings.radius;
            m_outerRadius = planetSettings.radius * planetSettings.outerRadiusScale;

            //		m_radius = m_outerRadius;//planetSettings.radius*planetSettings.cloudRadius;	
            m_radius = planetSettings.radius * planetSettings.renderedCloudRadius;

            cs.Initialize((Material)Resources.Load("RenderedClouds"), ps, sun);

            m_skyMaterial = m_cloudSettings.material;
            InitAtmosphereMaterial(m_skyMaterial);

            m_innerRadius = planetSettings.radius;
            m_outerRadius = planetSettings.radius * planetSettings.outerRadiusScale;
            m_outerRadius = planetSettings.atmosphereHeight * planetSettings.radius;
            m_radius = m_outerRadius;

            InitializeSkyMesh();


            m_sky.transform.Rotate(new Vector3(90, 0, 0));
        }


        public override void Update()
        {
            if (m_skyMaterial!=null)
                InitAtmosphereMaterial(m_skyMaterial);

            m_skyMaterial.SetFloat("sradius", m_radius);
            //   Debug.Log("WHOOO" + m_skyMaterial.name);

            m_skySphere.GetComponent<MeshRenderer>().enabled = toggleClouds;
        }




    }

}