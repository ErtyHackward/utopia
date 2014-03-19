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
    public class EnergyBarsContainer : Control
    {
        #region Private variables
        private D3DEngine _d3DEngine;

        private int _topPanelheight;

        //Child components
        private PanelControl _energiesPanel;
        private PlayerEntityManager _playerEntityManager;

        //Energy bars
        private PanelControl _healthFrame;
        private EnergyBar _healthBar;

        private PanelControl _oxygenFrame;
        private EnergyBar _oxygenBar;

        private PanelControl _staminaFrame;
        private EnergyBar _staminaBar;
        #endregion

        #region Public properties
        #endregion

        public EnergyBarsContainer(D3DEngine d3DEngine, PlayerEntityManager playerEntityManager)
        {
            _d3DEngine = d3DEngine;
            _topPanelheight = 100;
            _d3DEngine.ScreenSize_Updated += ScreenSize_Updated;
            _playerEntityManager = playerEntityManager;

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
            _healthBar.NewValue = _playerEntityManager.PlayerCharacter.Health.CurrentAsPercent;
            _staminaBar.NewValue = _playerEntityManager.PlayerCharacter.Stamina.CurrentAsPercent;
            _oxygenBar.NewValue = _playerEntityManager.PlayerCharacter.Oxygen.CurrentAsPercent;

            _healthBar.Update(timeSpend);
            _staminaBar.Update(timeSpend);
            _oxygenBar.Update(timeSpend);

            //Show / Hide Oxygen bar
            if (_playerEntityManager.PlayerCharacter.Oxygen.CurrentAsPercent == 1.0f) _oxygenFrame.IsVisible = false;
            else _oxygenFrame.IsVisible = true;
        }

        public void VTSUpdate(double interpolationHd, float interpolationLd, float elapsedTime)
        {
            _healthBar.VTSUpdate(interpolationHd, interpolationLd, elapsedTime);
            _staminaBar.VTSUpdate(interpolationHd, interpolationLd, elapsedTime);
            _oxygenBar.VTSUpdate(interpolationHd, interpolationLd, elapsedTime);
        }
        #endregion


        #region Private Methods
        private void CreateChildsComponents()
        {
            _energiesPanel = ToDispose(new PanelControl() { HidedPanel=true, Bounds = new UniRectangle(0, 0, 200, 150), Color = new ByteColor(255, 255, 255, 128) });

            _healthFrame = new PanelControl() { FrameName = "LifeEnergyBar", Bounds = new UniRectangle(5, 5, new UniScalar(1.0f, -10), 30) };
            _energiesPanel.Children.Add(_healthFrame);

            _healthBar = new EnergyBar() { FrameName = "EnergyBar", Bounds = new UniRectangle(2, 2 + 7, new UniScalar(1.0f / 4, 0.0f, -24f), new UniScalar(1.0f, -11f)), Color = new ByteColor(255, 40, 40, 255), TimeFromOldToNewInMS = 500 };
            _healthFrame.Children.Add(_healthBar);

            _staminaFrame = new PanelControl() { FrameName = "StaminaEnergyBar", Bounds = new UniRectangle(5, 40, new UniScalar(1.0f, -10f), 30) };
            _energiesPanel.Children.Add(_staminaFrame);

            _staminaBar = new EnergyBar() { FrameName = "EnergyBar", Bounds = new UniRectangle(2, 2 + 7, new UniScalar(1.0f / 2, 0.0f, -24f), new UniScalar(1.0f, -11f)), Color = new ByteColor(255, 177, 43, 255), TimeFromOldToNewInMS = 500 };
            _staminaFrame.Children.Add(_staminaBar);

            _oxygenFrame = new PanelControl() { FrameName = "AirEnergyBar", Bounds = new UniRectangle(5, 75, new UniScalar(1.0f, -10f), 30) };
            _energiesPanel.Children.Add(_oxygenFrame);

            _oxygenBar = new EnergyBar() { FrameName = "EnergyBar", Bounds = new UniRectangle(2, 2 + 7, new UniScalar(1.0f / 3, 0.0f, -24f), new UniScalar(1.0f, -11f)), Color = new ByteColor(63, 25, 255, 255), TimeFromOldToNewInMS = 500 };
            _oxygenFrame.Children.Add(_oxygenBar);

            _healthBar.Value = _playerEntityManager.PlayerCharacter.Health.CurrentAsPercent;
            _staminaBar.Value = _playerEntityManager.PlayerCharacter.Stamina.CurrentAsPercent;
            _oxygenBar.Value = _playerEntityManager.PlayerCharacter.Oxygen.CurrentAsPercent;

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
