using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.MoveStrategies
{
    public interface IWalkStrategy
    {
        void InitStrategy();
        List<Vector3> GetMoveCells(Vector3 direction, Vector3 end_pos, int unitCount, Grid grid, Item item);
        List<Vector3> ExpandPoints(Vector3 direction, Vector3 end_pos, int unitCount, Grid grid, Item item);
        List<Vector2> GetStartPoints(Vector2 Center, List<Spot.UnitPoint> pointsToCover);
    }
}
