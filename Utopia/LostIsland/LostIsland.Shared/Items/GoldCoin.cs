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
        }

        public override int MaxStackSize
        {
            get { return 100000; }
        }

        public override string Description
        {
            get { return "A coin made of gold. Very valuable thing."; }
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
