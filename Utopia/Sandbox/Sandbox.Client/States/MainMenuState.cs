using System;
using System.Windows.Forms;
using Ninject;
using Sandbox.Client.Components;
using Utopia;
using S33M3CoreComponents.States;
using S33M3CoreComponents.GUI;

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

        public MainMenuState(GameStatesManager stateManager, IKernel iocContainer)
            :base(stateManager)
        {
            _iocContainer = iocContainer;
        }

        public override void Initialize(SharpDX.Direct3D11.DeviceContext context)
        {
            var bg = _iocContainer.Get<BlackBgComponent>();
            var gui = _iocContainer.Get<GuiManager>();
            var menu = _iocContainer.Get<MainMenuComponent>();
            _vars = _iocContainer.Get<RuntimeVariables>();

            AddComponent(bg);
            AddComponent(gui);
            AddComponent(menu);

            menu.CreditsPressed += MenuCreditsPressed;
            menu.SinglePlayerPressed += MenuSinglePlayerPressed;
            menu.MultiplayerPressed += MenuMultiplayerPressed;
            menu.EditorPressed += MenuEditorPressed;
            menu.ExitPressed += MenuExitPressed;

            base.Initialize(context);
        }

        void MenuEditorPressed(object sender, EventArgs e)
        {
            StatesManager.ActivateGameState("Editor");
        }

        void MenuExitPressed(object sender, EventArgs e)
        {
            Application.ExitThread();
        }

        void MenuMultiplayerPressed(object sender, EventArgs e)
        {
            _vars.SinglePlayer = false;
            StatesManager.ActivateGameState("SelectServer");
        }

        void MenuSinglePlayerPressed(object sender, EventArgs e)
        {
            _vars.SinglePlayer = true;
            StatesManager.ActivateGameState("LoadingGame");
        }

        public override void OnEnabled(GameState previousState)
        {
            StatesManager.PrepareState("LoadingGame");
            base.OnEnabled(previousState);
        }
        
        void MenuCreditsPressed(object sender, EventArgs e)
        {
            // showing the credits
            StatesManager.ActivateGameState("Credits");
        }
    }
}
