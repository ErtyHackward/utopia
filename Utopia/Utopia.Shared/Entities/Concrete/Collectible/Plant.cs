using System.IO;
using S33M3Resources.Structs;
using Utopia.Shared.Entities.Interfaces;

namespace Utopia.Shared.Entities.Concrete.Collectible
{
    /// <summary>
    /// Represents a top block linked item that can be picked and non-player collidable
    /// </summary>
    public abstract class Plant : CubePlaceableItem
    {
        public override bool IsPickable
        {
            get { return true; }
        }

        public override bool IsPlayerCollidable
        { 
            get { return false; }
        }

        public override BlockFace MountPoint
        {
            get { return BlockFace.Top; }
        }

        public override string StackType
        {
            get { return GetType().Name; }
        }

        protected Plant()
        {
            Type = EntityType.Static;
        }

    }
}