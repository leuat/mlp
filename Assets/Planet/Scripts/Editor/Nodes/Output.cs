using UnityEngine;
using System.Collections;
using UnityEditor;

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
		public class Output : CTextureNode
		{
		
		
				public override void Initialize (int windowID, int type, int x, int y)
				{
			
						status_after_click = CNodeManager.STATUS_NONE;
						InitializeWindow (windowID, type, x, y, "Output");		
						setupParameters ();
						colors = new C2DRGBMap ();
			
			color = LStyle.getColor(1);
						color.b = color.r;	
//			Inputs.Add (new CConnection (this, 0, CConnection.TYPE0));
						Inputs.Add (new CConnection (this, 1, CConnection.TYPE0));
						Inputs.Add (new CConnection (this, 2, CConnection.TYPE1));
						Inputs.Add (new CConnection (this, 3, CConnection.TYPE2));
						types = new string[] {"Output"};
						Type = 0;
						size.x = rightSize.x + 100;
						rightSize.x = size.x - 10;
						window.width = size.x;
			
						helpMessage = "<size=24>" + LStyle.hexColors [1] + "Output node. </color></size>" +
								"\n" +
								"\n" +
								"This node is the primary node that computes the final material using three inputs (heightmap, color map and normal map). Shader parameters are set as follows:" +
								"\n" +
								"<size=15>" + LStyle.hexColors [2] + "Parameters:</color></size>\n" +
								"  " + LStyle.hexColors [1] + "Size</color>: Texture size, from 16 to 2048. Might run out of memory on 2048 patterns though. \n" +
								"  " + LStyle.hexColors [1] + "Diffuse</color>: Color of diffuse lightning of the material. \n" + 
								"  " + LStyle.hexColors [1] + "Specular</color>: Color of specular lightning of the material (shininess). \n" + 
								"  " + LStyle.hexColors [1] + "Parallax</color>: The amplitude of the height mapping for the parallax shader. \n" + 
								"  " + LStyle.hexColors [1] + "Shininess</color>: The shininess of the material. Higher value for more shiny. \n" +
								"  " + LStyle.hexColors [1] + "Tiling</color>: Scaling the size of the texture.\n";
			
			
				}
		
				public void CalculateTextures ()
				{
						CTextureNode Heightmap = (CTextureNode)getNode (Inputs, 0);
						CTextureNode ColorMap = (CTextureNode)getNode (Inputs, 1);
						CTextureNode NormalMap = (CTextureNode)getNode (Inputs, 2);
			
						if (NormalMap == null || ColorMap == null || Heightmap == null)
								return;
						Heightmap.GenerateTexture ();
						ColorMap.GenerateTexture ();
						NormalMap.GenerateTexture ();
			
						MaterialPreviewEditor.SetTextures (NormalMap.texture, Heightmap.texture, ColorMap.texture, MaterialPreviewEditor.currentMaterial);
						MaterialPreviewEditor.forceRepaint ();
						CNodeManager.Progress ();
				}
		
				public override void Calculate ()
				{
						base.Calculate ();
			
//			if (!verified)
//				return;
						if (!changed)
								return;
//			CTextureNode ShininessMap = (CTextureNode)getNode (Inputs, 0);
						CTextureNode Heightmap = (CTextureNode)getNode (Inputs, 0);
						CTextureNode ColorMap = (CTextureNode)getNode (Inputs, 1);
						CTextureNode NormalMap = (CTextureNode)getNode (Inputs, 2);
			
						if (NormalMap == null || ColorMap == null || Heightmap == null)
								return;
			
			
			
						Heightmap.Calculate ();
						ColorMap.Calculate ();
						NormalMap.Calculate ();

			
						changed = false;
				}
		
				public void Export (string file)
				{
			
//			CTextureNode ShininessMap = (CTextureNode)getNode (Inputs, 0);
						CTextureNode HeightMap = (CTextureNode)getNode (Inputs, 0);
						CTextureNode ColorMap = (CTextureNode)getNode (Inputs, 1);
						CTextureNode NormalMap = (CTextureNode)getNode (Inputs, 2);
			
						string FileNormalMap = file + "_Normal";
						string FileColorMap = file + "_Color";
						string FileHeightMap = file + "_Height";
						MaterialPreviewEditor.forceRepaint ();
			
						if (NormalMap != null)
								Util.SaveTextureFile (NormalMap.texture, "", FileNormalMap);
			
						if (ColorMap != null)
								Util.SaveTextureFile (ColorMap.texture, "", FileColorMap);
			
						if (HeightMap != null)
								Util.SaveTextureFile (HeightMap.texture, "", FileHeightMap);
			
						AssetDatabase.Refresh ();
						try {
								Material mat = new Material (file);//MaterialPreviewEditor.currentMaterial;
								AssetDatabase.CreateAsset (mat, file + ".mat");
								mat.shader = MaterialPreviewEditor.currentMaterial.shader;
			
								Texture2D T_hm = (Texture2D)AssetDatabase.LoadAssetAtPath (FileHeightMap + ".png", typeof(Texture2D));
								Texture2D T_c = (Texture2D)AssetDatabase.LoadAssetAtPath (FileColorMap + ".png", typeof(Texture2D));
//			Texture2D T_ex = (Texture2D)AssetDatabase.LoadAssetAtPath(FileExtraMap + ".png", typeof(Texture2D));
								Texture2D T_nr = (Texture2D)AssetDatabase.LoadAssetAtPath (FileNormalMap + ".png", typeof(Texture2D));
								MaterialPreviewEditor.SetTextures (T_nr, T_hm, T_c, mat);
								MaterialPreviewEditor.SetProperties (mat);
						} catch (System.Exception e) {
								Debug.Log (e.Message);
							
						}
			
			
				
			
				}
		
				public override void Verify ()
				{
						verified = true;
						foreach (CConnection c in Inputs)
//				if (i++!=0)
								if (c.pointer == null)
										verified = false;
			
						setupAlternativeNames ();
				}
		
				protected override void ExtraOnGUI ()
				{
						base.ExtraOnGUI ();
						int size = (int)getValue ("size");
						for (int i=4; i<12; i++) {
								int prev = (int)Mathf.Pow (2, i - 1);
								int next = (int)Mathf.Pow (2, i);
								if (size >= prev && size < next) { 
										size = prev;
								}
						}
						setValue ("size", size);
						MaterialPreviewEditor.setParameters (getValue ("parallax"), getValue ("shininess"), getColor ("matcolor"), getColor ("shinecolor"), getValue ("direction"), getValue ("threshold"), getValue ("tiling"));
						int tiling = (int)getValue ("tiling");
						setValue ("tiling", tiling);
			
						if (CNodeManager.status == CNodeManager.STATUS_NONE)
						if (size != C2DMap.sizeX) {
								CNodeManager.status = CNodeManager.STATUS_TASK4;
						}
						window.height += LStyle.FontSize * 3;
			
				}
		
				public void setupParameters ()
				{
		
						parameters ["size"] = new Parameter ("Size:", 256, 16, 2048, 0, "size");
						parameters ["matcolor"] = new Parameter ("ColorDiffuse:", 1, 1, 1, 1, "matcolor");
						parameters ["shinecolor"] = new Parameter ("ColorSpecular:", 0.5f, 0.5f, 0.5f, 2, "shinecolor");
						parameters ["parallax"] = new Parameter ("Parallax:", 0.1f, 0.0f, 0.108f, 3, "parallax");	
						parameters ["shininess"] = new Parameter ("Shininess:", 0.1f, 0.0f, 1.0f, 4, "shininess");	
//			parameters ["threshold"] = new Parameter("Threshold:", 0.0f, 0.0f, 1.0f,5,"threshold");	
//			parameters ["direction"] = new Parameter("Direction:", 1.0f, 0.0f, 1.0f,6,"direction");	
						parameters ["tiling"] = new Parameter ("Tiling:", 1.0f, 1.0f, 20.0f, 7, "tiling");	
			
				}
		
		}
		
		*/
}