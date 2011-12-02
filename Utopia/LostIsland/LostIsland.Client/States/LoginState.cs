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

            login.Login += LoginLogin;

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
            // for now, while we have not a web part just open the main menu
            StatesManager.SetGameState("MainMenu");
        }
    }
}
