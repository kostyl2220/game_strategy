using Npgsql;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class UnitManager : MonoBehaviour, IGameManager {
    public ManagerStatus status { get; private set; }
    public GameObject unitPool;
    public MouseSelection selection;

    private Dictionary<int, TowerChar> unitChars;
    private Dictionary<int, Dictionary<int, Dictionary<int, double>>> UnitLevels;

    private Dictionary<int, Unit> units;
    private int unic_unit_id = 1;

    void SetupUnits()
    {
        units = new Dictionary<int, Unit>();

        foreach (Transform child in unitPool.transform)
        {
            Unit unit = child.GetComponent<Unit>();
            units[unic_unit_id++] = unit;
        }

    }

    public int[,] AddUnitMask(int[,] mask)
    {
        int[,] new_mask = (int[,])mask.Clone();
        foreach (var unit in units.Values)
        {
            int X = Managers.Items.GameGrid.GetXByPosition(unit.transform.position);
            int Z = Managers.Items.GameGrid.GetZByPosition(unit.transform.position);

            int unique_id = unit.GetUniqueID();
            if (Managers.Items.GameGrid.InRangePoint(X, Z))
            {
                for (int x = X; x < X + unit.SizeX; ++x)
                {
                    for (int z = Z; z < Z + unit.SizeZ; ++z)
                    {
                        new_mask[x - 1, z - 1] = 1;
                    }
                }
            }
        }

        return new_mask;
    }

    public void DisableAllSelection()
    {
        selection.UnSelectAll();
    }

    public void CheckOnUnit(Item item)
    {
        int item_id = item.GetID();
        if (UnitLevels.ContainsKey(item_id))
        {
            Unit unit = item.GetComponent<Unit>();
            unit.InitUnit();
            units[unic_unit_id++] = unit;
        }
    }

    public bool IsUnit(int item_id)
    {
        return UnitLevels.ContainsKey(item_id);
    }

    void InitUnits()
    {
        foreach (Item item in ItemManager.GetItems())
        {
            CheckOnUnit(item);
        }
    }

    void LoadSession()
    {
        using (IDataReader dr = Managers.Database.GetSQLiteQuery(String.Format("SElECT SavedUnits.unit_id AS item_id, pos_x AS x, pos_z AS z, rotation AS rot, SavedUnits.level, SavedUnits.hp FROM SavedUnits WHERE session_id = {0};"
            , Managers.Session.GetSession())))
        {
            while (dr.Read())
            {
                int id = Convert.ToInt32(dr["item_id"]);
                float posX = (float)Convert.ToDouble(dr["x"]);
                float posZ = (float)Convert.ToDouble(dr["z"]);
                float rot = (float)Convert.ToDouble(dr["rot"]);
                int level = Convert.ToInt32(dr["level"]);
                double hp = Convert.ToDouble(dr["hp"]);

                Unit item = GetNewUnit(id);
                item.SetLevel(level);
                item.SetHP(hp);

                item.transform.position = new Vector3(posX, 0, posZ);

                item.transform.rotation = Quaternion.Euler(0, rot, 0);

                item.InitUnit();
            }
            dr.Close();
        }
    }

    public void Shutdowm()
    {
        Managers.Database.PutSQLiteQuery(String.Format("DELETE FROM SavedUnits WHERE session_id = {0};", Managers.Session.GetSession()));
        foreach (Unit item in units.Values)
        {
            Vector2 pos = new Vector2(item.transform.position.x, item.transform.position.z);
            Managers.Database.PutSQLiteQuery(String.Format("INSERT INTO SavedUnits(unit_id, pos_x, pos_z, rotation, level, hp, session_id) VALUES({0}, {1}, {2}, {3}, {4}, {5}, {6})",
                item.GetID(), pos.x, pos.y, item.transform.rotation.eulerAngles.y, item.GetLevel(), item.GetHP(), Managers.Session.GetSession()));
        }
    }

    public void Startup()
    {
        SetupUnits();

        unitChars = new Dictionary<int, TowerChar>();

        using (IDataReader dr = Managers.Database.GetSQLiteQuery("SELECT * FROM TowerChars"))
        {

            while (dr.Read())
            {
                int char_id = Convert.ToInt32(dr["ID"]);

                unitChars[char_id] = new TowerChar(dr["name"].ToString(), dr["image"].ToString());
            }

            UnitLevels = new Dictionary<int, Dictionary<int, Dictionary<int, double>>>();

            dr.Close();
        }

        using (IDataReader dr = Managers.Database.GetSQLiteQuery("SELECT u.item_id, ul.level,  ulc.unitcharid AS char_id, ulc.value FROM Units AS u	JOIN UnitLevels AS ul ON ul.unit_id = u.ID JOIN UnitLevelChars AS ulc ON ulc.unitlevelid = ul.ID;"))
        {

            while (dr.Read())
            {
                int item_id = Convert.ToInt32(dr["item_id"]);
                int level = Convert.ToInt32(dr["level"]);
                int char_id = Convert.ToInt32(dr["char_id"]);
                double value = Convert.ToDouble(dr["value"]);

                if (!UnitLevels.ContainsKey(item_id))
                    UnitLevels[item_id] = new Dictionary<int, Dictionary<int, double>>();

                if (!UnitLevels[item_id].ContainsKey(level))
                    UnitLevels[item_id][level] = new Dictionary<int, double>();

                UnitLevels[item_id][level][char_id] = value;
            }
            dr.Close();
        }

        LoadSession();
        status = ManagerStatus.Started;
    }

    public TowerChar GetUnitChar(int char_id)
    {
        return unitChars[char_id];
    }

    public IEnumerable GetAllUnits()
    {
        return units.Values;
    }

    public Dictionary<int, double> GetUnitChars(int unit_id, int level)
    {
        if (!UnitLevels[unit_id].ContainsKey(level))
            return new Dictionary<int, double>();

        return UnitLevels[unit_id][level];
    }

    public Unit GetNewUnit(int unit_id)
    {
        Item item = ItemManager.GetItemByID(unit_id);
        Unit unit = item.GetComponent<Unit>();
        if (!unit)
            return null;

        Unit newUnit = Instantiate(unit);
        newUnit.gameObject.SetActive(true);
        newUnit.SetData(unit);
        newUnit.SetUniqueID(unic_unit_id);
        newUnit.transform.localScale = new Vector3(1, 1, 1);
        units[unic_unit_id] = newUnit;

        newUnit.SetLevel(1);
        newUnit.SetHP(unit.GetMaxHP());
        newUnit.transform.SetParent(unitPool.transform);
        unic_unit_id++;
        return newUnit;
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
