using System;
using Realms.Client.Components.GUI.Settings;
using S33M3CoreComponents.States;
using Ninject;
using S33M3CoreComponents.GUI;
using S33M3CoreComponents.Inputs;

namespace Realms.Client.States
{
    public class SettingsState : GameState
    {
        private readonly IKernel _iocContainer;

        public override string Name
        {
            get { return "Settings"; }
        }

        public SettingsState(GameStatesManager stateManager, IKernel iocContainer)
            : base(stateManager)
        {
            _iocContainer = iocContainer;
            AllowMouseCaptureChange = false;
        }

        public override void Initialize(SharpDX.Direct3D11.DeviceContext context)
        {
            var gui = _iocContainer.Get<GuiManager>();
            var settings = _iocContainer.Get<SettingsComponent>();

            AddComponent(settings);
            AddComponent(gui);

            settings.BackPressed += settings_BackPressed;

            base.Initialize(context);
        }

        public override void OnEnabled(GameState previousState)
        {
            var settings = _iocContainer.Get<SettingsComponent>();
            var inputManager = _iocContainer.Get<InputsManager>();
            inputManager.MouseManager.MouseCapture = false;
            settings.isGameRunning = this.PreviousGameState == StatesManager.GetByName("InGameMenu");
        }

        void settings_BackPressed(object sender, EventArgs e)
        {
            StatesManager.ActivateGameStateAsync(this.PreviousGameState);
        }

    }
}
