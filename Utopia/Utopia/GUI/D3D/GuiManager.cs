using System.Collections.Generic;
using S33M3Engines.D3D;
using S33M3Engines.Sprites;
using SharpDX;
using Nuclex.UserInterface;
using S33M3Engines.InputHandler.MouseHelper;
using S33M3Engines.InputHandler;
using Nuclex.UserInterface.Input;
using Utopia.GUI.D3D.Inventory;
using S33M3Engines.Shared.Sprites;
using S33M3Engines;

namespace Utopia.GUI.D3D
{
    /// <summary>
    /// General GUI manager component, handles mouse input and draws the gui
    /// other component/classes have the responsability to add controls to _screen,
    /// No one should reference this class apart from the game initialization (ninject bind and get)
    ///
    /// </summary>
    public class GuiManager : GameComponent
    {
        /// <summary>Draws the GUI</summary>
        private Nuclex.UserInterface.Visuals.IGuiVisualizer _guiVisualizer;

        /// <summary>The GUI screen representing the desktop</summary>
        private readonly Screen _screen;

        private SpriteRenderer _spriteRender;
        private SpriteTexture _crosshair;
        private SpriteFont _font;

        private readonly D3DEngine _d3DEngine;
        private MouseState _prevMouseState;

        public GuiManager(Screen screen, D3DEngine d3DEngine)
        {
            _screen = screen;
            _d3DEngine = d3DEngine;
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
            // TODO (Simon) dispose NuclexUI resources
            if(_spriteRender != null) _spriteRender.Dispose();
            if (_crosshair != null) _crosshair.Dispose();
            if (_font != null) _font.Dispose();
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
        public override void DrawDepth2()
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
        }
    }
}