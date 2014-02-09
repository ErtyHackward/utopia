using System;
using Ninject;
using Realms.Client.Components.GUI;
using S33M3CoreComponents.Inputs;
using S33M3CoreComponents.States;
using S33M3CoreComponents.GUI;
using Utopia.Components;
using S33M3CoreComponents.Sound;
using Utopia.GUI.Inventory;

namespace Realms.Client.States
{
    /// <summary>
    /// Main menu state handling
    /// </summary>
    public class InGameMenuState : GameState
    {
        private readonly IKernel _ioc;
        private ISoundEngine _soundEngine;

        // do we need to capture mouse on continue?
        private bool _captureMouse;

        public override string Name
        {
            get { return "InGameMenu"; }
        }

        public InGameMenuState(GameStatesManager stateManager, IKernel iocContainer, ISoundEngine soundEngine)
            :base(stateManager)
        {
            _ioc = iocContainer;
            AllowMouseCaptureChange = false;
            _soundEngine = soundEngine;
        }

        public override void Initialize(SharpDX.Direct3D11.DeviceContext context)
        {
            var gui = _ioc.Get<GuiManager>();
            var menu = _ioc.Get<InGameMenuComponent>();
            var fade = _ioc.Get<FadeComponent>();

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
            var inputManager = _ioc.Get<InputsManager>();
            inputManager.MouseManager.MouseCapture = _captureMouse;

            StatesManager.ActivateGameStateAsync("Gameplay");
        }

        void MenuSettingsButtonPressed(object sender, EventArgs e)
        {
            StatesManager.ActivateGameStateAsync("Settings");
        }
        
        void MenuExitPressed(object sender, EventArgs e)
        {
            var vars = _ioc.Get<RealmRuntimeVariables>();
            vars.DisposeGameComponents = true;
            vars.MessageOnExit = null;

            WithPreservePreviousStates = false;
            StatesManager.ActivateGameStateAsync("MainMenu");
        }

        public override void OnEnabled(GameState previousState)
        {
            var fade = _ioc.Get<FadeComponent>();
            fade.Color = new SharpDX.Color4(0, 0, 0, 0.85f);
            fade.Visible = true;

            var inputManager = _ioc.Get<InputsManager>();
            _captureMouse = inputManager.MouseManager.MouseCapture;
            inputManager.MouseManager.MouseCapture = false;

            base.OnEnabled(previousState);
        }

        public override void OnDisabled(GameState nextState)
        {
            var fade = _ioc.Get<FadeComponent>();
            fade.Visible = false;
            
            var inputManager = _ioc.Get<InputsManager>();
            inputManager.MouseManager.MouseCapture = _captureMouse;
        }

    }
}
