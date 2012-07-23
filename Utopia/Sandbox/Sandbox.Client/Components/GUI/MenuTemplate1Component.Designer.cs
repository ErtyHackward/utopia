using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX.Direct3D11;
using S33M3CoreComponents.GUI.Nuclex.Controls.Desktop;
using S33M3CoreComponents.Sprites;
using S33M3Resources.Structs;
using S33M3CoreComponents.GUI.Nuclex.Controls;
using S33M3CoreComponents.GUI.Nuclex.Visuals.Flat;
using S33M3CoreComponents.GUI.Nuclex.Controls.Arcade;
using SharpDX;
using S33M3CoreComponents.GUI.Nuclex;

namespace Sandbox.Client.Components.GUI
{
    public partial class MenuTemplate1Component
    {
        #region Private variables
        protected Control _form;

        protected LabelControl _windowLabel;

        protected ButtonControl _btBack;

        protected SpriteTexture _stMenuButton;
        protected SpriteTexture _stMenuHover;
        protected SpriteTexture _stMenuDown;

        protected PanelControl _leftMenuPanel;
        protected ImageControl _cubesPatern;
        protected ImageControl _linenPatern;
        protected ImageControl _backPanel;
        protected ImageControl _shadow;
        #endregion

        #region Public variables/Properties
        public event EventHandler BackPressed;
        #endregion

        #region Private methods
        
        private void btBackPressed()
        {
            if (BackPressed != null) BackPressed(this, EventArgs.Empty);
        }

        private void InitializeComponentInternal()
        {
            _stMenuButton = ToDispose(new SpriteTexture(_engine.Device, @"Images\MainMenu\menu_button.png", new Vector2I()));
            _stMenuHover = ToDispose(new SpriteTexture(_engine.Device, @"Images\MainMenu\menu_button_hover.png", new Vector2I()));
            _stMenuDown = ToDispose(new SpriteTexture(_engine.Device, @"Images\MainMenu\menu_button_down.png", new Vector2I()));

            _form = new Control();

            _windowLabel = new LabelControl()
            {
                Text = "??????",
                Color = new ByteColor(198, 0, 75),
                CustomFont = CommonResources.FontBebasNeue35,
                CustomHorizontalPlacement = FlatGuiGraphics.Frame.HorizontalTextAlignment.Center,
                CustomVerticalPlacement = FlatGuiGraphics.Frame.VerticalTextAlignment.Center
            };

            _leftMenuPanel = new PanelControl() { Color = Color.WhiteSmoke };
            _cubesPatern = new ImageControl() { Image = CommonResources.StCubesPattern };
            _linenPatern = new ImageControl() { Image = CommonResources.StLinenPattern };
            _shadow = new ImageControl() { Image = CommonResources.StShadow };
            _backPanel = new ImageControl() { Image = CommonResources.StLinenPattern };

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

            InitializeComponent();
        }

        protected virtual void InitializeComponent()
        {
        }

        private void RefreshComponentsVisibility()
        {
            if (Updatable)
            {
                _screen.Desktop.Children.Add(_form);
                UpdateLayoutInternal(_engine.ViewPort, _engine.BackBufferTex.Description);
            }
            else
            {
                _screen.Desktop.Children.Remove(_form);
            }
        }

        //Resize, place the components depending of the viewport size
        private void UpdateLayoutInternal(Viewport viewport, Texture2DDescription newBackBufferDescr)
        {
            if (Updatable)
            {
                int _headerHeight = (int)(viewport.Height * 0.1f);

                _form.Bounds = new UniRectangle(0, 0, _engine.ViewPort.Width, _engine.ViewPort.Height);
                _leftMenuPanel.Bounds = new UniRectangle(0, 0, _engine.ViewPort.Width / 4, _engine.ViewPort.Height);

                _windowLabel.Bounds = new UniRectangle(0, 0, _leftMenuPanel.Bounds.Size.X.Offset, _headerHeight);
                _cubesPatern.Bounds = new UniRectangle(0, 0, _leftMenuPanel.Bounds.Size.X.Offset, _headerHeight);
                _linenPatern.Bounds = new UniRectangle(0, _headerHeight, _leftMenuPanel.Bounds.Size.X, viewport.Height - _headerHeight);
                _shadow.Bounds = new UniRectangle(0, _headerHeight - 117, _leftMenuPanel.Bounds.Size.X, 287);
                _backPanel.Bounds = new UniRectangle(_leftMenuPanel.Bounds.Size.X.Offset, 0, _engine.ViewPort.Width * 3 / 4, _engine.ViewPort.Height);
                _btBack.Bounds = new UniRectangle(5, new UniScalar(1, -60), _leftMenuPanel.Bounds.Size.X.Offset - 10, 50);
            }

            UpdateLayout(viewport, newBackBufferDescr);
        }

        protected virtual void UpdateLayout(Viewport viewport, Texture2DDescription newBackBufferDescr)
        {

        }

        #endregion

        #region Public methods
        #endregion
    }
}
