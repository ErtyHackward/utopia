using S33M3CoreComponents.GUI.Nuclex;
using S33M3CoreComponents.GUI.Nuclex.Controls.Desktop;
using S33M3CoreComponents.Inputs;
using S33M3CoreComponents.Sprites2D;
using S33M3DXEngine;
using Utopia.Entities;
using Utopia.Entities.Managers;
using Utopia.GUI.Crafting;
using Utopia.Shared.Configuration;

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
        private SpriteTexture _stLabelIngredients;
        private SpriteTexture _stLabelCraft;

        private SpriteTexture _stBtnCraft;
        private SpriteTexture _stBtnCraftDown;
        private SpriteTexture _stBtnCraftHover;
        
        public CraftingInventory(
                D3DEngine engine, 
                WorldConfiguration conf, 
                PlayerEntityManager character, 
                IconFactory iconFactory, 
                InputsManager inputManager, 
                SandboxCommonResources commonResources) :
            base(conf, character, iconFactory, inputManager)
        {
            _engine = engine;
            _iconFactory = iconFactory;
            _inputManager = inputManager;
            _commonResources = commonResources;
            _stInventoryWindow = ToDispose(new SpriteTexture(engine.Device, @"Images\Inventory\crafting_window.png"));

            _stLabelRecipes     = ToDispose(new SpriteTexture(engine.Device, @"Images\Inventory\label_crafting_recipes.png"));
            _stLabelIngredients = ToDispose(new SpriteTexture(engine.Device, @"Images\Inventory\label_ingredients.png"));
            _stLabelCraft       = ToDispose(new SpriteTexture(engine.Device, @"Images\Inventory\label_craft.png"));
            _stBtnCraft         = ToDispose(new SpriteTexture(engine.Device, @"Images\Inventory\inventory_close.png"));
            _stBtnCraftDown     = ToDispose(new SpriteTexture(engine.Device, @"Images\Inventory\inventory_close_down.png"));
            _stBtnCraftHover    = ToDispose(new SpriteTexture(engine.Device, @"Images\Inventory\inventory_close_hover.png"));

            CustomWindowImage = _stInventoryWindow;
            Bounds.Size = new UniVector(627, 388);

            InitializeComponent();
        }

        public override void InitializeComponent()
        {
            // create base controls
            base.InitializeComponent();

            // customize them

            // labels
            Children.Add(new ImageControl { Image = _stLabelRecipes, Bounds = new UniRectangle(20, 5, 136, 30), IsClickTransparent = true });
            Children.Add(new ImageControl { Image = _stLabelIngredients, Bounds = new UniRectangle(370, 250, 96, 18), IsClickTransparent = true });

            // cells
            for (int i = 0; i < 6; i++)
            {
                var cell = _ingredientCells[i];

                cell.DrawIconsGroupId = 5;
                cell.DrawIconsActiveCellId = 6;
                cell.CustomBackground = _commonResources.StInventorySlot;
                cell.CustomBackgroundHover = _commonResources.StInventorySlotHover;
            }

            // craft button

            const int buttonWidth = 185;
            const int buttomHeight = 50;

            _craftButton.CustomImage = _stBtnCraft;
            _craftButton.CustomImageDown = _stBtnCraftDown;
            _craftButton.CustomImageHover = _stBtnCraftHover;
            _craftButton.CusomImageLabel = _stLabelCraft;

            _craftButton.Bounds = new UniRectangle(436, 320, buttonWidth, buttomHeight);

        }
    }
}
