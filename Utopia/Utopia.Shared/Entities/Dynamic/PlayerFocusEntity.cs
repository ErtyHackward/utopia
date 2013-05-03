using ProtoBuf;
using S33M3Resources.Structs;

namespace Utopia.Shared.Entities.Dynamic
{
    /// <summary>
    /// Invisible entity to link god-mode camera to
    /// This entity is always located on top of the surface of somewhere inside the surface (leveling mode)
    /// HeadRotation holds 3rd person mode camera rotation
    /// BodyRotation contains rotation in horisontal plane only
    /// </summary>
    [ProtoContract]
    public class PlayerFocusEntity : DynamicEntity
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
    }
}
