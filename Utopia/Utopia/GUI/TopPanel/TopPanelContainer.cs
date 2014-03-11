using S33M3CoreComponents.GUI.Nuclex;
using S33M3CoreComponents.GUI.Nuclex.Controls;
using S33M3DXEngine;
using S33M3Resources.Structs;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.GUI.Inventory;

namespace Utopia.GUI.TopPanel
{
    public class TopPanelContainer : Control
    {
        #region Private variables
        private D3DEngine _d3DEngine;

        private int _topPanelheight;
        #endregion

        #region Public properties
        #endregion

        public TopPanelContainer(D3DEngine d3DEngine)
        {
            _d3DEngine = d3DEngine;
            _topPanelheight = 100;
            _d3DEngine.ViewPort_Updated += viewPort_Updated;

            RefreshSize(_d3DEngine.ViewPort);
        }

        public override void BeforeDispose()
        {
            _d3DEngine.ViewPort_Updated -= viewPort_Updated;
            base.BeforeDispose();
        }

        #region Public Methods
        #endregion

        #region Private Methods
        private void viewPort_Updated(ViewportF viewport, SharpDX.Direct3D11.Texture2DDescription newBackBuffer)
        {
            RefreshSize(viewport);
        }

        private void RefreshSize(ViewportF viewport)
        {
            var screenSize = new Vector2I((int)viewport.Width, (int)viewport.Height);
            this.Bounds.Size = new UniVector(screenSize.X, _topPanelheight);
        }
        #endregion



    }
}
