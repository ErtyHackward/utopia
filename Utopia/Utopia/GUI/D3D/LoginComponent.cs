using System;
using System.Drawing;
using Nuclex.UserInterface;
using Nuclex.UserInterface.Controls;
using Nuclex.UserInterface.Controls.Desktop;
using S33M3Engines;
using S33M3Engines.D3D;
using SharpDX.Direct3D11;

namespace Utopia.GUI.D3D
{
    /// <summary>
    /// Provides login gui
    /// </summary>
    public class LoginComponent : GameComponent
    {
        WindowControl _loginWindow;
        private readonly D3DEngine _engine;
        private readonly Screen _screen;

        /// <summary>
        /// Occurs when you press login button
        /// </summary>
        public event EventHandler Login;
        
        public LoginComponent(D3DEngine engine, Screen screen)
        {
            _engine = engine;
            _screen = screen;
            _engine.ViewPort_Updated += _engine_ViewportUpdated;
        }

        public override void Dispose()
        {
            _loginWindow = null;
            _engine.ViewPort_Updated -= _engine_ViewportUpdated;
        }

        void _engine_ViewportUpdated(Viewport viewport)
        {
            // locate login window
            CenterWindow(new Size((int)viewport.Width, (int)viewport.Height));
        }

        private void CenterWindow(Size screenSize)
        {
            _loginWindow.Bounds = new UniRectangle(
                new UniVector((screenSize.Width - _loginWindow.Bounds.Size.X.Offset) / 2, (screenSize.Height - _loginWindow.Bounds.Size.Y.Offset) / 2), 
                _loginWindow.Bounds.Size);
        }

        public override void Initialize()
        {
            var dx = 20;
            var dy = 40;

            _loginWindow = new WindowControl
            {
                Bounds = new UniRectangle(300, 200, 245, 170),
                Title = "Login"
            };

            _loginWindow.Children.Add(new LabelControl
            {
                Bounds = new UniRectangle(dx, dy, 30, 20),
                Text = "Login"
            });

            _loginWindow.Children.Add(new LabelControl
            {
                Bounds = new UniRectangle(dx, dy + 30, 30, 20),
                Text = "Password"
            });

            var loginInput = new InputControl
            {
                Bounds = new UniRectangle(dx + 60, dy, 140, 20),
                Text = "",
            };
            _loginWindow.Children.Add(loginInput);

            _loginWindow.Children.Add(new InputControl
            {
                Bounds = new UniRectangle(dx + 60, dy + 30, 140, 20),
                Text = "",
                IsPassword = true
            });

            _loginWindow.Children.Add(new OptionControl
            {
                Bounds = new UniRectangle(dx, dy + 60, 100, 16),
                Text = "Remember",
                Selected = true
            });

            _loginWindow.Children.Add(new ButtonControl
            {
                Bounds = new UniRectangle(dx, dy + 90, 200, 20),
                Text = "Login"
            });

            _screen.Desktop.Children.Add(_loginWindow);

            _screen.FocusedControl = loginInput;

            CenterWindow(new Size((int)_engine.ViewPort.Width, (int)_engine.ViewPort.Height));
        }

        public override void Update(ref GameTime timeSpent)
        {
            

        }



    }
}
