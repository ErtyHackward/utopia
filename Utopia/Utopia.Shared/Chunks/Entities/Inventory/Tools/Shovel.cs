using System.Collections.Generic;
using Utopia.Shared.Cubes;


namespace Utopia.Shared.Chunks.Entities.Inventory.Tools
{
    //a shovel is blockRemover restricted to grass & dirt
    public class Shovel : BlockRemover
    {
        public Shovel()
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

        public override string DisplayName
        {
            get
            {
                return "Shovel";
            }
        }
    }
}
