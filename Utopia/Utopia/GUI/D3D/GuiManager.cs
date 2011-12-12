using System.Windows.Forms;
using Nuclex.UserInterface;
using Nuclex.UserInterface.Controls;
using Nuclex.UserInterface.Controls.Desktop;
using S33M3Engines.D3D;
using S33M3Engines.D3D.DebugTools;
using S33M3Engines.InputHandler.MouseHelper;
using S33M3Engines.InputHandler;
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
    public class GuiManager : DrawableGameComponent, IDebugInfo
    {
        /// <summary>Draws the GUI</summary>
        private Nuclex.UserInterface.Visuals.IGuiVisualizer _guiVisualizer;

        /// <summary>The GUI screen representing the desktop</summary>
        private readonly Screen _screen;

        private readonly D3DEngine _d3DEngine;
        private MouseState _prevMouseState;

        private string _debugString;

        public GuiManager(Screen screen, D3DEngine d3DEngine)
        {
            _screen = screen;
            _d3DEngine = d3DEngine;

            _d3DEngine.GameWindow.KeyPress += GameWindowKeyPress;
            _d3DEngine.GameWindow.KeyDown += GameWindowKeyDown;
            _d3DEngine.GameWindow.KeyUp += GameWindowKeyUp;

            DrawOrders.UpdateIndex(0, 10001);
        }

        public override void Dispose()
        {
            _d3DEngine.GameWindow.KeyPress -= GameWindowKeyPress;
            _d3DEngine.GameWindow.KeyDown -= GameWindowKeyDown;
            _d3DEngine.GameWindow.KeyUp -= GameWindowKeyUp;
        }

        void GameWindowKeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyData)
            {
                case Keys.Back:
                case Keys.Return:
                case Keys.Escape:
                case Keys.Left:
                case Keys.Right:
                case Keys.Up:
                case Keys.Down:
                case Keys.Delete:
                    _screen.InjectKeyRelease(e.KeyData);
                    break;
                case Keys.Tab:
                    _screen.InjectKeyRelease(Keys.Down);
                    break;
            }
        }

        void GameWindowKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyData)
            {
                case Keys.Back:
                case Keys.Return:
                case Keys.Escape:
                case Keys.Left:
                case Keys.Right:
                case Keys.Up:
                case Keys.Down:
                case Keys.Delete:
                    _screen.InjectKeyPress(e.KeyData);
                    break;
                case Keys.Tab:
                    _screen.InjectKeyPress(Keys.Down);
                    break;
            }
        }

        void GameWindowKeyPress(object sender, KeyPressEventArgs e)
        {
            if (char.IsLetterOrDigit(e.KeyChar) || char.IsPunctuation(e.KeyChar))
                _screen.InjectCharacter(e.KeyChar);
        }

        public override void Initialize()
        {
            _guiVisualizer = Nuclex.UserInterface.Visuals.Flat.FlatGuiVisualizer.FromFile(_d3DEngine, "Resources\\Skins\\Suave\\Suave.skin.xml");
        }

        public override void LoadContent()
        {
        }

        public override void UnloadContent()
        {
            if (_guiVisualizer != null) _guiVisualizer.Dispose();
        }

        public override void Update(ref GameTime timeSpend)
        {
            InjectInput();
        }

        public override void Interpolation(ref double interpolatioHd, ref float interpolationHd)
        {
        }

        public void MessageBox(string message, string title = "", string buttonText = "Ok")
        {
            var mbWindow = new WindowControl { Title = title, Bounds = new UniRectangle(100,100,100,100) };

            mbWindow.Children.Add(new LabelControl { Text = message, Bounds = new UniRectangle( 10, 25, 100, 40) });

            var button = new ButtonControl { Text = buttonText, Bounds = new UniRectangle(10, 65, 50, 20) };

            button.Pressed += delegate { mbWindow.Close(); };

            mbWindow.Children.Add(button);

            _screen.Desktop.Children.Add(mbWindow);
        }
        
        //Draw at 2d level ! (Last draw called)
        public override void Draw(int index)
        {
            _guiVisualizer.Draw(_screen);
            var v = (Nuclex.UserInterface.Visuals.Flat.FlatGuiVisualizer)_guiVisualizer;
            _debugString = string.Format("Gui Draw calls: {0} Items: {1}", v.Graphics.DrawCalls, v.Graphics.DrawItems);
        }

        private void InjectInput()
        {
            MouseState mouseState = Mouse.GetState();

            if (_prevMouseState.LeftButton == ButtonState.Released && mouseState.LeftButton == ButtonState.Pressed)
                _screen.InjectMousePress(MouseButtons.Left);

            if (_prevMouseState.LeftButton == ButtonState.Pressed && mouseState.LeftButton == ButtonState.Released)
                _screen.InjectMouseRelease(MouseButtons.Left);

            if (_prevMouseState.RightButton == ButtonState.Released && mouseState.RightButton == ButtonState.Pressed)
                _screen.InjectMousePress(MouseButtons.Right);

            if (_prevMouseState.RightButton == ButtonState.Pressed && mouseState.RightButton == ButtonState.Released)
                _screen.InjectMouseRelease(MouseButtons.Right);

            _screen.InjectMouseMove(mouseState.X, mouseState.Y);

            _prevMouseState = Mouse.GetState();
        }

        public string GetInfo()
        {
            return _debugString;
        }
    }
}