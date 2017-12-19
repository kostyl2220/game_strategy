using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FactoryDrop : MonoBehaviour {
    public Image image;
    public Text text;

	// Use this for initialization
	void Start () {
		
	}

    public void SetData(int resource_id, int count)
    {
        text.text = count.ToString();
        this.image.material = StorageManager.GetResource(resource_id).Image;
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
