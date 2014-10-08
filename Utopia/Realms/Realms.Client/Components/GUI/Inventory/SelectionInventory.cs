using S33M3CoreComponents.GUI.Nuclex;
using S33M3CoreComponents.Sprites2D;
using S33M3DXEngine;
using Utopia.Entities;
using Utopia.GUI.CharacterSelection;
using Utopia.Shared.Configuration;

namespace Realms.Client.Components.GUI.Inventory
{
    public class SelectionInventory : CharacterSelectionWindow
    {
        private readonly SandboxCommonResources _commonResources;
        private SpriteTexture _stBtnSelect;
        private SpriteTexture _stBtnSelectDown;
        private SpriteTexture _stBtnSelectHover;

        private SpriteTexture _stInventoryWindow;

        public SelectionInventory(D3DEngine engine, WorldConfiguration conf, IconFactory iconFactory, SandboxCommonResources commonResources) : 
            base(conf, iconFactory)
        {
            _commonResources = commonResources;
            _stBtnSelect = ToDispose(new SpriteTexture(engine.Device, @"Images\Inventory\inventory_close.png"));
            _stBtnSelectDown = ToDispose(new SpriteTexture(engine.Device, @"Images\Inventory\inventory_close_down.png"));
            _stBtnSelectHover = ToDispose(new SpriteTexture(engine.Device, @"Images\Inventory\inventory_close_hover.png"));

            _stInventoryWindow = ToDispose(new SpriteTexture(engine.Device, @"Images\Inventory\crafting_window.png"));

            CustomWindowImage = _stInventoryWindow;
            Bounds.Size = new UniVector(627, 388);

            InitializeComponent();
        }

        protected override void InitializeComponent()
        {
            base.InitializeComponent();

            const int buttonWidth = 185;
            const int buttomHeight = 50;

            _selectButton.CustomImage = _stBtnSelect;
            _selectButton.CustomImageDown = _stBtnSelectDown;
            _selectButton.CustomImageHover = _stBtnSelectHover;
            _selectButton.CustomFont = _commonResources.FontBebasNeue25;
            //_selectButton.CusomImageLabel = _stLabelCraft;

            _selectButton.Bounds = new UniRectangle(436, 320, buttonWidth, buttomHeight);

        }
    }
}
