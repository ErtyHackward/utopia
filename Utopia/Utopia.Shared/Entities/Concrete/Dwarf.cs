using ProtoBuf;
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
            Type = EntityType.Dynamic;
            ModelName = "Dwarf";
            Name = "NPC";
        }

    }
}
