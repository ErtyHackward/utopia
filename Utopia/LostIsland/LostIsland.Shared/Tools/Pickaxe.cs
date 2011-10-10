using Utopia.Shared.Chunks.Entities.Inventory.Tools;
using Utopia.Shared.Cubes;

namespace LostIsland.Shared.Tools
{
    public class Pickaxe : BlockRemover
    {

        public Pickaxe()
            : base()
        {
            //a pickaxe can remove anything like base class except water
            RemoveableCubeIds.Remove(CubeId.Water);
            RemoveableCubeIds.Remove(CubeId.WaterSource);
        }

        public override ushort ClassId
        {
            get { return LostIslandEntityClassId.Pickaxe; }
        }

        public override int MaxStackSize
        {
            get { return 1; }
        }

        public override Utopia.Shared.Chunks.Entities.Interfaces.IToolImpact Use(bool runOnServer = false)
        {
            throw new System.NotImplementedException();
        }

        public override void Rollback(Utopia.Shared.Chunks.Entities.Interfaces.IToolImpact impact)
        {
            throw new System.NotImplementedException();
        }
    }
}
