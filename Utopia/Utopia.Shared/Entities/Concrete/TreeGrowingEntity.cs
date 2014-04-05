using ProtoBuf;

namespace Utopia.Shared.Entities.Concrete
{
    [ProtoContract]
    public class TreeGrowingEntity : GrowingEntity
    {
        [ProtoMember(1)]
        public ushort TreeTypeId { get; set; }
    }
}