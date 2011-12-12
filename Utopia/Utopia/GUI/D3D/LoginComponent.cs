﻿using System;
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
        private string _email;
        private string _password;
        private InputControl _emailControl;
        private InputControl _passwordControl;
        private ButtonControl _loginButton;
        
        /// <summary>
        /// Gets or sets current displayed email
        /// </summary>
        public string Email
        {
            get { return _email; }
            set { 
                _email = value;
                if (_emailControl != null)
                    _emailControl.Text = _email;
            }
        }
        
        /// <summary>
        /// Gets or sets current password in the component
        /// </summary>
        public string Password
        {
            get { return _password; }
            set { 
                _password = value;
                if (_passwordControl != null)
                    _passwordControl.Text = _password;
            }
        }

        public bool Locked
        {
            set { _loginButton.Enabled = value; }
        }

        /// <summary>
        /// Occurs when user press login button
        /// </summary>
        public event EventHandler Login;

        protected void OnLogin()
        {
            var handler = Login;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        /// <summary>
        /// Occurs when user press a register button
        /// </summary>
        public event EventHandler Register;

        protected void OnRegister()
        {
            var handler = Register;
            if (handler != null) handler(this, EventArgs.Empty);
        }
        
        public LoginComponent(D3DEngine engine, Screen screen)
        {
            _engine = engine;
            _screen = screen;
            _engine.ViewPort_Updated += EngineViewportUpdated;
        }

        public override void Dispose()
        {
            _loginWindow = null;
            _engine.ViewPort_Updated -= EngineViewportUpdated;
        }

        void EngineViewportUpdated(Viewport viewport)
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
                Text = "Email"
            });

            _loginWindow.Children.Add(new LabelControl
            {
                Bounds = new UniRectangle(dx, dy + 30, 30, 20),
                Text = "Password"
            });

            var loginInput = new InputControl
            {
                Bounds = new UniRectangle(dx + 60, dy, 140, 20),
                Text = Email,
            };
            _loginWindow.Children.Add(loginInput);

            _loginWindow.Children.Add(new InputControl
            {
                Bounds = new UniRectangle(dx + 60, dy + 30, 140, 20),
                Text = Password,
                IsPassword = true
            });

            var regButton = new ButtonControl
            {
                Bounds = new UniRectangle(dx, dy + 90, 200, 20),
                Text = "Register",
            };

            regButton.Pressed += delegate { OnRegister(); };

            _loginWindow.Children.Add(regButton);

            _loginButton = new ButtonControl
            {
                Bounds = new UniRectangle(dx, dy + 60, 200, 20),
                Text = "Login"
            };

            _loginButton.Pressed += delegate { OnLogin(); };

            _loginWindow.Children.Add(_loginButton);

            if (Enabled)
            {
                _screen.Desktop.Children.Add(_loginWindow);
                _screen.FocusedControl = loginInput;
                CenterWindow(new Size((int)_engine.ViewPort.Width, (int)_engine.ViewPort.Height));
            }
            
        }

        protected override void OnEnabledChanged()
        {
            if (!IsInitialized) return;

            if (Enabled)
            {
                _screen.Desktop.Children.Add(_loginWindow);
                CenterWindow(new Size((int)_engine.ViewPort.Width, (int)_engine.ViewPort.Height));
            }
            else
            {
                _screen.Desktop.Children.Remove(_loginWindow);
            }

            base.OnEnabledChanged();
        }

        public override void Update(ref GameTime timeSpend)
        {
            

        }



    }
}
