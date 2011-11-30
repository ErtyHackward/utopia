using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject;
using Utopia;
using Utopia.GUI.D3D;

namespace LostIsland.Client.States
{
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

            login.Login += login_Login;
            
            EnabledComponents.Add(gui);
            EnabledComponents.Add(login);

            VisibleComponents.Add(gui);
        }

        public override void OnEnabled(GameState previousState)
        {
            StatesManager.PrepareStateAsync("MainMenu");
            
            base.OnEnabled(previousState);
        }

        void login_Login(object sender, EventArgs e)
        {
            StatesManager.SetGameState("MainMenu");
        }
    }
}
