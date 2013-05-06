using System.Collections.Generic;
using ProtoBuf;
using S33M3Resources.Structs;
using Utopia.Shared.Structs;

namespace Utopia.Shared.Entities.Dynamic
{
    /// <summary>
    /// Invisible entity to link god-mode camera to
    /// This entity is always located on top of the surface of somewhere inside the surface (leveling mode)
    /// HeadRotation holds 3rd person mode camera rotation
    /// BodyRotation contains rotation in horisontal plane only
    /// </summary>
    [ProtoContract]
    public class GodEntity : DynamicEntity
    {
        public override ushort ClassId
        {
            get { return EntityClassId.FocusEntity; }
        }

        [ProtoMember(1)]
        public override SharpDX.Quaternion HeadRotation
        {
            get
            {
                return base.HeadRotation;
            }
            set
            {
                base.HeadRotation = value;
            }
        }

        /// <summary>
        /// Gets or sets position to fly to
        /// </summary>
        public Vector3D FinalPosition { get; set; }

        public override Vector3D Position
        {
            get
            {
                return base.Position;
            }
            set
            {
                base.Position = value;
                FinalPosition = value;
            }
        }

        /// <summary>
        /// Gets list of selected entities by the god-entity
        /// </summary>
        public List<EntityLink> SelectedEntities { get; private set; }

        /// <summary>
        /// Gets list of selected blocks by the player
        /// </summary>
        public List<Vector3I> SelectedBlocks { get; private set; }

        public GodEntity()
        {
            SelectedEntities = new List<EntityLink>();
            SelectedBlocks = new List<Vector3I>();
        }
    }
}
