using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.FSM
{
    class MoveState : State
    {
        public MoveState(Unit parent) : base(parent)
        {
        }

        public override bool Perform()
        {
            bool res = Parent.Move(); 
            if (!res) {
                Item item = Parent.GetTarget();
                if (item)
                    Parent.SetEndDirection(item.transform.position - Parent.transform.position);
                Parent.SetState(new RotateState(Parent));
            }
            return res;
        }
    }
}
