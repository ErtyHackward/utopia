using System;
using S33M3CoreComponents.GUI.Nuclex.Controls;
using S33M3CoreComponents.GUI.Nuclex.Visuals.Flat;
using S33M3CoreComponents.Sprites2D;
using S33M3Resources.Structs.Helpers;
using SharpDX.Direct3D11;
using S33M3DXEngine.Main;
using S33M3DXEngine;
using S33M3CoreComponents.GUI.Nuclex;
using S33M3CoreComponents.GUI.Nuclex.Controls.Desktop;
using SharpDX;

namespace Realms.Client.Components.GUI
{
    public class ServerSelectionComponent : SandboxMenuComponent
    {
        private readonly D3DEngine _engine;
        private readonly MainScreen _screen;
        private readonly SandboxCommonResources _commonResources;

        private ButtonControl _backButton;
        private ButtonControl _connectButton;
        private ListControl _serverList;
        private LabelControl _serversLabel;
        private LabelControl _serverDescriptionLabel;

        private SpriteTexture _stLabelConnect;

        public ListControl List
        {
            get { return _serverList; }
        }

        public string Description {
            set { _serverDescriptionLabel.Text = value; }
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


        public ServerSelectionComponent(D3DEngine engine, MainScreen screen, SandboxCommonResources commonResources)
            : base(engine, screen, commonResources)
        {
            if (engine == null) throw new ArgumentNullException("engine");
            if (screen == null) throw new ArgumentNullException("screen");
            _engine = engine;
            _screen = screen;
            _commonResources = commonResources;

            _engine.ScreenSize_Updated += UpdateLayout;
        }

        public override void Initialize()
        {
            base.Initialize();

            _stLabelConnect = ToDispose(SandboxCommonResources.LoadTexture(_engine, "Images\\connect_label.png"));

            _serversLabel = new LabelControl
            {
                Text = "Servers:",
                Color = ColorHelper.ToColor4(System.Drawing.Color.White),
                CustomFont = _commonResources.FontBebasNeue25
            };

            _serverDescriptionLabel = new LabelControl {
                Color = ColorHelper.ToColor4(System.Drawing.Color.White), 
                CustomVerticalPlacement = FlatGuiGraphics.Frame.VerticalTextAlignment.Top 
            };

            _backButton = new ButtonControl
                              {
                                  Text = "Back",
                                  CustomImage = _commonResources.StButtonBackground,
                                  CustomImageHover = _commonResources.StButtonBackgroundHover,
                                  CustomImageDown = _commonResources.StButtonBackgroundDown,
                                  CustomImageDisabled = _commonResources.StButtonBackground,
                                  CusomImageLabel = _commonResources.StBackLabel
                              };
            _backButton.Pressed += delegate { OnBackPressed(); };

            _connectButton = new ButtonControl
                                 {
                                     Text = "Connect",
                                     Enabled = false,
                                     CustomImage = _commonResources.StButtonBackground,
                                     CustomImageHover = _commonResources.StButtonBackgroundHover,
                                     CustomImageDown = _commonResources.StButtonBackgroundDown,
                                     CusomImageLabel = _stLabelConnect
                                 };
            _connectButton.Pressed += delegate { OnConnectPressed(); };

            _serverList = new ListControl { Bounds = new UniRectangle(100, 100, 400, 400), SelectionMode = ListSelectionMode.Single };
            _serverList.SelectionChanged += ServerListSelectionChanged;

            UpdateLayout(_engine.ViewPort, _engine.BackBufferTex.Description);            
        }

        void ServerListSelectionChanged(object sender, EventArgs e)
        {
            _connectButton.Enabled = _serverList.SelectedItems.Count > 0;
        }

        public override void EnableComponent(bool forced)
        {
            if (!AutoStateEnabled && !forced) return;

            _screen.Desktop.Children.Add(_serverList);
            _screen.Desktop.Children.Add(_connectButton);
            _screen.Desktop.Children.Add(_backButton);
            _screen.Desktop.Children.Add(_serversLabel);
            _screen.Desktop.Children.Add(_serverDescriptionLabel);
            UpdateLayout(_engine.ViewPort, _engine.BackBufferTex.Description);

            base.EnableComponent(forced);
        }

        public override void DisableComponent()
        {
            _screen.Desktop.Children.Remove(_serverList);
            _screen.Desktop.Children.Remove(_connectButton);
            _screen.Desktop.Children.Remove(_backButton);
            _screen.Desktop.Children.Remove(_serversLabel);
            _screen.Desktop.Children.Remove(_serverDescriptionLabel);

            base.DisableComponent();
        }

        private void UpdateLayout(ViewportF viewport, Texture2DDescription newBackBufferDescr)
        {
            _serversLabel.Bounds = new UniRectangle(200, _headerHeight + 107, 200, 20);
            _serverList.Bounds = new UniRectangle(200, _headerHeight + 137, 400, _engine.ViewPort.Height - _headerHeight - 200);
            _connectButton.Bounds = new UniRectangle(_engine.ViewPort.Width - 300, _engine.ViewPort.Height - 140, 212, 40);
            _backButton.Bounds = new UniRectangle(_engine.ViewPort.Width - 300, _engine.ViewPort.Height - 100, 212, 40);
            _serverDescriptionLabel.Bounds = new UniRectangle(620, _headerHeight + 137, 300, 100);
        }
    }
}
