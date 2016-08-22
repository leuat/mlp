using UnityEngine;
using System.Collections;
using System.Xml.Linq;

namespace LemonSpawn {


	[System.Serializable]
	public class IDValue {
		public string ID;
		public string name; 
		public IDValue(string id, string n) {
			name = n;
			ID = id;
		}
	}

	public class Verification {

		public static string MCAstName = "MCAst";
		public static string SolarSystemViewerName = "Solar system viewer";


		public static IDValue[] IDValues = new IDValue[2] {
			new IDValue("5acbd644-37c7-11e6-ac61-9e71128cae77", MCAstName),
			new IDValue("7aca6974-37c7-11e6-ac61-9e71128cae77", SolarSystemViewerName)
        };




        public static string findValueInElements(System.Collections.Generic.IEnumerable<XElement> e,string name) {
			foreach (XElement el in e) {
				if (el.Name == name)
					return el.Value;
				else {
					string s = findValueInElements(el.Elements(), name);
					if (s!="")
						return s;
					}

			}
			return "";
        } 

        public static bool VerifyXML(string fileName, string application) {
            return true;

			XDocument doc = XDocument.Load(fileName);
			string value = findValueInElements(doc.Root.Elements(),"uuid");
			foreach (IDValue i in IDValues) {
				if (i.ID == value.Trim().ToLower())
					if (i.name == application)
						return true;

			}
			return false;


			}

	}

}
