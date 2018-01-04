using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FactoryResource : MonoBehaviour {
    private Factory Parent;
    private float ReloadTime;
    private int ResCount;
    private int resource_id;

    private float lastReload = -1000;
    private bool init;
	// Use this for initialization
	void Start () {

	}

    void Awake ()
    {
        init = false;
    }
	
	// Update is called once per frame
	void Update () {
		if (init && Time.time > lastReload + ReloadTime)
        {
            StorageManager.AddResource(resource_id, ResCount);
            FactoryDrop drop = Parent.GetDrop();

            drop.SetData(resource_id, ResCount, Parent);
            Parent.AddToQueue(drop);

            lastReload = Time.time;
        }
	}

    public void SetData(int res_id, float time, int count, Factory parent)
    {
        resource_id = res_id;
        ReloadTime = time;
        ResCount = count;
        Parent = parent;
        init = true;
    }
}
