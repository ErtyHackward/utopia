using System;
using System.ComponentModel;
using System.Linq;
using System.Collections.Generic;
using ProtoBuf;
using S33M3Resources.Structs;
using Utopia.Shared.Configuration;
using Utopia.Shared.Entities.Concrete;
using Utopia.Shared.Entities.Events;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Entities.Inventory;
using Utopia.Shared.Entities.Concrete.System;
using Container = Utopia.Shared.Entities.Concrete.Container;
using SharpDX;

namespace Utopia.Shared.Entities.Dynamic
{
    /// <summary>
    /// Provides character base properties. Character entity has an equipment and inventory. It can wear the tool.
    /// </summary>
    [ProtoContract]
    [ProtoInclude(100, typeof(RpgCharacterEntity))]
    [ProtoInclude(101, typeof(Npc))]
    public abstract class CharacterEntity : DynamicEntity, ICharacterEntity, IWorldInteractingEntity, IContainerEntity
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private DynamicEntityHealthState _healthState;
        private DynamicEntityAfflictionState _afflictions;
        private Energy _health;
        private Energy _stamina;
        private Energy _oxygen;

        /// <summary>
        /// Gets character name
        /// </summary>
        [ProtoMember(1)]
        [Browsable(false)]
        public string CharacterName { get; set; }

        /// <summary>
        /// Gets character equipment
        /// </summary>
        [ProtoMember(2)]
        [Browsable(false)]
        public CharacterEquipment Equipment { get; private set; }

        /// <summary>
        /// Gets character inventory
        /// </summary>
        [ProtoMember(3)]
        [Browsable(false)]
        public SlotContainer<ContainedSlot> Inventory { get; private set; }

        /// <summary>
        /// Health energy. When it depletes the character will die.
        /// </summary>
        [Category("Gameplay")]
        [ProtoMember(4)]
        public Energy Health
        {
            get { return _health; }
            set { _health = value; _health.EntityOwnerId= DynamicId; }
        }

        /// <summary>
        /// Allows to performs run and jumps
        /// </summary>
        [Category("Gameplay")]
        [ProtoMember(5)]
        public Energy Stamina
        {
            get { return _stamina; }
            set { _stamina = value; _stamina.EntityOwnerId = DynamicId; }
        }

        /// <summary>
        /// Allow to limit time under the water
        /// </summary>
        [Category("Gameplay")]
        [ProtoMember(6)]
        public Energy Oxygen
        {
            get { return _oxygen; }
            set { _oxygen = value; _oxygen.EntityOwnerId = DynamicId; }
        }

        [Browsable(false)]
        [ProtoMember(7)]
        public DynamicEntityHealthState HealthState
        {
            get { return _healthState; }
            set
            {
                if (_healthState == value) return;
                var eventArg = new EntityHealthStateChangeEventArgs 
                { 
                    DynamicEntity = this, 
                    NewState = value, 
                    PreviousState = _healthState 
                };
                _healthState = value;
                OnHealthStateChanged(eventArg);
            }
        }

        [Browsable(false)]
        [ProtoMember(8)]
        public DynamicEntityAfflictionState Afflictions
        {
            get { return _afflictions; }
            set
            {
                if (_afflictions == value) return;
                var eventArg = new EntityAfflicationStateChangeEventArgs 
                { 
                    DynamicEntity = this, 
                    NewState = value, 
                    PreviousState = _afflictions 
                };
                _afflictions = value;
                OnAfflictionStateChanged(eventArg);
            }
        }

        [Browsable(false)]
        [ProtoMember(9)]
        public SoulStone BindedSoulStone { get; set;}

        /// <summary>
        /// Gets entityFactory, this field is injected
        /// </summary>
        [Browsable(false)]
        public EntityFactory EntityFactory { get; set; }

        /// <summary>
        /// Gets a special tool of the character to use when no tool is set
        /// </summary>
        [Browsable(false)]
        public HandTool HandTool { get; private set; }

        /// <summary>
        /// Indicates if this charater controlled by real human
        /// </summary>
        [Browsable(false)]
        public bool IsRealPlayer { get; set; }

        public event EventHandler InventoryUpdated;
        protected virtual void OnInventoryUpdated()
        {
            if (InventoryUpdated != null) InventoryUpdated(this, EventArgs.Empty);
        }

        public event EventHandler<EntityHealthStateChangeEventArgs> HealthStateChanged;
        protected virtual void OnHealthStateChanged(EntityHealthStateChangeEventArgs e)
        {
            if (HealthStateChanged != null) HealthStateChanged(this, e);
        }

        public event EventHandler<EntityAfflicationStateChangeEventArgs> AfflictionStateChanged;
        protected virtual void OnAfflictionStateChanged(EntityAfflicationStateChangeEventArgs e)
        {
            if (AfflictionStateChanged != null) AfflictionStateChanged(this, e);
        }

        public event EventHandler<EntityHealthChangeEventArgs> HealthChanged;
        protected virtual void OnHealthChanged(EntityHealthChangeEventArgs e)
        {
            if (HealthChanged != null) HealthChanged(this, e);
        }

        protected CharacterEntity()
        {
            Initialize();
            HandTool = new HandTool();
            Health = new Energy();
            Stamina = new Energy();
            Oxygen = new Energy();
        }

        internal override void FactoryInitialize()
        {
            base.FactoryInitialize();

            Health.CurrentValue = Health.MaxValue;
        }

        private void Initialize()
        {
            Equipment = new CharacterEquipment(this);
            Inventory = new SlotContainer<ContainedSlot>(this, new Vector2I(7, 5));

            Equipment.ItemTaken += EquipmentOnItemEvent;
            Equipment.ItemPut += EquipmentOnItemEvent;
            Equipment.ItemExchanged += EquipmentOnItemEvent;

            Inventory.ItemTaken += EquipmentOnItemEvent;
            Inventory.ItemPut += EquipmentOnItemEvent;
            Inventory.ItemExchanged += EquipmentOnItemEvent;

            // we need to have single id scope with two of these containers
            Equipment.JoinIdScope(Inventory);
            Inventory.JoinIdScope(Equipment);
        }

        private void EquipmentOnItemEvent(object sender, EntityContainerEventArgs<ContainedSlot> entityContainerEventArgs)
        {
            OnInventoryUpdated();
        }

        /// <summary>
        /// Returns tool that can be used
        /// </summary>
        /// <param name="staticId">tool static id</param>
        /// <returns>Tool instance or null</returns>
        public IItem FindItemById(uint staticId)
        {
            if (Equipment.RightTool != null && Equipment.RightTool.StaticId == staticId)
                return Equipment.RightTool;

            var slot = Inventory.Find(staticId);

            return slot != null ? slot.Item : null;
        }

        /// <summary>
        /// Performs search in Inventory and Equipment
        /// </summary>
        /// <returns></returns>
        public ContainedSlot FindSlot(Func<ContainedSlot, bool> pred)
        {
            var slot = Equipment.FirstOrDefault(pred);

            if (slot != null)
                return slot;

            slot = Inventory.FirstOrDefault(pred);

            return slot;
        }

        /// <summary>
        /// Enumerates all slots of the character (Inventory and Equipment)
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ContainedSlot> Slots()
        {
            foreach (var slot in Equipment)
            {
                yield return slot;
            }

            foreach (var slot in Inventory)
            {
                yield return slot;
            }
        }

        public bool TakeItems(ushort blueprintId, int count)
        {
            while (count > 0)
            {
                var slot = Inventory.LastOrDefault(s => s.Item.BluePrintId == blueprintId);

                if (slot == null)
                    break;

                var takeItems = Math.Min(slot.ItemsCount, count);

                Inventory.TakeItem(slot.GridPosition, takeItems);

                count -= takeItems;
            }

            while (count > 0)
            {
                var slot = Equipment.LastOrDefault(s => s.Item.BluePrintId == blueprintId);

                if (slot == null)
                    break;

                var takeItems = Math.Min(slot.ItemsCount, count);

                Equipment.TakeItem(slot.GridPosition, takeItems);

                count -= takeItems;
            }

            return count == 0;
        }

        public bool PutItems(IItem item, int count)
        {
            return Inventory.PutItem(item, count);
        }

        /// <summary>
        /// Fires entity use event for the craft operation
        /// </summary>
        /// <param name="recipeIndex"></param>
        public IToolImpact CraftUse(int recipeIndex)
        {
            var args = EntityUseEventArgs.FromState(this);
            args.UseType = UseType.Craft;
            args.RecipeIndex = recipeIndex;
            args.Impact = Craft(recipeIndex);
            OnUse(args);

            return args.Impact;
        }

        /// <summary>
        /// Creates the item from its recipe
        /// </summary>
        /// <param name="recipeIndex"></param>
        /// <returns></returns>
        public IToolImpact Craft(int recipeIndex)
        {
            var impact = new ToolImpact();

            try
            {
                var recipe = EntityFactory.Config.Recipes[recipeIndex];
                
                IContainerEntity container;
                if (recipe.ContainerBlueprintId == 0)
                {
                    container = this;
                }
                else
                {
                    container = (Container)EntityState.PickedEntityLink.ResolveStatic(EntityFactory.LandscapeManager);

                    if (container == null)
                        return impact;
                }

                // check if we have all ingredients
                foreach (var ingredient in recipe.Ingredients)
                {
                    var count = container.Slots().Where(s => s.Item.BluePrintId == ingredient.BlueprintId).Sum(s => s.ItemsCount);
                    if (count < ingredient.Count)
                    {
                        impact.Message = "Not enough ingerdients";
                        return impact;
                    }
                }
                
                foreach (var ingredient in recipe.Ingredients)
                {
                    if (!container.TakeItems(ingredient.BlueprintId, ingredient.Count))
                    {
                        impact.Message = "Can't take items from the inventory";
                        return impact;
                    }
                }
                
                var item = (Item)EntityFactory.CreateFromBluePrint(recipe.ResultBlueprintId);
                
                if (!container.PutItems(item, recipe.ResultCount))
                {
                    impact.Message = "Can't put item to the inventory";
                    return impact;
                }
                
                impact.Success = true;
                return impact;
            }
            catch (Exception x)
            {
                impact.Message = x.Message;
                return impact;
            }
        }

        /// <summary>
        /// Damage handling
        /// </summary>
        /// <param name="change">Use negative value to do the damage, and positive to heal</param>
        /// <param name="sourceEntity">Entity that hits us (or heal)</param>
        /// <returns></returns>
        public IToolImpact HealthImpact(float change, IDynamicEntity sourceEntity = null)
        {
            var impact = new EntityToolImpact();

            if (HealthState == DynamicEntityHealthState.Dead)
            {
                impact.Message = "The player is dead, cannot be subject to health change";
                return impact;
            }

            Health.CurrentValue += change;

            //If change > some Trigger ==> Risk of Afflication change like Stunt !

            if (Health.CurrentValue <= 0 && HealthState != DynamicEntityHealthState.Dead)
            {
                impact = ActivateDead();
            }

            //Raise trigger here : CharacterEntityHealthChange (Health energy + Contact point + Normal vector for the damage)
            //Will be subscribed by client to play Hurt sound, show animation on hit point, ...
            var e = new EntityHealthChangeEventArgs
            {
                Change = change,
                Health = Health,
                ImpactedEntity = this,
                SourceEntity = sourceEntity,
                HealthChangeHitLocation = sourceEntity == null ? default(Vector3) : sourceEntity.EntityState.PickPoint,
                HealthChangeHitLocationNormal = sourceEntity == null ? default(Vector3I) : sourceEntity.EntityState.PickPointNormal
            };
            OnHealthChanged(e);
            impact.Success = true;
            

            return impact;
        }

        private EntityToolImpact ActivateDead()
        {
            HealthState = DynamicEntityHealthState.Dead; //Set Dead state to the player
            DisplacementMode = EntityDisplacementModes.Dead; //Set character displacement mode 
            
            //Create grave logic
            var impact = new EntityToolImpact();

            var graveBp = EntityFactory.Config.GraveBlueprint;

            if (graveBp < 256)
            {
                logger.Warn("Unable to create grave entity");
                return impact;
            }

            // first we will find a place for a grave

            var blockPos = Position.ToCubePosition();
            var cursor = EntityFactory.LandscapeManager.GetCursor(blockPos);
            if (cursor == null)
            {
                impact.Dropped = true;
                return impact;
            }

            cursor.OwnerDynamicId = DynamicId;

            while (cursor.GlobalPosition.Y > 0)
            {
                if (cursor.PeekValue(new Vector3I(0, -1, 0)) != WorldConfiguration.CubeId.Air)
                    break;
                cursor.Move(new Vector3I(0, -1, 0));
            }

            // check if the grave already exists

            var chunk = EntityFactory.LandscapeManager.GetChunkFromBlock(cursor.GlobalPosition);

            Entity grave = null;

            foreach (var staticEntity in chunk.Entities)
            {
                var entity = (Entity)staticEntity;
                if (entity.BluePrintId == graveBp && entity.Position == cursor.GlobalPosition)
                    grave = entity;
            }

            // create new if not

            if (grave == null)
            {
                grave = EntityFactory.CreateFromBluePrint(graveBp);
                grave.Position = cursor.GlobalPosition;
                cursor.AddEntity((IStaticEntity)grave);
            }

            var graveContainer = grave as Container;

            if (graveContainer != null)
            {
                foreach (var containedSlot in Slots())
                {
                    if (!graveContainer.PutItems(containedSlot.Item, containedSlot.ItemsCount))
                    {
                        logger.Warn("Can't put all items to the container, it is too small!");
                        break;
                    }
                }
            }

            // remove all items from the player

            foreach (var containedSlot in Slots().ToList())
            {
                TakeItems(containedSlot.Item.BluePrintId, containedSlot.ItemsCount);
            }

            impact.Success = true;
            return impact;
        }


        public override object Clone()
        {
            var obj = base.Clone();

            var cont = obj as CharacterEntity;

            if (cont != null)
            {
                cont.Initialize();
            }

            return obj;
        }
    }
}
