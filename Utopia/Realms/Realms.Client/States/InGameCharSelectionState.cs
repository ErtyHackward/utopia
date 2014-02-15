using Ninject;
using S33M3CoreComponents.GUI;
using S33M3CoreComponents.States;
using Utopia.Components;
using Utopia.GUI.CharacterSelection;

namespace Realms.Client.States
{
    public class InGameCharSelectionState : GameState
    {
        private readonly IKernel _iocContainer;

        public override string Name
        {
            get { return "CharSelection"; }
        }

        public InGameCharSelectionState(GameStatesManager statesManager, IKernel iocContainer) : base(statesManager)
        {
            _iocContainer = iocContainer;
        }

        public override void OnEnabled(GameState previousState)
        {
            var fadeComponent = _iocContainer.Get<FadeComponent>();
            fadeComponent.Color = new SharpDX.Color4(0, 0, 0, 0.85f);
            fadeComponent.Visible = true;

            var guiManager = _iocContainer.Get<GuiManager>();
            guiManager.SetDialogMode(true);

            var charSelection = _iocContainer.Get<CharacterSelectionComponent>();
            charSelection.ShowSelection();
            charSelection.SelectionWindow.SelectionButton.Pressed += SelectionButton_Clicked;

            base.OnEnabled(previousState);
        }

        void SelectionButton_Clicked(object sender, System.EventArgs e)
        {
            StatesManager.ActivateGameStateAsync("Gameplay");
        }

        public override void OnDisabled(GameState nextState)
        {
            var fadeComponent = _iocContainer.Get<FadeComponent>();
            fadeComponent.Visible = false;

            var guiManager = _iocContainer.Get<GuiManager>();
            guiManager.SetDialogMode(false);

            var charSelection = _iocContainer.Get<CharacterSelectionComponent>();
            charSelection.HideSelection();
            charSelection.SelectionWindow.SelectionButton.Pressed += SelectionButton_Clicked;

            base.OnDisabled(nextState);
        }
    }
}
