using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine.EventSystems;
using System.Xml.Serialization;


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


        private void HideSettingsPanels()
        {
            if (pnlNumber != null)
                pnlNumber.SetActive(false);
            if (pnlString != null)
                pnlString.SetActive(false);
            if (pnlBool != null)
                pnlBool.SetActive(false);
        }


        public override void Start() {

    		base.Start();
            Update();
            pnlNumber = GameObject.Find("pnlGroupNumber");
            pnlString = GameObject.Find("pnlGroupString");

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
            setInput("InputPlanetTypeName", PlanetTypes.currentSettings.name);
            SetNewPlanetType();

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

		  }    	

        public void NewPlanetType()
        {
            PlanetTypes.currentSettings = PlanetTypes.p.NewPlanetType();
            GameObject.Find("txtPlanetName").GetComponent<Text>().text = PlanetTypes.currentSettings.name;
            PopulatePlanetTypes(1);
            SetNewPlanetType();


        }

        public void SetNewPlanetType() {
            PlanetTypes.currentSettings.PopulateGroupsDrop("DropdownGroups");
            //            PlanetTypes.currentSettings.Realize(new System.Random());
            SelectGroup();
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

        private void setText(string box, string text)
        {
          //  Debug.Log(box);
            GameObject.Find(box).GetComponent<Text>().text = text;
        }

        private void setInput(string box, string text)
        {
           
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



        }


    }


}
