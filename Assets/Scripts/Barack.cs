using Assets.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Barack : Item {

    public BarackInfo barack_info;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    Vector2 FindPlacePoint()
    {
        int[,] grid = Managers.Items.GameGrid.GetGrid();
        for (int l = 1; l < 4; ++l)
        {
            for (int i = 0; i < 2; ++i)
            {
                for (int j = 0; j < SizeX; ++j)
                {
                    int DeltaY = (i == 0 ? -l : SizeZ + l - 1);
          
                    if (grid[(int)Position.x + j - 1, (int)Position.y + DeltaY - 1] == 0)
                    {
                        return Position + new Vector2(j, DeltaY);
                    }
                }

                for (int j = 0; j < SizeZ; ++j)
                {
                    int DeltaX = (i == 0 ? -l : SizeX + l - 1);

                    if (grid[(int)Position.x + DeltaX - 1, (int)Position.y + j - 1] == 0)
                    {
                        return Position + new Vector2(DeltaX, j);
                    }
                }

            }
        }
    
        return new Vector2();
    }

    public void CreateUnit(int unit_id)
    {
        Debug.Log(String.Format("Create unit {0}", unit_id));
        Vector2 PlacePoint = FindPlacePoint();
        Debug.Log(PlacePoint);
       

        Unit unit = Managers.Units.GetNewUnit(unit_id);
        Vector3 endPos = Managers.Items.GameGrid.GetPositionByXZ(PlacePoint.x, PlacePoint.y, 0);
        unit.transform.position = endPos;
        unit.InitUnit();
        //Vector2 startPos = Managers.Items.GameGrid.GetPointByPosition(transform.position);
    
        //List<Node> nodes = MouseSelection.pathfinding.FindPath(startPos, PlacePoint);
        //unit.MoveByPoints(nodes);
    }

    protected override void UpdateAfterLevel()
    {
        
    }

    public override void SetInfoData(GameObject parent)
    {
        barack_info = parent.GetComponentInChildren<BarackInfo>(true);
        barack_info.gameObject.SetActive(true);
        SetInfoData();
    }

    public override void SetInfoData()
    {
        barack_info.SetBarack(this);
        barack_info.SetChars(Managers.Barack.GetBarackUnits(itemId, level));
       //barack_info.SetHiddenInfo(Managers.Factory.GetFactory(itemId, level + 1));
    }

    public override void RemoveData()
    {
        Debug.Log("Remove Barack");
        barack_info.gameObject.SetActive(false);
    }

    public override void ShowInfoUpgrade(bool show)
    {
        //barack_info.ShowUpgrade(show);
    }
}
