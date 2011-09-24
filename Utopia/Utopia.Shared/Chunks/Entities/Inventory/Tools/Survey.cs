using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.Structs;
using Utopia.Shared.Structs.Landscape;

namespace Utopia.Shared.Chunks.Entities.Inventory.Tools
{

    //Survey gets you the number of blocks of selected cubeid under the selection cube. 
    public class Survey : Tool
    {        
        public override EntityClassId ClassId
        {
            get { return EntityClassId.Survey; }
        }

        public override int MaxStackSize
        {
            get { return 1; }
        }
    }
}
