using System;
using System.Diagnostics;
using LostIsland.Shared.Web;
using Ninject;
using Utopia;
using Utopia.GUI.D3D;
using Utopia.Settings;
using Utopia.Shared.ClassExt;

namespace LostIsland.Client.States
{
    /// <summary>
    /// First state of the game, requests login and password
    /// </summary>
    public class LoginState : GameState
    {
        private readonly IKernel _iocContainer;
        private UtopiaWebApi _webApi;

        public override string Name
        {
            get { return "Login"; }
        }

        public LoginState(IKernel iocContainer)
        {
            _iocContainer = iocContainer;
        }

        public override void Initialize()
        {
            var gui = _iocContainer.Get<GuiManager>();
            var login = _iocContainer.Get<LoginComponent>();
            _webApi = _iocContainer.Get<UtopiaWebApi>();

            login.Email = ClientSettings.Current.Settings.Login;

            login.Login += LoginLogin;
            login.Register += delegate { try { Process.Start("http://api.cubiquest.com/register"); } catch { } };

            _webApi.LoginCompleted += WebApiLoginCompleted;

            AddComponent(gui);
            AddComponent(login);
        }

        void WebApiLoginCompleted(object sender, WebEventArgs<LoginResponce> e)
        {
            if (e.Exception != null)
            {
                var gui = _iocContainer.Get<GuiManager>();

                gui.MessageBox("Exception occured");
                return;
            }

            if (e.Responce != null && e.Responce.Logged)
            {
                StatesManager.SetGameState("MainMenu");
            }
            
        }
        
        public override void OnEnabled(GameState previousState)
        {
            // start preparing of the main menu
            StatesManager.PrepareStateAsync("MainMenu");
        }

        void LoginLogin(object sender, EventArgs e)
        {
            var login = _iocContainer.Get<LoginComponent>();

            if (string.IsNullOrWhiteSpace(login.Email) || string.IsNullOrWhiteSpace(login.Password))
            {
                var gui = _iocContainer.Get<GuiManager>();

                gui.MessageBox("Please fill the form before press a login button");
                return;
            }


            // request our server for a authorization
            _webApi.UserLogin(login.Email, login.Password.GetSHA1Hash());

            login.Locked = true;
        }
    }
}
