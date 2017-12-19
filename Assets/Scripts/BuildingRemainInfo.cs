using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildingRemainInfo : MonoBehaviour {
    public Text textEx;

    private List<Text> texts;

	// Use this for initialization
	void Awake() {
        texts = new List<Text>();
	}

    private void ClearOld()
    {
        foreach (var text in texts)
        {
            Destroy(text.gameObject);
        }

        texts.Clear();
    }

    public void SetText(List<String> remain_list) 
    {
        ClearOld();

        foreach (var name in remain_list) {
            Text newText = Instantiate(textEx, transform);
            newText.gameObject.SetActive(true);
            newText.text = name;
            texts.Add(newText);
        }
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
