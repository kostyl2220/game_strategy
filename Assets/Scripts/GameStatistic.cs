using Npgsql;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameStatistic : MonoBehaviour {
    public GameObject statPanel;
    public Text HardestRes;

    public Text UnitName;
    public Text UnitLevel;
    public Text UnitDPS;

    public Text TotalUnitsPower;

    public InputField InputRadius;
    public InputField InputDPS;

    public Text TextName;
    public Text TextLevel;
    public Text TextTotal;
    public Text TextRadius;
    public Text TextDPS;

    public Button Button;

    private bool opened;
	// Use this for initialization
	void Start () {
        opened = false;
        statPanel.gameObject.SetActive(false);
        GetHardestRes();
        GetMostPowerFullUnit();
        GetTotalDPS();
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void OnClick()
    {
        opened = !opened;
        statPanel.gameObject.SetActive(opened);
    }

    void GetHardestRes()
    {
        String name = "";
        NpgsqlDataReader dr = Managers.Database.GetQuery("SELECT * FROM get_hardest_res()");
        while (dr.Read())
        {
            name = dr["res"].ToString();
        }
        HardestRes.text = name;
    }

    void GetMostPowerFullUnit()
    {
        NpgsqlDataReader dr = Managers.Database.GetQuery(String.Format("SELECT * FROM get_most_dangerous_unit({0})", Managers.Session.GetSession()));
        while (dr.Read())
        {
            String Name = dr["name"].ToString();
            String level = dr["level"].ToString();
            String dps = dr["dps"].ToString();
            UnitName.text = Name;
            UnitLevel.text = level;
            UnitDPS.text = dps;
        }
    }

    void GetTotalDPS()
    {
        NpgsqlDataReader dr = Managers.Database.GetQuery(String.Format("SELECT get_total_dps({0}) AS val", Managers.Session.GetSession()));

        while (dr.Read())
        {
            String DPS = dr["val"].ToString();
            TotalUnitsPower.text = DPS;
        }
    }
    
    public void GetCheapestTower()
    {
        double radius = Convert.ToDouble(InputRadius.text);
        double dps = Convert.ToDouble(InputDPS.text);
        NpgsqlDataReader dr = Managers.Database.GetQuery(String.Format("SELECT * FROM get_cheapest_tower({0}, {1});", radius, dps));
        while (dr.Read())
        {
            String Name = dr["_name"].ToString();
            String Level = dr["_level"].ToString();
            String GenValue = dr["_gen_value"].ToString();
            String DPS = dr["_dps"].ToString();
            String Raduis = dr["_radius"].ToString();
            TextName.text = Name;
            TextLevel.text = Level;
            TextTotal.text = GenValue;
            TextRadius.text = Raduis;
            TextDPS.text = DPS;
        }
    }
}
