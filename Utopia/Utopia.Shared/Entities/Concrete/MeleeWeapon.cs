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
        [Category("Gameplay")]
        [Description("Weapon damage")]
        [ProtoMember(1)]
        public int Damage { get; set; }

        [Category("Gameplay")]
        public bool RepeatedActionsAllowed { get; set; }

        public IToolImpact Use(IDynamicEntity owner)
        {
            var impact = new EntityToolImpact();

            if (!owner.EntityState.IsEntityPicked)
            {
                impact.Success = true;
                return impact;
            }

            var entity = owner.EntityState.PickedEntityLink.Resolve(EntityFactory) as CharacterEntity;

            if (entity == null)
            {
                impact.Success = true;
                return impact;
            }

            var imp = entity.HealthImpact(-Damage, owner);
            imp.Success = true;

            return imp;
        }
    }
}
