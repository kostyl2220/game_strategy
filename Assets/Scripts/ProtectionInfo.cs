using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProtectionInfo : MonoBehaviour {

    public GridLayoutGroup grid_group;
    public ScrollRect scroll;

    public ProtectionChar prot_char;
    public BulletInfo bull_info;

    private GridLayoutGroup hidden_grid;
    private ScrollRect hidden_scroll;

    private List<ProtectionChar> hidden_prot_chars;
    private List<ProtectionChar> prot_chars;
    private List<BulletInfo> bull_infos;
    private List<BulletInfo> hidden_bulls;
    // Use this for initialization
    void Awake () {
        prot_chars = new List<ProtectionChar>();
        hidden_prot_chars = new List<ProtectionChar>();
        bull_infos = new List<BulletInfo>();
        hidden_bulls = new List<BulletInfo>();
        hidden_grid = Instantiate(grid_group, grid_group.transform.parent);
        hidden_grid.gameObject.SetActive(false);
        hidden_scroll = Instantiate(scroll, scroll.transform.parent);
        hidden_scroll.gameObject.SetActive(false);
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

    void DeleteOldBResources(List<BulletInfo> res_to_delete)
    {
        foreach (var res in res_to_delete)
        {
            Destroy(res.gameObject);
        }
        res_to_delete.Clear();
    }



    public void SetOneBullet(List<int> bullets, List<BulletInfo> bul_list, ScrollRect parent)
    {
        DeleteOldBResources(bul_list);
        foreach (var bullet in bullets)
        {
            TowerBullet bull = TowerManager.GetBullet(bullet);
            BulletInfo newBullet = Instantiate(bull_info);
            newBullet.SetData(bull);
            newBullet.transform.SetParent(parent.content.transform);
            bul_list.Add(newBullet);
        }
    }

    public void SetHiddenBullets(List<int> bullets)
    {
        SetOneBullet(bullets, hidden_bulls, hidden_scroll);
    }

    public void SetBullets(List<int> bullets)
    {
        SetOneBullet(bullets, bull_infos, scroll);
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
        hidden_scroll.gameObject.SetActive(show);
        scroll.gameObject.SetActive(!show);
    }
}
