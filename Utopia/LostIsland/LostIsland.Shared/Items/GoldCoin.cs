using Utopia.Shared.Entities.Inventory;

namespace LostIsland.Shared.Items
{
    /// <summary>
    /// Represents a gold coin entity
    /// </summary>
    public sealed class GoldCoin : SpriteItem
    {
        public GoldCoin()
        {
            UniqueName = DisplayName;
            MaxStackSize = 100000;
        }

        

        public override ushort ClassId
        {
            get { return LostIslandEntityClassId.GoldCoin; }
        }

        public override string DisplayName
        {
            get { return "Gold coin"; }
        }
        
    }
}
