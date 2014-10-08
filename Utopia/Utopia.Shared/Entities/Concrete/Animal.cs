using System.ComponentModel;
using System.Drawing.Design;
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
        [Description("Weapon of the animal")]
        [Category("Gameplay")]
        [Editor(typeof(BlueprintTypeEditor<MeleeWeapon>), typeof(UITypeEditor))]
        [TypeConverter(typeof(BlueprintTextHintConverter))]
        [ProtoMember(1)]
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