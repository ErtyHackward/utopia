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

        private SpriteTexture _stBtnCraft;
        private SpriteTexture _stBtnCraftDown;
        private SpriteTexture _stBtnCraftHover;
        
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

            _stBtnCraft = ToDispose(new SpriteTexture(engine.Device, @"Images\Inventory\inventory_close.png"));
            _stBtnCraftDown = ToDispose(new SpriteTexture(engine.Device, @"Images\Inventory\inventory_close_down.png"));
            _stBtnCraftHover = ToDispose(new SpriteTexture(engine.Device, @"Images\Inventory\inventory_close_hover.png"));

            //Bounds.Size = new S33M3CoreComponents.GUI.Nuclex.UniVector(312, 388);
            InitializeComponent();

            _craftButton.CustomImage = _stBtnCraft;
            _craftButton.CustomImageDown = _stBtnCraftDown;
            _craftButton.CustomImageHover = _stBtnCraftHover;
            _craftButton.CustomFont = _commonResources.FontBebasNeue25;
            _craftButton.Text = "CRAFT";
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

            foreach (var ingredientCell in _ingredientCells)
            {
                ingredientCell.CustomBackground = _commonResources.StInventorySlot;
                ingredientCell.CustomBackgroundHover = _commonResources.StInventorySlotHover;
                ingredientCell.DrawIconsGroupId = 5;
                ingredientCell.DrawIconsActiveCellId = 6;
            }
        }
    }
}