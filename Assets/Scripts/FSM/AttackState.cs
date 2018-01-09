using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.FSM
{
    class AttackState : State
    {
        public AttackState(Unit parent) : base(parent)
        {
        }

        public override bool Perform()
        {
            bool res = Parent.Attack();
            if (!res)
                Parent.SetState(new IdleState(Parent));
            return res;
        }
    }
}
