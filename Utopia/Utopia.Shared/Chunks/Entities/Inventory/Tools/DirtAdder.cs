using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utopia.Shared.Chunks.Entities.Inventory.Tools
{
    public class DirtAdder : Tool
    {
        public override int MaxStackSize
        {
            get { return 1; }
        }

        public override EntityClassId ClassId
        {
            get { return EntityClassId.DirtAdder; }
        }
    }
}
