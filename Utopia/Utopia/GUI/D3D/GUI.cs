using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Engines.D3D;
using S33M3Engines.Sprites;
using S33M3Engines.StatesManager;
using SharpDX.Direct3D11;
using S33M3Engines.Struct.Vertex;
using SharpDX;
using S33M3Engines.D3D.Effects.Basics;
using S33M3Engines.Struct;

namespace Utopia.GUI.D3D
{
    public class GUI : GameComponent
    {
        SpriteRenderer _spriteRender;
        SpriteTexture _crosshair;
        SpriteFont _font;

        public GUI(Game game)
            : base(game)
        {
        }

        public override void Initialize()
        {
        }

        public override void LoadContent()
        {
            _crosshair = new SpriteTexture(Game, @"Textures\Gui\Crosshair.png");

            _spriteRender = new SpriteRenderer();
            _spriteRender.Initialize(Game);
            _font = new SpriteFont();
            _font.Initialize("Segoe UI Mono", 13f, System.Drawing.FontStyle.Regular, true, Game.GraphicDevice);

        }

        public override void UnloadContent()
        {
            _spriteRender.Dispose();
            _crosshair.Dispose();
            _font.Dispose();
        }

        public override void Update(ref GameTime TimeSpend)
        {
        }

        public override void Interpolation(ref double interpolation_hd, ref float interpolation_ld)
        {
        }

        //Draw at 2d level ! (Last draw called)
        public override void DrawDepth2()
        {
            _spriteRender.Begin(SpriteRenderer.FilterMode.Linear);

            _spriteRender.Render(_crosshair, ref _crosshair.ScreenPosition, new Vector4(1, 0, 0, 1));

            //_spriteRender.RenderText(_font, "That's Bumbas baby !\nDeuxième ligne !", Matrix.Translation(0, 0, 0), new Vector4(1, 1, 0, 1));

            _spriteRender.End();
            //StatesMnger.ApplyStates(_statesId);
            //// + Call to shader
            //_effect.DiffuseTexture = _sprite.Texture;
            //_effect.WorldVariable = Matrix.Identity;
            //_effect.ViewVariable = Matrix.Identity;
            //_effect.ProjectionVariable = Game.ActivCamera.Projection2D;

            //_effect.ApplyPass(0);
            //_sprite.Draw();
            //StatesMnger.ApplyStates(DefaultRenderStates.DepthStencil);
            //StatesMnger.ApplyStates(DefaultRenderStates.BlendStates);
        }
    }
}
