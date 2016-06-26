﻿using UnityEngine;
using System.Collections;
using System.IO;

namespace LemonSpawn {





    [System.Serializable]
    public class CloudSettings {//: MonoBehaviour {
	
	public float LS_CloudTimeScale = 5;
	public float LS_CloudScale = 0.11f;
	public float LS_CloudScattering = 1f;
	public float LS_CloudIntensity = 2;
	public float LS_CloudSharpness = 1f;
	public float LS_CloudThickness = 0.6f;
	public float LS_ShadowScale = 0.75f;
	public float LS_DistScale = 10.0f;
        public float LS_LargeVortex = 0.1f;
        public float LS_SmallVortex = 0.02f;
        public Vector3 LS_Stretch = new Vector3(1, 1, 1);



        public Vector3 LS_CloudColor = Vector3.zero;
	public Texture2D cloudTexture;
	public Texture2D LSCloudTexture;
	public Quaternion rot = Quaternion.identity;
	public Material material;		
	public CloudTexture cTexture = new CloudTexture();
	public GameObject m_sun;


    public void RandomizeTerra(System.Random r)
        {
            LS_CloudScale = 0.05f + (float)r.NextDouble() * 0.55f;
//            LS_CloudSharpness = 0.2f + (float)r.NextDouble() * 1.4f;
            LS_CloudScattering = 0.7f + (float)r.NextDouble() * 0.4f;
            LS_LargeVortex = (float)r.NextDouble() * 0.1f;
            //LS_SmallVortex = Random.value * 0.03f;
        }

        public void GenerateTexture() {
		cTexture.RenderCloud();
		LSCloudTexture = cTexture.ToTexture(new Color(1f,0.9f, 0.7f,1)*0.4f);
			
	}

        public void RandomizeGas(System.Random r)
        {
            LS_CloudScattering = (float)(5 + r.NextDouble() * 6f);
            LS_CloudIntensity = 1;


            LS_CloudScale = 0.1f + (float)r.NextDouble() * 0.65f;
            LS_CloudSharpness = 0.45f + (float)r.NextDouble() * 0.1f;
            LS_LargeVortex = (float)r.NextDouble() * 0.5f;
            LS_Stretch.x = 0.1f + (float)r.NextDouble() * 0.3f;
            //LS_SmallVortex = Random.value * 0.03f;

        }


        public void GenerateSeamless() {
			C2DMap m1 = new C2DMap();
			m1.calculatePerlin(0.06f, 2.5f, 10, 0.75f, 0 , 0.03f, 3, Vector2.one, true);
//			m1.Inv (0.1f);
//			m1.Smooth(1, 0);
			Texture2D t = m1.ToTexture(new Color(1,1,1,1));
			
			material.SetTexture("_CloudTex", t);
			
	}
					
	public void Initialize(Material org, PlanetSettings ps, GameObject sun, Vector3 cc) {
//		material = new Material(org.shader);
	//	material.SetTexture("_MainTex", ps.clouds);
		m_sun = sun;
            material = org;
            LS_CloudColor = cc;
		//material.SetTexture("_CloudTex", (Texture)Resources.Load ("cloudsTexture2"));
			
//		GenerateSeamless();
		//	material.SetTexture("_CloudTex", LSCloudTexture);
		}	
	
	void Start() {
			
	}
	
	public void Update () {
	
		if (material==null)
			return;
			
		//LS_CloudColor.Set (1.0f,0.9f,0.8f);	
		material.SetFloat("ls_time", 2*Time.time*LS_CloudTimeScale*0.25f);
		material.SetFloat("ls_cloudscale", LS_CloudScale);
		material.SetFloat("ls_cloudscattering", LS_CloudScattering);
		material.SetFloat("ls_cloudintensity", LS_CloudIntensity);
		material.SetFloat("ls_cloudsharpness", LS_CloudSharpness);
		material.SetFloat("ls_shadowscale", LS_ShadowScale);
		material.SetFloat("ls_cloudthickness", LS_CloudThickness);
		material.SetVector("ls_cloudcolor", LS_CloudColor);
		material.SetFloat("ls_distScale", LS_DistScale);
            material.SetFloat("LS_LargeVortex", LS_LargeVortex);
            material.SetFloat("LS_SmallVortex", LS_SmallVortex);
            material.SetVector("stretch", LS_Stretch);


            //		material.SetVector("lightDir", rot*m_sun.transform.forward*-1.0f*-1);
        }
    }


}