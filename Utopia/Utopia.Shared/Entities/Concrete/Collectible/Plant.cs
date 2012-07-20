using System.IO;
using S33M3Resources.Structs;
using Utopia.Shared.Entities.Interfaces;

namespace Utopia.Shared.Entities.Concrete.Collectible
{
    /// <summary>
    /// Represents a top block linked item that can be picked and non-player collidable
    /// </summary>
    public abstract class Plant : CubePlaceableItem, IBlockLinkedEntity
    {
        public override bool IsPickable
        {
            get { return true; }
        }

        public override bool IsPlayerCollidable
        { 
            get { return false; }
        }

        public Vector3I LinkedCube { get; set; }

        public BlockFace MountPoint
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

        public override void Load(BinaryReader reader, EntityFactory factory)
        {
            // first we need to load base information
            base.Load(reader, factory);
            LinkedCube = reader.ReadVector3I();
        }

        public override void Save(BinaryWriter writer)
        {
            // first we need to save base information
            base.Save(writer);
            writer.Write(LinkedCube);
        }
    }
}