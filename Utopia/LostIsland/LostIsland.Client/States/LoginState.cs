using System;
using Ninject;
using Utopia;
using Utopia.GUI.D3D;

namespace LostIsland.Client.States
{
    /// <summary>
    /// First state of the game, requests login and password
    /// </summary>
    public class LoginState : GameState
    {
        public override string Name
        {
            get { return "Login"; }
        }

        public LoginState(IKernel iocContainer)
        {
            var gui = iocContainer.Get<GuiManager>();
            var login = iocContainer.Get<LoginComponent>();

            login.Login += LoginLogin;
            
            EnabledComponents.Add(gui);
            EnabledComponents.Add(login);

            VisibleComponents.Add(gui);
        }

        public override void OnEnabled(GameState previousState)
        {
            // start preparing of the main menu
            StatesManager.PrepareStateAsync("MainMenu");
        }

        void LoginLogin(object sender, EventArgs e)
        {
            // for now, while we have not a web part just open the main menu
            StatesManager.SetGameState("MainMenu");
        }
    }
}
