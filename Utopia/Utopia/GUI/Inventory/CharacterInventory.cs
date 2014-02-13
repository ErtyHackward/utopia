using System;
using System.Drawing;
using System.Linq;
using Utopia.Entities;
using Utopia.Entities.Managers;
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
        private readonly PlayerEntityManager _playerEntityManager;
        private readonly IconFactory _iconFactory;

        public CharacterInventory(PlayerEntityManager playerEntityManager, IconFactory iconFactory, Point windowStartPosition,
                                  Point gridOffset, InputsManager inputManager)
            : base(playerEntityManager.PlayerCharacter.Inventory, iconFactory, windowStartPosition, gridOffset, inputManager)
        {
            _playerEntityManager = playerEntityManager;
            _iconFactory = iconFactory;

            _playerEntityManager.PlayerEntityChanged += _playerEntityManager_PlayerEntityChanged;
        }

        void _playerEntityManager_PlayerEntityChanged(object sender, EventArgs e)
        {
            // rebuild the slots grid
            Content = _playerEntityManager.PlayerCharacter.Inventory;

            foreach (var control in Children.OfType<InventoryCell>().Where(c => c.Container.Parent != _playerEntityManager.PlayerCharacter))
            {
                control.Container = _playerEntityManager.PlayerCharacter.Equipment;
            }
        }

        protected InventoryCell BuildBodyslot(EquipmentSlotType inventorySlot, int x, int y, int size = 32)
        {
            var bodyCell = new InventoryCell(
                _playerEntityManager.PlayerCharacter.Equipment,
                _iconFactory,
                new Vector2I(0, (int)inventorySlot),
                _inputManager
                );

            bodyCell.DrawGroupId = DrawGroupId;
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
