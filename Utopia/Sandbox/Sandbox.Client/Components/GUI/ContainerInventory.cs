using System.Drawing;
using S33M3CoreComponents.Inputs;
using S33M3CoreComponents.Sprites2D;
using S33M3DXEngine;
using S33M3Resources.Structs;
using Sandbox.Client.Components.GUI;
using Utopia.Entities;
using Utopia.GUI.Inventory;

namespace Realms.Client.Components.GUI
{
    public class ContainerInventory : InventoryWindow
    {
        private readonly SandboxCommonResources _commonResources;
        private SpriteTexture _stInventoryWindow;

        public ContainerInventory(D3DEngine engine, IconFactory iconFactory, InputsManager inputManager, SandboxCommonResources commonResources) : 
            base(null, iconFactory, new Point(20,20), new Point(20,20), inputManager)
        {
            _commonResources = commonResources;
            _stInventoryWindow = new SpriteTexture(engine.Device, @"Images\Inventory\inventory_window_container.png");

            CustomWindowImage = _stInventoryWindow;
            Bounds.Size = new S33M3CoreComponents.GUI.Nuclex.UniVector(312, 388);

        }

        protected override void CellsCreated()
        {
            if (_commonResources == null)
                return;

            var cellSize = new Vector2I(42, 42);

            for (var x = 0; x < UiGrid.GetLength(0); x++)
            {
                for (var y = 0; y < UiGrid.GetLength(1); y++)
                {
                    var cell = UiGrid[x, y];

                    cell.CustomBackground = _commonResources.StInventorySlot;
                    cell.CustomBackgroundHover = _commonResources.StInventorySlotHover;
                    cell.Bounds = new S33M3CoreComponents.GUI.Nuclex.UniRectangle(GridOffset.X + x * cellSize.X, GridOffset.Y + y * cellSize.Y, 42, 42);
                    cell.DrawIconsGroupId = 5;
                    cell.DrawIconsActiveCellId = 6;
                }
            }
        }
    }
}