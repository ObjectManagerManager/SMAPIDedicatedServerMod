using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DedicatedServer.HostAutomatorStages
{
    internal abstract class BehaviorLink
    {
        private BehaviorLink next;
        
        public BehaviorLink(BehaviorLink next = null)
        {
            this.next = next;
        }

        public void SetNext(BehaviorLink next)
        {
            this.next = next;
        }

        public abstract void Process(BehaviorState state);
        protected void processNext(BehaviorState state)
        {
            if (next != null)
            {
                next.Process(state);
            }
        }
    }
}
