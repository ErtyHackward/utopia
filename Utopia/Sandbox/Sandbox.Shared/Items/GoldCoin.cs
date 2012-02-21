using Utopia.Shared.Entities.Inventory;
using Utopia.Shared.Entities;

namespace Sandbox.Shared.Items
{
    /// <summary>
    /// Represents a gold coin entity
    /// </summary>
    public sealed class GoldCoin : SpriteItem
    {
        public GoldCoin()
        {
            UniqueName = DisplayName;
            Format = SpriteFormat.Billboard;
            Type = EntityType.Static;
            Size = new SharpDX.Vector3(0.3f, 0.3f, 0.3f);
        }

        public override int MaxStackSize
        {
            get { return int.MaxValue; }
        }

        public override string Description
        {
            get { return "A coin made of gold. Very valuable thing."; }
        }

        public override ushort ClassId
        {
            get { return SandboxEntityClassId.GoldCoin; }
        }

        public override string DisplayName
        {
            get { return "Gold coin"; }
        }
    }
}
