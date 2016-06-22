using UnityEngine;
using System.Collections;
using System.Threading;


namespace LemonSpawn
{

    public class QuadNode : ThreadQueue
    {

        public PlanetSettings planetSettings;

        private bool isEnvironment = false;
        public Vector3 currentPos;


        private bool recalculate = false;
        private Bounds bounds = new Bounds();

        public QuadField quadField;
        public QuadNode parent;
        public int currentQuadrant = -1, currentLevel;
        public float currentScale;

        public QuadNode father;

        QuadBlock qb = null;

        int[] neighbourLOD = new int[4];


        /*		public void SetMaterial(Material mat) {
                    if (quadGO!=null)
                        quadGO.GetComponent<Renderer>().material = mat;

                    if (children!=null)
                        foreach (QuadNode qn in children)
                            qn.SetMaterial (mat);

                }*/

        public static Vector3 mtmp = new Vector3();
        public static Vector3 mtmp2 = new Vector3();

        public Vector3 n1 = new Vector3();


        public QuadNode[] children = null;

        // Neighbours
        public QuadNode up = null, down = null, left = null, right = null;

        public bool useChildren = false;

        public GameObject quadGO = null;

        private void setGOproperties()
        {
            if (quadGO == null)
                return;

            quadGO.GetComponent<Renderer>().enabled = false;


            if (parent != null && parent.quadGO != null)
                quadGO.transform.parent = parent.quadGO.transform;
            else
                quadGO.transform.parent = planetSettings.properties.terrainObject.transform;

            quadGO.transform.localScale = Vector3.one;
            quadGO.transform.position = Vector3.zero;
            quadGO.transform.rotation = Quaternion.identity;
            quadGO.transform.localRotation = Quaternion.identity;
            quadGO.transform.localPosition = Vector3.zero;

        }

        public override void PostThread()
        {
            base.PostThread();
            if (thread == null)
                return;

            quadGO = quadField.Realise(planetSettings.castShadows);
            if (RenderSettings.createTerrainColliders)
            {
                MeshCollider c = quadGO.AddComponent<MeshCollider>();
                c.enabled = false;
                c.material = (PhysicMaterial)Resources.Load("PhysicsMaterial/surface");
            }
//            c.fri
            


            //            if (currentLevel == max)
            setGOproperties();
            thread = null;
        }

        public void ThreadedGenerateSurface()
        {
            threadDone = false;
            if (quadField == null)
            {
                quadField = new QuadField();
                quadField.Init(RenderSettings.sizeVBO);
            }
            decideNeighbors();
            quadField.Calculate(planetSettings.radius, qb.P[0].P, qb.P[1].P, qb.P[3].P, planetSettings, neighbourLOD);
            threadDone = true;
        }

        TQueue thread = null;



        public void RealiseDirect()
        {
            if (quadField == null)
            {
                quadField = new QuadField();
                quadField.Init(RenderSettings.sizeVBO);
            }
            decideNeighbors();
            quadField.Calculate(planetSettings.radius, qb.P[0].P, qb.P[1].P, qb.P[3].P, planetSettings, neighbourLOD);
            quadGO = quadField.Realise(planetSettings.castShadows);
            quadGO.GetComponent<Renderer>().enabled = false;


            if (parent != null && parent.quadGO != null)
                quadGO.transform.parent = parent.quadGO.transform;
            else
                quadGO.transform.parent = planetSettings.properties.terrainObject.transform;

            quadGO.transform.localScale = Vector3.one;
            quadGO.transform.position = Vector3.zero;
            quadGO.transform.localPosition = Vector3.zero;
        }

        public override bool isCancelable()
        {
            return false;
            /*
                if (children == null)
                    return false;
                else
                    return true;*/
        }

        public void setAllEnabled(bool enabled)
        {
            if (quadGO != null)
            {
                quadGO.GetComponent<Renderer>().enabled = enabled;
				MeshCollider mc = quadGO.GetComponent<MeshCollider>();
				if (mc!=null)
                	mc.enabled = enabled;

            }
            
            if (children != null)
                foreach (QuadNode qn in children)
                    qn.setAllEnabled(enabled);


        }

        
        public void Realise()
        {
            // Faen fix denne buggen med children == null
            //            RenderSettings.UseThreading = false;
            if (quadGO == null && thread == null && isWithinCameraBounds()) //  && children == null) 
            {
                if (RenderSettings.UseThreading)
                {
                    thread = new TQueue();
                    thread.thread = new Thread(new ThreadStart(ThreadedGenerateSurface));
                    thread.gt = this;
                    AddThread(thread);
                }
                else
                {
                    RealiseDirect();
                }
            }
            else
            {
                //if (quadGO != null)
                //	quadGO.GetComponent<Renderer>().enabled = true;
            }
            if (recalculate == true && quadField != null && thread == null && quadGO != null)
            {
                decideNeighbors();
                LSMesh m = quadField.ReCalculate(neighbourLOD, planetSettings.castShadows);
                quadGO.GetComponent<MeshFilter>().mesh = m.mesh;
                
                recalculate = false;
            }


            if (children != null)
            {
                /*if (thread!=null) {
					
					ThreadQueue.Remove(thread);
					thread = null;
				}
				*/
                bool allSet = true;
                foreach (QuadNode qn in children)
                {
                    if (qn.quadGO == null)// && qn.children == null)
                        allSet = false;
                }

                if (quadGO != null)
                {
                    quadGO.GetComponent<Renderer>().enabled = !allSet;
					MeshCollider mc = quadGO.GetComponent<MeshCollider>();
					if (mc!=null)
						mc.enabled = !allSet;
                }

                    foreach (QuadNode qn in children)
                    //if (qn.quadGO != null)
                    //	qn.quadGO.GetComponent<Renderer>().enabled = allSet;
                    qn.setAllEnabled(allSet);


                foreach (QuadNode qn in children)
                    qn.Realise();

                //					quadGO.SetActive(!allSet);
            }

        }


        public QuadNode getQuadrant(int quad, Vector3 C, int myLod)
        {

            if (children == null)
                return this;

            return children[quad];
        }

        public QuadNode(QuadPoint p1, QuadPoint p2, QuadPoint p3,
                         QuadPoint p4, int level, QuadNode f, int cq,
                        PlanetSettings ps, bool isEnv, QuadNode par)
        {

            isEnvironment = isEnv;
            planetSettings = ps;
            qb = new QuadBlock(p1, p2, p3, p4, level, planetSettings);
            qb.lod = level;
            sort = level;
            parent = par;
            currentQuadrant = cq;
            father = f;
            localPosition = qb.centerReal.P;


        }

        private QuadNode verify(QuadNode child, QuadNode old,
                                 QuadNode n)
        {

            if (old == null)
                return n;

            /*
		 * if (useChildren) return n;
		 */
            if (old != n && child.useChildren == false && child.quadGO != null && child.thread == null)
                child.recalculate = true;

            return n;
        }

        public void PopulateChildNeighbours(QuadNode child, int quadrant)
        {

            if (quadrant == 0)
            {
                child.left = verify(child, child.left,
                                    left.getQuadrant(1, child.qb.center.P, child.qb.lod));
                child.up = verify(child, child.up,
                                  up.getQuadrant(3, child.qb.center.P, child.qb.lod));

                child.right = verify(child, child.right,
                                     getQuadrant(1, child.qb.center.P, child.qb.lod));
                child.down = verify(child, child.down,
                                    getQuadrant(3, child.qb.center.P, child.qb.lod));
            }
            if (quadrant == 1)
            {
                child.right = verify(child, child.right,
                                     right.getQuadrant(0, child.qb.center.P, child.qb.lod));
                child.up = verify(child, child.up,
                                  up.getQuadrant(2, child.qb.center.P, child.qb.lod));

                child.left = verify(child, child.left,
                                    getQuadrant(0, child.qb.center.P, child.qb.lod));
                child.down = verify(child, child.down,
                                    getQuadrant(2, child.qb.center.P, child.qb.lod));
            }

            if (quadrant == 2)
            {
                child.down = verify(child, child.down,
                                    down.getQuadrant(1, child.qb.center.P, child.qb.lod));
                child.right = verify(child, child.right,
                                     right.getQuadrant(3, child.qb.center.P, child.qb.lod));

                child.up = verify(child, child.up,
                                  getQuadrant(1, child.qb.center.P, child.qb.lod));
                child.left = verify(child, child.left,
                                    getQuadrant(3, child.qb.center.P, child.qb.lod));
            }

            if (quadrant == 3)
            {
                child.down = verify(child, child.down,
                                    down.getQuadrant(0, child.qb.center.P, child.qb.lod));
                child.left = verify(child, child.left,
                                    left.getQuadrant(2, child.qb.center.P, child.qb.lod));

                child.up = verify(child, child.up,
                                  getQuadrant(0, child.qb.center.P, child.qb.lod));
                child.right = verify(child, child.right,
                                     getQuadrant(2, child.qb.center.P, child.qb.lod));
            }
        }



        public void decideNeighbors()
        {

            for (int i = 0; i < 4; i++)
                neighbourLOD[i] = 0;

            if (up != null)
                if (up.qb.lod < qb.lod)
                    neighbourLOD[0] = 1;

            if (down != null)
                if (down.qb.lod < qb.lod)
                    neighbourLOD[1] = 1;

            if (left != null)
                if (left.qb.lod < qb.lod)
                    neighbourLOD[2] = 1;

            if (right != null)
                if (right.qb.lod < qb.lod)
                    neighbourLOD[3] = 1;


        }


        public void setupNeighbors()
        {

            if (children != null)
            {
                for (int i = 0; i < children.Length; i++)
                    PopulateChildNeighbours(children[i], i);

                for (int i = 0; i < children.Length; i++)
                    children[i].setupNeighbors();

            }
        }



        public void deleteChildren()
        {


            if (children != null)
                //if (!isEnvironment)

                for (int i = 0; i < children.Length; i++)
                {
                    if (children[i].quadGO != null)
                        GameObject.Destroy(children[i].quadGO);
                    if (children[i].thread != null)
                    {
                        ThreadQueue.Remove(children[i].thread);
                        children[i].thread = null;

                    }
                    children[i].deleteChildren();
                }


            children = null;
        }

        private float findMinDistance(Vector3 cam, bool nestFurther)
        {
            float min = 10E30f;

            mtmp = (cam - qb.centerReal.P) / planetSettings.getPlanetSize();
            min = Mathf.Min(min, mtmp.sqrMagnitude);


            //}
            return min;
        }

        public int wait = 0;
        Vector3 ttmp;



        void displayDebug()
        {
            float sec = 0.001f;
            Vector3 pos = planetSettings.transform.position;
            float d = 0.0002f*(qb.centerReal.P + pos).magnitude;
            Color c = Color.green / d;
            c.a = 1;
            Debug.DrawLine(qb.PReal[0].P + pos, qb.PReal[1].P + pos, c, sec, false);
            Debug.DrawLine(qb.PReal[1].P + pos, qb.PReal[2].P + pos, c, sec, false);
            Debug.DrawLine(qb.PReal[2].P + pos, qb.PReal[3].P + pos, c, sec, false);
            Debug.DrawLine(qb.PReal[3].P + pos, qb.PReal[0].P + pos, c, sec, false);

        }

        bool isWithinCameraBounds()
        {
        	if (!RenderSettings.cullCamera)
        		return true;

            bounds.SetMinMax(qb.PReal[0].P + planetSettings.transform.position, qb.PReal[2].P + planetSettings.transform.position);


            //if (children != null)
            {

            if (!GeometryUtility.TestPlanesAABB(planetSettings.properties.cameraPlanes, bounds))
                {
                    //                    deleteChildren();
                    return false;
                }
            }
            return true;

        }


        public void Subdivide(int level, float size)
        {
            if (Vector3.Dot((World.MainCamera.transform.position - (qb.centerReal.P + planetSettings.properties.localCamera)).normalized, qb.normal.normalized) > 0.1)
            {
                deleteChildren();
                return;
            }
            //

            currentLevel = level;

            int modifier = 0;
            if (!isEnvironment)
            {
//                modifier = 	(int) (Settings.settings.globalVariables.currentSpeed * 0.8);
            }
            if (level > planetSettings.maxQuadNodeLevel - modifier)
            {
                deleteChildren();
                return;
            }
            if (quadGO != null) 
                if (quadGO.GetComponent<Renderer>().enabled == true)
            if (RenderSettings.displayDebugLines)
                displayDebug();



            float l = Mathf.Pow(findMinDistance(planetSettings.properties.localCamera, true), 1);
/*            float height = RenderSettings.ResolutionScale * 0.035f;
            l = l / (height);*/
            //			l = Mathf.Min (l, 10.0f);
            if (level >= RenderSettings.minQuadNodeLevel)
                if (l > size * size)
                {
                    // Don't subdivide more
                    useChildren = false;
                    deleteChildren();
                    return;
                }
            // Subdivide!
            useChildren = true;

            // Create children

            if (children == null)
            {
                children = new QuadNode[4];

                QuadPoint[] tmp = new QuadPoint[4];
                for (int i = 0; i < tmp.Length; i++)
                    tmp[i] = new QuadPoint();

                tmp[0].P = (qb.P[0].P + qb.P[1].P) / 2f;
                tmp[1].P = (qb.P[1].P + qb.P[2].P) / 2f;
                tmp[2].P = (qb.P[2].P + qb.P[3].P) / 2f;
                tmp[3].P = (qb.P[3].P + qb.P[0].P) / 2f;

                children[0] = new QuadNode(qb.P[0], tmp[0], qb.center, tmp[3], level + 1,
                                           this, 0, planetSettings, isEnvironment, this);

                children[1] = new QuadNode(tmp[0], qb.P[1], tmp[1], qb.center, level + 1,
                                           this, 1, planetSettings, isEnvironment, this);
                children[2] = new QuadNode(qb.center, tmp[1], qb.P[2], tmp[2], level + 1,
                                           this, 2, planetSettings, isEnvironment, this);
                children[3] = new QuadNode(tmp[3], qb.center, tmp[2], qb.P[3], level + 1,
                                           this, 3, planetSettings, isEnvironment, this);

                /*				if (quadGO != null)
                                    quadGO.SetActive(false);
                */
                // shadows:
                /*				if (qb.lod==Settings.settings.planetSettings.ShadowQuadNodeLevel || qb.lod==Settings.settings.planetSettings.ShadowQuadNodeLevel2) {
                                    currentScale = 1.0f;
                                    currentPos.set(0,0,0); 
                                }*/
                for (int i = 0; i < children.Length; i++)
                {
                    children[i].currentScale = currentScale / 2.0f;
                }

                // for shadows: set positioning
                children[0].currentPos = currentPos;

                ttmp = new Vector3(0.5f, 0.0f, 0.0f);
                ttmp = ttmp * currentScale;
                mtmp = currentPos + ttmp;
                children[1].currentPos = currentPos + new Vector3(0.5f, 0.0f, 0.0f) * currentScale;
                children[3].currentPos = currentPos + new Vector3(0.0f, 0.5f, 0.0f) * currentScale;
                children[2].currentPos = currentPos + new Vector3(0.5f, 0.5f, 0.0f) * currentScale;


            }

            for (int i = 0; i < children.Length; i++)
            {
                children[i].Subdivide(level + 1, size / 2.0f);
            }
        }

    }


}