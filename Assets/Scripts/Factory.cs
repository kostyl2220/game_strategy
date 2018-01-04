using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Factory : Item {
    private static float WaitTime = 0.5f;

    public FactoryDrop drop;
    public FactoryResource factoryRes;

    private FactoryInfo factory_info;
    private Dictionary<int, int[]> resources;
    private FactoryInfo instance;

    private List<FactoryDrop> drops;
    private Queue<FactoryDrop> queue;

    private List<FactoryDrop> used;
    private float lastReload = -WaitTime;

    private List<FactoryResource> resource_list;

	// Use this for initialization
	void Awake () {
        drops = new List<FactoryDrop>();
        queue = new Queue<FactoryDrop>();
        used = new List<FactoryDrop>();
        resource_list = new List<FactoryResource>();
    }

    void RunFactory()
    {
        StopAllDrops();
        StopResources();
        foreach (var res in resources)
        {
            FactoryResource fac_res = Instantiate(factoryRes, transform);
            fac_res.SetData(res.Key, res.Value[1] / 1000f, res.Value[0], this);
            resource_list.Add(fac_res);
        }
    }



    void StopAllDrops()
    {   
        foreach(var drop in drops)
        {
            drop.gameObject.SetActive(false);
            used.Add(drop);
        }
        queue.Clear();
        drops.Clear();
    }

    public void StopResources()
    {
        foreach (var res in resource_list)
            Destroy(res.gameObject);
        resource_list.Clear();
    }

    public void AddToQueue(FactoryDrop drop)
    {
        queue.Enqueue(drop);
    }

    IEnumerator AddResource(int resource_id)
    {
        int[] resource_count = resources[resource_id];
        while (true)
        {
            yield return new WaitForSeconds(resource_count[1] / 1000.0f);
            StorageManager.AddResource(resource_id, resource_count[0]);

            FactoryDrop drop = GetDrop();
            drop.SetData(resource_id, resource_count[0], this);
            queue.Enqueue(drop);
        }
    }

    public void SetResources(Dictionary<int, int[]> resources)
    {
        this.resources = resources;
        RunFactory();
    }

    public FactoryDrop GetDrop()
    {
        if (used.Count == 0)
            return Instantiate(this.drop, transform.position, this.drop.transform.rotation, transform);
        FactoryDrop dr = used[0];
        dr.transform.position = transform.position;
        dr.transform.rotation = drop.transform.rotation;
        used.RemoveAt(0);
        return dr;
    }

    public void RemoveDrop(FactoryDrop drop)
    {
        drops.Remove(drop);
        drop.gameObject.SetActive(false);
        used.Add(drop);
    }

    // Update is called once per frame
    void Update () {

        if (queue.Count != 0 && lastReload + WaitTime < Time.time)
        {
            FactoryDrop dr = queue.Dequeue();
            dr.gameObject.SetActive(true);
            drops.Add(dr);
            dr.Start(transform.position + new Vector3(0, 2f, 0));
            lastReload = Time.time;
        }
        
    }

    protected override void UpdateAfterLevel()
    {
        SetResources(Managers.Factory.GetFactory(itemId, level));
    }

    public override void SetInfoData(GameObject parent)
    {
        factory_info = parent.GetComponentInChildren<FactoryInfo>(true);
        factory_info.gameObject.SetActive(true);
        SetInfoData();
    }

    public override void SetInfoData()
    {
        factory_info.SetInfo(Managers.Factory.GetFactory(itemId, level));
        factory_info.SetHiddenInfo(Managers.Factory.GetFactory(itemId, level + 1));
    }

    public override void RemoveData()
    {
        factory_info.gameObject.SetActive(false);
    }

    public override void ShowInfoUpgrade(bool show)
    {
        factory_info.ShowUpgrade(show);
    }
}
