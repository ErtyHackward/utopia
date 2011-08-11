using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CeGui;
using S33M3Engines.Sprites;
using SharpDX.Direct3D11;
using S33M3Engines.D3D;

namespace Utopia.GUI.cegui
{
    public class SpriteGuiTexture : CeGui.Texture, IDisposable
    {

        protected SpriteTexture _texture;
        protected string _filename;

        protected Game _game;

        public SpriteTexture SpriteTexture { get { return _texture; } }


        public SpriteGuiTexture(Renderer owner)
            : base(owner)
        {
            _game = ((SpriteGuiRenderer)owner).Game;
        }

        public override void LoadFromFile(string fileName)
        {
            Dispose();

            _texture = new SpriteTexture(((SpriteGuiRenderer)owner).Game, fileName);
            _filename = fileName;

            // grab the inferred dimensions of the texture
            this.width = _texture.TextureDescr.Width;
            this.height = _texture.TextureDescr.Height;
        }

        public override void LoadFromMemory(System.IO.Stream buffer, int bufferWidth, int bufferHeight)
        {

            Texture2D d3dTexture = Texture2D.FromStream<Texture2D>(_game.GraphicDevice, buffer, (int)buffer.Length);

            /* directx9 :
                        this.texture = D3D.TextureLoader.FromStream(
                 device, buffer, bufferWidth, bufferHeight,
                 1, D3D.Usage.None, D3D.Format.A8R8G8B8, D3D.Pool.Managed,
                 D3D.Filter.Point, D3D.Filter.Point, 0
               );*/
            _filename = string.Empty;

            // grab the inferred dimensions of the texture
            Texture2DDescription desc = d3dTexture.Description;
            this.width = desc.Width;
            this.height = desc.Height;

            _texture = new SpriteTexture(_game.GraphicDevice, d3dTexture, SharpDX.Vector2.Zero);
        }


        public void Dispose()
        {
            if (_texture != null)
            {
                _texture.Dispose();
                _texture = null;
            }
        }
    }
}
