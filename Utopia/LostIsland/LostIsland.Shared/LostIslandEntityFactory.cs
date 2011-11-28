using LostIsland.Shared.Items;
using LostIsland.Shared.Tools;
using Utopia.Shared.Entities;
using Utopia.Shared.Entities.Concrete;
using Utopia.Shared.Interfaces;

namespace LostIsland.Shared
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
                case LostIslandEntityClassId.Annihilator: return new Annihilator(LandscapeManager);
                case LostIslandEntityClassId.CubeResource: return new CubeResource(LandscapeManager);
                case LostIslandEntityClassId.Shovel: return new Shovel(LandscapeManager);
                case LostIslandEntityClassId.GoldCoin: return new GoldCoin();
                case LostIslandEntityClassId.Editor: return new Editor();
                case LostIslandEntityClassId.Carver: return new Carver();
            }

            return null;
        }

    }
}
