using System;
using System.Drawing;
using Utopia.Entities;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Entities.Inventory;
using S33M3CoreComponents.Sprites;
using S33M3CoreComponents.GUI.Nuclex;
using S33M3CoreComponents.GUI.Nuclex.Controls;
using S33M3Resources.Structs;
using S33M3CoreComponents.Inputs;

namespace Utopia.GUI.Inventory
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
        public event EventHandler<InventoryWindowEventArgs> EquipmentSlotClicked;

        private void OnEquipmentSlotClicked(InventoryWindowEventArgs e)
        {
            var handler = EquipmentSlotClicked;
            if (handler != null) handler(this, e);
        }

        public PlayerInventory(SpriteTexture back, PlayerCharacter player, IconFactory iconFactory, Point windowStartPosition, InputsManager inputManager)
            : base(player.Inventory, iconFactory, windowStartPosition, new Point(4 + back.Width, 24), inputManager)
        {
            _player = player;
            _iconFactory = iconFactory;
            BuildCharacterSheet(back);
        }

        private void BuildCharacterSheet(SpriteTexture back)
        {
            var characterSheet = new ContainerControl();
            characterSheet.background = back;
            characterSheet.Bounds = new UniRectangle(4, 24, back.Width, back.Height);
            Children.Add(characterSheet);

            //BuildBodyslot(characterSheet, EquipmentSlotType.RightHand, 146, 64);
            BuildBodyslot(characterSheet, EquipmentSlotType.LeftHand, 2, 64);

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
             var bodyCell = new InventoryCell(_player.Equipment, _iconFactory, new Vector2I(0,(int)inventorySlot), _inputManager);
             bodyCell.Name = inventorySlot.ToString();
             bodyCell.Bounds = new UniRectangle(x, y, size, size);
             bodyCell.MouseDown += BodyCellMouseDown;
             bodyCell.MouseEnter += BodyCellMouseEnter;
             bodyCell.MouseLeave += BodyCellMouseLeave;
             parent.Children.Add(bodyCell);
         }

         void BodyCellMouseLeave(object sender, EventArgs e)
         {
             OnCellMouseLeave(new InventoryWindowCellMouseEventArgs { Cell = (InventoryCell)sender });
         }

         void BodyCellMouseEnter(object sender, EventArgs e)
         {
             OnCellMouseEnter(new InventoryWindowCellMouseEventArgs { Cell = (InventoryCell)sender });
         }

         void BodyCellMouseDown(object sender, MouseDownEventArgs e)
         {
             var cell = (InventoryCell)sender;
             OnEquipmentSlotClicked(new InventoryWindowEventArgs { Container = _player.Equipment, SlotPosition = cell.InventoryPosition });
         }
    }
}
