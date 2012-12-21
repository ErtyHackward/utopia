using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3CoreComponents.GUI.Nuclex.Controls.Desktop;
using S33M3CoreComponents.Sprites2D;
using S33M3Resources.Structs;
using SharpDX.Direct3D11;
using S33M3CoreComponents.GUI.Nuclex;
using SharpDX;

namespace Sandbox.Client.Components.GUI.SinglePlayer
{
    public partial class SinglePlayerComponent
    {
        #region Private variables
        protected ButtonControl _btNew;
        protected ButtonControl _btSavedGame;

        //Panels
        protected SavedGamePanel _savedGamePanel;
        protected NewGamePanel _newGamePanel;
        #endregion

        #region Private methods
        protected override void InitializeComponent()
        {
            _windowLabel.Text = "Single player";

            _btNew = ToDispose(new ButtonControl
            {
                CustomImage = _stMenuButton,
                CustomImageDown = _stMenuDown,
                CustomImageHover = _stMenuHover,
                TextFontId = 1,
                Text = "New Game",
                Color = new ByteColor(200, 200, 200, 255)
            });
            _btNew.Pressed += delegate { btNewPressed(); };

            _btSavedGame = ToDispose(new ButtonControl
            {
                CustomImage = _stMenuButton,
                CustomImageDown = _stMenuDown,
                CustomImageHover = _stMenuHover,
                TextFontId = 1,
                Text = "Saved Games",
                Color = new ByteColor(200, 200, 200, 255)
            });
            _btSavedGame.Pressed += delegate { btSavedGame(); };

            //Add components to the left Panel, including the Derived class Components
            _leftMenuPanel.Children.Add(_btBack);
            _leftMenuPanel.Children.Add(_windowLabel);
            _leftMenuPanel.Children.Add(_btNew);
            _leftMenuPanel.Children.Add(_btSavedGame);
            _leftMenuPanel.Children.Add(_shadow);
            _leftMenuPanel.Children.Add(_cubesPatern);
            _leftMenuPanel.Children.Add(_linenPatern);

            _form.Children.Add(_backPanel);
            _form.Children.Add(_leftMenuPanel);

            UpdateLayout(_engine.ViewPort, _engine.BackBufferTex.Description);

            //Pre select Saved Game Panel
            btSavedGame();
        }

        protected override void UpdateLayout(ViewportF viewport, Texture2DDescription newBackBufferDescr)
        {
            base.UpdateLayout(viewport, newBackBufferDescr);

            if (Updatable)
            {
                int _headerHeight = (int)(viewport.Height * 0.1f);
                int btPlacementY = _headerHeight;

                btPlacementY += 20;
                _btNew.Bounds = new UniRectangle(5, btPlacementY, _leftMenuPanel.Bounds.Size.X.Offset - 10, 50);
                btPlacementY += 50;
                _btSavedGame.Bounds = new UniRectangle(5, btPlacementY, _leftMenuPanel.Bounds.Size.X.Offset - 10, 50);

                if (_savedGamePanel != null) _savedGamePanel.Resize();
                if (_newGamePanel != null) _newGamePanel.Resize();
            }
        }

        private void btNewPressed()
        {
            if (_newGamePanel == null)
            {
                _newGamePanel = new NewGamePanel(CommonResources, _currentWorldParameter, _vars, _guiManager) { Bounds = new UniRectangle(0, 0, _backPanel.Bounds.Size.X.Offset, _backPanel.Bounds.Size.Y.Offset) };
                _newGamePanel.BtCreate.Pressed += _btCreate_Pressed;
            }
            if (_backPanel.Children.Contains(_newGamePanel) == false)
            {
                _backPanel.Children.Clear();
                _backPanel.Children.Add(_newGamePanel);
            }
        }

        private void btSavedGame()
        {
            if (_savedGamePanel == null)
            {
                _savedGamePanel = new SavedGamePanel(CommonResources, _currentWorldParameter, _vars) { Bounds = new UniRectangle(0, 0, _backPanel.Bounds.Size.X.Offset, _backPanel.Bounds.Size.Y.Offset) };
                _savedGamePanel.BtLoad.Pressed += _btLoad_Pressed;
            }
            if (_backPanel.Children.Contains(_savedGamePanel) == false)
            {
                _backPanel.Children.Clear();
                _savedGamePanel.RefreshWorldListAsync();
                _backPanel.Children.Add(_savedGamePanel);
            }
        }

        void _btLoad_Pressed(object sender, EventArgs e)
        {
            OnStartingGameRequested();
        }

        void _btCreate_Pressed(object sender, EventArgs e)
        {
            if (_currentWorldParameter.WorldName != null)
            {
                OnStartingGameRequested();
            }
        }

        public override void BeforeDispose()
        {
            if(_savedGamePanel != null) _savedGamePanel.BtLoad.Pressed -= _btLoad_Pressed;
            if(_newGamePanel != null) _newGamePanel.BtCreate.Pressed -= _btCreate_Pressed;
        }
        #endregion
    }
}
