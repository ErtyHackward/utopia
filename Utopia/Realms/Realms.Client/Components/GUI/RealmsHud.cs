using System;
using Ninject;
using Realms.Client.Components.GUI.Settings;
using S33M3CoreComponents.Cameras;
using S33M3CoreComponents.Cameras.Interfaces;
using S33M3CoreComponents.GUI.Nuclex;
using S33M3CoreComponents.GUI.Nuclex.Controls;
using S33M3CoreComponents.Inputs;
using S33M3CoreComponents.Sprites2D;
using S33M3DXEngine;
using S33M3Resources.Structs.Vertex;
using Utopia.Entities.Managers;
using Utopia.GUI;
using Utopia.GUI.Inventory;
using Utopia.Resources.Effects.Entities;
using Utopia.Shared.Settings;
using Utopia.Worlds.Weather;
using Utopia.Worlds.Chunks;

namespace Realms.Client.Components.GUI
{
    public class RealmsHud : Hud
    {
        private readonly MainScreen _screen;
        private readonly D3DEngine _d3DEngine;
        private SpriteTexture _stIconInventory;
        private SpriteTexture _stIconCrafting;

        private LabelControl _inventoryLabel;
        private LabelControl _craftingLabel;

        /// <summary>
        /// Whether to display help icons: Inventory, Crafting
        /// </summary>
        public bool DisplayHintsIcons { get; set; }

        /// <summary>
        /// Gets Inventory hint button
        /// </summary>
        public AlphaImageButtonControl InventoryButton { get; private set; }

        /// <summary>
        /// Gets crafting hint button
        /// </summary>
        public AlphaImageButtonControl CraftingButton { get; private set; }

        [Inject]
        public SettingsComponent SettignsComponent
        {
            set
            {
                value.KeyBindingChanged += delegate { UpdateLabels(); };
            }
        }

        public RealmsHud(MainScreen screen, 
                         D3DEngine d3DEngine, 
                         ToolBarUi toolbar, 
                         InputsManager inputManager, 
                         CameraManager<ICameraFocused> camManager,
                         PlayerEntityManager playerEntityManager,
                         IWeather weather,
                         IWorldChunks2D worldChunks
                         ) :
            base(screen, d3DEngine, toolbar, inputManager, camManager, playerEntityManager, weather, worldChunks)
        {
            _screen = screen;
            _d3DEngine = d3DEngine;

            _d3DEngine.ScreenSize_Updated += UpdateLayout;
        }

        private void UpdateLabels()
        {
            _inventoryLabel.Text = ClientSettings.Current.Settings.KeyboardMapping.Game.Inventory.MainKey.ToString();
            _craftingLabel.Text = ClientSettings.Current.Settings.KeyboardMapping.Game.Crafting.MainKey.ToString();

        }

        public override void Initialize()
        {
            _stIconInventory = ToDispose(SandboxCommonResources.LoadTexture(_d3DEngine, "Images\\Inventory\\icon_inventory.png"));
            _stIconCrafting = ToDispose(SandboxCommonResources.LoadTexture(_d3DEngine, "Images\\Inventory\\icon_crafting.png"));

            InventoryButton = new AlphaImageButtonControl
            {
                CustomImage = _stIconInventory,
                CustomImageDown = _stIconInventory,
                CustomImageHover = _stIconInventory,
                AlphaDefault = 0.5f,
                AlphaHover = 0.8f,
                AlphaDown = 1f,
                LayoutFlags = ControlLayoutFlags.Skip
            };

            CraftingButton = new AlphaImageButtonControl
            {
                CustomImage = _stIconCrafting,
                CustomImageDown = _stIconCrafting,
                CustomImageHover = _stIconCrafting,
                AlphaDefault = 0.5f,
                AlphaHover = 0.8f,
                AlphaDown = 1f,
                LayoutFlags = ControlLayoutFlags.Skip
            };

            _inventoryLabel = new LabelControl
            {
                IsClickTransparent = true,
                Color = new S33M3Resources.Structs.ByteColor(255, 255, 255, 100),
                LayoutFlags = ControlLayoutFlags.Skip
            };

            _craftingLabel = new LabelControl
            {
                IsClickTransparent = true,
                Color = new S33M3Resources.Structs.ByteColor(255, 255, 255, 100),
                LayoutFlags = ControlLayoutFlags.Skip
            };

            UpdateLabels();

            UpdateLayout(_d3DEngine.ViewPort, _d3DEngine.BackBufferTex.Description);

            base.Initialize();
        }

        public override void EnableComponent(bool forced)
        {
            _screen.Desktop.Children.Add(InventoryButton);
            _screen.Desktop.Children.Add(CraftingButton);
            _screen.Desktop.Children.Add(_inventoryLabel);
            _screen.Desktop.Children.Add(_craftingLabel);

            base.EnableComponent(forced);
        }

        public override void DisableComponent()
        {
            _screen.Desktop.Children.Remove(InventoryButton);
            _screen.Desktop.Children.Remove(CraftingButton);
            _screen.Desktop.Children.Remove(_inventoryLabel);
            _screen.Desktop.Children.Remove(_craftingLabel);

            base.DisableComponent();
        }

        void UpdateLayout(SharpDX.ViewportF viewport, SharpDX.Direct3D11.Texture2DDescription newBackBuffer)
        {
            InventoryButton.Bounds = new UniRectangle(20, viewport.Height - 90, 64, 64);
            CraftingButton.Bounds = new UniRectangle(104, viewport.Height - 84, 64, 64);
            _inventoryLabel.Bounds = new UniRectangle(47, viewport.Height - 30, 20, 20);
            _craftingLabel.Bounds = new UniRectangle(134, viewport.Height - 30, 20, 20);
        }
    }
}
