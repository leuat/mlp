using UnityEngine;
using System.Collections;
using LemonSpawn;
using UnityEngine.SceneManagement;
using UnityEngine.UI;



public class MainApp : MonoBehaviour
{

	// Use this for initialization

	void LoadOSX ()
	{

	}


	void Start ()
	{

		bool ok = false;

		string[] cmd = System.Environment.GetCommandLineArgs();
		//Text text = GameObject.Find ("Text").GetComponent<Text> ();
		//text.text = cmd [0] + " " + cmd [1];
		if (cmd.Length > 1) {
			if (cmd [1] == "mcast") {
				SceneManager.LoadScene (1);
				ok = true;
			}
		
			if (cmd [1] == "ssview") {
				SceneManager.LoadScene (2);
				ok = true;
			}

			if (!ok)
				Application.Quit();
		}
	}
	


	// Update is called once per frame
	void Update ()
	{
		if (Input.GetKey (KeyCode.Escape))
			Application.Quit ();
	}
}
