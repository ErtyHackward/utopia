using System;
using System.Collections.Generic;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Entities.Inventory;
using Utopia.Shared.Interfaces;
using Utopia.Shared.Structs;

namespace Utopia.Shared.Entities
{
    /// <summary>
    /// Provides basic functionality for all static entities
    /// </summary>
    public abstract class StaticEntity : Entity, IStaticEntity
    {
        /// <summary>
        /// Gets or sets static entity id. This id is unique only in current container. Invalid without Container property set
        /// </summary>
        public uint StaticId { get; set; }

        /// <summary>
        /// Gets or sets current parent container
        /// </summary>
        public IStaticContainer Container { get; set; }

        /// <summary>
        /// Returns link to the entity
        /// </summary>
        /// <returns></returns>
        public override EntityLink GetLink()
        {
            if(Container == null)
                throw new InvalidOperationException("Unable to take link from the entity without parent container");
            
            
            var entities = new List<uint>();
            var container = Container;

            entities.Add(StaticId);

            // collect static entities id chain
            while (container is IStaticEntity)
            {
                entities.Add((container as IStaticEntity).StaticId);
                container = (container as IStaticEntity).Container;
            }

            // reverse the chain to keep correct order
            entities.Reverse();

            // obtain root information

            // root is the chunk
            if (container is EntityCollection)
            {
                var ec = container as EntityCollection;
                return new EntityLink((ec.Chunk as IChunkLayout2D).Position, entities.ToArray());
            }

            // root is the dynamicEntity
            if (container is ISlotContainer<ContainedSlot>)
            {
                var sc = container as ISlotContainer<ContainedSlot>;
                return new EntityLink((sc.Parent as IDynamicEntity).DynamicId, entities.ToArray());
            }
            
            // wrong root
            throw new InvalidOperationException("Unable to take link from that object");
        }
    }
}
