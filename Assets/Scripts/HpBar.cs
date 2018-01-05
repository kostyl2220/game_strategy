using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HpBar : MonoBehaviour {
    public RectTransform HPbar;
    public Text HPtext;
    public Item item;

    //HP bar
    private float totalHPbarWidth;

    // Use this for initialization
    void Start () {
        if (HPbar)
            totalHPbarWidth = HPbar.sizeDelta.x;
        gameObject.SetActive(false);
        if (item)
        {
            DrawHP(item.GetHP(), item.GetMaxHP());
            item.EventOnDamage += OnDamageReceive;
            if (item.GetHP() > 0 && item.GetHP() != item.GetMaxHP())
                gameObject.SetActive(true);
        }

    }
	
	// Update is called once per frame
	void Update () {
		
	}
    
    void OnDamageReceive(double Hp, double MaxHp, double damage)
    {
        if (Hp != MaxHp)
        {
            gameObject.SetActive(true);
            DrawHP(Hp, MaxHp);
        }
        else
        {
            gameObject.SetActive(false);
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
}
