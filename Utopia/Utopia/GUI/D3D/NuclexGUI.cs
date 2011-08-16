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
using Nuclex.UserInterface;
using Utopia.GUI.XnaAdapters;
using Utopia.GUI.NuclexUIPort.Visuals.Flat;
using S33M3Engines.InputHandler.MouseHelper;
using S33M3Engines.InputHandler;
using Nuclex.UserInterface.Input;
using Nuclex.UserInterface.Controls.Desktop;

namespace Utopia.GUI.D3D
{
    public class GUI : GameComponent
    {

        /// <summary>Draws the GUI</summary>
        private Nuclex.UserInterface.Visuals.IGuiVisualizer _guiVisualizer;
        
        /// <summary>The GUI screen representing the desktop</summary>
        private Screen _screen;


        SpriteRenderer _spriteRender;
        SpriteTexture _crosshair;
        SpriteFont _font;

        public GUI(Game game)
            : base(game)
        {
        }

        public override void Initialize()
        {
            //all this crap is to feed the GraphicsDevice to contentManager + FlatGuiVisualizer
            // this shows the superiority of clean explicit dependency injection vs service locator anti pattern
            //  this is a good explanation of the problem :  http://www.beefycode.com/post/Why-I-Hate-IServiceProvider.aspx
          
            ServiceProvider serviceProvider = new ServiceProvider();
            GraphicsDeviceService graphicsDeviceService = new GraphicsDeviceService();
            graphicsDeviceService.GraphicsDevice = Game.GraphicDevice;
            serviceProvider.AddService<IGraphicsDeviceService>(graphicsDeviceService);

            _guiVisualizer = Nuclex.UserInterface.Visuals.Flat.FlatGuiVisualizer.FromFile(Game, serviceProvider, "Resources\\Skins\\Suave\\Suave.skin.xml");

            _screen = new Screen();

            ButtonControl testBtn = new ButtonControl();
            testBtn.Bounds = new UniRectangle(40,40,80,20);
            testBtn.Text="Salut";

            _screen.Desktop.Children.Add(testBtn);
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

        public override void Update(ref GameTime TimeSpent)
        {

            InjectInput();
        }

        public override void Interpolation(ref double interpolation_hd, ref float interpolation_ld)
        {
        }

        //Draw at 2d level ! (Last draw called)
        public override void DrawDepth2()
        {
            _spriteRender.Begin(SpriteRenderer.FilterMode.Linear);
            _spriteRender.Render(_crosshair, ref _crosshair.ScreenPosition, new Color4(1, 0, 0, 1));
            _spriteRender.End();

            // NuclexUI handles its own spritebatch, separate from the crosshair
            RenderGui();
        }

        private  void RenderGui()
        {
            _guiVisualizer.Draw(_screen);
        }

      
        private  void InjectInput()
        {  
            MouseState mouseState = Game.InputHandler.CurMouseState;
            
            MouseState prevMouseState = Game.InputHandler.PrevMouseState;

            MouseButtons pressedButtons = 0;
            MouseButtons releasedButtons = 0;

            if (mouseState.LeftButton.HasFlag(ButtonState.Pressed))
                pressedButtons |= MouseButtons.Left;
            else
                releasedButtons |= MouseButtons.Left;

            if (mouseState.RightButton.HasFlag(ButtonState.Pressed)) 
                pressedButtons |= MouseButtons.Right;
            else
                releasedButtons |= MouseButtons.Right;

            _screen.InjectMouseMove(mouseState.X,mouseState.Y);
            _screen.InjectMousePress(pressedButtons);
            _screen.InjectMouseRelease(releasedButtons);
        }
    }
}
