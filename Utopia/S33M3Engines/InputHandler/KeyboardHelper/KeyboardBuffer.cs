using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Utopia.Shared.Config;

namespace S33M3Engines.InputHandler.KeyboardHelper
{
    public class KeyboardBuffer
    {
        private KeyboardState _curKeyboardState;
        private KeyboardState _prevKeyboardState;

        public bool IsKeyDown(Keys key)
        {
            return _curKeyboardState.IsKeyDown(key);
        }

        public bool IsKeyDown(KeyWithModifier key)
        {
            return _curKeyboardState.IsKeyDown(key);
        }

        public bool IsKeyUp(Keys key)
        {
            return _curKeyboardState.IsKeyUp(key);
        }

        public bool IsKeyUp(KeyWithModifier key)
        {
            return _curKeyboardState.IsKeyUp(key);
        }

        public bool IsKeyPressed(Keys key)
        {
            return _prevKeyboardState.IsKeyDown(key) && _curKeyboardState.IsKeyUp(key);
        }

        public bool IsKeyPressed(KeyWithModifier key)
        {
            if (key.Modifier != Keys.None)
            {
                return _curKeyboardState.IsKeyDown(key.Modifier) && _prevKeyboardState.IsKeyDown(key.MainKey) && _curKeyboardState.IsKeyUp(key.MainKey);
            }
            else
            {
                return _prevKeyboardState.IsKeyDown(key) && _curKeyboardState.IsKeyUp(key);
            }
        }
    }
}
