using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Globalization;
using ProtoBuf;
using S33M3Resources.Structs;
using SharpDX;
using Utopia.Shared.Entities.Concrete;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Entities.Models;
using Utopia.Shared.Interfaces;
using Utopia.Shared.Tools;

namespace Utopia.Shared.Entities.Inventory
{
    /// <summary>
    /// Represents any lootable voxelEntity, tool, weapon, armor, collectible. This entity can be put into the inventory
    /// </summary>
    [ProtoContract]
    public abstract class Item : StaticEntity, ITool, IWorldIntercatingEntity
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

        [Category("Sound")]
        [Description("Sound of item put")]
        [TypeConverter(typeof(SoundSelector))]
        [ProtoMember(4)]
        public string PutSound { get; set; }

        [Category("Sound")]
        [Description("Ambient sound of an item")]
        [TypeConverter(typeof(SoundSelector))]
        [ProtoMember(5)]
        public string AmbientSound { get; set; }

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
            get { return BluePrintId.ToString(CultureInfo.InvariantCulture); }            
        }

        /// <summary>
        /// Gets possible slot types where the item can be put to
        /// </summary>
        public EquipmentSlotType AllowedSlots { get; set; }

        /// <summary>
        /// Indicates if the tool have special use logic (like resource collectors, guns etc)
        /// </summary>
        public virtual bool CanUse {
            get { 
                // generally items can only be put
                return false; 
            }
        }

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

        /// <summary>
        /// Returns new entity position correspoding to the player
        /// </summary>
        /// <param name="owner">An entity wich trying to put the entity</param>
        /// <returns></returns>
        public virtual EntityPosition GetPosition(IDynamicEntity owner)
        {
            EntityPosition position;

            position.Valid       = true;
            position.Position    = new Vector3D(owner.EntityState.PickPoint);
            position.Rotation    = Quaternion.Identity;
            position.Orientation = ItemOrientation.East;

            return position;
        }

        /// <summary>
        /// Sets item position
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="item"></param>
        public virtual void SetPosition(EntityPosition pos, IItem item)
        {
            item.Position = pos.Position;
            item.Rotation = pos.Rotation;
        }

        /// <summary>
        /// Executes put operation
        /// Removes one item from the inventory and puts it into 
        /// the world
        /// </summary>
        /// <param name="owner">entity that runs the operation</param>
        /// <returns></returns>
        public IToolImpact Put(IDynamicEntity owner)
        {
            // by default all items can only be dropped to some position
            var impact = new ToolImpact { Success = false };

            var blockPicked = owner.EntityState.IsBlockPicked;
            var entityPicked = owner.EntityState.IsEntityPicked;

            // allow to put the item only if user picks something
            if (!blockPicked && !entityPicked)
                return impact;

            var pos = GetPosition(owner);

            if (!pos.Valid)
                return impact;

            var cursor = LandscapeManager.GetCursor(new Vector3D(owner.EntityState.PickPoint));

            var entity = (Item)Clone();

            SetPosition(pos, entity);

            // put entity into the world
            cursor.AddEntity(entity, owner.DynamicId);

            // take entity from the inventory
            var charEntity = owner as CharacterEntity;
            if (charEntity != null)
            {
                var slot = charEntity.Inventory.Find(s => s.Item.StackType == entity.StackType);

                if (slot == null)
                {
                    // we have no more items in the inventory, remove from the hand
                    slot = charEntity.Equipment[EquipmentSlotType.Hand];
                    charEntity.Equipment.TakeItem(slot.GridPosition);
                }
                else
                {
                    charEntity.Inventory.TakeItem(slot.GridPosition);
                }
            }

            return impact;
        }

        /// <summary>
        /// Handles item drop to world
        /// Puts an item in the world and removes one from the inventory
        /// </summary>
        /// <param name="owner"></param>
        /// <returns></returns>
        public virtual IToolImpact Use(IDynamicEntity owner)
        {
            return new ToolImpact { Message  = "This operation is not supported! Sorry!" };
        }

        public virtual void Rollback(IToolImpact impact)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets entityFactory, this field is injected
        /// </summary>
        public EntityFactory EntityFactory { get; set; }

        /// <summary>
        /// Gets landscape manager, this field is injected
        /// </summary>
        public ILandscapeManager2D LandscapeManager { get; set; }

        /// <summary>
        /// Defines tool pick behaviour for the blocks
        /// </summary>
        /// <param name="blockId"></param>
        /// <returns></returns>
        public virtual PickType CanPickBlock(byte blockId)
        {
            if (blockId == 0)
                return PickType.Transparent;

            return PickType.Pick;
        }

        /// <summary>
        /// Defines tool pick behaviour for the entities
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public virtual PickType CanPickEntity(IEntity entity)
        {
            return PickType.Pick;
        }
    }
}
