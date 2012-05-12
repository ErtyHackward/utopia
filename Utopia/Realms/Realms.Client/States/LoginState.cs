using System;
using System.Diagnostics;
using Ninject;
using Realms.Client.Components.GUI;
using Utopia.Components;
using Utopia.Shared.ClassExt;
using S33M3CoreComponents.States;
using S33M3CoreComponents.GUI;
using Utopia.Shared.Net.Web;
using Utopia.Shared.Net.Web.Responces;
using Utopia.Shared.Settings;

namespace Realms.Client.States
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
            AllowMouseCaptureChange = false;
        }

        public override void Initialize(SharpDX.Direct3D11.DeviceContext context)
        {
            var bg = _iocContainer.Get<BlackBgComponent>();
            var gui = _iocContainer.Get<GuiManager>();
            var login = _iocContainer.Get<LoginComponent>();
            var sound = _iocContainer.Get<GeneralSoundManager>();
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

            if (e.Exception != null)
            {
                login.ShowErrorText(e.Exception.Message);
                login.Locked = false;
                return;
            }

            if (!e.Responce.Logged)
            {
                login.ShowErrorText("Wrong login/password combination");
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

                StatesManager.ActivateGameStateAsync("MainMenu");
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
            
            if (string.IsNullOrWhiteSpace(login.Email) || string.IsNullOrWhiteSpace(login.Password))
            {
                login.ShowErrorText("Please, fill the form first");
                return;
            }

            login.ShowErrorText("");

            login.Locked = true;

            // request our server for authorization
            _webApi.UserLoginAsync(login.Email, login.Password.GetSHA1Hash());
        }
    }
}
