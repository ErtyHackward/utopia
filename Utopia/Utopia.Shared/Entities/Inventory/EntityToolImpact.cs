using ProtoBuf;

namespace Utopia.Shared.Entities.Inventory
{
    /// <summary>
    /// Entity tool related impact
    /// </summary>
    [ProtoContract]
    public class EntityToolImpact : ToolImpact
    {
        [ProtoMember(1)]
        public uint EntityId { get; set; }

        public override bool Equals(object obj)
        {
            if (!base.Equals(obj))
                return false;

            var other = (EntityToolImpact)obj;
            return EntityId == other.EntityId;
        }
    }
}