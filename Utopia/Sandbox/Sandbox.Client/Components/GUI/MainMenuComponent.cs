using System;
using System.Drawing;
using S33M3CoreComponents.Sprites2D;
using S33M3Resources.Structs.Helpers;
using SharpDX.Direct3D11;
using S33M3DXEngine;
using S33M3CoreComponents.GUI.Nuclex;
using S33M3CoreComponents.GUI.Nuclex.Controls.Desktop;
using S33M3CoreComponents.GUI.Nuclex.Controls;
using SharpDX;
using Color = System.Drawing.Color;

namespace Sandbox.Client.Components.GUI
{
    public class MainMenuComponent : SandboxMenuComponent
    {
        private readonly D3DEngine _engine;
        private readonly MainScreen _screen;
        private readonly SandboxRuntimeVariables _runtime;

        private SpriteTexture _stLabelCredits;
        private SpriteTexture _stLabelExit;
        private SpriteTexture _stLabelSingleplayer;
        private SpriteTexture _stLabelSettings;
        private SpriteTexture _stMainMenuLabel;

        private ButtonControl _singlePlayer;
        private ButtonControl _settingsButton;
        private ButtonControl _credits;
        private ButtonControl _exitButton;
        private ImageControl _mainMenuLabel;
        private LabelControl _helloLabel;
        private LabelControl _nicknameLabel;

        private Control _buttonsGroup;

        #region Events
        public event EventHandler ContinuePressed;
        private void OnContinuePressed()
        {
            if (ContinuePressed != null) ContinuePressed(this, EventArgs.Empty);
        }

        public event EventHandler SinglePlayerPressed;
        private void OnSinglePlayerPressed()
        {
            if (SinglePlayerPressed != null) SinglePlayerPressed(this, EventArgs.Empty);
        }

        public event EventHandler SettingsButtonPressed;
        private void OnSettingsButtonPressed()
        {
            if (SettingsButtonPressed != null) SettingsButtonPressed(this, EventArgs.Empty);
        }

        public event EventHandler MultiplayerPressed;
        private void OnMultiplayerPressed()
        {
            if (MultiplayerPressed != null) MultiplayerPressed(this, EventArgs.Empty);
        }

        public event EventHandler EditorPressed;
        private void OnEditorPressed()
        {
            if (EditorPressed != null) EditorPressed(this, EventArgs.Empty);
        }

        public event EventHandler CreditsPressed;

        private void OnCreditsPressed()
        {
            if (CreditsPressed != null) CreditsPressed(this, EventArgs.Empty);
        }

        public event EventHandler ExitPressed;
        private void OnExitPressed()
        {
            if (ExitPressed != null) ExitPressed(this, EventArgs.Empty);
        }
        #endregion

        public MainMenuComponent(D3DEngine engine, MainScreen screen, SandboxRuntimeVariables runtime, SandboxCommonResources commonResources)
            : base(engine, screen, commonResources)
        {
            if (engine == null) throw new ArgumentNullException("engine");
            if (screen == null) throw new ArgumentNullException("screen");
            _engine = engine;
            _screen = screen;
            _runtime = runtime;

            _engine.ViewPort_Updated += UpdateLayout;
            
            _stLabelCredits         = ToDispose(SandboxCommonResources.LoadTexture(engine, "Images\\MainMenu\\main_menu_label_credits.png"));
            _stLabelExit            = ToDispose(SandboxCommonResources.LoadTexture(engine, "Images\\MainMenu\\main_menu_label_exit.png"));
            _stLabelSingleplayer    = ToDispose(SandboxCommonResources.LoadTexture(engine, "Images\\MainMenu\\main_menu_label_singleplayer.png"));
            _stLabelSettings        = ToDispose(SandboxCommonResources.LoadTexture(engine, "Images\\MainMenu\\main_menu_label_settings.png"));
            _stMainMenuLabel        = ToDispose(SandboxCommonResources.LoadTexture(engine, "Images\\MainMenu\\main_menu.png"));
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
            
            _singlePlayer = new ButtonControl
            {
                CustomImage = _commonResources.StButtonBackground,
                CustomImageDown = _commonResources.StButtonBackgroundDown,
                CustomImageHover = _commonResources.StButtonBackgroundHover,
                CusomImageLabel = _stLabelSingleplayer,
                Bounds = new UniRectangle(0, 0, buttonWidth, buttomHeight)
            };
            _singlePlayer.Pressed += delegate { OnSinglePlayerPressed(); };

            _settingsButton = new ButtonControl
            {
                CustomImage = _commonResources.StButtonBackground,
                CustomImageDown = _commonResources.StButtonBackgroundDown,
                CustomImageHover = _commonResources.StButtonBackgroundHover,
                CusomImageLabel = _stLabelSettings,
                Bounds = new UniRectangle(0, 0, buttonWidth, buttomHeight)

            };
            _settingsButton.Pressed += delegate { OnSettingsButtonPressed(); };

            _credits = new ButtonControl
            {
                CustomImage = _commonResources.StButtonBackground,
                CustomImageDown = _commonResources.StButtonBackgroundDown,
                CustomImageHover = _commonResources.StButtonBackgroundHover,
                CusomImageLabel = _stLabelCredits,
                Bounds = new UniRectangle(0, 0, buttonWidth, buttomHeight)
            };
            _credits.Pressed += delegate { OnCreditsPressed(); };

            _exitButton = new ButtonControl
            {
                CustomImage = _commonResources.StButtonBackground,
                CustomImageDown = _commonResources.StButtonBackgroundDown,
                CustomImageHover = _commonResources.StButtonBackgroundHover,
                CusomImageLabel = _stLabelExit,
                Bounds = new UniRectangle(0, 0, buttonWidth, buttomHeight)
            };
            _exitButton.Pressed += delegate { OnExitPressed(); };
            
            _buttonsGroup.Children.Add(_singlePlayer);
            _buttonsGroup.Children.Add(_settingsButton);
            _buttonsGroup.Children.Add(_credits);
            _buttonsGroup.Children.Add(_exitButton);
            _buttonsGroup.ControlsSpacing = new SharpDX.Vector2(0, 0);

            _buttonsGroup.UpdateLayout();

            _helloLabel = new LabelControl { 
                Text = "HELLO",
                Color = ColorHelper.ToColor4(Color.FromArgb(198, 0, 75)),
                CustomFont = _commonResources.FontBebasNeue25
            };
            
            _nicknameLabel = new LabelControl {
                Color = SharpDX.Color.White,
                CustomFont = _commonResources.FontBebasNeue25
            };

            _mainMenuLabel = new ImageControl { Image = _stMainMenuLabel };

            UpdateLayout(_engine.ViewPort, _engine.BackBufferTex.Description);
        }

        public override void EnableComponent(bool forced)
        {
            if (!AutoStateEnabled && !forced) return;

            _nicknameLabel.Text = _runtime.DisplayName;
            _screen.Desktop.Children.Add(_helloLabel);
            _screen.Desktop.Children.Add(_nicknameLabel);
            _screen.Desktop.Children.Add(_buttonsGroup);
            _screen.Desktop.Children.Add(_mainMenuLabel);
            UpdateLayout(_engine.ViewPort, _engine.BackBufferTex.Description);
            base.EnableComponent(forced);
        }

        public override void DisableComponent()
        {
            _screen.Desktop.Children.Remove(_helloLabel);
            _screen.Desktop.Children.Remove(_nicknameLabel);
            _screen.Desktop.Children.Remove(_buttonsGroup);
            _screen.Desktop.Children.Remove(_mainMenuLabel);
            base.DisableComponent();
        }

        private void UpdateLayout(ViewportF viewport, Texture2DDescription newBackBufferDescr)
        {
            _helloLabel.Bounds    = new UniRectangle((_engine.ViewPort.Width - 212) / 2 - 250, _headerHeight + 90, 50, 40);
            _nicknameLabel.Bounds = new UniRectangle((_engine.ViewPort.Width - 212) / 2 - 195, _headerHeight + 90, 200, 40);
            _mainMenuLabel.Bounds = new UniRectangle((_engine.ViewPort.Width - 212) / 2 + 65, _headerHeight + 96, 85, 50);
            _buttonsGroup.Bounds  = new UniRectangle((_engine.ViewPort.Width - 212) / 2, _headerHeight + 137, 212, 400);
        }
    }
}
