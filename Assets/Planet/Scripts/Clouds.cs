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
		m_radius = planetSettings.radius*planetSettings.cloudRadius;	
			
		cs.Initialize((Material)Resources.Load ("Clouds"), ps, sun);
		//InitializeMesh();
//		m_sky = m_cloudObject;
		m_skyMaterial = m_cloudSettings.material;
		InitMaterial(m_skyMaterial);
			
		InitializeSkyMesh();
		m_sky.transform.Rotate(new Vector3(90,0,0));
	}
	
		protected override void InitMaterial(Material mat) {
			m_cloudSettings.LS_CloudColor = planetSettings.cloudColor;
			base.InitMaterial(mat);
			
			m_skyMaterial.SetFloat("cloudHeight", m_innerRadius*planetSettings.cloudRadius);
		}
		
		
}
}