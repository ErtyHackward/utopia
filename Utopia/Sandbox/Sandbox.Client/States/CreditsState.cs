using Ninject;
using Sandbox.Client.Components;
using Utopia;
using S33M3_CoreComponents.GUI;
using S33M3_CoreComponents.States;

namespace Sandbox.Client.States
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

        }

        public override void Initialize()
        {
            var bg = _iocContainer.Get<BlackBgComponent>();
            var gui = _iocContainer.Get<GuiManager>();
            var credits = _iocContainer.Get<CreditsComponent>();

            credits.BackPressed += CreditsBackPressed;

            AddComponent(bg);
            AddComponent(gui);
            AddComponent(credits);
        }

        void CreditsBackPressed(object sender, System.EventArgs e)
        {
            // when you press "back" we returning to the main menu
            StatesManager.SetGameState("MainMenu");
        }
    }
}
