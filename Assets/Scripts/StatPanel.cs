using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatPanel : MonoBehaviour {
    public ResourceCell res_cell;
    public HorizontalLayoutGroup group;
    // Use this for initialization

    private Dictionary<int, ResourceCell> res_cells;
    void Start () {
		
	}

    public void SetPanel()
    {
        Dictionary<int, int> res_counts = StorageManager.GetUserResources();
        res_cells = new Dictionary<int, ResourceCell>();
        foreach (var key in res_counts.Keys)
        {
            var count = res_counts[key];
            ResourceCell resource = Instantiate(res_cell);
            resource.gameObject.SetActive(true);
            resource.SetData(StorageManager.GetResource(key), count);
            resource.transform.SetParent(group.transform);
            resource.transform.localScale = new Vector3(1, 1, 1);
            res_cells[key] = resource;
        }
    }

    public void SetTotalIncome(Dictionary<int, double> total_res)
    {
        foreach(var pair in total_res)
        {
            res_cells[pair.Key].SetIncome(pair.Value);
        }
    }

    public void UpdatePanel()
    {
        Dictionary<int, int> res_counts = StorageManager.GetUserResources();

        foreach(var key in res_counts.Keys)
        {
            res_cells[key].UpdateValue(res_counts[key]);
        }
    }

    // Update is called once per frame
    void Update () {
		
	}
}
