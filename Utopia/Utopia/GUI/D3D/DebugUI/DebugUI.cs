using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nuclex.UserInterface.Controls.Desktop;
using S33M3Engines;
using S33M3Engines.D3D;
using Nuclex.UserInterface;
using Nuclex.UserInterface.Controls;
using S33M3Engines.GameStates;

namespace Utopia.GUI.D3D.DebugUI
{
    public class DebugUi : WindowControl
    {
        private readonly GameComponentCollection _gameComponentCollection;

        private readonly UtopiaRender _game;
        private readonly D3DEngine _d3DEngine;
        private readonly GameStatesManager _gameStateManagers;

        private const float Step = 20f;

        public DebugUi(UtopiaRender game, GameComponentCollection gameComponentCollection, D3DEngine d3DEngine,
                       GameStatesManager gameStateManagers)
            : base()
        {
            _gameComponentCollection = gameComponentCollection;
            _gameStateManagers = gameStateManagers;
            _d3DEngine = d3DEngine;
            _game = game;

            this.Bounds = new UniRectangle(90.0f, 70.0f, 500.0f, 250.0f);
            this.Title = "Debug options";

            InitGameComponents();

            InitGameFlags();

            ButtonControl closeButton = new ButtonControl();
            closeButton.Bounds = new UniRectangle(
                new UniScalar(1.0f, -90.0f), new UniScalar(1.0f, -40.0f), 80, 24
                );
            closeButton.Text = "Close";
            closeButton.Pressed += (sender, e) => Close();

            Children.Add(closeButton);
        }

        private void InitGameFlags()
        {
            float y = 40f;

            OptionControl fixedTimeStep = new OptionControl();
            fixedTimeStep.Bounds = new UniRectangle(360f, y, 40.0f, 16.0f);
            fixedTimeStep.Text = "FixeTimeStep Mode";
            fixedTimeStep.Changed += (sender, e) => _game.FixedTimeSteps = !_game.FixedTimeSteps;
            fixedTimeStep.Selected = _game.FixedTimeSteps;
            Children.Add(fixedTimeStep);
            y = y + Step;

            OptionControl vSync = new OptionControl();
            vSync.Bounds = new UniRectangle(360f, y, 40.0f, 16.0f);
            vSync.Text = "VSync Mode";
            vSync.Changed += (sender, e) => _game.VSync = !_game.VSync;
            vSync.Selected = _game.VSync;
            Children.Add(vSync);
            y = y + Step;

            OptionControl fullScreen = new OptionControl();
            fullScreen.Bounds = new UniRectangle(360f, y, 40.0f, 16.0f);
            fullScreen.Text = "FullScreen Mode";
            fullScreen.Changed += (sender, e) => _d3DEngine.isFullScreen = !_d3DEngine.isFullScreen;
            fullScreen.Selected = _d3DEngine.isFullScreen;
            Children.Add(fullScreen);
            y = y + Step;

            OptionControl debugActif = new OptionControl();
            debugActif.Bounds = new UniRectangle(360f, y, 40.0f, 16.0f);
            debugActif.Text = "DebugActif Mode";
            debugActif.Changed += (sender, e) => _gameStateManagers.DebugActif = !_gameStateManagers.DebugActif;
            debugActif.Selected = _gameStateManagers.DebugActif;
            Children.Add(debugActif);
        }


        private void InitGameComponents()
        {
            float y = 40f;

            foreach (IGameComponent component in _gameComponentCollection)
            {
                LabelControl nameLbl = new LabelControl();
                nameLbl.Bounds = new UniRectangle(10.0f, y, 110.0f, 16.0f);
                nameLbl.Text = component.GetType().Name;
                Children.Add(nameLbl);


                IUpdateableComponent updateableComponent = component as IUpdateableComponent;
                if (updateableComponent != null)
                {
                    OptionControl enable = new OptionControl();
                    enable.Bounds = new UniRectangle(120.0f, y, 20.0f, 16.0f);
                    enable.Text = "E";

                    enable.Changed +=
                        (sender, e) => updateableComponent.Enabled = !updateableComponent.Enabled;

                    enable.Selected = updateableComponent.Enabled;
                    Children.Add(enable);

                    InputControl updateOrder = new InputControl();
                    updateOrder.Bounds = new UniRectangle(150.0f, y, 40.0f, 16.0f);
                    updateOrder.Text = updateableComponent.UpdateOrder.ToString();
                    Children.Add(updateOrder);
                }

                IDrawableComponent drawableComponent = component as IDrawableComponent;
                if (drawableComponent != null)
                {
                    OptionControl view = new OptionControl();
                    view.Bounds = new UniRectangle(200.0f, y, 20.0f, 16.0f);
                    view.Text = "V";
                    view.Selected = drawableComponent.Visible;

                    view.Changed +=
                        (sender, e) => drawableComponent.Visible = !drawableComponent.Visible;

                    Children.Add(view);

                    InputControl draworder = new InputControl();
                    draworder.Bounds = new UniRectangle(230.0f, y, 40.0f, 16.0f);
                    draworder.Text = drawableComponent.DrawOrder.ToString();
                    Children.Add(draworder);
                }
                y = y + Step;
            }
        }
    }
}