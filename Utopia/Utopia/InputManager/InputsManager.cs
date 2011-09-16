using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Engines.D3D;
using S33M3Engines.InputHandler.KeyboardHelper;
using S33M3Engines.InputHandler.MouseHelper;
using S33M3Engines.InputHandler;
using S33M3Engines;
using S33M3Engines.Cameras;
using SharpDX.Direct3D11;
using Utopia.Shared.Structs;
using System.Windows.Forms;

namespace Utopia.InputManager
{
    /// <summary>
    /// Will handle basic InputManager work
    /// </summary>
    public class InputsManager : GameComponent
    {
        #region Private variables
        private MouseState _curMouseState;

        private D3DEngine _engine;
        private CameraManager _cameraManager;

        private IntVector2 _centerViewPort;

        private bool _keyBoardListening;
        #endregion

        #region Public variables/Properties
        public IntVector2 MouseMoveDelta = new IntVector2(0, 0);

        public delegate void KeyPress(object sender, KeyPressEventArgs e);
        public event KeyPress OnKeyPressed;

        public bool KeyBoardListening
        {
            get { return _keyBoardListening; }
            set
            {
                _keyBoardListening = value;
                if (value) RegisterKeybardWinformEvents();
                else UnRegisterKeybardWinformEvents();
            }
        }
        #endregion

        public InputsManager(D3DEngine engine, CameraManager cameraManager)
            :base()
        {
            _engine = engine;
            _cameraManager = cameraManager;
            //Should have the smallest UpdateOrder possible.
            this.UpdateOrder = 0;

            _keyBoardListening = false;

            //Subscibe the viewPort update
            engine.ViewPort_Updated += D3dEngine_ViewPort_Updated;
        }

        #region Public Methods
        public override void Initialize()
        {
            ComputeCenterViewport(_engine.ViewPort);
        }

        public override void Update(ref GameTime timeSpent)
        {
            //Refresh mouse states
            _curMouseState = Mouse.GetState();

            ProcessMouseStates();
        }
        #endregion

        #region Private Methods
        private void RegisterKeybardWinformEvents()
        {
            _engine.GameWindow.KeyPress += new KeyPressEventHandler(GameWindow_KeyPress);
        }

        private void UnRegisterKeybardWinformEvents()
        {
            _engine.GameWindow.KeyPress -= GameWindow_KeyPress;
        }

        private void GameWindow_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (OnKeyPressed != null) OnKeyPressed(this, e);
        }

        private void ProcessMouseStates()
        {
            //If the mouse is hiden, then start tracking mouse mouvement, and recenter the mouse to the center of the screen at each update !
            if (!_engine.UnlockedMouse)
            {
                //Set the mouse to the Center Screen
                Mouse.SetPosition(_centerViewPort.X, _centerViewPort.Y);
                MouseMoveDelta.X = _curMouseState.X - _centerViewPort.X;
                MouseMoveDelta.Y = _curMouseState.Y - _centerViewPort.Y;
            }
        }

        private void D3dEngine_ViewPort_Updated(Viewport viewport)
        {
            ComputeCenterViewport(viewport);
        }

        private void ComputeCenterViewport(Viewport viewport)
        {
            _centerViewPort = new Location2<int>((int)viewport.Width / 2, (int)viewport.Height / 2);
        }
        #endregion
    }
}
