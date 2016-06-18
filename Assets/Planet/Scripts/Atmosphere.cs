using UnityEngine;
using System.Collections;


namespace LemonSpawn {


public class Atmosphere
{
	protected GameObject m_sun;
	
	public Material m_groundMaterial, m_skyMaterial;
	protected GameObject m_skySphere;
	protected GameObject m_sky;
	protected Mesh m_skyMesh;
	protected PlanetSettings planetSettings;
	protected float localscale;
    protected float m_innerRadiusScale = 1f;
	public Vector3 m_waveLength; // Wave length of sun light
	public float m_kr = 0.0025f; 			// Rayleigh scattering constant
	public float m_km = 0.0010f; 			// Mie scattering constant
	public float m_g = -0.990f;             // The Mie phase asymmetry factor, must be between 0.999 to -0.999
 //       public float m_g = .990f;             // The Mie phase asymmetry factor, must be between 0.999 to -0.999
        protected float m_radius;
	//Dont change these
//	private float m_outerScaleFactor = 1.025f; // Difference between inner and ounter radius. Must be 2.5%
	protected float m_innerRadius;		 	// Radius of the ground sphere
	protected float m_outerRadius;		 	// Radius of the sky sphere
	private float m_scaleDepth = 0.25f; 	// The scale depth (i.e. the altitude at which the atmosphere's average density is found)
	public static float sunScale = 1;				
	protected Quaternion rot = Quaternion.identity;
	protected Texture2D noiseTexture = null;
	protected void InitializeSkyMesh() {
		m_sky = new GameObject("Atmosphere");
		m_skySphere = new GameObject("Atmosphere Sky");
		m_sky.transform.parent = planetSettings.gameObject.transform;
		m_skySphere.transform.parent = m_sky.transform;
		m_sky.transform.localPosition = Vector3.zero;
		
		m_sky.transform.localScale = new Vector3(m_radius,m_radius,m_radius);
		m_skySphere.AddComponent<MeshRenderer>().material = m_skyMaterial;
			m_skySphere.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
		m_skySphere.GetComponent<MeshRenderer>().receiveShadows = false;
		MeshFilter mf = m_skySphere.AddComponent<MeshFilter>();
		mf.mesh = m_skyMesh;
		
//		Material m = new Material
//		m.CopyPropertiesFromMaterial(m_groundMaterial);
//		m_groundMaterial = m;		
//		m_groundMaterial.color = planetSettings.m_surfaceColor;
		
	}		
	public Atmosphere() {
	}


        public void initGroundMaterial(bool bump)
        {
            m_groundMaterial.SetColor("middleColor", planetSettings.m_surfaceColor);
            m_groundMaterial.SetColor("middleColor2", planetSettings.m_surfaceColor2);
            m_groundMaterial.SetColor("basinColor", planetSettings.m_basinColor);
            m_groundMaterial.SetColor("basinColor2", planetSettings.m_basinColor2);
            m_groundMaterial.SetColor("basinColor2", planetSettings.m_basinColor2);
            m_groundMaterial.SetColor("waterColor", planetSettings.m_waterColor);
            m_groundMaterial.SetFloat("atmosphereDensity", planetSettings.atmosphereDensity);
            m_groundMaterial.SetColor("topColor", planetSettings.m_topColor);
            if (bump)
            m_groundMaterial.SetTexture("_BumpMap", planetSettings.bumpMap);
            m_groundMaterial.SetFloat("metallicity", planetSettings.metallicity);
            m_groundMaterial.SetFloat("_Metallic", planetSettings.metallicity);
            m_groundMaterial.SetFloat("hillyThreshold", planetSettings.hillyThreshold);
            m_groundMaterial.SetFloat("_BumpScale", planetSettings.bumpScale);
            m_groundMaterial.SetColor("_EmissionColor", planetSettings.emissionColor);
            m_groundMaterial.SetFloat("_EmissionScaleUI", 0.2f);
            m_groundMaterial.SetFloat("_Glossiness", planetSettings.specularity );

            m_groundMaterial.SetFloat("liquidThreshold", planetSettings.liquidThreshold);
            m_groundMaterial.SetTexture("_Noise", noiseTexture);
            //	m_groundMaterial.SetTexture ("_DetailNormalMap", planetSettings.bumpMap);
            //	m_groundMaterial.SetFloat ("_DetailNormalMapScale", planetSettings.bumpScale);	
            m_groundMaterial.shaderKeywords = new string[3] { "_DETAIL_MULX2", "_NORMALMAP", "_EMISSION" };
            //	m_groundMaterial.mainTextureScale.Set (1350,1350);
            if (bump)
            {
                m_groundMaterial.SetTextureScale("_BumpMap", new Vector2(0.1f, 0.1f));
                m_groundMaterial.SetTextureOffset("_BumpMap", new Vector2(1, 1));
            }

        }



        public void InitializeParameters()
        {
            m_waveLength = planetSettings.m_atmosphereWavelengths;

            //m_kr*=pSettings.planetType.atmosphereDensity;
            m_kr *= planetSettings.atmosphereDensity;
            //		m_scaleDepth/=pSettings.atmosphereHeight;


        }

        public Atmosphere (GameObject sun, Material ground, Material sky, Mesh sphere, PlanetSettings pSettings) 
	{
		
		planetSettings = pSettings;
		if (pSettings == null)
			return;

            InitializeParameters();			
		//Get the radius of the sphere. This presumes that the sphere mesh is a unit sphere (radius of 1)
		//that has been scaled uniformly on the x, y and z axis
		float radius = pSettings.radius;
            m_innerRadius = radius * m_innerRadiusScale;
		if (noiseTexture == null)
			noiseTexture = (Texture2D)Resources.Load("NoiseTexture");
		m_sun = sun;
		m_groundMaterial = new Material(ground.shader);
		m_groundMaterial.CopyPropertiesFromMaterial(ground);
        initGroundMaterial(true);
//			_DETAIL_MULX2		
		
		
//		m_groundMaterial = ground;
									
		m_skyMaterial = new Material(sky.shader);
		m_skyMesh = sphere;
		//The outer sphere must be 2.5% larger that the inner sphere
		//m_outerScaleFactor = m_skySphere.transform.localScale.x;
		m_outerRadius = pSettings.outerRadiusScale* radius;
        m_radius = m_outerRadius;		
		InitAtmosphereMaterial(m_groundMaterial);
        InitAtmosphereMaterial(m_skyMaterial);
		InitializeSkyMesh();

//            m_outerRadius = pSettings.outerRadiusScale * radius;

            if (m_sky != null)
    		localscale = m_sky.transform.parent.localScale.x;
		
	}
	
	public virtual void Update () 
	{
		if (planetSettings.bumpMap!=null && m_groundMaterial!=null) {
			m_groundMaterial.SetTexture ("_BumpMap", planetSettings.bumpMap);
		}
			
		iscale = Mathf.Clamp(0.99f + planetSettings.getScaledHeight()*0.0001f,0,1);
		if (m_groundMaterial!=null)
			m_groundMaterial.SetFloat ("time",Time.time);



		iscale = 1f;	
		if (m_groundMaterial!=null)
			InitAtmosphereMaterial(m_groundMaterial);
		iscale = 1;
		if (m_skyMaterial!=null)
            InitAtmosphereMaterial(m_skyMaterial);
		
	}
	
	public void setClippingPlanes() {
		float h = planetSettings.localCamera.magnitude - m_innerRadius;
		float np = Mathf.Max (Mathf.Min (h*0.01f, 50), 0.1f);
            float fp = Mathf.Min(Mathf.Max(h*150, 100000), 200000);
            //		Debug.Log (np);
            //		np = 10f;
            if (World.CloseCamera!=null)
        World.CloseCamera.nearClipPlane = np;
            World.CloseCamera.farClipPlane = fp;
            //		Debug.Log (np);

        }

        protected float iscale = 1;
	
/*	protected virtual void InitMaterial(Material mat)
	{
			if (m_sky == null)
			return;

		localscale = m_sky.transform.parent.localScale.x;
		//rot = Quaternion.Inverse(m_sky.transform.parent.localRotation);
		rot = Quaternion.Inverse(m_sky.transform.rotation);
			
		float ds = localscale;
		Vector3 invWaveLength4 = new Vector3(1.0f / Mathf.Pow(m_waveLength.x, 4.0f), 1.0f / Mathf.Pow(m_waveLength.y, 4.0f), 1.0f / Mathf.Pow(m_waveLength.z, 4.0f));
		float scale = 1.0f / (m_outerRadius - m_innerRadius);
		mat.SetVector("v3LightPos",  (m_sun.transform.forward*-1.0f));
		mat.SetVector("lightDir",  rot*(m_sun.transform.forward*-1.0f));
		mat.SetVector("v3InvWavelength", invWaveLength4);
		mat.SetFloat("fOuterRadius", m_outerRadius*ds);
		mat.SetFloat("fOuterRadius2", m_outerRadius*m_outerRadius*ds*ds);
		mat.SetFloat("fInnerRadius", m_innerRadius*ds*iscale);
		mat.SetFloat("fInnerRadius2", m_innerRadius*m_innerRadius*ds);
		mat.SetFloat("fKrESun", m_kr*planetSettings.m_ESun*sunScale);
		mat.SetFloat("fKmESun", m_km*planetSettings.m_ESun*sunScale);
		mat.SetFloat("fKr4PI", m_kr*4.0f*Mathf.PI);
		mat.SetFloat("fKm4PI", m_km*4.0f*Mathf.PI);
		mat.SetFloat("fScale", scale);	
		mat.SetFloat("fScaleDepth", m_scaleDepth);
		mat.SetFloat("fScaleOverScaleDepth", scale/m_scaleDepth);
		mat.SetFloat("fHdrExposure", planetSettings.m_hdrExposure);
		mat.SetFloat("g", m_g);
		mat.SetFloat("g2", m_g*m_g);
		mat.SetVector("v3Translate", planetSettings.transform.position);
        mat.SetFloat("atmosphereDensity", planetSettings.atmosphereDensity);

        }
        */
        public virtual void InitAtmosphereMaterial(Material mat)
        {


            localscale = planetSettings.transform.parent.localScale.x;
           // Debug.Log(localscale);
            //rot = Quaternion.Inverse(m_sky.transform.parent.localRotation);
            rot = Quaternion.Inverse(planetSettings.transform.rotation);
            
            float ds = localscale;
            Vector3 invWaveLength4 = new Vector3(1.0f / Mathf.Pow(m_waveLength.x, 4.0f), 1.0f / Mathf.Pow(m_waveLength.y, 4.0f), 1.0f / Mathf.Pow(m_waveLength.z, 4.0f));
            float scale = 1.0f / (m_outerRadius - m_innerRadius);
            mat.SetVector("v3LightPos", (m_sun.transform.forward * -1.0f));
            mat.SetVector("lightDir", rot * (m_sun.transform.forward * -1.0f));
            mat.SetVector("v3InvWavelength", invWaveLength4);
            mat.SetFloat("fOuterRadius", m_outerRadius * ds);
            mat.SetFloat("fOuterRadius2", m_outerRadius * m_outerRadius * ds * ds);
            mat.SetFloat("fInnerRadius", m_innerRadius * ds * iscale);
            mat.SetFloat("fInnerRadius2", m_innerRadius * m_innerRadius * ds);
            mat.SetFloat("fKrESun", m_kr * planetSettings.m_ESun); // * sunScale
            mat.SetFloat("fKmESun", m_km * planetSettings.m_ESun);// * sunScale;;
            mat.SetFloat("fKr4PI", m_kr * 4.0f * Mathf.PI);
            mat.SetFloat("fKm4PI", m_km * 4.0f * Mathf.PI);
            mat.SetFloat("fScale", scale);
            mat.SetFloat("fScaleDepth", m_scaleDepth);
            mat.SetFloat("fScaleOverScaleDepth", scale / m_scaleDepth);
            mat.SetFloat("fHdrExposure", planetSettings.m_hdrExposure);
            mat.SetFloat("g", m_g);
            mat.SetFloat("g2", m_g * m_g);
            mat.SetVector("v3Translate", planetSettings.transform.position);
            mat.SetFloat("atmosphereDensity", planetSettings.atmosphereDensity);


/*            Debug.Log("exposure:" + planetSettings.m_hdrExposure);
            Debug.Log("sun:" + planetSettings.m_ESun);
            Debug.Log("l:" + planetSettings.m_atmosphereWavelengths);
            */
        }
    }

}

