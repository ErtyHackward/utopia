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
using S33M3Engines.InputHandler.MouseHelper;
using S33M3Engines.InputHandler;
using Nuclex.UserInterface.Input;
using Nuclex.UserInterface.Controls.Desktop;
using Utopia.Shared.Chunks.Entities.Inventory;
using Utopia.Shared.Chunks.Entities.Inventory.Tools;
using Utopia.GUI.D3D.Inventory;
using S33M3Engines.Shared.Sprites;

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
            _guiVisualizer = Nuclex.UserInterface.Visuals.Flat.FlatGuiVisualizer.FromFile(Game, "Resources\\Skins\\Suave\\Suave.skin.xml");
            _screen = new Screen();

            //PlayerInventory inventory = new PlayerInventory(); //TODO this would move to player class 
            //Pickaxe tool = new Pickaxe();
            //tool.AllowedSlots = InventorySlot.Bags;
            //tool.Icon = new SpriteTexture(Game.GraphicDevice, @"Textures\pickaxe-icon.png", new Vector2(0, 0));

            //Armor ring = new Armor();
            //ring.AllowedSlots = InventorySlot.Bags | InventorySlot.LeftRing; //FIXME slot system is ko
            //ring.Icon = new SpriteTexture(Game.GraphicDevice, @"Textures\ring-icon.png", new Vector2(0, 0));

            //inventory.bag.Items = new List<Item>();
            //inventory.bag.Items.Add(tool);
            //inventory.bag.Items.Add(ring);

            //SpriteTexture backGround = new SpriteTexture(Game.GraphicDevice, @"Textures\charactersheet.png", new Vector2(0, 0));
            //InventoryWindow invWin = new InventoryWindow(inventory, backGround);

            //_screen.Desktop.Children.Add(invWin);

            //WindowControl window = new WindowControl();
            //window.Bounds = new UniRectangle(40, 40, 300, 300);
            //window.Title = "NuclexUI Testing";
            //_screen.Desktop.Children.Add(window);

            //ButtonControl testBtn = new ButtonControl();
            //testBtn.Bounds = new UniRectangle(40, 40, 80, 20);
            //testBtn.Text = "Hello !";

            //ButtonControl testBtn2 = new ButtonControl();
            //testBtn2.Bounds = new UniRectangle(130, 40, 80, 20);
            //testBtn2.Text = "goodBye !";

            //window.Children.Add(testBtn);
            //window.Children.Add(testBtn2);
        }

        public override void LoadContent()
        {
            _crosshair = new SpriteTexture(Game.GraphicDevice, @"Textures\Gui\Crosshair.png", ref Game.D3dEngine.ViewPort_Updated, Game.ActivCamera.Viewport);

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

        private void RenderGui()
        {
            _guiVisualizer.Draw(_screen);
        }


        MouseState prevMouseState;

        private void InjectInput()
        {
            MouseState mouseState = Mouse.GetState();

             if (prevMouseState.LeftButton==ButtonState.Released && mouseState.LeftButton==ButtonState.Pressed)
                _screen.InjectMousePress(MouseButtons.Left);

             if (prevMouseState.LeftButton==ButtonState.Pressed && mouseState.LeftButton==ButtonState.Released)
                _screen.InjectMouseRelease(MouseButtons.Left);

            _screen.InjectMouseMove(mouseState.X, mouseState.Y);

            prevMouseState = Mouse.GetState(); 
        }
    }
}
