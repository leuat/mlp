using UnityEngine;
using System.Collections;
using UnityEditor;

/*
 * 
 * © 2014 LemonSpawn. All rights reserved. Source code in this project ("LemonSpawn") are not supported under any LemonSpawn standard support program or service. 
 * The scripts are provided AS IS without warranty of any kind. LemonSpawn disclaims all implied warranties including, without limitation, 
 * any implied warranties of merchantability or of fitness for a particular purpose. 
 * 
 * Basically, do what you want with this code, but don't blame us if something blows up. 
 * 
 * 
*/

namespace LemonSpawn{
	/*
	*
	* Class that renders links
	*
	*
	*/
	public class CLink
	{

		public float time = 0;
		public float scale = 1f;
		public float width = 4f;
		public CConnection from = null, to = null;
		public Color color = new Color (0.2f, 0.8f, 0.1f);
		
		private Vector2 center = new Vector2 ();
		private Vector2 centerA = new Vector2 ();
		private Vector2 centerB = new Vector2 ();
		//public Color colorTmp = new Color(1,0.2f,0.2f);
		public int drawType = 0;
		
		public void Draw ()
		{
			if (to == null && from == null) {
				Debug.Log("ERROR EMPTY LINK");
				return;
				}
			
			// Just draw a line if one of the connections are not set
			if ((to != null && from == null) || (to == null && from != null)) {

				CConnection n = from;
				if (n == null) {
					n = to;
					}

				Color col = LStyle.connectionColors [from.Type];



				Handles.DrawBezier (n.position, SurfaceEditor.mousePos, 
			                   n.position, 
				               SurfaceEditor.mousePos,
			                   col, null, 3f);

				CNodeManager.ForceRepaint = true;

			}
			
			if (to != null && from != null) {
				float t = 0.5f;
				Color col = LStyle.connectionColors [from.Type];
				center.x = t * (from.position.x) + (1 - t) * (to.position.x);
				center.y = t * (from.position.y) + (1 - t) * (to.position.y);


				t = 0.2f;
				centerA.x = t * (center.x) + (1 - t) * (to.position.x);
				centerA.y = t * (center.y) + (1 - t) * (from.position.y);
				t = 0.2f;
				centerB.x = t * (center.x) + (1 - t) * (from.position.x);
				centerB.y = t * (center.y) + (1 - t) * (to.position.y);

				if (drawType == 1) {
					centerA.x = t * (center.x) + (1 - t) * (from.position.x);
					centerA.y = t * (center.y) + (1 - t) * (to.position.y);
					centerB.x = t * (center.x) + (1 - t) * (to.position.x);
					centerB.y = t * (center.y) + (1 - t) * (from.position.y);

				}
				Handles.DrawBezier (from.position, to.position, 
		                       centerA, centerB,

		                   col, null, width);
			}

		}


	}
}
