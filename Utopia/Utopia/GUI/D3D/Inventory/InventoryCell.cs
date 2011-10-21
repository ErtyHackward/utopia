using System;
using Nuclex.UserInterface.Controls;
using Nuclex.UserInterface.Input;
using S33M3Engines.InputHandler;
using Utopia.Entities;
using Utopia.Shared.Entities.Inventory;
using Utopia.Shared.Structs;

namespace Utopia.GUI.D3D.Inventory
{
    /// <summary>
    /// Represents inventory cell.
    /// </summary>
    public class InventoryCell : Control, IDropTarget
    {
        private readonly SlotContainer<ContainedSlot> _container;
        private readonly IconFactory _iconFactory;
        private ContainedSlot _slot;
        private Shared.Entities.Dynamic.PlayerCharacter _player;

        /// <summary>
        /// Gets current cell grid position
        /// </summary>
        public Vector2I InventoryPosition { get; private set; }

        /// <summary>
        /// Gets current slot
        /// </summary>
        public ContainedSlot Slot
        {
            get
            {
                if (_slot != null) return _slot;
                if(_container != null)
                    return _container.PeekSlot(InventoryPosition);
                if (_player != null)
                    return _player.Equipment[SlotType];
                return null;
            }
            set { _slot = value; }
        }

        /// <summary>
        /// Whether to draw the cell background
        /// </summary>
        public bool DrawCellBackground { get; set; }

        /// <summary>
        /// Gets icon factory used by cell to draw items icons
        /// </summary>
        public IconFactory IconFactory
        {
            get { return _iconFactory; }
        }
        
        public bool MouseHovering
        {
            get
            {
                var ms = Mouse.GetState();
                return GetAbsoluteBounds().Contains(ms.X, ms.Y);
            }
            set { throw new NotSupportedException(); }
        }

        public EquipmentSlotType SlotType { get; set; }

        public event EventHandler<MouseDownEventArgs> MouseDown;
        
        private void OnMouseDown(MouseDownEventArgs e)
        {
            var handler = MouseDown;
            if (handler != null) handler(this, e);
        }


        /// <summary>
        /// Creates new inventory cell and links it with some container
        /// </summary>
        /// <param name="container"></param>
        /// <param name="iconFactory"></param>
        /// <param name="position"></param>
        public InventoryCell(SlotContainer<ContainedSlot> container, IconFactory iconFactory, Vector2I position)
        {
            _container = container;
            _iconFactory = iconFactory;
            InventoryPosition = position;
            DrawCellBackground = true;
        }

        protected override void OnMousePressed(MouseButtons button)
        {
            OnMouseDown(new MouseDownEventArgs { Buttons = button });
        }
    }

    public class MouseDownEventArgs : EventArgs
    {
        public MouseButtons Buttons { get; set; }
    }
}