using UnityEngine;
using System.Collections;


namespace LemonSpawn {

	public class Rings {

		private GameObject GO;
		private PlanetSettings planetSettings;
		private Material mat;
		private GameObject m_sun;
		
		public Rings(PlanetSettings ps, GameObject sun) {
			planetSettings = ps;
			
			Material org = (Material)Resources.Load ("RingMaterial");
			mat = new Material(org.shader);
			mat.CopyPropertiesFromMaterial(org);
			m_sun = sun;
			//		mat = org;	
			GO = GameObject.CreatePrimitive(PrimitiveType.Plane);
			GO.name = "Rings";
			GO.transform.position = Vector3.zero;
			GO.transform.parent = planetSettings.gameObject.transform;
			GO.transform.localScale = Vector3.one*planetSettings.radius*1.2f;
			GO.GetComponent<Renderer>().material = mat;
			
		}
				
		
		public void Update() {
			if (mat==null)
				return;
			
			mat.SetColor("_Color", planetSettings.ringColor);		
			mat.SetFloat("amplitude", planetSettings.ringAmplitude);		
			mat.SetFloat("scale", planetSettings.ringScale);		
			mat.SetFloat("radius1", planetSettings.ringRadius.x);		
			mat.SetFloat("radius2", planetSettings.ringRadius.y);		
			mat.SetFloat ("planetRadius", planetSettings.radius);
			
			Quaternion rot = Quaternion.Inverse(GO.transform.parent.localRotation);
			
			mat.SetVector("lightDir",  rot*(m_sun.transform.forward*-1.0f));
			
		}


	}
}