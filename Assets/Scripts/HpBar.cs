using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HpBar : MonoBehaviour {
    public RectTransform HPbar;
    public Text HPtext;
    public Item item;

    private Canvas Can;
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
        Can = gameObject.GetComponent<Canvas>();
    }
	
	// Update is called once per frame
	void Update () {
        Vector3 Direction = Camera.main.transform.position - transform.position;
        Direction.x = 0;
        Quaternion rotation = Quaternion.LookRotation(Direction);
        transform.rotation = rotation;
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
