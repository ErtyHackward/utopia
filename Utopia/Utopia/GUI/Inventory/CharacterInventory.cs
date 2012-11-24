﻿using System;
using System.Drawing;
using Utopia.Entities;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Entities.Inventory;
using S33M3CoreComponents.GUI.Nuclex;
using S33M3Resources.Structs;
using S33M3CoreComponents.Inputs;

namespace Utopia.GUI.Inventory
{
    /// <summary>
    /// Represents the character inventory base with an equipment sheet and inventory grid
    /// Inherit this class and use BuildBodyslot protected function to build inventory grid
    /// </summary>
    public class CharacterInventory : InventoryWindow
    {
        private readonly CharacterEntity _character;
        private readonly IconFactory _iconFactory;

        public CharacterInventory(CharacterEntity character, IconFactory iconFactory, Point windowStartPosition,
                                  Point gridOffset, InputsManager inputManager)
            : base(character.Inventory, iconFactory, windowStartPosition, gridOffset, inputManager)
        {
            _character = character;
            _iconFactory = iconFactory;

        }

        //private void BuildCharacterSheet(SpriteTexture back)
        //{
        //    //BuildBodyslot(characterSheet, EquipmentSlotType.RightHand, 146, 64);
        //    BuildBodyslot(EquipmentSlotType.LeftHand, 2, 64);

        //    //XXX externalize charactersheet slot positions. clientsettings.xml or somewhere else

        //    // TODO charactersheet has to be redone, CharacterEquipment is not ContainedSlot based but uses specialized methods like SetHeadGear

        //    BuildBodyslot(characterSheet, EquipmentSlotType.Head, 74, 2);
        //    BuildBodyslot(characterSheet, EquipmentSlotType.Neck, 82, 46, 16);
        //    BuildBodyslot(characterSheet, EquipmentSlotType.Torso, 74, 71);
        //    BuildBodyslot(characterSheet, EquipmentSlotType.RightHand, 145, CellSize);
        //    BuildBodyslot(characterSheet, EquipmentSlotType.LeftHand, 2, CellSize);
        //    BuildBodyslot(characterSheet, EquipmentSlotType.Legs, 110, 136);
        //    BuildBodyslot(characterSheet, EquipmentSlotType.Feet, 48, 178);
        //    BuildBodyslot(characterSheet, EquipmentSlotType.LeftRing, 5, 101, 16);
        //}

        protected InventoryCell BuildBodyslot(EquipmentSlotType inventorySlot, int x, int y, int size = 32)
        {
            var bodyCell = new InventoryCell(_character.Equipment, _iconFactory, new Vector2I(0, (int) inventorySlot),
                                             _inputManager);
            bodyCell.DrawGroupId = this.DrawGroupId;
            bodyCell.Name = inventorySlot.ToString();
            bodyCell.Bounds = new UniRectangle(x, y, size, size);
            bodyCell.MouseDown += ControlMouseDown;
            bodyCell.MouseEnter += ControlMouseEnter;
            bodyCell.MouseLeave += ControlMouseLeave;
            bodyCell.MouseUp += ControlMouseUp;
            Children.Add(bodyCell);
            return bodyCell;
        }

    }
}
