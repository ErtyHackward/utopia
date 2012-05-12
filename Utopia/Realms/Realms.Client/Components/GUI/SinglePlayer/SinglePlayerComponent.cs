using System;
using S33M3DXEngine.Main;
using S33M3DXEngine;
using S33M3CoreComponents.GUI.Nuclex;
using Utopia.Shared.World;
using S33M3CoreComponents.GUI;

namespace Realms.Client.Components.GUI.SinglePlayer
{
    public partial class SinglePlayerComponent : MenuTemplate1Component
    {
        #region Private variables
        private WorldParameters _currentWorldParameter;
        private RuntimeVariables _vars;
        private GuiManager _guiManager;
        #endregion

        #region Public properties/Variables
        #endregion

        #region Events
        public event EventHandler StartingGameRequested;
        private void OnStartingGameRequested()
        {
            if (StartingGameRequested != null) StartingGameRequested(this, EventArgs.Empty);
        }

        #endregion

        public SinglePlayerComponent(Game game, D3DEngine engine, MainScreen screen, SandboxCommonResources commonResources, WorldParameters currentWorldParameter, RuntimeVariables var, GuiManager guiManager)
            : base(game, engine, screen, commonResources)
        {
            _guiManager = guiManager;
            _vars = var;
            _currentWorldParameter = currentWorldParameter;
        }

        public override void Update(GameTime timeSpent)
        {
            if (_savedGamePanel.NeedShowResults)
                _savedGamePanel.ShowResults();

            base.Update(timeSpent);
        }

    }
}
