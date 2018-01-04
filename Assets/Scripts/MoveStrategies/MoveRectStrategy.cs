using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.MoveStrategies
{
    public class MoveRectStrategy : IWalkStrategy
    {
        protected bool TryReverse;
        protected int RectSize;
        protected int StartRectSize;

        public List<Vector3> ExpandPoints(Vector3 direction, Vector3 end_pos, int unitCount, Grid grid, Item item)
        {
            List<Vector3> NewCells = new List<Vector3>();
            if (!TryReverse)
            {
                NewCells = GetReversedMoveCells(direction, end_pos, unitCount);
                TryReverse = true;
            }
            else
            {
                RectSize += 1;
                NewCells = ExpandPoints(direction, end_pos);
            }

            return NewCells;
        }

        public void InitStrategy()
        {
            TryReverse = false;
        }

        public List<Vector3> GetMoveCells(Vector3 direction, Vector3 end_pos, int unitCount, Grid grid, Item item)
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

        protected List<Vector3> GetReversedMoveCells(Vector3 direction, Vector3 end_pos, int unitCount)
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

        protected List<Vector3> ExpandPoints(Vector3 direction, Vector3 end_pos)
        {
            Vector3 right = Quaternion.Euler(0, 90f, 0) * direction;
            List<Vector3> NewPos = new List<Vector3>();
            for (int i = 0; i < RectSize; ++i)
            {
                float MoveZ = i - (RectSize - 1) / 2.0f;

                for (int j = 0; j < RectSize; ++j)
                {
                    if (i > 0 && i < RectSize - 1 &&
                        j > 0 && j < RectSize - 1)
                        continue;

                    float MoveX = j - (RectSize - 1) / 2f;

                    Vector3 pos = end_pos - direction * MoveZ - right * MoveX;
                    NewPos.Add(pos);
                }
            }

            return NewPos;
        }

        public List<Vector2> GetStartPoints(Vector2 Center, List<Spot.UnitPoint> pointsToCover)
        {
            List<Vector2> list = new List<Vector2>();
            list.Add(Center);
            return list;
        }
    }
}
