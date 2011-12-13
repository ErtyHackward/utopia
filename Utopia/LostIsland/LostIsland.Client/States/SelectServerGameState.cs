using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LostIsland.Client.Components;
using Ninject;
using Utopia;
using Utopia.GUI.D3D;

namespace LostIsland.Client.States
{
    public class SelectServerGameState : GameState
    {
        private readonly IKernel _iocContainer;

        public override string Name
        {
            get { return "SelectServer"; }
        }

        public SelectServerGameState(IKernel iocContainer)
        {
            _iocContainer = iocContainer;
        }

        public override void Initialize()
        {
            var gui = _iocContainer.Get<GuiManager>();
            var selection = _iocContainer.Get<ServerSelectionComponent>();

            EnabledComponents.Add(gui);
            EnabledComponents.Add(selection);

            VisibleComponents.Add(gui);

            selection.BackPressed += SelectionBackPressed;
        }

        void SelectionBackPressed(object sender, System.EventArgs e)
        {
            // when you press "back" we returning to the main menu
            StatesManager.SetGameState("MainMenu");
        }
    }
}
