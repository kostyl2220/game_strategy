using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.MoveStrategies
{
    public class MoveRectDirFrontStrategy : MoveRectStrategy
    {
        protected new List<Vector3> ExpandPoints(Vector3 direction, Vector3 end_pos)
        {
            Vector3 right = Quaternion.Euler(0, 90f, 0) * direction;
            List<Vector3> NewPos = new List<Vector3>();

            //left and right points
            float MoveX = (2 * RectSize - 1 - StartRectSize) / 2f;
            for (int i = 0; i < RectSize; ++i)
            {
                float MoveZ = i - (StartRectSize - 1) / 2f;

                Vector3 pos = end_pos - direction * MoveZ + right * MoveX;
                NewPos.Add(pos);
                pos = end_pos - direction * MoveZ + right * MoveX;
                NewPos.Add(pos);
            }

            //back positions
            float MoveZ2 = RectSize / 2f;
            int count_of_ells = (RectSize - 1) * 2 - StartRectSize;
            for (int i = 0; i < count_of_ells; ++i)
            {
                float MoveX2 = i - (count_of_ells - 1) / 2f;
                Vector3 pos = end_pos - direction * MoveZ2 - right * MoveX2;
                NewPos.Add(pos);
            }

            return NewPos;
        }

    }
}
