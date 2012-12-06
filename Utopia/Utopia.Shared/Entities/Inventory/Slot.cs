using System;
using ProtoBuf;
using Utopia.Shared.Entities.Interfaces;

namespace Utopia.Shared.Entities.Inventory
{
    /// <summary>
    /// Each slot has an entity and number of entities count.
    /// </summary>
    [ProtoContract]
    public class Slot : ICloneable 
    {
        /// <summary>
        /// Gets or sets items count
        /// </summary>
        [ProtoMember(1)]
        public int ItemsCount { get; set; }

        /// <summary>
        /// Gets or sets entity
        /// </summary>
        [ProtoMember(2)]
        public IItem Item { get; set; }

        /// <summary>
        /// Indicates if slot is empty
        /// </summary>
        public bool IsEmpty { get { return Item == null || ItemsCount == 0; } }

        public virtual object Clone()
        {
            var slot = new Slot
                {
                    Item = Item,
                    ItemsCount = ItemsCount
                };
            return slot;
        }
    }
}