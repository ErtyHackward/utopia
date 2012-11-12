using System.ComponentModel;
using System.IO;
using Utopia.Shared.Entities.Interfaces;

namespace Utopia.Shared.Entities.Concrete
{
    /// <summary>
    /// Represents a top block linked item that can be picked and non-player collidable
    /// </summary>
    public class Plant : CubePlaceableItem
    {
        [Description("Create a randome Rotation around the Y axis of the item")]
        public bool RndRotationAroundY { get; set; }

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

        public override void Load(BinaryReader reader, EntityFactory factory)
        {
            // first we need to load base information
            base.Load(reader, factory);
            RndRotationAroundY = reader.ReadBoolean();
        }

        public override void Save(BinaryWriter writer)
        {
            // first we need to save base information
            base.Save(writer);
            writer.Write(RndRotationAroundY);
        }

    }
}