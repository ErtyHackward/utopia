using Utopia.Shared.Chunks.Entities.Inventory;
using Utopia.Shared.Structs;

namespace Utopia.Shared.Chunks.Entities
{
    /// <summary>
    /// Represents a player character (it has a toolbar)
    /// </summary>
    public class PlayerCharacter : SpecialCharacterEntity
    {
        public PlayerCharacter()
        {
            Toolbar = new SlotContainer<ToolbarSlot>(new Location2<byte>(10,1));
        }

        public SlotContainer<ToolbarSlot> Toolbar { get; set; }

        public override EntityClassId ClassId
        {
            get { return EntityClassId.PlayerCharacter; }
        }

        public override string DisplayName
        {
            get { return CharacterName; }
        }
    }
}
