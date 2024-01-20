using DedicatedServer.HostAutomatorStages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DedicatedServer.Utils
{
    internal class Invincible : InvincibleWorker
    {
        private Invincible() : base(null)
        {
            
        }

        /// <summary>
        /// <inheritdoc cref = "InvincibleWorker.InvincibilityOverwrite"/>
        /// </summary>
        public static new bool? InvincibilityOverwrite
        {
            get{ return InvincibleWorker.InvincibilityOverwrite; }
            set{ InvincibleWorker.InvincibilityOverwrite = value; }
        }
    }
}
