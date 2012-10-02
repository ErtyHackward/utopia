using Utopia.Shared.Entities.Interfaces;

namespace Utopia.Shared.Entities.Concrete.Collectible
{
    /// <summary>
    /// Represents a top block linked item that can be picked and non-player collidable
    /// </summary>
    public class Plant : CubePlaceableItem
    {
        public override ushort ClassId
        {
            get { return EntityClassId.Plant; }
        }
        
        public Plant()
        {
            Type = EntityType.Static;
            Name = "Plant";
            MountPoint = BlockFace.Top;
            IsPlayerCollidable = false;
            IsPickable = true;
        }

    }
}