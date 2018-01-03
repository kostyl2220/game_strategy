using Assets.Scripts.MoveStrategies;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spot {
    private List<SpotPoint> activeSpots;
    private Dictionary<int , Dictionary<int, SpotPoint>> spotsArray;

    private List<UnitPoint> pointsToCover;
    private List<UnitPoint> CoveredPoints;

    private Grid grid;
    private IWalkStrategy Strategy; 

    public class UnitPoint
    {
        public Vector3 RealPos;
        public int X;
        public int Z;

        public UnitPoint(Grid grid, Vector3 position)
        {
            RealPos = position;
            Vector2 pos = grid.GetPointByPosition(position);
            X = (int)pos.x;
            Z = (int)pos.y;
        }
    }

    public class SpotPoint
    {
        public bool Walkable;
        public int X;
        public int Z;

        public SpotPoint(Vector2 pos, bool walk)
        {
            X = (int)pos.x;
            Z = (int)pos.y;
            Walkable = walk;
        }
        
        public bool Compare(UnitPoint cmp)
        {
            return (int)cmp.X == X && (int)cmp.Z == Z;
        }
    }

    public Spot()
    {
        activeSpots = new List<SpotPoint>();
        spotsArray = new Dictionary<int, Dictionary<int, SpotPoint>>();
        pointsToCover = new List<UnitPoint>();
        CoveredPoints = new List<UnitPoint>();
        Strategy = new AttackCircleStratery();
    }

    public void SetStrategy(IWalkStrategy strategy)
    {
        Strategy = strategy;
    }

    public List<SpotPoint> GetNeighbours(SpotPoint point, Grid grid)
    {
        List<SpotPoint> Neighbours = new List<SpotPoint>();

        int[,] neighCoords = { { -1, 0 }, { 1, 0 }, { 0, -1 }, { 0, 1 } };

        for (int i = 0; i < 4; ++i)
        {
            Vector2 newPos = new Vector2(point.X + neighCoords[i, 0], point.Z + neighCoords[i, 1]);
            if (grid.InRangePoint(newPos.x + 1, newPos.y + 1))
            {
                bool Color = point.Walkable && grid.GetGrid()[(int)newPos.x, (int)newPos.y] == 0;
                Neighbours.Add(new SpotPoint(newPos, Color));
            }
        }

        return Neighbours;   
    }

    public List<UnitPoint> MakeSpot(Vector3 direction, Vector3 Center, Grid grid, int unitCount, int SizeX = 1, int SizeZ = 1)
    {
        this.grid = grid;
        Strategy.InitStrategy();
        List<Vector3> EndCells = Strategy.GetMoveCells(direction, Center, unitCount, grid, SizeX, SizeZ);
        spotsArray.Clear();
        activeSpots.Clear();

        pointsToCover.Clear();
        CoveredPoints.Clear();
        foreach (var Cell in EndCells)
        {
            AddToCover(new UnitPoint(grid, Cell));
        }

        Vector2 centerPos = grid.GetPointByPosition(Center);
        foreach (var startPos in Strategy.GetStartPoints(centerPos, pointsToCover))
            AddToSpot(new SpotPoint(startPos, grid.GetGrid()[(int)startPos.x, (int)startPos.y] == 0));

        while (CoveredPoints.Count < unitCount)
        {
            while (pointsToCover.Count != 0 && CoveredPoints.Count < unitCount && HasActiveSpot())
            {
                SpotPoint point = GetCurrentActiveSpot();
                List<SpotPoint> Neighbours = GetNeighbours(point, grid);
                AddToSpot(Neighbours, unitCount);
            }

            if (CoveredPoints.Count < unitCount)
            {
                List<Vector3> NewCells = Strategy.ExpandPoints(direction, Center, unitCount, grid, SizeX, SizeZ);
                foreach (var cell in NewCells)
                    AddToCover(new UnitPoint(grid, cell));

                CheckNewPointsToCover(unitCount);
            }

            if (!HasActiveSpot())
            {
                /*foreach (var line in spotsArray.Values)
                {
                    foreach (SpotPoint point in line.Values)
                    {
                        grid.PlaceSpot(point.X, point.Z, point.Walkable);
                    }
                } */
                return new List<UnitPoint>();
            }
        }

        /*foreach (var line in spotsArray.Values)
        {
            foreach (SpotPoint point in line.Values)
            {
                grid.PlaceSpot(point.X, point.Z, point.Walkable);
            }
        }*/
  
        return CoveredPoints;
    }

    void CheckNewPointsToCover(int unitCount)
    {
        int index = 0;
        while (index < pointsToCover.Count)
        {
            if (CoveredPoints.Count >= unitCount)
                return;

            UnitPoint point = pointsToCover[index];

            if (!spotsArray.ContainsKey(point.X))
            {
                ++index;
                continue;
            }

            if (!spotsArray[point.X].ContainsKey(point.Z))
            {
                ++index;
                continue;
            }

            if (spotsArray[point.X][point.Z].Walkable)
            {
                CoveredPoints.Add(point);
                pointsToCover.RemoveAt(index);
            }
            else
                pointsToCover.RemoveAt(index);
        }
    }

    bool HasActiveSpot()
    {
        return activeSpots.Count != 0;
    }

    void AddToCover(UnitPoint point)
    {
        if (grid.InRangePoint(point.X + 1, point.Z + 1))
            pointsToCover.Add(point);
    }

    SpotPoint GetCurrentActiveSpot()
    {
        SpotPoint point = activeSpots[0];
        activeSpots.RemoveAt(0);
        return point;
    }

    void AddToSpot(SpotPoint point)
    {
        if (TryAddToSpot(point))
        {
            int index = 0;
            while (index < pointsToCover.Count)
            {
                if (point.Compare(pointsToCover[index]))
                {
                    if (point.Walkable)
                        CoveredPoints.Add(pointsToCover[index]);
                    pointsToCover.RemoveAt(index);
                }
                else index++;
            }
        }
    }

    void AddToSpot(List<SpotPoint> newPoints, int unitCount)
    {
        foreach(var point in newPoints)
        {
            if (CoveredPoints.Count >= unitCount)
                return;
            AddToSpot(point);
        }
    }

    bool TryAddToSpot(SpotPoint newPoint)
    {
        if (!spotsArray.ContainsKey(newPoint.X))
            spotsArray[newPoint.X] = new Dictionary<int, SpotPoint>();

        if (!spotsArray[newPoint.X].ContainsKey(newPoint.Z))
        {
            spotsArray[newPoint.X][newPoint.Z] = newPoint;
            activeSpots.Add(newPoint);
            return true;
        }
        else
        {
            if (newPoint.Walkable && !spotsArray[newPoint.X][newPoint.Z].Walkable)
            {
                spotsArray[newPoint.X][newPoint.Z] = newPoint;
                activeSpots.Add(newPoint);
                return true;
            }
        }
        return false;
    }
}
