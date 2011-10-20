using System;
using System.Drawing;
using Nuclex.UserInterface;
using Nuclex.UserInterface.Controls;
using S33M3Engines.Shared.Sprites;
using Utopia.Entities;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Entities.Inventory;

namespace Utopia.GUI.D3D.Inventory
{
    /// <summary>
    /// Represents the player inventory with character equipment sheet
    /// </summary>
    public class PlayerInventory : InventoryWindow
    {
        private readonly PlayerCharacter _player;
        private readonly IconFactory _iconFactory;

        /// <summary>
        /// Occurs when euipment slot is clicked
        /// </summary>
        public event EventHandler<PlayerEquipmentEventArgs> EquipmentSlotClicked;

        private void OnEquipmentSlotClicked(PlayerEquipmentEventArgs e)
        {
            var handler = EquipmentSlotClicked;
            if (handler != null) handler(this, e);
        }

        public PlayerInventory(SpriteTexture back, PlayerCharacter player, IconFactory iconFactory, Point windowStartPosition)
            : base(player.Inventory, iconFactory, windowStartPosition)
        {
            _player = player;
            _iconFactory = iconFactory;
            BuildCharacterSheet(back);
        }

        private void BuildCharacterSheet(SpriteTexture back)
        {
            var characterSheet = new ContainerControl();
            characterSheet.background = back;
            characterSheet.Bounds = new UniRectangle(-back.Width, 0, back.Width, back.Height);
            Children.Add(characterSheet);

            BuildBodyslot(characterSheet, EquipmentSlotType.RightHand, 145, CellSize);
            BuildBodyslot(characterSheet, EquipmentSlotType.LeftHand, 2, CellSize);

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

         private void BuildBodyslot(Control parent, EquipmentSlotType inventorySlot, int x, int y, int size = 32)
         {
             var bodyCell = new InventoryCell(_player, inventorySlot, _iconFactory);
             bodyCell.Name = inventorySlot.ToString();
             bodyCell.Bounds = new UniRectangle(x, y, size, size);
             bodyCell.MouseDown += BodyCellMouseDown;
             parent.Children.Add(bodyCell);
         }

         void BodyCellMouseDown(object sender, MouseDownEventArgs e)
         {
             OnEquipmentSlotClicked(new PlayerEquipmentEventArgs { EquipmentCell = (InventoryCell)sender });
         }
    }

    public class PlayerEquipmentEventArgs : EventArgs
    {
        public InventoryCell EquipmentCell { get; set; }
    }
}
