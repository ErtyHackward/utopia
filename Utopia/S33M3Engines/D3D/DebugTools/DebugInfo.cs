using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX.Direct3D11;
using SharpDX;
using S33M3Engines.Struct;
using S33M3Engines.StatesManager;
using S33M3Engines.D3D.Effects;
using S33M3Engines.Buffers;
using S33M3Engines.Sprites;
using Utopia.Shared.Structs;

namespace S33M3Engines.D3D.DebugTools
{
    public interface IDebugInfo
    {
        string GetInfo();
    }

    public class DebugInfo : DrawableGameComponent
    {
        SpriteFont _font;
        SpriteRenderer _spriteRender;
        Matrix _textPosition = Matrix.Translation(5, 0, 0);

        IDebugInfo[] _args;
        string[] _infos;
        public bool Activated = false;
        private Color4 _fontColor = new Color4(Color.Yellow.A, Color.Yellow.R, Color.Yellow.G, Color.Yellow.B);
        private D3DEngine _d3dEngine;

        public DebugInfo(D3DEngine d3dEngine)
        {
            _d3dEngine = d3dEngine;
            DrawOrders.UpdateIndex(0, 10000);
        }

        public void SetComponants(params IDebugInfo[] args)
        {
            _args = args;
            _infos = new string[_args.Length];
        }

        public override void LoadContent()
        {
            _font = new SpriteFont();
            _font.Initialize("Segoe UI Mono", 11.5f, System.Drawing.FontStyle.Regular, true, _d3dEngine.Device);
            _spriteRender = new SpriteRenderer();
            _spriteRender.Initialize(_d3dEngine);
        }

        public override void UnloadContent()
        {
            _font.Dispose();
            _spriteRender.Dispose();
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        /// 
        public override void Update(ref GameTime TimeSpend)
        {
            if (Activated)
            {
                for (int CompIndex = 0; CompIndex < _args.Length; CompIndex++)
                {
                    _infos[CompIndex] = _args[CompIndex].GetInfo() + "\n";
                }
            }
        }


        public override void Draw(int Index)
        {
            //Afficher la console, ou bien les infos !
            if (Activated)
            {
                //_sprite.Begin(SpriteFlags.SaveState); //==> Problèmes avec PIX ! ==> Veuillez a tjs mettre mes states avec chaque draw !
                //for (int CompIndex = 0; CompIndex < _args.Length; CompIndex++)
                //{
                //    string.Concat(_infos[CompIndex] + "\n");

                //    _font.Draw(_sprite, _infos[CompIndex], new System.Drawing.Rectangle(0, _fontHeight * CompIndex, Game.GameWindow.ClientSize.Width, _fontHeight * CompIndex), FontDrawFlags.NoClip, _fontColor);
                //}

                _spriteRender.Begin(SpriteRenderer.FilterMode.Point);
                _spriteRender.RenderText(_font, string.Concat(_infos), _textPosition, _fontColor);
                _spriteRender.End();
            }
        }

    }
}
