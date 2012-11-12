using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.Entities.Interfaces;

namespace Utopia.Shared.Entities.Concrete
{
    public class OrientedCubePlaceableItem : CubePlaceableItem
    {
        public override ushort ClassId
        {
            get { return EntityClassId.OrientedCubePlaceableItem; }
        }

        public OrientedCubePlaceableItem()
        {
            Type = EntityType.Static;
            Name = "Plant";
            MountPoint = BlockFace.Top;
            IsPlayerCollidable = false;
            IsPickable = true;
        }
    }
}
