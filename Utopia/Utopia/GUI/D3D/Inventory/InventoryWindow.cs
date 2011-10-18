using System.Drawing;
using System.Linq;
using Nuclex.UserInterface;
using Nuclex.UserInterface.Controls.Desktop;
using Utopia.Entities;
using Utopia.Shared.Entities.Inventory;
using Utopia.Shared.Structs;

namespace Utopia.GUI.D3D.Inventory
{
    /// <summary>
    /// Represents common window for inventory containers
    /// </summary>
    public class InventoryWindow : WindowControl
    {
        private readonly SlotContainer<ContainedSlot> _container;
        private InventoryCell[,] _uiGrid;
        private readonly IconFactory _iconFactory;
        private readonly Point _windowStartPosition;

        private const int CellSize = 38;

        public InventoryWindow(SlotContainer<ContainedSlot> container, IconFactory iconFactory, Point windowStartPosition)
        {
            _container = container;
            _iconFactory = iconFactory;
            _windowStartPosition = windowStartPosition;
            Title = "Inventory";

            var width = _container.GridSize.X * CellSize + 8;      // 8 = 4+4 (each window side)
            var height = _container.GridSize.Y * CellSize + 24 + 4; // 24 = window header, 4 - bottom side

            Bounds = new UniRectangle(_windowStartPosition.X, _windowStartPosition.Y, width, height);
            
            BuildGrid(new Point(4, 24));         
        }
        
        public void BuildGrid(Point offset)
        {
            var container = _container;

            _uiGrid = new InventoryCell[container.GridSize.X, container.GridSize.Y];

            for (var x = 0; x < container.GridSize.X; x++)
            {
                for (var y = 0; y < container.GridSize.Y; y++)
                {
                    var control = new InventoryCell(_container, _iconFactory, new Vector2I(x, y))
                                      {
                                          Bounds = new UniRectangle(offset.X + x*CellSize, offset.Y + y*CellSize, CellSize, CellSize),
                                          Name = "Cell"+ x + "," + y,
                                      };
                    Children.Add(control);

                    _uiGrid[x, y] = control;
                }
            }
        }
    }
}