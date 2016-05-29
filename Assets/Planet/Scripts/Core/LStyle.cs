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
		/*
		* LimeStone styles, constants, colors and whatnot. 
		*/
		public class LStyle
		{
				public static int LargeFontSize = 16;
				public static int FontSize = 20;
				public static int ConnectionTypeX = 20;
				public static int ConnectionTypeY = 25;
				public static string PresetsFilename = "Assets/LemonSpawn/Data/Presets/presets.dat";
				public static string SnippetDirectory = "Assets/LemonSpawn/Data/Snippets/";
				public static string HeightMapTexture = "Heightmap";
				public static string NormalMapTexture = "Normalmap";
				public static string ColorMapTexture = "Colormap";
				public static string ResourceDirectory = "Assets/LemonSpawn/Resources/";
				public static string MaterialName = "PreviewMaterial";
				public static Color[] Colors = { 
					new Color (0.3f, 0.25f, 0.2f, 1.0f), 
					new Color (0.8f, 0.2f, 0.1f, 1.0f),
					new Color (0.8f, 1.0f, 0.8f, 1.0f),
					new Color (0.8f, 0.8f, 1.0f, 1.0f) ,
					new Color (0.5f, 0.8f, 2.0f, 1.0f) 
				};
		
		
				public static string objectName ="box";
		
				// Colors of the links
				public static Color[] connectionColors = {
					Colors [1] * 1.5f,
					Colors [4],
					Colors [0] * 3,
					Colors [0] * 2
				};
		
				// Display name of textures		
				public static string[] TextureNames = {
			"Texture 0",
			"Texture 1",
			"Texture 2",
			"Normal map" ,
			"WTF"
		};
				public static string[] hexColors = {
					"<color=" + Util.ColorToHex (Colors [0]) + ">", 
					"<color=" + Util.ColorToHex (Colors [1]) + ">", 
					"<color=" + Util.ColorToHex (Colors [2]) + ">", 
					"<color=" + Util.ColorToHex (Colors [3]) + ">", 
					"<color=" + Util.ColorToHex (Colors [4]) + ">" };
				public static string Filetype = "orb";
				public static string TextureGameobjectName = "LemonSpawn";
		
				// Check if everything is initialized		
				public static int NO_TEXTURES = 3;
		
		}
	
	
	
}