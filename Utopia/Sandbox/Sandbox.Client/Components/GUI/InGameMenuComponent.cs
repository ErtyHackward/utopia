using System;
using System.Drawing;
using S33M3CoreComponents.Sprites;
using SharpDX.Direct3D11;
using S33M3DXEngine;
using S33M3CoreComponents.GUI.Nuclex;
using S33M3CoreComponents.GUI.Nuclex.Controls.Desktop;
using S33M3CoreComponents.GUI.Nuclex.Controls;
using S33M3CoreComponents.Inputs;

namespace Sandbox.Client.Components.GUI
{
    public class InGameMenuComponent : SandboxMenuComponent
    {
        private readonly D3DEngine _engine;
        private readonly MainScreen _screen;
        private readonly RuntimeVariables _runtime;
        private readonly InputsManager _inputmnger;

        private SpriteTexture _stMenuButton;
        private SpriteTexture _stMenuHover;
        private SpriteTexture _stMenuDown;

        private SpriteTexture _stLabelContinue;
        private SpriteTexture _stLabelCredits;
        private SpriteTexture _stLabelExit;
        private SpriteTexture _stLabelSettings;

        private ButtonControl _continueButton;
        private ButtonControl _settingsButton;
        private ButtonControl _exitButton;

        private Control _buttonsGroup;

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

        public InGameMenuComponent(D3DEngine engine, MainScreen screen, RuntimeVariables runtime, SandboxCommonResources commonResources, InputsManager inputmnger)
            : base(engine, screen, commonResources)
        {
            if (engine == null) throw new ArgumentNullException("engine");
            if (screen == null) throw new ArgumentNullException("screen");
            _engine = engine;
            _screen = screen;
            _runtime = runtime;
            _inputmnger = inputmnger;

            _engine.ViewPort_Updated += UpdateLayout;

            _stMenuButton   = ToDispose(SandboxCommonResources.LoadTexture(engine, "Images\\MainMenu\\menu_button.png"));
            _stMenuHover    = ToDispose(SandboxCommonResources.LoadTexture(engine, "Images\\MainMenu\\menu_button_hover.png"));
            _stMenuDown     = ToDispose(SandboxCommonResources.LoadTexture(engine, "Images\\MainMenu\\menu_button_down.png"));

            _stLabelContinue        = ToDispose(SandboxCommonResources.LoadTexture(engine, "Images\\MainMenu\\main_menu_label_continue.png"));
            _stLabelCredits         = ToDispose(SandboxCommonResources.LoadTexture(engine, "Images\\MainMenu\\main_menu_label_credits.png"));
            _stLabelExit            = ToDispose(SandboxCommonResources.LoadTexture(engine, "Images\\MainMenu\\main_menu_label_exit.png"));
            _stLabelSettings        = ToDispose(SandboxCommonResources.LoadTexture(engine, "Images\\MainMenu\\main_menu_label_settings.png"));

            this._borderOffset = 10;
        }

        public override void BeforeDispose()
        {
            _engine.ViewPort_Updated -= UpdateLayout;
        }

        public override void Initialize()
        {
            base.Initialize();

            _buttonsGroup = new Control();

            const int buttonWidth = 212;
            const int buttomHeight = 40;
            
            _continueButton = new ButtonControl{ 
                CustomImage = _stMenuButton, 
                CustomImageDown = _stMenuDown, 
                CustomImageHover = _stMenuHover,
                CusomImageLabel = _stLabelContinue,
                Bounds = new UniRectangle(0, 0, buttonWidth, buttomHeight)
            };
            //_continueButton.Enabled = false;
            _continueButton.Pressed += delegate { OnContinuePressed(); };

            _settingsButton = new ButtonControl
            {
                CustomImage = _stMenuButton,
                CustomImageDown = _stMenuDown,
                CustomImageHover = _stMenuHover,
                CusomImageLabel = _stLabelSettings,
                Bounds = new UniRectangle(0, 0, buttonWidth, buttomHeight)

            };
            _settingsButton.Pressed += delegate { OnSettingsButtonPressed(); };

            _exitButton = new ButtonControl
            {
                CustomImage = _stMenuButton,
                CustomImageDown = _stMenuDown,
                CustomImageHover = _stMenuHover,
                CusomImageLabel = _stLabelExit,
                Bounds = new UniRectangle(0, 0, buttonWidth, buttomHeight)
            };
            _exitButton.Pressed += delegate { OnExitPressed(); };
            
            _buttonsGroup.Children.Add(_continueButton);
            _buttonsGroup.Children.Add(_settingsButton);
            _buttonsGroup.Children.Add(_exitButton);
            _buttonsGroup.ControlsSpacing = new SharpDX.Vector2(0, 0);

            _buttonsGroup.UpdateLayout();

            UpdateLayout(_engine.ViewPort, _engine.BackBufferTex.Description);
            
            if (Updatable)
            {
                //_screen.Desktop.Children.Add(_buttonsGroup);
                //_screen.FocusedControl = _multiplayer;
            }
        }

        public override void EnableComponent()
        {
            //Hide all currently existing components
            _screen.HideAll();

            _screen.Desktop.Children.Add(_buttonsGroup);
            UpdateLayout(_engine.ViewPort, _engine.BackBufferTex.Description);
            base.EnableComponent();
        }

        public override void DisableComponent()
        {
            _screen.Desktop.Children.Remove(_buttonsGroup);
            base.DisableComponent();
        }

        private void UpdateLayout(Viewport viewport, Texture2DDescription newBackBufferDescr)
        {
            _buttonsGroup.Bounds  = new UniRectangle((_engine.ViewPort.Width - 212) / 2, _headerHeight + 137, 212, 400);
        }
    }
}
