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

        public bool RepeatedActionsAllowed { get; set; }

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
                impact.Message = "Can't find such entity, desync?";
                return impact;
            }

            return entity.HealthImpact(-Damage, owner);
        }
    }
}
