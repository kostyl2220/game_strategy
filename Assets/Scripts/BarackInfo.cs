using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class BarackInfo : MonoBehaviour {

    public BarackUnitInfo unitInfo;
    public GridLayoutGroup grid_group;

    private List<BarackUnitInfo> UnitsToCall;
    private Barack Barack;
    // Use this for initialization
    void Awake()
    {
        UnitsToCall = new List<BarackUnitInfo>();
    }

    public void SetBarack(Barack parent)
    {
        Barack = parent;
    }

    void DeleteOldResources(List<BarackUnitInfo> res_to_delete)
    {
        foreach (var res in res_to_delete)
        {
            Destroy(res.gameObject);
        }
        res_to_delete.Clear();
    }

    public void SetOneUnitInfo(List<int> units, List<BarackUnitInfo> list, GridLayoutGroup grid)
    {
        DeleteOldResources(list);

        foreach (var unit_id in units)
        {
            BarackUnitInfo newUnit = Instantiate(unitInfo);
            newUnit.SetInfo(unit_id, this);
            newUnit.transform.SetParent(grid.transform);
            list.Add(newUnit);
        }
    }

    public void SetChars(List<int> units)
    {
        SetOneUnitInfo(units, UnitsToCall, grid_group);
    }

    // Update is called once per frame
    void Update () {
		
	}

    public void CreateUnit(int unit_id)
    {
        Barack.CreateUnit(unit_id);
    }
}
