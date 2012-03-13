using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3_CoreComponents.GUI.Nuclex.Controls.Desktop;
using S33M3_DXEngine.Main;
using S33M3_CoreComponents.Inputs.Actions;
using S33M3_CoreComponents.Inputs;
using S33M3_CoreComponents.Inputs.KeyboardHandler;
using S33M3_CoreComponents.GUI.Nuclex;
using S33M3_CoreComponents.Debug.GUI.Controls;
using S33M3_CoreComponents.Components.Debug;
using S33M3_DXEngine;
using S33M3_CoreComponents.Debug.Components;
using S33M3_CoreComponents.GUI;
using SharpDX.Direct3D11;

namespace S33M3_CoreComponents.Debug
{
    /// <summary>
    /// Component that will be used to display "Live debug informations"
    /// </summary>
    public class DebugComponent : DrawableGameComponent
    {
        #region Private variables
        private DebugWindowControl _mainControl;
        private readonly InputsManager _inputManager;
        private readonly GuiManager _guiManager;
        private DisplayInfo _displayInfo;
        private FPSComponent _fps;
        private ColumnChart _chart;
        private D3DEngine _engine;

        private Game _game;
        #endregion

        #region Public variables
        #endregion

        public DebugComponent(InputsManager inputManager, GuiManager guiManager, Game game, D3DEngine engine)
        {
            this.IsSystemComponent = true;
            _inputManager = inputManager;
            _engine = engine;
            _guiManager = guiManager;
            _game = game;
            _displayInfo = ToDispose(new DisplayInfo(_engine, game));

            _fps = ToDispose(new FPSComponent());
            _fps.ShowDebugInfo = true;

            _displayInfo.AddComponants(_fps);

            this.DrawOrders.UpdateIndex(0, 10000);
            this.DrawOrders.AddIndex(_guiManager.DrawOrders.DrawOrdersCollection[0].Order + 1, "Chart");
        }

        #region Private Methods
        private void HideControl(object sender, EventArgs e)
        {
            _mainControl.ControlClosed -= HideControl;
            _guiManager.Screen.Desktop.Children.Remove(_mainControl.DebugWindow);
        }

        private void ShowControl(object sender, EventArgs e)
        {
            _mainControl.ControlClosed += HideControl;
            _guiManager.Screen.Desktop.Children.Add(_mainControl.DebugWindow);
        }
        #endregion

        #region Public methods
        public override void Initialize()
        {
            //Register key binding for showing the Debug Window
            _inputManager.ActionsManager.AddActions(new KeyboardTriggeredAction()
            {
                ActionId = Actions.EngineShowDebugUI,
                TriggerType = KeyboardTriggerMode.KeyPressed,
                Binding = new KeyWithModifier() { MainKey = System.Windows.Forms.Keys.F4 }
            });

            _mainControl = ToDispose(new DebugWindowControl(_game, _displayInfo, _fps));
            //_mainControl.ControlClosed += HideControl;
            //_guiManager.Screen.Desktop.Children.Add(_mainControl.DebugWindow);

            _chart = ToDispose(new ColumnChart(_engine, new SharpDX.Rectangle((int)_mainControl.DebugWindow.Bounds.Left.Offset,
                                                                    (int)_engine.ViewPort.Height - (int)_mainControl.DebugWindow.Bounds.Bottom.Offset,
                                                                    (int)_mainControl.DebugWindow.Bounds.Left.Offset + 200,
                                                                    (int)_engine.ViewPort.Height - (int)_mainControl.DebugWindow.Bounds.Bottom.Offset + 100)));

            _displayInfo.Initialize();
            _fps.Initialize();
            _chart.Initialize();

            this.EnableComponent();
        }

        public override void LoadContent(SharpDX.Direct3D11.DeviceContext Context)
        {
            _displayInfo.LoadContent(Context);
            _fps.LoadContent(Context);
            _chart.LoadContent(Context);
        }

        public override void Update(GameTime timeSpent)
        {
            //Show GUI debug interface
            if (_inputManager.ActionsManager.isTriggered(Actions.EngineShowDebugUI))
            {
                if (_guiManager.Screen.Desktop.Children.Contains(_mainControl.DebugWindow))
                    HideControl(this, null);
                else
                    ShowControl(this, null);
            }

            //Relay Debug Component Update
            if (_displayInfo.Updatable) _displayInfo.Update(timeSpent);
            if (_fps.Updatable) _fps.Update(timeSpent);
            if (_game.ComponentsPerfMonitor.Updatable && _mainControl.DebugWindow.Children[0] == _mainControl.Dpc)
            {
                _chart.AddValue((float)_mainControl.Dpc.UpdateData());
                _chart.Update(timeSpent);
            }
        }

        public override void Draw(DeviceContext context, int index)
        {
            switch (index)
            {
                case 0:
                    //Relay Debug Component Update
                    if (_displayInfo.Visible) _displayInfo.Draw(context, 0);
                    if (_fps.Visible) _fps.Draw(context, 0);
                    break;
                case 1: // Draw After The Gui
                    //Draw only if the component is visible and has the focus
                    if (_game.ComponentsPerfMonitor.Updatable && _mainControl.Dpc != null && _mainControl.Dpc.Parent != null && _mainControl.Dpc.Parent.Parent != null)
                    {
                        if (_mainControl.DebugWindow.Children[0] == _mainControl.Dpc)
                        {
                            _chart.ScreenPosition = new SharpDX.Rectangle((int)_mainControl.DebugWindow.Bounds.Left.Offset + (int)_mainControl.Dpc.Bounds.Left.Offset + 10,
                                                                    (int)_engine.ViewPort.Height - (int)_mainControl.DebugWindow.Bounds.Bottom.Offset + 10,
                                                                    (int)_mainControl.DebugWindow.Bounds.Left.Offset + (int)_mainControl.Dpc.Bounds.Left.Offset + (int)_mainControl.Dpc.Bounds.Size.X.Offset - 10,
                                                                    (int)_engine.ViewPort.Height - (int)_mainControl.DebugWindow.Bounds.Bottom.Offset + 150 );

                            _chart.Draw(context, 0);
                        }
                    }

                    break;
            }
        }

        #endregion
    }
}
