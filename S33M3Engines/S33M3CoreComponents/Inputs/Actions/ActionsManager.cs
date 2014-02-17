using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3CoreComponents.Inputs.KeyboardHandler;
using S33M3CoreComponents.Inputs.MouseHandler;
using S33M3DXEngine;
using System.Diagnostics;
using System.Reflection;
using S33M3DXEngine.Main;
using SharpDX;

namespace S33M3CoreComponents.Inputs.Actions
{
    /// <summary>
    /// The aim of this class is to be able to pool the Inputs devices, and to react to them to see if an action is fired, or not.
    /// Everything will run in an separated thread, running as fast as possible to avoid missing a input event (Keypress, ...)
    /// The result will be "buffered" until requested.
    /// </summary>
    public class ActionsManager : BaseComponent
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        [Flags]
        public enum ActionRaisedSources
        {
            None = 0,
            Mouse = 1,
            Keyboard = 2
        }

        public struct ActionData
        {
            public bool Triggered;
            public float ElapsedTimeInS;
            public ActionRaisedSources RaisedSources;
            public bool IsAutoRepeatedEvent;
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

        private bool _isMouseExclusiveMode;
        private int _isMouseExclusiveModeCpt = 0;
        private bool _isKeyboardExclusiveMode;
        private int _isKeyboardExclusiveModeCpt = 0;
        #endregion

        #region Public variables/properties
        public bool isKeyboardActionsEnabled { get; set; }
        public bool isMouseActionsEnabled { get; set; }

        public bool IsFullExclusiveMode
        {
            get { return _isMouseExclusiveMode && _isKeyboardExclusiveMode; }
            set
            {
                IsMouseExclusiveMode = value;
                IsKeyboardExclusiveMode = value;
                ClearBuffer();
            }
        }

        public bool IsMouseExclusiveMode
        {
            get { return _isMouseExclusiveMode; }
            set
            {
                _isMouseExclusiveMode = value;
                if (value) _isMouseExclusiveModeCpt++;
                else _isMouseExclusiveModeCpt--;
                _isMouseExclusiveMode = _isMouseExclusiveModeCpt > 0;
#if DEBUG
                logger.Trace("Action Manager MouseExclusive Mode change to {0} ; Cpt : {1}", _isMouseExclusiveMode, _isMouseExclusiveModeCpt);
#endif
            }
        }

        public bool IsKeyboardExclusiveMode
        {
            get { return _isKeyboardExclusiveMode; }
            set
            {
                if (value) _isKeyboardExclusiveModeCpt++;
                else _isKeyboardExclusiveModeCpt--;
                _isKeyboardExclusiveMode = _isKeyboardExclusiveModeCpt > 0;
#if DEBUG
                logger.Trace("Action Manager KeyboardExclusive Mode change to {0} ; Cpt : {1}", _isKeyboardExclusiveMode, _isKeyboardExclusiveModeCpt);
#endif
            }
        }

        public Type ActionType
        {
            get { return _actionType; }
            set { _actionType = value; ResizeBufferArrays(GetNbrActionsFromType(this.ActionType)); }
        }

        #endregion

        public event EventHandler<ActionsManagerEventArgs> KeyboardAction;

        protected void OnKeyboardAction(ActionsManagerEventArgs e)
        {
            var handler = KeyboardAction;
            if (handler != null) handler(this, e);
        }

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
        public void AddActions(KeyboardTriggeredAction keyboardAction, bool rebind = false)
        {
            if (rebind)
            {
                //Remove the previously added Action
                int nbr = _keyboardActions.RemoveAll(x => x.ActionId == keyboardAction.ActionId && x.TriggerType == keyboardAction.TriggerType);
                logger.Warn("Rebinded KeyboardBinding remove more than one action : {0}", nbr);
            }
            _keyboardActions.Add(keyboardAction);
        }

        /// <summary>
        /// Register new Actions triggered by a Mouse event
        /// </summary>
        /// <param name="action">The action</param>
        /// <param name="inputActivationMode">The input mode that will be needed to make this action fired</param>
        /// <param name="bindingKey">The keybard binding</param>
        public void AddActions(MouseTriggeredAction mouseAction, bool rebind = false)
        {
            if (rebind)
            {
                //Remove the previously added Action
                int nbr = _mouseActions.RemoveAll(x => x.ActionId == mouseAction.ActionId && x.TriggerType == mouseAction.TriggerType);
                logger.Warn("Rebinded MouseBinding remove more than one action : {0}", nbr);
            }
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

            foreach (var keyboardAction in _keyboardActions)
            {
                if (isTriggered(keyboardAction.ActionId))
                    OnKeyboardAction(new ActionsManagerEventArgs { Action = keyboardAction});
            }
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
            if (withExclusive == false)
            {
                //If I'm not in keyboard exclusive mode, and this action has been at least raised by the keyboard, Do the trigger test
                if (_isKeyboardExclusiveMode == false && (_actions[actionId].RaisedSources & ActionRaisedSources.Keyboard) == ActionRaisedSources.Keyboard)
                {
                    return true;
                }

                //If I'm not in Mouse exclusive mode, and this action has been at least raised by the Mouse, Do the trigger test
                if (_isMouseExclusiveMode == false && (_actions[actionId].RaisedSources & ActionRaisedSources.Mouse) == ActionRaisedSources.Mouse)
                {
                    return true;
                }

                return false;
            }

            return _actions[actionId].Triggered;
        }

        /// <summary>
        /// Is an action Triggered !
        /// </summary>
        /// <param name="action">The action to look at</param>
        /// <returns></returns>
        public bool isTriggered(int actionId, out float ActionTimeElapsedInS, bool withExclusive = false)
        {
            ActionData data = _actions[actionId];
            ActionTimeElapsedInS = data.ElapsedTimeInS;

            if (withExclusive == false)
            {
                //If I'm not in keyboard exclusive mode, and this action has been at least raised by the keyboard, Do the trigger test
                if (_isKeyboardExclusiveMode == false && (data.RaisedSources & ActionRaisedSources.Keyboard) == ActionRaisedSources.Keyboard)
                {
                    return true;
                }

                //If I'm not in Mouse exclusive mode, and this action has been at least raised by the Mouse, Do the trigger test
                if (_isMouseExclusiveMode == false && (data.RaisedSources & ActionRaisedSources.Mouse) == ActionRaisedSources.Mouse)
                {
                    return true;
                }

                return false;
            }

            return data.Triggered;
        }


        /// <summary>
        /// Is an action Triggered !
        /// </summary>
        /// <param name="action">The action to look at</param>
        /// <returns></returns>
        public bool isTriggered(int actionId, out bool IsAutoRepeatedEvent, bool withExclusive = false)
        {
            ActionData data = _actions[actionId];
            IsAutoRepeatedEvent = data.IsAutoRepeatedEvent;

            if (withExclusive == false)
            {
                //If I'm not in keyboard exclusive mode, and this action has been at least raised by the keyboard, Do the trigger test
                if (_isKeyboardExclusiveMode == false && (_actions[actionId].RaisedSources & ActionRaisedSources.Keyboard) == ActionRaisedSources.Keyboard)
                {
                    return true;
                }

                //If I'm not in Mouse exclusive mode, and this action has been at least raised by the Mouse, Do the trigger test
                if (_isMouseExclusiveMode == false && (_actions[actionId].RaisedSources & ActionRaisedSources.Mouse) == ActionRaisedSources.Mouse)
                {
                    return true;
                }

                return false;
            }

            return _actions[actionId].Triggered;
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
                                if (_curMouseState.LeftButton == ButtonState.Pressed) { _bufferedActionsInProgress[_mouseAction.ActionId].Triggered = true; _bufferedActionsInProgress[_mouseAction.ActionId].RaisedSources |= ActionRaisedSources.Mouse; }
                                break;
                            case MouseButton.MiddleButton:
                                if (_curMouseState.MiddleButton == ButtonState.Pressed) { _bufferedActionsInProgress[_mouseAction.ActionId].Triggered = true; _bufferedActionsInProgress[_mouseAction.ActionId].RaisedSources |= ActionRaisedSources.Mouse; }
                                break;
                            case MouseButton.RightButton:
                                if (_curMouseState.RightButton == ButtonState.Pressed) { _bufferedActionsInProgress[_mouseAction.ActionId].Triggered = true; _bufferedActionsInProgress[_mouseAction.ActionId].RaisedSources |= ActionRaisedSources.Mouse; }
                                break;
                            case MouseButton.XButton1:
                                if (_curMouseState.XButton1 == ButtonState.Pressed) { _bufferedActionsInProgress[_mouseAction.ActionId].Triggered = true; _bufferedActionsInProgress[_mouseAction.ActionId].RaisedSources |= ActionRaisedSources.Mouse; }
                                break;
                            case MouseButton.XButton2:
                                if (_curMouseState.XButton2 == ButtonState.Pressed) { _bufferedActionsInProgress[_mouseAction.ActionId].Triggered = true; _bufferedActionsInProgress[_mouseAction.ActionId].RaisedSources |= ActionRaisedSources.Mouse; }
                                break;
                            case MouseButton.LeftAndRightButton:
                                if (_curMouseState.LeftButton == ButtonState.Pressed && _curMouseState.RightButton == ButtonState.Pressed) { _bufferedActionsInProgress[_mouseAction.ActionId].Triggered = true; _bufferedActionsInProgress[_mouseAction.ActionId].RaisedSources |= ActionRaisedSources.Mouse; }
                                break;
                        }
                        break;
                    case MouseTriggerMode.ButtonReleased:
                        //Set the Action Flag if required
                        switch (_mouseAction.Binding)
                        {
                            case MouseButton.LeftButton:
                                if (_curMouseState.LeftButton == ButtonState.Released && _prevMouseState.LeftButton == ButtonState.Pressed) { _bufferedActionsInProgress[_mouseAction.ActionId].Triggered = true; _bufferedActionsInProgress[_mouseAction.ActionId].RaisedSources |= ActionRaisedSources.Mouse; }
                                break;
                            case MouseButton.MiddleButton:
                                if (_curMouseState.MiddleButton == ButtonState.Released && _prevMouseState.MiddleButton == ButtonState.Pressed) { _bufferedActionsInProgress[_mouseAction.ActionId].Triggered = true; _bufferedActionsInProgress[_mouseAction.ActionId].RaisedSources |= ActionRaisedSources.Mouse; }
                                break;
                            case MouseButton.RightButton:
                                if (_curMouseState.RightButton == ButtonState.Released && _prevMouseState.RightButton == ButtonState.Pressed) { _bufferedActionsInProgress[_mouseAction.ActionId].Triggered = true; _bufferedActionsInProgress[_mouseAction.ActionId].RaisedSources |= ActionRaisedSources.Mouse; }
                                break;
                            case MouseButton.XButton1:
                                if (_curMouseState.XButton1 == ButtonState.Released && _prevMouseState.XButton1 == ButtonState.Pressed) { _bufferedActionsInProgress[_mouseAction.ActionId].Triggered = true; _bufferedActionsInProgress[_mouseAction.ActionId].RaisedSources |= ActionRaisedSources.Mouse; }
                                break;
                            case MouseButton.XButton2:
                                if (_curMouseState.XButton2 == ButtonState.Released && _prevMouseState.XButton2 == ButtonState.Pressed) { _bufferedActionsInProgress[_mouseAction.ActionId].Triggered = true; _bufferedActionsInProgress[_mouseAction.ActionId].RaisedSources |= ActionRaisedSources.Mouse; }
                                break;
                            case MouseButton.LeftAndRightButton:
                                if (_curMouseState.LeftButton == ButtonState.Released && _prevMouseState.LeftButton == ButtonState.Pressed && _curMouseState.RightButton == ButtonState.Released && _prevMouseState.RightButton == ButtonState.Pressed) { _bufferedActionsInProgress[_mouseAction.ActionId].Triggered = true; _bufferedActionsInProgress[_mouseAction.ActionId].RaisedSources |= ActionRaisedSources.Mouse; }
                                break;
                        }
                        break;
                    case MouseTriggerMode.ButtonPressed:

                        //Set the Action Flag if required
                        switch (_mouseAction.Binding)
                        {
                            case MouseButton.LeftButton:
                                if (_mouseAction.WithAutoResetButtonPressed && _mouseAction.StartTimeAutoResetTick == 0 && _curMouseState.LeftButton == ButtonState.Pressed)
                                {
                                    //Mouse Button DOWN and autoresetting its ButtonPressed value - Memorize when the time when the mouse has been pressed
                                    _mouseAction.StartTimeAutoResetTick = Stopwatch.GetTimestamp();
                                }

                                if (_mouseAction.WithAutoResetButtonPressed)
                                {
                                    _actionTimeElapsedInS = (float)(((Stopwatch.GetTimestamp() - _mouseAction.StartTimeAutoResetTick) / (float)Stopwatch.Frequency));
                                }

                                if ((_prevMouseState.LeftButton == ButtonState.Released && _curMouseState.LeftButton == ButtonState.Pressed) ||
                                    (_mouseAction.WithAutoResetButtonPressed && _curMouseState.LeftButton == ButtonState.Pressed && _actionTimeElapsedInS >= _mouseAction.AutoResetTimeInS)
                                    )
                                {
                                    _bufferedActionsInProgress[_mouseAction.ActionId].Triggered = true; 
                                    _bufferedActionsInProgress[_mouseAction.ActionId].RaisedSources |= ActionRaisedSources.Mouse;
                                    if (_prevMouseState.LeftButton == ButtonState.Pressed) _bufferedActionsInProgress[_mouseAction.ActionId].IsAutoRepeatedEvent = true;
                                    if (_mouseAction.WithAutoResetButtonPressed)
                                    {
                                            _mouseAction.StartTimeAutoResetTick = Stopwatch.GetTimestamp();
                                    }
                                }

                                if (_mouseAction.WithAutoResetButtonPressed && _curMouseState.LeftButton != ButtonState.Pressed)
                                {
                                    _mouseAction.StartTimeAutoResetTick = 0;
                                }
                                break;
                            case MouseButton.MiddleButton:
                                if (_mouseAction.WithAutoResetButtonPressed && _mouseAction.StartTimeAutoResetTick == 0 && _curMouseState.middleButton == ButtonState.Pressed)
                                {
                                    //Mouse Button DOWN and autoresetting its ButtonPressed value - Memorize when the time when the mouse has been pressed
                                    _mouseAction.StartTimeAutoResetTick = Stopwatch.GetTimestamp();
                                }

                                if (_mouseAction.WithAutoResetButtonPressed)
                                {
                                    _actionTimeElapsedInS = (float)(((Stopwatch.GetTimestamp() - _mouseAction.StartTimeAutoResetTick) / (float)Stopwatch.Frequency));
                                }

                                if ((_prevMouseState.middleButton == ButtonState.Released && _curMouseState.middleButton == ButtonState.Pressed) ||
                                    (_mouseAction.WithAutoResetButtonPressed && _curMouseState.middleButton == ButtonState.Pressed && _actionTimeElapsedInS >= _mouseAction.AutoResetTimeInS)
                                    )
                                {
                                    _bufferedActionsInProgress[_mouseAction.ActionId].Triggered = true; 
                                    _bufferedActionsInProgress[_mouseAction.ActionId].RaisedSources |= ActionRaisedSources.Mouse;
                                    if (_prevMouseState.middleButton == ButtonState.Pressed) _bufferedActionsInProgress[_mouseAction.ActionId].IsAutoRepeatedEvent = true;
                                    if (_mouseAction.WithAutoResetButtonPressed)
                                    {
                                            _mouseAction.StartTimeAutoResetTick = Stopwatch.GetTimestamp();
                                    }
                                }

                                if (_mouseAction.WithAutoResetButtonPressed && _curMouseState.middleButton != ButtonState.Pressed)
                                {
                                    _mouseAction.StartTimeAutoResetTick = 0;
                                }
                                break;
                            case MouseButton.RightButton:
                                if (_mouseAction.WithAutoResetButtonPressed && _mouseAction.StartTimeAutoResetTick == 0 && _curMouseState.rightButton == ButtonState.Pressed)
                                {
                                    //Mouse Button DOWN and autoresetting its ButtonPressed value - Memorize when the time when the mouse has been pressed
                                    _mouseAction.StartTimeAutoResetTick = Stopwatch.GetTimestamp();
                                }

                                if (_mouseAction.WithAutoResetButtonPressed)
                                {
                                    _actionTimeElapsedInS = (float)(((Stopwatch.GetTimestamp() - _mouseAction.StartTimeAutoResetTick) / (float)Stopwatch.Frequency));
                                }

                                if ((_prevMouseState.rightButton == ButtonState.Released && _curMouseState.rightButton == ButtonState.Pressed) ||
                                    (_mouseAction.WithAutoResetButtonPressed && _curMouseState.rightButton == ButtonState.Pressed && _actionTimeElapsedInS >= _mouseAction.AutoResetTimeInS)
                                    )
                                {
                                    _bufferedActionsInProgress[_mouseAction.ActionId].Triggered = true; 
                                    _bufferedActionsInProgress[_mouseAction.ActionId].RaisedSources |= ActionRaisedSources.Mouse;
                                    if (_prevMouseState.rightButton == ButtonState.Pressed) _bufferedActionsInProgress[_mouseAction.ActionId].IsAutoRepeatedEvent = true;
                                    if (_mouseAction.WithAutoResetButtonPressed)
                                    {
                                            _mouseAction.StartTimeAutoResetTick = Stopwatch.GetTimestamp();
                                    }
                                }

                                if (_mouseAction.WithAutoResetButtonPressed && _curMouseState.rightButton != ButtonState.Pressed)
                                {
                                    _mouseAction.StartTimeAutoResetTick = 0;
                                }
                                break;
                            case MouseButton.XButton1:
                                if (_mouseAction.WithAutoResetButtonPressed && _mouseAction.StartTimeAutoResetTick == 0 && _curMouseState.XButton1 == ButtonState.Pressed)
                                {
                                    //Mouse Button DOWN and autoresetting its ButtonPressed value - Memorize when the time when the mouse has been pressed
                                    _mouseAction.StartTimeAutoResetTick = Stopwatch.GetTimestamp();
                                }

                                if (_mouseAction.WithAutoResetButtonPressed)
                                {
                                    _actionTimeElapsedInS = (float)(((Stopwatch.GetTimestamp() - _mouseAction.StartTimeAutoResetTick) / (float)Stopwatch.Frequency));
                                }

                                if ((_prevMouseState.XButton1 == ButtonState.Released && _curMouseState.XButton1 == ButtonState.Pressed) ||
                                    (_mouseAction.WithAutoResetButtonPressed && _curMouseState.XButton1 == ButtonState.Pressed && _actionTimeElapsedInS >= _mouseAction.AutoResetTimeInS)
                                    )
                                {
                                    _bufferedActionsInProgress[_mouseAction.ActionId].Triggered = true; 
                                    _bufferedActionsInProgress[_mouseAction.ActionId].RaisedSources |= ActionRaisedSources.Mouse;
                                    if (_prevMouseState.XButton1 == ButtonState.Pressed) _bufferedActionsInProgress[_mouseAction.ActionId].IsAutoRepeatedEvent = true;
                                    if (_mouseAction.WithAutoResetButtonPressed)
                                    {
                                            _mouseAction.StartTimeAutoResetTick = Stopwatch.GetTimestamp();
                                    }
                                }

                                if (_mouseAction.WithAutoResetButtonPressed && _curMouseState.XButton1 != ButtonState.Pressed)
                                {
                                    _mouseAction.StartTimeAutoResetTick = 0;
                                }
                                break;
                            case MouseButton.XButton2: 
                                if (_mouseAction.WithAutoResetButtonPressed && _mouseAction.StartTimeAutoResetTick == 0 && _curMouseState.XButton2 == ButtonState.Pressed)
                                {
                                    //Mouse Button DOWN and autoresetting its ButtonPressed value - Memorize when the time when the mouse has been pressed
                                    _mouseAction.StartTimeAutoResetTick = Stopwatch.GetTimestamp();
                                }

                                if (_mouseAction.WithAutoResetButtonPressed)
                                {
                                    _actionTimeElapsedInS = (float)(((Stopwatch.GetTimestamp() - _mouseAction.StartTimeAutoResetTick) / (float)Stopwatch.Frequency));
                                }

                                if ((_prevMouseState.XButton2 == ButtonState.Released && _curMouseState.XButton2 == ButtonState.Pressed) ||
                                    (_mouseAction.WithAutoResetButtonPressed && _curMouseState.XButton2 == ButtonState.Pressed && _actionTimeElapsedInS >= _mouseAction.AutoResetTimeInS)
                                    )
                                {
                                    _bufferedActionsInProgress[_mouseAction.ActionId].Triggered = true; 
                                    _bufferedActionsInProgress[_mouseAction.ActionId].RaisedSources |= ActionRaisedSources.Mouse;
                                    if (_prevMouseState.XButton2 == ButtonState.Pressed) _bufferedActionsInProgress[_mouseAction.ActionId].IsAutoRepeatedEvent = true;
                                    if (_mouseAction.WithAutoResetButtonPressed)
                                    {
                                            _mouseAction.StartTimeAutoResetTick = Stopwatch.GetTimestamp();
                                    }
                                }

                                if (_mouseAction.WithAutoResetButtonPressed && _curMouseState.XButton2 != ButtonState.Pressed)
                                {
                                    _mouseAction.StartTimeAutoResetTick = 0;
                                }
                                break;
                            case MouseButton.LeftAndRightButton:
                                
                                if (_mouseAction.WithAutoResetButtonPressed && _mouseAction.StartTimeAutoResetTick == 0 && _curMouseState.LeftButton == ButtonState.Pressed && _curMouseState.rightButton == ButtonState.Pressed)
                                {
                                    //Mouse Button DOWN and autoresetting its ButtonPressed value - Memorize when the time when the mouse has been pressed
                                    _mouseAction.StartTimeAutoResetTick = Stopwatch.GetTimestamp();
                                }

                                if (_mouseAction.WithAutoResetButtonPressed)
                                {
                                    _actionTimeElapsedInS = (float)(((Stopwatch.GetTimestamp() - _mouseAction.StartTimeAutoResetTick) / (float)Stopwatch.Frequency));
                                }

                                if ((_prevMouseState.leftButton == ButtonState.Released && _curMouseState.leftButton == ButtonState.Pressed && _prevMouseState.rightButton == ButtonState.Released && _curMouseState.rightButton == ButtonState.Pressed) ||
                                    (_mouseAction.WithAutoResetButtonPressed && _curMouseState.rightButton == ButtonState.Pressed && _curMouseState.leftButton == ButtonState.Pressed && _actionTimeElapsedInS >= _mouseAction.AutoResetTimeInS)
                                    )
                                {
                                    _bufferedActionsInProgress[_mouseAction.ActionId].Triggered = true; 
                                    _bufferedActionsInProgress[_mouseAction.ActionId].RaisedSources |= ActionRaisedSources.Mouse;
                                    if (_prevMouseState.leftButton == ButtonState.Pressed && _prevMouseState.rightButton == ButtonState.Pressed) _bufferedActionsInProgress[_mouseAction.ActionId].IsAutoRepeatedEvent = true;
                                    if (_mouseAction.WithAutoResetButtonPressed)
                                    {
                                            _mouseAction.StartTimeAutoResetTick = Stopwatch.GetTimestamp();
                                    }
                                }

                                if (_mouseAction.WithAutoResetButtonPressed && _curMouseState.leftButton != ButtonState.Pressed && _curMouseState.rightButton != ButtonState.Pressed)
                                {
                                    _mouseAction.StartTimeAutoResetTick = 0;
                                }

                                break;
                        }
                        break;
                    case MouseTriggerMode.ScrollWheelForward:
                        if (_curMouseState.ScrollWheelTicks > _prevMouseState.ScrollWheelTicks) { _bufferedActionsInProgress[_mouseAction.ActionId].Triggered = true; _bufferedActionsInProgress[_mouseAction.ActionId].RaisedSources |= ActionRaisedSources.Mouse; }
                        break;
                    case MouseTriggerMode.ScrollWheelBackWard:
                        if (_curMouseState.ScrollWheelTicks < _prevMouseState.ScrollWheelTicks) { _bufferedActionsInProgress[_mouseAction.ActionId].Triggered = true; _bufferedActionsInProgress[_mouseAction.ActionId].RaisedSources |= ActionRaisedSources.Mouse; }
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
                        { 
                            _bufferedActionsInProgress[_keyboardAction.ActionId].Triggered = true; 
                            _bufferedActionsInProgress[_keyboardAction.ActionId].RaisedSources |= ActionRaisedSources.Keyboard; 
                        }
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
                            _bufferedActionsInProgress[_keyboardAction.ActionId].RaisedSources |= ActionRaisedSources.Keyboard;
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
                        if (_keyboardAction.WithAutoResetButtonPressed && _keyboardAction.StartTimeAutoResetTick == 0 && _curKeyboardState.IsKeyDown(_keyboardAction.Binding))
                        {
                            //Mouse Button DOWN and autoresetting its ButtonPressed value - Memorize when the time when the mouse has been pressed
                            _keyboardAction.StartTimeAutoResetTick = Stopwatch.GetTimestamp();
                        }

                        if (_keyboardAction.WithAutoResetButtonPressed)
                        {
                            _actionTimeElapsedInS = (float)(((Stopwatch.GetTimestamp() - _keyboardAction.StartTimeAutoResetTick) / (float)Stopwatch.Frequency));
                        }

                        if ((_curKeyboardState.IsKeyDown(_keyboardAction.Binding) && _prevKeyboardState.IsKeyUp(_keyboardAction.Binding)) ||
                            (_keyboardAction.WithAutoResetButtonPressed && (_curKeyboardState.IsKeyDown(_keyboardAction.Binding) && _actionTimeElapsedInS >= _keyboardAction.AutoResetTimeInS))
                            )
                        {
                            _bufferedActionsInProgress[_keyboardAction.ActionId].Triggered = true;
                            _bufferedActionsInProgress[_keyboardAction.ActionId].RaisedSources |= ActionRaisedSources.Keyboard;
                            if (_prevKeyboardState.IsKeyDown(_keyboardAction.Binding)) _bufferedActionsInProgress[_keyboardAction.ActionId].IsAutoRepeatedEvent = true;
                            if (_keyboardAction.WithAutoResetButtonPressed)
                            {
                                _keyboardAction.StartTimeAutoResetTick = Stopwatch.GetTimestamp();
                            }
                        }

                        if (_keyboardAction.WithAutoResetButtonPressed && _curKeyboardState.IsKeyUp(_keyboardAction.Binding))
                        {
                            _keyboardAction.StartTimeAutoResetTick = 0;
                        }
                        break;
                }
            }
        }
        #endregion
    }

    public class ActionsManagerEventArgs : EventArgs
    {
        public KeyboardTriggeredAction Action { get; set; }
    }
}
