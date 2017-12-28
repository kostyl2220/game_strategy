using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Npgsql;
using System;
using System.Data;

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

    private static Dictionary<int, int> CountOfItems;

    private static Dictionary<int, Dictionary<int, int>> ItemRequirements;

    private static int LastItemId = 1;
    private static GameObject staticItemPool;

    public void Startup()
    {
        itemStorage = new Dictionary<int, StoredItem>();
        ItemPool = new Dictionary<int, PooledItem>();
        ObjectPool = new Dictionary<int, Item>();
        CountOfItems = new Dictionary<int, int>();
        ItemRequirements = new Dictionary<int, Dictionary<int, int>>();
        using (IDataReader dr = Managers.Database.GetSQLiteQuery("SELECT * FROM Items ORDER BY Name;"))
        {
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

            dr.Close();
        }

        using(IDataReader dr = Managers.Database.GetSQLiteQuery("WITH RECURSIVE Rec (root, child, count) AS (SELECT owner_id, req_id, count FROM requirements UNION ALL SELECT Rec.root, r.req_id, r.count FROM Rec INNER JOIN requirements AS r ON r.owner_id = Rec.child )SELECT root, child, NeedTo FROM (SELECT distinct root, child, max(Rec.count) AS NeedTo FROM Rec GROUP BY root, child) AS query;"))
        {
            while (dr.Read())
            {
                int root_id = Convert.ToInt32(dr["root"]);
                int child_id = Convert.ToInt32(dr["child"]);
                int needTo = Convert.ToInt32(dr["NeedTo"]);

                if (!ItemRequirements.ContainsKey(root_id))
                    ItemRequirements[root_id] = new Dictionary<int, int>();

                ItemRequirements[root_id][child_id] = needTo;
            }
        }

        staticItemPool = ItemObjectPool;
        status = ManagerStatus.Started;

        LoadSession();
    }
    
    public Dictionary<int, int> GetRequirements(int item_id)
    {
        Dictionary<int, int> dictOfReq = new Dictionary<int, int>();
        if (!ItemRequirements.ContainsKey(item_id))
            return dictOfReq;

        foreach (var pair in ItemRequirements[item_id])
        {
            if (!CountOfItems.ContainsKey(pair.Key))
            {
                dictOfReq[pair.Key] = pair.Value;
                continue;
            }

            if (CountOfItems[pair.Key] < pair.Value)
                dictOfReq[pair.Key] = pair.Value - CountOfItems[pair.Key];
        }

        return dictOfReq;
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
        AddItemCount(item_id, 1);
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

        using (IDataReader dr = Managers.Database.GetSQLiteQuery(String.Format("SElECT Save.item_id, pos_x AS x, pos_z AS z, rotation AS rot, Save.level, Save.hp FROM Save WHERE session_id = {0};"
            , Managers.Session.GetSession())))
        {
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

                AddItemCount(id, 1);
            }
            dr.Close();
        }

        GameGrid.SetGrid(grid);
    }

    public void Shutdowm()
    {
        Managers.Database.PutSQLiteQuery(String.Format("DELETE FROM Save WHERE session_id = {0};", Managers.Session.GetSession()));
        foreach (PooledItem item in ItemPool.Values)
        {
            Vector2 pos = item.Item.GetPosition();
            Managers.Database.PutSQLiteQuery(String.Format("INSERT INTO Save(item_id, pos_x, pos_z, rotation, level, hp, session_id) VALUES({0}, {1}, {2}, {3}, {4}, {5}, {6})", 
                item.Item.GetID(), pos.x, pos.y, item.Rotation, item.Item.GetLevel(), item.Item.GetHP(), Managers.Session.GetSession()));
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

    public static void AddItemCount(int id, int count)
    {
        if (!CountOfItems.ContainsKey(id))
            CountOfItems[id] = 0;
        CountOfItems[id] += count;
    }
}
