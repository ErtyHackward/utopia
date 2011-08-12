using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Engines.D3D;
using S33M3Engines.Sprites;
using SharpDX;
using CeGui;
using RectangleF = System.Drawing.RectangleF;

namespace Utopia.GUI.cegui
{
    public class SpriteGuiRenderer : CeGui.Renderer
    {

        Game _game;
        SpriteRenderer _spriteRenderer;
        List<UISprite> _sprites = new List<UISprite>();

        public Game Game { get { return _game; } }

        private class UISprite
        {
            public SpriteGuiTexture guiTexture;
            public Matrix transform;
            public Color4 color;
            public RectangleF sourceRect;

            public UISprite(SpriteGuiTexture guiTexture, ref Matrix transform, Color4 color, RectangleF sourceRect)
            {
                this.guiTexture = guiTexture;
                this.transform = transform;
                this.color = color;
                this.sourceRect = sourceRect;
            }
        }


        public SpriteGuiRenderer(Game game, SpriteRenderer spriteRenderer)
        {
            _game = game;
            _spriteRenderer = spriteRenderer;
        }

        public override void AddQuad(CeGui.Rect destRect, float z, CeGui.Texture texture, CeGui.Rect textureRect, CeGui.ColourRect colors, CeGui.QuadSplitMode quadSplitMode)
        {
            //original d3d & xna renderer did not use  quadSplitMode, ignore this
            SpriteGuiTexture guiTexture = texture as SpriteGuiTexture;

            //Transform the TextureRect from CeGUI to tru-1e dimention (At this moment, it is in the range of 0-1
            //textureRect.Bottom *= texture.Height;
            //textureRect.Top *= texture.Height;
            //textureRect.Right *= texture.Width;
            //textureRect.Left *= texture.Width;

            // somewhat put destrect into a matrix ? 
            // why doesnt spriteRenderer.Render have 2 rectangles like xna spritebatch ?
            Matrix transform = Matrix.Scaling(destRect.Width / textureRect.Width, destRect.Height / textureRect.Height, 0) * 
                               Matrix.Translation(destRect.Position.X, destRect.Position.Y, 0);

            Color4 color = new Color4(1, 1, 1, 1);

            RectangleF sourceRect = new RectangleF(textureRect.Left, textureRect.Top, textureRect.Width, textureRect.Height);

            _sprites.Add(new UISprite(guiTexture, ref transform, color, sourceRect));
        }

        public override void DoRender()
        {
            foreach (UISprite sprite in _sprites)
            {
                _spriteRenderer.Render(sprite.guiTexture.SpriteTexture, ref sprite.transform, sprite.color, sprite.sourceRect, false);
            }
        }

        public override void ClearRenderList()
        {

        }

        public override CeGui.Texture CreateTexture()
        {
            SpriteGuiTexture tex = new SpriteGuiTexture(this);
            return tex;
        }

        public override CeGui.Texture CreateTexture(string fileName, string resourceGroup)
        {
            SpriteGuiTexture tex = new SpriteGuiTexture(this);
            tex.LoadFromFile(fileName);
            return tex;
        }

        public override CeGui.Texture CreateTexture(float size)
        {
            SpriteGuiTexture tex = new SpriteGuiTexture(this);
            return tex;
        }

        public override void DestroyTexture(CeGui.Texture texture)
        {
            if(texture != null)
                (texture as SpriteGuiTexture).Dispose(); 
        }

        public override void DestroyAllTextures()
        {
            foreach (UISprite sprite in _sprites)
            {
                sprite.guiTexture.Dispose();
            }
            _sprites.Clear();
        }

        public override float Width
        {
            get { return _game.ViewPort.Width; }
        }

        public override float Height
        {
            get { return _game.ViewPort.Height; }
        }

        public override System.Drawing.SizeF Size
        {
            get { return new System.Drawing.SizeF(_game.ViewPort.Width, _game.ViewPort.Height); }
        }

        public override CeGui.Rect Rect
        {
            get { return new CeGui.Rect(0, 0, _game.ViewPort.Width, _game.ViewPort.Height); }
        }

        public override int MaxTextureSize
        {
            get { throw new NotImplementedException(); }
        }

        public override int HorizontalScreenDPI
        {
            get { return 96; }
        }

        public override int VerticalScreenDPI
        {
            get { return 96; }
        }

        public void loadCeGuiResources()
        {

            // Widget sets are collections of widgets that provide the widget classes defined
            // in CeGui (like PushButton, CheckBox and so on) with their own distinctive look
            // (like a theme) and possibly even custom behavior.
            //
            // Here we load all compiled widget sets we can find in the current directory. This
            // is done to demonstrate how you could add widget set dynamically to your
            // application. Other possibilities would be to hardcode the widget set an
            // application uses or determining the assemblies to load from a configuration file.
            string[] assemblyFiles = System.IO.Directory.GetFiles(
              System.IO.Directory.GetCurrentDirectory(), "CeGui.WidgetSets.*.dll"
            );
            foreach (string assemblyFile in assemblyFiles)
            {
                WindowManager.Instance.AttachAssembly(
                  System.Reflection.Assembly.LoadFile(assemblyFile)
                );
            }

            // Imagesets area a collection of named areas within a texture or image file. Each
            // area becomes an Image, and has a unique name by which it can be referenced. Note
            // that an Imageset would normally be specified as part of a scheme file, although
            // as this example is demonstrating, it is not a requirement.
            //
            // Again, we load all image sets we can find, this time searching the resources folder.
            string[] imageSetFiles = System.IO.Directory.GetFiles(
              System.IO.Directory.GetCurrentDirectory() + "\\Resources", "*.imageset"
            );
            foreach (string imageSetFile in imageSetFiles)
                ImagesetManager.Instance.CreateImageset(imageSetFile);

        }

        /// <summary>Configures the default cursor and font for CeGui</summary>
        public void setupDefaults()
        {

            // When the gui imagery side of thigs is set up, we should load in a font.
            // You should always load in at least one font, this is to ensure that there
            // is a default available for any gui element which needs to draw text.
            // The first font you load is automatically set as the initial default font,
            // although you can change the default later on if so desired.  Again, it is
            // possible to list fonts to be automatically loaded as part of a scheme, so
            // this step may not usually be performed explicitly.
            //
            // Fonts are loaded via the FontManager singleton.
            FontManager.Instance.CreateFont("Default", "Arial", 9, FontFlags.None);
            FontManager.Instance.CreateFont("WindowTitle", "Arial", 12, FontFlags.Bold);
            GuiSystem.Instance.SetDefaultFont("Default");

            // The next thing we do is to set a default mouse cursor image.  This is
            // not strictly essential, although it is nice to always have a visible
            // cursor if a window or widget does not explicitly set one of its own.
            //
            // This is a bit hacky since we're assuming the SuaveLook image set, referenced
            // below, will always be available.
            GuiSystem.Instance.SetDefaultMouseCursor(
              ImagesetManager.Instance.GetImageset("SuaveLook").GetImage("Mouse-Arrow")
            );

        }


    }
}
