using System;
using Realms.Client.Components.GUI.SinglePlayer;
using S33M3CoreComponents.States;
using Ninject;
using S33M3CoreComponents.GUI;

namespace Realms.Client.States
{
    public class SinglePlayerMenuState : GameState
    {
        private readonly IKernel _iocContainer;
        private RealmRuntimeVariables _vars;

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
            _vars = _iocContainer.Get<RealmRuntimeVariables>();

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
