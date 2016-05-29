using UnityEngine;
using System.Collections;

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


namespace LemonSpawn
{
		public class CTextureNode : CNode
		{

				public Texture2D texture = null;
				//public C2DMap map = null;
				//public C2DRGBMap colors = null;
				public bool updateTexture = true;
		
				public SurfaceNode surfaceNode;
	
				public override void Draw (int ID)
				{
						base.Draw (ID);
			
						buildGUI (types [Type]);
						renderPresets ();
		
					
						GUI.DragWindow ();
						GUI.color = Color.white;
						int d = 12;
						int sz = (int)(window.width / 2 - 2 * d);
						if (texture != null)  
								GUI.DrawTexture (new Rect (rightSize.x + d + 3, 7 + d, sz, sz), texture);
/*			for (int i=0;i<2;i++)
				for (int j=0;j<2;j++)
						GUI.DrawTexture (new Rect (rightSize.x + d+3 + i*window.width/4f,  j*window.width/4, window.width/4, window.width/4), texture);
*/			
				
						//size.y += window.width;
						window.height = Mathf.Max (size.y, sz + 2 * d);
			
						ExtraOnGUI ();				
				
				}
		
				protected virtual void ExtraOnGUI ()
				{
		
				}
		
		}

}