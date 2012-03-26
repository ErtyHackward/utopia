using System;
using System.Drawing;
using S33M3CoreComponents.GUI.Nuclex;
using S33M3CoreComponents.GUI.Nuclex.Controls.Desktop;
using S33M3CoreComponents.Sprites;
using S33M3DXEngine;
using S33M3DXEngine.Main;

namespace Sandbox.Client.Components
{
    /// <summary>
    /// Base component for utopia menu (display logo and background)
    /// </summary>
    public abstract class SandboxMenuComponent : GameComponent
    {
        // common resources
        public static SpriteTexture StShadow;
        public static SpriteTexture StLogo;
        public static SpriteTexture StGameName;
        public static SpriteTexture StCubesPattern;
        public static SpriteTexture StLinenPattern;
        public static SpriteFont FontBebasNeue;


        private readonly D3DEngine _engine;
        private readonly MainScreen _screen;

        protected int _headerHeight;

        protected ImageControl _linen;
        protected ImageControl _cubes;
        protected ImageControl _shadow;
        protected ImageControl _logo;
        protected ImageControl _version;



        public static SpriteTexture LoadTexture(D3DEngine engine, string filePath)
        {
            return new SpriteTexture(engine.Device, (Bitmap)Image.FromFile(filePath),
                                             new SharpDX.Vector2(), engine.B8G8R8A8_UNormSupport);
        }

        public static void LoadCommonImages(D3DEngine engine)
        {
            if (StShadow != null)
                throw new InvalidOperationException("Common images already loaded");

            StShadow        = LoadTexture(engine, "Images\\shadow.png");
            StLogo          = LoadTexture(engine, "Images\\logo.png");
            StGameName      = LoadTexture(engine, "Images\\version.png");
            StCubesPattern  = LoadTexture(engine, "Images\\cubes.png");
            StLinenPattern  = LoadTexture(engine, "Images\\black-linen.png");

            FontBebasNeue = new SpriteFont();
            FontBebasNeue.Initialize("Images\\BebasNeue.otf", 35, FontStyle.Regular, true, engine.Device);
        }

        protected SandboxMenuComponent(D3DEngine engine, MainScreen screen)
        {
            _engine = engine;
            _screen = screen;
            _engine.ViewPort_Updated += EngineViewPortUpdated;

            _linen = new ImageControl { Image = StLinenPattern };
            _cubes = new ImageControl { Image = StCubesPattern };
            _shadow = new ImageControl { Image = StShadow };
            _logo = new ImageControl { Image = StLogo };
            _version = new ImageControl { Image = StGameName };

            Resize(_engine.ViewPort);

        }

        public override void Dispose()
        {
            _engine.ViewPort_Updated -= EngineViewPortUpdated;
            base.Dispose();
        }

        protected virtual void EngineViewPortUpdated(SharpDX.Direct3D11.Viewport viewport, SharpDX.Direct3D11.Texture2DDescription newBackBuffer)
        {
            Resize(viewport);
        }

        public override void EnableComponent()
        {
            _screen.Desktop.Children.Add(_logo);
            _screen.Desktop.Children.Add(_version);
            _screen.Desktop.Children.Add(_shadow);
            _screen.Desktop.Children.Add(_cubes);
            _screen.Desktop.Children.Add(_linen);
        }

        public override void DisableComponent()
        {
            _screen.Desktop.Children.Remove(_linen);
            _screen.Desktop.Children.Remove(_cubes);
            _screen.Desktop.Children.Remove(_shadow);
            _screen.Desktop.Children.Remove(_logo);
            _screen.Desktop.Children.Remove(_version);
        }

        private void Resize(SharpDX.Direct3D11.Viewport viewport)
        {
            if (viewport.Height >= 620)
                _headerHeight = (int)(viewport.Height * 0.3f);
            else
                _headerHeight = Math.Abs((int)viewport.Height - 434);

            _cubes.Bounds = new UniRectangle(0, 0, viewport.Width, _headerHeight);
            _linen.Bounds = new UniRectangle(0, _headerHeight, viewport.Width, viewport.Height - _headerHeight);
            _shadow.Bounds = new UniRectangle(0, _headerHeight - 117, viewport.Width, 287);
            _logo.Bounds = new UniRectangle((viewport.Width - 562) / 2, _headerHeight - 44, 562, 113);
            _version.Bounds = new UniRectangle((viewport.Width - 562) / 2 + 360, _headerHeight + 49, 196, 31);
        }
    }
}
