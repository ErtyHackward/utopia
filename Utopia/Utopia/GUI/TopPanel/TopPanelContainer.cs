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
        private PanelControl _energiesPanel;
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
            _d3DEngine.ScreenSize_Updated -= ScreenSize_Updated;
            base.BeforeDispose();
        }

        #region Public Methods
        #endregion

        #region Private Methods
        private void CreateChildsComponents()
        {
            _compassPanel = ToDispose(new PanelControl() { Bounds = new UniRectangle(new UniScalar(1.0f, -150), 0, 150, 150), Color = new ByteColor(255,255,255,128) });
            _energiesPanel = ToDispose(new PanelControl() { HidedPanel=true, Bounds = new UniRectangle(0, 0, 200, 150), Color = new ByteColor(255, 255, 255, 128) });

            var life = new EnergyBar() { FrameName = "LifeEnergyBar", Bounds = new UniRectangle(5, 5, new UniScalar(1.0f, -10), 30) };
            _energiesPanel.Children.Add(life);

            var lifeBar = new EnergyBar() { FrameName = "EnergyBar", Bounds = new UniRectangle(2, 2 + 7, new UniScalar(1.0f / 4, 0.0f, -24f), new UniScalar(1.0f, -11f)), Color = new ByteColor(255, 40, 40, 255) };
            life.Children.Add(lifeBar);

            var air = new EnergyBar() { FrameName = "AirEnergyBar", Bounds = new UniRectangle(5, 40, new UniScalar(1.0f, -10f), 30) };
            _energiesPanel.Children.Add(air);

            var airBar = new EnergyBar() { FrameName = "EnergyBar", Bounds = new UniRectangle(2, 2 + 7, new UniScalar(1.0f / 3, 0.0f, -24f), new UniScalar(1.0f, -11f)), Color = new ByteColor(63, 25, 255, 255) };
            air.Children.Add(airBar);

            var stamina = new EnergyBar() { FrameName = "StaminaEnergyBar", Bounds = new UniRectangle(5, 75, new UniScalar(1.0f, -10f), 30) };
            _energiesPanel.Children.Add(stamina);

            var staminaBar = new EnergyBar() { FrameName = "EnergyBar", Bounds = new UniRectangle(2, 2 + 7, new UniScalar(1.0f / 2, 0.0f, -24f), new UniScalar(1.0f, -11f)), Color = new ByteColor(255, 177, 43, 255) };
            stamina.Children.Add(staminaBar);

            this.Children.Add(_compassPanel);
            this.Children.Add(_energiesPanel);
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
