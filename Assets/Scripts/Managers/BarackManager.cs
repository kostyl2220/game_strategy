using Npgsql;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarackManager : MonoBehaviour, IGameManager {
    public ManagerStatus status { get; private set; }

    private Dictionary<int, Dictionary<int, List<int>>> UnitList;

    public void Shutdowm()
    {

    }

    public void Startup()
    {
        UnitList = new Dictionary<int, Dictionary<int, List<int>>>();

        NpgsqlDataReader dr = Managers.Database.GetQuery("SELECT * FROM UnitFactory;");
        while (dr.Read())
        {
            int item_id = Convert.ToInt32(dr["item_id"]);
            int level = Convert.ToInt32(dr["level"]);
            int unit_id = Convert.ToInt32(dr["unit_id"]);

            if (!UnitList.ContainsKey(item_id))
                UnitList[item_id] = new Dictionary<int, List<int>>();

            if (!UnitList[item_id].ContainsKey(level))
                UnitList[item_id][level] = new List<int>();

            UnitList[item_id][level].Add(unit_id);
        }

        status = ManagerStatus.Started;
    }

    public List<int> GetBarackUnits(int item_id, int level)
    {
        if (!UnitList.ContainsKey(item_id))
            return new List<int>();

        if (!UnitList[item_id].ContainsKey(level))
            return new List<int>();

        return UnitList[item_id][level];
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
