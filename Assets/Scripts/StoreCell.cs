using Npgsql;
using System;
using System.Collections;
using System.Collections.Generic;
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
        NpgsqlDataReader dr = Managers.Database.GetQuery(String.Format("SELECT * FROM get_requirements({0}, {1});", item.GetID(), Managers.Session.GetSession()));
        validRequirements = !dr.HasRows;
        requirements = new Dictionary<int, int>();
        while (dr.Read())
        {
            int item_id = Convert.ToInt32(dr["item_id"]);
            int count = Convert.ToInt32(dr["need"]);
            requirements[item_id] = count;
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
            remainInfo.transform.localPosition = new Vector3(0, 0, -10);
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
