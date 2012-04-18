using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3DXEngine.Main;
using S33M3DXEngine;
using SharpDX.Direct3D11;
using S33M3CoreComponents.GUI.Nuclex;
using Utopia.Shared.World;

namespace Sandbox.Client.Components.GUI.SinglePlayer
{
    public partial class SinglePlayerComponent : MenuTemplate1Component
    {
        #region Private variables
        private WorldParameters _currentWorldParameter;
        private RuntimeVariables _vars;
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

        public SinglePlayerComponent(Game game, D3DEngine engine, MainScreen screen, SandboxCommonResources commonResources, WorldParameters currentWorldParameter, RuntimeVariables var)
            : base(game, engine, screen, commonResources)
        {
            _vars = var;
            _currentWorldParameter = currentWorldParameter;
        }

        #region Public methods
        #endregion

        #region Private methods
        #endregion
    }
}
