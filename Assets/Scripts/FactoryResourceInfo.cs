using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FactoryResourceInfo : MonoBehaviour {
    public Text count;
    public Text time;
    public Image image;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void SetData(Material material, int count, int time)
    {
        image.material = material;
        this.count.text = String.Format("+{0}", count);
        this.time.text = String.Format("{0} sec", time / 1000f);
    }
}
