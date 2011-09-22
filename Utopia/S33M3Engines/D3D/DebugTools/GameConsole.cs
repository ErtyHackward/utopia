using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX.Direct3D11;
using SharpDX;
using S33M3Engines.StatesManager;
using S33M3Engines.Sprites;
using S33M3Engines.Maths;
using S33M3Engines.Struct;
using S33M3Engines.Shared.Sprites;

namespace S33M3Engines.D3D.DebugTools
{
    public static class GameConsole
    {
        #region Variables
        static SpriteTexture _spriteTexture;
        static Sprites.SpriteFont _font;
        static Sprites.SpriteRenderer _spriteRender;
        static List<string> _loggedInfo = new List<string>();
        static D3DEngine _d3dEngine;
        static Color4 _colorText = new Color4(1, 1, 1, 1);
        static Matrix _textPosition = Matrix.Translation(5, 0, 0);

        public static bool Show;
        public static bool Actif;
        public static int DisplayInfoCount;
        public delegate void ActionStarted(string Action);
        public static event ActionStarted Action_Started;
        #endregion

        static GameConsole()
        {
            Show = false;
            Actif = false;
            DisplayInfoCount = 10;
        }

        public static void Write(string info)
        {
            if (Actif) _loggedInfo.Add(info);
        }

        public static void Initialize(D3DEngine d3dEngine)
        {
            _d3dEngine = d3dEngine;

            _font = new Sprites.SpriteFont();
            _font.Initialize("Segoe UI Mono", 11.5f, System.Drawing.FontStyle.Regular, true, _d3dEngine.Device);

            _spriteRender = new Sprites.SpriteRenderer();
            _spriteRender.Initialize(_d3dEngine);

            //create backGround texture
            Texture2DDescription desc = new Texture2DDescription()
            {
                Width = 1,
                Height = 1,
                MipLevels = 1,
                ArraySize = 1,
                Format = SharpDX.DXGI.Format.R32G32B32A32_Float,
                SampleDescription = new SharpDX.DXGI.SampleDescription() { Count = 1 },
                Usage = ResourceUsage.Dynamic,
                BindFlags = BindFlags.ShaderResource,
                CpuAccessFlags = CpuAccessFlags.Write
            };

            Texture2D backTexture = new Texture2D(_d3dEngine.Device, desc);

            DataBox data = _d3dEngine.Context.MapSubresource(backTexture, 0, 16, MapMode.WriteDiscard, MapFlags.None);
            data.Data.Position = 0;
            data.Data.Write<SharpDX.Vector4>(new SharpDX.Vector4(0, 0, 0, 0.5f)); //Ecrire dans la texture
            data.Data.Position = 0;
            _d3dEngine.Context.UnmapSubresource(backTexture, 0);

            _spriteTexture = new SpriteTexture(_d3dEngine.Device, backTexture, new Vector2(0, 0));
            backTexture.Dispose();
        }

        public static void CleanUp()
        {
            if (_font != null)
            {
                _font.Dispose();
                _font = null;
            }
            if (_spriteTexture != null)
            {
                _spriteTexture.Dispose();
                _spriteTexture = null;
            }
            if (_spriteRender != null)
            {
                _spriteRender.Dispose();
                _spriteRender = null;
            }
            _d3dEngine = null;
        }

        public static void RunAction(string info)
        {
            if (Action_Started != null) Action_Started(info);
        }

        public static void Draw()
        {
            if (Show)
            {
                _spriteRender.Begin();
                DrawInterface();
                DrawText();
                _spriteRender.End();
            }
        }

        private static void DrawInterface()
        {
            _spriteRender.Render(_spriteTexture, ref _spriteTexture.ScreenPosition, new Color4(1, 1, 1, 1), new RectangleF(0, 0, _d3dEngine.ViewPort.Width, _d3dEngine.ViewPort.Height / 3));
        }

        private static void DrawText()
        {
            int line = 1;
            string result = "";

            for (int CompIndex = Math.Max(_loggedInfo.Count - 1 - DisplayInfoCount, 0); CompIndex <= _loggedInfo.Count - 1; CompIndex++)
            {
                result = String.Concat(result, _loggedInfo[CompIndex] + "\n");
                line++;
            }
            _spriteRender.RenderText(_font, result, _textPosition, _colorText);
        }
    }
}
