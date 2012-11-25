using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utopia.Shared.Entities.Concrete
{
    public class BasicCollector : ResourcesCollector
    {
        public override ushort ClassId { get { return EntityClassId.BasicCollector; } }

        public BasicCollector()
        {
            Type = EntityType.Gear;
            Name = "Basic Collector Tool";
            IsPlayerCollidable = false;
            IsPickable = true;
        }

    }
}
