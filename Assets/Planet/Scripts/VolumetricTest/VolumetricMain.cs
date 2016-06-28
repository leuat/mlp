using UnityEngine;
using System.Collections;



namespace LemonSpawn {

    public class VolumetricTexture {

        public Texture3D texture;
        public int size;

        public void CreateNoise(int N, float scale)
        {
            size = N;
            texture = new Texture3D(size,size,size, TextureFormat.ARGB32, true);

            cols = new Color[size * size * size];
            Color c = new Color(1.0f, 0.7f, 0.2f, 1.0f);
            for (int i=0;i<N;i++)
                for (int j = 0; j < N; j++)
                    for (int k = 0; k < N; k++)
                    {
                        Vector3 v = new Vector3(i / (float)N, j / (float)N, k / (float)N);
                        v *= scale;
                        float val = Util.noise4D.raw_noise_3d(v.x, v.y, v.z);
                        c.a = val;
                        setColor(i, j, k, c);

                    }
            texture.SetPixels(cols);
            texture.Apply();
        }

        private void setColor(int x, int y, int z, Color c)
        {
            if (x < 0 || x >= size)
                return;
            if (y < 0 || y >= size)
                return;
            if (z < 0 || z >= size)
                return;
//            cols[x * size * size + (size-1-y) * size + z] = c;
            cols[x * size * size + (y) * size + z] = c;
        }

        private void singleGrass(int x, int y, int wx, int wy, float h)
        {
            Color c = new Color(0.3f, 1f, 0.3f, 1);
            float theta = Random.value * 2 * Mathf.PI;


            Vector2 dir = new Vector3(Mathf.Cos(theta), Mathf.Sin(theta));
            Vector2 pert = new Vector2(0, 0);

            for (int i=0;i<h;i++)
            {
                for (int xx = x - wx; xx < x + wx; xx++)
                    for (int yy = y - wy; yy < y + wy; yy++)
                    {
                        float dist = 1 / (new Vector2(xx-x, yy-y).magnitude+1);
                        Color col = c*dist;
                        col.a = 1;
                        setColor(xx+(int)pert.x, i, yy+(int)pert.y, col);
                        pert += dir * i*i / size*0.001f;



                    }

            }


        }
        Color[] cols;

        public void CreateGrass(int N, int Count)
        {
            size = N;
            texture = new Texture3D(size, size, size, TextureFormat.ARGB32, true);

            cols = new Color[size * size * size];
            for (int i = 0; i < cols.Length; i++)
                cols[i] = new Color(0, 0, 0, 0);

            int s = 2;

            for (int i=0;i<Count;i++)
            {
                int x = (int)Mathf.Clamp((int)(Random.value * size),s,size-s);
                int y = (int)Mathf.Clamp((int)(Random.value * size), s, size - s); 
                int h = (int)(size*0.5f + 0.5f*(int)(Random.value * size));

                singleGrass(x, y, s, s, h);


            }

            texture.SetPixels(cols);
            texture.Apply();
        }




    }

public class VolumetricMain : MonoBehaviour {

        // Use this for initialization
    public GameObject testObject, testObject2;
	void Start () {
            VolumetricTexture vt = new VolumetricTexture();
            vt.CreateGrass(256, 200);
        //    vt.CreateNoise(64, 6.123f);
            testObject.GetComponent<Renderer>().material.SetTexture("_MainTex", vt.texture);
          //  testObject2.GetComponent<Renderer>().material.SetTexture("_MainTex", vt.texture);


        }

        // Update is called once per frame
        void Update () {
            if (Camera.current == null)
                return;

            Camera.current.transform.RotateAround(Vector3.zero, Vector3.up, 0.2f);
            float l = 5 + Mathf.Cos(Time.time) * 4;
            Camera.current.transform.position = l * Camera.current.transform.position.normalized;
	}
}

}