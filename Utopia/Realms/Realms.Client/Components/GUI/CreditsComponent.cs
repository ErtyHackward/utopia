using System;
using SharpDX.Direct3D11;
using S33M3DXEngine.Main;
using S33M3DXEngine;
using S33M3CoreComponents.GUI.Nuclex;
using S33M3CoreComponents.GUI.Nuclex.Controls;
using S33M3CoreComponents.GUI.Nuclex.Controls.Desktop;
using SharpDX;

namespace Realms.Client.Components.GUI
{
    public class CreditsComponent : GameComponent
    {
        private readonly D3DEngine _engine;
        private readonly MainScreen _screen;
        private LabelControl _creditsLabel;
        private ButtonControl _backButton;

        public event EventHandler BackPressed;

        private void OnBackPressed()
        {
            var handler = BackPressed;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public CreditsComponent(D3DEngine engine, MainScreen screen)
        {
            if (engine == null) throw new ArgumentNullException("engine");
            if (screen == null) throw new ArgumentNullException("screen");
            _engine = engine;
            _screen = screen;

            _engine.ScreenSize_Updated += UpdateLayout;
        }

        public override void Initialize()
        {
            _creditsLabel = new LabelControl { Text = "Credits: \nFabian Ceressia [s33m3]\nIgor Romanov [rkit]\nVladislav Pozdnyakov [Erty Hackward]" };
            _backButton = new ButtonControl { Text = "Back" };
            _backButton.Pressed += delegate { OnBackPressed(); };
            UpdateLayout(_engine.ViewPort, _engine.BackBufferTex.Description);

            if (Updatable)
            {
                _screen.Desktop.Children.Add(_creditsLabel);
                _screen.Desktop.Children.Add(_backButton);
            }
        }

        protected override void OnUpdatableChanged(object sender, EventArgs args)
        {
            if (!IsInitialized) return;

            if (Updatable)
            {
                _screen.Desktop.Children.Add(_creditsLabel);
                _screen.Desktop.Children.Add(_backButton);
                UpdateLayout(_engine.ViewPort, _engine.BackBufferTex.Description);
            }
            else
            {
                _screen.Desktop.Children.Remove(_creditsLabel);
                _screen.Desktop.Children.Remove(_backButton);
            }
            
            base.OnUpdatableChanged(sender, args);
        }

        private void UpdateLayout(ViewportF viewport, Texture2DDescription newBackBufferDescr)
        {
            if (Updatable)
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
