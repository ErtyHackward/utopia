using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Utopia.Shared.Config;
using S33M3Engines.InputHandler.KeyboardHelper;
using S33M3Engines.InputHandler;

namespace Utopia.Actions
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

        private Thread _actionThread;
        private List<KeyboardTriggeredAction> _keyboardActions;
        private bool _isAction1Exposed;
        private bool[] _bufferedActions1;
        private bool[] _bufferedActions2;
        private bool[] _bufferedActionsInProgress;
        private bool[] _actions;        
        #endregion

        #region Public variables/properties
        public bool IsRunning { get; set; }
        #endregion

        public ActionsManager()
        {
            _keyboardActions = new List<KeyboardTriggeredAction>();

            _bufferedActions1 = new bool[Enum.GetValues(typeof(enuActions)).Length];
            _bufferedActions2 = new bool[_bufferedActions1.Length];
            _actions = _bufferedActions1;
            _bufferedActionsInProgress = _bufferedActions2;
            _isAction1Exposed = true;

            IsRunning = true;
            _actionThread = new Thread(ActionThreadedLoop);
            _actionThread.Start();
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
        /// Is an action Triggered !
        /// </summary>
        /// <param name="action">The action to look at</param>
        /// <returns></returns>
        public bool isTriggered(enuActions action)
        {
            return _actions[(int)action];
        }

        public void Dispose()
        {
            IsRunning = false;
        }
        #endregion

        #region Private methods
        private void ActionThreadedLoop()
        {
            while (IsRunning)
            {
                Thread.Sleep(10);
                ProcessInputs();
            }
        }

        private void ProcessInputs()
        {
            ProcessKeyboardStates();
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
                    case enuKeyboardTriggerMode.KeyDown:
                        //Set the Action Flag if required
                        if (_curKeyboardState.IsKeyDown(_keyboardAction.Binding))
                            _bufferedActionsInProgress[(int)_keyboardAction.Action] = true;
                        break;
                    case enuKeyboardTriggerMode.KeyDownUp:
                        //Set the Action Flag if required
                        if (_prevKeyboardState.IsKeyDown(_keyboardAction.Binding) && _curKeyboardState.IsKeyUp(_keyboardAction.Binding))
                            _bufferedActionsInProgress[(int)_keyboardAction.Action] = true;
                        break;
                }
            }
        }
        #endregion


    }
}
