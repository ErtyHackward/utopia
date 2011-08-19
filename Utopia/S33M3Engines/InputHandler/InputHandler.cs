using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Engines.D3D;
using System.Runtime.InteropServices;
using S33M3Engines.InputHandler.KeyboardHelper;
using S33M3Engines.InputHandler.MouseHelper;
using System.Windows.Forms;
using Utopia.Shared;
using Utopia.Shared.Config;

namespace S33M3Engines.InputHandler
{
    public class InputHandlerManager
    {
        #region Private Variables
        private KeyboardState _curKeyboardState;
        private KeyboardState _prevKeyboardState;
        private MouseState _curMouseState;
        private MouseState _prevMouseState;
        #endregion

        #region Public properties
        public KeyboardState CurKeyboardState
        {
            get { return _curKeyboardState; }
        }
        public KeyboardState PrevKeyboardState
        {
            get { return _prevKeyboardState; }
        }
        public MouseState CurMouseState
        {
            get { return _curMouseState; }
        }
        public MouseState PrevMouseState
        {
            get { return _prevMouseState; }
        }
        #endregion

        public InputHandlerManager()
        {
            Initialize();
        }

        private void Initialize()
        {
            //Hooking Windows Mouse messages
            Mouse.mouseMessageHooker.WindowHandle = D3DEngine.WindowHandle;
            _curMouseState = Mouse.GetState();
            _curKeyboardState = Keyboard.GetState();
        }

        public void CleanUp()
        {
            Mouse.CleanUp();
        }

        public void GetCurrentMouseState(out MouseState mouseState)
        {
            mouseState = Mouse.GetState();
        }

        public void GetCurrentKeyboardState(out KeyboardState keyboardState)
        {
            keyboardState = Keyboard.GetState();
        }

        public bool IsKeyPressed(Keys key)
        {
            return _prevKeyboardState.IsKeyDown(key) && _curKeyboardState.IsKeyUp(key);
        }

        public bool IsKeyPressed(KeyWithModifier key)
        {
            if (key.Modifier != Keys.None)
            {
                return CurKeyboardState.IsKeyDown(key.Modifier) && PrevKeyboardState.IsKeyDown(key.MainKey) && CurKeyboardState.IsKeyUp(key.MainKey);
            }
            else
            {
                return _prevKeyboardState.IsKeyDown(key) && _curKeyboardState.IsKeyUp(key);
            }
        }

        public void ReshreshStates()
        {
            UpdatekeyBoardStatus();
            UpdateMouseStatus();
        }

        private void UpdateMouseStatus()
        {
            _prevMouseState = _curMouseState;
            _curMouseState = Mouse.GetState();
        }

        private void UpdatekeyBoardStatus()
        {
            _prevKeyboardState = _curKeyboardState;
            _curKeyboardState = Keyboard.GetState();
        }

    }
}
