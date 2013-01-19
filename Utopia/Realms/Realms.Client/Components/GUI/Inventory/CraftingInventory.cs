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
using Utopia.Shared.Configuration;
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

        public CraftingInventory(D3DEngine engine, WorldConfiguration conf, PlayerCharacter character, IconFactory iconFactory, InputsManager inputManager, SandboxCommonResources commonResources) :
            base(conf, character, iconFactory, inputManager)
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

            InitializeComponent();
        }

        public override void InitializeComponent()
        {
            // create base controls
            base.InitializeComponent();

            // customize them

            // labels
            Children.Add(new ImageControl { Image = _stLabelRecipes, Bounds = new UniRectangle(20, 20, 60, 18) });
            Children.Add(new ImageControl { Image = _stLabelIngredients, Bounds = new UniRectangle(240, 120, 96, 18) });

            // cells
            for (int i = 0; i < 6; i++)
            {
                var cell = _ingredientCells[i];

                cell.DrawIconsGroupId = 5;
                cell.DrawIconsActiveCellId = 6;
                cell.CustomBackground = _commonResources.StInventorySlot;
                cell.CustomBackgroundHover = _commonResources.StInventorySlotHover;

                _ingredientCells.Add(cell);
                Children.Add(cell);
            }

            // craft button

            const int buttonWidth = 212;
            const int buttomHeight = 40;

            _craftButton.CustomImage = _commonResources.StButtonBackground;
            _craftButton.CustomImageDown = _commonResources.StButtonBackgroundDown;
            _craftButton.CustomImageHover = _commonResources.StButtonBackgroundHover;
            _craftButton.CusomImageLabel = _stLabelCraft;

            _craftButton.Bounds = new UniRectangle(240, 250, buttonWidth, buttomHeight);

        }
    }
}
