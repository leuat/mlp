using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;


namespace LemonSpawn {

public class ExampleSceneSlapDash : MonoBehaviour {

	// Use this for initialization

    private SlapDash slapDash = new SlapDash();
    private Language currentLanguage = null;
    private List<Syllable> currentSyllables = null;

    private int curRand= 0;
	void Start () {
        slapDash.Load("slapdash.xml");
        PopulateLanguageCombobox("ddSelectLanguage");
        GameObject.Find("ddSelectLanguage").GetComponent<Dropdown>().value = 3;
        ClickLanguage();
        PopulateTypeCombobox("ddSelectType");

	}


    public void SelectCurrentLanguage() {

        int idx = 0;
        for (int i=0;i<slapDash.languages.Count;i++) {
            if (slapDash.languages[i].name == currentLanguage.name)
                idx = i;
        }

        GameObject.Find("ddSelectLanguage").GetComponent<Dropdown>().value = idx;
    }

    public void ClickLanguage() {
        int value = GameObject.Find("ddSelectLanguage").GetComponent<Dropdown>().value;
        Language l = slapDash.languages[value];
        currentLanguage = l;
        System.Random r = new System.Random(curRand);
        string s = "";
        for (int i=0;i<200;i++) {
            s+=l.GenerateWord(r) + "   ";
        }
        GameObject.Find("Names").GetComponent<Text>().text = s + " (" + curRand + ")";
        curRand++;
        UpdateGUI();

    }

    private int getCurrentFix() {
        int fix = 0;
        if (GameObject.Find("togglePrefix").GetComponent<Toggle>().isOn)
            fix = fix | 1;
        if (GameObject.Find("toggleInfix").GetComponent<Toggle>().isOn)
            fix = fix | 2;
        if (GameObject.Find("toggleSuffix").GetComponent<Toggle>().isOn)
            fix = fix | 4;
        return fix;
    }

    public void UpdateGUI() {
 //       if (currentLanguage == null)
   //         return;
        int value = GameObject.Find("ddSelectType").GetComponent<Dropdown>().value;
        Syllable.Types t = (Syllable.Types)(Enum.GetValues(typeof(Syllable.Types))).GetValue(value);
        string txt = currentLanguage.getSyllableString(t, getCurrentFix(), out currentSyllables);
        GameObject.Find("inputList").GetComponent<InputField>().text = txt;

        GameObject.Find("inputName").GetComponent<InputField>().text = currentLanguage.name;
        GameObject.Find("inputExceptDoubleC").GetComponent<InputField>().text = currentLanguage.exceptDoubles;
        GameObject.Find("inputExceptDoubleCEnd").GetComponent<InputField>().text = currentLanguage.exceptDoubleEndings;
        GameObject.Find("inputMinSyll").GetComponent<InputField>().text = ""+currentLanguage.minSyllables;
        GameObject.Find("inputMaxSyll").GetComponent<InputField>().text = ""+currentLanguage.maxSyllables;
        GameObject.Find("inputNoDoubleC").GetComponent<InputField>().text = ""+currentLanguage.maxDoubleC;
        GameObject.Find("toggleAllowDCEnd").GetComponent<Toggle>().isOn= currentLanguage.allowDoubleCEnd;

  

    }

    public void NewLanguage() {
        Language l = new Language("New Language",2,5,1,true);
        slapDash.languages.Add(l);
        PopulateLanguageCombobox("ddSelectLanguage");
        currentLanguage = l;
        SelectCurrentLanguage();
        UpdateGUI();
    }

    public void DeleteLanguage() {
        slapDash.languages.Remove(currentLanguage);
        PopulateLanguageCombobox("ddSelectLanguage");
        currentLanguage = slapDash.languages[0];
        SelectCurrentLanguage();
        ClickLanguage();
        UpdateGUI();
    }

    public void SaveAll() {
        slapDash.Save("slapdash.xml");
    }


    public void UpdateData() {
        int value = GameObject.Find("ddSelectType").GetComponent<Dropdown>().value;
        Syllable.Types t = (Syllable.Types)Enum.GetValues(typeof(Syllable.Types)).GetValue(value);
        string list = GameObject.Find("inputList").GetComponent<InputField>().text;
        currentLanguage.setSyllableString(currentSyllables, list,getCurrentFix(), t);

        currentLanguage.name = GameObject.Find("inputName").GetComponent<InputField>().text;
        currentLanguage.exceptDoubles = GameObject.Find("inputExceptDoubleC").GetComponent<InputField>().text;
        currentLanguage.exceptDoubleEndings = GameObject.Find("inputExceptDoubleCEnd").GetComponent<InputField>().text;
        currentLanguage.minSyllables = int.Parse(GameObject.Find("inputMinSyll").GetComponent<InputField>().text);
        currentLanguage.maxSyllables = int.Parse(GameObject.Find("inputMaxSyll").GetComponent<InputField>().text);
        currentLanguage.maxDoubleC = int.Parse(GameObject.Find("inputNoDoubleC").GetComponent<InputField>().text);
        currentLanguage.allowDoubleCEnd = GameObject.Find("toggleAllowDCEnd").GetComponent<Toggle>().isOn;
        PopulateLanguageCombobox("ddSelectLanguage");
    }



    private void PopulateLanguageCombobox(string box)
        {
            Dropdown cbx = GameObject.Find(box).GetComponent<Dropdown>();
            cbx.ClearOptions();
            List<Dropdown.OptionData> l = new List<Dropdown.OptionData>();

            for (int i=0;i<slapDash.languages.Count;i++)
            {
                string s = slapDash.languages[i].name;
                ComboBoxItem ci = new ComboBoxItem();
                l.Add(new Dropdown.OptionData(s));
            }

            cbx.AddOptions(l);

        }

        private void PopulateTypeCombobox(string box)
        {
            Dropdown cbx = GameObject.Find(box).GetComponent<Dropdown>();
            cbx.ClearOptions();
            List<Dropdown.OptionData> l = new List<Dropdown.OptionData>();

            foreach (Syllable.Types t in Enum.GetValues(typeof(Syllable.Types)))
            {
                string s = t.ToString();
                ComboBoxItem ci = new ComboBoxItem();
                l.Add(new Dropdown.OptionData(s));
            }

            cbx.AddOptions(l);

        }

	// Update is called once per frame
	void Update () {
	
	}
}
}