using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.FSM
{
    class IdleState : State
    {
        public IdleState(Unit parent) : base(parent)
        {
        }

        public override bool Perform()
        {
            return true;
        }
    }
}
