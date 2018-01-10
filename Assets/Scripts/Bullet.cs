using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Bullet : MonoBehaviour {

    private float Damage;
    private String PlayerName;
    private Rigidbody bulletRigid;
    private Vector3 vel;
    // Use this for initialization
    void Start () {
        bulletRigid = transform.GetComponent<Rigidbody>();
    }

    public void SetDamage(float damage)
    {
        Damage = damage;
    }

    public void SetPlayerName(String Name)
    {
        PlayerName = Name;
    }
	
	// Update is called once per frame
	void Update () {
        vel = bulletRigid.velocity;
    }

    void OnCollisionEnter(Collision collision)
    {
        Unit unit = collision.gameObject.GetComponent<Unit>();
        if (unit)
        {
            if (unit.IsFriend(PlayerName))
            {
                Physics.IgnoreCollision(collision.collider, transform.GetComponent<Collider>());
                bulletRigid.velocity = vel;
                return;
            }
            unit.GetDamage(Damage);
            Destroy(gameObject);
            return;
        }

        Item item = collision.gameObject.GetComponent<Item>();
        if (item)
        {
            if (item.IsFriend(PlayerName))
            {
                Physics.IgnoreCollision(collision.collider, transform.GetComponent<Collider>());
                bulletRigid.velocity = vel;
                return;
            }
            item.GetDamage(Damage);
        }

        Destroy(gameObject);
    }
}
