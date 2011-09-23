using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Utopia.Shared.Config;
using S33M3Engines.InputHandler.KeyboardHelper;
using S33M3Engines.InputHandler;
using S33M3Engines.InputHandler.MouseHelper;
using S33M3Engines;

namespace Utopia.Action
{
    /// <summary>
    /// The aim of this class is to be able to pool the Inputs devices, and to react to them to see if an action is fired, or not.
    /// Everything will run in an separated thread, running as fast as possible to avoid missing a input event (Keypress, ...)
    /// The result will be "buffered" until requested.
    /// </summary>
    public class ActionsManager : IDisposable
    {
        #region Private variables
        private KeyboardState _curKeyboardState;
        private KeyboardState _prevKeyboardState;
        private KeyboardTriggeredAction _keyboardAction;
        private List<KeyboardTriggeredAction> _keyboardActions;

        private MouseState _curMouseState;
        private MouseState _prevMouseState;
        private MouseTriggeredAction _mouseAction;
        private List<MouseTriggeredAction> _mouseActions;
        private D3DEngine _engine;
        
        private bool _isAction1Exposed;
        private bool[] _bufferedActions1;
        private bool[] _bufferedActions2;
        private bool[] _bufferedActionsInProgress;
        private bool[] _actions;        
        #endregion

        #region Public variables/properties
        public bool KeyboardActionsProcessing { get; set; }
        public bool MouseActionsProcessing { get; set; }
        #endregion

        public ActionsManager(D3DEngine engine)
        {
            _engine = engine;
            _keyboardActions = new List<KeyboardTriggeredAction>();
            _mouseActions = new List<MouseTriggeredAction>();

            _bufferedActions1 = new bool[Enum.GetValues(typeof(Actions)).Length];
            _bufferedActions2 = new bool[_bufferedActions1.Length];
            _actions = _bufferedActions1;
            _bufferedActionsInProgress = _bufferedActions2;
            _isAction1Exposed = true;

            KeyboardActionsProcessing = true;
            MouseActionsProcessing = true;
        }

        public void Dispose()
        {
        }

        #region Public methods
        /// <summary>
        /// Register new Actions triggered by a keyboard event
        /// </summary>
        /// <param name="action">The action</param>
        /// <param name="inputActivationMode">The input mode that will be needed to make this action fired</param>
        /// <param name="bindingKey">The keybard binding</param>
        public void AddActions(KeyboardTriggeredAction keyboardAction)
        {
            _keyboardActions.Add(keyboardAction);
        }

        /// <summary>
        /// Register new Actions triggered by a Mouse event
        /// </summary>
        /// <param name="action">The action</param>
        /// <param name="inputActivationMode">The input mode that will be needed to make this action fired</param>
        /// <param name="bindingKey">The keybard binding</param>
        public void AddActions(MouseTriggeredAction mouseAction)
        {
            _mouseActions.Add(mouseAction);
        }

        /// <summary>
        /// Will make the buffered actions public, and ready to be requested
        /// </summary>
        public void Update()
        {
            if (_isAction1Exposed)
            {
                _actions = _bufferedActions2;
                _bufferedActionsInProgress = _bufferedActions1;
                _isAction1Exposed = false;
            }
            else
            {
                _actions = _bufferedActions1;
                _bufferedActionsInProgress = _bufferedActions2;
                _isAction1Exposed = true;
            }

            //Reset array values
            Array.Clear(_bufferedActionsInProgress, 0, _bufferedActionsInProgress.Length);
        }

        /// <summary>
        /// Process inputs handlers to update Actions
        /// This must be called as much as possible
        /// </summary>
        public void FetchInputs()
        {
            ProcessInputs();
        }

        /// <summary>
        /// Is an action Triggered !
        /// </summary>
        /// <param name="action">The action to look at</param>
        /// <returns></returns>
        public bool isTriggered(Actions action)
        {
            return _actions[(int)action];
        }

        #endregion

        #region Private methods
        private void ProcessInputs()
        {
            if(KeyboardActionsProcessing && _engine.HasFocus) ProcessKeyboardStates();
            if (MouseActionsProcessing && _engine.HasFocus) ProcessMouseStates();
        }

        private void ProcessMouseStates()
        {
            //Refresh mouse states
            _prevMouseState = _curMouseState;
            _curMouseState = Mouse.GetState();

            //Check if an action needs to be triggered
            for (int i = 0; i < _mouseActions.Count; i++)
            {
                _mouseAction = _mouseActions[i];

                if (_mouseAction.WithCursorLocked != null)
                {
                    if (_mouseAction.WithCursorLocked == _engine.UnlockedMouse) continue;
                }

                switch (_mouseAction.TriggerType)
                {
                    case MouseTriggerMode.ButtonDown:
                        switch (_mouseAction.Binding)
	                    {
                            case MouseButton.LeftButton:
                                if (_curMouseState.LeftButton == ButtonState.Pressed) _bufferedActionsInProgress[(int)_mouseAction.Action] = true;
                                break;
                            case MouseButton.MiddleButton:
                                if (_curMouseState.MiddleButton == ButtonState.Pressed) _bufferedActionsInProgress[(int)_mouseAction.Action] = true;
                                break;
                            case MouseButton.RightButton:
                                if (_curMouseState.RightButton == ButtonState.Pressed) _bufferedActionsInProgress[(int)_mouseAction.Action] = true;
                                break;
                            case MouseButton.XButton1:
                                if (_curMouseState.XButton1 == ButtonState.Pressed) _bufferedActionsInProgress[(int)_mouseAction.Action] = true;
                                break;
                            case MouseButton.XButton2:
                                if (_curMouseState.XButton2 == ButtonState.Pressed) _bufferedActionsInProgress[(int)_mouseAction.Action] = true;
                                break;
                            case MouseButton.LeftAndRightButton:
                                if (_curMouseState.LeftButton == ButtonState.Pressed && _curMouseState.RightButton == ButtonState.Pressed) _bufferedActionsInProgress[(int)_mouseAction.Action] = true;
                                break;
	                    }
                        break;
                    case MouseTriggerMode.ButtonDownUp:
                        //Set the Action Flag if required
                        switch (_mouseAction.Binding)
                        {
                            case MouseButton.LeftButton:
                                if (_curMouseState.LeftButton == ButtonState.Released && _prevMouseState.LeftButton == ButtonState.Pressed) _bufferedActionsInProgress[(int)_mouseAction.Action] = true;
                                break;
                            case MouseButton.MiddleButton:
                                if (_curMouseState.MiddleButton == ButtonState.Released && _prevMouseState.MiddleButton == ButtonState.Pressed) _bufferedActionsInProgress[(int)_mouseAction.Action] = true;
                                break;
                            case MouseButton.RightButton:
                                if (_curMouseState.RightButton == ButtonState.Released && _prevMouseState.RightButton == ButtonState.Pressed) _bufferedActionsInProgress[(int)_mouseAction.Action] = true;
                                break;
                            case MouseButton.XButton1:
                                if (_curMouseState.XButton1 == ButtonState.Released && _prevMouseState.XButton1 == ButtonState.Pressed) _bufferedActionsInProgress[(int)_mouseAction.Action] = true;
                                break;
                            case MouseButton.XButton2:
                                if (_curMouseState.XButton2 == ButtonState.Released && _prevMouseState.XButton2 == ButtonState.Pressed) _bufferedActionsInProgress[(int)_mouseAction.Action] = true;
                                break;
                            case MouseButton.LeftAndRightButton:
                                if (_curMouseState.LeftButton == ButtonState.Released && _prevMouseState.LeftButton == ButtonState.Pressed && _curMouseState.RightButton == ButtonState.Released && _prevMouseState.RightButton == ButtonState.Pressed) _bufferedActionsInProgress[(int)_mouseAction.Action] = true;
                                break;
                        }
                        break;
                    case MouseTriggerMode.ButtonUpDown:
                        //Set the Action Flag if required
                        switch (_mouseAction.Binding)
                        {
                            case MouseButton.LeftButton:
                                if (_prevMouseState.LeftButton == ButtonState.Released && _curMouseState.LeftButton == ButtonState.Pressed) _bufferedActionsInProgress[(int)_mouseAction.Action] = true;
                                break;
                            case MouseButton.MiddleButton:
                                if (_prevMouseState.MiddleButton == ButtonState.Released && _curMouseState.MiddleButton == ButtonState.Pressed) _bufferedActionsInProgress[(int)_mouseAction.Action] = true;
                                break;
                            case MouseButton.RightButton:
                                if (_prevMouseState.RightButton == ButtonState.Released && _curMouseState.RightButton == ButtonState.Pressed) _bufferedActionsInProgress[(int)_mouseAction.Action] = true;
                                break;
                            case MouseButton.XButton1:
                                if (_prevMouseState.XButton1 == ButtonState.Released && _curMouseState.XButton1 == ButtonState.Pressed) _bufferedActionsInProgress[(int)_mouseAction.Action] = true;
                                break;
                            case MouseButton.XButton2:
                                if (_prevMouseState.XButton2 == ButtonState.Released && _curMouseState.XButton2 == ButtonState.Pressed) _bufferedActionsInProgress[(int)_mouseAction.Action] = true;
                                break;
                            case MouseButton.LeftAndRightButton:
                                if (_prevMouseState.LeftButton == ButtonState.Released && _curMouseState.LeftButton == ButtonState.Pressed && _prevMouseState.RightButton == ButtonState.Released && _curMouseState.RightButton == ButtonState.Pressed) _bufferedActionsInProgress[(int)_mouseAction.Action] = true;
                                break;
                        }
                        break;
                    case MouseTriggerMode.ScrollWheelForward:
                        if (_curMouseState.ScrollWheelTicks > _prevMouseState.ScrollWheelTicks) 
                            _bufferedActionsInProgress[(int)_mouseAction.Action] = true;
                        break;
                    case MouseTriggerMode.ScrollWheelBackWard:
                        if (_curMouseState.ScrollWheelTicks < _prevMouseState.ScrollWheelTicks) 
                            _bufferedActionsInProgress[(int)_mouseAction.Action] = true;
                        break;
                }
            }
        }

        private void ProcessKeyboardStates()
        {
            //Refresh Keyboard states
            _prevKeyboardState = _curKeyboardState;
            _curKeyboardState = Keyboard.GetState();

            //Check if an action needs to be triggered
            for (int i = 0; i < _keyboardActions.Count; i++)
            {
                _keyboardAction = _keyboardActions[i];
                switch ( _keyboardAction.TriggerType)
                {
                    case KeyboardTriggerMode.KeyDown:
                        //Set the Action Flag if required
                        if (_curKeyboardState.IsKeyDown(_keyboardAction.Binding))
                            _bufferedActionsInProgress[(int)_keyboardAction.Action] = true;
                        break;
                    case KeyboardTriggerMode.KeyDownUp:
                        //Set the Action Flag if required

                        if (_prevKeyboardState.IsKeyDown(_keyboardAction.Binding) && _curKeyboardState.IsKeyUp(_keyboardAction.Binding))
                        {
                            _bufferedActionsInProgress[(int)_keyboardAction.Action] = true;
                        }
                        break;
                    case KeyboardTriggerMode.KeyUpDown:
                        //Set the Action Flag if required

                        if (_curKeyboardState.IsKeyDown(_keyboardAction.Binding) && _prevKeyboardState.IsKeyUp(_keyboardAction.Binding))
                        {
                            _bufferedActionsInProgress[(int)_keyboardAction.Action] = true;
                        }
                        break;
                }
            }
        }
        #endregion
    }
}
