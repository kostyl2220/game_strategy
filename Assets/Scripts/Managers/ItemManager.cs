using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Npgsql;
using System;

public class ItemManager : MonoBehaviour, IGameManager {
    public ManagerStatus status { get; private set; }
    public GameObject pool;
    public GameObject ItemObjectPool;
    public Grid GameGrid;

    private struct PooledItem
    {
        public int Rotation { get; }
        public Item Item { get; }

        public PooledItem(Item item, int rot)
        {
            this.Item = item;
            this.Rotation = rot;
        }
    }

    private struct StoredItem
    {
        public Item Item { get; }
        public bool InStore { get; }

        public StoredItem(Item it, bool show)
        {
            Item = it;
            InStore = show;
        }
    }

    private static Dictionary<int, StoredItem> itemStorage; // 1 copy of every item

    private static Dictionary<int , PooledItem> ItemPool; // set of items 

    private static Dictionary<int, Item> ObjectPool; //returned unused items

    private static int LastItemId = 1;
    private static GameObject staticItemPool;

    public void Startup()
    {
        itemStorage = new Dictionary<int, StoredItem>();
        ItemPool = new Dictionary<int, PooledItem>();
        ObjectPool = new Dictionary<int, Item>();
        NpgsqlDataReader dr = Managers.Database.GetQuery("SELECT * FROM Items ORDER BY Name;");
        while (dr.Read())
        {
            int item_id = Convert.ToInt32(dr["id"]);
            Item prefab = Instantiate(Resources.Load<Item>(dr["prefab"].ToString()));
            prefab.SetId(item_id);
            prefab.Name = dr["Name"].ToString();
            prefab.SizeX = Convert.ToInt32(dr["size_x"]);
            prefab.SizeZ = Convert.ToInt32(dr["size_z"]);
            prefab.material = Resources.Load<Material>(dr["icon"].ToString());
            prefab.transform.parent = pool.transform;
            prefab.MaxHP = Convert.ToDouble(dr["HP"]);
            prefab.gameObject.SetActive(false);
            itemStorage[item_id] = new StoredItem(prefab, Convert.ToBoolean(dr["inStore"]));
        }

        staticItemPool = ItemObjectPool;
        status = ManagerStatus.Started;

        LoadSession();
    }
    
    int InsertItemToPool(Item item, int rotation)
    {
        ItemPool[LastItemId] = new PooledItem(item, rotation);
        item.transform.SetParent(staticItemPool.transform);
        return LastItemId++;
    }

    public static IEnumerable GetItems()
    {
        foreach (var item in ItemPool) {
            yield return item.Value.Item;
        }
    }

    public static int AddItemToPool(Item item, Vector2 position, int rotation)
    {
        item.GetComponent<Collider>().enabled = true;
        item.SetUniqueID(LastItemId);
        item.SetLevel(1);
        item.SetPosition(position);
        item.SetHP(item.GetMaxHP());
        item.transform.SetParent(staticItemPool.transform);

        ItemPool[LastItemId] = new PooledItem(item, rotation);

        Managers.Factory.CheckOnFactory(item);
        TowerManager.CheckOnTower(item);

        SaveItemToDatabase(item.GetID());
        PayForItem(item.GetID());
        return LastItemId++;
    }

    static void PayForItem(int item_id)
    {
        StorageManager.PayResources(StoreManager.GetItemPrice(item_id));
    }

    static void SaveItemToDatabase(int item_id)
    {
        NpgsqlDataReader dr = Managers.Database.GetQuery(String.Format("SELECT add_item_count({0}, {1}, {2});", item_id, 1, Managers.Session.GetSession()));
        dr.Close();
    }

    public static IEnumerable GetAllItems()
    {
        foreach (var storedItem in itemStorage.Values)
        {
            yield return storedItem.Item;
        }
    }

    public IEnumerable GetStoreItems()
    {
        foreach (var storedItem in itemStorage.Values)
        {
            if (storedItem.InStore)
                yield return storedItem.Item;
        }
    }
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void LoadSession()
    {
        int[,] grid = new int[GameGrid.CountInX, GameGrid.CountInZ];

        NpgsqlDataReader dr = Managers.Database.GetQuery(String.Format("SELECT * FROM load_session({0})", Managers.Session.GetSession()));
        while (dr.Read())
        {
            int id = Convert.ToInt32(dr["item_id"]);
            int posX = Convert.ToInt32(dr["x"]);
            int posZ = Convert.ToInt32(dr["z"]);
            int rot = Convert.ToInt32(dr["rot"]);
            int level = Convert.ToInt32(dr["level"]);
            double hp = Convert.ToDouble(dr["hp"]);

            Item item = GetItemFromPool(id, new Vector3());
            item.SetLevel(level);
            item.SetHP(hp);

            item.transform.position = GameGrid.GetPositionByXZ(posX + (item.SizeX - 1) / 2.0f, posZ + (item.SizeZ - 1) / 2.0f, 0);
            item.SetPosition(new Vector2(posX, posZ));

            item.transform.rotation = Quaternion.Euler(0, rot, 0) * item.transform.rotation;
            int new_id = InsertItemToPool(item, rot);
            item.SetUniqueID(new_id);

            posX--; posZ--;
            for (int i = posX; i < posX + item.SizeX; ++i)
            {
                for (int j = posZ; j < posZ + item.SizeZ; ++j)
                {
                    grid[i, j] = new_id;
                }
            }
            
        }

        GameGrid.SetGrid(grid);
    }

    public void Shutdowm()
    {
        NpgsqlDataReader dr = Managers.Database.GetQuery(String.Format("SELECT delete_session({0})", Managers.Session.GetSession()));
        dr.Close();
        foreach (PooledItem item in ItemPool.Values)
        {
            Vector2 pos = item.Item.GetPosition();
            dr = Managers.Database.GetQuery(String.Format("INSERT INTO Save(item_id, pos_x, pos_z, rotation, level, hp, session_id) VALUES({0}, {1}, {2}, {3}, {4}, {5}, {6})", 
                item.Item.GetID(), pos.x, pos.y, item.Rotation, item.Item.GetLevel(), item.Item.GetHP(), Managers.Session.GetSession()));
            dr.Close();
        }
    }

    public static Item GetItemByID(int item_id)
    {
        return itemStorage[item_id].Item;
    }

    //Item pool

    public static Item GetItemFromPool(int item_id, Vector3 Position)
    {
        if (ObjectPool.ContainsKey(item_id))
        {
            Item obj = ObjectPool[item_id];
            obj.transform.position = Position;
            obj.transform.rotation = staticItemPool.transform.rotation;
            obj.transform.parent = staticItemPool.transform;
            obj.gameObject.SetActive(true);
            ObjectPool.Remove(item_id);
            return obj;
        }
        Item item = GetItemByID(item_id);
        Item instance = Instantiate(item, Position, staticItemPool.transform.rotation, staticItemPool.transform);
        instance.gameObject.SetActive(true);
        instance.transform.localScale = new Vector3(1, 1, 1);
        instance.SetData(item);
        return instance;
    }

    public static void ReturnToPool(Item item)
    {
        item.gameObject.SetActive(false);
        ObjectPool[item.GetID()] = item;
    }
}
