using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine.EventSystems;
using System.Xml.Serialization;
using System.Reflection;


#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;

namespace LemonSpawn
{

   

    public class PlanetDesigner : World
    {

    	private System.Random r;

        private GameObject pnlNumber;
        private GameObject pnlString;
        private GameObject pnlBool;
        private GameObject pnlColor;


        private void HideSettingsPanels()
        {
            if (pnlNumber != null)
                pnlNumber.SetActive(false);
            if (pnlString != null)
                pnlString.SetActive(false);
            if (pnlBool != null)
                pnlBool.SetActive(false);
            if (pnlColor != null)
                pnlColor.SetActive(false);
        }


        public override void Start() {

    		base.Start();
            Update();
            pnlNumber = GameObject.Find("pnlGroupNumber");
            pnlString = GameObject.Find("pnlGroupString");
            pnlColor = GameObject.Find("pnlGroupColor");

            RenderSettings.MoveCam = false;

            PlanetTypes.Initialize();

            PopulatePlanetTypes(0);
            SelectPlanetType();
            HideSettingsPanels();


        }


        public void Save()
        {
            PlanetTypes.Save();
        }


        public void PopulatePlanetTypes(int type)
        {

            PlanetTypes.p.PopulatePlanetTypesDrop("DropDownPlanetType", type);

        }


        public void UpdateParamString()
        {
            if (settingsType == null)
                return;

            settingsType.stringValue = getInput("InputParamString");

        }


        public void SelectPlanetType()
        {
            int idx = GameObject.Find("DropDownPlanetType").GetComponent<Dropdown>().value;
            PlanetTypes.currentSettings = PlanetTypes.p.FindPlanetType(idx);
            PopulatePlanetGUI();
            SetNewPlanetType();

        }

        public void PopulatePlanetGUI()
        {
            setInput("InputPlanetTypeName", PlanetTypes.currentSettings.name);
            setInput("InputRadiusRange1", "" + PlanetTypes.currentSettings.RadiusRange.x);
            setInput("InputRadiusRange2", "" + PlanetTypes.currentSettings.RadiusRange.y);
            setInput("InputTempRange1", "" + PlanetTypes.currentSettings.TemperatureRange.x);
            setInput("InputTempRange2", "" + PlanetTypes.currentSettings.TemperatureRange.y);
            setInput("InputPlanetInfo", PlanetTypes.currentSettings.PlanetInfo);
        }

        



        public override void Update() {
			base.Update();

            if (Input.GetKeyUp(KeyCode.F1))
            {
                if (PlanetTypes.currentSettings != null)
                {
                    System.Random r = new System.Random();
                    PlanetTypes.currentSettings.Realize(r);
                    PlanetTypes.currentSettings.setParameters(SolarSystem.planet.pSettings,r);
                    PopulateSettings();
                }
            }
            if (Input.GetKeyUp(KeyCode.F2))
            {
                CycleParameter(-1);
            }
            if (Input.GetKeyUp(KeyCode.F3))
                CycleParameter(+1);


            if (!RenderSettings.MoveCam)
                RotateCamera();
         
        }
        private Vector3 mouseAccel = new Vector3();
        private float scrollWheelAccel = 0, scrollWheel;
        private DVector focusPoint = new DVector();
        private DVector focusPointCur = new DVector();
        private void RotateCamera()
        {
            float s = 1.0f;
            float theta = 0.0f;
            float phi = 0.0f;

            if (Input.GetMouseButton(1))
            {
                theta = s * Input.GetAxis("Mouse X");
                phi = s * Input.GetAxis("Mouse Y") * -1.0f;
            }
            focusPointCur = SolarSystem.planet.pSettings.properties.orgPos;
//            focusPointCur *= (float)((1.0f / RenderSettings.AU));
            mouseAccel += new Vector3(theta, phi, 0);

            scrollWheelAccel = Input.GetAxis("Mouse ScrollWheel");
            scrollWheel = scrollWheel * 0.9f + scrollWheelAccel*0.1f;

            double scale = 10000;
            Quaternion q = Quaternion.AngleAxis(mouseAccel.x, SpaceCamera.transform.up);
            Vector3 p = q* (((SpaceCamera.getPos()/RenderSettings.AU) - focusPointCur)*scale).toVectorf();
            p *= (-scrollWheel*0.25f + 1);

            q = Quaternion.AngleAxis(mouseAccel.y, SpaceCamera.transform.right);
            p = q * p; 

//            p += focusPointCur.toVectorf();
            SpaceCamera.SetLookCamera(new DVector(p)/scale + focusPointCur, (p)*-1, Vector3.up);

            mouseAccel *= 0.9f;
        }


        public void NewPlanetType()
        {
            PlanetTypes.currentSettings = PlanetTypes.p.NewPlanetType(null);
            PopulatePlanetGUI();
            PopulatePlanetTypes(1);
            SetNewPlanetType();
        }

        public void DeletePlanetType()
        {
            PlanetTypes.p.planetTypes.Remove(PlanetTypes.currentSettings);
            PopulatePlanetTypes(0);
            SetNewPlanetType();
        }

        public void CopyPlanetType()
        {
            PlanetTypes.currentSettings = PlanetTypes.p.NewPlanetType(PlanetTypes.currentSettings);
            PopulatePlanetGUI();
            PopulatePlanetTypes(1);
            SetNewPlanetType();


        }


        public void ToggleFlyCam()
        {
            RenderSettings.MoveCam = !RenderSettings.MoveCam;
        }

        public void SetNewPlanetType() {
            PlanetTypes.currentSettings.PopulateGroupsDrop("DropdownGroups");
            //            PlanetTypes.currentSettings.Realize(new System.Random());
            SelectGroup();
            PlanetTypes.currentSettings.setParameters(SolarSystem.planet.pSettings, new System.Random());
        }


        public void SelectGroup()
        {
            string group = PlanetTypes.currentSettings.groups[GameObject.Find("DropdownGroups").GetComponent<Dropdown>().value];
            PlanetTypes.currentSettings.PopulateSettingsDrop("DropdownSettings",group);
            settingsType = PlanetTypes.currentSettings.getSettingsFromDropdown("DropdownSettings");
            PopulateSettings();
        }

        public SettingsType settingsType;

        public void SelectParameter()
        {
            if (PlanetTypes.currentSettings == null)
                return;

            settingsType = PlanetTypes.currentSettings.getSettingsFromDropdown("DropdownSettings");

            PopulateSettings();
        }

        public void CycleParameter(int idx)
        {
            if (PlanetTypes.currentSettings == null)
                return;

            settingsType = PlanetTypes.p.CycleSettings(settingsType, idx);
            PopulateSettings();

        }


        private void setText(string box, string text)
        {
          //  Debug.Log(box);
            GameObject.Find(box).GetComponent<Text>().text = text;
        }

        private void setInput(string box, string text)
        {
//            Debug.Log(box);
            InputField f = GameObject.Find(box).GetComponent<InputField>();
            if (f==null)
            {
                Debug.Log("COULD NOT FIND TEXT INPUT : " + box);
                return;
            }
            if (text == null)
                text = "";

            f.text = text;
        }

        private string getInput(string box)
        {
            //  Debug.Log(box);
            return GameObject.Find(box).GetComponent<InputField>().text;
        }


        private void setSlider(string n, float min, float max, float val)
        {
            Slider s = GameObject.Find(n).GetComponent<Slider>();
            s.minValue = min;
            s.maxValue = max;
            s.value = val;
        }


        public void PopulatePlanetTypeFromGUI()
        {
            if (PlanetTypes.currentSettings == null)
                return;

            PlanetTypes.currentSettings.name = getInput("InputPlanetTypeName");
            PlanetTypes.currentSettings.RadiusRange.x = (int)float.Parse(getInput("InputRadiusRange1"));
            PlanetTypes.currentSettings.RadiusRange.y = (int)float.Parse(getInput("InputRadiusRange2"));
            PlanetTypes.currentSettings.TemperatureRange.x = (int)float.Parse(getInput("InputTempRange1"));
            PlanetTypes.currentSettings.TemperatureRange.y = (int)float.Parse(getInput("InputTempRange2"));
            PlanetTypes.currentSettings.PlanetInfo = getInput("InputPlanetInfo");


            PopulatePlanetTypes(2);


        }


    public void MoveSliderMax()
        {
            if (settingsType == null)
                return;

            settingsType.upper = GameObject.Find("SliderMaxValue").GetComponent<Slider>().value;
            if (settingsType.upper<settingsType.lower)
            {
                settingsType.upper = settingsType.lower;
                GameObject.Find("SliderMaxValue").GetComponent<Slider>().value = settingsType.upper;

            }
            PopulateTextValues();

        }

        public void MoveSliderMin()
        {
            if (settingsType == null)
                return;

            settingsType.lower = GameObject.Find("SliderMinValue").GetComponent<Slider>().value;
            if (settingsType.lower > settingsType.upper)
            {
                settingsType.lower = settingsType.upper;
                GameObject.Find("SliderMinValue").GetComponent<Slider>().value = settingsType.lower;

            }

            PopulateTextValues();
        }

        public void MoveSliderRealized()
        {
            if (settingsType == null)
                return;
            if (SolarSystem.planet == null)
                return;
            
            settingsType.realizedValue = GameObject.Find("SliderRealizedValue").GetComponent<Slider>().value;
            
            settingsType.setParameter(SolarSystem.planet.pSettings);

            PopulateTextValues();
        }

        private void PopulateTextValues()
        {
            if (settingsType == null)
                return;

            SettingsType s = settingsType;

            if (s.type == SettingsType.NUMBER)
            {
                setText("txtRealizedValueVal", ""+s.realizedValue);
                setText("txtMinValueVal", "" + s.lower);
                setText("txtMaxValueVal", "" + s.upper);
            }

        }

        public void setColor(Color org, Color var)
        {
            GameObject.Find("ColorPicker").GetComponent<ColorPicker>().CurrentColor = org;
            GameObject.Find("ColorPickerVariation").GetComponent<ColorPicker>().CurrentColor = var;

        }

        public void getColor(out Color org, out Color var)
        {
            org = GameObject.Find("ColorPicker").GetComponent<ColorPicker>().CurrentColor;
            var = GameObject.Find("ColorPickerVariation").GetComponent<ColorPicker>().CurrentColor;

        }

        public void SelectColor()
        {
            if (settingsType == null)
                return;

            getColor(out settingsType.color, out settingsType.variation);
            settingsType.realizedColor = settingsType.color;
            if (SolarSystem.planet!=null)
                settingsType.setParameter(SolarSystem.planet.pSettings);
        }

        public void SaveScreenshot()
        {

           WriteScreenshot(RenderSettings.screenshotDir, 2048/2,1080/2);

        }

        



        public void PopulateSettings()
        {
            if (settingsType == null)
                return;

            SettingsType s = settingsType;
            setText("settingsName", s.name);
            setText("settingsInfoText", s.info);
            HideSettingsPanels();
            if (s.type == SettingsType.NUMBER)
            {
                pnlNumber.SetActive(true);
                setSlider("SliderRealizedValue", s.minMax.x, s.minMax.y, s.realizedValue);
                setSlider("SliderMinValue", s.minMax.x, s.minMax.y, s.lower);
                setSlider("SliderMaxValue", s.minMax.x, s.minMax.y, s.upper);
               // Debug.Log(s.minMax);
              //  Debug.Log(s.lower);
             //   Debug.Log(s.upper);
                PopulateTextValues();

            }
            if (s.type == SettingsType.STRING)
            {
                pnlString.SetActive(true);
                setInput("InputParamString", s.stringValue);

            }
            if (s.type == SettingsType.COLOR)
            {
                pnlColor.SetActive(true);
                setColor(s.color, s.variation);
            }



        }


    }


}
