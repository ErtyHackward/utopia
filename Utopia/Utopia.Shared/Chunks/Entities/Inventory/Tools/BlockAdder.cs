
#region Using Statements
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using System.Diagnostics;
using Utopia.Shared.Structs;
using Utopia.Shared.Structs.Landscape;

#endregion

namespace Utopia.Shared.Chunks.Entities.Inventory.Tools
{
    public class BlockAdder : Tool
    {
        public override bool Use()
        {
            throw new NotImplementedException();
        }

        public override EntityClassId ClassId
        {
            get { return EntityClassId.Sword; }
        }

        public override int MaxStackSize
        {
            get { return 1; }
        }
    }
}
