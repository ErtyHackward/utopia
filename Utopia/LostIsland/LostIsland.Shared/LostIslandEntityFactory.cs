using Sandbox.Shared.Items;
using Sandbox.Shared.Tools;
using Utopia.Shared.Entities;
using Utopia.Shared.Interfaces;

namespace Sandbox.Shared
{
    public class LostIslandEntityFactory : EntityFactory
    {
        public LostIslandEntityFactory(ILandscapeManager2D landscapeManager)
            : base(landscapeManager)
        {
        }

        protected override Entity CreateCustomEntity(ushort classId)
        {
            switch (classId)
            {
                case LostIslandEntityClassId.Annihilator: return new Annihilator();
                case LostIslandEntityClassId.Shovel: return new Shovel();
                case LostIslandEntityClassId.GoldCoin: return new GoldCoin();
                case LostIslandEntityClassId.Editor: return new Editor();
                case LostIslandEntityClassId.Carver: return new Carver();
                case LostIslandEntityClassId.Torch: return new Torch();
            }

            return null;
        }

    }
}
