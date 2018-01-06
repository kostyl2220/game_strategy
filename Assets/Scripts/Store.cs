using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Npgsql;
using System;

public class Store : MonoBehaviour {

    public GameObject content;
    
    public Grid grid;
    public ButtonStoreClick button;

    public int Height = 100;

    private List<StoreCell> storeCells;
    
    // Use this for initialization
    //TODO SEPARATE ICONS FROM ITEMS
    void Awake () {
       
    }

    public Store()
    {
        storeCells = new List<StoreCell>();
    }

    void SetPrices()
    {

    }

    public void SetItems(IEnumerable items)
    {
        storeCells.Clear();
        int num = 0;
        foreach (Item childItem in items)
        { 
            if (!StoreManager.IsItemPrice(childItem.GetID()))
                continue;
            Vector3 pos = new Vector3(transform.position.x, transform.position.y - (num + 0.5f) * Height, 0);
            GameObject instance = Instantiate(content, pos, transform.rotation, transform);
            instance.SetActive(true);
            StoreCell cell = instance.GetComponent<StoreCell>();
            cell.SetItem(childItem);
            storeCells.Add(cell);
            num += 1;    
        }
    }

    public void UpdateRequirements()
    {
        foreach(var cell in storeCells)
        {
            cell.CheckValidation();
        }
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void PlaceOnGrid(Item item)
    {
        button.TaskOnClick();
        grid.PlaceOnGrid(item.GetID());
    }
}
