using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3CoreComponents.GUI.Nuclex.Controls.Desktop;
using S33M3CoreComponents.GUI.Nuclex;
using SharpDX;
using S33M3CoreComponents.GUI.Nuclex.Controls.Arcade;
using S33M3CoreComponents.GUI.Nuclex.Controls;
using S33M3DXEngine.Main;
using S33M3CoreComponents.Components.Debug;
using S33M3CoreComponents.Debug.Components;

namespace S33M3CoreComponents.Debug.GUI.Controls
{
    public class DebugWindowControl : Component
    {
        #region Private variables
        private WindowControl _debugWindow;
        private CloseWindowButtonControl _closeBt;
        private DisplayInfo _displayInfo;

        private GameComponentControl _gcc;
        private DebugOptionsControl _doc;
        private DebugPerfControl _dpc;



        private FPSComponent _fps;

        private Game _game;
        #endregion

        #region Public variables
        public event EventHandler ControlClosed;

        public DebugPerfControl Dpc
        {
            get { return _dpc; }
        }

        public WindowControl DebugWindow
        {
            get { return _debugWindow; }
            set { _debugWindow = value; }
        }
        #endregion

        public DebugWindowControl(Game game, DisplayInfo displayInfo, FPSComponent fps)
        {
            _fps = fps;
            _displayInfo = displayInfo;
            _game = game;
            BuildWindow();
        }

        #region Private methods
        private void BuildWindow()
        {
            //Create the Close Button
            _debugWindow = new WindowControl();
            _debugWindow.Name = "DebugComponent";
            _debugWindow.Title = "Debug Component";
            _debugWindow.Bounds = new UniRectangle(0, 0, 600, 400);
            _closeBt = ToDispose(new CloseWindowButtonControl() { Bounds = new UniRectangle(_debugWindow.Bounds.Right - 20, 5, 15, 15) });
            _closeBt.Pressed += closeBt_Pressed;
            _debugWindow.Children.Add(_closeBt);

            //Create the Left Menu panel
            PanelControl menu = ToDispose(new PanelControl() { Bounds = new UniRectangle(4, 24, 100, _debugWindow.Bounds.Size.Y - 28), Color = new Color4(0.16f, 0.44f, 0.64f, 1.0f) });
                LabelControl menu_title = ToDispose(new LabelControl() { Bounds = new UniRectangle(5, 0, menu.Bounds.Size.X, 30), Color = Colors.Yellow, Text = "Visualisation" });
                menu.Children.Add(menu_title);

                ButtonControl btOptions = ToDispose(new ButtonControl() { Tag = "btOptions", Bounds = new UniRectangle(4, menu_title.Bounds.Bottom - 6, 92, 40), Text = "Debug Options" });
                btOptions.Pressed += new EventHandler(btMenu_Pressed);
                menu.Children.Add(btOptions);

                ButtonControl btComponent = ToDispose(new ButtonControl() { Tag = "btComponent", Bounds = new UniRectangle(4, menu_title.Bounds.Bottom - 6 + 40, 92, 40), Text = "Game Comp." });
                btComponent.Pressed += new EventHandler(btMenu_Pressed);
                menu.Children.Add(btComponent);

                ButtonControl btPerf = ToDispose(new ButtonControl() { Tag = "btPerf", Bounds = new UniRectangle(4, menu_title.Bounds.Bottom - 6 + 80, 92, 40), Text = "Perf. Comp." });
                btPerf.Pressed += new EventHandler(btMenu_Pressed);
                menu.Children.Add(btPerf);    

            _debugWindow.Children.Add(menu);

        }

        //Form Events handling
        void btMenu_Pressed(object sender, EventArgs e)
        {
            switch (((Control)sender).Tag.ToString())
            {
                case "btComponent":
                    if (_gcc == null || !_debugWindow.Children.Contains(_gcc))
                    {
                        _gcc = ToDispose(new GameComponentControl(_debugWindow, new UniRectangle(104, 24, _debugWindow.Bounds.Size.X - 108, _debugWindow.Bounds.Size.Y - 28), _game));
                        _gcc.BringToFront();
                    }
                    else
                    {
                        if (_gcc != null) _gcc.BringToFront();
                    }
                    break;
                case "btOptions":
                    if (_doc == null || !_debugWindow.Children.Contains(_doc))
                    {
                        _doc = ToDispose(new DebugOptionsControl(_debugWindow, new UniRectangle(104, 24, _debugWindow.Bounds.Size.X - 108, _debugWindow.Bounds.Size.Y - 28), _game, _displayInfo, _fps));
                        _doc.BringToFront();
                    }
                    else
                    {
                        if (_doc != null) _doc.BringToFront();
                    }
                    break;
                case "btPerf":
                    if (_dpc == null || !_debugWindow.Children.Contains(_dpc))
                    {
                        _dpc = ToDispose(new DebugPerfControl(_debugWindow, new UniRectangle(104, 24, _debugWindow.Bounds.Size.X - 108, _debugWindow.Bounds.Size.Y - 28), _game));
                        _dpc.BringToFront();
                    }
                    else
                    {
                        if (_dpc != null) _dpc.BringToFront();
                    }
                    break;
            }
        }

        #endregion

        #region Public methods
        private void closeBt_Pressed(object sender, EventArgs e)
        {
            if (ControlClosed != null) ControlClosed(this, null);
        }
        #endregion
    }
}
