using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProtectionChar : MonoBehaviour {

    public Text text;
    public Image image;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void SetData(TowerChar chr, double value)
    {
        text.text = String.Format("{0}: {1}", chr.Name, value);
        image.material = chr.Material;
    }
}
