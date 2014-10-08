using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using S33M3DXEngine.Main;
using SharpDX;
using S33M3Resources.Structs;
using S33M3DXEngine;
using SharpDX.Direct3D11;
using S33M3CoreComponents.Cameras;
using S33M3CoreComponents.Cameras.Interfaces;
using Rectangle = System.Drawing.Rectangle;

namespace S33M3CoreComponents.Inputs.MouseHandler
{
    public class MouseManager : BaseComponent
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        #region Private Variables
        private D3DEngine _engine;
        private Vector2I _centerViewPort;
        private Vector2I _mousePosiBeforeCaptureMode;
        private bool _mouseCapture;
        private int _mouseHideCount;
        private bool _isRunning;
        private bool _strategyMode;
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
        /// Strategy mode shows the cursor and locks it inside the viewport
        /// </summary>
        public bool StrategyMode
        {
            get { return _strategyMode; }
            set
            {
                _strategyMode = value;

                if (_strategyMode)
                {
                    MouseCapture = false;
                    
                    Cursor.Clip = (Rectangle)_engine.GameWindow.Invoke(new Func<Rectangle>(() => _engine.GameWindow.RectangleToScreen(_engine.GameWindow.ClientRectangle))); ;
                }
                else
                {
                    Cursor.Clip = new Rectangle();
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

        public MouseManager(D3DEngine engine)
        {
            _engine = engine;

            _engine.ScreenSize_Updated += _engine_ScreenSize_Updated;
            _engine.GameWindow.GotFocus += GameWindow_GotFocus;
            _engine.GameWindow.LostFocus += GameWindow_LostFocus;
            _engine.GameWindow.Closed += _renderForm_Closed;
            ComputeCenterViewport(_engine.ViewPort);

            Mouse = ToDispose(new Mouse());
            //Link the mouse to the windows handle
            Mouse.SetMouseMessageHooker = engine.GameWindow.Handle;
        }

        public override void BeforeDispose()
        {
            _engine.ScreenSize_Updated -= _engine_ScreenSize_Updated;
            _engine.GameWindow.LostFocus -= GameWindow_LostFocus;
            _engine.GameWindow.Closed -= _renderForm_Closed;
        }

        #region Private Methods
        private void ProcessMouseStates()
        {
            //If the mouse is hiden, then start tracking mouse mouvement, and recenter the mouse to the center of the screen at each update !
            if (_mouseCapture && _engine.HasFocus)
            {
                //Set the mouse to the Center Screen
                MouseMoveDelta.X = _centerViewPort.X - CurMouseState.X;
                MouseMoveDelta.Y = _centerViewPort.Y - CurMouseState.Y;
                Mouse.SetPosition(_centerViewPort.X, _centerViewPort.Y);
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
            Mouse.SetPosition(_centerViewPort.X, _centerViewPort.Y);
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

        private void ComputeCenterViewport(ViewportF viewport)
        {
            _centerViewPort = new Vector2I((int)viewport.Width / 2, (int)viewport.Height / 2);
        }

        private void _engine_ScreenSize_Updated(ViewportF viewport, Texture2DDescription newBackBufferDescr)
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
        public void UnprojectMouseCursor(ICamera camera, out Vector3D MouseWorldPosition, out Vector3D MouseLookAt, bool fixToCenter = false)
        {
            //Get mouse Position on the screen
            var mouseState = Mouse.GetState();
            int x, y;
            if (fixToCenter)
            {
                y = (int)camera.Viewport.Height / 2;
                x = (int)camera.Viewport.Width / 2;
            }
            else
            {
                x = mouseState.X;
                y = mouseState.Y;
            }

            Vector3 nearClipVector = new Vector3(x, y, 0);
            Vector3 farClipVector = new Vector3(x, y, 1);

            Matrix cameraWVP = camera.ViewProjection3D;

            Vector3 UnprojecNearClipVector;
            Vector3.Unproject(ref nearClipVector,
                              _engine.ViewPort.X,
                              _engine.ViewPort.Y,
                              _engine.ViewPort.Width,
                              _engine.ViewPort.Height,
                              _engine.ViewPort.MinDepth,
                              _engine.ViewPort.MaxDepth,
                              ref cameraWVP,
                              out UnprojecNearClipVector);

            Vector3 UnprojecFarClipVector;
            Vector3.Unproject(ref farClipVector,
                              _engine.ViewPort.X,
                              _engine.ViewPort.Y,
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

        public void UnprojectMouseCursor(ref Matrix viewProjection, out Vector3D mouseWorldPosition, out Vector3D mouseLookAt)
        {
            //Get mouse Position on the screen
            var mouseState = Mouse.GetState();

            var x = mouseState.X;
            var y = mouseState.Y;


            var nearClipVector = new Vector3D(x, y, 0);
            var farClipVector = new Vector3D(x, y, 1);

            Vector3D unprojecNearClipVector;
            Vector3D.Unproject(ref nearClipVector,
                              _engine.ViewPort.X,
                              _engine.ViewPort.Y,
                              _engine.ViewPort.Width,
                              _engine.ViewPort.Height,
                              _engine.ViewPort.MinDepth,
                              _engine.ViewPort.MaxDepth,
                              ref viewProjection,
                              out unprojecNearClipVector);

            Vector3D unprojecFarClipVector;
            Vector3D.Unproject(ref farClipVector,
                              _engine.ViewPort.X,
                              _engine.ViewPort.Y,
                              _engine.ViewPort.Width,
                              _engine.ViewPort.Height,
                              _engine.ViewPort.MinDepth,
                              _engine.ViewPort.MaxDepth,
                              ref viewProjection,
                              out unprojecFarClipVector);

            //To apply From Camera Position !
            mouseWorldPosition = unprojecNearClipVector;
            mouseLookAt = Vector3D.Normalize(unprojecFarClipVector - unprojecNearClipVector);
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
