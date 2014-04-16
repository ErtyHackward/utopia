using S33M3CoreComponents.GUI.Nuclex;
using S33M3CoreComponents.GUI.Nuclex.Controls;
using S33M3CoreComponents.GUI.Nuclex.Controls.Arcade;
using S33M3DXEngine;
using S33M3DXEngine.Main;
using S33M3Resources.Structs;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Entities.Managers;
using Utopia.GUI.Inventory;

namespace Utopia.GUI.TopPanel
{
    public class WeatherContainer : Control
    {
        #region Private variables
        private D3DEngine _d3DEngine;

        private int _topPanelheight;

        //Child components
        private PanelControl _weatherPanel;

        //Energy bars
        private PanelControl _tempFrame;
        private WeatherCursor _tempCursor;

        private PanelControl _moistureFrame;
        private WeatherCursor _moistureCursor;
        #endregion

        #region Public properties
        #endregion

        public WeatherContainer(D3DEngine d3DEngine)
        {
            _d3DEngine = d3DEngine;
            _topPanelheight = 100;
            _d3DEngine.ScreenSize_Updated += ScreenSize_Updated;

            RefreshSize(_d3DEngine.ViewPort);
            CreateChildsComponents();
        }

        public override void BeforeDispose()
        {
            _d3DEngine.ScreenSize_Updated -= ScreenSize_Updated;
            base.BeforeDispose();
        }

        #region Public Methods
        public void Update(GameTime timeSpend)
        {

        }

        public void VTSUpdate(double interpolationHd, float interpolationLd, float elapsedTime)
        {

        }
        #endregion


        #region Private Methods
        private void CreateChildsComponents()
        {
            _weatherPanel = ToDispose(new PanelControl() { FrameName = "panel", Bounds = new UniRectangle(new UniScalar(1.0f, -260.0f), 20, 100, 150) });

            _tempFrame = new PanelControl() { FrameName = "TemperatureFrame", Bounds = new UniRectangle(5, 5, new UniScalar(1.0f, -10), 16) };
            _weatherPanel.Children.Add(_tempFrame);

            _moistureFrame = new PanelControl() { FrameName = "MoistureFrame", Bounds = new UniRectangle(5, 26, new UniScalar(1.0f, -10), 16) };
            _weatherPanel.Children.Add(_moistureFrame);

            this.Children.Add(_weatherPanel);
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
