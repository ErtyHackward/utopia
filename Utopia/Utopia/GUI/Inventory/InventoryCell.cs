using System;
using S33M3CoreComponents.Sprites2D;
using Utopia.Entities;
using Utopia.Shared.Entities.Inventory;
using S33M3Resources.Structs;
using S33M3CoreComponents.GUI.Nuclex.Input;
using S33M3CoreComponents.GUI.Nuclex.Controls;
using S33M3CoreComponents.Inputs;

namespace Utopia.GUI.Inventory
{
    /// <summary>
    /// Represents inventory cell.
    /// </summary>
    public class InventoryCell : Control, IDropTarget
    {
        private readonly InputsManager _inputManager;
        private readonly IconFactory _iconFactory;
        private SlotContainer<ContainedSlot> _container;
        private ContainedSlot _slot;
        private int _drawIconsGroupId;

        public SlotContainer<ContainedSlot> Container
        {
            get { return _container; }
            set { _container = value; }
        }

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
                if (_slot != null) 
                    return _slot;
                if(_container != null)
                    return _container.PeekSlot(InventoryPosition);
                return null;
            }
            set { _slot = value; }
        }

        public int DrawIconsActiveCellId { get; set; }
        
        public int DrawIconsGroupId
        {
            get {
                if (IsCellSelected || MouseHovering)
                    return DrawIconsActiveCellId;
                return _drawIconsGroupId; 
            }
            set { _drawIconsGroupId = value; }
        }

        /// <summary>
        /// Whether to draw the cell background
        /// </summary>
        public bool DrawCellBackground { get; set; }

        /// <summary>
        /// Whether the cell is drawn selected
        /// </summary>
        public bool IsCellSelected { get; set; }

        /// <summary>
        /// Disabled icons is drawn with transparency
        /// </summary>
        public bool IsDisabledCell { get; set; }

        /// <summary>
        /// Allows to customize the apperance of the cell
        /// </summary>
        public SpriteTexture CustomBackground { get; set; }

        /// <summary>
        /// Allows to customize the apperance of the cell
        /// </summary>
        public SpriteTexture CustomBackgroundHover { get; set; }

        /// <summary>
        /// Set to true if you want to fit the custom background to the cell size
        /// </summary>
        public bool CustomBackgroundAutosize { get; set; }

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
                var ms = _inputManager.MouseManager.Mouse.GetState();
                return GetAbsoluteBounds().Contains(ms.X, ms.Y);
            }
            set { throw new NotSupportedException(); }
        }

        public EquipmentSlotType SlotType { get; set; }

        /// <summary>
        /// Special string to display instead of the count
        /// </summary>
        public string CountString { get; set; }

        public override bool ToolTipEnabled
        {
            get { return base.ToolTipEnabled && Slot != null; }
            set
            {
                base.ToolTipEnabled = value;
            }
        }

        public event EventHandler<MouseDownEventArgs> MouseUp;

        private void OnMouseUp(MouseDownEventArgs e)
        {
            var handler = MouseUp;
            if (handler != null) handler(this, e);
        }

        public event EventHandler<MouseDownEventArgs> MouseDown;
        
        private void OnMouseDown(MouseDownEventArgs e)
        {
            var handler = MouseDown;
            if (handler != null) handler(this, e);
        }

        public event EventHandler MouseEnter;

        private void OnMouseEnter(EventArgs e)
        {
            var handler = MouseEnter;
            if (handler != null) handler(this, e);
        }

        public event EventHandler MouseLeave;

        private void OnMouseLeave(EventArgs e)
        {
            var handler = MouseLeave;
            if (handler != null) handler(this, e);
        }


        /// <summary>
        /// Creates new inventory cell and links it with some container
        /// </summary>
        /// <param name="container"></param>
        /// <param name="iconFactory"></param>
        /// <param name="position"></param>
        public InventoryCell(SlotContainer<ContainedSlot> container, IconFactory iconFactory, Vector2I position, InputsManager inputManager)
        {
            _inputManager = inputManager;
            _container = container;
            _iconFactory = iconFactory;
            InventoryPosition = position;
            DrawCellBackground = true;
            DrawIconsGroupId = 2;
            base.ToolTipEnabled = true;
        }

        protected override void OnMouseEntered()
        {
            OnMouseEnter(EventArgs.Empty);
        }

        protected override void OnMouseLeft()
        {
            OnMouseLeave(EventArgs.Empty);
        }

        protected override void OnMousePressed(MouseButtons button)
        {
            OnMouseDown(new MouseDownEventArgs { Buttons = button });
        }

        protected override void OnMouseReleased(MouseButtons button)
        {
            OnMouseUp(new MouseDownEventArgs {Buttons = button});
        }
    }

    public class MouseDownEventArgs : EventArgs
    {
        public MouseButtons Buttons { get; set; }
    }
}