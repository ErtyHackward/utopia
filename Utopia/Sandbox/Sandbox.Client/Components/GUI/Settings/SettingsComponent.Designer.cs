using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3CoreComponents.GUI.Nuclex.Controls;
using S33M3CoreComponents.GUI.Nuclex.Controls.Arcade;
using SharpDX.Direct3D11;
using S33M3CoreComponents.GUI.Nuclex;
using SharpDX;
using S33M3CoreComponents.GUI.Nuclex.Controls.Desktop;
using S33M3Resources.Structs;
using S33M3CoreComponents.Sprites;
using S33M3CoreComponents.Textures;
using S33M3CoreComponents.GUI.Nuclex.Visuals.Flat;
using Utopia.Shared.Settings;

namespace Sandbox.Client.Components.GUI.Settings
{
    partial class SettingsComponent
    {
        //Components
        protected LabelControl _windowLabel, _settingsStateLabel;
        protected Control _settingsForm;
        protected PanelControl _leftMenuPanel;
        protected ImageControl _cubesPatern;
        protected ImageControl _linenPatern;
        protected ImageControl _shadow;

        protected SpriteTexture _transparentBackGroundTexture;
        protected ImageControl _backPanel;

        protected SpriteTexture _stMenuButton;
        protected SpriteTexture _stMenuHover;
        protected SpriteTexture _stMenuDown;

        protected ButtonControl _btGraphic;
        protected ButtonControl _btSound;
        protected ButtonControl _btCoreEngine;
        protected ButtonControl _btKeyBinding;
        protected ButtonControl _btBack;

        //Various Panels
        protected GraphicSettingsPanel _graphSettingsPanel;
        protected CoreEngineSettingsPanel _coreEngineSetingsPanel;
        protected SoundSettingsPanel _soundSettingsPanel;
        protected KeyBindingSettingsPanel _keyBindingSettingsPanel;

        protected void InitializeComponent()
        {
            _stMenuButton = ToDispose(new SpriteTexture(_engine.Device, @"Images\MainMenu\menu_button.png", new Vector2I()));
            _stMenuHover = ToDispose(new SpriteTexture(_engine.Device, @"Images\MainMenu\menu_button_hover.png", new Vector2I()));
            _stMenuDown = ToDispose(new SpriteTexture(_engine.Device, @"Images\MainMenu\menu_button_down.png", new Vector2I()));

            _windowLabel = new LabelControl() { Text = "Settings", 
                                                Color = new ByteColor(198,0,75), 
                                                CustomFont = SandboxMenuComponent.FontBebasNeue35,
                                                CustomHorizontalPlacement = FlatGuiGraphics.Frame.HorizontalTextAlignment.Center,
                                                CustomVerticalPlacement = FlatGuiGraphics.Frame.VerticalTextAlignment.Center};

            _settingsStateLabel = new LabelControl() { Text = "Restart needed to apply new setting(s)",
                                                        Color = Colors.Red, IsVisible = false};

            _leftMenuPanel = new PanelControl() { Color = Colors.WhiteSmoke };
            _cubesPatern = new ImageControl() { Image = SandboxMenuComponent.StCubesPattern };
            _linenPatern = new ImageControl() { Image = SandboxMenuComponent.StLinenPattern };
            _shadow = new ImageControl() { Image = SandboxMenuComponent.StShadow };

            _settingsForm = new Control();

            _btGraphic = new ButtonControl
            {
                CustomImage = _stMenuButton,
                CustomImageDown = _stMenuDown,
                CustomImageHover = _stMenuHover,
                TextFontId = 1,
                Text = "Graphic",
                Color = new ByteColor(200,200,200,255)
            };
            _btGraphic.Pressed += delegate { btGraphicPressed(); };

            _btSound = new ButtonControl
            {
                CustomImage = _stMenuButton,
                CustomImageDown = _stMenuDown,
                CustomImageHover = _stMenuHover,
                TextFontId = 1,
                Text = "Sound",
                Color = new ByteColor(200, 200, 200, 255)
            };
            _btSound.Pressed += delegate { btSoundPressed(); };

            _btCoreEngine = new ButtonControl
            {
                CustomImage = _stMenuButton,
                CustomImageDown = _stMenuDown,
                CustomImageHover = _stMenuHover,
                TextFontId = 1,
                Text = "Core Engine",
                Color = new ByteColor(200, 200, 200, 255)
            };
            _btCoreEngine.Pressed += delegate { btCoreEnginePressed(); };

            _btKeyBinding = new ButtonControl
            {
                CustomImage = _stMenuButton,
                CustomImageDown = _stMenuDown,
                CustomImageHover = _stMenuHover,
                TextFontId = 1,
                Text = "Key Binding",
                Color = new ByteColor(200, 200, 200, 255)
            };
            _btKeyBinding.Pressed += delegate { btKeyBindingPressed(); };

            _btBack = new ButtonControl
            {
                CustomImage = _stMenuButton,
                CustomImageDown = _stMenuDown,
                CustomImageHover = _stMenuHover,
                TextFontId = 1,
                Text = "Back",
                Color = new ByteColor(200, 200, 200, 255)
            };
            _btBack.Pressed += delegate { btBackPressed(); };

            //Add components to the left Panel
            _leftMenuPanel.Children.Add(_btBack);
            _leftMenuPanel.Children.Add(_btKeyBinding);
            _leftMenuPanel.Children.Add(_btCoreEngine);
            _leftMenuPanel.Children.Add(_btSound);
            _leftMenuPanel.Children.Add(_btGraphic);
            _leftMenuPanel.Children.Add(_windowLabel);
            _leftMenuPanel.Children.Add(_settingsStateLabel);
            _leftMenuPanel.Children.Add(_shadow);
            _leftMenuPanel.Children.Add(_cubesPatern);
            _leftMenuPanel.Children.Add(_linenPatern);


            _settingsForm.Children.Add(_leftMenuPanel);
        }

        /// <summary>
        /// Load content in thread safe mode (Context usage being not tread safe)
        /// </summary>
        /// <param name="context"></param>
        protected void LoadContentComponent(DeviceContext context)
        {
            _transparentBackGroundTexture = ToDispose(new SpriteTexture(1,1, TextureCreator.GenerateColoredTexture(_engine.Device, context, new ByteColor(100,100,100, 200)), new Vector2(0,0)));
            _backPanel = ToDispose(new ImageControl() { Image = _transparentBackGroundTexture, Name = "BackPanel" });

            _settingsForm.Children.Add(_backPanel);

            UpdateLayout(_engine.ViewPort, _engine.BackBufferTex.Description);
        }

        //Resize, place the components depending of the viewport size
        private void UpdateLayout(Viewport viewport, Texture2DDescription newBackBufferDescr)
        {
            if (Updatable)
            {

                int _headerHeight = (int)(viewport.Height * 0.1f);

                _settingsForm.Bounds = new UniRectangle(0, 0, _engine.ViewPort.Width, _engine.ViewPort.Height);

                _leftMenuPanel.Bounds = new UniRectangle(0, 0, _engine.ViewPort.Width / 4, _engine.ViewPort.Height);

                _windowLabel.Bounds = new UniRectangle(0, 0, _leftMenuPanel.Bounds.Size.X.Offset, _headerHeight);
                _cubesPatern.Bounds = new UniRectangle(0, 0, _leftMenuPanel.Bounds.Size.X.Offset, _headerHeight);
                _linenPatern.Bounds = new UniRectangle(0, _headerHeight, _leftMenuPanel.Bounds.Size.X, viewport.Height - _headerHeight);
                _shadow.Bounds = new UniRectangle(0, _headerHeight - 117, _leftMenuPanel.Bounds.Size.X, 287);
                _backPanel.Bounds = new UniRectangle(_leftMenuPanel.Bounds.Size.X.Offset, 0, _engine.ViewPort.Width * 3 / 4, _engine.ViewPort.Height);

                int btPlacementY = _headerHeight;

                btPlacementY += 20;
                _btGraphic.Bounds = new UniRectangle(5, btPlacementY, _leftMenuPanel.Bounds.Size.X.Offset - 10, 50);
                btPlacementY += 50;
                _btSound.Bounds = new UniRectangle(5, btPlacementY, _leftMenuPanel.Bounds.Size.X.Offset - 10, 50);
                btPlacementY += 50;
                _btCoreEngine.Bounds = new UniRectangle(5, btPlacementY, _leftMenuPanel.Bounds.Size.X.Offset - 10, 50);
                btPlacementY += 50;
                _btKeyBinding.Bounds = new UniRectangle(5, btPlacementY, _leftMenuPanel.Bounds.Size.X.Offset - 10, 50);

                _btBack.Bounds = new UniRectangle(5, new UniScalar(1, -60), _leftMenuPanel.Bounds.Size.X.Offset - 10, 50);
                _settingsStateLabel.Bounds = new UniRectangle(10, new UniScalar(1f, -75.0f), 0, 0);
                if (_graphSettingsPanel != null) _graphSettingsPanel.Resize();
                if (_coreEngineSetingsPanel != null) _coreEngineSetingsPanel.Resize();
            }
        }

        public event EventHandler BackPressed;
        private void btBackPressed()
        {
            if (BackPressed != null) BackPressed(this, EventArgs.Empty);
        }

        private void btGraphicPressed()
        {
            if (_graphSettingsPanel == null) _graphSettingsPanel = new GraphicSettingsPanel(this, ClientSettings.Current.Settings.GraphicalParameters) { Bounds = new UniRectangle(0,0, _backPanel.Bounds.Size.X.Offset, _backPanel.Bounds.Size.Y.Offset) };
            if (_backPanel.Children.Contains(_graphSettingsPanel) == false)
            {
                _backPanel.Children.Clear();
                _backPanel.Children.Add(_graphSettingsPanel);
            }
        }

        private void btSoundPressed()
        {
            if (_soundSettingsPanel == null) _soundSettingsPanel = new SoundSettingsPanel(this, ClientSettings.Current.Settings.SoundParameters) { Bounds = new UniRectangle(0, 0, _backPanel.Bounds.Size.X.Offset, _backPanel.Bounds.Size.Y.Offset) };
            if (_backPanel.Children.Contains(_soundSettingsPanel) == false)
            {
                _backPanel.Children.Clear();
                _backPanel.Children.Add(_soundSettingsPanel);
            }
        }

        private void btCoreEnginePressed()
        {
            if (_coreEngineSetingsPanel == null) _coreEngineSetingsPanel = new CoreEngineSettingsPanel(this, ClientSettings.Current.Settings.EngineParameters) { Bounds = new UniRectangle(0, 0, _backPanel.Bounds.Size.X.Offset, _backPanel.Bounds.Size.Y.Offset) };
            if (_backPanel.Children.Contains(_coreEngineSetingsPanel) == false)
            {
                _backPanel.Children.Clear();
                _backPanel.Children.Add(_coreEngineSetingsPanel);
            }
        }

        private void btKeyBindingPressed()
        {
            if (_keyBindingSettingsPanel == null) _keyBindingSettingsPanel = new KeyBindingSettingsPanel(this, _engine, new UniRectangle(0, 0, _backPanel.Bounds.Size.X.Offset, _backPanel.Bounds.Size.Y.Offset));
            if (_backPanel.Children.Contains(_keyBindingSettingsPanel) == false)
            {
                _backPanel.Children.Clear();
                _backPanel.Children.Add(_keyBindingSettingsPanel);
            }
        }

        private void RefreshComponentsVisibility()
        {
            if (Updatable)
            {
                _screen.Desktop.Children.Add(_settingsForm);
                UpdateLayout(_engine.ViewPort, _engine.BackBufferTex.Description);
            }
            else
            {
                _screen.Desktop.Children.Remove(_settingsForm);
            }
        }
    }
}
