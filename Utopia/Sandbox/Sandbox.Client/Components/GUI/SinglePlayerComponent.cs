using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3DXEngine.Main;
using S33M3DXEngine;
using SharpDX.Direct3D11;

namespace Sandbox.Client.Components.GUI
{
    public partial class SinglePlayerComponent : GameComponent
    {
        #region Private variables
        protected D3DEngine _engine;
        #endregion

        #region Public properties/Variables
        #endregion
        public SinglePlayerComponent(D3DEngine engine)
        {
            _engine = engine;

            InitializeComponent();

            _engine.ViewPort_Updated += UpdateLayout;
        }

        public override void BeforeDispose()
        {
            _engine.ViewPort_Updated -= UpdateLayout;
        }

        #region Public methods
        #endregion

        #region Private methods
        #endregion
    }
}
