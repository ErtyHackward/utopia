﻿using LostIsland.Client.Components;
using Ninject;
using Utopia;
using Utopia.GUI.D3D;

namespace LostIsland.Client.States
{
    public class CreditsState : GameState
    {
        public override string Name
        {
            get { return "Credits"; }
        }

        public CreditsState(IKernel iocContainer)
        {
            var gui = iocContainer.Get<GuiManager>();
            var credits = iocContainer.Get<CreditsComponent>();
            
            EnabledComponents.Add(gui);
            EnabledComponents.Add(credits);

            VisibleComponents.Add(gui);

            credits.BackPressed += CreditsBackPressed;
        }

        void CreditsBackPressed(object sender, System.EventArgs e)
        {
            StatesManager.SetGameState("MainMenu");
        }
    }
}
