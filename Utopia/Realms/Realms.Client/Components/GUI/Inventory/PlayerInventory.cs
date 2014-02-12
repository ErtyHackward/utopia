using System.Drawing;
using S33M3CoreComponents.Inputs;
using S33M3CoreComponents.Sprites2D;
using S33M3DXEngine;
using S33M3Resources.Structs;
using Utopia.Entities;
using Utopia.Entities.Managers;
using Utopia.GUI.Inventory;

namespace Realms.Client.Components.GUI.Inventory
{
    /// <summary>
    /// Sandbox inventory window
    /// </summary>
    public class PlayerInventory : CharacterInventory
    {
        private readonly SandboxCommonResources _commonResources;
        private SpriteTexture _stInventoryWindow;
        private SpriteTexture _stInventoryCloseButton;
        private SpriteTexture _stInventoryCloseButtonHover;
        private SpriteTexture _stInventoryCloseButtonDown;
        private SpriteTexture _stInventoryCloseButtonLabel;
        public override int DrawGroupId
        {
            get
            {
                return 2;
            }
        }

        public PlayerInventory(D3DEngine engine, PlayerEntityManager character, IconFactory iconFactory, InputsManager inputManager, SandboxCommonResources commonResources) : 
            base(character, iconFactory, new Point(200,120), new Point(270,50), inputManager)
        {
            _commonResources = commonResources;
            _stInventoryWindow              = new SpriteTexture(engine.Device, @"Images\Inventory\inventory_window.png");
            _stInventoryCloseButton         = new SpriteTexture(engine.Device, @"Images\Inventory\inventory_close.png");
            _stInventoryCloseButtonHover    = new SpriteTexture(engine.Device, @"Images\Inventory\inventory_close_hover.png");
            _stInventoryCloseButtonDown     = new SpriteTexture(engine.Device, @"Images\Inventory\inventory_close_down.png");
            _stInventoryCloseButtonLabel    = new SpriteTexture(engine.Device, @"Images\Inventory\inventory_close_label.png");

            CustomWindowImage = _stInventoryWindow;
            Bounds.Size = new S33M3CoreComponents.GUI.Nuclex.UniVector(674, 388);

            CellsCreated();
            PrepareCells();
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

        private void PrepareCells()
        {
            
            var slot = BuildBodyslot(Utopia.Shared.Entities.Inventory.EquipmentSlotType.Hand, 19, 142, 42);

            slot.CustomBackground = _commonResources.StInventoryEquipmentSlot;
            slot.CustomBackgroundHover = _commonResources.StInventoryEquipmentSlotHover;

        }
    }
}
