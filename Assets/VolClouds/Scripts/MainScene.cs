using UnityEngine;
using System.Collections;

public class MainScene : MonoBehaviour {

	// Use this for initialization

    private Vector3 cameraAcc = Vector3.zero;
    private Vector3 sunRotation = new Vector3(25, 300, 0);
    private float moveScale = 0.5f;

	void Start () {

	}

    void OnGUI() {

        int w = 400;
        int dy = 25;
        sunRotation.x = GUI.HorizontalSlider(new Rect(25, 25, w, 30), sunRotation.x, 0.0F, 360.0F);
        sunRotation.y = GUI.HorizontalSlider(new Rect(25, 25+dy, w, 30), sunRotation.y, 0.0F, 360.0F);
        GUI.Label(new Rect(0, 0, 100, 100), "FPS: "+(int)(1.0f / Time.smoothDeltaTime)); 

    }
	
	// Update is called once per frame
	void Update () {

        if (Input.GetButton("Fire2"))
           cameraAcc += (Vector3.right*Input.GetAxis("Mouse X") + Vector3.up*Input.GetAxis("Mouse Y")*-1)*moveScale;

        Camera c = GameObject.Find("Main Camera").GetComponent<Camera>();
        if (c==null)
            return;
            c.transform.position = c.transform.position +cameraAcc;
        cameraAcc *=0.95f;


        GameObject sun = GameObject.Find("Sun");
        if (sun==null)
            return;

        float scale = 50.5f;
        //sun.transform.Rotate(new Vector3(Time.deltaTime*scale, Time.deltaTime*0.52312f*scale, Time.deltaTime*0.82354f*scale));
        sun.transform.rotation = Quaternion.Euler(sunRotation);
	}
}
