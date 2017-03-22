using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace LemonSpawn
{


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
        public float m_kr = 0.0025f;            // Rayleigh scattering constant
        public float m_km = 0.0010f;            // Mie scattering constant
        public float m_g = -0.990f;             // The Mie phase asymmetry factor, must be between 0.999 to -0.999
                                                //       public float m_g = .990f;             // The Mie phase asymmetry factor, must be between 0.999 to -0.999
        protected float m_radius;
        //Dont change these
        //	private float m_outerScaleFactor = 1.025f; // Difference between inner and ounter radius. Must be 2.5%
        protected float m_innerRadius;          // Radius of the ground sphere
        protected float m_outerRadius;          // Radius of the sky sphere
        private float m_scaleDepth = 0.25f;     // The scale depth (i.e. the altitude at which the atmosphere's average density is found)
        public static float sunScale = 1;
        protected Quaternion rot = Quaternion.identity;
        protected Texture2D noiseTexture = null;

        protected List<Material> affectedMaterials = new List<Material>();


        public void addAffectedMaterial(Material mat) {
            affectedMaterials.Add(mat);
        }
        public void removeAffectedMaterial(Material mat) {
            affectedMaterials.Remove(mat);
        }

        public void UpdateAffectedMaterials() {
            foreach (Material mat in affectedMaterials)
                InitAtmosphereMaterial(mat);
        }


        protected void InitializeSkyMeshSphere(float radius)
        {
            m_sky = new GameObject("Atmosphere");
            m_skySphere = new GameObject("Atmosphere Sky");
            m_sky.transform.parent = planetSettings.gameObject.transform;
            m_skySphere.transform.parent = m_sky.transform;
            m_sky.transform.localPosition = Vector3.zero;

            m_sky.transform.localScale = new Vector3(radius, radius, radius);
            m_skySphere.AddComponent<MeshRenderer>().material = m_skyMaterial;
            m_skySphere.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            m_skySphere.GetComponent<MeshRenderer>().receiveShadows = false;
            MeshFilter mf = m_skySphere.AddComponent<MeshFilter>();

            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);


            mf.mesh = go.GetComponent<MeshFilter>().mesh;
            GameObject.DestroyImmediate(go);
            m_skySphere.transform.localScale = Vector3.one * 2;

        }

        protected void InitializeSkyMesh(float radius)
        {
            m_sky = new GameObject("Atmosphere");
            m_skySphere = new GameObject("Atmosphere Sky");
            m_sky.transform.parent = planetSettings.gameObject.transform;
            m_skySphere.transform.parent = m_sky.transform;
            m_sky.transform.localPosition = Vector3.zero;

            m_sky.transform.localScale = new Vector3(radius, radius, radius);
            m_skySphere.AddComponent<MeshRenderer>().material = m_skyMaterial;
            m_skySphere.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            m_skySphere.GetComponent<MeshRenderer>().receiveShadows = false;
            MeshFilter mf = m_skySphere.AddComponent<MeshFilter>();
            mf.mesh = m_skyMesh;

        }

        public Atmosphere()
        {
        }

        public void initGroundMaterial(bool bump, Material mat)
        {
            


            mat.SetColor("hillColor", planetSettings.m_hillColor);
            mat.SetColor("middleColor", planetSettings.m_surfaceColor);
            mat.SetColor("middleColor2", planetSettings.m_surfaceColor2);
            mat.SetColor("basinColor", planetSettings.m_basinColor);
            //            Debug.Log(planetSettings.m_basinColor);
            mat.SetColor("basinColor2", planetSettings.m_basinColor2);
            mat.SetColor("waterColor", planetSettings.m_waterColor);
            mat.SetFloat("atmosphereDensity", planetSettings.atmosphereDensity);
            mat.SetColor("topColor", planetSettings.m_topColor);
            if (bump)
                mat.SetTexture("_BumpMap", planetSettings.bumpMap);
            mat.SetFloat("metallicity", planetSettings.metallicity);
            mat.SetFloat("_Metallic", planetSettings.metallicity);
            mat.SetFloat("hillyThreshold", planetSettings.hillyThreshold);
            mat.SetFloat("_BumpScale", planetSettings.bumpScale);
            mat.SetColor("_EmissionColor", planetSettings.emissionColor);
            mat.SetFloat("_EmissionScaleUI", 0.2f);
            mat.SetFloat("_Glossiness", planetSettings.specularity);

            mat.SetFloat("liquidThreshold", planetSettings.liquidThreshold);
            mat.SetFloat("topThreshold", planetSettings.topThreshold);
            mat.SetFloat("basinThreshold", planetSettings.basinThreshold);
//            mat.SetTexture("_Noise", noiseTexture);
            //	mat.SetTexture ("_DetailNormalMap", planetSettings.bumpMap);
            //	mat.SetFloat ("_DetailNormalMapScale", planetSettings.bumpScale);	
            mat.shaderKeywords = new string[3] { "_DETAIL_MULX2", "_NORMALMAP", "_EMISSION" };
            //	mat.mainTextureScale.Set (1350,1350);
            if (bump)
            {
                mat.SetTextureScale("_BumpMap", new Vector2(0.1f, 0.1f));
                mat.SetTextureOffset("_BumpMap", new Vector2(1, 1));
            }
            if (planetSettings.m_hillTexture != null)
                mat.SetTexture("_Mountain", planetSettings.m_hillTexture);
            if (planetSettings.m_basinTexture != null)
                mat.SetTexture("_Basin", planetSettings.m_basinTexture);
            if (planetSettings.m_topTexture != null)
                mat.SetTexture("_Top", planetSettings.m_topTexture);
            if (planetSettings.m_surfaceTexture != null)
                mat.SetTexture("_Surface", planetSettings.m_surfaceTexture);

        }

        private static Texture2D mountain, basin, top, surface;


        public void InitializeDefaultTextures(Material mat)
        {
            if (mountain == null)
                mountain = (Texture2D)Resources.Load("Textures/seamless_rock");
            if (basin == null)
                basin = (Texture2D)Resources.Load("Textures/seamless_drough");
            if (top == null)
                top = (Texture2D)Resources.Load("Textures/snow");
            if (surface == null)
                surface = (Texture2D)Resources.Load("Textures/seamless_dirt");
            m_groundMaterial.SetTexture("_Mountain", mountain);
            m_groundMaterial.SetTexture("_Basin", basin);
            m_groundMaterial.SetTexture("_Top", top);
            m_groundMaterial.SetTexture("_Surface", surface);
        }


        public void InitializeParameters(float radius)
        {

            //m_kr*=pSettings.planetType.atmosphereDensity;
            m_kr *= planetSettings.atmosphereDensity;
            //		m_scaleDepth/=pSettings.atmosphereHeight;
            m_outerRadius = radius * planetSettings.outerRadiusScale;
            m_innerRadius = radius;

            if (noiseTexture == null)
                noiseTexture = (Texture2D)Resources.Load("NoiseTexture");

        }


        public void ReinitializeGroundMaterial(Material ground)
        {
            m_groundMaterial = new Material(ground.shader);
            m_groundMaterial.CopyPropertiesFromMaterial(ground);
            initGroundMaterial(true, m_groundMaterial);
            InitAtmosphereMaterial(m_groundMaterial);
            InitializeDefaultTextures(m_groundMaterial);

        }


        public Atmosphere(GameObject sun, Material ground, Material sky, Mesh sphere, PlanetSettings pSettings)
        {

            planetSettings = pSettings;
            if (pSettings == null)
                return;

            InitializeParameters(planetSettings.radius);
            //Get the radius of the sphere. This presumes that the sphere mesh is a unit sphere (radius of 1)
            //that has been scaled uniformly on the x, y and z axis

            m_sun = sun;
            m_groundMaterial = new Material(ground.shader);
            m_groundMaterial.CopyPropertiesFromMaterial(ground);
            initGroundMaterial(true, m_groundMaterial);



            m_skyMaterial = new Material(sky.shader);
            m_skyMesh = sphere;
            //The outer sphere must be 2.5% larger that the inner sphere
            //m_outerScaleFactor = m_skySphere.transform.localScale.x;
            InitAtmosphereMaterial(m_groundMaterial);
            InitAtmosphereMaterial(m_skyMaterial);
            InitializeSkyMesh(m_outerRadius);

            InitializeDefaultTextures(m_groundMaterial);

            if (m_sky != null)
                localscale = m_sky.transform.parent.localScale.x;

        }

        public virtual void Update()
        {
            if (planetSettings.bumpMap != null && m_groundMaterial != null)
            {
                m_groundMaterial.SetTexture("_BumpMap", planetSettings.bumpMap);
            }

            iscale = Mathf.Clamp(0.99f + planetSettings.getScaledHeight() * 0.0001f, 0, 1);
            if (m_groundMaterial != null)
                m_groundMaterial.SetFloat("time", Time.time);


            iscale = 1f;
            if (m_groundMaterial != null)
            {
//                DebugLog();
                InitAtmosphereMaterial(m_groundMaterial);
                initGroundMaterial(true, m_groundMaterial);

            }
            iscale = 1;
            if (m_skyMaterial != null)
                InitAtmosphereMaterial(m_skyMaterial);

            UpdateAffectedMaterials();

        }

        public void setClippingPlanes()
        {
            if (!World.SzWorld.useSpaceCamera)
                return;
            float h = planetSettings.properties.localCamera.magnitude - m_innerRadius;
            
            float np = Mathf.Max(Mathf.Min(h * 0.01f, 50), 0.1f);
            float fp = Mathf.Min(Mathf.Max(h * 150, 100000), 200000);
            //		Debug.Log (np);
            //		np = 10f;
            if (World.CloseCamera != null) {
                World.CloseCamera.nearClipPlane = np;
                World.CloseCamera.farClipPlane = fp;
            }
            //		Debug.Log (np);

        }

        protected float iscale = 1;


        public void DebugLog()
        {
            Debug.Log("waveLength: " + planetSettings.m_atmosphereWavelengths);
            Debug.Log("LightPos:" + m_sun.transform.forward);
            Debug.Log("OuterRadius:" + m_outerRadius);
            Debug.Log("InnerRadius:" + m_innerRadius);
            Debug.Log("ScaleDepth:" + m_scaleDepth);
            Debug.Log("Exp:" + planetSettings.m_hdrExposure);
            Debug.Log("g:" + m_g);
            Debug.Log("Translate:" + planetSettings.transform.position);
            Debug.Log("atm density:" + planetSettings.atmosphereDensity);
        }

        public Matrix4x4 rotMat = new Matrix4x4();
        Matrix4x4 rotMatInv = new Matrix4x4();

        public virtual void InitAtmosphereMaterial(Material mat)
        {

            localscale = planetSettings.transform.parent.localScale.x;
            // Debug.Log(localscale);
            //rot = Quaternion.Inverse(m_sky.transform.parent.localRotation);
            rot = Quaternion.Inverse(planetSettings.transform.rotation);
            rotMat = Matrix4x4.TRS(Vector3.zero, rot, Vector3.one);
           

            float ds = localscale;
            Vector3 invWaveLength4 = new Vector3(1.0f / Mathf.Pow(planetSettings.m_atmosphereWavelengths.x, 4.0f), 1.0f / Mathf.Pow(planetSettings.m_atmosphereWavelengths.y, 4.0f), 1.0f / Mathf.Pow(planetSettings.m_atmosphereWavelengths.z, 4.0f));
            float scale = 1.0f / (m_outerRadius - m_innerRadius);

            Vector3 lightDir = (m_sun.transform.forward * -1.0f);

            if (RenderSettings.usePointLightSource)
                lightDir =  (m_sun.transform.position - planetSettings.gameObject.transform.position).normalized;

            mat.SetVector("v3LightPos", lightDir);
            float rscale = planetSettings.gameObject.transform.localScale.x;
            mat.SetVector("lightDir", rot * lightDir);
            mat.SetVector("v3InvWavelength", invWaveLength4);
            mat.SetFloat("fOuterRadius", m_outerRadius * ds*rscale);
            mat.SetFloat("fOuterRadius2", m_outerRadius * m_outerRadius * ds * ds*rscale*rscale);
            mat.SetFloat("fInnerRadius", m_innerRadius * ds * iscale*rscale);
            mat.SetFloat("fInnerRadius2", m_innerRadius * m_innerRadius * ds*rscale*rscale);
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

            mat.SetVector("surfaceNoiseSettings", planetSettings.ExpSurfSettings);
            mat.SetVector("surfaceNoiseSettings2", planetSettings.ExpSurfSettings2);
            mat.SetVector("surfaceNoiseSettings3", planetSettings.ExpSurfSettings3);
            mat.SetVector("surfaceNoiseSettings4", planetSettings.ExpSurfSettings4);
            mat.SetVector("surfaceNoiseSettings5", planetSettings.ExpSurfSettings5);
            mat.SetVector("surfaceNoiseSettings6", planetSettings.ExpSurfSettings6);
            mat.SetVector("surfaceVortex1", planetSettings.SurfaceVortex1);
            mat.SetVector("surfaceVortex2", planetSettings.SurfaceVortex2);
            mat.SetMatrix("rotMatrix", rotMat);


           /*            Debug.Log("exposure:" + planetSettings.m_hdrExposure);
                        Debug.Log("sun:" + planetSettings.m_ESun);
                        Debug.Log("l:" + planetSettings.m_atmosphereWavelengths);
                        */
        }
    }

}

