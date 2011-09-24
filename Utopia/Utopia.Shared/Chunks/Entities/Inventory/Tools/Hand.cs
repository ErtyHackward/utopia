using System;

namespace Utopia.Shared.Chunks.Entities.Inventory.Tools
{
    /// <summary>
    /// Test tool that can remove anything
    /// </summary>
    public class Annihilator : BlockRemover
    {
        public override EntityClassId ClassId
        {
            get { return EntityClassId.Annihilator; }
        }

        public override int MaxStackSize
        {
            get { return 1; }
        }
    }
}
