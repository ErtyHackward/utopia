using ProtoBuf;
using SharpDX;
using Utopia.Shared.Entities.Dynamic;

namespace Utopia.Shared.Entities.Concrete
{
    [ProtoContract]
    public class Dwarf : CharacterEntity
    {
        public override ushort ClassId
        {
            get { return EntityClassId.Dwarf; }
        }

        public Dwarf()
        {
            DefaultSize = new Vector3(0.8f, 1.4f, 0.8f);
            ModelName = "Dwarf";
            Name = "NPC";
            MoveSpeed = 2.2f;
        }

    }
}
