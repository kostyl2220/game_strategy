using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateDetails : MonoBehaviour {

    public GameObject tree;
    public GameObject stone;
    public float XFrom = 5.5f;
    public float XTo = 10f;
    public float ZFrom = 5.5f;
    public float ZTo = 10f;
    public float littleTreeForestRadius = 2f;

    private GameObject DecorationSon; 

    // Use this for initialization
    void Start() {
        DecorationSon = transform.Find("Decorations").gameObject;

        //generate Trees and Stones

        RangomCirclePlace();

    }

    struct PlaceChance
    {
        public float Max;
        public float Current;
        public float Cost;
        public float Attenuation;
        public float AngleKoef;
        public int MinCount;
        public int MaxCount;
        public float Radius;
        public GameObject obj;

        public PlaceChance(float max, float cur, float cost, float att, float anglekoef, int minC, int maxC, float rad, GameObject obj)
        {
            this.Max = max;
            this.Current = cur;
            this.Cost = cost;
            this.Attenuation = att;
            this.AngleKoef = anglekoef;
            this.MinCount = minC;
            this.MaxCount = maxC;
            this.Radius = rad;
            this.obj = obj;
        }
    }

    void RangomCirclePlace()
    {
        PlaceChance TreeChance = new PlaceChance(80f, 15f, 8f, 10f, 3f, 6, 8, littleTreeForestRadius, tree);
        PlaceChance StoneChance = new PlaceChance(25f, 0f, 5f, 6f, 2f, 1, 2, 2f, stone);
        PlaceChance[] Chances = { TreeChance, StoneChance };
        float angle = -150;
        while (angle < 150)
        {
            angle += GetAngleToRotate(Chances) * Random.Range(1f, 2f);
            if (angle > 150)
                return;

            int index = GetItemIndexToPlace(Chances);
            AttenuateExcept(Chances, index);

            PlaceChance item = Chances[index];
            int countOfItems = Random.Range(item.MinCount, item.MaxCount);
            Chances[index].Current = Mathf.Min(item.Current + item.Cost * countOfItems, item.Max);
            Vector3 Position = GetPointOnRadius(angle);
            if (!inBounds(Position))
            {
                Debug.Log("Not in bounds!");
                continue;
            }
            CreateItemsInRadius(item.obj, countOfItems, Position, item.Radius);
        }
    }

    void AttenuateExcept(PlaceChance[] Chances, int index)
    {
        for (int i = 0; i < Chances.Length; ++i)
        {
            if (i == index)
                continue;
            Chances[i].Current -= Chances[i].Attenuation;
        }
    }

    int GetItemIndexToPlace(PlaceChance[] Chances)
    {
        int pos = 0;
        for (int i = 1; i < Chances.Length; ++i)
        {
            if (Chances[i].Current / Chances[i].Max < Chances[pos].Current / Chances[pos].Max)
            {
                pos = i;
            }
        }
        return pos;
    }

    Vector3 GetPointOnRadius(float angle)
    {
        angle = angle * Mathf.PI / 180f;
        Vector3 center = transform.position;
        float Xvar = Mathf.Abs(XFrom / Mathf.Sin(angle));
        float Zvar = Mathf.Abs(ZFrom / Mathf.Cos(angle));
        if (Zvar < Xvar)
        {
            float ZvarFar = Mathf.Abs(ZTo / Mathf.Cos(angle));
            float Radius = Random.Range(Zvar, ZvarFar);
            return new Vector3(center.x + Radius * Mathf.Sin(angle), center.y, center.z + Radius * Mathf.Cos(angle));
        }
        else
        {
            float XvarFar = Mathf.Abs(XTo / Mathf.Sin(angle));
            float Radius = Random.Range(Xvar, XvarFar);
            return new Vector3(center.x + Radius * Mathf.Sin(angle), center.y, center.z + Radius * Mathf.Cos(angle));
        }
    }

    float GetAngleToRotate(PlaceChance[] chances)
    {
        float general = 0;
        foreach (PlaceChance chance in chances)
            general += chance.AngleKoef * chance.Current / chance.Max;
        return general;
    }

    Vector3 GetRandomPointInRadius(Vector3 pos, float radius)
    {
        return new Vector3(pos.x + radius * Random.Range(-1f, 1f), 0, pos.z + radius * Random.Range(-1f, 1f));
    }

    Vector3 GetRandomPointInFrame()
    {
        Vector3 center = transform.position;
        return new Vector3(center.x + (Random.Range(0, 2) * 2 - 1) * Random.Range(XFrom, XTo), center.y, center.z + (Random.Range(0, 2) * 2 - 1) * Random.Range(ZFrom, ZTo));
    }

    void CreateItemsInRadius(GameObject item, int count, Vector3 center, float radius)
    {
        for (int i = 0; i < count; ++i)
        {
            Vector3 position = GetRandomPointInRadius(center, radius);
            if (!inBounds(position))
                continue;

            GameObject instance = Instantiate(item, position, transform.rotation);
            if (DecorationSon)
                instance.transform.parent = DecorationSon.transform;
        }
    }

    bool inBounds(Vector3 pos)
    {
        Vector3 center = transform.position;
        float x = pos.x - center.x;
        float z = pos.z - center.z;
        return InBox(x, z, XTo, ZTo) && !InBox(x, z, XFrom, ZFrom);
    }

    bool InBox(float x, float z, float Xsize, float Zsize)
    {
        return Mathf.Abs(x) < Xsize && Mathf.Abs(z) < Zsize;
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
