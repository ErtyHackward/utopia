﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3CoreComponents.GUI.Nuclex.Controls.Desktop;
using S33M3DXEngine.Main;
using S33M3CoreComponents.Inputs.Actions;
using S33M3CoreComponents.Inputs;
using S33M3CoreComponents.Inputs.KeyboardHandler;
using S33M3CoreComponents.GUI.Nuclex;
using S33M3CoreComponents.Debug.GUI.Controls;
using S33M3CoreComponents.Components.Debug;
using S33M3DXEngine;
using S33M3CoreComponents.Debug.Components;
using S33M3CoreComponents.GUI;
using SharpDX.Direct3D11;
using S33M3CoreComponents.Cameras.Interfaces;
using S33M3Resources.Structs;

namespace S33M3CoreComponents.Debug
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
        private GeneralDebugComponent _fps;
        private ColumnChart _chart;
        private D3DEngine _engine;

        ByteColor _leftPanelColor;

        private bool _wasMouseCaptured;
        private Game _game;
        #endregion

        #region Public variables
        #endregion

        public DebugComponent(InputsManager inputManager, GuiManager guiManager, Game game, D3DEngine engine, ByteColor LeftPanelColor, bool withDisplayInfoActivated = false)
        {
            this.IsSystemComponent = true;
            _inputManager = inputManager;
            _engine = engine;
            _leftPanelColor = LeftPanelColor;
            _guiManager = guiManager;
            _game = game;
            _displayInfo = ToDispose(new DisplayInfo(_engine, game));
            _displayInfo.EnableComponent();

            _fps = ToDispose(new GeneralDebugComponent(inputManager));
            if (withDisplayInfoActivated) _fps.EnableComponent();
            _fps.ShowDebugInfo = true;

            _displayInfo.AddComponants(_fps);

            this.DrawOrders.UpdateIndex(0, int.MaxValue);
            this.DrawOrders.AddIndex(_guiManager.DrawOrders.DrawOrdersCollection[0].Order + 2, "Chart");
        }

        #region Private Methods
        private void HideControl(object sender, EventArgs e)
        {
            _mainControl.ControlClosed -= HideControl;
            _guiManager.Screen.Desktop.Children.Remove(_mainControl.DebugWindow);
            ReleaseExclusiveMode();
        }

        private void ShowControl(object sender, EventArgs e)
        {
            _mainControl.ControlClosed += HideControl;
            _guiManager.Screen.Desktop.Children.Add(_mainControl.DebugWindow);
        }

        private void ReleaseExclusiveMode()
        {
            this.CatchExclusiveActions = false;
            _inputManager.MouseManager.MouseCapture = _wasMouseCaptured;
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

            _mainControl = ToDispose(new DebugWindowControl(_game, _displayInfo, _fps, _leftPanelColor));

            _chart = ToDispose(new ColumnChart(_engine, new SharpDX.Rectangle((int)_mainControl.DebugWindow.Bounds.Left.Offset,
                                                                    (int)_engine.ViewPort.Height - (int)_mainControl.DebugWindow.Bounds.Bottom.Offset,
                                                                    (int)_mainControl.DebugWindow.Bounds.Left.Offset + 200,
                                                                    (int)_engine.ViewPort.Height - (int)_mainControl.DebugWindow.Bounds.Bottom.Offset + 100)));

            _displayInfo.Initialize();
            _fps.Initialize();
            _chart.Initialize();

            this.EnableComponent();
        }

        public override void LoadContent(SharpDX.Direct3D11.DeviceContext context)
        {
            _displayInfo.LoadContent(context);
            _fps.LoadContent(context);
            _chart.LoadContent(context);
        }

        public override void FTSUpdate(GameTime timeSpent)
        {
            //Show GUI debug interface
            if (_inputManager.ActionsManager.isTriggered(Actions.EngineShowDebugUI, CatchExclusiveActions))
            {
                if (_guiManager.Screen.Desktop.Children.Contains(_mainControl.DebugWindow))
                {
                    HideControl(this, null);
                }
                else
                {
                    //Show the GUI, and start the action exclusive mode
                    ShowControl(this, null);
                    this.CatchExclusiveActions = true; //The gui will automaticaly set the actionmanager in Exclusive mode
                    _wasMouseCaptured = _inputManager.MouseManager.MouseCapture;
                    _inputManager.MouseManager.MouseCapture = false;
                }
            }

            //Relay Debug Component Update
            if (_displayInfo.Updatable) _displayInfo.FTSUpdate(timeSpent);
            if (_fps.Updatable) _fps.FTSUpdate(timeSpent);
            if (_game.ComponentsPerfMonitor.Updatable && _mainControl.DebugWindow.Children[0] == _mainControl.Dpc)
            {
                _chart.AddValue((float)_mainControl.Dpc.UpdateData());
                _chart.FTSUpdate(timeSpent);
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
                                                                    (int)_engine.ViewPort.Height - (int)_mainControl.DebugWindow.Bounds.Bottom.Offset + 150);

                            _chart.Draw(context, 0);
                        }
                    }

                    break;
            }
        }

        #endregion
    }
}
