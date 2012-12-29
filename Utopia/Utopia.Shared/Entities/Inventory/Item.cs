using System;
using System.ComponentModel;
using System.Drawing.Design;
using ProtoBuf;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Entities.Models;
using Utopia.Shared.Tools;

namespace Utopia.Shared.Entities.Inventory
{
    /// <summary>
    /// Represents any lootable voxelEntity, tool, weapon, armor, collectible. This entity can be put into the inventory
    /// </summary>
    [ProtoContract]
    public abstract class Item : StaticEntity, IItem, IVoxelEntity
    {
        private VoxelModelInstance _modelInstance;

        #region Properties
        /// <summary>
        /// Gets or sets current voxel model name
        /// </summary>
        [Editor(typeof(ModelSelector), typeof(UITypeEditor))]
        [ProtoMember(1)]
        public string ModelName { get; set; }

        /// <summary>
        /// Gets an item description
        /// </summary>
        [ProtoMember(2)]
        public string Description { get; set; }

        /// <summary>
        /// Gets maximum allowed number of items in one stack (set one if item is not stackable)
        /// </summary>
        [ProtoMember(3)]
        public int MaxStackSize { get; set; }

        /// <summary>
        /// Gets or sets voxel model instance
        /// </summary>
        [Browsable(false)]
        public VoxelModelInstance ModelInstance
        {
            get { return _modelInstance; }
            set
            {
                // note:
                // if you want to make possible to change model instance
                // you need to change method OnInstanceChanged and provide
                // previous instance to dispose it properly

                if (_modelInstance != null)
                    throw new InvalidOperationException("Unable to change model instance, you can set it only once");
                
                _modelInstance = value;
                OnInstanceChanged();
            }
        }

        /// <summary>
        /// Gets stack string. Entities with the same stack string will be possible to put together in a single slot
        /// </summary>
        public virtual string StackType
        {
            get { return BluePrintId.ToString(); }            
        }

        /// <summary>
        /// Gets possible slot types where the item can be put to
        /// </summary>
        public EquipmentSlotType AllowedSlots { get; set; }
        
        #endregion

        /// <summary>
        /// Executed when the model instance is changed
        /// Allows to initialize the instance
        /// </summary>
        protected virtual void OnInstanceChanged() { }

        protected Item()
        {
            AllowedSlots = EquipmentSlotType.Hand;
            MaxStackSize = 1;
        }
    }
}
