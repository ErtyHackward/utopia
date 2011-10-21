﻿using System;
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
using S33M3Engines.Shared.Math;
using SharpDX;

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

        private Vector2I _centerViewPort;

        private bool _keyBoardListening;
        private Vector2I _cursorPosition;
        #endregion

        #region Public variables/Properties
        public Vector2I MouseMoveDelta = new Vector2I(0, 0);

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

            _engine.MouseCaptureChanged += _engine_MouseCaptureChanged;

            _keyBoardListening = false;

            //Subscibe the viewPort update
            engine.ViewPort_Updated += D3dEngine_ViewPort_Updated;
        }

        void _engine_MouseCaptureChanged(object sender, D3DEngineMouseCaptureChangedEventArgs e)
        {
            var mouseState = Mouse.GetState();
            if (e.MouseCaptured)
            {
                // save mouse position
                _cursorPosition = new Vector2I(mouseState.X, mouseState.Y);
                Mouse.SetPosition(_centerViewPort.X, _centerViewPort.Y);
                MouseMoveDelta.X = 0;
                MouseMoveDelta.Y = 0;
            }
            else
            {
                // restore
                Mouse.SetPosition(_cursorPosition.X, _cursorPosition.Y);
            }
        }

        public override void Dispose()
        {
            _engine.ViewPort_Updated -= D3dEngine_ViewPort_Updated;
            base.Dispose();
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

        /// <summary>
        /// Will tranform the mouse screen position into world coordinate + a direction vector called "MouseLookat"
        /// </summary>
        /// <param name="MouseWorldPosition"></param>
        /// <param name="MouseLookAt"></param>
        /// <param name="fixToCenter">to bypass the mouse pick and use screen center instead, useful for debug</param>
        public void UnprojectMouseCursor(out Vector3D MouseWorldPosition, out Vector3D MouseLookAt,bool fixToCenter=false)
        {
            //Get mouse Position on the screen
            var mouseState = Mouse.GetState();
            int x, y;
            if (fixToCenter)
            {
                y = (int)_cameraManager.ActiveCamera.Viewport.Height / 2;
                x = (int) _cameraManager.ActiveCamera.Viewport.Width / 2;
            } else
            {
                x = mouseState.X;
                y = mouseState.Y;
            }

            Vector3 nearClipVector = new Vector3(x, y, 0);
            Vector3 farClipVector = new Vector3(x, y, 1);

            Matrix cameraWVP = _cameraManager.ActiveCamera.ViewProjection3D;

            Vector3 UnprojecNearClipVector;
            Vector3.Unproject(ref nearClipVector,
                              _engine.ViewPort.TopLeftX,
                              _engine.ViewPort.TopLeftY,
                              _engine.ViewPort.Width,
                              _engine.ViewPort.Height,
                              _engine.ViewPort.MinDepth,
                              _engine.ViewPort.MaxDepth,
                              ref cameraWVP,
                              out UnprojecNearClipVector);

            Vector3 UnprojecFarClipVector;
            Vector3.Unproject(ref farClipVector,
                              _engine.ViewPort.TopLeftX,
                              _engine.ViewPort.TopLeftY,
                              _engine.ViewPort.Width,
                              _engine.ViewPort.Height,
                              _engine.ViewPort.MinDepth,
                              _engine.ViewPort.MaxDepth,
                              ref cameraWVP,
                              out UnprojecFarClipVector);

            //To apply From Camera Position !
            MouseWorldPosition = new Vector3D(UnprojecNearClipVector);
            MouseLookAt = new Vector3D(Vector3.Normalize(UnprojecFarClipVector - UnprojecNearClipVector));
        }
        #endregion

        #region Private Methods
        private void RegisterKeybardWinformEvents()
        {
            _engine.GameWindow.KeyPress += GameWindow_KeyPress;
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
            if (_engine.MouseCapture && _engine.HasFocus)
            {
                //Set the mouse to the Center Screen
                Mouse.SetPosition(_centerViewPort.X, _centerViewPort.Y);
                MouseMoveDelta.X = _centerViewPort.X - _curMouseState.X;
                MouseMoveDelta.Y = _centerViewPort.Y - _curMouseState.Y;
            }
        }

        private void D3dEngine_ViewPort_Updated(Viewport viewport)
        {
            ComputeCenterViewport(viewport);
        }

        private void ComputeCenterViewport(Viewport viewport)
        {
            _centerViewPort = new Vector2I((int)viewport.Width / 2, (int)viewport.Height / 2);
            if (_cursorPosition.IsZero()) _cursorPosition = _centerViewPort;
        }
        #endregion
    }
}
