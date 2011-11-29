using System;
using Nuclex.UserInterface;
using Nuclex.UserInterface.Controls;
using Nuclex.UserInterface.Controls.Desktop;
using S33M3Engines.D3D;

namespace Utopia.GUI.D3D
{
    /// <summary>
    /// Provides login gui
    /// </summary>
    public class LoginComponent : GameComponent
    {
        WindowControl _loginWindow;
        private readonly Screen _screen;

        /// <summary>
        /// Occurs when you press login button
        /// </summary>
        public event EventHandler Login;
        
        public LoginComponent(Screen screen)
        {
            _screen = screen;
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

            _loginWindow.Children.Add(new InputControl
            {
                Bounds = new UniRectangle(dx + 60, dy, 140, 20),
                Text = ""
            });

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
        }

        public override void Update(ref GameTime timeSpent)
        {
            

        }

        public override void Dispose()
        {
            _loginWindow = null;
        }

    }
}
