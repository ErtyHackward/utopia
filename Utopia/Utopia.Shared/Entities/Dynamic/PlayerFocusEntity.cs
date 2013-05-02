using ProtoBuf;

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
    }
}
