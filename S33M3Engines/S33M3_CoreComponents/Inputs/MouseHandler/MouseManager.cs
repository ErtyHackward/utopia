using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using S33M3_Resources.Structs;
using S33M3_DXEngine;
using SharpDX.Direct3D11;
using S33M3_CoreComponents.Cameras;
using S33M3_CoreComponents.Cameras.Interfaces;

namespace S33M3_CoreComponents.Inputs.MouseHandler
{
    public class MouseManager : Component
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        #region Private Variables
        private D3DEngine _engine;
        private CameraManager<ICamera> _cameraManager;
        private Vector2I _centerViewPort;
        private Vector2I _mousePosiBeforeCaptureMode;
        private bool _mouseCapture;
        private int _mouseHideCount;
        private bool _isRunning;
        #endregion

        #region Public variables/properties
        public readonly Mouse Mouse;
        public Vector2I MouseMoveDelta = new Vector2I(0, 0);
        public int ScroolWheelDeltaValue = 0;
        public int ScroolWheelDeltaTick = 0;
        public MouseState CurMouseState;
        public MouseState PrevMouseState;

        /// <summary>
        /// Gets or sets whether the mouse is captured by the engine
        /// It make the mouse to be hided, and "snapped" to the center of the windows.
        /// The aim is to give the amount of move (MouseMoveDelta) of the mouse between 2 Mainloop Update()
        /// </summary>
        public bool MouseCapture
        {
            get { return _mouseCapture; }
            set
            {
                if (value != _mouseCapture)
                {
                    _mouseCapture = value;
                    if (_mouseCapture)
                    {
                        SaveMousePosition();
                        HideMouseCursor();
                    }
                    else
                    {
                        RestoreMousePosition();
                        ShowMouseCursor();
                    }
                }
            }
        }

        /// <summary>
        /// Stop/Start the mouse signals processing
        /// </summary>
        public bool IsRunning
        {
            get { return _isRunning; }
            set { _isRunning = value; }
        }
        #endregion

        public MouseManager(D3DEngine engine,
                            CameraManager<ICamera> cameraManager)
        {
#if DEBUG
            logger.Warn("No camera attached to the MouseManager => UnprojectMouseCursor won't be usable !");
#endif
            _engine = engine;
            _cameraManager = cameraManager;

            _engine.ViewPort_Updated += _engine_ViewPort_Updated;
            _engine.GameWindow.GotFocus += GameWindow_GotFocus;
            _engine.GameWindow.LostFocus += GameWindow_LostFocus;
            _engine.GameWindow.Closed += _renderForm_Closed;
            ComputeCenterViewport(_engine.ViewPort);

            Mouse = ToDispose(new Mouse());
            //Link the mouse to the windows handle
            Mouse.SetMouseMessageHooker = engine.GameWindow.Handle;
        }

        public override void Dispose()
        {
            _engine.ViewPort_Updated -= _engine_ViewPort_Updated;
            _engine.GameWindow.LostFocus -= GameWindow_LostFocus;
            _engine.GameWindow.Closed -= _renderForm_Closed;
            base.Dispose();
        }

        #region Private Methods
        private void ProcessMouseStates()
        {
            //If the mouse is hiden, then start tracking mouse mouvement, and recenter the mouse to the center of the screen at each update !
            if (_mouseCapture && _engine.HasFocus)
            {
                //Set the mouse to the Center Screen
                Mouse.SetPosition(_centerViewPort.X, _centerViewPort.Y);
                MouseMoveDelta.X = _centerViewPort.X - CurMouseState.X;
                MouseMoveDelta.Y = _centerViewPort.Y - CurMouseState.Y;
            }
            else
            {
                MouseMoveDelta.X = PrevMouseState.X - CurMouseState.X;
                MouseMoveDelta.Y = PrevMouseState.Y - CurMouseState.Y;
            }

            ScroolWheelDeltaValue = PrevMouseState.ScrollWheelValue - CurMouseState.ScrollWheelValue;
            ScroolWheelDeltaTick = PrevMouseState.ScrollWheelTicks - CurMouseState.ScrollWheelTicks;
        }

        private void SaveMousePosition()
        {
            CurMouseState = Mouse.GetState();
            _mousePosiBeforeCaptureMode.X = CurMouseState.X;
            _mousePosiBeforeCaptureMode.Y = CurMouseState.Y;
        }

        private void RestoreMousePosition()
        {
            Mouse.SetPosition(_mousePosiBeforeCaptureMode.X, _mousePosiBeforeCaptureMode.Y);
        }

        void GameWindow_GotFocus(object sender, EventArgs e)
        {
            if (MouseCapture) HideMouseCursor();
        }

        void GameWindow_LostFocus(object sender, EventArgs e)
        {
            ShowMouseCursor();
        }

        void _renderForm_Closed(object sender, EventArgs e)
        {
            ResetMouseCursor();
        }

        private void ComputeCenterViewport(Viewport viewport)
        {
            _centerViewPort = new Vector2I((int)viewport.Width / 2, (int)viewport.Height / 2);
        }

        private void _engine_ViewPort_Updated(Viewport viewport)
        {
            ComputeCenterViewport(viewport);
        }
        #endregion

        #region Public methods
        public void Update()
        {
            if (_isRunning == false) return;
            //Refresh mouse states
            PrevMouseState = CurMouseState;
            CurMouseState = Mouse.GetState();
            ProcessMouseStates();
        }

        /// <summary>
        /// Will tranform the mouse screen position into world coordinate + a direction vector called "MouseLookat"
        /// </summary>
        /// <param name="MouseWorldPosition"></param>
        /// <param name="MouseLookAt"></param>
        /// <param name="fixToCenter">to bypass the mouse pick and use screen center instead, useful for debug</param>
        public void UnprojectMouseCursor(ref Matrix cameraWVP, out Vector3D MouseWorldPosition, out Vector3D MouseLookAt, bool fixToCenter = false)
        {
            if (_cameraManager == null)
            {
                logger.Error("Cannot use UnprojectMouseCursor without Camera !!");
                throw new Exception("Cannot use UnprojectMouseCursor without Camera !!");
            }

            //Get mouse Position on the screen
            var mouseState = Mouse.GetState();
            int x, y;
            if (fixToCenter)
            {
                y = (int)_cameraManager.ActiveCamera.Viewport.Height / 2;
                x = (int)_cameraManager.ActiveCamera.Viewport.Width / 2;
            }
            else
            {
                x = mouseState.X;
                y = mouseState.Y;
            }

            Vector3 nearClipVector = new Vector3(x, y, 0);
            Vector3 farClipVector = new Vector3(x, y, 1);

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

        /// <summary>
        /// Will tranform the mouse screen position into world coordinate + a direction vector called "MouseLookat"
        /// </summary>
        /// <param name="MouseWorldPosition"></param>
        /// <param name="MouseLookAt"></param>
        /// <param name="fixToCenter">to bypass the mouse pick and use screen center instead, useful for debug</param>
        public void UnprojectMouseCursor(out Vector3D MouseWorldPosition, out Vector3D MouseLookAt, bool fixToCenter = false)
        {
            Matrix wvp = _cameraManager.ActiveCamera.ViewProjection3D;
            UnprojectMouseCursor(ref wvp, out MouseWorldPosition, out MouseLookAt, fixToCenter);
        }

        
        private void HideMouseCursor()
        {
            while (_mouseHideCount >= 0)
            {
                System.Windows.Forms.Cursor.Hide();
                _mouseHideCount--;
            }
        }

        private void ShowMouseCursor()
        {
            while (_mouseHideCount < 0)
            {
                System.Windows.Forms.Cursor.Show();
                _mouseHideCount++;
            }
        }

        private void ResetMouseCursor()
        {
            while (_mouseHideCount < 0)
            {
                System.Windows.Forms.Cursor.Show();
                _mouseHideCount++;
            }

            while (_mouseHideCount > 0)
            {
                System.Windows.Forms.Cursor.Hide();
                _mouseHideCount--;
            }
        }
        #endregion

    }
}
