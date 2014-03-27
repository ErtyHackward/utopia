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
    [Description("Use this class for the plants.")]
    public class Plant : BlockLinkedItem, IRndYRotation
    {
        [Category("Gameplay")]
        [Description("Create a randome Rotation around the Y axis of the item")]
        [ProtoMember(1)]
        public bool RndRotationAroundY { get; set; }

        public Plant()
        {
            Name = "Plant";
            MountPoint = BlockFace.Top;
        }
    }
}