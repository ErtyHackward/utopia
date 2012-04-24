using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3CoreComponents.GUI.Nuclex.Controls.Arcade;
using S33M3DXEngine;
using S33M3CoreComponents.GUI.Nuclex.Controls;
using S33M3CoreComponents.GUI.Nuclex;
using S33M3CoreComponents.GUI.Nuclex.Controls.Desktop;
using SharpDX;
using S33M3DXEngine.Main;
using S33M3CoreComponents.Components.Debug;
using S33M3CoreComponents.Debug.Components;
using S33M3DXEngine.Debug.Interfaces;

namespace S33M3CoreComponents.Debug.GUI.Controls
{
    public class DebugOptionsControl : PanelControl
    {
        #region Private variables
        private Game _game;
        private DisplayInfo _displayinfo;
        private GeneralDebugComponent _fps;

        private const float Step = 20f;
        #endregion

        #region Public variables
        #endregion

        public DebugOptionsControl(Control parent, 
                                   UniRectangle bounds, 
                                   Game game, 
                                   DisplayInfo displayinfo, 
                                   GeneralDebugComponent fps)
        {
            this.Bounds = bounds;
            parent.Children.Add(this);

            _fps = fps;
            _game = game;
            _displayinfo = displayinfo;

            BuildWindow();
        }

        #region Private methods
        private void BuildWindow()
        {
            this.Color = Colors.Wheat;

            CloseWindowButtonControl closeBt = ToDispose(new CloseWindowButtonControl() { Bounds = new UniRectangle(this.Bounds.Size.X - 20, 5, 15, 15) });
            closeBt.Pressed += (sender, e) => { this.RemoveFromParent(); };
            this.Children.Add(closeBt);

            InitGameComponents();
        }

        private void InitGameComponents()
        {
            OptionControl oc;
            float y = 20f;

            LabelControl Titles;
            Titles = ToDispose(new LabelControl());
            Titles.FontStyle = System.Drawing.FontStyle.Bold;
            Titles.Bounds = new UniRectangle(10.0f, 5.0f, 110.0f, 18.0f);
            Titles.Text = "Debugs Options";
            Children.Add(Titles);

            oc = ToDispose(new OptionControl());
            oc.Bounds = new UniRectangle(10f, y, 40.0f, 16.0f);
            oc.Text = "VSync Mode";
            oc.Changed += (sender, e) => _game.VSync = !_game.VSync;
            oc.Selected = _game.VSync;
            Children.Add(oc);
            y = y + Step;

            oc = ToDispose(new OptionControl());
            oc.Bounds = new UniRectangle(10f, y, 40.0f, 16.0f);
            oc.Text = "FPS computation";
            oc.Changed += (sender, e) =>
            {
                _fps.Updatable = !_fps.Updatable;
                _fps.Visible = !_fps.Visible;
            };
            oc.Selected = _fps.Updatable;
            Children.Add(oc);
            y = y + Step;

            oc = ToDispose(new OptionControl());
            oc.Bounds = new UniRectangle(10f, y, 40.0f, 16.0f);
            oc.Text = "Show Debug Information";
            oc.Changed += (sender, e) =>
            {
                _displayinfo.Updatable = !_displayinfo.Updatable;
                _displayinfo.Visible = !_displayinfo.Visible;
            };
             oc.Selected = _displayinfo.Updatable;
            Children.Add(oc);
            y = y + Step;

            y = 20;
            Titles = ToDispose(new LabelControl());
            Titles.FontStyle = System.Drawing.FontStyle.Bold;
            Titles.Bounds = new UniRectangle(180.0f, 5.0f, 110.0f, 18.0f);
            Titles.Text = "Show components Debug info";
            Children.Add(Titles);

            //For each IDebugInfo Components registered
            foreach (var comp in _displayinfo.Components)
            {
                GameComponent gc = comp as GameComponent;
                if (gc != null)
                {
                    oc = ToDispose(new OptionControl());
                    oc.Bounds = new UniRectangle(180.0f, y, 40.0f, 16.0f);
                    oc.Text = gc.GetType().Name;
                    oc.Tag = comp;
                    oc.Changed += (sender, e) =>
                    {
                        IDebugInfo gameComp = ((OptionControl)sender).Tag as IDebugInfo;
                        gameComp.ShowDebugInfo = !gameComp.ShowDebugInfo;
                    };
                    oc.Selected = comp.ShowDebugInfo;
                    Children.Add(oc);
                    y = y + Step;
                }
            }

        }

        //Form events
        private void _closeBt_Pressed(object sender, EventArgs e)
        {
            this.RemoveFromParent();
        }
        #endregion

        #region Public methods
        #endregion
    }
}
