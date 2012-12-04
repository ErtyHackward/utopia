using System.ComponentModel;
using ProtoBuf;
using Utopia.Shared.Entities.Concrete.Interface;
using Utopia.Shared.Entities.Interfaces;

namespace Utopia.Shared.Entities.Concrete
{
    /// <summary>
    /// Represents a top block linked item that can be picked and non-player collidable
    /// </summary>
    [ProtoContract]
    public class Plant : BlockLinkedItem, IRndYRotation
    {
        [Description("Create a randome Rotation around the Y axis of the item")]
        [ProtoMember(1)]
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
    }
}