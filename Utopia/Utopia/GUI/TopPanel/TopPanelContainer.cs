using S33M3CoreComponents.GUI.Nuclex;
using S33M3CoreComponents.GUI.Nuclex.Controls;
using S33M3CoreComponents.GUI.Nuclex.Controls.Arcade;
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

        //Child components
        private PanelControl _compassPanel;
        private PanelControl _mainPanel;

        private EnergyBar _life;
        #endregion

        #region Public properties
        #endregion

        public TopPanelContainer(D3DEngine d3DEngine)
        {
            _d3DEngine = d3DEngine;
            _topPanelheight = 100;
            _d3DEngine.ScreenSize_Updated += ScreenSize_Updated;

            RefreshSize(_d3DEngine.ViewPort);
            CreateChildsComponents();
        }

        public override void BeforeDispose()
        {
            _life.Dispose();
            _d3DEngine.ScreenSize_Updated -= ScreenSize_Updated;
            base.BeforeDispose();
        }

        #region Public Methods
        #endregion

        #region Private Methods
        private void CreateChildsComponents()
        {
            _compassPanel = ToDispose(new PanelControl() { Bounds = new UniRectangle(new UniScalar(1.0f, -150), 0, 150, 150), Color = new ByteColor(255,255,255,128) });
            _mainPanel = ToDispose(new PanelControl() { Bounds = new UniRectangle(0, 0, new UniScalar(1.0f, -150), 75), Color = new ByteColor(255, 255, 255, 128) });

            _life = new EnergyBar() { Bounds = new UniRectangle(5, 5, 500, 30) };
            _mainPanel.Children.Add(_life);

            this.Children.Add(_compassPanel);
            this.Children.Add(_mainPanel);
        }

        private void ScreenSize_Updated(ViewportF viewport, SharpDX.Direct3D11.Texture2DDescription newBackBuffer)
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
