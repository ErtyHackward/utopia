using System;
using SharpDX.Direct3D11;
using S33M3_DXEngine.Main;
using S33M3_DXEngine;
using S33M3_CoreComponents.GUI.Nuclex;
using S33M3_CoreComponents.GUI.Nuclex.Controls.Desktop;

namespace Sandbox.Client.Components
{
    public class ServerSelectionComponent : GameComponent
    {
        private readonly D3DEngine _engine;
        private readonly MainScreen _screen;

        private ButtonControl _backButton;
        private ButtonControl _connectButton;
        private ListControl _serverList;

        public ListControl List
        {
            get { return _serverList; }
        }

        public event EventHandler BackPressed;

        private void OnBackPressed()
        {
            var handler = BackPressed;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public event EventHandler ConnectPressed;

        private void OnConnectPressed()
        {
            var handler = ConnectPressed;
            if (handler != null) handler(this, EventArgs.Empty);
        }


        public ServerSelectionComponent(D3DEngine engine, MainScreen screen)
        {
            if (engine == null) throw new ArgumentNullException("engine");
            if (screen == null) throw new ArgumentNullException("screen");
            _engine = engine;
            _screen = screen;

            _engine.ViewPort_Updated += UpdateLayout;
        }

        public override void Initialize()
        {
            _backButton = new ButtonControl { Text = "Back" };
            _backButton.Pressed += delegate { OnBackPressed(); };

            _connectButton = new ButtonControl { Text = "Connect", Enabled = false };
            _connectButton.Pressed += delegate { OnConnectPressed(); };

            _serverList = new ListControl { Bounds = new UniRectangle(100, 100, 400, 400), SelectionMode = ListSelectionMode.Single };
            _serverList.SelectionChanged += ServerListSelectionChanged;

            UpdateLayout(_engine.ViewPort);

            if (Updatable)
            {
                //_screen.Desktop.Children.Add();
            }
        }

        void ServerListSelectionChanged(object sender, EventArgs e)
        {
            _connectButton.Enabled = _serverList.SelectedItems.Count > 0;
        }

        protected override void OnEnabledChanged(object sender, EventArgs args)
        {
            if (!IsInitialized) return;

            if (Updatable)
            {
                _screen.Desktop.Children.Add(_serverList);
                _screen.Desktop.Children.Add(_connectButton);
                _screen.Desktop.Children.Add(_backButton);
                UpdateLayout(_engine.ViewPort);
            }
            else
            {
                _screen.Desktop.Children.Remove(_serverList);
                _screen.Desktop.Children.Remove(_connectButton);
                _screen.Desktop.Children.Remove(_backButton);
            }

            base.OnEnabledChanged(sender, args);
        }

        private void UpdateLayout(Viewport viewport)
        {
            if (Updatable)
            {
                _connectButton.Bounds = new UniRectangle(_engine.ViewPort.Width - 200, _engine.ViewPort.Height - 100, 120, 24);
                _backButton.Bounds = new UniRectangle(_engine.ViewPort.Width - 200, _engine.ViewPort.Height - 60, 120, 24);
            }
        }
    }
}
