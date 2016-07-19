using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class Parameter
{
    public string Name;
    public string ShaderParam;
    public float value;
    public float min;
    public float max;

    public Parameter(string _Name, string _ShaderParam, float _value, float _min, float _max)
    {
        Name = _Name;
        ShaderParam = _ShaderParam;
        value = _value;
        min = _min;
        max = _max;
    }

    public void SetValue(Material m)
    {
        m.SetFloat(ShaderParam, value);
    }
    public void Render(int y, int w, int tw, int margin)
    {
        GUI.Label(new Rect(margin, y, tw, 30), Name + ":");
        value = GUI.HorizontalSlider(new Rect(margin + tw + margin, y, w, 30), value, min,max);
    }

}

public class MainScene : MonoBehaviour {

	// Use this for initialization

    private Vector3 cameraAcc = Vector3.zero;
    private Vector3 viewAcc = Vector3.zero;
    private Vector3 view = Vector3.zero;
    private Vector3 sunRotation = new Vector3(25, 300, 0);
    private float moveScale = 2.5f;
    private List<Parameter> parameters = new List<Parameter>();

    private static string infoText =
        "LemonSpawn VolClouds 1.0 example scene\n" +
        "Left mouse button to look, WSAD to move\n" +
        "Volumetric clouds currently not renderable from inside the clouds, so height is constrained to below the clouds.\n" +
        "GPU-heavy but sexy!"; 

                                        


	void Start () {
        parameters.Add(new Parameter("Scale", "_CloudScale", 0.3f, 0, 1));
        parameters.Add(new Parameter("Distance", "_CloudDistance", 0.03f, 0, 0.25f));
        parameters.Add(new Parameter("Detail", "_MaxDetail", 0.33f, 0, 1f));
        parameters.Add(new Parameter("Subtract", "_CloudSubtract", 0.6f, 0, 1f));
        parameters.Add(new Parameter("Scattering", "_CloudScattering", 1.5f, 1f, 3f));
        parameters.Add(new Parameter("Y Spread", "_CloudHeightScatter", 1.75f, 0.5f, 6f));
        parameters.Add(new Parameter("Density", "_CloudAlpha", 0.6f, 0f, 1f));
        parameters.Add(new Parameter("Hardness", "_CloudHardness", 0.9f, 0f, 1f));
        parameters.Add(new Parameter("Brightness", "_CloudBrightness", 1.4f, 0f, 2f));
        parameters.Add(new Parameter("Sun Glare", "_SunGlare", 0.6f, 0f, 2f));
       // parameters.Add(new Parameter("Time", "_CloudTime", 0, 0f, 100f));

        parameters.Add(new Parameter("XShift", "_XShift", 0f, 0f, 1f));
        parameters.Add(new Parameter("YShift", "_YShift", 0f, 0f, 1f));
        parameters.Add(new Parameter("ZShift", "_ZShift", 0f, 0f, 1f));

    }

    void OnGUI() {
        int w = 100;
        int dy = 30;
        int textWidth = 75;
        int margin = 10;

        GUI.Label(new Rect(margin, dy, textWidth, dy), "Time of day");
        GUI.Label(new Rect(margin, 2*dy, textWidth, dy), "Sun rotation");
        sunRotation.x = GUI.HorizontalSlider(new Rect(2*margin + textWidth, dy, w, dy), sunRotation.x, 0.0F, 360.0F);
        sunRotation.y = GUI.HorizontalSlider(new Rect(2* margin + textWidth, 2*dy, w, dy), sunRotation.y, 0.0F, 360.0F);
        GUI.Label(new Rect(0, 0, 100, 100), "FPS: "+(int)(1.0f / Time.smoothDeltaTime));

        GUI.Label(new Rect(Screen.width - 600, 1.1f*Screen.height - 400, 600, 400), infoText);

        int i = 4;
        foreach (Parameter p in parameters) {
            p.Render(i++ * dy, w, textWidth, margin);
        }



    }
	
    void MoveCamera()
    {
        Camera c = GameObject.Find("Main Camera").GetComponent<Camera>();
        if (c == null)
            return;
        if (Input.GetKey(KeyCode.W))
            cameraAcc += c.transform.forward * moveScale;
        if (Input.GetKey(KeyCode.S))
            cameraAcc += c.transform.forward * moveScale * -1;
        if (Input.GetKey(KeyCode.D))
            cameraAcc += c.transform.right * moveScale;
        if (Input.GetKey(KeyCode.A))
            cameraAcc += c.transform.right * moveScale * -1;

        if (Input.GetButton("Fire2"))
            viewAcc += (Vector3.right * Input.GetAxis("Mouse X") + Vector3.up * Input.GetAxis("Mouse Y") * -1) * 0.2f;

        c.transform.position = c.transform.position + cameraAcc;
        Vector3 pos = c.transform.position;
        pos.y = Mathf.Clamp(pos.y, 1f, 100);
        c.transform.position = pos;
        view += viewAcc;
        c.transform.eulerAngles = new Vector3(view.y, view.x, 0.0f);
        cameraAcc *= 0.90f;
        viewAcc *= 0.90f;

    }

    void MoveSun()
    {
        GameObject sun = GameObject.Find("Sun");
        if (sun == null)
            return;

        sun.transform.rotation = Quaternion.Euler(sunRotation);
    }

    void UpdateMaterial()
    {
        Material mat = GameObject.Find("Sphere").GetComponent<MeshRenderer>().material;
        foreach (Parameter p in parameters)
            p.SetValue(mat);

        mat.SetFloat("_CloudTime", Time.time);
    }

    void Update () {
        MoveCamera();
        MoveSun();
        UpdateMaterial();

	}
}
