using System;
using Ninject;
using S33M3CoreComponents.Inputs;
using S33M3CoreComponents.States;
using S33M3CoreComponents.GUI;
using Sandbox.Client.Components.GUI;
using Utopia.Components;

namespace Sandbox.Client.States
{
    /// <summary>
    /// Main menu state handling
    /// </summary>
    public class InGameMenuState : GameState
    {
        private readonly IKernel _iocContainer;
        private bool _isGameExited;

        // do we need to capture mouse on continue?
        private bool _captureMouse;

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
            var gui = _iocContainer.Get<GuiManager>();
            var menu = _iocContainer.Get<InGameMenuComponent>();
            var fade = _iocContainer.Get<FadeComponent>();

            fade.Color = new SharpDX.Color4(0, 0, 0, 0.85f);

            AddComponent(fade);
            AddComponent(gui);
            AddComponent(menu);

            menu.ContinuePressed += MenuContinuePressed;
            menu.ExitPressed += MenuExitPressed;
            menu.SettingsButtonPressed += MenuSettingsButtonPressed;

            base.Initialize(context);
        }

        void MenuContinuePressed(object sender, EventArgs e)
        {
            var inputManager = _iocContainer.Get<InputsManager>();
            inputManager.MouseManager.MouseCapture = _captureMouse;

            StatesManager.ActivateGameStateAsync("Gameplay");
        }

        void MenuSettingsButtonPressed(object sender, EventArgs e)
        {
            StatesManager.ActivateGameStateAsync("Settings");
        }
        
        void MenuExitPressed(object sender, EventArgs e)
        {
            _isGameExited = true;
            WithPreservePreviousStates = false;
            StatesManager.ActivateGameStateAsync("MainMenu");
        }

        public override void OnEnabled(GameState previousState)
        {
            var inputManager = _iocContainer.Get<InputsManager>();
            _captureMouse = inputManager.MouseManager.MouseCapture;
            inputManager.MouseManager.MouseCapture = false;

            base.OnEnabled(previousState);
        }

        public override void OnDisabled(GameState nextState)
        {
            if (_isGameExited)
            {
                //Disconnect in a clean way from the server
                //var servercomp = _iocContainer.Get<ServerComponent>();
                //servercomp.Disconnect();

                //Dispose all components related to the Game scope
                GameScope.CurrentGameScope.Dispose();
                //Create a new Scope
                GameScope.CreateNewScope();
                _isGameExited = false;
            }
        }

    }
}
