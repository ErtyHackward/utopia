using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3CoreComponents.GUI.Nuclex.Controls;
using Utopia.Shared.World;
using S33M3CoreComponents.GUI;

namespace Sandbox.Client.Components.GUI.SinglePlayer
{
    public partial class NewGamePanel : Control
    {
        #region Private variables
        private SandboxCommonResources _commonResources;
        private WorldParameters _currentWorldParameter;
        private SandboxRuntimeVariables _vars;
        private GuiManager _guiManager;
        #endregion

        #region Public variable/properties
        #endregion

        public NewGamePanel(SandboxCommonResources commonResources, WorldParameters currentWorldParameter, SandboxRuntimeVariables vars, GuiManager guiManager)
        {
            _vars = vars;
            _guiManager = guiManager;
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
