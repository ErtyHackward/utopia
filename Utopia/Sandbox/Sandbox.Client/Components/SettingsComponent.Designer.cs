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

namespace Sandbox.Client.Components
{
    partial class SettingsComponent
    {
        //Components
        protected LabelControl _windowLabel;
        protected PanelControl _leftMenuPanel;
        protected ImageControl _cubesPatern;
        protected ImageControl _linenPatern;
        protected ImageControl _shadow;

        protected SpriteTexture _transparentBackGroundTexture;
        protected ImageControl _backGround;

        protected void InitializeComponent()
        {
            _windowLabel = new LabelControl() { Text = "Settings", Color = new ByteColor(198,0,75), CustomFont = SandboxMenuComponent.FontBebasNeue25 };
            _leftMenuPanel = new PanelControl() { Color = Colors.WhiteSmoke };
            _cubesPatern = new ImageControl() { Image = SandboxMenuComponent.StCubesPattern };
            _linenPatern = new ImageControl() { Image = SandboxMenuComponent.StLinenPattern };
            _shadow = new ImageControl() { Image = SandboxMenuComponent.StShadow };

            //Add components to the left Panel
            _leftMenuPanel.Children.Add(_windowLabel);
            _leftMenuPanel.Children.Add(_shadow);
            _leftMenuPanel.Children.Add(_cubesPatern);
            _leftMenuPanel.Children.Add(_linenPatern);
        }

        /// <summary>
        /// Load content in thread safe mode (Context usage being not tread safe)
        /// </summary>
        /// <param name="context"></param>
        protected void LoadContentComponent(DeviceContext context)
        {
            _transparentBackGroundTexture = ToDispose(new SpriteTexture(1,1, TextureCreator.GenerateColoredTexture(_engine.Device, context, new ByteColor(255,255,255, 127)), new Vector2(0,0)));
            _backGround = ToDispose(new ImageControl() { Image = _transparentBackGroundTexture });

            _leftMenuPanel.Children.Add(_backGround);

            UpdateLayout(_engine.ViewPort, _engine.BackBufferTex.Description);
        }

        //Resize, place the components depending of the viewport size
        private void UpdateLayout(Viewport viewport, Texture2DDescription newBackBufferDescr)
        {
            if (Updatable)
            {
                int _headerHeight = (int)(viewport.Height * 0.1f);

                _leftMenuPanel.Bounds = new UniRectangle(0, 0, _engine.ViewPort.Width / 4, _engine.ViewPort.Height);

                _windowLabel.Bounds = new UniRectangle(0, 0, _leftMenuPanel.Bounds.Size.X, _headerHeight);
                _cubesPatern.Bounds = new UniRectangle(0, 0, _leftMenuPanel.Bounds.Size.X, _headerHeight);
                _linenPatern.Bounds = new UniRectangle(0, _headerHeight, _leftMenuPanel.Bounds.Size.X, viewport.Height - _headerHeight);
                _shadow.Bounds = new UniRectangle(0, _headerHeight - 117, _leftMenuPanel.Bounds.Size.X, 287);
                _backGround.Bounds = new UniRectangle(_leftMenuPanel.Bounds.Size.X.Offset, 0, _engine.ViewPort.Width * 3 / 4, _engine.ViewPort.Height);
            }
        }

        private void RefreshComponentsVisibility()
        {
            if (Updatable)
            {
                _screen.Desktop.Children.Add(_leftMenuPanel);
                UpdateLayout(_engine.ViewPort, _engine.BackBufferTex.Description);
            }
            else
            {
                _screen.Desktop.Children.Remove(_leftMenuPanel);
            }
        }
    }
}
