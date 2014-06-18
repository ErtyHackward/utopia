using System.ComponentModel;
using System.Linq;
using ProtoBuf;
using S33M3CoreComponents.Sound;
using S33M3Resources.Structs;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Entities.Inventory;
using Utopia.Shared.Entities.Sound;
using Utopia.Shared.Tools;

namespace Utopia.Shared.Entities.Concrete
{
    [ProtoContract]
    [Description("Food entity allows to restore player health and still hunger.")]
    public class Food : Item, ITool
    {
        /// <summary>
        /// Amount of energy provide when used
        /// </summary>
        [ProtoMember(1)]
        [Category("Food")]
        [Description("How much health will be restored")]
        public int Calories { get; set; }

        [Category("Sound")]
        [Description("Sound played when entity is used")]
        [TypeConverter(typeof(ShortSoundSelector))]
        [ProtoMember(2)]
        public StaticEntitySoundSource UseSound { get; set; }

        public bool RepeatedActionsAllowed { get; set; }

        public override EntityPosition GetPosition(IDynamicEntity owner)
        {
            var pos = new EntityPosition();

            // allow to put only on top of the entity
            if (owner.EntityState.PickPointNormal.Y != 1)
                return pos;

            pos.Position = new Vector3D(owner.EntityState.PickPoint);
            pos.Valid = true;

            return pos;
        }

        public IToolImpact Use(IDynamicEntity owner)
        {
            var impact = new EntityToolImpact();

            var charEntity = owner as CharacterEntity;
            if (charEntity != null)
            {
                var slot = charEntity.Inventory.FirstOrDefault(s => s.Item.StackType == StackType);

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

                charEntity.Health.CurrentValue += Calories;
                if (SoundEngine != null && UseSound != null)
                {
                    SoundEngine.StartPlay3D(UseSound, owner.Position.AsVector3());
                }
            }

            return impact;
        }
    }
}
