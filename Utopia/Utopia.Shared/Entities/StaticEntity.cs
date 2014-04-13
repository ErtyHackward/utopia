using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using ProtoBuf;
using SharpDX;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Entities.Inventory;
using Utopia.Shared.Interfaces;
using Utopia.Shared.Structs;

namespace Utopia.Shared.Entities
{
    /// <summary>
    /// Provides basic functionality for all static entities
    /// </summary>
    [ProtoContract]
    [ProtoInclude(100, typeof(Item))]
    public abstract class StaticEntity : Entity, IStaticEntity
    {
        /// <summary>
        /// Gets or sets static entity id. This id is unique only in current container. Invalid without Container property set
        /// </summary>
        [Browsable(false)]
        [ProtoMember(1)]
        public uint StaticId { get; set; }
        
        /// <summary>
        /// Gets or sets entity world rotation
        /// </summary>
        [Browsable(false)]
        [ProtoMember(2)]
        public Quaternion Rotation { get; set; }

        [Description("The entity will be destroyed when removed from the world")]
        [Category("Gameplay")]
        [ProtoMember(3)]
        public bool IsDestroyedOnWorldRemove { get; set; }

        private IStaticContainer _container;

        /// <summary>
        /// Gets or sets current parent container
        /// </summary>
        [Browsable(false)]
        public IStaticContainer Container
        {
            get { return _container; }
            set { 
                _container = value; 
            }
        }

        protected StaticEntity()
        {
            Rotation = Quaternion.Identity;
        }

        protected void NotifyParentContainerChange()
        {
            var container = Container;

            if (container is EntityCollection)
            {
                var collection = container as EntityCollection;
                collection.SetDirty();
            }
        }

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
                return new EntityLink(((IAbstractChunk)ec.Chunk).Position, entities.ToArray());
            }

            // root is the dynamicEntity
            if (container is ISlotContainer<ContainedSlot>)
            {
                var sc = container as ISlotContainer<ContainedSlot>;
                return new EntityLink(((IDynamicEntity)sc.Parent).DynamicId, entities.ToArray());
            }
            
            // wrong root
            throw new InvalidOperationException("Unable to take link from that object");
        }

        /// <summary>
        /// Will be called before an entity is removed from world without going into inventory
        /// </summary>
        public virtual void BeforeDestruction(IDynamicEntity destructor)
        {

        }
    }
}
