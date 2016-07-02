using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;

namespace LemonSpawn
{





    [System.Serializable]
    public class CloudSettings
    {//: MonoBehaviour {

        public float LS_CloudTimeScale = 5;
        public float LS_CloudScale = 0.11f;
        public float LS_CloudScattering = 1.5f;
        public float LS_CloudIntensity = 1.2f;
        public float LS_CloudSharpness = 1.5f;
        public float LS_CloudThickness = 0.65f;
        public float LS_ShadowScale = 0.006f;
        public float LS_LargeVortex = 0.1f;
        public float LS_SmallVortex = 0.02f;
        public float LS_CloudSubScale = 0.5f;
        public Vector3 LS_Stretch = new Vector3(1, 1, 1);
        public float cloudRadius;
        public int LS_HasCloudShadows = 0;
        public float LS_DistScale = 10.0f;
    
        public float LS_CloudShadowStrength = 0.25f;
      

        public PlanetSettings planetSettings;



        private static Texture2D CloudTexture1, CloudTexture2;
        public Quaternion rot = Quaternion.identity;
        public Material material;
        public CloudTexture cTexture = new CloudTexture();
        public GameObject m_sun;

        private List<Material> additionalMaterials = new List<Material>();


        public void GenerateTexture()
        {
            cTexture.RenderCloud();
            CloudTexture1 = cTexture.ToTexture(new Color(1f, 0.9f, 0.7f, 1) * 0.4f);

        }

        public void GenerateSeamless()
        {
            C2DMap m1 = new C2DMap();
            m1.calculatePerlin(0.06f, 2.5f, 10, 0.75f, 0, 0.03f, 3, Vector2.one, true);
            //			m1.Inv (0.1f);
            //			m1.Smooth(1, 0);
            Texture2D t = m1.ToTexture(new Color(1, 1, 1, 1));

            material.SetTexture("_CloudTex", t);

        }

        public void Initialize(Material org, PlanetSettings ps, GameObject sun)
        {
            m_sun = sun;
            material = org;
            planetSettings = ps;

        }

        void Start()
        {

        }

        public void addMaterial(Material mat)
        {
            additionalMaterials.Add(mat);
        }

        public void setMaterial(Material mat)
        {
            mat.SetFloat("ls_time", 2 * Time.time * LS_CloudTimeScale * 0.25f);
            mat.SetFloat("ls_cloudscale", LS_CloudScale);
            mat.SetFloat("ls_cloudscattering", LS_CloudScattering);
            mat.SetFloat("ls_cloudintensity", LS_CloudIntensity);
            mat.SetFloat("ls_cloudsharpness", LS_CloudSharpness);
            mat.SetFloat("ls_shadowscale", LS_ShadowScale);
            mat.SetFloat("ls_cloudthickness", LS_CloudThickness);
            mat.SetVector("ls_cloudcolor", planetSettings.cloudColor);
            mat.SetFloat("ls_distScale", LS_DistScale);
            mat.SetFloat("LS_LargeVortex", LS_LargeVortex);
            mat.SetFloat("LS_SmallVortex", LS_SmallVortex);
            mat.SetVector("stretch", LS_Stretch);
            mat.SetFloat("ls_cloudShadowStrength", LS_CloudShadowStrength);
            mat.SetFloat("ls_cloudSubScale", LS_CloudSubScale);
            mat.SetInt("hasCloudShadows", LS_HasCloudShadows);
            mat.SetTexture("_CloudTex", CloudTexture1);
            mat.SetTexture("_CloudTex2", CloudTexture2);

            mat.SetFloat("cloudRadius", cloudRadius);

        }


        public void Update()
        {

            if (material == null)
                return;

            setMaterial(material);
            foreach (Material m in additionalMaterials)
            {
                setMaterial(m);
            }

        }
    }


}