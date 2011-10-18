using System.Drawing;
using Nuclex.UserInterface;
using S33M3Engines.Shared.Sprites;
using Utopia.Entities;
using Utopia.Shared.Entities.Inventory;

namespace Utopia.GUI.D3D.Inventory
{
    /// <summary>
    /// Represents the player inventory with character equipment sheet
    /// </summary>
    public class PlayerInventory : InventoryWindow
    {
        public PlayerInventory(SpriteTexture back, SlotContainer<ContainedSlot> container, IconFactory iconFactory, Point windowStartPosition) : base(container, iconFactory, windowStartPosition)
        {
            BuildCharacterSheet(back);
        }

        private void BuildCharacterSheet(SpriteTexture back)
        {
            
            var characterSheet = new ContainerControl();
            characterSheet.background = back;
            characterSheet.Bounds = new UniRectangle(-back.Width, 0, back.Width, back.Height);
            Children.Add(characterSheet);
            //XXX externalize charactersheet slot positions. clientsettings.xml or somewhere else

            // TODO charactersheet has to be redone, CharacterEquipment is not ContainedSlot based but uses specialized methods like SetHeadGear

            /*BuildBodyslot(characterSheet, EquipmentSlotType.Head, 74, 2);
              BuildBodyslot(characterSheet, EquipmentSlotType.Neck, 82, 46, 16);
              BuildBodyslot(characterSheet, EquipmentSlotType.Torso, 74, 71);
              BuildBodyslot(characterSheet, EquipmentSlotType.RightHand, 145, CellSize);
              BuildBodyslot(characterSheet, EquipmentSlotType.LeftHand, 2, CellSize);
              BuildBodyslot(characterSheet, EquipmentSlotType.Legs, 110, 136);
              BuildBodyslot(characterSheet, EquipmentSlotType.Feet, 48, 178);
              BuildBodyslot(characterSheet, EquipmentSlotType.LeftRing, 5, 101, 16);*/
        }

        /* private void BuildBodyslot(Control parent, EquipmentSlotType inventorySlot, int x, int y, int size = 32)
         {
             _player.Equipment.Torso
             var bodyCell = new InventoryCell(inventorySlot);
             bodyCell.Name = inventorySlot.ToString();
             bodyCell.Bounds = new UniRectangle(x, y, size, size);
             bodyCell.IsLink = true;
             parent.Children.Add(bodyCell);
         }*/
    }
}
