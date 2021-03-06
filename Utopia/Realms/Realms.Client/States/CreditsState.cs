﻿using Ninject;
using Realms.Client.Components.GUI;
using S33M3CoreComponents.GUI;
using S33M3CoreComponents.States;

namespace Realms.Client.States
{
    /// <summary>
    /// Controls display of the authors
    /// </summary>
    public class CreditsState : GameState
    {
        private readonly IKernel _iocContainer;

        public override string Name
        {
            get { return "Credits"; }
        }

        public CreditsState(GameStatesManager stateManager, IKernel iocContainer)
            :base(stateManager)
        {
            _iocContainer = iocContainer;
            AllowMouseCaptureChange = false;
        }

        public override void Initialize(SharpDX.Direct3D11.DeviceContext context)
        {
            var bg = _iocContainer.Get<BlackBgComponent>();
            var gui = _iocContainer.Get<GuiManager>();
            var credits = _iocContainer.Get<CreditsComponent>();

            credits.BackPressed += CreditsBackPressed;

            AddComponent(bg);
            AddComponent(gui);
            AddComponent(credits);

            base.Initialize(context);
        }

        void CreditsBackPressed(object sender, System.EventArgs e)
        {
            // when you press "back" we returning to the main menu
            StatesManager.ActivateGameStateAsync("MainMenu");
        }
    }
}
