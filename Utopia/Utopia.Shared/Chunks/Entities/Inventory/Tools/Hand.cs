using System;
using Utopia.Shared.Cubes;

namespace Utopia.Shared.Chunks.Entities.Inventory.Tools
{
    /// <summary>
    /// Every normal human-like NPC or player should have one or two
    /// </summary>
    public class Hand : BlockRemover
    {
        public Hand()
        {
            //a hand can remove anything like base class except water
            RemoveableCubeIds.Remove(CubeId.Water);
            RemoveableCubeIds.Remove(CubeId.WaterSource);
        }

        public override bool Use()
        {
            throw new NotImplementedException();
        }

        public override EntityClassId ClassId
        {
            get { return EntityClassId.Hand; }
        }

        public override int MaxStackSize
        {
            get { return 1; }
        }
    }
}
