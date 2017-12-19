using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLoad : MonoBehaviour {

    public RectTransform Bar;
    public float FullTime;


    private float CurrentTimePassed;
    private float TotalBarWidth;

    // Use this for initialization
    void Start () {
        TotalBarWidth = Bar.sizeDelta.x;
        CurrentTimePassed = 0;
    }
	
	// Update is called once per frame
	void Update () {
		if (CurrentTimePassed < FullTime)
        {
            CurrentTimePassed += Time.deltaTime;
            UpdateBar();
        }
        else
        {
            Destroy(gameObject);
        }
	}

    void UpdateBar()
    {
        Bar.sizeDelta = new Vector2((float)(CurrentTimePassed / FullTime * TotalBarWidth), Bar.sizeDelta.y);
    }
}
