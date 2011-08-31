using Utopia.Shared.Chunks.Entities.Events;
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

        public void LeftToolUse()
        {
            if (Equipment.LeftTool != null)
            {
                var args = EntityUseEventArgs.FromState(EntityState);
                args.Tool = Equipment.LeftTool;
                OnUse(args);
            }
        }

        public void RightToolUse()
        {
            if (Equipment.RightTool != null)
            {
                var args = EntityUseEventArgs.FromState(EntityState);
                args.Tool = Equipment.RightTool;
                OnUse(args);
            }
        }

        public void EntityUse()
        {
            if (EntityState.PickedEntityId != 0)
            {
                var args = EntityUseEventArgs.FromState(EntityState);
                OnUse(args);
            }
        }

        public override void AddArea(Management.MapArea area)
        {
            
        }

        public override void RemoveArea(Management.MapArea area)
        {
            
        }

        public override void Update(System.DateTime gameTime)
        {
            // no need to do something here
        }
    }
}
