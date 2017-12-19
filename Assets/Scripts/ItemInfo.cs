using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemInfo : MonoBehaviour {
    public Image icon;
    public Text text;
    public float speed = 10f;
    public GameObject contentFilling;
    public RectTransform HPbar;
    public Text HPtext;

    private Vector3 startPos;
    private Vector3 dest;
    private bool opened;
    private Dictionary<int, int> levelResources;
    private Item inner_item;

    //HP bar
    private float totalHPbarWidth;

    // Use this for initialization
    void Start () {
        startPos = transform.position;
        opened = false;
        dest = startPos;
        if (HPbar)
            totalHPbarWidth = HPbar.sizeDelta.x;
    }
	
	// Update is called once per frame
	void Update () {
        if (transform.position != dest)
        {
            transform.position = Vector3.MoveTowards(transform.position, dest, Time.deltaTime * speed);
        }
    }

    public void OnInfoClose()
    {
        opened = false;
        dest = startPos;
        if (inner_item)
            inner_item.HideInfo();
    }

    public void Open()
    {
        opened = true;
        dest = new Vector3(startPos.x, 0, startPos.z);
    }

    public bool SetInfo(Item item)
    {
        if (inner_item != null &&
            item.GetUniqueID() == inner_item.GetUniqueID() && opened == true)
        {
            return false;
        }

        if (inner_item != null)
        {
            inner_item.RemoveData();
        }

        int item_level = item.GetLevel();
        int item_id = item.GetID();
        inner_item = item;

        icon.material = item.material;
        text.text = item.Name;
        DrawHP(item.GetHP(), item.GetMaxHP());

        levelResources = StoreManager.GetItemPrice(item_id, item_level);

        inner_item.SetInfoData(contentFilling);

        return true;
    }

    public void OnItemUpgrade()
    {
        if (levelResources != null)
        {
            if (StorageManager.EnoughResources(levelResources))
            {
                StorageManager.PayResources(levelResources);
                inner_item.LevelUp();
                inner_item.SetInfoData();
                inner_item.ShowInfoUpgrade(true);

                int item_level = inner_item.GetLevel();
                int item_id = inner_item.GetID();
                levelResources = StoreManager.GetItemPrice(item_id, item_level);
            }
        }
    }

    void DrawHP(double HP, double fullHP)
    {
        if (HPbar)
        {
            HPbar.sizeDelta = new Vector2((float)(HP / fullHP * totalHPbarWidth), HPbar.sizeDelta.y);
        }
        if (HPtext)
            HPtext.text = string.Format("{0}/{1}", HP, fullHP);
    }

    public void OnShowUpgrade()
    {
        inner_item.ShowInfoUpgrade(true);
    }

    public void OnHideUpgrade()
    {
        inner_item.ShowInfoUpgrade(false);
    }
}
