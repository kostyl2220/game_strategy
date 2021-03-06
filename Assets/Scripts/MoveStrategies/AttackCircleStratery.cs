﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.MoveStrategies
{
    public class AttackCircleStratery : IWalkStrategy
    {
        protected int Radius;

        public List<Vector3> ExpandPoints(Vector3 direction, Vector3 end_pos, int unitCount, Grid grid, Item item)
        {
            Radius++;
            return GetMoveCells(direction, end_pos, unitCount, grid, item);
        }

        public List<Vector3> GetMoveCells(Vector3 direction, Vector3 end_pos, int unitCount, Grid grid, Item item)
        {
            List<Vector3> NewPoints = new List<Vector3>();
            if (item == null)
                return NewPoints;

            Vector2 CenterXZ = grid.GetPointByPosition(end_pos);

            //left & right
            int X = (int)CenterXZ.x;
            int Z = (int)CenterXZ.y;
            float GridSize = grid.Size;

            int X0 = X - Radius;
            int X1 = X + item.SizeX + Radius - 1;

            for (int z = -Radius; z < item.SizeZ + Radius; z++)
            {
                Vector3 leftPos = grid.GetPositionByXZ(X0 + 1, Z + z + 1, 0);
                
                Vector3 rightPos = grid.GetPositionByXZ(X1 + 1, Z + z + 1, 0);

                NewPoints.Add(leftPos + new Vector3(GridSize / 4f, 0, GridSize / 4f));
                NewPoints.Add(leftPos + new Vector3(GridSize / 4f, 0, -GridSize / 4f));

                NewPoints.Add(rightPos + new Vector3(-GridSize / 4f, 0, GridSize / 4f));
                NewPoints.Add(rightPos - new Vector3(GridSize / 4f, 0, GridSize / 4f));
            }

            //up & down
            int Z0 = Z - Radius;
            int Z1 = Z + item.SizeZ + Radius - 1;

            for (int x = -Radius + 1; x < item.SizeX + Radius - 1; x++)
            {
                Vector3 downPos = grid.GetPositionByXZ(X + x + 1, Z0 + 1, 0);

                Vector3 upPos = grid.GetPositionByXZ(X + x + 1, Z1 + 1, 0);

                NewPoints.Add(downPos + new Vector3(-GridSize / 4f, 0, GridSize / 4f));
                NewPoints.Add(downPos + new Vector3(GridSize / 4f, 0, GridSize / 4f));

                NewPoints.Add(upPos - new Vector3(GridSize / 4f, 0, GridSize / 4f));
                NewPoints.Add(upPos + new Vector3(GridSize / 4f, 0, -GridSize / 4f));
            }

            return NewPoints;
        }

        public List<Vector2> GetStartPoints(Vector2 Center, List<Spot.UnitPoint> pointsToCover)
        {
            List<Vector2> list = new List<Vector2>();
            foreach (var elem in pointsToCover)
                list.Add(new Vector2(elem.X, elem.Z));
            return list;
        }

        public void InitStrategy()
        {
            Radius = 1;
        }
    }
}
