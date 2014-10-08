using System;
using System.Diagnostics;
using Ninject;
using Realms.Client.Components.GUI;
using Utopia.Components;
using Utopia.Shared.ClassExt;
using S33M3CoreComponents.States;
using S33M3CoreComponents.GUI;
using Utopia.Shared.Net.Web;
using Utopia.Shared.Net.Web.Responses;
using Utopia.Shared.Settings;
using Utopia.Sounds;

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
            login.Register += delegate { try { Process.Start("http://utopiarealms.com"); } catch { } };

            _webApi.TokenReceived += WebApiTokenReceived;
            _webApi.TokenVerified += WebApiTokenVerified;

            AddComponent(bg);
            AddComponent(gui);
            AddComponent(login);
            AddComponent(sound);
            base.Initialize(context);

            ////Bypassing Login check
            //var vars = _iocContainer.Get<RealmRuntimeVariables>();

            //vars.Login = login.Email;
            //vars.PasswordHash = login.Password.GetSHA1Hash();

            //ClientSettings.Current.Settings.Login = login.Email;
            //ClientSettings.Current.Settings.Token = "??/";
            //ClientSettings.Current.Settings.PasswordHash = vars.PasswordHash;
            //ClientSettings.Current.Save();

            //vars.DisplayName = "Local TEST";

            //StatesManager.ActivateGameStateAsync("MainMenu");
        }

        void WebApiTokenVerified(object sender, VerifyResponse e)
        {
            if (e.Error != 0 || e.Exception != null)
            {
                var login = _iocContainer.Get<LoginComponent>();
                if (e.Exception != null)
                    login.ShowErrorText("Error: " + e.Exception.Message);
                else
                    login.ShowErrorText("Error: " + e.ErrorText);
                login.Locked = false;
                return;
            }

            var vars = _iocContainer.Get<RealmRuntimeVariables>();
            vars.DisplayName = e.DisplayName;

            StatesManager.ActivateGameStateAsync("MainMenu");
        }

        void WebApiTokenReceived(object sender, TokenResponse e)
        {
            var login = _iocContainer.Get<LoginComponent>();

            if (e.Exception != null)
            {
                login.ShowErrorText(e.Exception.Message);
                login.Locked = false;
                return;
            }

            if (e.Error == 2)
            {
                login.ShowErrorText("You don't have realms account");
                login.Locked = false;
                return;
            }

            if (string.IsNullOrEmpty(e.AccessToken))
            {
                login.ShowErrorText("Wrong login/password combination");
                login.Locked = false;
                return;
            }
            
            var vars = _iocContainer.Get<RealmRuntimeVariables>();

            vars.Login = login.Email;
            vars.PasswordHash = login.Password.GetSHA1Hash();
                
            ClientSettings.Current.Settings.Login = login.Email;
            ClientSettings.Current.Settings.Token = e.AccessToken;
            ClientSettings.Current.Settings.PasswordHash = vars.PasswordHash;
            ClientSettings.Current.Save();
            
            // verify token to receive nickname
            _webApi.OauthVerifyTokenAsync(e.AccessToken);
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
            _webApi.OauthTokenAsync(login.Email, (login.Password.GetSHA1Hash() + login.Email.ToLower()).GetSHA1Hash());
        }
    }
}
