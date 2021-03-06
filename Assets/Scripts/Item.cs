﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour {

    public Material material;
    public string Name = "Unknown";
    public int SizeX = 1;
    public int SizeZ = 1;
    public int rotation;
    public double MaxHP;

    public String PlayerName = "Player";
    public int itemId;
    protected int uniqueId;
    public int level;
    public double HP = 0;
    private bool Dead = false;
    
    protected Vector2 Position;


    //event system
    public delegate void OnDamageReceive(double Hp, double MaxHP, double damage);
    public event OnDamageReceive EventOnDamage;
    //remove level and rotation to item
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public String GetPlayerName()
    {
        return PlayerName;
    }

    public bool IsFriend(Item item)
    {
        return item.PlayerName == PlayerName;
    }

    public bool IsFriend(String Name)
    {
        return Name == PlayerName;
    }

    public string GetName()
    {
        return Name;
    }

    public bool GetDamage(float damage)
    {
        HP -= damage;
        TriggerOnDamage(damage);
        if (HP <= 0)
        {
            //Dead
            //Remove from items
            if (!Dead)
            {
                RemoveFromManager();
                Dead = true;
            }
            return false;
        }
        return true;
    }

    protected virtual void RemoveFromManager()
    {
        Managers.Items.RemoveItem(this);
    }

    public void SetData(Item item)
    {
        Name = item.Name;
        SizeX = item.SizeX;
        SizeZ = item.SizeZ;
        itemId = item.GetID();
        MaxHP = item.GetMaxHP();
    }

    public void SetPlayerName(String name)
    {
        PlayerName = name;
    }

    public void SetHP(double HP)
    {
        this.HP = HP;
        Dead = HP <= 0;
        TriggerOnDamage(0);
    }

    void TriggerOnDamage(double damage)
    {
        if (EventOnDamage != null)
            EventOnDamage(HP, MaxHP, damage);
    }

    public double GetHP()
    {
        return HP;
    }

    public double GetMaxHP()
    {
        return MaxHP;
    }

    public Material GetMaterial()
    {
        return material;
    }

    public void SetId(int ID)
    {
        itemId = ID;
    }

    public void SetLevel(int level)
    {
        this.level = level;
    }

    public int GetLevel()
    {
        return level;
    }

    public void SetUniqueID(int id)
    {
        uniqueId = id;
    }

    public int GetUniqueID()
    {
        return uniqueId;
    }

    public int GetID()
    {
        return itemId;
    }

    public void LevelUp()
    {
        level++;
        UpdateAfterLevel();
    }

    protected virtual void UpdateAfterLevel()
    {

    }

    public virtual void SetInfoData(GameObject parent)
    {

    }

    public virtual void SetInfoData()
    {

    }

    public virtual void RemoveData()
    {

    }

    public virtual void HideInfo()
    {

    }

    public virtual void ShowInfoUpgrade(bool show)
    {

    }

    public void SetPosition(Vector2 position)
    {
        Position = position;
    }

    public virtual Vector2 GetPosition()
    {
        return Position;
    }

    public bool IsAlive()
    {
        return HP > 0;
    }
} 
