using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Engines.D3D;
using S33M3Engines.Sprites;
using SharpDX;

namespace Utopia.GUI.cegui
{
    public class GuiRenderer : CeGui.Renderer
    {
        Game _game;
        SpriteRenderer _spriteRenderer;

        public Game Game { get { return _game; } }

        public GuiRenderer(Game game)
        {
            _game = game;
            _spriteRenderer = new SpriteRenderer();
            _spriteRenderer.Initialize(game);
        }

        public override void AddQuad(CeGui.Rect destRect, float z, CeGui.Texture texture, CeGui.Rect textureRect, CeGui.ColourRect colors, CeGui.QuadSplitMode quadSplitMode)
        {

            //original d3d & xna renderer did not use  quadSplitMode, ignore this

            GuiTexture guiTexture = texture as GuiTexture;
            
            // somewhat put destrect into a matrix ? 
            // why doesnt spriteRenderer.Render have 2 rectangles like xna spritebatch ?
            Matrix transform = Matrix.Translation(destRect.Position.X, 0,destRect.Position.Y);
            
            
            Vector4 color = new Vector4(1, 0, 0, 1);

            //using vector4 is not explicit, what is the mapping between x y z w and left right top bottom
            Vector4 sourceRect = new Vector4(textureRect.Left,textureRect.Right,textureRect.Top,textureRect.Bottom );
            _spriteRenderer.Render(guiTexture.SpriteTexture, ref transform, color, sourceRect);
        }

        public override void DoRender()
        {
            throw new NotImplementedException();
        }

        public override void ClearRenderList()
        {
            throw new NotImplementedException();
        }

        public override CeGui.Texture CreateTexture()
        {
            throw new NotImplementedException();
        }

        public override CeGui.Texture CreateTexture(string fileName, string resourceGroup)
        {
            throw new NotImplementedException();
        }

        public override CeGui.Texture CreateTexture(float size)
        {
            throw new NotImplementedException();
        }

        public override void DestroyTexture(CeGui.Texture texture)
        {
            throw new NotImplementedException();
        }

        public override void DestroyAllTextures()
        {
            throw new NotImplementedException();
        }

        public override float Width
        {
            get { throw new NotImplementedException(); }
        }

        public override float Height
        {
            get { throw new NotImplementedException(); }
        }

        public override System.Drawing.SizeF Size
        {
            get { throw new NotImplementedException(); }
        }

        public override CeGui.Rect Rect
        {
            get { throw new NotImplementedException(); }
        }

        public override int MaxTextureSize
        {
            get { throw new NotImplementedException(); }
        }

        public override int HorizontalScreenDPI
        {
            get { throw new NotImplementedException(); }
        }

        public override int VerticalScreenDPI
        {
            get { throw new NotImplementedException(); }
        }
    }
}
