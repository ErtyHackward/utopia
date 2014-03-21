using System.ComponentModel;
using ProtoBuf;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Entities.Inventory;

namespace Utopia.Shared.Entities.Concrete
{
    [Description]
    [ProtoContract]
    public class MeleeWeapon : Item, ITool
    {
        [Description("Weapon damage")]
        [ProtoMember(1)]
        public int Damage { get; set; }

        public MeleeWeapon()
        {
            RepeatedActionsAllowed = true;
            IsPickable = true;
        }

        public IToolImpact Use(IDynamicEntity owner)
        {
            var impact = new EntityToolImpact();

            if (!owner.EntityState.IsEntityPicked)
            {
                return impact;
            }

            var entity = owner.EntityState.PickedEntityLink.Resolve(EntityFactory) as CharacterEntity;

            if (entity == null)
            {
                return impact;
            }

            return entity.HealthImpact(-Damage);
        }

        public bool RepeatedActionsAllowed { get; set; }
    }
}
