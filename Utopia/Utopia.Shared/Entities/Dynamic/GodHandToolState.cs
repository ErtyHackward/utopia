using ProtoBuf;

namespace Utopia.Shared.Entities.Dynamic
{
    [ProtoContract]
    public class GodHandToolState : ToolState
    {
        /// <summary>
        /// Current slice level (-1 if disabled)
        /// </summary>
        [ProtoMember(1)]
        public int SliceValue;

        /// <summary>
        /// Contains blueprintId of the item to place
        /// 0 means no item
        /// </summary>
        [ProtoMember(2)]
        public ushort DesignationBlueprintId;
    }
}