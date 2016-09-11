using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;


namespace LemonSpawn {

public class ExampleSceneSlapDash : MonoBehaviour {

	// Use this for initialization

    private SlapDash slapDash = new SlapDash();
    private int curRand= 0;
	void Start () {
        PopulateLanguageCombobox("ddSelectLanguage");
            GameObject.Find("ddSelectLanguage").GetComponent<Dropdown>().value = 3;
        ClickLanguage();
	}


    public void ClickLanguage() {
        int value = GameObject.Find("ddSelectLanguage").GetComponent<Dropdown>().value;
        Language l = slapDash.languages[value];
        System.Random r = new System.Random(curRand);
        string s = "";
        for (int i=0;i<200;i++) {
            s+=l.GenerateWord(r) + "   ";
        }
        GameObject.Find("Names").GetComponent<Text>().text = s + " (" + curRand + ")";
        curRand++;

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

	// Update is called once per frame
	void Update () {
	
	}
}
}