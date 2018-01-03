using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FactoryDrop : MonoBehaviour {
    public Image image;
    public Text text;

    private bool Started;
    private Vector3 EndPos;
    private Vector3 Direction;
    private Factory Parent;
	// Use this for initialization
	void Start () {
        Started = false;
	}

    public void SetData(int resource_id, int count, Factory parent)
    {
        text.text = count.ToString();
        this.image.material = StorageManager.GetResource(resource_id).Image;
        Started = false;
        Parent = parent;
    }

    public void Start(Vector3 EndPos)
    {
        this.EndPos = EndPos;
        Started = true;
        Direction = Vector3.Normalize(EndPos - transform.position);
    }
	
	// Update is called once per frame
	void Update () {
		if (Started)
        {
            if (transform.position.y < EndPos.y)
            {
                transform.position += Direction * Time.deltaTime;
            }
            else
            {
                Started = false;
                Parent.RemoveDrop(this);
            }
        }
	}
}
