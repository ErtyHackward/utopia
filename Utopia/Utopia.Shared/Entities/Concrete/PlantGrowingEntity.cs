using ProtoBuf;

namespace Utopia.Shared.Entities.Concrete
{
    [ProtoContract]
    public class PlantGrowingEntity : GrowingEntity
    {
        [ProtoMember(1)]
        public ushort GeneratedBlueprint { get; set; }
    }
}