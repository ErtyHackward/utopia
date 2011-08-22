using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.Landscaping;


namespace Utopia.Shared.Chunks.Entities.Inventory.Tools
{
    //a shvovel is blockRemover restricted to grass & dirt
    public class Shovel : BlockRemover
    {

        public Shovel()
            : base()
        {

            RemoveableCubeIds = new HashSet<byte>();
            RemoveableCubeIds.Add(CubeId.Dirt);
            RemoveableCubeIds.Add(CubeId.Grass);
        }


        public override EntityClassId ClassId
        {
            get { return EntityClassId.Shovel; }
        }

        public override int MaxStackSize
        {
            get { return 1; }
        }
    }
}
