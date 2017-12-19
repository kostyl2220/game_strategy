using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BulletInfo : MonoBehaviour {

    public Text DamageText;
    public Text RadiusText;
    public Text NameText;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void SetData(TowerBullet bullet)
    {
        NameText.text = String.Format("{0}", bullet.Name);
        DamageText.text = String.Format("Damage: {0}", bullet.Damage);
        RadiusText.text = String.Format("Radius: {0}", bullet.Radius);
    }
}
