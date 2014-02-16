using ProtoBuf;
using S33M3Resources.Structs;

namespace Utopia.Shared.Entities.Inventory
{
    /// <summary>
    /// Block tool related impact
    /// </summary>
    [ProtoContract]
    public class BlockToolImpact : ToolImpact
    {
        [ProtoMember(1)]
        public Vector3I Position { get; set; }

        [ProtoMember(2)]
        public byte CubeId { get; set; }
        
        public override bool Equals(object obj)
        {
            if (!base.Equals(obj))
                return false;

            var other = (BlockToolImpact)obj;
            return Position == other.Position && CubeId == other.CubeId;
        }
    }
}