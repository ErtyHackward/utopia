using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.Cubes;


namespace Utopia.Shared.Chunks.Entities.Inventory.Tools
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

        public override EntityClassId ClassId
        {
            get { return EntityClassId.PickAxe; }
        }

        public override int MaxStackSize
        {
            get { return 1; }
        }
    }
}
