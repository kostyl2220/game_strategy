using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.FSM
{
    public abstract class State
    {
        protected Unit Parent;

        public abstract bool Perform();

        public State(Unit parent)
        {
            Parent = parent;
        }
    }
}
