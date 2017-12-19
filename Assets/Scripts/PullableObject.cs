using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PullableObject : MonoBehaviour {

    public int UniqueID;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void SetID(int ID)
    {
        UniqueID = ID;
    }

    public int GetID()
    {
        return UniqueID;
    }
}
