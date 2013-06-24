using ProtoBuf;
using S33M3Resources.Structs;
using Utopia.Shared.Entities.Inventory;
using Utopia.Shared.Entities.Models;

namespace Utopia.Shared.Entities
{
    /// <summary>
    /// Base class for designations
    /// Contains order information for minions to execute
    /// </summary>
    [ProtoContract]
    [ProtoInclude(100, typeof(PlaceDesignation))]
    [ProtoInclude(101, typeof(DigDesignation))]
    public class Designation
    {
        /// <summary>
        /// DynamicEntityId who is responsible to complete this designation
        /// 0 means no performer is assigned
        /// </summary>
        [ProtoMember(1)]
        public uint Owner { get; set; }
    }

    /// <summary>
    /// Contains information about placing an item somewhere
    /// </summary>
    [ProtoContract]
    public class PlaceDesignation : Designation
    {
        /// <summary>
        /// An id for an entity to be placed
        /// </summary>
        [ProtoMember(1)]
        public ushort BlueprintId { get; set; }

        /// <summary>
        /// Desired entity position 
        /// </summary>
        [ProtoMember(2)]
        public EntityPosition Position { get; set; }

        /// <summary>
        /// Cached instance to display ghosted model
        /// </summary>
        public VoxelModelInstance ModelInstance { get; set; }
    }

    /// <summary>
    /// Tells that the block should be removed
    /// </summary>
    [ProtoContract]
    public class DigDesignation : Designation
    {
        [ProtoMember(1)]
        public Vector3I BlockPosition { get; set; }
    }
}