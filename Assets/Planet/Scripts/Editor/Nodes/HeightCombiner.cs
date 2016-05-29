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
/*		public class HeightCombiner : CTextureNode
		{
		
				public static int TYPE_BLEND = 0;
				public static int TYPE_MUL = 1;
				public static int TYPE_SUB = 2;
				public static int TYPE_MIN = 3;
				public static int TYPE_MAX = 4;
			
		
				public override void Initialize (int windowID, int type, int x, int y)
				{
			
						status_after_click = CNodeManager.STATUS_NONE;
						InitializeWindow (windowID, type, x, y, "Heightmap Combiner");		
						setupParameters ();
						
						color = LStyle.Colors[1];//LStyle.getColor(1);
						color.b *= 0.5f;
			
						Outputs.Add (new CConnection (this, 0, CConnection.TYPE0));
						Outputs.Add (new CConnection (this, 1, CConnection.TYPE0));
						Outputs.Add (new CConnection (this, 2, CConnection.TYPE0));
						Outputs.Add (new CConnection (this, 3, CConnection.TYPE0));
						Inputs.Add (new CConnection (this, 4, CConnection.TYPE0));
						Inputs.Add (new CConnection (this, 5, CConnection.TYPE0));
						Inputs.Add (new CConnection (this, 6, CConnection.TYPE0));
						types = new string[] {"Blend","Multiply", "Subtract", "Min", "Max"};
						texture = new Texture2D (C2DMap.sizeX, C2DMap.sizeY);						
			
						helpMessage = "<size=24>" + LStyle.hexColors [1] + "Height map combiner. </color></size>" +
								"\n" +
								"\n" +
								"This node takes two compulsory height map as input and cobines them through various expressions.\n" + 
								"If the upper input is connected to a heightmap source, the result is blended using this map.\n " +
								".\n " +
								"There are three methods of combining maps: by addition (blend), subraction and multiplication.\n " +
								"\n" +
								"<size=15>" + LStyle.hexColors [2] + "Parameters:</color></size>\n" +
								"  " + LStyle.hexColors [1] + "Amplitude</color>: Controls the amplitude of the output signal. \n" +
								"  " + LStyle.hexColors [1] + "Blendval</color>: Weights the two input signals between 0-100% (does not apply for multiply). \n";
			
			
				}
		
				public override void Calculate ()
				{
						//base.Calculate();
			
						if (!verified)
								return;
			
						if (!changed) 
								return;
			
			
						CTextureNode combiner = (CTextureNode)getNode (Inputs, 0);
						CTextureNode m1 = (CTextureNode)getNode (Inputs, 1);
						CTextureNode m2 = (CTextureNode)getNode (Inputs, 2);
						C2DMap combineMap = null;
			
						if (combiner != null) {
								combiner.Calculate ();
								combineMap = combiner.map;
						}
						if (m1 == null || m2 == null) {
								verified = false;
								return;
						}
						m1.Calculate ();						
						m2.Calculate ();						
			
						map.Combine (m1.map, m2.map, combineMap, getValue ("blendval"), Type);
						updateTexture = true;
			
//			map.Normalize(getValue("amplitude"));
						map.ScaleMap (getValue ("amplitude"), 0);
						GenerateHeightTexture ();
						CNodeManager.Progress ();
						changed = false;
			
				}
		
				public override void Verify ()
				{
						verified = true;
						int cnt = 0;
						foreach (CConnection c in Inputs) {
								if (c.pointer == null) 
										verified = false;
								if (cnt == 0)
										verified = true;
								cnt++;			
						}
						setupAlternativeNames ();
				}
		
				public void setupParameters ()
				{
						parameters ["amplitude"] = new Parameter ("Amplitude:", 1.0f, 0, 2f, 0, "amplitude");
						parameters ["blendval"] = new Parameter ("Blendval:", 0.5f, 0f, 1, 1, "blendval");
				
			
				}
		
		}
		*/
}