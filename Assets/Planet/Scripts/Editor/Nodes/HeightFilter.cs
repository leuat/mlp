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
/*		public class HeightFilter : CTextureNode
		{
		
				public static int TYPE_SMOOTH = 0;
				public static int TYPE_SCALE = 1;
				public static int TYPE_POWER = 2;
				public static int TYPE_CONTOURS = 3;
				public static int TYPE_CLAMP = 4;
				public static int TYPE_PIXEL = 5;
		
				public override void Initialize (int windowID, int type, int x, int y)
				{
			
						status_after_click = CNodeManager.STATUS_NONE;
						InitializeWindow (windowID, type, x, y, "Heightmap Filter");		
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
						types = new string[] {
								"Blur",
								"Scale",
								"Power",
								"Contour",
								"Clamp",
								"Pixelate"
						};
						texture = new Texture2D (C2DMap.sizeX, C2DMap.sizeY);						
			
						helpMessage = "<size=24>" + LStyle.hexColors [1] + "Height map filter. </color></size>" +
								"\n" +
								"\n" +
								"This node is a filter that takes one heightmap input and transforms the map according to various mathematical procedures, producing an output. " +
								"\n" +
								"<size=15>" + LStyle.hexColors [2] + "Blur:</color></size>\n" +
								"  " + LStyle.hexColors [1] + "Amplitude</color>: Controls the amplitude of the output signal. \n" +
								"  " + LStyle.hexColors [1] + "Radius</color>: Scaled pixel distance of blur radius. \n" + 
								"  " + LStyle.hexColors [1] + "Type</color>: Type of Blur - normal (XY) is 0, Y-only is 0.66 and X-only i 1. \n" + 
								"\n" +
								"<size=15>" + LStyle.hexColors [2] + "Scale:</color></size>\n" +
								"  " + LStyle.hexColors [1] + "Amplitude</color>: Controls the amplitude of the output signal. \n" +
								"  " + LStyle.hexColors [1] + "Scale</color>: Multiplied with the value. Can be negative (for inversion). \n" + 
								"  " + LStyle.hexColors [1] + "Offset</color>: Shifts the input value by offset. \n" + 
								"\n" +
								"<size=15>" + LStyle.hexColors [2] + "Power:</color></size>\n" +
								"  " + LStyle.hexColors [1] + "Amplitude</color>: Controls the amplitude of the output signal. \n" +
								"  " + LStyle.hexColors [1] + "Power</color>: Calculates new_value = old_value^power. \n" + 
								"  " + LStyle.hexColors [1] + "Offset</color>: Shifts the input value by offset. \n" + 
								"\n" +
								"<size=15>" + LStyle.hexColors [2] + "Contour:</color></size>\n" +
								"  " + LStyle.hexColors [1] + "Amplitude</color>: Controls the amplitude of the output signal. \n" +
								"  " + LStyle.hexColors [1] + "Contour</color>: Calculates the normal for each pixel and sets the z-component value of the normal. \n" + 
								"  " + LStyle.hexColors [1] + "Offset</color>: Shifts the input value by offset. \n" + 
								"\n" +
								"<size=15>" + LStyle.hexColors [2] + "Clamp:</color></size>\n" +
								"  " + LStyle.hexColors [1] + "Amplitude</color>: Controls the amplitude of the output signal. \n" +
								"  " + LStyle.hexColors [1] + "Min/Max</color>: Clamps the value between min and max. \n" + 
								"\n" +
								"<size=15>" + LStyle.hexColors [2] + "Pixelate:</color></size>\n" +
								"  " + LStyle.hexColors [1] + "Amplitude</color>: Controls the amplitude of the output signal. \n" +
								"  " + LStyle.hexColors [1] + "Pixel size</color>: The larger value, the larger the pixel \n";
					
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
						map.CopyFrom (m1.map);
						float ascale = 1;
			
						if (Type == TYPE_SMOOTH)  
								map.Smooth ((int)(getValue ("value1") * 40f), (int)(getValue ("value2") * 3f));
						if (Type == TYPE_SCALE)  
								map.ScaleMap ((getValue ("value1") - 0.5f) * 4, 2 * (getValue ("value2") - 0.5f));
						if (Type == TYPE_POWER)  
								map.Pow ((getValue ("value1") - 0.5f) * 3, 2 * (getValue ("value2") - 0.5f));
						if (Type == TYPE_CONTOURS)  
								map.Contour (getValue ("value1") * 100f, 4 * (getValue ("value2") - 0.5f));
						if (Type == TYPE_CLAMP) {
								map.Clamp (getValue ("value1"), (getValue ("value2")));
								ascale *= 5;
						}
						if (Type == TYPE_PIXEL)  
								map.Pixelate (getValue ("value1"));
			
			
						map.ScaleMap (getValue ("amplitude") * ascale, 0);
						GenerateHeightTexture ();
						if (Type != TYPE_SMOOTH)
								CNodeManager.Progress ();
						updateTexture = true;
			
						changed = false;
				}

				protected override void ExtraOnGUI ()
				{
						base.ExtraOnGUI ();
						if (Type == TYPE_SMOOTH) {
								setValue ("value2", ((int)(getValue ("value2") * 3f)) / 3f);
						}
				}
								
				public void setupParameters ()
				{
						parameters ["amplitude"] = new Parameter ("Amplitude:", 1.0f, -1, 1, 0, "amplitude");
						parameters ["value1"] = new Parameter ("Value1:", 0.1f, 0, 1, 1, "value1");
						parameters ["value2"] = new Parameter ("Value2:", 0.1f, 0, 1, 2, "value2");
			
						alternativeNames.Add (new AlternativeName ("value1", "Radius:", TYPE_SMOOTH));
						alternativeNames.Add (new AlternativeName ("value2", "Type", TYPE_SMOOTH));
			
						alternativeNames.Add (new AlternativeName ("value1", "Scale:", TYPE_SCALE));
						alternativeNames.Add (new AlternativeName ("value2", "Offset:", TYPE_SCALE));

						alternativeNames.Add (new AlternativeName ("value1", "Power:", TYPE_POWER));
						alternativeNames.Add (new AlternativeName ("value2", "Offset:", TYPE_POWER));

						alternativeNames.Add (new AlternativeName ("value1", "Contour:", TYPE_CONTOURS));
						alternativeNames.Add (new AlternativeName ("value2", "Offset:", TYPE_CONTOURS));

						alternativeNames.Add (new AlternativeName ("value1", "Min:", TYPE_CLAMP));
						alternativeNames.Add (new AlternativeName ("value2", "Max:", TYPE_CLAMP));

						alternativeNames.Add (new AlternativeName ("value1", "Pixel size:", TYPE_PIXEL));
						alternativeNames.Add (new AlternativeName ("value2", "", TYPE_PIXEL));
			
				}
		
		
		
		}

*/
}
