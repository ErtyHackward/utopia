using System;
using System.Diagnostics;
using Ninject;
using Utopia;
using Utopia.GUI.D3D;
using Utopia.Settings;

namespace LostIsland.Client.States
{
    /// <summary>
    /// First state of the game, requests login and password
    /// </summary>
    public class LoginState : GameState
    {
        private readonly IKernel _iocContainer;

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

            login.Email = ClientSettings.Current.Settings.Login;

            login.Login += LoginLogin;
            login.Register += delegate { try { Process.Start("http://api.cubiquest.com/register"); } catch { } };

            AddComponent(gui);
            AddComponent(login);
        }
        
        public override void OnEnabled(GameState previousState)
        {
            // start preparing of the main menu
            StatesManager.PrepareStateAsync("MainMenu");
        }

        void LoginLogin(object sender, EventArgs e)
        {
            // request our server for a authorization
            StatesManager.SetGameState("MainMenu");
        }
    }
}
