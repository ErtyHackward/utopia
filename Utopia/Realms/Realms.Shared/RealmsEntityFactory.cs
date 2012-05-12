using Realms.Shared.Items;
using Realms.Shared.Tools;
using Utopia.Shared.Entities;
using Utopia.Shared.Interfaces;

namespace Realms.Shared
{
    public class RealmsEntityFactory : EntityFactory
    {
        public RealmsEntityFactory(ILandscapeManager2D landscapeManager)
            : base(landscapeManager)
        {
        }

        protected override Entity CreateCustomEntity(ushort classId)
        {
            switch (classId)
            {
                case RealmsEntityClassId.Annihilator: return new Annihilator();
                case RealmsEntityClassId.Shovel: return new Shovel();
                case RealmsEntityClassId.GoldCoin: return new GoldCoin();
                case RealmsEntityClassId.Editor: return new Editor();
                case RealmsEntityClassId.Carver: return new Carver();
                case RealmsEntityClassId.Torch: return new Torch();
            }

            return null;
        }

    }
}
