using LostIsland.Shared.Tools;
using Utopia.Shared.Chunks.Entities;
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
                case LostIslandEntityClassId.Shovel: return new Shovel();
                case LostIslandEntityClassId.Pickaxe: return new Pickaxe();
                case LostIslandEntityClassId.Survey: return new Survey();
            }

            return null;
        }

    }
}
