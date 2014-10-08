using S33M3Resources.Structs;
using SharpDX;
using System;
using Utopia.Shared.Entities.Concrete.System;
using Utopia.Shared.Entities.Events;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Entities.Inventory;

namespace Utopia.Shared.Entities.Dynamic
{
    /// <summary>
    /// Provides character base properties. Character entity has an equipment and inventory. It can wear the tool. It has health, stamina etc
    /// </summary>
    public interface ICharacterEntity : IDynamicEntity
    {
        /// <summary>
        /// Gets character equipment
        /// </summary>
        CharacterEquipment Equipment { get; }

        /// <summary>
        /// Gets character inventory
        /// </summary>
        SlotContainer<ContainedSlot> Inventory { get; }

        /// <summary>
        /// Gets character name
        /// </summary>
        string CharacterName { get; set; }

        /// <summary>
        /// Represent the Health of the entity
        /// </summary>
        Energy Health { get; set; }

        /// <summary>
        /// Represent the stamina of the entity
        /// </summary>
        Energy Stamina { get; set; }

        /// <summary>
        /// Represent the oxygen blood saturation of the entity
        /// </summary>
        Energy Oxygen { get; set; }

        /// <summary>
        /// Health status of the entity
        /// </summary>
        DynamicEntityHealthState HealthState { get; set; }

        /// <summary>
        /// One or more Afflication currently being applied to the entity
        /// </summary>
        DynamicEntityAfflictionState Afflictions { get; set; }

        /// <summary>
        /// Occurs when the entity health state change
        /// </summary>
        event EventHandler<EntityHealthStateChangeEventArgs> HealthStateChanged;

        /// <summary>
        /// Occurs when the entity Afflication state change
        /// </summary>
        event EventHandler<EntityAfflicationStateChangeEventArgs> AfflictionStateChanged;

        /// <summary>
        /// Occurs when the entity has its health being modified
        /// </summary>
        event EventHandler<EntityHealthChangeEventArgs> HealthChanged;

        /// <summary>
        /// The binded player soulstone
        /// </summary>
        SoulStone BindedSoulStone { get; set; }

        /// <summary>
        /// Damage handling
        /// </summary>
        /// <param name="change">Use negative value to do the damage, and positive to heal</param>
        /// <param name="sourceEntity">Entity that hits us (or heal)</param>
        /// <returns></returns>
        IToolImpact HealthImpact(float change, IDynamicEntity sourceEntity = null);
    }
}