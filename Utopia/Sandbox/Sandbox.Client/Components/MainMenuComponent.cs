using System;
using SharpDX.Direct3D11;
using S33M3DXEngine.Main;
using S33M3DXEngine;
using S33M3CoreComponents.GUI.Nuclex;
using S33M3CoreComponents.GUI.Nuclex.Controls.Desktop;
using S33M3CoreComponents.GUI.Nuclex.Controls;

namespace Sandbox.Client.Components
{
    public class MainMenuComponent : GameComponent
    {
        private readonly D3DEngine _engine;
        private readonly MainScreen _screen;

        private ButtonControl _continueButton;
        private ButtonControl _singlePlayer;
        private ButtonControl _multiplayer;
        private ButtonControl _editor;
        private ButtonControl _credits;
        private ButtonControl _exitButton;

        private Control _buttonsGroup;

        #region Events
        public event EventHandler ContinuePressed;

        private void OnContinuePressed()
        {
            var handler = ContinuePressed;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public event EventHandler SinglePlayerPressed;

        private void OnSinglePlayerPressed()
        {
            var handler = SinglePlayerPressed;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public event EventHandler MultiplayerPressed;

        private void OnMultiplayerPressed()
        {
            var handler = MultiplayerPressed;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public event EventHandler EditorPressed;

        private void OnEditorPressed()
        {
            var handler = EditorPressed;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public event EventHandler CreditsPressed;

        private void OnCreditsPressed()
        {
            var handler = CreditsPressed;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public event EventHandler ExitPressed;

        private void OnExitPressed()
        {
            var handler = ExitPressed;
            if (handler != null) handler(this, EventArgs.Empty);
        }
        #endregion

        public MainMenuComponent(D3DEngine engine, MainScreen screen)
        {
            if (engine == null) throw new ArgumentNullException("engine");
            if (screen == null) throw new ArgumentNullException("screen");
            _engine = engine;
            _screen = screen;

            _engine.ViewPort_Updated += UpdateLayout;
        }

        public override void Dispose()
        {
            _engine.ViewPort_Updated -= UpdateLayout;
        }

        public override void Initialize()
        {
            _buttonsGroup = new Control();

            UpdateLayout(_engine.ViewPort);

            _continueButton = CreateButton("Continue", 0);
            _continueButton.Enabled = false;
            _continueButton.Pressed += delegate { OnContinuePressed(); };

            _singlePlayer = CreateButton("Single player", 30);
            _singlePlayer.Pressed += delegate { OnSinglePlayerPressed(); };

            _multiplayer = CreateButton("Multiplayer", 60);
            _multiplayer.Pressed += delegate { OnMultiplayerPressed(); };

            _editor = CreateButton("Editor", 90);
            _editor.Pressed += delegate { OnEditorPressed(); };

            _credits = CreateButton("Credits", 120);
            _credits.Pressed += delegate { OnCreditsPressed(); };

            _exitButton = CreateButton("Exit", 150);
            _exitButton.Pressed += delegate { OnExitPressed(); };

            _buttonsGroup.Children.Add(_continueButton);
            _buttonsGroup.Children.Add(_singlePlayer);
            _buttonsGroup.Children.Add(_multiplayer);
            _buttonsGroup.Children.Add(_editor);
            _buttonsGroup.Children.Add(_credits);
            _buttonsGroup.Children.Add(_exitButton);

            if (Updatable)
            {
                _screen.Desktop.Children.Add(_buttonsGroup);
                _screen.FocusedControl = _multiplayer;
            }
        }

        protected override void OnEnabledChanged(object sender, EventArgs args)
        {
            if (!IsInitialized) return;

            if (Updatable)
            {
                _screen.Desktop.Children.Add(_buttonsGroup);
                UpdateLayout(_engine.ViewPort);

                //_screen.FocusedControl = _multiplayer;

            }
            else
            {
                _screen.Desktop.Children.Remove(_buttonsGroup);
            }
            base.OnEnabledChanged(sender, args);
        }


        private ButtonControl CreateButton(string text, int position)
        {
            return new ButtonControl { Text = text, Bounds = new UniRectangle(0, position, 120, 24) };
        }

        private void UpdateLayout(Viewport viewport)
        {
            if(Updatable)
                _buttonsGroup.Bounds = new UniRectangle(_engine.ViewPort.Width - 200, _engine.ViewPort.Height - 230, 200, 200);
        }
    }
}
