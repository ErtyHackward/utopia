using ProtoBuf;

namespace Utopia.Shared.Entities.Concrete
{
    [ProtoContract]
    public class BasicCollector : ResourcesCollector
    {
        public override ushort ClassId { get { return EntityClassId.BasicCollector; } }

        public BasicCollector()
        {
            Type = EntityType.Gear;
            Name = "Basic Collector Tool";
            IsPlayerCollidable = false;
            IsPickable = true;
        }

    }
}
