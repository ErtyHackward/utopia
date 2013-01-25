using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3CoreComponents.States;
using Ninject;
using S33M3CoreComponents.GUI;
using Sandbox.Client.Components.GUI;
using Sandbox.Client.Components.GUI.SinglePlayer;

namespace Sandbox.Client.States
{
    public class SinglePlayerMenuState : GameState
    {
        private readonly IKernel _iocContainer;
        private SandboxRuntimeVariables _vars;

        public override string Name
        {
            get { return "SinglePlayerMenu"; }
        }

        public SinglePlayerMenuState(GameStatesManager stateManager, IKernel iocContainer)
            : base(stateManager)
        {
            _iocContainer = iocContainer;
            AllowMouseCaptureChange = false;
        }

        public override void Initialize(SharpDX.Direct3D11.DeviceContext context)
        {
            var gui = _iocContainer.Get<GuiManager>();
            var singlePlayer = _iocContainer.Get<SinglePlayerComponent>();
            _vars = _iocContainer.Get<SandboxRuntimeVariables>();

            AddComponent(singlePlayer);
            AddComponent(gui);

            singlePlayer.BackPressed += settings_BackPressed;
            singlePlayer.StartingGameRequested += singlePlayer_StartingGameRequested;

            base.Initialize(context);
        }

        void singlePlayer_StartingGameRequested(object sender, EventArgs e)
        {
            _vars.SinglePlayer = true;
            StatesManager.ActivateGameStateAsync("LoadingGame");
        }

        void settings_BackPressed(object sender, EventArgs e)
        {
            StatesManager.ActivateGameStateAsync(this.PreviousGameState);
        }
    }
}
