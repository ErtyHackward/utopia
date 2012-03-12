using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using S33M3_DXEngine;
using System.Windows.Forms;

namespace S33M3_CoreComponents.Inputs.KeyboardHandler
{
    public class KeyboardManager : Component
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        #region Private variables/properties
        private D3DEngine _engine;
        private bool _isRunning;
        private bool _bufferUpdated;
        #endregion

        #region Public variables/properties
        public bool IsRunning
        {
            get { return _isRunning; }
            set
            {
                if (_isRunning == value) return;
                _isRunning = value;
                RegisterOrUnregisterFormEvents();
            }
        }

        public CharKey[] KeyBuffer;
        public int BufferSize;
        public KeyboardState CurKeyboardState;
        #endregion

        public KeyboardManager(D3DEngine engine)
        {
            _engine = engine;
            IsRunning = false;
            KeyBuffer = new CharKey[255];
            for (int i = 0; i < KeyBuffer.Length; i++)
            {
                KeyBuffer[i] = new CharKey();
            }
        }

        public override void Dispose()
        {
            IsRunning = false;
            base.Dispose();
        }

        #region Private methods
        private void RegisterOrUnregisterFormEvents()
        {
            if (_isRunning)
            {
                _engine.GameWindow.KeyPress += GameWindow_KeyPress;
                _engine.GameWindow.KeyDown += GameWindow_KeyDown;
            }
            else
            {
                _engine.GameWindow.KeyPress -= GameWindow_KeyPress;
                _engine.GameWindow.KeyDown -= GameWindow_KeyDown;
            }
        }

        void GameWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (
                e.KeyCode != Keys.Right && 
                e.KeyCode != Keys.Left && 
                e.KeyCode != Keys.Up && 
                e.KeyCode != Keys.Down && 
                e.KeyCode != Keys.Delete &&
                e.KeyCode != Keys.Home &&
                e.KeyCode != Keys.End) return;

            if (_bufferUpdated) ResetBuffer();
            KeyBuffer[BufferSize].Key = e.KeyCode;
            KeyBuffer[BufferSize].isChar = false;
            BufferSize++;
        }

        private void GameWindow_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (_bufferUpdated) ResetBuffer();
            KeyBuffer[BufferSize].Char = e.KeyChar;
            KeyBuffer[BufferSize].isChar = true;
            BufferSize++;
        }

        private void ResetBuffer()
        {
            BufferSize = 0;
            _bufferUpdated = false;
        }
        #endregion

        #region Public methods
        public void Update()
        {
            CurKeyboardState = Keyboard.GetState();
            if (_bufferUpdated) ResetBuffer();
            _bufferUpdated = true;
        }

        public IEnumerable<CharKey> GetPressedKeys()
        {
            for (int i = 0; i < BufferSize; i++)
            {
                yield return KeyBuffer[i];
            }
        }
        #endregion
    }
}
