using System;
using System.Windows.Forms;
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
        private readonly IKernel _iocContainer;
        private RuntimeVariables _vars;

        public override string Name
        {
            get { return "MainMenu"; }
        }

        public MainMenuState(IKernel iocContainer)
        {
            _iocContainer = iocContainer;
        }

        public override void Initialize()
        {
            var gui = _iocContainer.Get<GuiManager>();
            var menu = _iocContainer.Get<MainMenuComponent>();
            _vars = _iocContainer.Get<RuntimeVariables>();

            AddComponent(gui);
            AddComponent(menu);
            
            menu.CreditsPressed += MenuCreditsPressed;
            menu.SinglePlayerPressed += MenuSinglePlayerPressed;
            menu.MultiplayerPressed += MenuMultiplayerPressed;
            menu.ExitPressed += menu_ExitPressed;
        }

        void menu_ExitPressed(object sender, EventArgs e)
        {
            Application.ExitThread();
        }

        void MenuMultiplayerPressed(object sender, EventArgs e)
        {
            _vars.SinglePlayer = false;
            StatesManager.SetGameState("SelectServer");
        }

        void MenuSinglePlayerPressed(object sender, EventArgs e)
        {
            _vars.SinglePlayer = true;
            StatesManager.SetGameState("GameLoading");
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
