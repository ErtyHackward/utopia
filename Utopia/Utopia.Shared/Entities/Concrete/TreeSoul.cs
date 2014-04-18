using ProtoBuf;
using Utopia.Shared.Structs;

namespace Utopia.Shared.Entities.Concrete
{
    /// <summary>
    /// Invisible entity that holds tree data
    /// </summary>
    [ProtoContract]
    [EditorHide]
    public class TreeSoul : StaticEntity
    {
        /// <summary>
        /// Configuration blueprint index
        /// </summary>
        [ProtoMember(1)]
        public int TreeTypeId { get; set; }

        /// <summary>
        /// Random seed to create the tree
        /// </summary>
        [ProtoMember(2)]
        public int TreeRndSeed { get; set; }

        /// <summary>
        /// In case of huge damage of the trunk the tree will slowly disappear
        /// This time marks the beginning of the process
        /// </summary>
        [ProtoMember(3)]
        public UtopiaTime LastUpdate { get; set; }

        /// <summary>
        /// Indicates if the tree is damaged and should restore itself or die
        /// </summary>
        [ProtoMember(4)]
        public bool IsDamaged { get; set; }

        [ProtoMember(5)]
        public bool IsDying { get; set; }

        [ProtoMember(6)]
        public UtopiaTime LastItemsRegeneration { get; set; }
    }
}
