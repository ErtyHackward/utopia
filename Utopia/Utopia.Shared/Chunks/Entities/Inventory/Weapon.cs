using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utopia.Shared.Chunks.Entities.Inventory
{
    /// <summary>
    /// A weapon is a specialized Tool for doing damage
    /// </summary>
    public class Weapon : Tool
    {
        public int Speed { get; set; }
        public int Damage { get; set; }
    }
}
