using Utopia.Shared.Entities.Inventory;
using Utopia.Shared.Entities;

namespace Realms.Shared.Items
{
    public class Torch : Item
    {
        public Torch()
        {
            UniqueName = DisplayName;
            Type = EntityType.Static;
        }

        public override int MaxStackSize
        {
            get { return int.MaxValue; }
        }

        
        public override string Description
        {
            get { return "Let there be light"; }
        }

        public override ushort ClassId
        {
            get { return RealmsEntityClassId.Torch; }
        }

        public override string DisplayName
        {
            get { return "Torch"; }
        }
    }
}
