using LostIsland.Shared.Items;
using LostIsland.Shared.Tools;
using Utopia.Shared.Entities;
using Utopia.Shared.Entities.Concrete;
using Utopia.Shared.Interfaces;

namespace LostIsland.Shared
{
    public class LostIslandEntityFactory : EntityFactory
    {
        private readonly ILandscapeManager2D _landscapeManager;

        public LostIslandEntityFactory(ILandscapeManager2D landscapeManager)
        {
            _landscapeManager = landscapeManager;
        }

        protected override Entity CreateCustomEntity(ushort classId)
        {
            switch (classId)
            {
                case LostIslandEntityClassId.Annihilator: return new Annihilator(_landscapeManager);
                case LostIslandEntityClassId.CubeResource: return new CubeResource(_landscapeManager);
                case LostIslandEntityClassId.Shovel: return new Shovel(_landscapeManager);
                case LostIslandEntityClassId.GoldCoin: return new GoldCoin();
                case LostIslandEntityClassId.Editor: return new Editor();
                case LostIslandEntityClassId.Carver: return new Carver();
            }

            return null;
        }

    }
}
