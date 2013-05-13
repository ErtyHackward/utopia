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
            Type = EntityType.Dynamic;
            ModelName = "Dwarf";
            Name = "NPC";
        }

    }
}
