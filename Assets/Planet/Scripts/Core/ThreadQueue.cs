using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;







namespace LemonSpawn {

	public class TQueue {
		public Thread thread;
		public ThreadQueue gt;
	}	
	
	
	public class ThreadQueue {

		public static List<TQueue> threadQueue = new List<TQueue>(); 
		public static int orgThreads = 0;
		public static List<TQueue> currentThreads =new List<TQueue>();
		public bool threadDone = true;
		private static int maxThreads = SystemInfo.processorCount-1;	
		public Vector3 localPosition;
		public float sort = 0;
		
		public static void SortQueue(Vector3 cam) {
			threadQueue.Sort(
				delegate(TQueue p1, TQueue p2)
				{
                    /*					float d1 = (cam - p1.gt.localPosition).magnitude;
                                        float d2 = (cam - p2.gt.localPosition).magnitude;
                                        float r1 = p1.gt.sort;
                                        float r2 = p2.gt.sort;

                                        float a = d1 + r1*0.001f;
                                        float b = d2 + r2*0.001f;

                                        */
                    float d1 = p1.gt.sort;
                    float d2 = p2.gt.sort;

                    if (RenderSettings.sortInverse)
                    {
                        d1 = p2.gt.sort;
                        d2 = p1.gt.sort;

                    }


                    //    Debug.Log(d1);
                    if (d1>d2) return 1;
					if (d1<d2) return -1;
					return 0;
				}
			);
		}
		
		public static void Remove(TQueue tq) {
			threadQueue.Remove(tq);
			//tq.threadDone = true;
				
		}
		
		public void AddThread(TQueue thread) {
			threadQueue.Add (thread);
			orgThreads = threadQueue.Count;			
		}
		
		
		public static void RemoveDefunctThreads() {
			foreach (TQueue tq in threadQueue) {
				if (tq.gt.isCancelable()) {
					threadQueue.Remove(tq);
					RemoveDefunctThreads();
					return;
				}
			}
		}
		
		
		public static void MaintainThreadQueue() {
			RemoveDefunctThreads();
			List<TQueue> removes = new List<TQueue>();
			foreach (TQueue tq in currentThreads) {
				if (tq.gt.threadDone) {	
					tq.gt.PostThread();
					removes.Add (tq);
				}
				
			}	
			foreach (TQueue tq in removes)
				currentThreads.Remove(tq);
			
			if (threadQueue.Count==0)
				return;
			
			while (currentThreads.Count<maxThreads && threadQueue.Count>0) {
				TQueue currentThread = threadQueue[0];
				currentThreads.Add(currentThread);
			
				threadQueue.RemoveAt(0);
				currentThread.gt.threadDone = false;
				currentThread.thread.Start();
			}
		}
		
		public virtual bool isCancelable() {
			return false;
		}
		
		
		public virtual void PostThread() {
		
		}

        public static void AbortAll()
        {
            threadQueue.Clear();
            bool done = false;
            while (!done)
            {
                done = true;
                foreach (TQueue tq in threadQueue)
                {
                    if (!tq.gt.threadDone)
                        done = false;
                }
                Thread.Sleep(10);
            }
        }

}

}