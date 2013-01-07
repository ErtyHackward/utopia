using ProtoBuf;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Entities.Inventory;

namespace Utopia.Shared.Entities.Dynamic
{
    /// <summary>
    /// Provides character base properties. Character entity has an equipment and inventory. It can wear the tool.
    /// </summary>
    [ProtoContract]
    public abstract class CharacterEntity : DynamicEntity, ICharacterEntity
    {
        /// <summary>
        /// Gets character name
        /// </summary>
        [ProtoMember(1)]
        public string CharacterName { get; set; }

        /// <summary>
        /// Gets character equipment
        /// </summary>
        [ProtoMember(2)]
        public CharacterEquipment Equipment { get; private set; }

        /// <summary>
        /// Gets character inventory
        /// </summary>
        [ProtoMember(3)]
        public SlotContainer<ContainedSlot> Inventory { get; private set; }

        /// <summary>
        /// Gets current health points of the entity
        /// </summary>
        [ProtoMember(4)]
        public int Health { get; set; }

        /// <summary>
        /// Gets maximum health point of the entity
        /// </summary>
        [ProtoMember(5)]
        public int MaxHealth { get; set; }

        /// <summary>
        /// Indicates if this charater controlled by real human
        /// </summary>
        public bool IsRealPlayer { get; set; }
        
        protected CharacterEntity()
        {
            Equipment = new CharacterEquipment(this);
            Inventory = new SlotContainer<ContainedSlot>(this, new S33M3Resources.Structs.Vector2I(7,5));

            // we need to have single id scope with two of these containers
            Equipment.JoinIdScope(Inventory);
            Inventory.JoinIdScope(Equipment);
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
    }
}
