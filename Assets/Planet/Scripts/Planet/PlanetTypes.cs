using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;
using System.Reflection;
using System;
using UnityEngine.UI;
using System.Runtime.Serialization.Formatters.Binary;

namespace LemonSpawn
{





    public class AtmosphereType
    {
        public Vector3 values;
        public string name;
        public AtmosphereType(string n, Vector3 v)
        {
            name = n;
            values = v;
        }

        public static AtmosphereType[] AtmosphereTypes = new AtmosphereType[] {
           new AtmosphereType("NORMAL", new Vector3(0.65f, 0.57f, 0.475f)), // CONFIRMED NORMAL
			new AtmosphereType("BLEAK", new Vector3(0.6f, 0.6f, 0.6f)),    // BLEAK 
			new AtmosphereType("RED",new Vector3(0.5f, 0.62f, 0.625f)), // CONFIRMED RED
			new AtmosphereType("CYAN", new Vector3(0.65f, 0.47f, 0.435f)), // CONFIRMED CYAN
			new AtmosphereType("GREEN",new Vector3(0.60f, 0.5f, 0.60f)),  // CONFIRMED GREEN
			new AtmosphereType("PURPLE",new Vector3(0.65f, 0.67f, 0.475f)),   //  Confirmed PURPLE 
			new AtmosphereType("YELLOW",new Vector3(0.5f, 0.54f, 0.635f)),   // CONFIRMED YELLOW
			new AtmosphereType("PINK",new Vector3(0.45f, 0.72f, 0.675f)) // CONFIRMED PINK
        };

        public static Vector3 getAtmosphereValue(string n)
        {
            foreach (AtmosphereType at in AtmosphereTypes)
                if (at.name.ToLower() == n.ToLower())
                    return at.values;

            return Vector3.zero;
        }
    }

    [System.Serializable]
    public class SettingsType
    {
        public string name;
        public string stringValue;
        public string propName;
        public int index;
        public string info;
        public string group;
        public float lower;
        public float upper;
        public Vector3 minMax = new Vector3(); // Min / max max
        public float realizedValue;
        public int type;
        public Color color, variation, realizedColor;


        public static int STRING = 0;
        public static int NUMBER = 1;
        public static int BOOL = 2;
        public static int COLOR = 3;


        public SettingsType()
        {

        }

        public SettingsType(string _name, int _type, string _propname, string _info, Vector3 _minMax, Vector3 init, string _group, int _index = 0)
        {
            name = _name;
            type = _type;
            propName = _propname;
            minMax = _minMax;
            info = _info;
            index = _index;
            group = _group;

            lower = init.x;
            upper = init.y;
            realizedValue = init.z;
        }

        public SettingsType(string _name, string _propname, string _info, string _g, string strval)
        {
            name = _name;
            type = STRING;
            propName = _propname;
            info = _info;
            group = _g;
            stringValue = strval;
        }

        public SettingsType(string _name, string _propname, string _info, string _group, Color _color, Color _variation)
        {
            name = _name;
            type = COLOR;
            propName = _propname;
            minMax = new Vector3(0,1,0);
            info = _info;
            group = _group;

            color = _color;
            variation = _variation;

        }


        public void Realize(System.Random r)
        {
            if (type == NUMBER)
            {
                float span = upper - lower;
                realizedValue = (float)r.NextDouble() * span + lower;
                //     Debug.Log("Realizing: " + propName + "[" + index + "] (" + lower + " / " + upper + "):" + realizedValue);
            }
            if (type == COLOR)
            {
                realizedColor.r = color.r + (float)r.NextDouble() * variation.r;
                realizedColor.g = color.g + (float)r.NextDouble() * variation.g;
                realizedColor.b = color.b + (float)r.NextDouble() * variation.b;
                realizedColor.a = 1;
            }
        }

        public float getValue()
        {
            return realizedValue;
        }

        public void setParameter(PlanetSettings ps)
        {
            if (ps == null)
                return;
            object o = Util.GetPropertyValue(ps, propName);
       //     Debug.Log(o + " : "+ propName);
//            return;
            if (o is Color)
            {
                Color v = (Color)o;
                //v[index] = realizedValue;
                //Util.SetPropertyValue(ps, propName, v);
                Util.SetPropertyValue(ps, propName, realizedColor);
            }
            if (o is Vector3)
            {
                Vector3 v = (Vector3)o;
                // Debug.Log("Index: " + index + " of " + name);
                v[index] = realizedValue;
                Util.SetPropertyValue(ps, propName, v);
            }
            if (o is float)
            {
                Util.SetPropertyValue(ps, propName, realizedValue);
            }
            if (o is double)
            {
                Util.SetPropertyValue(ps, propName, realizedValue);
            }
            if (o is int)
            {
                Util.SetPropertyValue(ps, propName, realizedValue);
            }
            if (o is string)
            {
                Util.SetPropertyValue(ps, propName, stringValue);
            }
        }





    }
    [System.Serializable]
    public class SettingsTypes
    {
        public string name = "New planet";

        public List<SettingsType> settingsTypes = new List<SettingsType>();
        public List<string> groups = new List<string>();
        public Vector3 TemperatureRange = new Vector3(300, 500, 0);
        public Vector3 RadiusRange = new Vector3(1000, 7000, 0);
        public string PlanetInfo = "info about this planet type";
        public void InitializeNew()
        {

            settingsTypes.Add(
                new SettingsType("Atmosphere density",
                SettingsType.NUMBER,
                "atmosphereDensity",
                "1 = thick, 0 = none",
                new Vector3(0, 1, 0),
                new Vector3(0.65f, 0.75f, 0.85f),
                "Atmosphere"));

            settingsTypes.Add(
                new SettingsType("Outer radius scale",
                SettingsType.NUMBER,
                "outerRadiusScale",
                "Best is 1.025",
                new Vector3(1, 1.05f, 1.025f),
                new Vector3(1.024f, 1.026f, 1.025f),
                "Atmosphere"));

            settingsTypes.Add(
                new SettingsType("HDR Exposure",
                SettingsType.NUMBER,
                "m_hdrExposure",
                "HDR exposure of atmosphere: More lets in a lot of sun",
                new Vector3(0, 5, 1.5f),
                new Vector3(1.4f, 1.6f, 1.5f),
                "Atmosphere"));

            settingsTypes.Add(
                new SettingsType("Atmosphere color",
                SettingsType.STRING,
                "atmosphereString",
                "Variations: normal, bleak, red, cyan, green, purple, yellow, pink. Can be separated by comma, will choose one by random. ",
                new Vector3(0, 5, 1.5f),
                new Vector3(1.4f, 1.6f, 1.5f),
                "Atmosphere"));

            settingsTypes.Add(
              new SettingsType("Reflection intensity",
              SettingsType.NUMBER,
              "m_reflectionIntensity",
              "Skybox reflection intensity. Good for ice and stuff.",
              new Vector3(0, 1, 0),
              new Vector3(0, 0, 0),
              "Atmosphere"));

            settingsTypes.Add(
              new SettingsType("Specularity",
              SettingsType.NUMBER,
              "specularity",
              "Specularity reflection",
              new Vector3(0, 4, 0),
              new Vector3(1, 1, 1),
              "Atmosphere"));

            settingsTypes.Add(
              new SettingsType("Metallicity",
              SettingsType.NUMBER,
              "metallicity",
              "Metallic reflection of planet",
              new Vector3(0, 3, 0),
              new Vector3(0, 0, 0),
              "Atmosphere"));

            settingsTypes.Add(
                new SettingsType("Lacunarity",
                SettingsType.NUMBER,
                "ExpSurfSettings",
                "Lacunarity defines the overall shape of the fractal, high lacunarity = lots of \"lakes\"",
                new Vector3(1.5f, 4, 2.5f),
                new Vector3(2.45f, 2.55f, 2.5f),

                "Surface", 0));

            settingsTypes.Add(
                new SettingsType("Offset",
                SettingsType.NUMBER,
                "ExpSurfSettings",
                "Offset",
                new Vector3(-3, 3, 0.65f),
                new Vector3(0.60f, 0.7f, 0.65f),

                "Surface", 1));

            settingsTypes.Add(
                new SettingsType("Gain",
                SettingsType.NUMBER,
                "ExpSurfSettings",
                "Gain",
                new Vector3(-3, 3, 1.7f),
                new Vector3(1.65f, 1.75f, 1.7f),

                "Surface", 2));

            settingsTypes.Add(
                new SettingsType("Initial offset",
                SettingsType.NUMBER,
                "ExpSurfSettings2",
                "Initial offset",
                new Vector3(-2, 2, -0.5f),
                new Vector3(-0.55f, -0.45f, -0.5f),

                "Surface", 0));

            settingsTypes.Add(
                new SettingsType("Surface height",
                SettingsType.NUMBER,
                "ExpSurfSettings2",
                "Height surface multiplier",
                new Vector3(0, 0.5f, 0.01f),
                new Vector3(0.01f, 0.02f, 0.01f),

                "Surface", 1));

            settingsTypes.Add(
                new SettingsType("Surface scale",
                SettingsType.NUMBER,
                "ExpSurfSettings2",
                "Surface scale",
                new Vector3(0, 50, 5),
                new Vector3(3f, 10f, 5f),
                "Surface", 2));

            settingsTypes.Add(
                new SettingsType("Vortex 1 scale",
                SettingsType.NUMBER,
                "SurfaceVortex1",
                "Vortex 1 scale",
                new Vector3(-50, 50, 5),
                new Vector3(3f, 7f, 5f),
                "Surface", 0));

            settingsTypes.Add(
                new SettingsType("Vortex 1 amplitude",
                SettingsType.NUMBER,
                "SurfaceVortex1",
                "Vortex 1 amplitude",
                new Vector3(0, 10, 1),
                new Vector3(0.9f, 1.1f, 1f),
                "Surface", 1));


            settingsTypes.Add(
               new SettingsType("Vortex 2 scale",
               SettingsType.NUMBER,
               "SurfaceVortex2",
               "Vortex 2 scale",
               new Vector3(-50, 50, 5),
               new Vector3(3f, 7f, 0f),
               "Surface", 0));

            settingsTypes.Add(
                new SettingsType("Vortex 2 amplitude",
                SettingsType.NUMBER,
                "SurfaceVortex2",
                "Vortex 2 amplitude",
                new Vector3(0, 10, 1),
                new Vector3(0.9f, 1.1f, 0f),
                "Surface", 1));

            settingsTypes.Add(
             new SettingsType("Height subtract",
             SettingsType.NUMBER,
             "ExpSurfSettings3",
             "Subtracting from height",
             new Vector3(-3, 3, 1),
             new Vector3(0.2f, 0.3f, 0.25f),
             "Surface", 0));

            settingsTypes.Add(
            new SettingsType("Octaves",
            SettingsType.NUMBER,
            "ExpSurfSettings3",
            "Subtracting from height",
            new Vector3(1, 14, 1),
            new Vector3(10, 10, 10),
            "Surface", 1));

            settingsTypes.Add(
            new SettingsType("Power",
            SettingsType.NUMBER,
            "ExpSurfSettings3",
            "Subtracting from height",
            new Vector3(-2, 4, 1),
            new Vector3(0.9f, 1.1f, 1),
            "Surface", 2));


            settingsTypes.Add(new SettingsType("Basin color", "m_basinColor", "Basin color (lake shores)", "Surface color",
                new Color(0.5f, 0.3f, 0.1f),
                new Color(0.1f, 0.05f, 0.1f)
                ));

            settingsTypes.Add(new SettingsType("Floor color", "m_basinColor2", "Floor color (below basin, below lakes)", "Surface color",
              new Color(0.5f, 0.3f, 0.1f),
              new Color(0.1f, 0.05f, 0.1f)
              ));

            settingsTypes.Add(new SettingsType("Surface color 1", "m_surfaceColor", "Surface color 1", "Surface color",
              new Color(0.7f, 0.5f, 0.025f),
              new Color(0.1f, 0.1f, 0.1f)
              ));

            settingsTypes.Add(new SettingsType("Surface color 2", "m_surfaceColor2", "Surface color 2", "Surface color",
              new Color(0.3f, 0.8f, 0.025f),
              new Color(0.1f, 0.1f, 0.1f)
              ));

            settingsTypes.Add(new SettingsType("Top color", "m_topColor", "Top color (everything aboce top threshold)", "Surface color",
              new Color(0.9f, 0.9f, 0.9f),
              new Color(0.02f, 0.02f, 0.02f)
              ));

            settingsTypes.Add(new SettingsType("Hill color", "m_hillColor", "Hill color (everything below hill threshold)", "Surface color",
              new Color(0.4f, 0.4f, 0.4f),
              new Color(0.02f, 0.02f, 0.02f)
              ));



            settingsTypes.Add(
                new SettingsType("Hill threshold",
                SettingsType.NUMBER,
                "hillyThreshold",
                "Hill normal threshold. 1 = Everything has hill texture (0 degrees from normal), 1 = nothing (90 degrees)",
                new Vector3(0, 1, 1),
                new Vector3(0.95f, 0.99f, 0.98f),
                "Surface thresholds", 0));

            settingsTypes.Add(
                new SettingsType("Top threshold",
                SettingsType.NUMBER,
                "topThreshold",
                "Top threshold. Above this value (multiplied with the radius), all colors are top color.",
                new Vector3(0, 0.1f, 1),
                new Vector3(0.005f, 0.012f, 0.01f),
                "Surface thresholds", 0));

            settingsTypes.Add(
                new SettingsType("Basin threshold",
                SettingsType.NUMBER,
                "basinThreshold",
                "Basin threshold. Below this value (multiplied with the radius), all colors are basin colors.",
                new Vector3(0, 0.01f, 1),
                new Vector3(0.001f, 0.001f, 0.001f),
                "Surface thresholds", 0));


            settingsTypes.Add(new SettingsType("Water color", "m_waterColor", "Water color", "Water",
              new Color(0.3f, 0.5f, 0.9f),
              new Color(0.02f, 0.02f, 0.02f)
              ));


            settingsTypes.Add(
                new SettingsType("Sea level",
                SettingsType.NUMBER,
                "liquidThreshold",
                "Sea level. Below this value (multiplied with the radius), Everything is sea.",
                new Vector3(0, 0.02f, 1),
                new Vector3(0.0003f, 0.0006f, 0.0005f),
                "Water", 0));

            //          addColorSetting("Cloud", "cloudColor", "Clouds", new Vector3(0.55f, 0.55f, 0.55f), new Vector3(0.7f, 0.7f, 0.7f), new Vector3(0.9f, 0.9f, 0.9f));
            settingsTypes.Add(new SettingsType("Cloud color", "cloudColor", "Cloud color", "Clouds",
              new Color(0.65f, 0.7f, 0.9f),
              new Color(0.00f, 0.00f, 0.00f)
              ));

            settingsTypes.Add(
                new SettingsType("Cloud scale",
                SettingsType.NUMBER,
                "cloudSettings.LS_CloudScale",
                "Size of cloud fluctuations.",
                new Vector3(0, 3, 1),
                new Vector3(0.9f, 1.3f, 1f),
                "Clouds", 0));

            settingsTypes.Add(
                new SettingsType("Cloud scattering",
                SettingsType.NUMBER,
                "cloudSettings.LS_CloudScattering",
                "Cloud noise amplitude falloff frequency. Large value = small scale perturbations, low value = large scale.",
                new Vector3(0, 3, 1),
                new Vector3(1.4f, 1.6f, 1.5f),
                "Clouds", 0));

            settingsTypes.Add(
              new SettingsType("Cloud intensity",
              SettingsType.NUMBER,
              "cloudSettings.LS_CloudIntensity",
              "Colour intensity of the clouds.",
              new Vector3(0, 10, 1),
              new Vector3(1.1f, 1.3f, 1.2f),
              "Clouds", 0));

            settingsTypes.Add(
              new SettingsType("Cloud thickness",
              SettingsType.NUMBER,
              "cloudSettings.LS_CloudThickness",
              "Alpha value of clouds. Set to 0 to remove clouds completely.",
              new Vector3(0, 1, 1),
              new Vector3(0.6f, 0.7f, 0.65f),
              "Clouds", 0));

            /*            settingsTypes.Add(
                          new SettingsType("Cloud sharpness",
                          SettingsType.NUMBER,
                          "cloudSettings.LS_CloudSharpness",
                          "Colour intensity of the clouds.",
                          new Vector3(0, 3, 1),
                          new Vector3(1.4f, 1.6f, 1.5f),
                          "Clouds", 0));
                          */
            /*                      public float LS_ShadowScale = 0.006f;
                    public float LS_LargeVortex = 0.1f;
                    public float LS_SmallVortex = 0.02f;
                    public float LS_CloudSubScale = 0.5f;
                    public Vector3 LS_Stretch = new Vector3(1, 1, 1);
                    */
            settingsTypes.Add(
              new SettingsType("Cloud subtract value",
              SettingsType.NUMBER,
              "cloudSettings.LS_CloudSubScale",
              "Value to subtract from cloud, higher value yields scattered clouds.",
              new Vector3(0, 4, 1),
              new Vector3(0.4f,0.6f, 0.5f),
              "Clouds", 0));
        settingsTypes.Add(
              new SettingsType("Cloud shadow scale",
              SettingsType.NUMBER,
              "cloudSettings.LS_ShadowScale",
              "Length of internal cloud shadoes",
              new Vector3(0, 0.05f, 1),
              new Vector3(0.006f,0.006f, 0.006f),
              "Clouds", 0));
        settingsTypes.Add(
              new SettingsType("Large vortex amplitude",
              SettingsType.NUMBER,
              "cloudSettings.LS_LargeVortex",
              "Large vortex amplitude",
              new Vector3(-2f, 2f, 1),
              new Vector3(-0.5f,-0.5f, -0.5f),
              "Clouds", 0));
        settingsTypes.Add(
              new SettingsType("Small vortex amplitude",
              SettingsType.NUMBER,
              "cloudSettings.LS_SmallVortex",
              "Small vortex amplitude",
              new Vector3(-1f, 1f, 1),
              new Vector3(0.01f,0.01f, 0.01f),
              "Clouds", 0));

        settingsTypes.Add(
              new SettingsType("Stretch X",
              SettingsType.NUMBER,
              "cloudSettings.LS_Stretch",
              "X-axis stretch component",
              new Vector3(0, 3f, 1),
              new Vector3(0.8f,1.1f, 1f),
              "Clouds", 0));
        settingsTypes.Add(
              new SettingsType("Stretch Y",
              SettingsType.NUMBER,
              "cloudSettings.LS_Stretch",
              "Y-axis stretch component",
              new Vector3(0, 3f, 1),
              new Vector3(1f,1f, 1f),
              "Clouds", 1));
            settingsTypes.Add(
                  new SettingsType("Stretch Z",
                  SettingsType.NUMBER,
                  "cloudSettings.LS_Stretch",
                  "Z-axis stretch component",
                  new Vector3(0, 3f, 1),
                  new Vector3(1f, 1f, 1f),
                  "Clouds", 2));

        }




        public void addColorSettingOld(string baseName, string property, string group, Vector3 c1, Vector3 c2, Vector3 c3)
        {

            settingsTypes.Add(
                new SettingsType(baseName + " color red",
                SettingsType.NUMBER,
                property,
                baseName + " color red",
                new Vector3(0, 1, 1),
                c1,
                group, 0));

            settingsTypes.Add(
                new SettingsType(baseName + " color green",
                SettingsType.NUMBER,
                property,
                baseName + " color green",
                new Vector3(0, 1, 1),
                c2,
                group, 1));

            settingsTypes.Add(
                new SettingsType(baseName + " color blue",
                SettingsType.NUMBER,
                property,
                baseName + " color blue",
                new Vector3(0, 1, 1),
                c3,
                group, 2));
        }


        public void Realize(System.Random r)
        {
            foreach (SettingsType st in settingsTypes)
                st.Realize(r);
        }

        public void setParameters(PlanetSettings ps, System.Random r)
        {
            foreach (SettingsType st in settingsTypes)
                st.setParameter(ps); ;

            ps.UpdateParameters(r);
        }


        public SettingsType findParameter(string name)
        {
            foreach (SettingsType st in settingsTypes)
                if (st.name.ToLower() == name.ToLower())
                    return st;

            return null;
        }


        public SettingsType getSettingsFromDropdown(string box)
        {
            Dropdown d = GameObject.Find(box).GetComponent<Dropdown>();
            int idx = d.value;
            string name = d.options[idx].text;
            return findParameter(name);

        }

        private void buildGroupList()
        {
            groups.Clear();
            foreach (SettingsType st in settingsTypes)
            {
                bool isNew = true;
                foreach (String s in groups)
                    if (s.ToLower() == st.group.ToLower())
                        isNew = false;

                if (isNew)
                    groups.Add(st.group);

            }

        }

        public void PopulateGroupsDrop(string box)
        {
            Dropdown cbx = GameObject.Find(box).GetComponent<Dropdown>();
            buildGroupList();

            cbx.ClearOptions();
            List<Dropdown.OptionData> l = new List<Dropdown.OptionData>();

            for (int i = 0; i < groups.Count; i++)
            {
                ComboBoxItem ci = new ComboBoxItem();
                l.Add(new Dropdown.OptionData(groups[i]));
            }

            cbx.AddOptions(l);

            cbx.value = 0;

        }

        public void MaintainUpdatedParameters()
        {
            SettingsTypes set = new SettingsTypes();
            set.InitializeNew();

            // First, add new settings types
            foreach (SettingsType newSt in set.settingsTypes)
            {
                SettingsType found = null;
                foreach (SettingsType st in settingsTypes)
                    if (st.name == newSt.name)
                        found = st;

                if (found == null)
                    settingsTypes.Add(newSt);
                else
                {
                    // Update params, minmax etc
                    found.info = newSt.info;
                    found.propName = newSt.propName;
                    found.index = newSt.index;
                    found.minMax = newSt.minMax;
                    found.group = newSt.group;
                }

            }
            // Remove unused parameters!
            List<SettingsType> removeList = new List<SettingsType>();
            foreach (SettingsType curSt in settingsTypes)
            {
                bool found = false;
                foreach (SettingsType st in set.settingsTypes)
                    if (st.name == curSt.name)
                        found = true;

                if (!found)
                    removeList.Add(curSt);

            }
            foreach (SettingsType st in removeList)
                settingsTypes.Remove(st);



        }

        /* public SettingsTypes DeepCopy()
         {
             using (MemoryStream ms = new MemoryStream())
             {
                 BinaryFormatter formatter = new BinaryFormatter();
                 formatter.Serialize(ms, this);
                 ms.Position = 0;
                 return (SettingsTypes)formatter.Deserialize(ms);
             }
         }
         */

        public SettingsTypes DeepCopy()
        {
            SettingsTypes st = new SettingsTypes();
            st.name = name;
            foreach (SettingsType s in settingsTypes)
            {
                if (s.type == SettingsType.NUMBER)
                    st.settingsTypes.Add(
                        new SettingsType(s.name, s.type, s.propName, s.info, s.minMax, new Vector3(s.lower, s.upper, s.realizedValue), s.group, s.index)
                        );
                if (s.type == SettingsType.STRING)
                    st.settingsTypes.Add(
                       new SettingsType(s.name, s.propName, s.info, s.group, s.stringValue)
                    );

                if (s.type == SettingsType.COLOR)
                    st.settingsTypes.Add(
                       new SettingsType(s.name, s.propName, s.info, s.group, s.color, s.variation)
                    );

            }

            return st;
        }

        public void PopulateSettingsDrop(string box, string group)
        {
            Dropdown cbx = GameObject.Find(box).GetComponent<Dropdown>();

            cbx.ClearOptions();
            List<Dropdown.OptionData> l = new List<Dropdown.OptionData>();

            foreach (SettingsType st in settingsTypes)
            {
                if (st.group.ToLower() == group.ToLower())
                {
                    ComboBoxItem ci = new ComboBoxItem();
                    l.Add(new Dropdown.OptionData(st.name));
                }
            }

            cbx.AddOptions(l);
            cbx.value = 0;

        }


    }


    [System.Serializable]
    public class PlanetTypes
    {

      
        public List<SettingsTypes> planetTypes = new List<SettingsTypes>();
       
   


        public SettingsTypes NewPlanetType(SettingsTypes copy)
        {
            SettingsTypes st = new SettingsTypes();
            if (copy != null)
            {
                st = copy.DeepCopy();
                st.name = copy.name + " copy";
            }
            else
                st.InitializeNew();

            planetTypes.Add(st);
            currentSettings = st;
            return st;
        }

        public void MaintainNewParameters()
        {
            foreach (SettingsTypes pt in planetTypes)
                pt.MaintainUpdatedParameters();
        }

        public static SettingsTypes currentSettings = null;

        public void PopulatePlanetTypesDrop(string box, int type)
        {
            Dropdown cbx = GameObject.Find(box).GetComponent<Dropdown>();

            cbx.ClearOptions();
            List<Dropdown.OptionData> l = new List<Dropdown.OptionData>();

            foreach (SettingsTypes st in planetTypes)
            {
                ComboBoxItem ci = new ComboBoxItem();
                l.Add(new Dropdown.OptionData(st.name));
            }
            cbx.options = l;
            if (type == 0)
                cbx.value = 0;
            if (type == 1)
                cbx.value = l.Count - 1;

        }

        public SettingsTypes FindPlanetType(int idx)
        {
            return planetTypes[idx];
        }


        public SettingsType CycleSettings(SettingsType cur, int idx)
        {
            if (currentSettings == null)
                return cur;

            int i = currentSettings.settingsTypes.IndexOf(cur) + idx;
            if (i >= currentSettings.settingsTypes.Count)
                i -= currentSettings.settingsTypes.Count;
            if (i < 0)
                i += currentSettings.settingsTypes.Count;

            return currentSettings.settingsTypes[i];

        }

        public static PlanetTypes DeSerialize(string filename)
        {
            XmlSerializer deserializer = new XmlSerializer(typeof(PlanetTypes));
            TextReader textReader = new StreamReader(filename);
            PlanetTypes sz = (PlanetTypes)deserializer.Deserialize(textReader);
            textReader.Close();
            return sz;
        }
        static public void Serialize(PlanetTypes sz, string filename)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(PlanetTypes));
            TextWriter textWriter = new StreamWriter(filename);
            serializer.Serialize(textWriter, sz);
            textWriter.Close();
        }


        public static PlanetTypes p = new PlanetTypes();

        public static void Initialize()
        {


            string fname = RenderSettings.path + RenderSettings.planetTypesFilename;

            if (!File.Exists(fname))
            {
                World.FatalError("Could not find planet types file: " + RenderSettings.planetTypesFilename);
                return;
            }

            p = DeSerialize(fname);
            p.MaintainNewParameters();
        }

        public static void Save()
        {
            string fname = RenderSettings.path + RenderSettings.planetTypesFilename;
            Serialize(p, fname);
        }


    }

}
