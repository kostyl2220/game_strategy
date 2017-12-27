using Npgsql;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class FactoryManager : MonoBehaviour, IGameManager {

    public ManagerStatus status { get; private set; }

    private Dictionary<int, Dictionary<int, Dictionary<int, int[]>>> factories; //item_id, level, resource_id

    public void Shutdowm()
    {
        
    }

    public void Startup()
    {
        factories = new Dictionary<int, Dictionary<int, Dictionary<int, int[]>>>();
        using (IDataReader dr = Managers.Database.GetSQLiteQuery("SELECT f.item_id, fl.level, f.resource_id, fl.count, fl.time FROM Factories as f JOIN FactoryLevels AS fl ON fl.factory_id = f.id ORDER BY item_id, level;"))
        {
            while (dr.Read())
            {
                int item_id = Convert.ToInt32(dr["item_id"]);
                int level = Convert.ToInt32(dr["level"]);
                int resource = Convert.ToInt32(dr["resource_id"]);
                int count = Convert.ToInt32(dr["count"]);
                int time = Convert.ToInt32(dr["time"]);
                if (!factories.ContainsKey(item_id))
                {
                    factories[item_id] = new Dictionary<int, Dictionary<int, int[]>>();
                }
                if (!factories[item_id].ContainsKey(level))
                {
                    factories[item_id][level] = new Dictionary<int, int[]>();
                }
                factories[item_id][level][resource] = new int[] { count, time };
            }
            dr.Close();
        }

        InitFactories();

        status = ManagerStatus.Started;
    }

    public void CheckOnFactory(Item item)
    {
        int item_id = item.GetID();
        int item_level = item.GetLevel();
        if (factories.ContainsKey(item_id))
        {
            Dictionary<int, int[]> resources = GetFactory(item_id, item_level);
            item.GetComponent<Factory>().SetResources(resources);
        }
    }

    public bool IsFactory(int item_id)
    {
        return factories.ContainsKey(item_id);
    }

    void InitFactories()
    {
        foreach (Item item in ItemManager.GetItems())
        {
            CheckOnFactory(item);
        }
    }

    public Dictionary<int, int[]> GetFactory(int item_id, int level)
    {
        if (!factories[item_id].ContainsKey(level))
        {
            return new Dictionary<int, int[]>();
        }
        return factories[item_id][level];
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
