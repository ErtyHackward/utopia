using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.Chunks.Entities.Inventory.Tools;
using Utopia.Shared.Structs;
using Utopia.Shared.Structs.Landscape;

namespace Utopia.Shared.Chunks.Entities.Inventory
{
    /// <summary>
    /// A Tool is something you can use
    /// </summary>
    public abstract class Tool : Item
    {
        /// <summary>
        /// Tries to use the tool. The tool should decide is it possible to use and return true/false
        /// </summary>
        /// <returns></returns>
        public abstract bool Use();

        /// <summary>
        /// The tool owner, impossible to use the tool without owner
        /// </summary>
        public LivingEntity Parent { get; set; }

        protected Tool ()
        {
            UniqueName = this.GetType().Name;
        }

    }
}
