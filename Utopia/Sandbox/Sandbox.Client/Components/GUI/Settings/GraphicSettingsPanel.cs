using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3CoreComponents.GUI.Nuclex.Controls;
using S33M3DXEngine;
using S33M3CoreComponents.GUI.Nuclex;

namespace Sandbox.Client.Components.GUI.Settings
{
    public partial class GraphicSettingsPanel : Control
    {
        #region Private Variables
        private Control _parent;
        #endregion

        #region Public Variables
        #endregion

        public GraphicSettingsPanel(Control parent)
        {
            _parent = parent;
            this.CanBeRendered = false;
            //initialize the graphical component of the pannel
            InitializeComponent();
        }

        #region Public Methods
        #endregion

        #region Private Methods
        #endregion
    }
}
