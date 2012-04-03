using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3CoreComponents.GUI.Nuclex.Controls;
using S33M3DXEngine;
using SharpDX.Direct3D11;
using S33M3CoreComponents.GUI.Nuclex;

namespace Sandbox.Client.Components.GUI.Settings
{
    public partial class KeyBindingSettingsPanel : Control
    {
        #region Private Variables
        private SettingsComponent _parent;
        private string _panelName;
        #endregion

        #region Public Variables
        #endregion

        public KeyBindingSettingsPanel(SettingsComponent parent, D3DEngine engine, UniRectangle bound)
        {
            engine.ViewPort_Updated += engine_ViewPort_Updated;
            this.CanBeRendered = false;
            _panelName = "Key Bindings";
            _parent = parent;
            this.Bounds = bound;

            InitializeComponent();
        }

        void engine_ViewPort_Updated(Viewport viewport, Texture2DDescription newBackBuffer)
        {
            Resize();
        }

        #region Public Methods
        #endregion

        #region Private Methods
        #endregion
    }
}
