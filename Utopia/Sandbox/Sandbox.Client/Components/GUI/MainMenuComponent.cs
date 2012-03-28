using System;
using System.Drawing;
using S33M3CoreComponents.Sprites;
using SharpDX.Direct3D11;
using S33M3DXEngine;
using S33M3CoreComponents.GUI.Nuclex;
using S33M3CoreComponents.GUI.Nuclex.Controls.Desktop;
using S33M3CoreComponents.GUI.Nuclex.Controls;

namespace Sandbox.Client.Components.GUI
{
    public class MainMenuComponent : SandboxMenuComponent
    {
        private readonly D3DEngine _engine;
        private readonly MainScreen _screen;
        private readonly RuntimeVariables _runtime;

        private SpriteTexture _stMenuButton;
        private SpriteTexture _stMenuHover;
        private SpriteTexture _stMenuDown;

        private SpriteTexture _stLabelContinue;
        private SpriteTexture _stLabelCredits;
        private SpriteTexture _stLabelExit;
        private SpriteTexture _stLabelLogOut;
        private SpriteTexture _stLabelMultiplayer;
        private SpriteTexture _stLabelSingleplayer;
        private SpriteTexture _stLabelSettings;
        private SpriteTexture _stLabelEditor;
        private SpriteTexture _stMainMenuLabel;

        private ButtonControl _continueButton;
        private ButtonControl _singlePlayer;
        private ButtonControl _multiplayer;
        private ButtonControl _settingsButton;
        private ButtonControl _editor;
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

        public MainMenuComponent(D3DEngine engine, MainScreen screen, RuntimeVariables runtime)
            : base(engine, screen)
        {
            if (engine == null) throw new ArgumentNullException("engine");
            if (screen == null) throw new ArgumentNullException("screen");
            _engine = engine;
            _screen = screen;
            _runtime = runtime;

            _engine.ViewPort_Updated += UpdateLayout;

            _stMenuButton   = ToDispose(LoadTexture(engine, "Images\\MainMenu\\menu_button.png"));
            _stMenuHover    = ToDispose(LoadTexture(engine, "Images\\MainMenu\\menu_button_hover.png"));
            _stMenuDown     = ToDispose(LoadTexture(engine, "Images\\MainMenu\\menu_button_down.png"));

            _stLabelContinue        = ToDispose(LoadTexture(engine, "Images\\MainMenu\\main_menu_label_continue.png"));
            _stLabelCredits         = ToDispose(LoadTexture(engine, "Images\\MainMenu\\main_menu_label_credits.png"));
            _stLabelExit            = ToDispose(LoadTexture(engine, "Images\\MainMenu\\main_menu_label_exit.png"));
            _stLabelLogOut          = ToDispose(LoadTexture(engine, "Images\\MainMenu\\main_menu_label_logout.png"));
            _stLabelMultiplayer     = ToDispose(LoadTexture(engine, "Images\\MainMenu\\main_menu_label_multiplayer.png"));
            _stLabelSingleplayer    = ToDispose(LoadTexture(engine, "Images\\MainMenu\\main_menu_label_singleplayer.png"));
            _stLabelSettings        = ToDispose(LoadTexture(engine, "Images\\MainMenu\\main_menu_label_settings.png"));
            _stLabelEditor          = ToDispose(LoadTexture(engine, "Images\\MainMenu\\main_menu_label_editor.png"));
            _stMainMenuLabel        = ToDispose(LoadTexture(engine, "Images\\MainMenu\\main_menu.png"));
        }

        public override void Dispose()
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

            _singlePlayer = new ButtonControl
            {
                CustomImage = _stMenuButton,
                CustomImageDown = _stMenuDown,
                CustomImageHover = _stMenuHover,
                CusomImageLabel = _stLabelSingleplayer,
                Bounds = new UniRectangle(0, 0, buttonWidth, buttomHeight)
            };
            _singlePlayer.Pressed += delegate { OnSinglePlayerPressed(); };

            _multiplayer = new ButtonControl
            {
                CustomImage = _stMenuButton,
                CustomImageDown = _stMenuDown,
                CustomImageHover = _stMenuHover,
                CusomImageLabel = _stLabelMultiplayer,
                Bounds = new UniRectangle(0, 0, buttonWidth, buttomHeight)
            };
            _multiplayer.Pressed += delegate { OnMultiplayerPressed(); };

            _settingsButton = new ButtonControl
            {
                CustomImage = _stMenuButton,
                CustomImageDown = _stMenuDown,
                CustomImageHover = _stMenuHover,
                CusomImageLabel = _stLabelSettings,
                Bounds = new UniRectangle(0, 0, buttonWidth, buttomHeight)

            };
            _settingsButton.Pressed += delegate { OnSettingsButtonPressed(); };

            _editor = new ButtonControl
            {
                CustomImage = _stMenuButton,
                CustomImageDown = _stMenuDown,
                CustomImageHover = _stMenuHover,
                CusomImageLabel = _stLabelEditor,
                Bounds = new UniRectangle(0, 0, buttonWidth, buttomHeight)
            };
            _editor.Pressed += delegate { OnEditorPressed(); };

            _credits = new ButtonControl
            {
                CustomImage = _stMenuButton,
                CustomImageDown = _stMenuDown,
                CustomImageHover = _stMenuHover,
                CusomImageLabel = _stLabelCredits,
                Bounds = new UniRectangle(0, 0, buttonWidth, buttomHeight)
            };
            _credits.Pressed += delegate { OnCreditsPressed(); };

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
            _buttonsGroup.Children.Add(_singlePlayer);
            _buttonsGroup.Children.Add(_multiplayer);
            _buttonsGroup.Children.Add(_settingsButton);
            _buttonsGroup.Children.Add(_editor);
            _buttonsGroup.Children.Add(_credits);
            _buttonsGroup.Children.Add(_exitButton);
            _buttonsGroup.ControlsSpacing = new SharpDX.Vector2(0, 0);

            _buttonsGroup.UpdateLayout();

            _helloLabel = new LabelControl { 
                Text = "HELLO",
                Color = new SharpDX.Color4(Color.FromArgb(198,0,75).ToArgb()),
                CustomFont = FontBebasNeue25
            };
            
            _nicknameLabel = new LabelControl {
                Color = SharpDX.Colors.White,
                CustomFont = FontBebasNeue25
            };

            _mainMenuLabel = new ImageControl { Image = _stMainMenuLabel };

            UpdateLayout(_engine.ViewPort, _engine.BackBufferTex.Description);
            
            if (Updatable)
            {
                //_screen.Desktop.Children.Add(_buttonsGroup);
                //_screen.FocusedControl = _multiplayer;
            }
        }

        public override void EnableComponent()
        {

            _nicknameLabel.Text = _runtime.DisplayName;
            _screen.Desktop.Children.Add(_helloLabel);
            _screen.Desktop.Children.Add(_nicknameLabel);
            _screen.Desktop.Children.Add(_buttonsGroup);
            _screen.Desktop.Children.Add(_mainMenuLabel);
            UpdateLayout(_engine.ViewPort, _engine.BackBufferTex.Description);
            base.EnableComponent();
        }

        public override void DisableComponent()
        {
            _screen.Desktop.Children.Remove(_helloLabel);
            _screen.Desktop.Children.Remove(_nicknameLabel);
            _screen.Desktop.Children.Remove(_buttonsGroup);
            _screen.Desktop.Children.Remove(_mainMenuLabel);
            base.DisableComponent();
        }

        private void UpdateLayout(Viewport viewport, Texture2DDescription newBackBufferDescr)
        {
            _helloLabel.Bounds    = new UniRectangle((_engine.ViewPort.Width - 212) / 2 - 250, _headerHeight + 90, 50, 40);
            _nicknameLabel.Bounds = new UniRectangle((_engine.ViewPort.Width - 212) / 2 - 195, _headerHeight + 90, 200, 40);
            _mainMenuLabel.Bounds = new UniRectangle((_engine.ViewPort.Width - 212) / 2 + 60, _headerHeight + 96, 85, 50);
            _buttonsGroup.Bounds  = new UniRectangle((_engine.ViewPort.Width - 212) / 2, _headerHeight + 137, 212, 400);
        }
    }
}
