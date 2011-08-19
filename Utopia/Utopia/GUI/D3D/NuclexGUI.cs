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
using S33M3Engines;
using Utopia.Entities.Living;
using Utopia.GUI.D3D.DebugUI;

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

        ToolBarUI _toolbarUI;//this one is a field because it will be updateable when you change tools with mousewheel

        D3DEngine _d3dEngine;
        PlayerInventory _inventory;
        readonly List<IGameComponent> _components;

        public GUI(List<IGameComponent> components, D3DEngine d3dEngine, PlayerInventory inventory)
        {
            _d3dEngine = d3dEngine;
            _inventory = inventory;
            _components = components;
        }

        public override void Initialize()
        {
            _guiVisualizer = Nuclex.UserInterface.Visuals.Flat.FlatGuiVisualizer.FromFile(_d3dEngine, "Resources\\Skins\\Suave\\Suave.skin.xml");

            _screen = new Screen();

            //TODO best way for acceding player component from GUI component ? 


            SpriteTexture backGround = new SpriteTexture(_d3dEngine.Device, @"Textures\charactersheet.png", new Vector2(0, 0));
            
            InventoryWindow invWin = new InventoryWindow(_inventory, backGround);
            //_screen.Desktop.Children.Add(invWin);

            //TODO this one and the components dependency should surely be moved in a separate debug only component
            Utopia.GUI.D3D.DebugUI.DebugUI debugUI = new Utopia.GUI.D3D.DebugUI.DebugUI(_components);
            _screen.Desktop.Children.Add(debugUI);

            _toolbarUI = new ToolBarUI(_inventory);
            _screen.Desktop.Children.Add(_toolbarUI);
        }

        public override void LoadContent()
        {
            _crosshair = new SpriteTexture(_d3dEngine.Device, @"Textures\Gui\Crosshair.png", ref _d3dEngine.ViewPort_Updated, _d3dEngine.ViewPort);

            _spriteRender = new SpriteRenderer();
            _spriteRender.Initialize(_d3dEngine);

            _font = new SpriteFont();
            _font.Initialize("Segoe UI Mono", 13f, System.Drawing.FontStyle.Regular, true, _d3dEngine.Device);
        }

        public override void UnloadContent()
        {
            _spriteRender.Dispose();
            _crosshair.Dispose();
            _font.Dispose();
        }

        public override void Update(ref GameTime timeSpent)
        {
            InjectInput();
            _toolbarUI.Update(ref timeSpent);
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
