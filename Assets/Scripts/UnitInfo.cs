using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitInfo : MonoBehaviour {

    public GridLayoutGroup grid_group;

    public ProtectionChar prot_char;

    private GridLayoutGroup hidden_grid;

    private List<ProtectionChar> hidden_prot_chars;
    private List<ProtectionChar> prot_chars;
    // Use this for initialization
    void Awake () {
        hidden_prot_chars = new List<ProtectionChar>();
        prot_chars = new List<ProtectionChar>();
        hidden_grid = Instantiate(grid_group, grid_group.transform.parent);
        hidden_grid.gameObject.SetActive(false);
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    void DeleteOldResources(List<ProtectionChar> res_to_delete)
    {
        foreach (var res in res_to_delete)
        {
            Destroy(res.gameObject);
        }
        res_to_delete.Clear();
    }

    public void SetOneChar(Dictionary<int, double> towerChars, List<ProtectionChar> list, GridLayoutGroup grid)
    {
        DeleteOldResources(list);

        foreach (var pair in towerChars)
        {
            ProtectionChar newChar = Instantiate(prot_char);
            newChar.SetData(Managers.Units.GetUnitChar(pair.Key), pair.Value);
            newChar.transform.SetParent(grid.transform);
            list.Add(newChar);
        }
    }

    public void SetChars(Dictionary<int, double> towerChars)
    {
        SetOneChar(towerChars, prot_chars, grid_group);
    }

    public void SetHiddenChars(Dictionary<int, double> towerChars)
    {
        SetOneChar(towerChars, hidden_prot_chars, hidden_grid);
    }

    public void ShowUpgrade(bool show)
    {
        if (hidden_prot_chars.Count == 0)
        {
            show = false;
        }
        hidden_grid.gameObject.SetActive(show);
        grid_group.gameObject.SetActive(!show);
    }
}
