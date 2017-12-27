using Npgsql;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using UnityEngine.UI;

public class StoreCell : MonoBehaviour {

    public Text text;
    public Image image;
    public HorizontalLayoutGroup group;
    public ResourceCell res_cell;
    public BuildingRemainInfo remainInfo;

    public Material red;
    public Material green;

    private Item item;
    private Dictionary<int, int> res_counts;
    private bool validRequirements;
    private bool validResourceAvailability;
    private Dictionary<int, int> requirements;
    // Use this for initialization
    void Awake () {
        //requirements = new Dictionary<int, int>();
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void CheckRequirements()
    {
        using (IDataReader dr = Managers.Database.GetSQLiteQuery(String.Format("WITH RECURSIVE Rec(root, child, count) AS(SELECT owner_id, req_id, count FROM requirements UNION ALL SELECT Rec.root, r.req_id, r.count FROM Rec INNER JOIN requirements AS r ON r.owner_id = Rec.child) SELECT child, NeedTo FROM(SELECT root, child, mx - cnt AS NeedTo FROM(SELECT distinct root, child, session_id, coalesce(coi.count, 0) AS cnt, max(Rec.count)AS mx  FROM Rec LEFT JOIN(SELECT * FROM count_of_items WHERE session_id = {1})AS coi ON coi.item_id = Rec.child GROUP BY root, child, session_id, coi.count) AS query) AS q2 WHERE NeedTo > 0 AND root = {0};"
            , item.GetID(), Managers.Session.GetSession())))
        {
            validRequirements = true;
            requirements = new Dictionary<int, int>();
            while (dr.Read())
            {
                validRequirements = false;
                int item_id = Convert.ToInt32(dr["child"]);
                int count = Convert.ToInt32(dr["NeedTo"]);
                requirements[item_id] = count;
            }
            dr.Close();
        }
    }

    public void CheckResourcesAvailability()
    {
        validResourceAvailability = StorageManager.EnoughResources(res_counts);
    }

    public void CheckValidation()
    {
        CheckRequirements();
        CheckResourcesAvailability();

        Image im = transform.GetComponent<Image>();
        if (validRequirements && validResourceAvailability)
        {
            im.material = green;
        }
        else
        {
            im.material = red;
        }
    }

    public void SetItem(Item item)
    {
        text.text = item.GetName();
        image.material = item.GetMaterial();
        this.item = item;

        res_counts = StoreManager.GetItemPrice(item.GetID());
        CheckValidation();

        foreach (var key in res_counts.Keys)
        {
            var count = res_counts[key];
            ResourceCell resource = Instantiate(res_cell);
            resource.gameObject.SetActive(true);
            resource.SetData(StorageManager.GetResource(key), count);
            resource.transform.SetParent(group.transform);
            resource.transform.localScale = new Vector3(1, 1, 1);
        }
    }

    public void OnClick()
    {
        if (validRequirements && validResourceAvailability)
            transform.parent.GetComponent<Store>().PlaceOnGrid(item);
    }

    public void OnMouseEnter ()
    {
        if (requirements.Count > 0)
        {
            remainInfo.gameObject.transform.SetParent(transform);
            remainInfo.transform.localPosition = new Vector3(0, 0, 0);
            remainInfo.transform.SetParent(transform.parent.parent);
            remainInfo.gameObject.SetActive(true);
            List<String> remain = new List<String>();
            foreach (var req in requirements)
                remain.Add(String.Format("{0} :{1}", ItemManager.GetItemByID(req.Key).Name, req.Value));
            remainInfo.SetText(remain);
        }
    }

    public void OnMouseLeave()
    {
        remainInfo.gameObject.SetActive(false);
    }
}
