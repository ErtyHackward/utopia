using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3CoreComponents.GUI.Nuclex.Controls;
using Utopia.Shared.World;

namespace Sandbox.Client.Components.GUI.SinglePlayer
{
    public partial class NewGamePanel : Control
    {
        #region Private variables
        private SandboxCommonResources _commonResources;
        private WorldParameters _currentWorldParameter;
        #endregion

        #region Public variable/properties
        #endregion

        public NewGamePanel(SandboxCommonResources commonResources, WorldParameters currentWorldParameter, RuntimeVariables vars)
        {
            _commonResources = commonResources;
            _currentWorldParameter = currentWorldParameter;
            InitializeComponent();

            this.IsVisible = true;
            this.IsRendable = false;
        }

        #region Public methods
        #endregion

        #region Private methods
        #endregion
    }
}
