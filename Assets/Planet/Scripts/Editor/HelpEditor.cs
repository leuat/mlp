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

namespace LemonSpawn{
		/*
		* Help dialogue editor window class
		*/

		public class HelpEditor : EditorWindow
		{
				static string helpText = "";
				public static void Create (string ht)
				{
						helpText = ht;
						EditorWindow.GetWindow (typeof(HelpEditor));
				}

				GUIStyle textStyle = null;

				void Initialize ()
				{
						Screen.SetResolution (600, 900, false);
						textStyle = new GUIStyle ();
						textStyle.wordWrap = true;
						textStyle.richText = true;
						textStyle.normal.textColor = Color.gray;
				}

				void OnGUI ()
				{
						Initialize ();
						//GUI.DrawTexture (new Rect (0, 0, Screen.width, Screen.height), TangyTexturesEditor.background);
						GUI.color = 1.8f * LStyle.Colors [3];
						int s = 25;
						GUILayout.BeginVertical ();
						GUILayout.Space (s);

						GUILayout.BeginHorizontal ();
						GUILayout.Space (s);
						GUILayout.TextArea (helpText, textStyle);
						GUILayout.Space (s);
						GUILayout.EndHorizontal ();
						GUILayout.Space (s);

						GUILayout.BeginHorizontal ();
						GUILayout.Space (Screen.width / 2 - 40);
						if (GUILayout.Button ("Close", GUILayout.Width (80))) {
								this.Close ();
						}
						GUILayout.EndHorizontal ();
						GUILayout.EndVertical ();
				}

		}
}
