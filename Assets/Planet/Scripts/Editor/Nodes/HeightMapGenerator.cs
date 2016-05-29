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
/*
		public class HeightMapGenerator : CTextureNode
		{

				public static int TYPE_PERLIN = 0;
				public static int TYPE_MULTIRIDGED = 1;
				public static int TYPE_SWISS = 2;
				public static int TYPE_PERLINCLOUDS = 3;
		
				public override void Initialize (int windowID, int type, int x, int y)
				{
			
						status_after_click = CNodeManager.STATUS_NONE;
						InitializeWindow (windowID, type, x, y, "Heightmap Generator");		
						setupParameters ();
			
						color = LStyle.getColor(1);
						color.b *= 0.5f;
			
						displayTypes = false;
						
//						surfaceNode = new SurfaceGenerator
				
						
						Outputs.Add (new CConnection (this, 0, CConnection.TYPE0));
						Outputs.Add (new CConnection (this, 1, CConnection.TYPE0));
						Outputs.Add (new CConnection (this, 2, CConnection.TYPE0));
						Outputs.Add (new CConnection (this, 3, CConnection.TYPE0));
						types = new string[] {"Perlin","Multiridged", "Swiss", "PerlinClouds"};
						texture = new Texture2D (C2DMap.sizeX, C2DMap.sizeY);						
			
						if (type == TYPE_PERLIN) {
								helpMessage = "<size=24>" + LStyle.hexColors [1] + "Perlin texture generator. </color></size>" +
										"\n" +
										"\n" +
										"This node is a generator that produces a standard perlin pattern. " +
										"\n" +
										"<size=15>" + LStyle.hexColors [2] + "Parameters:</color></size>\n" +
										"  " + LStyle.hexColors [1] + "Amplitude</color>: Controls the overall height of the fluctuations. \n" +
										"  " + LStyle.hexColors [1] + "Octaves</color>: The number of different scales. 1 octave produces fluctuations of roughly the same size. A higher number produces a rich landscape consisting of both small and large structures. Note that more octaves are more computationally demanding. \n" + 
										"  " + LStyle.hexColors [1] + "Scale</color>: The starting width of the fluctuations. Low scale produces wide peaks, high scale produces dense, narrow peaks. \n" + 
										"  " + LStyle.hexColors [1] + "Damping</color>: Damping of small scales. A higher number will more strongly suppress the height of small structures. \n" +
										"  " + LStyle.hexColors [1] + "Seed</color>: The initial random seed for this generator. Change this to produce a different pattern with similar properties.\n" +
										"  " + LStyle.hexColors [1] + "Skew</color>: The perlin skewing amplitude. Increase for stronger circular patterns.\n" + 
										"  " + LStyle.hexColors [1] + "Skewscale</color>: The scale of the circular skewing modification.\n";
						}
						if (type == TYPE_PERLINCLOUDS) {
								helpMessage = "<size=24>" + LStyle.hexColors [1] + "Perlin cloud texture generator. </color></size>" +
										"\n" +
										"\n" +
										"This node is a generator that produces a modified perlin method, yielding a cloud-like pattern. " +
										"\n" +
										"<size=15>" + LStyle.hexColors [2] + "Parameters:</color></size>\n" +
										"  " + LStyle.hexColors [1] + "Amplitude</color>: Controls the overall height of the fluctuations. \n" +
										"  " + LStyle.hexColors [1] + "Octaves</color>: The number of different scales. 1 octave produces fluctuations of roughly the same size. A higher number produces a rich landscape consisting of both small and large structures. Note that more octaves are more computationally demanding. \n" + 
										"  " + LStyle.hexColors [1] + "Scale</color>: The starting width of the fluctuations. Low scale produces wide peaks, high scale produces dense, narrow peaks. \n" + 
										"  " + LStyle.hexColors [1] + "Damping</color>: Damping of small scales. A higher number will more strongly suppress the height of small structures. \n" +
										"  " + LStyle.hexColors [1] + "Seed</color>: The initial random seed for this generator. Change this to produce a different pattern with similar properties.\n" +
										"  " + LStyle.hexColors [1] + "Skew</color>: The perlin skewing amplitude. Increase for stronger circular patterns.\n" + 
										"  " + LStyle.hexColors [1] + "Skewscale</color>: The scale of the circular skewing modification.\n" + 
										"  " + LStyle.hexColors [1] + "Power</color>: Either smooths or intensifies the pattern by taking the power of the value.\n";
						}
						if (type == TYPE_MULTIRIDGED) {
								helpMessage = "<size=24>" + LStyle.hexColors [1] + "Multiridged texture generator. </color></size>" +
										"\n" +
										"\n" +
										"This node is a generator that produces an advanced pattern with ridges, lakes and hills. " +
										"\n" +
										"<size=15>" + LStyle.hexColors [2] + "Parameters:</color></size>\n" +
										"  " + LStyle.hexColors [1] + "Amplitude</color>: Controls the overall height of the fluctuations. \n" +
										"  " + LStyle.hexColors [1] + "Gain</color>: How 'pointy' the peaks are. Low gain for hilly pattern, high gain for Mordor. Parameter diverges, so play carefully. \n" +
										"  " + LStyle.hexColors [1] + "Offset</color>: Moves the entire signal up and down. Interfers strongly with gain, so tune this carefully. \n" +
										"  " + LStyle.hexColors [1] + "Offset2</color>: Internal parameter that changes the offset of the calculated signal. Change from zero to create smoother circle-like patterns. \n" +
										"  " + LStyle.hexColors [1] + "Lacunarity</color>: Controls the behaviour of small structures. Low lacunarity gives a smooth, hilly signal, high lacunarity gives lots of small structures, and flat lake areas. Parameter diverges, so play carefully. \n" +
										"  " + LStyle.hexColors [1] + "Scale</color>: The width of the peaks. Low scale produces wide fluctuations, high scale produces many narrow peaks. \n" + 
										"  " + LStyle.hexColors [1] + "Seed</color>: The initial random seed for this generator. Change this to produce a different pattern with similar properties";
				
						}
						if (type == TYPE_SWISS) {
			
								helpMessage = "<size=24>" + LStyle.hexColors [1] + "Swiss texture generator. </color></size>" +
										"\n" +
										"\n" +
										"This node is a generator that produces an advanced pattern with ridges, lakes and hills. " +
										"\n" +
										"<size=15>" + LStyle.hexColors [2] + "Parameters:</color></size>\n" +
										"  " + LStyle.hexColors [1] + "Amplitude</color>: Controls the overall height of the signal. \n" +
										"  " + LStyle.hexColors [1] + "Gain</color>:  How 'pointy' the fluctuations are. Low gain for hilly pattern, high gain for Mordor. Parameter diverges, so play carefully. \n" +
										"  " + LStyle.hexColors [1] + "Power</color>: Smooths the signal internally. Low power gives a contineous 'low mountain' pattern, high power yields smooth valleys between high peaks. \n" +
										"  " + LStyle.hexColors [1] + "Warp</color>: Defines directon of fake erosion patterns. Warp > 1 produces radial erosion (ravines), warp < 1 produces tangential erosion (circles).  \n" +
										"  " + LStyle.hexColors [1] + "Offset</color>: Moves the entire signal up and down. Interfers strongly with gain, so tune this carefully. \n" +
										"  " + LStyle.hexColors [1] + "Lacunarity</color>: Controls the behaviour of small structures. Low lacunarity gives a smooth, hilly signal, high lacunarity gives lots of small structures, and flat lake areas. Parameter diverges, so play carefully. \n" +
										"  " + LStyle.hexColors [1] + "Scale(x&y)</color>: The width of the peaks in x-y direction. Low scale produces wide fluctuations, high scale produces many narrow peaks. \n" + 
										"  " + LStyle.hexColors [1] + "Seed</color>: The initial random seed for this generator. Change this to produce a different pattern with similar properties";
						}
			
			
				}
				
				public override void Calculate ()
				{
						base.Calculate ();
			
						if (!changed) 
								return;
			
						if (Type == TYPE_PERLIN) 
								map.calculatePerlin (getValue ("amplitude"), getValue ("scale"), getValue ("octaves"), getValue ("kscale"), getValue ("seed"), getValue ("perlin"), getValue ("perlinskewscale"));
						if (Type == TYPE_PERLINCLOUDS) 
								map.calculatePerlinClouds (getValue ("amplitude"), getValue ("scale"), getValue ("octaves"), getValue ("kscale"), getValue ("seed"), getValue ("perlin"), getValue ("perlinskewscale"), getValue ("pow"));
						if (Type == TYPE_MULTIRIDGED) 
								map.calculateMultiridged (
					getValue ("seed"),
					getValue ("amplitude"),
					getValue ("scale") * 0.2f,
					getValue ("lacunarity"),
					getValue ("gain"),
					getValue ("offset"), getValue ("ioffset"));
			
						if (Type == TYPE_SWISS) {
								map.calculateSwiss (
					getValue ("seed"),
					getValue ("amplitude"),
					getValue ("scale") * 0.2f,
					getValue ("scaley") * 0.2f,
					getValue ("lacunarity"),
					getValue ("gain"),
					getValue ("offset"),
					getValue ("warp") * 2f,
					getValue ("power")
								);
//				map.MakeSeamless(getValue("seamless"));
						}
						//map.Normalize(getValue("amplitude"));
						map.ScaleMap (getValue ("amplitude"), 0);
						GenerateHeightTexture ();
						updateTexture = true;
			
						changed = false;
				}
		
				public void setupParameters ()
				{
						if (Type == TYPE_PERLIN) {
								parameters ["amplitude"] = new Parameter ("Amplitude:", 1.0f, 0, 1, 0, "amplitude");
								parameters ["scale"] = new Parameter ("Scale:", 1.0f, 0.1f, 6, 1, "scale");
								parameters ["octaves"] = new Parameter ("Octaves:", 1.0f, 1, 12, 2, "octaves");
								parameters ["kscale"] = new Parameter ("Damping:", 1.0f, 0.1f, 4f, 3, "kscale");
								parameters ["seed"] = new Parameter ("Seed:", 0.0f, 0.0f, 1000f, 4, "seed");
								parameters ["perlin"] = new Parameter ("Skew:", 0.0f, 0.0f, 0.3f, 5, "perlin");
								parameters ["perlinskewscale"] = new Parameter ("Skewsscale:", 1.0f, 0.0f, 10f, 6, "perlinskewscale");
				
								//parameters["kdiv"] = new Parameter("k_div:", 1.0f, 0, 10);
								color = new Color (0.5f, 1.0f, 0.7f);
				
						}
						if (Type == TYPE_PERLINCLOUDS) {
								parameters ["amplitude"] = new Parameter ("Amplitude:", 1.0f, 0, 1, 0, "amplitude");
								parameters ["scale"] = new Parameter ("Scale:", 1.0f, 0.1f, 4, 1, "scale");
								parameters ["octaves"] = new Parameter ("Octaves:", 1.0f, 1, 12, 2, "octaves");
								parameters ["kscale"] = new Parameter ("Damping:", 1.0f, 0.1f, 4f, 3, "kscale");
								parameters ["seed"] = new Parameter ("Seed:", 0.0f, 0.0f, 1000f, 4, "seed");
								parameters ["perlin"] = new Parameter ("Skew:", 0.0f, 0.0f, 0.3f, 5, "perlin");
								parameters ["perlinskewscale"] = new Parameter ("Skewsscale:", 1.0f, 0.0f, 10f, 6, "perlinskewscale");
								parameters ["pow"] = new Parameter ("Power:", 1, -1f, 1f, 7, "pow");
				
								//parameters["kdiv"] = new Parameter("k_div:", 1.0f, 0, 10);
								color = new Color (0.5f, 1.0f, 0.7f);
				
						}
						if (Type == TYPE_MULTIRIDGED) {
								color = new Color (0.8f, 1.0f, 0.4f);
								parameters ["seed"] = new Parameter ("Seed:", 0.0f, 0, 1000, 1, "seed");
								parameters ["amplitude"] = new Parameter ("Amplitude:", 0.5f, 0, 1, 0, "amplitude");
								parameters ["scale"] = new Parameter ("Scale:", 4.0f, 0.1f, 30, 3, "scale");
								parameters ["lacunarity"] = new Parameter ("Lacunarity:", 2.7f, 0, 8, 4, "lacunarity");
								parameters ["offset"] = new Parameter ("Offset:", 0.1f, 0, 4, 5, "offset");
								parameters ["ioffset"] = new Parameter ("Offset2:", 0.0f, -1, 1, 6, "ioffset");
								parameters ["gain"] = new Parameter ("Gain:", 1.2f, 0, 3f, 7, "gain");
				
						}
						if (Type == TYPE_SWISS) {
								color = new Color (0.2f, 1.0f, 0.2f);
								parameters ["seed"] = new Parameter ("Seed:", 0.0f, 0, 1000, 1, "seed");
								parameters ["amplitude"] = new Parameter ("Amplitude:", 0.37f, 0, 1, 0, "amplitude");
								parameters ["scale"] = new Parameter ("Scalex:", 8.60f, 0.1f, 40, 2, "scale");
								parameters ["scaley"] = new Parameter ("Scaley:", 8.60f, 0.1f, 40, 3, "scaley");
								parameters ["lacunarity"] = new Parameter ("Lacunarity:", 2.83f, 0, 8, 4, "lacunarity");
								parameters ["offset"] = new Parameter ("Offset:", 0.85f, 0, 4, 5, "offset");
								parameters ["gain"] = new Parameter ("Gain:", 0.63f, 0, 3f, 6, "gain");
								parameters ["warp"] = new Parameter ("Warp:", 0.36f + 1.0f, 0, 2f, 7, "warp");
								parameters ["power"] = new Parameter ("Power:", 0.33f, 0, 1f, 8, "power");
//				parameters ["seamless"] = new Parameter ("Seamless:", 0.33f, 0, 10f,9,"seamless");
				
						}
			
				}
		
		

		}
		*/
}