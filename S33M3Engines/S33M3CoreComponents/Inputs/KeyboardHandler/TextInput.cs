using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using S33M3Resources.Structs;
using S33M3DXEngine;
using System.Drawing;
using System.Windows.Forms;

namespace S33M3CoreComponents.Inputs.KeyboardHandler
{
    /// <summary>
    /// The aime of this class is to help realise a User Text inputed by keyboard.
    /// </summary>
    public class TextInput
    {
        #region Private Variables
        private StringBuilder _stringBuilder;
        private KeyboardManager _keyboardManager;
        private bool _isListening;
        private bool _textChanged = true;
        private bool _showCaret = false;
        private int _carretPositionInString = 0;
        private DateTime _caretSwitch = DateTime.Now;
        private bool _stringBuilderChanged;
        #endregion 

        #region Public Variables/Properties
        public bool isListening
        {
            get { return _isListening; }
            set
            {
                _isListening = value;
                ChangeKeyboardEventListening();
            }
        }

        public bool IsMultiline { get; set; }
        #endregion

        public TextInput(KeyboardManager keyboardManager, string InitTexte = "")
        {
            _keyboardManager = keyboardManager;

            _stringBuilder = new StringBuilder(InitTexte, 255);
            _isListening = false;
            if (string.IsNullOrEmpty(InitTexte) == false)
            {
                _stringBuilderChanged = true;
                _carretPositionInString = _stringBuilder.Length;
            }
        }
       
        #region Private methods
        private void ChangeKeyboardEventListening()
        {
            //Start Keyboard event listening if needed
            if (_keyboardManager.IsRunning == false && _isListening == true) _keyboardManager.IsRunning = true;
        }

        private void AddCharacterToDisplay(CharKey Key)
        {
            //handle not char keys
            if (Key.isChar == false)
            {
                _showCaret = true;

                switch (Key.Key)
                {
                    case Keys.Left:
                        _carretPositionInString--;
                        break;
                    case Keys.Right:
                        if (_carretPositionInString < _stringBuilder.Length) _carretPositionInString++;
                        break;
                    case Keys.Delete:
                        if (_stringBuilder.Length > 0 && (_carretPositionInString < _stringBuilder.Length))
                        {
                            _stringBuilder.Remove(_carretPositionInString, 1);
                        }
                        break;
                    case Keys.Home:
                        _carretPositionInString = 0;
                        break;
                    case Keys.End:
                        _carretPositionInString = _stringBuilder.Length;
                        break;
                    default:
                        break;
                }
            }
            else
            {
                switch (Key.Char)
                {
                    case (char)Keys.Back:

                        if (_stringBuilder.Length > 0)
                        {
                            _stringBuilder.Remove(_carretPositionInString - 1, 1);
                            _carretPositionInString--;
                            _showCaret = true;
                        }
                        return;
                    default:
                        if (IsMultiline == false && (Key.Char == '\n' || Key.Char == '\r')) return;
                        //Pure char character !
                        if (_carretPositionInString == _stringBuilder.Length)
                        {
                            _stringBuilder.Append(Key.Char);
                        }
                        else
                        {
                            _stringBuilder.Insert(_carretPositionInString, Key.Char);
                        }
                        _carretPositionInString++;
                        break;
                }
            }

          
        }
        #endregion

        #region Public methods
        public void Refresh()
        {
            _textChanged = false;

            if (_stringBuilderChanged)
            {
                _textChanged = true;
                _stringBuilderChanged = false;
            }

            if (_isListening == false && _showCaret == false) return;

            foreach (CharKey KeyChar in _keyboardManager.GetPressed())
            {
                AddCharacterToDisplay(KeyChar);
                _textChanged = true;
            }

            // swap carret display
            if ((DateTime.Now - _caretSwitch).TotalSeconds > 0.5 || (_showCaret && _isListening == false))
            {
                _showCaret = !_showCaret;
                _caretSwitch = DateTime.Now;
                _textChanged = true;
            }
        }

        public void Clear()
        {
            _stringBuilder.Clear();
            _carretPositionInString = 0;
            _stringBuilderChanged = true;
        }

        /// <summary>
        /// Get the string
        /// </summary>
        /// <param name="text">The Text</param>
        /// <param name="carretPositionInString">The Carret position, -1 if it should not be displayed</param>
        /// <returns>True if the string has change since last GetText call</returns>
        public bool GetText(out string text, out int carretPositionInString)
        {
            if (_showCaret) carretPositionInString = _carretPositionInString;
            else carretPositionInString = -1;

            text = _stringBuilder.ToString();

            return _textChanged;
        }

        /// <summary>
        /// Get the string
        /// </summary>
        /// <param name="text">The Text</param>
        /// <param name="carretPositionInString">The Carret position, -1 if it should not be displayed</param>
        /// <returns>True if the string has change since last GetText call</returns>
        public string GetText()
        {
            return _stringBuilder.ToString();
        }

        #endregion
    }
}
