using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3CoreComponents.Inputs.KeyboardHandler;
using S33M3CoreComponents.Inputs.MouseHandler;
using S33M3DXEngine;
using System.Diagnostics;
using System.Reflection;
using SharpDX;

namespace S33M3CoreComponents.Inputs.Actions
{
    /// <summary>
    /// The aim of this class is to be able to pool the Inputs devices, and to react to them to see if an action is fired, or not.
    /// Everything will run in an separated thread, running as fast as possible to avoid missing a input event (Keypress, ...)
    /// The result will be "buffered" until requested.
    /// </summary>
    public class ActionsManager : Component
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public struct ActionData
        {
            public bool Triggered;
            public float ElapsedTimeInS;
        }

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
        private float _actionTimeElapsedInS;

        private bool _isAction1Exposed;
        private ActionData[] _bufferedActions1;          // Is link to _bufferedActionsInProgress or _actions => They are swapped
        private ActionData[] _bufferedActions2;          // Is link to _bufferedActionsInProgress or _actions => They are swapped
        private ActionData[] _bufferedActionsInProgress; // BackGround buffer accumulator
        private ActionData[] _actions;                   // Default Accesible buffer

        private Type _actionType;                        //The Type that contains the possible actions
        private MouseManager _mouseManager;

        private bool _isExclusiveMode;
        #endregion

        #region Public variables/properties
        public bool isKeyboardActionsEnabled { get; set; }
        public bool isMouseActionsEnabled { get; set; }
        public bool IsExclusiveMode
        {
            get { return _isExclusiveMode; }
            set
            {
                _isExclusiveMode = value;
                ClearBuffer();
            }
        }

        public Type ActionType
        {
            get { return _actionType; }
            set { _actionType = value; ResizeBufferArrays(GetNbrActionsFromType(this.ActionType)); }
        }

        #endregion

        public ActionsManager(D3DEngine engine, MouseManager mouseManager ,Type actionType)
        {
            _engine = engine;
            _mouseManager = mouseManager;

            _keyboardActions = new List<KeyboardTriggeredAction>();
            _mouseActions = new List<MouseTriggeredAction>();

            this.ActionType = actionType;

            _actions = _bufferedActions1;
            _bufferedActionsInProgress = _bufferedActions2;
            _isAction1Exposed = true;

            isKeyboardActionsEnabled = true;
            isMouseActionsEnabled = true;
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
        /// Clear all buffered Actions raised
        /// </summary>
        public void ClearBuffer()
        {
            Array.Clear(_actions, 0, _actions.Length);
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
        public bool isTriggered(int actionId, bool withExclusive = false)
        {
            if (_isExclusiveMode && withExclusive == false) return false;
            return _actions[actionId].Triggered;
        }

        /// <summary>
        /// Is an action Triggered !
        /// </summary>
        /// <param name="action">The action to look at</param>
        /// <returns></returns>
        public bool isTriggered(int actionId, out float ActionTimeElapsedInS, bool withExclusive = false)
        {
            if (_isExclusiveMode && withExclusive == false)
            {
                ActionTimeElapsedInS = default(float);
                return false;
            }
            ActionData data = _actions[actionId];
            ActionTimeElapsedInS = data.ElapsedTimeInS;
            return data.Triggered;
        }

        #endregion

        #region Private methods
        private int GetNbrActionsFromType(Type actionType)
        {
#if DEBUG
            //Check if 2 actions share the same id
            HashSet<int> ActionsIds = new HashSet<int>();
            foreach (var field in actionType.GetFields(BindingFlags.Static | BindingFlags.FlattenHierarchy | BindingFlags.Public))
            {
                if (ActionsIds.Add((int)field.GetValue(null)) == false)
                {
                    logger.Error("Actions field {0} share its ActionsId value : {1} with another action !", field.Name, (int)field.GetValue(null));
                    throw new Exception();
                }
            }

            //Check if we don't have Id "Hole"
            int currentId = 0;
            foreach (var actionId in ActionsIds.OrderBy(x => x))
            {
                if (actionId != currentId)
                {
                    logger.Error("Action's ids dont follow a simple progression (0,1,2,...) missing id {0} : ", currentId);
                    throw new Exception();
                }
                currentId++;
            }

#endif
            return actionType.GetFields(BindingFlags.Static | BindingFlags.FlattenHierarchy | BindingFlags.Public).Length;
        }

        private void ResizeBufferArrays(int size)
        {
            _bufferedActions1 = new ActionData[size];
            _bufferedActions2 = new ActionData[size];
        }

        private void ProcessInputs()
        {
            if (isKeyboardActionsEnabled && _engine.HasFocus) ProcessKeyboardStates();
            if (isMouseActionsEnabled && _engine.HasFocus) ProcessMouseStates();
        }

        private void ProcessMouseStates()
        {
            //Refresh mouse states
            _prevMouseState = _curMouseState;
            _curMouseState = _mouseManager.Mouse.GetState();

            //Check if an action needs to be triggered
            for (int i = 0; i < _mouseActions.Count; i++)
            {
                _mouseAction = _mouseActions[i];

                //has this mouse action the need to be applied only in "Mouse Capture" mode ?
                if (_mouseAction.WithCursorLocked != null)
                {
                    if (_mouseAction.WithCursorLocked != _mouseManager.MouseCapture) continue;
                }

                switch (_mouseAction.TriggerType)
                {
                    case MouseTriggerMode.ButtonDown:
                        switch (_mouseAction.Binding)
                        {
                            case MouseButton.LeftButton:
                                if (_curMouseState.LeftButton == ButtonState.Pressed) _bufferedActionsInProgress[_mouseAction.ActionId].Triggered = true;
                                break;
                            case MouseButton.MiddleButton:
                                if (_curMouseState.MiddleButton == ButtonState.Pressed) _bufferedActionsInProgress[_mouseAction.ActionId].Triggered = true;
                                break;
                            case MouseButton.RightButton:
                                if (_curMouseState.RightButton == ButtonState.Pressed) _bufferedActionsInProgress[_mouseAction.ActionId].Triggered = true;
                                break;
                            case MouseButton.XButton1:
                                if (_curMouseState.XButton1 == ButtonState.Pressed) _bufferedActionsInProgress[_mouseAction.ActionId].Triggered = true;
                                break;
                            case MouseButton.XButton2:
                                if (_curMouseState.XButton2 == ButtonState.Pressed) _bufferedActionsInProgress[_mouseAction.ActionId].Triggered = true;
                                break;
                            case MouseButton.LeftAndRightButton:
                                if (_curMouseState.LeftButton == ButtonState.Pressed && _curMouseState.RightButton == ButtonState.Pressed) _bufferedActionsInProgress[_mouseAction.ActionId].Triggered = true;
                                break;
                        }
                        break;
                    case MouseTriggerMode.ButtonReleased:
                        //Set the Action Flag if required
                        switch (_mouseAction.Binding)
                        {
                            case MouseButton.LeftButton:
                                if (_curMouseState.LeftButton == ButtonState.Released && _prevMouseState.LeftButton == ButtonState.Pressed) _bufferedActionsInProgress[_mouseAction.ActionId].Triggered = true;
                                break;
                            case MouseButton.MiddleButton:
                                if (_curMouseState.MiddleButton == ButtonState.Released && _prevMouseState.MiddleButton == ButtonState.Pressed) _bufferedActionsInProgress[_mouseAction.ActionId].Triggered = true;
                                break;
                            case MouseButton.RightButton:
                                if (_curMouseState.RightButton == ButtonState.Released && _prevMouseState.RightButton == ButtonState.Pressed) _bufferedActionsInProgress[_mouseAction.ActionId].Triggered = true;
                                break;
                            case MouseButton.XButton1:
                                if (_curMouseState.XButton1 == ButtonState.Released && _prevMouseState.XButton1 == ButtonState.Pressed) _bufferedActionsInProgress[_mouseAction.ActionId].Triggered = true;
                                break;
                            case MouseButton.XButton2:
                                if (_curMouseState.XButton2 == ButtonState.Released && _prevMouseState.XButton2 == ButtonState.Pressed) _bufferedActionsInProgress[_mouseAction.ActionId].Triggered = true;
                                break;
                            case MouseButton.LeftAndRightButton:
                                if (_curMouseState.LeftButton == ButtonState.Released && _prevMouseState.LeftButton == ButtonState.Pressed && _curMouseState.RightButton == ButtonState.Released && _prevMouseState.RightButton == ButtonState.Pressed) _bufferedActionsInProgress[_mouseAction.ActionId].Triggered = true;
                                break;
                        }
                        break;
                    case MouseTriggerMode.ButtonPressed:
                        //Set the Action Flag if required
                        switch (_mouseAction.Binding)
                        {
                            case MouseButton.LeftButton:
                                if (_prevMouseState.LeftButton == ButtonState.Released && _curMouseState.LeftButton == ButtonState.Pressed) _bufferedActionsInProgress[_mouseAction.ActionId].Triggered = true;
                                break;
                            case MouseButton.MiddleButton:
                                if (_prevMouseState.MiddleButton == ButtonState.Released && _curMouseState.MiddleButton == ButtonState.Pressed) _bufferedActionsInProgress[_mouseAction.ActionId].Triggered = true;
                                break;
                            case MouseButton.RightButton:
                                if (_prevMouseState.RightButton == ButtonState.Released && _curMouseState.RightButton == ButtonState.Pressed) _bufferedActionsInProgress[_mouseAction.ActionId].Triggered = true;
                                break;
                            case MouseButton.XButton1:
                                if (_prevMouseState.XButton1 == ButtonState.Released && _curMouseState.XButton1 == ButtonState.Pressed) _bufferedActionsInProgress[_mouseAction.ActionId].Triggered = true;
                                break;
                            case MouseButton.XButton2:
                                if (_prevMouseState.XButton2 == ButtonState.Released && _curMouseState.XButton2 == ButtonState.Pressed) _bufferedActionsInProgress[_mouseAction.ActionId].Triggered = true;
                                break;
                            case MouseButton.LeftAndRightButton:
                                if (_prevMouseState.LeftButton == ButtonState.Released && _curMouseState.LeftButton == ButtonState.Pressed && _prevMouseState.RightButton == ButtonState.Released && _curMouseState.RightButton == ButtonState.Pressed) _bufferedActionsInProgress[_mouseAction.ActionId].Triggered = true;
                                break;
                        }
                        break;
                    case MouseTriggerMode.ScrollWheelForward:
                        if (_curMouseState.ScrollWheelTicks > _prevMouseState.ScrollWheelTicks)
                            _bufferedActionsInProgress[_mouseAction.ActionId].Triggered = true;
                        break;
                    case MouseTriggerMode.ScrollWheelBackWard:
                        if (_curMouseState.ScrollWheelTicks < _prevMouseState.ScrollWheelTicks)
                            _bufferedActionsInProgress[_mouseAction.ActionId].Triggered = true;
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
                switch (_keyboardAction.TriggerType)
                {
                    case KeyboardTriggerMode.KeyDown:
                        //Set the Action Flag if required
                        if (_curKeyboardState.IsKeyDown(_keyboardAction.Binding))
                            _bufferedActionsInProgress[_keyboardAction.ActionId].Triggered = true;
                        break;
                    case KeyboardTriggerMode.KeyReleased:
                        //Set start Action Flag if required
                        if (_keyboardAction.WithTimeElapsed &&
                            _prevKeyboardState.IsKeyDown(_keyboardAction.Binding) &&
                            _keyboardAction.StartTimeElapsedInTick == 0)
                        {
                            _keyboardAction.StartTimeElapsedInTick = Stopwatch.GetTimestamp();
                        }

                        if (_prevKeyboardState.IsKeyDown(_keyboardAction.Binding) && _curKeyboardState.IsKeyUp(_keyboardAction.Binding))
                        {
                            _bufferedActionsInProgress[_keyboardAction.ActionId].Triggered = true;
                            if (_keyboardAction.WithTimeElapsed)
                            {
                                _actionTimeElapsedInS = (float)(((Stopwatch.GetTimestamp() - _keyboardAction.StartTimeElapsedInTick) / (float)Stopwatch.Frequency));
                                if (_actionTimeElapsedInS > _keyboardAction.MaxTimeElapsedInS) _actionTimeElapsedInS = _keyboardAction.MaxTimeElapsedInS;
                                _bufferedActionsInProgress[_keyboardAction.ActionId].ElapsedTimeInS = _actionTimeElapsedInS;
                                _keyboardAction.StartTimeElapsedInTick = 0;
                            }
                        }
                        break;
                    case KeyboardTriggerMode.KeyPressed:
                        //Set the Action Flag if required

                        if (_keyboardAction.ActionId == 0)
                        {
                            //Console.WriteLine("_curKeyboardState key test : " + _curKeyboardState.IsKeyDown(_keyboardAction.Binding) + " _prevKeyboardState Key test : " + _prevKeyboardState.IsKeyUp(_keyboardAction.Binding));
                        }

                        if (_curKeyboardState.IsKeyDown(_keyboardAction.Binding) && _prevKeyboardState.IsKeyUp(_keyboardAction.Binding))
                        {
                            _bufferedActionsInProgress[_keyboardAction.ActionId].Triggered = true;
                        }
                        break;
                }
            }
        }
        #endregion
    }
}
