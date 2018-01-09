using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.FSM
{
    class RotateState : State
    {
        public RotateState(Unit parent) : base(parent)
        {
        }

        public override bool Perform()
        {
            bool res = Parent.Rotate();
            if (!res)
                Parent.SetState(new AttackState(Parent));
            return res;
        }
    }
}
