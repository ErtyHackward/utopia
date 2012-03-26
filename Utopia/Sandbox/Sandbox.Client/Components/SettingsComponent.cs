using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3DXEngine.Main;
using S33M3DXEngine;
using S33M3CoreComponents.GUI.Nuclex;
using S33M3CoreComponents.GUI.Nuclex.Controls;
using S33M3CoreComponents.GUI.Nuclex.Controls.Desktop;
using SharpDX.Direct3D11;
using SharpDX;
using S33M3CoreComponents.GUI.Nuclex.Controls.Arcade;

namespace Sandbox.Client.Components
{
    public partial class SettingsComponent : GameComponent
    {
        #region Private variables
        private readonly D3DEngine _engine;
        private readonly MainScreen _screen;

        #endregion

        #region Public properties/methods
        #endregion

        public SettingsComponent(D3DEngine engine, MainScreen screen)
        {
            _engine = engine;
            _screen = screen;

            _engine.ViewPort_Updated += UpdateLayout;
        }

        public override void Dispose()
        {
            _engine.ViewPort_Updated -= UpdateLayout;
            base.Dispose();
        }

        #region Public methods
        public override void Initialize()
        {
            InitializeComponent();

            RefreshComponentsVisibility();
        }

        protected override void OnUpdatableChanged(object sender, EventArgs args)
        {
            if (!IsInitialized) return;

            RefreshComponentsVisibility();

            base.OnUpdatableChanged(sender, args);
        }
        #endregion

        #region Private methods
        #endregion
    }
}
