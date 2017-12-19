using Npgsql;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerChar
{
    public String Name{ get; private set; }
    public Material Material { get; private set; }

    public TowerChar(String Name, String mat_path)
    {
        this.Name = Name;
        this.Material = Resources.Load<Material>(mat_path);
    }
} 

public class TowerBullet
{
    public String Name { get; }
    public double Damage { get; }
    public double Radius { get; }
    public Bullet Bullet { get; }
    
    public TowerBullet(String name, String prefab_path, double dam, double rad)
    {
        Name = name;
        Damage = dam;
        Radius = rad;
        Bullet = Resources.Load<Bullet>(prefab_path);
    }
}

public class TowerManager : MonoBehaviour, IGameManager {
    public ManagerStatus status { get; private set; }


    private static Dictionary<int, Dictionary<int, Dictionary<int, double>>> towerLevels;
    private static Dictionary<int, Dictionary<int, List<int>>> towerLevelBullets;
    private static Dictionary<int, TowerBullet> towerBullets;
    public void Shutdowm()
    {
        
    }

    public void Startup()
    {
        towerLevels = new Dictionary<int, Dictionary<int, Dictionary<int, double>>>();

        NpgsqlDataReader dr = Managers.Database.GetQuery("SELECT * FROM get_tower_levels();");

        while (dr.Read())
        {
            int item_id = Convert.ToInt32(dr["item_id"]);
            int level = Convert.ToInt32(dr["level"]);
            int char_id = Convert.ToInt32(dr["char_id"]);
            double value = Convert.ToDouble(dr["value"]);

            if (!towerLevels.ContainsKey(item_id))
                towerLevels[item_id] = new Dictionary<int, Dictionary<int, double>>();

            if (!towerLevels[item_id].ContainsKey(level))
                towerLevels[item_id][level] = new Dictionary<int, double>();

            towerLevels[item_id][level][char_id] = value;
        }

        towerLevelBullets = new Dictionary<int, Dictionary<int, List<int>>>();

        dr = Managers.Database.GetQuery("SELECT * FROM get_tower_bullets();");

        while (dr.Read())
        {
            int item_id = Convert.ToInt32(dr["item_id"]);
            int level = Convert.ToInt32(dr["level"]);
            int bullet_id = Convert.ToInt32(dr["bullet_id"]);
         
            if (!towerLevelBullets.ContainsKey(item_id))
                towerLevelBullets[item_id] = new Dictionary<int, List<int>>();

            if (!towerLevelBullets[item_id].ContainsKey(level))
                towerLevelBullets[item_id][level] = new List<int>();

            towerLevelBullets[item_id][level].Add(bullet_id);
        }

        towerBullets = new Dictionary<int, TowerBullet>();
        dr = Managers.Database.GetQuery("SELECT * FROM bullets;");

        while(dr.Read())
        {
            int id = Convert.ToInt32(dr["id"]);
            String name = dr["name"].ToString();
            String prefab_path = dr["prefab"].ToString();
            double damage = Convert.ToDouble(dr["damage"]);
            double radius = Convert.ToDouble(dr["radius"]);
            towerBullets[id] = new TowerBullet(name, prefab_path, damage, radius);
        }

        InitTowers();

        status = ManagerStatus.Started;
    }

    public static TowerBullet GetBullet(int bullet_id)
    {
        return towerBullets[bullet_id];
    }

    public static List<int> GetLevelBullets(int item_id, int level)
    {
        if (!towerLevelBullets[item_id].ContainsKey(level))
            return new List<int>();
        return towerLevelBullets[item_id][level];
    }

    public static void CheckOnTower(Item item)
    {
        int item_id = item.GetID();
        if (towerLevels.ContainsKey(item_id))
        {
            ProtectionBuilding tower = item.GetComponent<ProtectionBuilding>();
            tower.InitTower();
        }
    }

    void InitTowers()
    {
        foreach (Item item in ItemManager.GetItems())
        {
            CheckOnTower(item);
        }
    }

    public static Dictionary<int, double> GetLevelChars(int item_id, int level)
    {
        if (!towerLevels[item_id].ContainsKey(level))
            return new Dictionary<int, double>();
        return towerLevels[item_id][level];
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
