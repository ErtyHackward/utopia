using System.Collections.Generic;
using System.Drawing;
using S33M3CoreComponents.GUI.Nuclex;
using S33M3CoreComponents.GUI.Nuclex.Controls.Desktop;
using S33M3CoreComponents.Inputs;
using S33M3CoreComponents.Sprites2D;
using S33M3DXEngine;
using S33M3Resources.Structs;
using Utopia.Entities;
using Utopia.GUI.Inventory;
using Utopia.Shared.Entities.Dynamic;

namespace Realms.Client.Components.GUI.Inventory
{
    public class CraftingInventory : CraftingWindow
    {
        private readonly D3DEngine _engine;
        private readonly IconFactory _iconFactory;
        private readonly InputsManager _inputManager;
        private readonly SandboxCommonResources _commonResources;
        private SpriteTexture _stInventoryWindow;
        private SpriteTexture _stLabelRecipes;
        private SpriteTexture _stLabelResult;
        private SpriteTexture _stLabelIngredients;
        private SpriteTexture _stLabelCraft;

        private ListControl _recipesList;
        private List<InventoryCell> _ingredientCells;
        private InventoryCell _resultCell;
        private ButtonControl _craftButton;

        public CraftingInventory(D3DEngine engine, PlayerCharacter character, IconFactory iconFactory, InputsManager inputManager, SandboxCommonResources commonResources) :
            base(character.Inventory, iconFactory, new Point(200, 120), new Point(570, 50), inputManager)
        {
            _engine = engine;
            _iconFactory = iconFactory;
            _inputManager = inputManager;
            _commonResources = commonResources;
            _stInventoryWindow = new SpriteTexture(engine.Device, @"Images\Inventory\crafting_window.png");

            _stLabelRecipes = new SpriteTexture(engine.Device, @"Images\Inventory\label_recipes.png");
            _stLabelResult = new SpriteTexture(engine.Device, @"Images\Inventory\label_result.png");
            _stLabelIngredients = new SpriteTexture(engine.Device, @"Images\Inventory\label_ingredients.png");
            _stLabelCraft = new SpriteTexture(engine.Device, @"Images\Inventory\label_craft.png");

            CustomWindowImage = _stInventoryWindow;
            Bounds.Size = new UniVector(904, 388);

            CellsCreated();
            CreateCraftingControls();
        }

        private void CreateCraftingControls()
        {
            var recipesList = new ListControl();
            recipesList.Bounds = new UniRectangle(20, 50, 200, 300);
            Children.Add(recipesList);

            // labels
            Children.Add(new ImageControl { Image = _stLabelRecipes, Bounds = new UniRectangle(20, 20, 60, 18) });
            Children.Add(new ImageControl { Image = _stLabelResult, Bounds = new UniRectangle(240, 20, 55, 18) });
            Children.Add(new ImageControl { Image = _stLabelIngredients, Bounds = new UniRectangle(240, 120, 96, 18) });

            // cells
            _resultCell = new InventoryCell(null, _iconFactory, new Vector2I(), _inputManager)
                {
                    Bounds = new UniRectangle(240, 50, 42, 42),
                    DrawIconsGroupId = 5,
                    DrawIconsActiveCellId = 6,
                    CustomBackground = _commonResources.StInventorySlot,
                    CustomBackgroundHover = _commonResources.StInventorySlotHover
                };


            Children.Add(_resultCell);
            
            _ingredientCells = new List<InventoryCell>();
            for (int i = 0; i < 6; i++)
            {
                var cell = new InventoryCell(null, _iconFactory, new Vector2I(), _inputManager)
                    {
                        Bounds = new UniRectangle(240 + i % 3 * 50, 150 + i / 3 * 50, 42, 42),
                        DrawIconsGroupId = 5,
                        DrawIconsActiveCellId = 6,
                        CustomBackground = _commonResources.StInventorySlot,
                        CustomBackgroundHover = _commonResources.StInventorySlotHover
                    };

                _ingredientCells.Add(cell);
                Children.Add(cell);
            }

            // craft button

            const int buttonWidth = 212;
            const int buttomHeight = 40;

            _craftButton = new ButtonControl
            {
                CustomImage = _commonResources.StButtonBackground,
                CustomImageDown = _commonResources.StButtonBackgroundDown,
                CustomImageHover = _commonResources.StButtonBackgroundHover,
                CusomImageLabel = _stLabelCraft,
                Bounds = new UniRectangle(240, 250, buttonWidth, buttomHeight)
            };
            _craftButton.Pressed += delegate { };
            Children.Add(_craftButton);

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
