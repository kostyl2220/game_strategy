using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spot {
    private List<SpotPoint> activeSpots;
    private Dictionary<int , Dictionary<int, SpotPoint>> spotsArray;

    public class SpotPoint
    {
        public bool Walkable;
        public Vector2 Position;

        public SpotPoint(Vector2 pos, bool walk)
        {
            Position = pos;
            Walkable = walk;
        }
    }

    public Spot()
    {
        activeSpots = new List<SpotPoint>();
    }

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public List<SpotPoint> GetNeighbours(SpotPoint point, Grid grid)
    {
        List<SpotPoint> Neighbours = new List<SpotPoint>();

        int[,] neighCoords = { { -1, 0 }, { 1, 0 }, { 0, -1 }, { 0, 1 } };

        for (int i = 0; i < 4; ++i)
        {
            Vector2 newPos = new Vector2(point.Position.x + neighCoords[i, 0], point.Position.y + neighCoords[i, 1]);
            if (grid.InRangePoint(newPos.x + 1, newPos.y + 1))
            {
                bool Color = point.Walkable && grid.GetGrid()[(int)newPos.x, (int)newPos.y] == 0;
                Neighbours.Add(new SpotPoint(newPos, Color));
            }
        }

        return Neighbours;   
    }

    public void CheckOnPointsToCover(List<Vector2> pointsToCover, List<Vector2> Covered, List<SpotPoint> points)
    {
        foreach(var pnt in points)
        {
            int index = 0;
            while(index < pointsToCover.Count)
            {
                if (pnt.Position == pointsToCover[index])
                {
                    Covered.Add(pointsToCover[index]);
                    pointsToCover.RemoveAt(index);
                }
                else
                    ++index;
            }
        }
    }

    public List<Vector2> MakeSpot(List<Vector3> EndCells, Vector3 Center, Grid grid)
    {
        spotsArray = new Dictionary<int, Dictionary<int, SpotPoint>>();

        List<Vector2> pointsToCover = new List<Vector2>();
        List<Vector2> CoveredPoints = new List<Vector2>();
        foreach (var Cell in EndCells)
            pointsToCover.Add(grid.GetPointByPosition(Cell));

        AddToSpot(new SpotPoint(grid.GetPointByPosition(Center), true));

        while (pointsToCover.Count != 0 || HasActiveSpot())
        {
            SpotPoint point = GetCurrentActiveSpot();
            List<SpotPoint> Neighbours = GetNeighbours(point, grid);

            CheckOnPointsToCover(pointsToCover, CoveredPoints, Neighbours);
            AddToSpot(Neighbours);
        }
  
        return CoveredPoints;
    }

    bool HasActiveSpot()
    {
        return activeSpots.Count != 0;
    }

    SpotPoint GetCurrentActiveSpot()
    {
        SpotPoint point = activeSpots[0];
        activeSpots.RemoveAt(0);
        return point;
    }

    void AddToSpot(List<SpotPoint> newPoints)
    {
        foreach(var point in newPoints)
        {
            activeSpots.Add(point);
        }
    }

    void AddToSpot(SpotPoint newPoint)
    {
        activeSpots.Add(newPoint);
    }
}
