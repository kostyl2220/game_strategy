using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResourceCell : MonoBehaviour {

    public Text text;
    public Image image;
    public Text incomeText;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void SetData(GameResource resource, int count)
    {
        text.text = count.ToString();
        image.material = resource.Image;
    }

    public void UpdateValue(int count)
    {
        text.text = count.ToString();
    }

    public void SetIncome(double income)
    {
        incomeText.text = String.Format("+{0}", income);
        incomeText.gameObject.SetActive(true);
        StartCoroutine(RunShow());
    }

    IEnumerator RunShow()
    {
        yield return new WaitForSeconds(5);
        incomeText.gameObject.SetActive(false);
    }
}
