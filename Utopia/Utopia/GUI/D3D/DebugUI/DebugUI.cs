using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nuclex.UserInterface.Controls.Desktop;
using S33M3Engines.D3D;
using Nuclex.UserInterface;
using Nuclex.UserInterface.Controls;

namespace Utopia.GUI.D3D.DebugUI
{
     public class DebugUi : WindowControl
    {
        private readonly List<IGameComponent> _gameComponentCollection;
        public DebugUi(List<IGameComponent> gameComponentCollection)
            : base()
        {
            _gameComponentCollection = gameComponentCollection;

            this.Bounds = new UniRectangle(90.0f, 70.0f, 350.0f, 250.0f);
            this.Title = "Debug options";

            InitGameComponents();


            ButtonControl closeButton = new ButtonControl();
            closeButton.Bounds = new UniRectangle(
                new UniScalar(1.0f, -90.0f), new UniScalar(1.0f, -40.0f), 80, 24
            );
            closeButton.Text = "Close";
            closeButton.Pressed += delegate(object sender, EventArgs e)
            {
                Close();
            };

            Children.Add(closeButton);

        }



        private void InitGameComponents()
        {
            float y = 40f;
            float step = 20f;
            foreach (IGameComponent component in _gameComponentCollection)
            {

                LabelControl nameLbl = new LabelControl();
                nameLbl.Bounds = new UniRectangle(10.0f, y, 200.0f, 16.0f);
                nameLbl.Text = component.GetType().Name;
                Children.Add(nameLbl);

                OptionControl enable = new OptionControl();
                enable.Bounds = new UniRectangle(220.0f, y, 40.0f, 16.0f);
                enable.Text = "E";
                IGameComponent component1 = component;
                enable.Changed += delegate(object sender, EventArgs e)
               {
                   component1.CallUpdate = !component1.CallUpdate;
               };
                enable.Selected = component.CallUpdate;

                Children.Add(enable);

                OptionControl view = new OptionControl();
                view.Bounds = new UniRectangle(260.0f, y, 40.0f, 16.0f);
                view.Text = "V";
                view.Selected = component.CallDraw;
                IGameComponent component2 = component;
                view.Changed += delegate(object sender, EventArgs e)
                {
                    component2.CallDraw = !component2.CallDraw;
                };
                Children.Add(view);

                y = y + step;
            }
        }
    }
}

