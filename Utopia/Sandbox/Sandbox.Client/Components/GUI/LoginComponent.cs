using System;
using System.Windows.Forms;
using S33M3CoreComponents.GUI.Nuclex;
using S33M3CoreComponents.GUI.Nuclex.Controls;
using S33M3CoreComponents.GUI.Nuclex.Controls.Desktop;
using S33M3CoreComponents.Sprites2D;
using S33M3DXEngine;
using SharpDX.Direct3D11;
using SharpDX;

namespace Sandbox.Client.Components.GUI
{
    /// <summary>
    /// Provides login gui
    /// </summary>
    public class LoginComponent : SandboxMenuComponent
    {
        private readonly D3DEngine _engine;
        private readonly MainScreen _screen;
        private string _email;
        private string _password;
        private InputControl _emailControl;
        private InputControl _passwordControl;
        private ButtonControl _loginButton;
        private ImageControl _authenticationLabel;
        private ImageControl _errorImage;
        private LabelControl _errorText;
        private ButtonControl _regButton;

        #region Images
        private SpriteTexture _stEnter;
        private SpriteTexture _stEnterHover;
        private SpriteTexture _stEnterDown;
        private SpriteTexture _stEnterDisabled;

        private SpriteTexture _stSign;
        private SpriteTexture _stSignHover;

        private SpriteTexture _stInputBg;
        private SpriteTexture _stEmail;
        private SpriteTexture _stPassword;

        private SpriteTexture _stAutentification;
        private SpriteTexture _stError;
        #endregion

        /// <summary>
        /// Gets or sets current displayed email
        /// </summary>
        public string Email
        {
            get { return _emailControl == null ? _email : _emailControl.Text; }
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
            get { return _passwordControl == null ? _password : _passwordControl.Text; }
            set { 
                _password = value;
                if (_passwordControl != null)
                    _passwordControl.Text = _password;
            }
        }

        /// <summary>
        /// Gets or sets a value of current component lock state
        /// </summary>
        public bool Locked
        {
            get { return !_emailControl.Enabled; }
            set {
                _emailControl.Enabled = !value;
                _passwordControl.Enabled = !value;
                _loginButton.Enabled = !value; 
            }
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

        public LoginComponent(D3DEngine engine, MainScreen screen, SandboxCommonResources commonResources)
            : base(engine, screen, commonResources)
        {
            if (engine == null) throw new ArgumentNullException("engine");
            if (screen == null) throw new ArgumentNullException("screen");
            _engine = engine;
            _screen = screen;

            _stEnter            = ToDispose(SandboxCommonResources.LoadTexture(engine, "Images\\Login\\enter.png"));
            _stEnterHover       = ToDispose(SandboxCommonResources.LoadTexture(engine, "Images\\Login\\enter_hover.png"));
            _stEnterDown        = ToDispose(SandboxCommonResources.LoadTexture(engine, "Images\\Login\\enter_press.png"));
            _stEnterDisabled    = ToDispose(SandboxCommonResources.LoadTexture(engine, "Images\\Login\\enter_disabled.png"));

            _stSign             = ToDispose(SandboxCommonResources.LoadTexture(engine, "Images\\Login\\sign.png"));
            _stSignHover        = ToDispose(SandboxCommonResources.LoadTexture(engine, "Images\\Login\\sign_hover.png"));

            _stInputBg          = ToDispose(SandboxCommonResources.LoadTexture(engine, "Images\\Login\\login_input_bg.png"));
            _stEmail            = ToDispose(SandboxCommonResources.LoadTexture(engine, "Images\\Login\\email.png"));
            _stPassword         = ToDispose(SandboxCommonResources.LoadTexture(engine, "Images\\Login\\password.png"));

            _stAutentification  = ToDispose(SandboxCommonResources.LoadTexture(engine, "Images\\Login\\authentication.png"));
            _stError            = ToDispose(SandboxCommonResources.LoadTexture(engine, "Images\\Login\\error.png"));
        }

        protected override void EngineViewPortUpdated(ViewportF viewport, Texture2DDescription newBackBuffer)
        {
            base.EngineViewPortUpdated(viewport, newBackBuffer);
            Resize(viewport);
        }

        private void Resize(ViewportF viewport)
        {
            _regButton.Bounds = new UniRectangle((viewport.Width - 562) / 2 + 408, _headerHeight + 72, 88, 83);
            _authenticationLabel.Bounds = new UniRectangle((viewport.Width - 444) / 2+ 12, _headerHeight + 133, 126, 49);
            _emailControl.Bounds = new UniRectangle((viewport.Width - 444) / 2, _headerHeight + 173, 444, 85);
            _passwordControl.Bounds = new UniRectangle((viewport.Width - 444) / 2, _headerHeight + 253, 444, 85);
            _loginButton.Bounds = new UniRectangle((viewport.Width - 444) / 2, _headerHeight + 333, 444, 85);
            _errorImage.Bounds = new UniRectangle((viewport.Width - 444) / 2 - 290, _headerHeight + 323, 285, 110);

            _errorText.Bounds = _errorImage.Bounds;
            _errorText.Bounds.Location.X += 64;
            _errorText.Bounds.Location.Y += 10;
            _errorText.Bounds.Size.X -= 75;
        }

        void GameWindowKeyPress(object sender, KeyPressEventArgs e)
        {
            if ((Keys)e.KeyChar == Keys.Return)
            {
                if (string.IsNullOrEmpty(Email))
                    _screen.FocusedControl = _emailControl;
                else if (string.IsNullOrEmpty(Password))
                    _screen.FocusedControl = _passwordControl;
                else 
                    OnLogin();
            }
        }

        public void ShowErrorText(string error)
        {
            _errorText.Text = error;

            if (string.IsNullOrEmpty(_errorText.Text))
            {
                if (_screen.Desktop.Children.Contains(_errorText))
                {
                    _screen.Desktop.Children.Remove(_errorText);
                    _screen.Desktop.Children.Remove(_errorImage);
                }
            }
            else
            {
                if (!_screen.Desktop.Children.Contains(_errorText))
                {
                    _screen.Desktop.Children.Insert(0, _errorImage);
                    _screen.Desktop.Children.Insert(0,_errorText);
                    
                }
            }
        }

        public override void Initialize()
        {
            base.Initialize();

            _authenticationLabel = new ImageControl { Image = _stAutentification };

            _emailControl = new InputControl
            {
                Text = Email,
                CustomBackground = _stInputBg,
                CustomFont = _commonResources.FontBebasNeue35,
                Color = SharpDX.Color.White,
                CustomHintImage = _stEmail,
                TextOffset = new SharpDX.Vector2(25,22),
                CustomHintImageOffset = new SharpDX.Vector2(25, 30)
            };
            
            _passwordControl = new InputControl
            {
                Text = Password,
                IsPassword = true,
                CustomBackground = _stInputBg,
                CustomFont = _commonResources.FontBebasNeue35,
                Color = SharpDX.Color.White,
                CustomHintImage = _stPassword,
                TextOffset = new SharpDX.Vector2(25,22),
                CustomHintImageOffset = new SharpDX.Vector2(25, 30)
            };
            
            _regButton = new ButtonControl
                             {
                                 CustomImage = _stSign,
                                 CustomImageHover = _stSignHover,
                                 CustomImageDown = _stSignHover
                             };

            _regButton.Pressed += delegate { OnRegister(); };
            
            _loginButton = new ButtonControl
            {
                CustomImage = _stEnter,
                CustomImageHover = _stEnterHover,
                CustomImageDown = _stEnterDown,
                CustomImageDisabled = _stEnterDisabled
            };

            _loginButton.Pressed += delegate { OnLogin(); };
            
            _errorImage = new ImageControl 
            { 
                Image = _stError 
            };

            _errorText = new LabelControl 
            {
                CustomFont = _commonResources.FontBebasNeue25,
                Color = SharpDX.Color.White,
                Autosizing = true
            };

            
        }

        public override void EnableComponent(bool forced)
        {
            if (!AutoStateEnabled && !forced) return;

            _screen.Desktop.Children.Add(_regButton);
            _screen.Desktop.Children.Add(_loginButton);
            _screen.Desktop.Children.Add(_emailControl);
            _screen.Desktop.Children.Add(_passwordControl);
            _screen.Desktop.Children.Add(_authenticationLabel);

            _screen.FocusedControl = !string.IsNullOrEmpty(Email) ? _passwordControl : _emailControl;
            _engine.GameWindow.KeyPress += GameWindowKeyPress;

            Resize(_engine.ViewPort);

            base.EnableComponent(forced);
        }

        public override void DisableComponent()
        {
            base.DisableComponent();

            _screen.Desktop.Children.Remove(_regButton);
            _screen.Desktop.Children.Remove(_loginButton);
            _screen.Desktop.Children.Remove(_emailControl);
            _screen.Desktop.Children.Remove(_passwordControl);
            _screen.Desktop.Children.Remove(_authenticationLabel);

            if (_screen.Desktop.Children.Contains(_errorText))
            {
                _screen.Desktop.Children.Remove(_errorText);
                _screen.Desktop.Children.Remove(_errorImage);
            }

            _engine.GameWindow.KeyPress -= GameWindowKeyPress;
        }
    }
}
