﻿using Npgsql;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class StoreManager : MonoBehaviour, IGameManager {
    public ManagerStatus status { get; private set; }
    public Store store;
    public static Dictionary<int, Dictionary<int, Dictionary<int, int>>> itemPrices;

    public void Startup()
    {
        itemPrices = new Dictionary<int, Dictionary<int, Dictionary<int, int>>> ();
        using (IDataReader dr = Managers.Database.GetSQLiteQuery("SELECT * FROM ItemPrices;"))
        {

            while (dr.Read())
            {
                int item_id = Convert.ToInt32(dr["item_id"]);
                int level = Convert.ToInt32(dr["level"]);
                if (!itemPrices.ContainsKey(item_id))
                    itemPrices[item_id] = new Dictionary<int, Dictionary<int, int>>();
                if (!itemPrices[item_id].ContainsKey(level))
                    itemPrices[item_id][level] = new Dictionary<int, int>();
                itemPrices[item_id][level][Convert.ToInt32(dr["resource_id"])] = Convert.ToInt32(dr["count"]);
            }
            dr.Close();
        }
        store.SetItems(Managers.Items.GetStoreItems());
        status = ManagerStatus.Started;
    }

    public static Dictionary<int, int> GetItemPrice(int item_id, int level = 0)
    {
        if (!itemPrices[item_id].ContainsKey(level))
        {
            return null;
        }
        return itemPrices[item_id][level];
    }

    public Store GetStore()
    {
        return store;
    }

    public static bool IsItemPrice(int item_id)
    {
        return itemPrices.ContainsKey(item_id);
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void Shutdowm()
    {
        
    }
}
