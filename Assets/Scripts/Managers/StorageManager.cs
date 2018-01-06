using Npgsql;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class GameResource
{
    public int ID { get; }
    public String Name { get; }
    public String Type { get; }
    public Material Image { get; }

    public GameResource(IDataReader dr)
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

        using (IDataReader dr = Managers.Database.GetSQLiteQuery("SELECT * FROM resources;"))
        {

            while (dr.Read())
            {
                GameResource res = new GameResource(dr);
                gameResources[res.ID] = res;
            }

            userResources = new Dictionary<int, int>();

            dr.Close();
        }
        using (IDataReader dr = Managers.Database.GetSQLiteQuery(String.Format("SELECT resource_id, count FROM InitialStorage;"
            , Managers.Session.GetSession())))
        {
            while (dr.Read())
            {
                userResources[Convert.ToInt32(dr["resource_id"])] = Convert.ToInt32(dr["count"]);
            }

            dr.Close();
        }

        using (IDataReader dr = Managers.Database.GetSQLiteQuery(String.Format("SELECT resource_id, count FROM Storage WHERE session_id = {0};"
            , Managers.Session.GetSession())))
        {
            while (dr.Read())
            {
                userResources[Convert.ToInt32(dr["resource_id"])] = Convert.ToInt32(dr["count"]);
            }

            dr.Close();
        }

        using (IDataReader dr = Managers.Database.GetSQLiteQuery("SELECT * FROM MaxStorage;"))
        {

            resourceLimits = new Dictionary<int, int>();

            while (dr.Read())
            {
                resourceLimits[Convert.ToInt32(dr["resource_id"])] = Convert.ToInt32(dr["count"]);
            }
            dr.Close();
        }


        stat_panel = panel;
        stat_panel.SetPanel();
        status = ManagerStatus.Started;
    }

    public static void AddResource(int resource_id, int count)
    {
        if (resourceLimits.ContainsKey(resource_id))
            userResources[resource_id] = Mathf.Min(userResources[resource_id] + count, resourceLimits[resource_id]);
        else
            userResources[resource_id] += count;
        stat_panel.UpdatePanel();
        Managers.Store.GetStore().UpdateRequirements();
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
            Managers.Database.PutSQLiteQuery(String.Format("UPDATE storage SET count={1} WHERE resource_id = {0} AND session_id = {2}; INSERT INTO storage (resource_id, count, session_id) SELECT {0}, {1}, {2} WHERE NOT EXISTS (SELECT 1 FROM storage WHERE resource_id = {0} AND session_id = {2});"
                , key, userResources[key], Managers.Session.GetSession()));
        }
    }

    public static void PayResources(Dictionary<int, int> res)
    {
        if (EnoughResources(res))
        {
            foreach (var key in res.Keys)
            {
                userResources[key] -= res[key];
            }
        }
        stat_panel.UpdatePanel();
        Managers.Store.GetStore().UpdateRequirements();
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
