using System;
using Nuclex.UserInterface;
using Nuclex.UserInterface.Controls;
using Nuclex.UserInterface.Controls.Desktop;
using S33M3Engines;
using S33M3Engines.D3D;
using SharpDX.Direct3D11;

namespace LostIsland.Client.Components
{
    public class CreditsComponent : GameComponent
    {
        private readonly D3DEngine _engine;
        private readonly Screen _screen;
        private LabelControl _creditsLabel;
        private ButtonControl _backButton;

        public event EventHandler BackPressed;

        private void OnBackPressed()
        {
            var handler = BackPressed;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public CreditsComponent(D3DEngine engine, Screen screen)
        {
            if (engine == null) throw new ArgumentNullException("engine");
            if (screen == null) throw new ArgumentNullException("screen");
            _engine = engine;
            _screen = screen;

            _engine.ViewPort_Updated += UpdateLayout;
        }

        public override void Initialize()
        {
            _creditsLabel = new LabelControl { Text = "Credits: \nFabian Ceressia\nSimon Lebettre\nVladislav Pozdnyakov" };
            _backButton = new ButtonControl { Text = "Back" };
            _backButton.Pressed += delegate { OnBackPressed(); };
            UpdateLayout(_engine.ViewPort);

            if (Enabled)
            {
                _screen.Desktop.Children.Add(_creditsLabel);
                _screen.Desktop.Children.Add(_backButton);
            }
        }

        protected override void OnEnabledChanged()
        {
            if (!IsInitialized) return;

            if (Enabled)
            {
                _screen.Desktop.Children.Add(_creditsLabel);
                _screen.Desktop.Children.Add(_backButton);
                UpdateLayout(_engine.ViewPort);
            }
            else
            {
                _screen.Desktop.Children.Remove(_creditsLabel);
                _screen.Desktop.Children.Remove(_backButton);
            }
            
            base.OnEnabledChanged();
        }

        private void UpdateLayout(Viewport viewport)
        {
            if (Enabled)
            {
                _creditsLabel.Bounds = new UniRectangle((_engine.ViewPort.Width - 200)/2,
                                                        (_engine.ViewPort.Height - 200)/2, 600, 200);
                _backButton.Bounds = new UniRectangle(_engine.ViewPort.Width - 200, _engine.ViewPort.Height - 60, 120,
                                                      24);
            }
            //_buttonsGroup.Bounds = new UniRectangle(_engine.ViewPort.Width - 200, _engine.ViewPort.Height - 200, 200, 200);
        }
    }
}
