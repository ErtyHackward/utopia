using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LostIsland.Client.Components;
using Ninject;
using Utopia;
using Utopia.GUI.D3D;

namespace LostIsland.Client.States
{
    public class MainMenuState : GameState
    {
        public override string Name
        {
            get { return "MainMenu"; }
        }

        public MainMenuState(IKernel iocContainer)
        {
            var gui = iocContainer.Get<GuiManager>();
            var menu = iocContainer.Get<MainMenuComponent>();


            EnabledComponents.Add(gui);
            EnabledComponents.Add(menu);

            VisibleComponents.Add(gui);

            menu.CreditsPressed += menu_CreditsPressed;

        }

        void menu_CreditsPressed(object sender, EventArgs e)
        {
            StatesManager.SetGameState("Credits");
        }
    }
}
