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
    /// A weapon is a specialized Tool for doing damage
    /// </summary>
    public abstract class Weapon : Tool
    {
        public int Speed { get; set; }
        public int Damage { get; set; }
    }
}
