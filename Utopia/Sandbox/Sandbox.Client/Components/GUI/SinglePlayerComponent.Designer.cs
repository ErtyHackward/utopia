using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3CoreComponents.GUI.Nuclex.Controls.Desktop;
using S33M3CoreComponents.Sprites;
using S33M3Resources.Structs;
using SharpDX.Direct3D11;

namespace Sandbox.Client.Components.GUI
{
    public partial class SinglePlayerComponent
    {
        #region Private variables
        protected ButtonControl _btBack;
        protected SpriteTexture _stMenuButton;
        protected SpriteTexture _stMenuHover;
        protected SpriteTexture _stMenuDown;
        #endregion

        #region Public properties/Variables
        public event EventHandler BackPressed;
        #endregion

        #region Public methods
        #endregion

        #region Private methods
        protected void InitializeComponent()
        {
            _stMenuButton = ToDispose(new SpriteTexture(_engine.Device, @"Images\MainMenu\menu_button.png", new Vector2I()));
            _stMenuHover = ToDispose(new SpriteTexture(_engine.Device, @"Images\MainMenu\menu_button_hover.png", new Vector2I()));
            _stMenuDown = ToDispose(new SpriteTexture(_engine.Device, @"Images\MainMenu\menu_button_down.png", new Vector2I()));

            _btBack = new ButtonControl
            {
                CustomImage = _stMenuButton,
                CustomImageDown = _stMenuDown,
                CustomImageHover = _stMenuHover,
                TextFontId = 1,
                Text = "Back",
                Color = new ByteColor(200, 200, 200, 255)
            };
            _btBack.Pressed += delegate { OnBackPressed(); };

            UpdateLayout(_engine.ViewPort, _engine.BackBufferTex.Description);
        }

        private void OnBackPressed()
        {
            if (BackPressed != null) BackPressed(this, EventArgs.Empty);
        }

        private void UpdateLayout(Viewport viewport, Texture2DDescription newBackBufferDescr)
        {
            if (Updatable)
            {



            }
        }
        #endregion
    }
}
