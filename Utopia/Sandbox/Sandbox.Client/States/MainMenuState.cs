using System;
using System.Windows.Forms;
using Ninject;
using Sandbox.Client.Components;
using Utopia;
using S33M3CoreComponents.States;
using S33M3CoreComponents.GUI;
using Ninject.Parameters;
using Sandbox.Client.Components.GUI;
using Utopia.Components;

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
            AllowMouseCaptureChange = false;
        }

        public override void Initialize(SharpDX.Direct3D11.DeviceContext context)
        {
            var bg = _iocContainer.Get<BlackBgComponent>();
            var gui = _iocContainer.Get<GuiManager>();
            var menu = _iocContainer.Get<MainMenuComponent>();
            var sound = _iocContainer.Get<GeneralSoundManager>();
            _vars = _iocContainer.Get<RuntimeVariables>();

            AddComponent(bg);
            AddComponent(gui);
            AddComponent(menu);

            base.Initialize(context);
        }

        public override void OnEnabled(GameState previousState)
        {
            var menu = _iocContainer.Get<MainMenuComponent>();
            menu.CreditsPressed += MenuCreditsPressed;
            menu.SinglePlayerPressed += MenuSinglePlayerPressed;
            menu.MultiplayerPressed += MenuMultiplayerPressed;
            menu.EditorPressed += MenuEditorPressed;
            menu.ExitPressed += MenuExitPressed;
            menu.SettingsButtonPressed += menuSettingsButtonPressed;

            StatesManager.PrepareStateAsync("LoadingGame");
            base.OnEnabled(previousState);
        }

        public override void OnDisabled(GameState nextState)
        {
            var menu = _iocContainer.Get<MainMenuComponent>();
            menu.CreditsPressed -= MenuCreditsPressed;
            menu.SinglePlayerPressed -= MenuSinglePlayerPressed;
            menu.MultiplayerPressed -= MenuMultiplayerPressed;
            menu.EditorPressed -= MenuEditorPressed;
            menu.ExitPressed -= MenuExitPressed;
            menu.SettingsButtonPressed -= menuSettingsButtonPressed;
        }

        void menuSettingsButtonPressed(object sender, EventArgs e)
        {
            var state = StatesManager.GetByName("Settings");
            state.
            StatesManager.ActivateGameStateAsync(state);
        }

        void MenuEditorPressed(object sender, EventArgs e)
        {
            StatesManager.ActivateGameStateAsync("Editor");
        }

        void MenuExitPressed(object sender, EventArgs e)
        {
            Application.ExitThread();
        }

        void MenuMultiplayerPressed(object sender, EventArgs e)
        {
            _vars.SinglePlayer = false;
            StatesManager.ActivateGameStateAsync("SelectServer");
        }

        void MenuSinglePlayerPressed(object sender, EventArgs e)
        {
            _vars.SinglePlayer = true;
            //StatesManager.ActivateGameStateAsync("LoadingGame");

            StatesManager.ActivateGameStateAsync("SinglePlayerMenu");
        }
        
        void MenuCreditsPressed(object sender, EventArgs e)
        {
            // showing the credits
            StatesManager.ActivateGameStateAsync("Credits");
        }
    }
}
