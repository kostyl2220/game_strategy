using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FactoryInfo : MonoBehaviour {

    public FactoryResourceInfo info;
    public VerticalLayoutGroup group;

    private VerticalLayoutGroup hidden_group;
    private List<FactoryResourceInfo> resources;
    private List<FactoryResourceInfo> HiddenResources;
    // Use this for initialization
    void Awake() {
        resources = new List<FactoryResourceInfo>();
        HiddenResources = new List<FactoryResourceInfo>();
        hidden_group = Instantiate(group, group.transform.parent);
        hidden_group.gameObject.SetActive(false);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void DeleteOldResources(List<FactoryResourceInfo> res_to_delete)
    {
        foreach (var res in res_to_delete)
        {
            Destroy(res.gameObject);
        }
        res_to_delete.Clear();
    }
    
    public void SetHiddenInfo(Dictionary<int, int[]> resources)
    {
        DeleteOldResources(this.HiddenResources);
        if (resources == null)
            return;

        foreach (var pair in resources)
        {
            FactoryResourceInfo res = Instantiate(info, hidden_group.transform);
            res.transform.localScale = new Vector3(1, 1, 1);
            res.SetData(StorageManager.GetResource(pair.Key).Image, pair.Value[0], pair.Value[1]);
            this.HiddenResources.Add(res);
        }
    }

    public void SetInfo(Dictionary<int, int[]> resources)
    {
        DeleteOldResources(this.resources);
        foreach (var pair in resources) {
            FactoryResourceInfo res = Instantiate(info, group.transform);
            res.transform.localScale = new Vector3(1, 1, 1);
            res.SetData(StorageManager.GetResource(pair.Key).Image, pair.Value[0], pair.Value[1]);
            this.resources.Add(res);
        }
    }

    public void ShowUpgrade(bool show)
    {
        if (HiddenResources.Count == 0)
        {
            show = false;
        }
        hidden_group.gameObject.SetActive(show);
        group.gameObject.SetActive(!show);
    }
}
