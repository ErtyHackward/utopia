using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nuclex.UserInterface;
using Nuclex.UserInterface.Controls;
using Nuclex.UserInterface.Controls.Desktop;
using S33M3Engines;
using S33M3Engines.D3D;
using SharpDX.Direct3D11;

namespace LostIsland.Client.Components
{
    public class MainMenuComponent : GameComponent
    {
        private readonly D3DEngine _engine;
        private readonly Screen _screen;

        private ButtonControl _continueButton;
        private ButtonControl _singlePlayer;
        private ButtonControl _multiplayer;
        private ButtonControl _credits;
        private ButtonControl _exitButton;

        private Control _buttonsGroup;

        public MainMenuComponent(D3DEngine engine,  Screen screen)
        {
            if (engine == null) throw new ArgumentNullException("engine");
            if (screen == null) throw new ArgumentNullException("screen");
            _engine = engine;
            _screen = screen;

            _engine.ViewPort_Updated += UpdateLayout;
        }

        public override void Initialize()
        {
            _buttonsGroup = new Control();

            UpdateLayout(_engine.ViewPort);

            _continueButton = CreateButton("Continue", 0);
            _singlePlayer = CreateButton("Single player", 30);
            _multiplayer = CreateButton("Multiplayer", 60); 
            _credits = CreateButton("Credits", 90); 
            _exitButton = CreateButton("Exit", 120); 

            _buttonsGroup.Children.Add(_continueButton);
            _buttonsGroup.Children.Add(_singlePlayer);
            _buttonsGroup.Children.Add(_multiplayer);
            _buttonsGroup.Children.Add(_credits);
            _buttonsGroup.Children.Add(_exitButton);

            _screen.Desktop.Children.Add(_buttonsGroup);
        }

        protected override void OnEnabledChanged()
        {
            if (Enabled)
            {
                _screen.Desktop.Children.Remove(_buttonsGroup);
            }
            else
            {
                _screen.Desktop.Children.Add(_buttonsGroup);
            }


            base.OnEnabledChanged();
        }

        private ButtonControl CreateButton(string text, int position)
        {
            return new ButtonControl { Text = text, Bounds = new UniRectangle(0, position, 120, 20) };
        }

        public override void Dispose()
        {
            _engine.ViewPort_Updated -= UpdateLayout;
        }

        private void UpdateLayout(Viewport viewport)
        {
            _buttonsGroup.Bounds = new UniRectangle(_engine.ViewPort.Width - 200, _engine.ViewPort.Height - 200, 200, 200);
        }
    }
}
