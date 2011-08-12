using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Engines.D3D;
using S33M3Engines.Sprites;
using SharpDX;
using CeGui;
using RectangleF = System.Drawing.RectangleF;
using S33M3Engines.Struct.Vertex;

namespace S33M3Engines.Sprites.GUI
{

    /// <summary>
    /// Class responsible to render the GUI managed behind the scene by CeGui library
    /// </summary>
    public class SpriteGuiRenderer : CeGui.Renderer
    {
        #region Private variables
        private Game _game;
        //D3D Sprites rendering helper class
        private SpriteRenderer _spriteRenderer;
        //Sprites batch creator
        private UISpriteBatchManager _spriteManager;
        #endregion

        #region Public Properties/Variables
        public Game Game { get { return _game; } }

        /// <summary>
        /// Get render Screen Width
        /// </summary>
        public override float Width
        {
            get { return _game.ViewPort.Width; }
        }

        /// <summary>
        /// Get render screen Height
        /// </summary>
        public override float Height
        {
            get { return _game.ViewPort.Height; }
        }

        /// <summary>
        /// Get size of render screen
        /// </summary>
        public override System.Drawing.SizeF Size
        {
            get { return new System.Drawing.SizeF(_game.ViewPort.Width, _game.ViewPort.Height); }
        }

        /// <summary>
        /// Get size of render screen
        /// </summary>
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
        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">The Game Object</param>
        /// <param name="spriteRenderer">D3D Sprites rendering helper class</param>
        public SpriteGuiRenderer(Game game, SpriteRenderer spriteRenderer)
        {
            _game = game;
            _spriteRenderer = spriteRenderer;
            _spriteManager = new UISpriteBatchManager(100, 1000);
        }

        #region Public methods
        /// <summary>
        /// Called every time a Qued must be rendered (Directly) OR Queued for rendering (Deferred)
        /// </summary>
        /// <param name="destRect">The destination rectangle where the texture must be rendered - size specified with the 0->1 interval not true screen pixel coordinate</param>
        /// <param name="z">The Z parameter of the sprite</param>
        /// <param name="texture">The texture that must be used to render the sprite</param>
        /// <param name="textureRect">The subTexture to use from the texture for the sprite (In case of sprite sheet)</param>
        /// <param name="colors">The color sprite modifier</param>
        /// <param name="quadSplitMode">???</param>
        public override void AddQuad(CeGui.Rect destRect, float z, CeGui.Texture texture, CeGui.Rect textureRect, CeGui.ColourRect colors, CeGui.QuadSplitMode quadSplitMode)
        {
            SpriteGuiTexture spriteTexture = texture as SpriteGuiTexture;
            Matrix transform = Matrix.Scaling(destRect.Width / textureRect.Width, destRect.Height / textureRect.Height, 0) *
                   Matrix.Translation(destRect.Position.X, destRect.Position.Y, 0);

            //TODO Implement the 4 corner possible color
            Color4 color = new Color4(colors.topLeft.ToARGB());

            RectangleF sourceRect = new RectangleF(textureRect.Left, textureRect.Top, textureRect.Width, textureRect.Height);


            if (this.IsQueueingEnabled)
            {
                //Buffer the Rectangle ==> Deferred rendering, give the possibilit to batch the draw !
                _spriteManager.AddSprite(spriteTexture, ref transform, ref color, ref sourceRect);
            }
            else
            {
                //Direct Draw without queuing
                _spriteRenderer.Render(spriteTexture.SpriteTexture, ref transform, color, sourceRect, false);
            }
        }

        /// <summary>
        /// Render the Queued sprite's batches
        /// </summary>
        public override void DoRender()
        {
            foreach (UISpriteBatchManager.UISpriteBatch spriteBatch in _spriteManager.SpriteBatchs)
            {
                _spriteRenderer.RenderBatch(spriteBatch.SpriteTexture.SpriteTexture, spriteBatch.SpriteData.ToArray(), false);
            }
        }

        /// <summary>
        /// Clear the Queue list of sprite
        /// </summary>
        public override void ClearRenderList()
        {
            _spriteManager.ClearList();
        }

        /// <summary>
        /// Ask the creation of a new texture, texture will be empty here
        /// </summary>
        /// <returns></returns>
        public override CeGui.Texture CreateTexture()
        {
            SpriteGuiTexture tex = new SpriteGuiTexture(this);
            return tex;
        }

        /// <summary>
        /// Ask the creation of a new texture, from a filename + resourceGroup
        /// </summary>
        /// <param name="fileName">The Fle path</param>
        /// <param name="resourceGroup">The resource groupe</param>
        /// <returns></returns>
        public override CeGui.Texture CreateTexture(string fileName, string resourceGroup)
        {
            SpriteGuiTexture tex = new SpriteGuiTexture(this);
            tex.LoadFromFile(fileName);
            return tex;
        }        
        
        /// <summary>
        /// Ask the creation of a new texture, witha  specified size, but empty texture
        /// </summary>
        /// <param name="size">The default texture size</param>
        /// <returns></returns>
        public override CeGui.Texture CreateTexture(float size)
        {
            SpriteGuiTexture tex = new SpriteGuiTexture(this);
            return tex;
        }

        /// <summary>
        /// Dispose a specific texture held by a sprite
        /// </summary>
        /// <param name="texture"></param>
        public override void DestroyTexture(CeGui.Texture texture)
        {
            if (texture != null)
                (texture as SpriteGuiTexture).Dispose();
        }

        /// <summary>
        /// Dispose all Textures used by sprites
        /// </summary>
        public override void DestroyAllTextures()
        {
            _spriteManager.Dispose();
        }


        /// <summary>
        /// Load CeGui graphical theme resources
        /// </summary>
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

            /*
            GuiSystem.Instance.SetDefaultMouseCursor(
              ImagesetManager.Instance.GetImageset("SuaveLook").GetImage("Mouse-Arrow")
            );*/
        }
        #endregion









 

        

        


    }
}
