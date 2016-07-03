using UnityEngine;
using System.Collections;


namespace LemonSpawn  {
public class SpaceCamera : MonoBehaviour {

	
	
	public float mainSpeed = 100.0f; //regular speed
	public float shiftAdd = 250.0f; //multiplied by how long shift is held.  Basically running
	public float maxShift = 1000.0f; //Maximum speed when holdin gshift
	public float camSens = 0.35f; //How sensitive it with mouse
	private Vector3 lastMouse = new Vector3(255, 255, 255); //kind of in the middle of the screen, rather than at the top (play)
	private float totalRun = 1.0f;
	private Vector3 P;
	public DVector initPos = new DVector(0,0,0);
	public DVector initDir = new DVector(0,0,0);
	private Vector3 mouseAdd = new Vector3();
	float rotate = 0;
	float rotateT = 0;
        public Vector3 up;
	//private GameObject actualCamera;
	public DVector actualCamera = new DVector();
	
	void Start() {
//		actualCamera = new GameObject("ActualCamera");
		SetLookCamera(initPos,initDir.toVectorf(), Vector3.up);
	}
	
	void UpdateCam(Vector3 t) {
				actualCamera = actualCamera.Add(transform.rotation*t);
//		actualCamera.transform.rotation = transform.rotation;
//		actualCamera.transform.Translate( t );
	}
	
	public DVector getPos() {
		return actualCamera;
//		return actualCamera.transform.position;
	}

        public Vector3 getRPos()
        {
            return actualCamera.toVectorf()*(float)RenderSettings.AU;
            //		return actualCamera.transform.position;
        }


        public void SetLookCamera(DVector p, Vector3 dir, Vector3 u) {
//		transform.rotation  = rot;
		actualCamera = p*RenderSettings.AU;
        World.WorldCamera = p * RenderSettings.AU;

            curDir = dir;
		SetLookCamera(dir, u);
            up = u;
	}
	
	public void MoveCamera(Vector3 dp) {
		actualCamera = actualCamera.Add(new DVector(dp));		
		World.WorldCamera = World.WorldCamera.Add(new DVector(dp));
			
	}
	
	public void SetLookCameraTarget(Vector3 p, Vector3 target, Vector3 u) {
			//		transform.rotation  = rot;
		actualCamera.Set(p*(float)RenderSettings.AU);
		World.WorldCamera.Set (p*(float)RenderSettings.AU);
		transform.up = up;		
		transform.LookAt(target);
            up = u;

            //		SetLookCamera(theta, phi, up);		
        }


        public Vector3 curDir;

        public void SetLookCamera(Vector3 dir, Vector3 up) {
			//		transform.rotation  = rot;
			
			
			
			//Vector3 dir = new Vector3(Mathf.Sin (theta)*Mathf.Cos (phi), Mathf.Sin (theta)*Mathf.Sin (phi), Mathf.Cos (theta));
			Quaternion q = new Quaternion();
						
			q.SetLookRotation(dir, up);
            curDir = dir;
			transform.rotation = q;
			//transform.rotation = Quaternion.AngleAxis(roll, dir);									
		}
		
	
	public void SetCamera(Vector3 p, Quaternion rot) {
//		transform.position = p;
		transform.rotation = rot;
		actualCamera.Set(p*(float)RenderSettings.AU);
		World.WorldCamera.Set (p*(float)RenderSettings.AU);
//		actualCamera.transform.rotation = rot;
	}	
	
	void Update () {
		
		if (!RenderSettings.MoveCam)
			return;
		
		lastMouse = Input.mousePosition - lastMouse ;
		lastMouse = new Vector3(-lastMouse.y * camSens, lastMouse.x * camSens, 0 );
		mouseAdd+=0.1f*lastMouse;
		mouseAdd*=0.9f;
		transform.RotateAround(transform.right, mouseAdd.x*0.03f); 
		transform.RotateAround(transform.up, mouseAdd.y*0.03f); 
		lastMouse =  Input.mousePosition;
		Vector3 p = GetBaseInput();
		float h = 1;
		if (SolarSystem.planet!=null) {
			float r = SolarSystem.planet.pSettings.radius;
			h = Mathf.Min (0.0001f + SolarSystem.planet.pSettings.getScaledHeight()*r/5000f, 1);
			World.stats.Height = SolarSystem.planet.pSettings.getHeight();
		}
		p*=h;
		
		if (Input.GetKey (KeyCode.LeftShift)){
			totalRun += Time.deltaTime;
			p  = p * totalRun * shiftAdd;
			p.x = Mathf.Clamp(p.x, -maxShift, maxShift);
			p.y = Mathf.Clamp(p.y, -maxShift, maxShift);
			p.z = Mathf.Clamp(p.z, -maxShift, maxShift);
		}
		else{
			totalRun = Mathf.Clamp(totalRun * 0.5f, 1, 1000);
			p = p * mainSpeed;
		}
		
		if (Input.GetKey(KeyCode.Z)) {
			rotateT = Mathf.Min(rotateT+0.01f, 0.05f);
		}
		if (Input.GetKey(KeyCode.C)) {
			rotateT = Mathf.Max(rotateT-0.01f, -0.05f);;
		}
		rotate = rotate*0.9f +  rotateT*0.1f;
		rotateT*=0.9f;
		transform.RotateAround(transform.forward, rotateT);
		p*=10;
		P = P + p*0.5f;
		P*=0.8f/(1+Time.deltaTime);
		
		World.stats.Velocity = P.magnitude;
		UpdateCam(P*Time.deltaTime);
//		World.WorldCamera += P*Time.deltaTime;
		
	}
	
	private Vector3 GetBaseInput() { //returns the basic values, if it's 0 than it's not active.
		Vector3 p_Velocity = new Vector3();
		if (Input.GetKey (KeyCode.W)){
			p_Velocity += new Vector3(0,0,1);
		}
		if (Input.GetKey (KeyCode.S)){
				p_Velocity += new Vector3(0,0,-1);
		}
		if (Input.GetKey (KeyCode.A)){
				p_Velocity += new Vector3(-1, 0 , 0);
		}
		if (Input.GetKey (KeyCode.D)){
				p_Velocity += new Vector3(1, 0 , 0);
		}
		if (Input.GetKey(KeyCode.E)) {
				p_Velocity +=new Vector3(0,1,0);;
		}
		if (Input.GetKey(KeyCode.Q)) {
				p_Velocity -=new Vector3(0,1,0);;
		}
		return p_Velocity;
	}}
	}
