using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Bullet : MonoBehaviour {

    private float Damage;

	// Use this for initialization
	void Start () {
		
	}

    public void SetDamage(float damage)
    {
        Damage = damage;
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnCollisionEnter(Collision collision)
    {

        Unit unit = collision.gameObject.GetComponent<Unit>();
        if (unit)
        {
            unit.GetDamage(Damage);
            Destroy(gameObject);
            return;
        }

        Item item = collision.gameObject.GetComponent<Item>();
        if (item)
        {
            item.GetDamage(Damage);
            Destroy(gameObject);
        }
    }
}
