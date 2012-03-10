using System;
using System.Diagnostics;
using Ninject;
using Sandbox.Client.Components;
using Sandbox.Shared.Web;
using Sandbox.Shared.Web.Responces;
using Utopia;
using Utopia.Components;
using Utopia.Settings;
using Utopia.Shared.ClassExt;
using S33M3_CoreComponents.States;
using Utopia.GUI;
using S33M3_CoreComponents.GUI;

namespace Sandbox.Client.States
{
    /// <summary>
    /// First state of the game, requests login and password
    /// </summary>
    public class LoginState : GameState
    {
        private readonly IKernel _iocContainer;
        private ClientWebApi _webApi;

        public override string Name
        {
            get { return "Login"; }
        }

        public LoginState(GameStatesManager stateManager, IKernel iocContainer)
            :base(stateManager)
        {
            _iocContainer = iocContainer;
        }

        public override void Initialize(SharpDX.Direct3D11.DeviceContext context)
        {
            var bg = _iocContainer.Get<BlackBgComponent>();
            var gui = _iocContainer.Get<GuiManager>();
            var login = _iocContainer.Get<LoginComponent>();
            var sound = _iocContainer.Get<SoundManager>();
            _webApi = _iocContainer.Get<ClientWebApi>();

            login.Email = ClientSettings.Current.Settings.Login;

            login.Login += LoginLogin;
            login.Register += delegate { try { Process.Start("http://api.cubiquest.com/register"); } catch { } };

            _webApi.LoginCompleted += WebApiLoginCompleted;

            AddComponent(bg);
            AddComponent(gui);
            AddComponent(login);
            AddComponent(sound);
            base.Initialize(context);
        }

        void WebApiLoginCompleted(object sender, WebEventArgs<LoginResponce> e)
        {
            var login = _iocContainer.Get<LoginComponent>();
            var gui = _iocContainer.Get<GuiManager>();

            if (e.Exception != null)
            {
                gui.MessageBox("Error: "+e.Exception.Message);
                login.Locked = false;
                return;
            }

            if (!e.Responce.Logged)
            {
                gui.MessageBox("Wrong login/password combination, try again or register.");
                login.Locked = false;
                return;
            }

            if (e.Responce != null && e.Responce.Logged)
            {
                var vars = _iocContainer.Get<RuntimeVariables>();

                vars.Login = login.Email;
                vars.PasswordHash = login.Password.GetSHA1Hash();
                vars.DisplayName = e.Responce.DisplayName;

                ClientSettings.Current.Settings.Login = login.Email;
                ClientSettings.Current.Save();

                

                StatesManager.SetGameState("MainMenu");
            }
            
        }
        
        public override void OnEnabled(GameState previousState)
        {
            // start preparing of the main menu
            StatesManager.PrepareStateAsync("MainMenu");
        }

        public override void OnDisabled(GameState nextState)
        {
            var login = _iocContainer.Get<LoginComponent>();
            login.Password = null;
            base.OnDisabled(nextState);
        }

        void LoginLogin(object sender, EventArgs e)
        {
            var login = _iocContainer.Get<LoginComponent>();

            login.Locked = true;

            if (string.IsNullOrWhiteSpace(login.Email) || string.IsNullOrWhiteSpace(login.Password))
            {
                var gui = _iocContainer.Get<GuiManager>();

                gui.MessageBox("Please fill the form before press a login button", "Error", null, delegate { login.Locked = false; });
                return;
            }


            // request our server for a authorization
            _webApi.UserLoginAsync(login.Email, login.Password.GetSHA1Hash());

            
        }
    }
}
