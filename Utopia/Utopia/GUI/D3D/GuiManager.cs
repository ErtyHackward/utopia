using System;
using System.Collections.Generic;
using System.Windows.Forms;
using S33M3Engines.D3D;
using S33M3Engines.InputHandler.KeyboardHelper;
using S33M3Engines.Sprites;
using SharpDX;
using S33M3Engines.InputHandler.MouseHelper;
using S33M3Engines.InputHandler;
using Utopia.GUI.D3D.Inventory;
using S33M3Engines.Shared.Sprites;
using S33M3Engines;
using ButtonState = S33M3Engines.InputHandler.MouseHelper.ButtonState;
using MouseButtons = Nuclex.UserInterface.Input.MouseButtons;
using Screen = Nuclex.UserInterface.Screen;

namespace Utopia.GUI.D3D
{
    /// <summary>
    /// General GUI manager component, handles mouse input and draws the gui
    /// other component/classes have the responsability to add controls to _screen,
    /// No one should reference this class apart from the game initialization (ninject bind and get)
    ///
    /// </summary>
    public class GuiManager : DrawableGameComponent
    {
        /// <summary>Draws the GUI</summary>
        private Nuclex.UserInterface.Visuals.IGuiVisualizer _guiVisualizer;

        /// <summary>The GUI screen representing the desktop</summary>
        private readonly Screen _screen;

        private readonly D3DEngine _d3DEngine;
        private MouseState _prevMouseState;
        private KeyboardState _prevKeybState;

        public GuiManager(Screen screen, D3DEngine d3DEngine)
        {
            _screen = screen;
            _d3DEngine = d3DEngine;

            DrawOrders.UpdateIndex(0, 10000);
        }

        public override void Initialize()
        {
            _guiVisualizer = Nuclex.UserInterface.Visuals.Flat.FlatGuiVisualizer.FromFile(_d3DEngine,
                                                                                          "Resources\\Skins\\Suave\\Suave.skin.xml");
        }

        public override void LoadContent()
        {
        }

        public override void UnloadContent()
        {
            if (_guiVisualizer != null) _guiVisualizer.Dispose();
        }

        public override void Update(ref GameTime timeSpent)
        {
            InjectInput();
        }

        public override void Interpolation(ref double interpolatioHd, ref float interpolationHd)
        {
        }

        //Draw at 2d level ! (Last draw called)
        public override void Draw(int Index)
        {
            _guiVisualizer.Draw(_screen);
        }


        private void InjectInput()
        {
            MouseState mouseState = Mouse.GetState();

            if (_prevMouseState.LeftButton == ButtonState.Released && mouseState.LeftButton == ButtonState.Pressed)
                _screen.InjectMousePress(MouseButtons.Left);

            if (_prevMouseState.LeftButton == ButtonState.Pressed && mouseState.LeftButton == ButtonState.Released)
                _screen.InjectMouseRelease(MouseButtons.Left);


            _screen.InjectMouseMove(mouseState.X, mouseState.Y);

            _prevMouseState = Mouse.GetState();

            KeyboardState keybState = Keyboard.GetState();

            Keys[] keys = keybState.GetPressedKeys();

            foreach (var key in keys)
            {
                if (_prevKeybState.IsKeyUp(key) )
                {
                    if (key >= Keys.D0 && key <= Keys.D9)
                    {
                        char c = (char)key; //HACK done at midnight 
                        _screen.InjectCharacter((char)key);
                    }
                    
                }
           
            }

            _prevKeybState = Keyboard.GetState();
        }
    }
}