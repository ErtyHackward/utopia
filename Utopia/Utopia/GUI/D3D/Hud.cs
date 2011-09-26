using S33M3Engines;
using S33M3Engines.D3D;
using S33M3Engines.Shared.Sprites;
using S33M3Engines.Sprites;
using SharpDX;
using Nuclex.UserInterface;
using Utopia.GUI.D3D.Inventory;
using SharpDX.Direct3D11;

namespace Utopia.GUI.D3D
{

    /// <summary>
    /// Heads up display = crosshair + toolbar(s) / icons + life + mana + ... 
    /// </summary>
      
    public class Hud : DrawableGameComponent
    {
        private SpriteRenderer _spriteRender;
        private SpriteTexture _crosshair;
        private SpriteFont _font;
        private readonly Screen _screen;
        private readonly D3DEngine _d3DEngine;


        /// <summary>
        /// _toolbarUI is part of the hud 
        /// </summary>
        ToolBarUi _toolbarUi;

        public Hud(Screen screen, D3DEngine d3DEngine)
        {
            _screen = screen;
            _d3DEngine = d3DEngine;
            DrawOrders.UpdateIndex(0, 9000);
            _d3DEngine.ViewPort_Updated += D3dEngine_ViewPort_Updated;
        }

        public override void LoadContent()
        {
            _crosshair = new SpriteTexture(_d3DEngine.Device, @"Textures\Gui\Crosshair.png", ref _d3DEngine.ViewPort_Updated, _d3DEngine.ViewPort);

            _spriteRender = new SpriteRenderer();
            _spriteRender.Initialize(_d3DEngine);

            _font = new SpriteFont();
            _font.Initialize("Segoe UI Mono", 13f, System.Drawing.FontStyle.Regular, true, _d3DEngine.Device);

            _toolbarUi = new ToolBarUi();
            _toolbarUi.Bounds = new UniRectangle(0.0f, _d3DEngine.ViewPort.Height - 46, _d3DEngine.ViewPort.Width, 80.0f);
            _screen.Desktop.Children.Add(_toolbarUi);
            //the guimanager will draw the GUI screen, not the Hud !
        }

        //Refresh Sprite Centering when the viewPort size change !
        private void D3dEngine_ViewPort_Updated(Viewport viewport)
        {
            _toolbarUi.Bounds = new UniRectangle(0.0f, viewport.Height - 46, viewport.Width, 80.0f);
            _toolbarUi.Resized();
        }

        public override void UnloadContent()
        {
            _spriteRender.Dispose();
            _crosshair.Dispose();
            _font.Dispose();
            _d3DEngine.ViewPort_Updated -= D3dEngine_ViewPort_Updated;
        }

        public override void Update(ref GameTime timeSpent)
        {
            //TODO (Simon) update or updateOndraw toolbarUI  
        }

        public override void Interpolation(ref double interpolationHd, ref float interpolationLd)
        {
        }

        //Draw at 2d level ! (Last draw called)
        public override void Draw(int Index)
        {
            _spriteRender.Begin(SpriteRenderer.FilterMode.Linear);
            _spriteRender.Render(_crosshair, ref _crosshair.ScreenPosition, new Color4(1, 0, 0, 1));
            _spriteRender.End();

        }

        protected override void OnEnabledChanged(object sender, System.EventArgs args)
        {
            base.OnEnabledChanged(sender, args);
            if (Enabled)
            {
                if (!_screen.Desktop.Children.Contains(_toolbarUi))
                    _screen.Desktop.Children.Add(_toolbarUi);
            }
            else
            {
                _screen.Desktop.Children.Remove(_toolbarUi);
            }
        }
    }
}