using System;
using S33M3CoreComponents.Sprites2D;
using S33M3DXEngine.Main;
using SharpDX.Direct3D11;
using S33M3DXEngine;
using S33M3CoreComponents.GUI.Nuclex;
using S33M3CoreComponents.GUI.Nuclex.Controls.Desktop;
using Utopia.GUI.Inventory;
using SharpDX;

namespace Realms.Client.Components.GUI
{
    public class InGameMenuComponent : DrawableGameComponent
    {
        private readonly D3DEngine _engine;
        private readonly MainScreen _screen;
        private readonly SandboxCommonResources _commonResources;
        
        private readonly SpriteTexture _stLabelContinue;
        private readonly SpriteTexture _stLabelExit;
        private readonly SpriteTexture _stLabelSettings;
        private readonly SpriteTexture _stMenuBg;

        private ButtonControl _continueButton;
        private ButtonControl _settingsButton;
        private ButtonControl _exitButton;

        private ContainerControl _buttonsGroup;

        #region Events
        public event EventHandler ContinuePressed;
        private void OnContinuePressed()
        {
            if (ContinuePressed != null) ContinuePressed(this, EventArgs.Empty);
        }

        public event EventHandler SettingsButtonPressed;
        private void OnSettingsButtonPressed()
        {
            if (SettingsButtonPressed != null) SettingsButtonPressed(this, EventArgs.Empty);
        }

        public event EventHandler ExitPressed;
        private void OnExitPressed()
        {
            if (ExitPressed != null) ExitPressed(this, EventArgs.Empty);
        }
        #endregion

        public InGameMenuComponent(D3DEngine engine, MainScreen screen, SandboxCommonResources commonResources)
        {
            if (engine == null) throw new ArgumentNullException("engine");
            if (screen == null) throw new ArgumentNullException("screen");

            _engine = engine;
            _screen = screen;
            _commonResources = commonResources;
            
            _engine.ScreenSize_Updated += UpdateLayout;

            _stMenuBg               = ToDispose(SandboxCommonResources.LoadTexture(engine, "Images\\gameplay_menu.png"));
            _stLabelContinue        = ToDispose(SandboxCommonResources.LoadTexture(engine, "Images\\MainMenu\\main_menu_label_continue.png"));
            _stLabelSettings        = ToDispose(SandboxCommonResources.LoadTexture(engine, "Images\\MainMenu\\main_menu_label_settings.png"));
            _stLabelExit            = ToDispose(SandboxCommonResources.LoadTexture(engine, "Images\\MainMenu\\main_menu_label_exit.png"));
        }

        public override void BeforeDispose()
        {
            _engine.ScreenSize_Updated -= UpdateLayout;
        }

        public override void Initialize()
        {
            base.Initialize();

            _buttonsGroup = new ContainerControl { Bounds = new UniRectangle(0, 0, 417, 355), Background = _stMenuBg, DisplayBackground = true };

            const int buttonWidth = 212;
            const int buttomHeight = 40;
            
            _continueButton = new ButtonControl{ 
                CustomImage = _commonResources.StButtonBackground, 
                CustomImageDown = _commonResources.StButtonBackgroundDown, 
                CustomImageHover = _commonResources.StButtonBackgroundHover,
                CusomImageLabel = _stLabelContinue,
                Bounds = new UniRectangle(0, 0, buttonWidth, buttomHeight)
            };
            _continueButton.Pressed += delegate { OnContinuePressed(); };

            _settingsButton = new ButtonControl
            {
                CustomImage = _commonResources.StButtonBackground,
                CustomImageDown = _commonResources.StButtonBackgroundDown,
                CustomImageHover = _commonResources.StButtonBackgroundHover,
                CusomImageLabel = _stLabelSettings,
                Bounds = new UniRectangle(0, 0, buttonWidth, buttomHeight)

            };
            _settingsButton.Pressed += delegate { OnSettingsButtonPressed(); };

            _exitButton = new ButtonControl
            {
                CustomImage = _commonResources.StButtonBackground,
                CustomImageDown = _commonResources.StButtonBackgroundDown,
                CustomImageHover = _commonResources.StButtonBackgroundHover,
                CusomImageLabel = _stLabelExit,
                Bounds = new UniRectangle(0, 0, buttonWidth, buttomHeight)
            };
            _exitButton.Pressed += delegate { OnExitPressed(); };
            
            _buttonsGroup.Children.Add(_continueButton);
            _buttonsGroup.Children.Add(_settingsButton);
            _buttonsGroup.Children.Add(_exitButton);
            _buttonsGroup.LeftTopMargin = new SharpDX.Vector2(105, 141);
            _buttonsGroup.ControlsSpacing = new SharpDX.Vector2(0, 0);
            
            UpdateLayout(_engine.ViewPort, _engine.BackBufferTex.Description);
        }

        public override void EnableComponent(bool forced)
        {
            if (!AutoStateEnabled && !forced) return;

            //Hide all currently existing components
            _screen.HideAll();
            _screen.Desktop.Children.Add(_buttonsGroup);
            _buttonsGroup.UpdateLayout();
            UpdateLayout(_engine.ViewPort, _engine.BackBufferTex.Description);
            base.EnableComponent();
        }

        public override void DisableComponent()
        {
            _screen.Desktop.Children.Remove(_buttonsGroup);
            base.DisableComponent();
        }

        private void UpdateLayout(ViewportF viewport, Texture2DDescription newBackBufferDescr)
        {
            _buttonsGroup.Bounds.Location = new UniVector((_engine.ViewPort.Width - _buttonsGroup.Bounds.Size.X.Offset) / 2, (_engine.ViewPort.Height - _buttonsGroup.Bounds.Size.Y.Offset) / 2);
        }
    }
}
