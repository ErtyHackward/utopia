using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.Entities.Interfaces;

namespace Utopia.Shared.Entities.Inventory
{
    public class ToolSlot : ContainedSlot
    {
        public ITool Tool
        {
            get { return (ITool) Item; }
        }
    }
}
