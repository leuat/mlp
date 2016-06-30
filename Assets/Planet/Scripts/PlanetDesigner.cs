using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine.EventSystems;
using System.Xml.Serialization;


#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;

namespace LemonSpawn
{

   

    public class PlanetDesigner : World
    {

    	private System.Random r;

    	public override void Start() {

    		base.Start();

    		RenderSettings.MoveCam = false;


    	}

		public override void Update() {
			base.Update();


		  }    	

    }


}
