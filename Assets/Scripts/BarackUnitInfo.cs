using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BarackUnitInfo : MonoBehaviour {
    public Image image;
    public Text text;

    private int unit_id;

    private BarackInfo parent;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void SetInfo(int unit_id, BarackInfo barack)
    {
        this.unit_id = unit_id;
        Item unit = ItemManager.GetItemByID(unit_id);
        parent = barack;
        text.text = unit.Name;
        image.material = unit.material;
    }

    public void OnClick()
    {
        parent.CreateUnit(unit_id);
    }
}
