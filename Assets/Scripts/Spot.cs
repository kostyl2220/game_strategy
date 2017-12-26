using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spot {
    private List<SpotPoint> activeSpots;
    private Dictionary<int , Dictionary<int, SpotPoint>> spotsArray;

    private List<UnitPoint> pointsToCover;
    private List<UnitPoint> CoveredPoints;

    private int RectSize;
    private int StartRectSize;
    private Grid grid;

    private bool TryReverse;

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

    public List<UnitPoint> MakeSpot(Vector3 direction, Vector3 Center, Grid grid, int unitCount)
    {
        this.grid = grid;
        TryReverse = false;
        List<Vector3> EndCells = GetMoveCells(direction, Center, unitCount);
        spotsArray.Clear();
        activeSpots.Clear();

        pointsToCover.Clear();
        CoveredPoints.Clear();
        foreach (var Cell in EndCells)
        {
            AddToCover(new UnitPoint(grid, Cell));
        }

        Vector2 startPos = grid.GetPointByPosition(Center);
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
                Debug.Log("Not covered");
                if (!TryReverse)
                {
                    Debug.Log("Try reverse");
                    List<Vector3> ReversedCells = GetReversedMoveCells(direction, Center, unitCount);
                    foreach (var cell in ReversedCells)
                        AddToCover(new UnitPoint(grid, cell));

                    CheckNewPointsToCover(unitCount);
                    TryReverse = true;
                }
                else
                {
                    RectSize += 1;
                    Debug.Log(String.Format("Size of rect {0}", RectSize));
                    ExpandPoints(direction, Center, grid);
                    CheckNewPointsToCover(unitCount);
                }
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

    void ExpandPoints(Vector3 direction, Vector3 end_pos, Grid grid)
    {
        Vector3 right = Quaternion.Euler(0, 90f, 0) * direction;

        /*for (int i = 0; i < RectSize; ++i)
        {
            float MoveZ = i - (RectSize - 1) / 2.0f;

            for (int j = 0; j < RectSize; ++j)
            {
                if (i > 0 && i < RectSize - 1 &&
                    j > 0 && j < RectSize - 1)
                    continue;

                float MoveX = j - (RectSize - 1) / 2f;

                Vector3 pos = end_pos - direction * MoveZ - right * MoveX;
                AddToCover(new UnitPoint(grid, pos));
            }
        }*/

        //left and right points
         float MoveX = (2 * RectSize - 1 - StartRectSize) / 2f;
         for (int i = 0; i < RectSize; ++i)
         {
             float MoveZ = i - (StartRectSize - 1) / 2f;

             Vector3 pos = end_pos - direction * MoveZ + right * MoveX;
             AddToCover(new UnitPoint(grid, pos));
             Vector3 pos2 = end_pos - direction * MoveZ + right * MoveX;
             AddToCover(new UnitPoint(grid, pos2));
         }

         //back positions
         float MoveZ2 = RectSize / 2f;
         int count_of_ells = (RectSize - 1) * 2 - StartRectSize;
         for (int i = 0; i < count_of_ells; ++i)
         {
             float MoveX2 = i - (count_of_ells - 1) / 2f;
             Vector3 pos = end_pos - direction * MoveZ2 - right * MoveX2;
             AddToCover(new UnitPoint(grid, pos));
         }
    }

    public List<Vector3> GetMoveCells(Vector3 direction, Vector3 end_pos, int unitCount)
    {
        List<Vector3> PosList = new List<Vector3>();
        Vector3 right = Quaternion.Euler(0, 90f, 0) * direction;
        StartRectSize = (int)Mathf.Ceil(Mathf.Sqrt(unitCount));
        RectSize = StartRectSize;
        for (int i = 0; i < RectSize; ++i)
        {
            float MoveZ = i - (RectSize - 1) / 2.0f;

            for (int j = 0; j < RectSize; ++j)
            {
                if (unitCount == 0)
                    return PosList;

                float MoveX = j - (RectSize - 1) / 2f;

                Vector3 pos = end_pos - direction * MoveZ - right * MoveX;
                PosList.Add(pos);
                unitCount--;
            }
        }
        return PosList;
    }

    public List<Vector3> GetReversedMoveCells(Vector3 direction, Vector3 end_pos, int unitCount)
    {
        unitCount = StartRectSize * StartRectSize - unitCount;
        List<Vector3> PosList = new List<Vector3>();
        Vector3 right = Quaternion.Euler(0, 90f, 0) * direction;
        for (int i = 0; i < StartRectSize; ++i)
        {
            float MoveZ = i - (StartRectSize - 1) / 2.0f;

            for (int j = 0; j < StartRectSize; ++j)
            {
                if (unitCount == 0)
                    return PosList;

                float MoveX = j - (StartRectSize - 1) / 2f;

                Vector3 pos = end_pos + direction * MoveZ + right * MoveX;
                PosList.Add(pos);
                unitCount--;
            }
        }
        return PosList;
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
