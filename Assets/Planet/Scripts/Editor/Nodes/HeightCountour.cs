using UnityEngine;
using System.Collections;

/*
 * 
 * © 2014 LemonSpawn. All rights reserved. Source code in this project ("TangyTextures") are not supported under any LemonSpawn standard support program or service. 
 * The scripts are provided AS IS without warranty of any kind. LemonSpawn disclaims all implied warranties including, without limitation, 
 * any implied warranties of merchantability or of fitness for a particular purpose. 
 * 
 * Basically, do what you want with this code, but don't blame us if something blows up. 
 * 
 * 
*/

namespace LemonSpawn
{
/*		public class HeightCountour : CTextureNode
		{
		
				public static int TYPE_CONTOUR = 0;
				Vector3[,] normals = null;
		
				public override void Initialize (int windowID, int type, int x, int y)
				{
			
						status_after_click = CNodeManager.STATUS_NONE;
						InitializeWindow (windowID, type, x, y, "Heightmap Curvature");		
						map = new C2DMap ();
						setupParameters ();
						colors = new C2DRGBMap ();
			
						color = LStyle.getColor(1);
						color.b *= 0.5f;
						Inputs.Add (new CConnection (this, 0, CConnection.TYPE0));
			
						Outputs.Add (new CConnection (this, 1, CConnection.TYPE0));
						Outputs.Add (new CConnection (this, 2, CConnection.TYPE0));
						Outputs.Add (new CConnection (this, 3, CConnection.TYPE0));
						Outputs.Add (new CConnection (this, 4, CConnection.TYPE0));
						types = new string[] {"Curvature"};
						texture = new Texture2D (C2DMap.sizeX, C2DMap.sizeY);						
			
						helpMessage = "<size=24>" + LStyle.hexColors [1] + "Height map curvature. </color></size>" +
								"\n" +
								"\n" +
								"This node is a filter that takes one heightmap input and identifies the lakes, hills and curvature regions. " +
								"\n" +
								"<size=15>" + LStyle.hexColors [2] + "Parameters:</color></size>\n" +
								"  " + LStyle.hexColors [1] + "Amplitude</color>: Controls the amplitude of the output signal. \n" +
								"  " + LStyle.hexColors [1] + "Scale</color>: Amplifies the signal, creating steepers (and narrower) curves. \n" + 
								"  " + LStyle.hexColors [1] + "Type</color>: Type of identification: \n" + 
								"      " + LStyle.hexColors [1] + "1</color>: Hills. \n" + 
								"      " + LStyle.hexColors [1] + "2</color>: Lakes. \n" +
								"      " + LStyle.hexColors [1] + "3</color>: Hills + Lakes. \n" + 
								"      " + LStyle.hexColors [1] + "4</color>: Upward slopes. \n" + 
								"      " + LStyle.hexColors [1] + "5</color>: Downward slopes. \n" + 
								"      " + LStyle.hexColors [1] + "6</color>: All slopes. \n";
					
			
			
				}
		
				protected override void ExtraOnGUI ()
				{
						setValue ("curvtype", (int)getValue ("curvtype"));
				}
		
				public override void Calculate ()
				{
						base.Calculate ();
			
						if (!verified)
								return;
			
						if (!changed) 
								return;
			
						CTextureNode m1 = (CTextureNode)getNode (Inputs, 0);
						if (m1 == null)
								return;
						m1.Calculate ();
		
						float curvtype = getValue ("curvtype");
						float scale = getValue ("scale");
			
		
						if (normals == null)
								normals = new Vector3[C2DMap.sizeX, C2DMap.sizeY];
						if (normals.GetLength (0) != C2DMap.sizeX)
								normals = new Vector3[C2DMap.sizeX, C2DMap.sizeY];
			
						m1.map.calculateNormals (100, normals);
			
		
						for (int i=0; i<C2DMap.sizeX; i++)
								for (int j=0; j<C2DMap.sizeY; j++) {
										map [i, j] = m1.map.getCurvature (i, j, scale, (int)curvtype, normals);					
								}
						//map.Smooth(1);
			
					
						map.ScaleMap (getValue ("amplitude"), 0);
						GenerateHeightTexture ();
						CNodeManager.Progress ();
						updateTexture = true;
			
						changed = false;
				}
		
				public void setupParameters ()
				{
						parameters ["curvtype"] = new Parameter ("Type", 1.0f, 1.0f, 6.0f, 0, "curvtype");
						parameters ["scale"] = new Parameter ("Scale", 0.0f, 0.0f, 50.0f, 1, "scale");
						parameters ["amplitude"] = new Parameter ("Amplitude", 1.0f, 0.0f, 1.0f, 2, "amplitude");
			
				}
		
		
		
		}
	
	*/
}
