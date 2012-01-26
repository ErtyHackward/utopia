using System.Windows.Forms;
using Nuclex.UserInterface;
using Nuclex.UserInterface.Controls;
using Nuclex.UserInterface.Controls.Desktop;
using S33M3Engines.D3D;
using S33M3Engines.D3D.DebugTools;
using S33M3Engines.InputHandler.MouseHelper;
using S33M3Engines.InputHandler;
using S33M3Engines;
using SharpDX.Direct3D11;
using Utopia.GUI.NuclexUIPort.Controls.Desktop;
using ButtonState = S33M3Engines.InputHandler.MouseHelper.ButtonState;
using Control = Nuclex.UserInterface.Controls.Control;
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

        /// <summary>
        /// Indicates that some dialog just closed
        /// </summary>
        public static bool DialogClosed;

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
            DialogClosed = false;
            InjectInput();
        }

        public override void Interpolation(ref double interpolatioHd, ref float interpolationHd)
        {
        }

        public void MessageBox(string message, string title = "", string[] buttonsText = null, System.Action<string> action = null)
        {
            var screenWidth = _d3DEngine.ViewPort.Width;
            var screenHeight = _d3DEngine.ViewPort.Height;

            var windowWidth = 300;
            var windowHeight = 100;

            if (buttonsText == null)
                buttonsText = new[] { "Ok" };

            var mbWindow = new WindowControl { Title = title, Bounds = new UniRectangle((screenWidth - windowWidth) / 2, (screenHeight - windowHeight) / 2, windowWidth, windowHeight) };

            mbWindow.Children.Add(new LabelControl { Text = message, Bounds = new UniRectangle(15, 25, windowWidth - 40, 40) });

            var buttonsPlace = new Control { Bounds = new UniRectangle(0, 0, 0, 20), LayoutFlags = ControlLayoutFlags.WholeRowCenter, LeftTopMargin = new SharpDX.Vector2() };

            foreach (var text in buttonsText)
            {
                var button = new ButtonControl { Text = text, Bounds = new UniRectangle((windowWidth - 50) / 2, windowHeight - 30, 20 + 5 * text.Length, 20) };

                buttonsPlace.Bounds.Size.X += button.Bounds.Size.X + 5;


                var text1 = text;
                button.Pressed += delegate
                {

                    _screen.Desktop.Children.Remove(DialogHelper.DialogBg);
                    mbWindow.Close();
                    DialogClosed = true;
                    if (action != null) action(text1);
                };

                buttonsPlace.Children.Add(button);
            }

            buttonsPlace.UpdateLayout();

            mbWindow.Children.Add(buttonsPlace);
            mbWindow.UpdateLayout();

            _screen.Desktop.Children.Add(mbWindow);

            // block all underlying controls
            _screen.Desktop.Children.Add(DialogHelper.DialogBg);
            DialogHelper.DialogBg.BringToFront();

            mbWindow.BringToFront();
        }

        //Draw at 2d level ! (Last draw called)
        public override void Draw(int index)
        {
            _d3DEngine.Context.ClearDepthStencilView(_d3DEngine.DepthStencilTarget, DepthStencilClearFlags.Depth, 1.0f, 0);
            _guiVisualizer.Draw(_screen);
            var v = (Nuclex.UserInterface.Visuals.Flat.FlatGuiVisualizer)_guiVisualizer;
            _debugString = string.Format("Gui Draw calls: {0} Items: {1}", v.Graphics.DrawCalls, v.Graphics.DrawItems);
        }

        private void InjectInput()
        {
            var mouseState = Mouse.GetState();

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