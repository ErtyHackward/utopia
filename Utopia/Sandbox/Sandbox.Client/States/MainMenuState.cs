using System;
using System.Windows.Forms;
using Ninject;
using Sandbox.Client.Components;
using Utopia;
using Utopia.GUI.D3D;

namespace Sandbox.Client.States
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
            menu.EditorPressed += MenuEditorPressed;
            menu.ExitPressed += MenuExitPressed;
        }

        void MenuEditorPressed(object sender, EventArgs e)
        {
            StatesManager.SetGameState("Editor");
        }

        void MenuExitPressed(object sender, EventArgs e)
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
