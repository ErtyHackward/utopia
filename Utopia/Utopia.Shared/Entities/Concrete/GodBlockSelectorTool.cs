using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Entities.Inventory;

namespace Utopia.Shared.Entities.Concrete
{
    /// <summary>
    /// God mode tool to select range of blocks
    /// </summary>
    [ProtoContract]
    public class GodBlockSelectorTool : Item, ITool
    {
        public override ushort ClassId
        {
            get { return EntityClassId.GodBlockSelector;  }
        }

        public IToolImpact Use(IDynamicEntity owner)
        {
            throw new NotImplementedException();
        }

        public void Rollback(IToolImpact impact)
        {
            throw new NotImplementedException();
        }
    }
}
