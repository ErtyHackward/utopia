using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3CoreComponents.GUI.Nuclex.Controls;

namespace Sandbox.Client.Components.GUI.SinglePlayer
{
    public partial class SavedGamePanel : Control
    {
        #region Private variables
        private SandboxCommonResources _commonResources;
        #endregion

        #region Public variable/properties
        #endregion

        public SavedGamePanel(SandboxCommonResources commonResources)
        {
            _commonResources = commonResources;
            InitializeComponent();
        }

        #region Public methods
        #endregion

        #region Private methods
        #endregion
    }
}
