using LostIsland.Client.GUI;
using Ninject;
using Utopia;

namespace LostIsland.Client.States
{
    /// <summary>
    /// Loads everything needed for the gameplay process
    /// Shows loading screen
    /// </summary>
    public class GameLoadingState : GameState
    {
        public override string Name
        {
            get { return "GameLoading"; }
        }

        public GameLoadingState(IKernel iocContainer)
        {
            var loading = iocContainer.Get<LoadingComponent>();

            EnabledComponents.Add(loading);
            VisibleComponents.Add(loading);
        }

        public override void OnEnabled(GameState previousState)
        {
            // here we need to start loading of the game components
            
            base.OnEnabled(previousState);
        }
    }
}
