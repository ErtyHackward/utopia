using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject;
using S33M3CoreComponents.GUI;
using S33M3CoreComponents.States;
using Utopia.Components;

namespace Realms.Client.States
{
    public class InGameCraftingState : GameState
    {
        private readonly IKernel _iocContainer;

        public InGameCraftingState(GameStatesManager stateManager, IKernel iocContainer)
            : base(stateManager)
        {
            _iocContainer = iocContainer;
        }

        public override string Name
        {
            get { return "Crafting"; }
        }

        public override void OnEnabled(GameState previousState)
        {
            var fadeComponent = _iocContainer.Get<FadeComponent>();
            fadeComponent.Color = new SharpDX.Color4(0, 0, 0, 0.85f);
            fadeComponent.Visible = true;

            var guiManager = _iocContainer.Get<GuiManager>();
            guiManager.SetDialogMode(true);

            base.OnEnabled(previousState);
        }

        public override void OnDisabled(GameState nextState)
        {
            var fadeComponent = _iocContainer.Get<FadeComponent>();
            fadeComponent.Visible = false;

            var guiManager = _iocContainer.Get<GuiManager>();
            guiManager.SetDialogMode(false);

            base.OnDisabled(nextState);
        }
    }
}
