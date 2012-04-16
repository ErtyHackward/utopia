using System;
using System.Windows.Forms;
using Ninject;
using Sandbox.Client.Components;
using Utopia;
using S33M3CoreComponents.States;
using S33M3CoreComponents.GUI;
using Ninject.Parameters;
using Sandbox.Client.Components.GUI;
using Utopia.Network;
using Utopia.Components;

namespace Sandbox.Client.States
{
    /// <summary>
    /// Main menu state handling
    /// </summary>
    public class InGameMenuState : GameState
    {
        private readonly IKernel _iocContainer;
        private RuntimeVariables _vars;

        public override string Name
        {
            get { return "InGameMenu"; }
        }

        public InGameMenuState(GameStatesManager stateManager, IKernel iocContainer)
            :base(stateManager)
        {
            _iocContainer = iocContainer;
            AllowMouseCaptureChange = false;
        }

        public override void Initialize(SharpDX.Direct3D11.DeviceContext context)
        {
            var bg = _iocContainer.Get<BlackBgComponent>();
            var gui = _iocContainer.Get<GuiManager>();
            var menu = _iocContainer.Get<InGameMenuComponent>();
            var sound = _iocContainer.Get<GeneralSoundManager>();

            _vars = _iocContainer.Get<RuntimeVariables>();

            AddComponent(bg);
            AddComponent(gui);
            AddComponent(menu);

            menu.ContinuePressed += menu_ContinuePressed;
            menu.ExitPressed += MenuExitPressed;
            menu.SettingsButtonPressed += menuSettingsButtonPressed;

            base.Initialize(context);
        }

        void menu_ContinuePressed(object sender, EventArgs e)
        {
            StatesManager.ActivateGameStateAsync("Gameplay");
        }

        void menuSettingsButtonPressed(object sender, EventArgs e)
        {
            StatesManager.ActivateGameStateAsync("Settings");
        }

        private bool _isGameExited = false;
        void MenuExitPressed(object sender, EventArgs e)
        {
            _isGameExited = true;
            this.WithPreservePreviousStates = false;
            StatesManager.ActivateGameStateAsync("MainMenu");
        }

        public override void OnDisabled(GameState nextState)
        {
            if (_isGameExited)
            {
                //Dispose all components related to the Game scope
                GameScope.CurrentGameScope.Dispose();
                //Create a new Scope
                GameScope.CreateNewScope();
                _isGameExited = false;
            }
        }

    }
}
