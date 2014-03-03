using System.ComponentModel;
using System.Drawing.Design;
using System.Globalization;
using System.Linq;
using ProtoBuf;
using S33M3Resources.Structs;
using SharpDX;
using Utopia.Shared.Configuration;
using Utopia.Shared.Entities.Concrete;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Entities.Models;
using Utopia.Shared.Entities.Sound;
using Utopia.Shared.Interfaces;
using Utopia.Shared.Settings;
using Utopia.Shared.Tools;

namespace Utopia.Shared.Entities.Inventory
{
    /// <summary>
    /// Represents any lootable voxelEntity, tool, weapon, armor, collectible. This entity can be put into the inventory
    /// </summary>
    [ProtoContract]
    [ProtoInclude(100, typeof(BlockItem))]
    [ProtoInclude(101, typeof(BlockLinkedItem))]
    [ProtoInclude(102, typeof(ResourcesCollector))]
    [ProtoInclude(103, typeof(CubeResource))]
    [ProtoInclude(104, typeof(Food))]
    [ProtoInclude(105, typeof(Stuff))]
    [ProtoInclude(106, typeof(GodHandTool))]
    [ProtoInclude(107, typeof(Extractor))]
    public abstract class Item : StaticEntity, IItem, IWorldInteractingEntity
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
        [TypeConverter(typeof(ShortSoundSelector))]
        [ProtoMember(6)]
        public StaticEntitySoundSource PutSound { get; set; }

        [Category("Sound")]
        [Description("EmittedSound sound of an item")]
        [TypeConverter(typeof(FullSoundSelector))]
        [ProtoMember(7)]
        public StaticEntitySoundSource EmittedSound { get; set; }

        /// <summary>
        /// Gets or sets voxel model instance
        /// </summary>
        [Browsable(false)]
        public VoxelModelInstance ModelInstance
        {
            get { return _modelInstance; }
            set
            {
                var previous = _modelInstance;
                _modelInstance = value;
                OnInstanceChanged(previous);
            }
        }

        /// <summary>
        /// Gets stack string. Entities with the same stack string will be possible to put together in a single slot
        /// </summary>
        [Browsable(false)]
        public virtual string StackType
        {
            get { return BluePrintId.ToString(CultureInfo.InvariantCulture); }            
        }

        /// <summary>
        /// Gets possible slot types where the item can be put to
        /// </summary>
        public EquipmentSlotType AllowedSlots { get; set; }

        // -------------------------------------------------------------------------

        /// <summary>
        /// Gets entityFactory, this field is injected
        /// </summary>
        [Browsable(false)]
        public EntityFactory EntityFactory { get; set; }

        [Browsable(false)]
        public ILandscapeManager LandscapeManager
        {
            get { return EntityFactory.LandscapeManager; }
        }

        [Browsable(false)]
        public IDynamicEntityManager DynamicEntityManager
        {
            get { return EntityFactory.DynamicEntityManager; }
        }

        #endregion

        /// <summary>
        /// Executed when the model instance is changed
        /// Allows to initialize the instance
        /// </summary>
        protected virtual void OnInstanceChanged(VoxelModelInstance previousInstance) { }

        protected Item()
        {
            AllowedSlots = EquipmentSlotType.Hand;
            MaxStackSize = 1;

            EmittedSound = new StaticEntitySoundSource();
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
        /// <param name="owner"></param>
        public virtual void SetPosition(EntityPosition pos, IItem item, IDynamicEntity owner)
        {
            item.Position = pos.Position;
            item.Rotation = pos.Rotation;
        }

        protected bool CanDo(IDynamicEntity owner, out IToolImpact impact)
        {
            impact = null;
            if (owner.IsReadOnly)
            {
                impact = new ToolImpact
                {
                    Message = "You don't have the access to modify the world. Ask admins for access."
                };
                return false;
            }

            return true;
        }

        protected bool CanDoBlockAction(IDynamicEntity owner, out IToolImpact impact)
        {
            if (!owner.EntityState.IsBlockPicked)
            {
                impact = new ToolImpact
                {
                    Message = "Pick a block"
                };
                return false;
            }
            return CanDo(owner, out impact);
        }

        protected bool CanDoEntityAction(IDynamicEntity owner, out IToolImpact impact)
        {
            if (!owner.EntityState.IsEntityPicked)
            {
                impact = new ToolImpact
                {
                    Message = "Pick an entity"
                };
                return false;
            }
            return CanDo(owner, out impact);
        }

        protected bool CanDoBlockOrEntityAction(IDynamicEntity owner, out IToolImpact impact)
        {
            impact = null;
            if (!owner.EntityState.IsBlockPicked && !owner.EntityState.IsEntityPicked)
            {
                impact = new ToolImpact
                {
                    Message = "Pick an entity or a block"
                };
                return false;
            }
            return CanDo(owner, out impact);
        }


        /// <summary>
        /// Executes put operation
        /// Removes one item from the inventory and puts it into 
        /// the world
        /// </summary>
        /// <param name="owner">entity that runs the operation</param>
        /// <returns></returns>
        public virtual IToolImpact Put(IDynamicEntity owner)
        {
            // by default all items can only be dropped to some position
            IToolImpact checkImpact;
            
            if (!CanDoBlockOrEntityAction(owner, out checkImpact))
                return checkImpact;

            var impact = new EntityToolImpact();
            
            var pos = GetPosition(owner);

            if (!pos.Valid)
            {
                impact.Message = "Provided position is invalid";
                return impact;
            }

            var entityBB = new BoundingBox(pos.Position.AsVector3(), pos.Position.AsVector3() + DefaultSize);

            foreach (var dynEntity in EntityFactory.DynamicEntityManager.EnumerateAround(pos.Position.AsVector3()))
            {
                var dynBB = new BoundingBox(dynEntity.Position.AsVector3(), dynEntity.Position.AsVector3() + dynEntity.DefaultSize);
                if (entityBB.Intersects(ref dynBB))
                {
                    impact.Message = "Intersection with dynamic entity is detected";
                    return impact;
                }
            }


            var cursor = EntityFactory.LandscapeManager.GetCursor(new Vector3D(owner.EntityState.PickPoint));

            if (cursor == null)
            {
                impact.Dropped = true;
                return impact;
            }

            var entity = (Item)Clone();

            SetPosition(pos, entity, owner);



            // take entity from the inventory
            var charEntity = owner as CharacterEntity;
            if (charEntity != null)
            {
                var slot = charEntity.Inventory.FirstOrDefault(s => s.Item.StackType == entity.StackType);

                if (slot == null)
                {
                    // we have no more items in the inventory, remove from the hand
                    slot = charEntity.Equipment[EquipmentSlotType.Hand];
                    impact.Success = charEntity.Equipment.TakeItem(slot.GridPosition);
                }
                else
                {
                    impact.Success = charEntity.Inventory.TakeItem(slot.GridPosition);
                }

                if (!impact.Success)
                {
                    impact.Message = "Unable to take an item from the inventory";
                    return impact;
                }

                // put entity into the world
                cursor.AddEntity(entity, owner.DynamicId);
                impact.EntityId = entity.StaticId;
                return impact;
            }
            
            impact.Message = "CharacterEntity owner is expected";
            return impact;
        }

        /// <summary>
        /// Defines tool pick behaviour for the blocks
        /// </summary>
        /// <param name="blockProfile"></param>
        /// <returns></returns>
        public virtual PickType CanPickBlock(BlockProfile blockProfile)
        {
            if (blockProfile.Id == WorldConfiguration.CubeId.Air)
                return PickType.Transparent;

            //Default Block Behaviours here
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
