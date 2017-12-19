using Npgsql;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameResource
{
    public int ID { get; }
    public String Name { get; }
    public String Type { get; }
    public Material Image { get; }

    public GameResource(NpgsqlDataReader dr)
    {
        ID = Convert.ToInt32(dr["ID"]);
        Name = dr["Name"].ToString();
        Type = dr["Type"].ToString();
        Image = Resources.Load<Material>(dr["Icon"].ToString());
    }
}

public class StorageManager : MonoBehaviour, IGameManager {

    public ManagerStatus status { get; private set; }
    public StatPanel panel;

    private static Dictionary<int, GameResource> gameResources;
    private static Dictionary<int, int> userResources;
    private static Dictionary<int, int> resourceLimits;
    private static StatPanel stat_panel;

    public void Startup()
    {
        gameResources = new Dictionary<int, GameResource>();

        NpgsqlDataReader dr = Managers.Database.GetQuery("SELECT * FROM resources;");

        while(dr.Read())
        {
            GameResource res = new GameResource(dr);
            gameResources[res.ID] = res;
        }

        dr = Managers.Database.GetQuery(String.Format("SELECT * FROM get_user_resources({0});", Managers.Session.GetSession()));

        userResources = new Dictionary<int, int>();

        while (dr.Read())
        {
            userResources[Convert.ToInt32(dr["resource_id"])] = Convert.ToInt32(dr["count"]);
        }

        dr = Managers.Database.GetQuery("SELECT * FROM MaxStorage;");

        resourceLimits = new Dictionary<int, int>();

        while (dr.Read())
        {
            resourceLimits[Convert.ToInt32(dr["resource_id"])] = Convert.ToInt32(dr["count"]);
        }

        stat_panel = panel;
        stat_panel.SetPanel();
        SetStartTotalResCount();
        status = ManagerStatus.Started;
    }

    public void SetStartTotalResCount()
    {
        NpgsqlDataReader dr = Managers.Database.GetQuery(String.Format("SELECT * FROM get_total_res_count({0})", Managers.Session.GetSession()));
        Dictionary<int, double> total_res = new Dictionary<int, double>();
        while (dr.Read())
        {
            int resource = Convert.ToInt32(dr["name"]);
            double count = Convert.ToDouble(dr["count"]);
            total_res[resource] = count;
        }
        stat_panel.SetTotalIncome(total_res);
    }

    public static void AddResource(int resource_id, int count)
    {
        if (resourceLimits.ContainsKey(resource_id))
            userResources[resource_id] = Mathf.Min(userResources[resource_id] + count, resourceLimits[resource_id]);
        else
            userResources[resource_id] += count;
        stat_panel.UpdatePanel();
        StoreManager.GetStore().UpdateRequirements();
    }

    public static bool EnoughResources(Dictionary<int, int> res)
    {
        foreach(var pair in res)
        {
            if (!userResources.ContainsKey(pair.Key) 
                || userResources[pair.Key] < pair.Value)
                return false;
        }
        return true;
    }

    public static void SaveStorage()
    {
        foreach (var key in gameResources.Keys)
        {
            NpgsqlDataReader dr = Managers.Database.GetQuery(String.Format("SELECT set_storage({0}, {1}, {2})", key, userResources[key], Managers.Session.GetSession()));
            dr.Close();
        }
    }

    public static void PayResources(Dictionary<int, int> res)
    {
        if (EnoughResources(res))
        {
            foreach (var key in res.Keys)
            {
                userResources[key] -= res[key];
                NpgsqlDataReader dr = Managers.Database.GetQuery(String.Format("SELECT set_storage({0}, {1}, {2})", key, userResources[key], Managers.Session.GetSession()));
                dr.Close();
            }
        }
        stat_panel.UpdatePanel();
        StoreManager.GetStore().UpdateRequirements();
    }

    public static Dictionary<int, int> GetUserResources()
    {
        return userResources;
    }

    public static GameResource GetResource(int resource_id)
    {
        return gameResources[resource_id];
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void Shutdowm()
    {
        SaveStorage();
    }
}
