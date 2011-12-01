using System;
using LostIsland.Client.Components;
using Ninject;
using Utopia;
using Utopia.GUI.D3D;

namespace LostIsland.Client.States
{
    /// <summary>
    /// Main menu state handling
    /// </summary>
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

            menu.CreditsPressed += MenuCreditsPressed;

        }

        public override void OnEnabled(GameState previousState)
        {
            StatesManager.PrepareStateAsync("GameLoading");
            base.OnEnabled(previousState);
        }
        
        void MenuCreditsPressed(object sender, EventArgs e)
        {
            // showing the credits
            StatesManager.SetGameState("Credits");
        }
    }
}
