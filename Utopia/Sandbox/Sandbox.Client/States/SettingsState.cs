using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3CoreComponents.States;
using Ninject;
using Sandbox.Client.Components;
using S33M3CoreComponents.GUI;
using Sandbox.Client.Components.GUI.Settings;

namespace Sandbox.Client.States
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

        void settings_BackPressed(object sender, EventArgs e)
        {
            StatesManager.ActivateGameStateAsync("MainMenu");
        }

    }
}
