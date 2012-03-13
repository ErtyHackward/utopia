﻿using System;
using System.Drawing;
using Utopia.Entities;
using Utopia.Shared.Entities.Inventory;
using Utopia.Shared.Structs;
using S33M3CoreComponents.GUI.Nuclex.Controls.Desktop;
using S33M3CoreComponents.GUI.Nuclex;
using S33M3Resources.Structs;
using S33M3CoreComponents.Inputs.MouseHandler;
using S33M3CoreComponents.Inputs;

namespace Utopia.GUI.Inventory
{
    /// <summary>
    /// Represents common window for inventory containers
    /// </summary>
    public class InventoryWindow : WindowControl
    {
        protected readonly InputsManager _inputManager;
        private readonly SlotContainer<ContainedSlot> _container;
        private InventoryCell[,] _uiGrid;
        private readonly IconFactory _iconFactory;
        private readonly Point _windowStartPosition;
        protected readonly Point GridOffset;
        public const int CellSize = 38;
        
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


        /// <summary>
        /// Gets container wrapped
        /// </summary>
        public SlotContainer<ContainedSlot> Container
        {
            get { return _container; }
        }

        protected InventoryWindow(SlotContainer<ContainedSlot> container, IconFactory iconFactory, Point windowStartPosition, Point gridOffset, InputsManager inputManager)
        {
            _inputManager = inputManager;
            GridOffset = gridOffset;
            _container = container;
            _iconFactory = iconFactory;
            _windowStartPosition = windowStartPosition;
            Title = "Inventory";

            var width = _container.GridSize.X * CellSize + GridOffset.X + 4;     
            var height = _container.GridSize.Y * CellSize + GridOffset.Y + 22 + 4; // 22 = bottom line, 4 - bottom side

            Bounds = new UniRectangle(_windowStartPosition.X, _windowStartPosition.Y, width, height);

            BuildGrid(GridOffset);  
        }

        public InventoryWindow(SlotContainer<ContainedSlot> container, IconFactory iconFactory, Point windowStartPosition, InputsManager inputManager)
            : this(container, iconFactory, windowStartPosition, new Point(4, 24), inputManager)
        {
       
        }
        
        public void BuildGrid(Point offset)
        {
            var container = _container;

            _uiGrid = new InventoryCell[container.GridSize.X, container.GridSize.Y];

            for (var x = 0; x < container.GridSize.X; x++)
            {
                for (var y = 0; y < container.GridSize.Y; y++)
                {
                    var control = new InventoryCell(_container, _iconFactory, new Vector2I(x, y), _inputManager)
                                      {
                                          Bounds = new UniRectangle(offset.X + x * CellSize, offset.Y + y * CellSize, CellSize, CellSize),
                                          Name = "Cell" + x + "," + y,
                                      };

                    control.MouseDown += ControlMouseDown;
                    control.MouseEnter += ControlMouseEnter;
                    control.MouseLeave += ControlMouseLeave;
                    Children.Add(control);

                    _uiGrid[x, y] = control;
                }
            }
        }

        void ControlMouseLeave(object sender, EventArgs e)
        {
            OnCellMouseLeave(new InventoryWindowCellMouseEventArgs { Cell = (InventoryCell)sender });
        }

        void ControlMouseEnter(object sender, EventArgs e)
        {
            OnCellMouseEnter(new InventoryWindowCellMouseEventArgs { Cell = (InventoryCell)sender });
        }

        void ControlMouseDown(object sender, MouseDownEventArgs e)
        {
            var state = _inputManager.MouseManager.Mouse.GetState();

            var bounds = GetAbsoluteBounds();

            // detect the slot was clicked
            var slotPosition = new Vector2I(
                (state.X - (int)bounds.X - GridOffset.X) / CellSize,
                (state.Y - (int)bounds.Y - GridOffset.Y) / CellSize
                );

            if (slotPosition.X < 0 || slotPosition.X >= _container.GridSize.X || slotPosition.Y < 0 || slotPosition.Y >= _container.GridSize.Y)
            {
                // out of bounds (put to world)
                return;
            }

            // slot offset
            var offset = new Point(
                (state.X - (int)bounds.X - GridOffset.X) % CellSize,
                (state.Y - (int)bounds.Y - GridOffset.Y) % CellSize
                );

            // tell everyone that user click some slot
            OnSlotClicked(new InventoryWindowEventArgs { SlotPosition = slotPosition, MouseState = state, Container = _container, Offset = offset });
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