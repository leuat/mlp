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

namespace LemonSpawn{
		/*
     * Class that describes the node connections
     * 
     * 
     * */
		public class CConnection
		{

				public int ID;
				public int Type;
				public int PosInArray;
				public static int TYPE0 = 0;
				public static int TYPE1 = 1;
				public static int TYPE2 = 2;
				public static int TYPE3 = 3;
				// Colors for types
				public CNode parent = null;
				public CConnection pointer = null;
				public Vector2 position = new Vector2 ();

				// Used for saving
				public void setupID ()
				{
						ID = parent.ID * 100 + PosInArray;

				}

				public CConnection (CNode p, int i, int t)
				{
						parent = p;
						PosInArray = i;
						Type = t;
				}

				public bool verifyConnection (CConnection b)
				{
						if (parent == b.parent) {
								//Debug.Log("Aborted: same parent");
								return false;
						}
        
						if (parent.Inputs.Contains (this) && b.parent.Inputs.Contains (b)) {
								//Debug.Log("Aborted: both are inputs");
								return false; 
						}

						if (parent.Outputs.Contains (this) && b.parent.Outputs.Contains (b)) {
								//Debug.Log("Aborted: both are outputs");
								return false; 
						}
						if (parent.Tops.Contains (this) && b.parent.Tops.Contains (b)) {
								//Debug.Log("Aborted: both are tops");
								return false; 
						}
						if (parent.Bottoms.Contains (this) && b.parent.Bottoms.Contains (b)) {
								//Debug.Log("Aborted: both are bottoms");
								return false; 
						}
						if (Type != b.Type) {
								//Debug.Log("Aborted: Incorrect type");
								return false; 

						}
						return true;
        
				}
				public void PropagateChange() {
			
					if (pointer!=null)
						if (pointer.parent!=null)
							pointer.parent.PropagateChange();
					if (parent!=null)
						parent.PropagateChange();
				    
				    }
				    
				public void Break ()
				{
						if (pointer != null)
								pointer.pointer = null;
								
						pointer = null;
				}

				public void Link (CConnection c)
				{
						if (pointer == null && c.pointer == null) {
							pointer = c;
							pointer.pointer = this;
						}
				}
				public CConnection Click (CConnection c)
				{
						// First, what if completely blank: Create new link

						if (c == null && pointer == null) {
								c = this;
								CLink l = new CLink ();
								l.from = c;
								l.to = null;
								//Debug.Log ("Created new link from empty");
								CNodeManager.link = l;
								//PropagateChange();
								return c;
						}
						// Attatch existing string to connection
						if (c != null && pointer == null) {
								// is this valid? 
								if (!verifyConnection (c))
										return c;

								Link (c);
//								PropagateChange();
				
								//Debug.Log ("Attatched link to empty");
								CNodeManager.link = null;
				
								return null;

						}

						// if clicked on existing link
						if (c == null && pointer != null) {
								c = pointer;
								CLink l = new CLink ();
								l.from = c;
								l.to = null;
								//Debug.Log ("Created new link from existing");
								CNodeManager.link = l;
								//PropagateChange();
								
								Break ();
								return c;

						}

						// SWAP!
						if (c != null && pointer != null) {
								if (!verifyConnection (c))
										return c;
								// Basically: Swap connections
								CConnection old = pointer;
								old.Break ();
								//Break();
								Link (c);
								CLink l = new CLink ();
								l.from = old;
								l.to = null;
								CNodeManager.link = l;
//								PropagateChange();
				

								return old;
            
						}

						CNodeManager.link = null;
						return null;
				}
		}
}
