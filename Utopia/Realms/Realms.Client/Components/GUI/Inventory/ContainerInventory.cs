using System.Drawing;
using S33M3CoreComponents.Inputs;
using S33M3CoreComponents.Sprites2D;
using S33M3DXEngine;
using S33M3Resources.Structs;
using Utopia.Entities;
using Utopia.Entities.Managers;
using Utopia.GUI.Inventory;
using Utopia.Shared.Configuration;

namespace Realms.Client.Components.GUI.Inventory
{
    public class ContainerInventory : ContainerWindow
    {
        private readonly SandboxCommonResources _commonResources;


        public ContainerInventory(
            D3DEngine engine, 
            IconFactory iconFactory, 
            InputsManager inputManager, 
            SandboxCommonResources commonResources, 
            WorldConfiguration config,
            PlayerEntityManager playerEntityManager
            ) : base(config, playerEntityManager, iconFactory, inputManager)
        {
            _commonResources = commonResources;

            Bounds.Size = new S33M3CoreComponents.GUI.Nuclex.UniVector(312, 388);
            InitializeComponent();
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