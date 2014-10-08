using System;
using System.Drawing;
using Utopia.Entities;
using Utopia.Shared.Entities.Inventory;
using S33M3CoreComponents.GUI.Nuclex.Controls.Desktop;
using S33M3CoreComponents.GUI.Nuclex;
using S33M3Resources.Structs;
using S33M3CoreComponents.Inputs.MouseHandler;
using S33M3CoreComponents.Inputs;

namespace Utopia.GUI.Inventory
{
    /// <summary>
    /// Represents common window for inventory containers that have a grid of items (chests)
    /// </summary>
    public class InventoryWindow : WindowControl
    {
        protected readonly InputsManager _inputManager;
        private SlotContainer<ContainedSlot> _content;

        /// <summary>
        /// Array of inventory cells controls
        /// </summary>
        protected InventoryCell[,] UiGrid;
        private readonly IconFactory _iconFactory;
        private readonly Point _windowStartPosition;
        protected readonly Point GridOffset;
        public const int CellSize = 52;
        
        /// <summary>
        /// Occurs when some slot get clicked
        /// </summary>
        public event EventHandler<InventoryWindowEventArgs> InventorySlotClicked;

        protected void OnSlotClicked(InventoryWindowEventArgs e)
        {
            var handler = InventorySlotClicked;
            if (handler != null) handler(this, e);
        }

        public event EventHandler<InventoryWindowCellMouseEventArgs> CellMouseEnter;

        protected void OnCellMouseEnter(InventoryWindowCellMouseEventArgs e)
        {
            var handler = CellMouseEnter;
            if (handler != null) handler(this, e);
        }

        public event EventHandler<InventoryWindowCellMouseEventArgs> CellMouseLeave;

        protected void OnCellMouseLeave(InventoryWindowCellMouseEventArgs e)
        {
            var handler = CellMouseLeave;
            if (handler != null) handler(this, e);
        }

        public event EventHandler<InventoryWindowEventArgs> CellMouseUp;

        private void OnMouseUp(InventoryWindowEventArgs e)
        {
            var handler = CellMouseUp;
            if (handler != null) handler(this, e);
        }

        public event EventHandler<InventoryWindowEventArgs> CellMouseDown;

        private void OnMouseDown(InventoryWindowEventArgs e)
        {
            var handler = CellMouseDown;
            if (handler != null) handler(this, e);
        }

        /// <summary>
        /// Gets or changes container wrapped
        /// </summary>
        public SlotContainer<ContainedSlot> Content
        {
            get { return _content; }
            set {
                _content = value;

                if (_content != null)
                    BuildGrid(GridOffset);
            }
        }

        protected InventoryWindow(SlotContainer<ContainedSlot> container, IconFactory iconFactory, Point windowStartPosition, Point gridOffset, InputsManager inputManager)
        {
            _inputManager = inputManager;
            GridOffset = gridOffset;
            _iconFactory = iconFactory;
            _windowStartPosition = windowStartPosition;

            Content = container;
        }

        public InventoryWindow(SlotContainer<ContainedSlot> container, IconFactory iconFactory, Point windowStartPosition, InputsManager inputManager)
            : this(container, iconFactory, windowStartPosition, new Point(4, 24), inputManager)
        {
       
        }

        protected virtual void CellsCreated()
        {

        }

        public void BuildGrid(Point offset)
        {
            var width = _content.GridSize.X * CellSize + GridOffset.X + 4;
            var height = _content.GridSize.Y * CellSize + GridOffset.Y + 22 + 4; // 22 = bottom line, 4 - bottom side

            //Bounds = new UniRectangle(_windowStartPosition.X, _windowStartPosition.Y, width, height);

            if (UiGrid != null)
            {
                for (int x = 0; x <= UiGrid.GetUpperBound(0); x++)
                {
                    for (int y = 0; y <= UiGrid.GetUpperBound(1); y++)
                    {
                        var cell = UiGrid[x, y];
                        cell.MouseDown -= ControlMouseDown;
                        cell.MouseEnter -= ControlMouseEnter;
                        cell.MouseLeave -= ControlMouseLeave;
                        cell.MouseUp -= ControlMouseUp;
                        Children.Remove(cell);
                    }
                }
            }
            
            var container = _content;
            
            UiGrid = new InventoryCell[container.GridSize.X, container.GridSize.Y];
            
            for (var x = 0; x < container.GridSize.X; x++)
            {
                for (var y = 0; y < container.GridSize.Y; y++)
                {
                    var control = new InventoryCell(_content, _iconFactory, new Vector2I(x, y), _inputManager)
                        {
                            Bounds = new UniRectangle(offset.X + x * CellSize, offset.Y + y * CellSize, CellSize, CellSize),
                            Name = "Cell" + x + "," + y,
                            DrawGroupId = DrawGroupId,
                        };
                    control.MouseDown += ControlMouseDown;
                    control.MouseEnter += ControlMouseEnter;
                    control.MouseLeave += ControlMouseLeave;
                    control.MouseUp += ControlMouseUp;
                    Children.Add(control);

                    UiGrid[x, y] = control;
                }
            }

            CellsCreated();
        }

        protected void ControlMouseUp(object sender, MouseDownEventArgs e)
        {
            OnMouseUp(CreateEventArgs((InventoryCell)sender));
        }

        protected void ControlMouseLeave(object sender, EventArgs e)
        {
            OnCellMouseLeave(new InventoryWindowCellMouseEventArgs { Cell = (InventoryCell)sender });
        }

        protected void ControlMouseEnter(object sender, EventArgs e)
        {
            OnCellMouseEnter(new InventoryWindowCellMouseEventArgs { Cell = (InventoryCell)sender });
        }

        public InventoryWindowEventArgs CreateEventArgs(InventoryCell cell)
        {
            if (cell == null)
                return null;

            var control = cell;
            var state = _inputManager.MouseManager.Mouse.GetState();
            var bounds = control.GetAbsoluteBounds();

            // slot offset
            var offset = new Point(
                (int)bounds.X - state.X,
                (int)bounds.Y - state.Y
                );

            return new InventoryWindowEventArgs
                {
                    SlotPosition = control.InventoryPosition,
                    MouseState = state,
                    Container = cell.Container,
                    Offset = offset
                };
        }


        protected void ControlMouseDown(object sender, MouseDownEventArgs e)
        {

            var ea = CreateEventArgs((InventoryCell)sender);

            OnMouseDown(ea);

            // tell everyone that user click some slot
            OnSlotClicked(ea);
        }
    }

    public class InventoryWindowCellMouseEventArgs : EventArgs
    {
        public InventoryCell Cell { get; set; }
    }

    public class InventoryWindowEventArgs : EventArgs
    {
        /// <summary>
        /// Slot position that was clicked
        /// </summary>
        public Vector2I SlotPosition { get; set; }

        /// <summary>
        /// Mouse state
        /// </summary>
        public MouseState MouseState { get; set; }

        /// <summary>
        /// Affected container
        /// </summary>
        public SlotContainer<ContainedSlot> Container { get; set; }

        public Point Offset { get; set; }
    }
}