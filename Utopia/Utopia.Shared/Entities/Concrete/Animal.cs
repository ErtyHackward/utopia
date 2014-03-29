using System.ComponentModel;
using ProtoBuf;
using Utopia.Shared.Entities.Concrete.Interface;
using Utopia.Shared.Entities.Inventory;
using Utopia.Shared.Tools;

namespace Utopia.Shared.Entities.Concrete
{
    [ProtoContract]
    [Description("Animal npc")]
    public class Animal : Npc, ICustomInitialization
    {
        /// <summary>
        /// Gets or sets weapon that animal will use
        /// </summary>
        [ProtoMember(1)]
        [Description("Weapon of the animal")]
        [Category("Gameplay")]
        [TypeConverter(typeof(BlueprintSelector))]
        public ushort WeaponBlueprint { get; set; }
        
        public void Initialize(EntityFactory factory)
        {
            if (WeaponBlueprint != 0)
            {
                ContainedSlot slot;
                var item = factory.CreateFromBluePrint(WeaponBlueprint) as Item;
                if (item != null)
                    Equipment.Equip(EquipmentSlotType.Hand, new ContainedSlot { Item = item, ItemsCount = 1 }, out slot);
            }
        }
    }
}