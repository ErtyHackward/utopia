using LostIsland.Client.Components;
using Ninject;
using Utopia;
using Utopia.GUI.D3D;

namespace LostIsland.Client.States
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

        public CreditsState(IKernel iocContainer)
        {
            _iocContainer = iocContainer;

        }

        public override void Initialize()
        {
            var gui = _iocContainer.Get<GuiManager>();
            var credits = _iocContainer.Get<CreditsComponent>();

            EnabledComponents.Add(gui);
            EnabledComponents.Add(credits);

            VisibleComponents.Add(gui);

            credits.BackPressed += CreditsBackPressed;
        }

        void CreditsBackPressed(object sender, System.EventArgs e)
        {
            // when you press "back" we returning to the main menu
            StatesManager.SetGameState("MainMenu");
        }
    }
}
