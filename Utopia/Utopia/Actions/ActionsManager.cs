using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Utopia.Shared.Config;

namespace Utopia.Actions
{
    /// <summary>
    /// The aim of this class is to be able to pool the Inputs devices, and to react to them to see if an action is fired, or not.
    /// Everything will run in an separated thread, running as fast as possible to avoid missing a input event (Keypress, ...)
    /// The result will be "buffered" until requested.
    /// </summary>
    public class ActionsManager
    {
        #region Private variables
        private Thread _actionThread;
        private List<object> _keyboardActions;
        #endregion

        #region Public variables/properties
        public bool IsRunning { get; set; }
        #endregion

        public ActionsManager()
        {
            _keyboardActions = new List<object>();

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
        public void RegisterActions(enuActions action, enuKeyboardTriggerMode inputTriggerMode, KeyWithModifier bindingKey)
        {

        }
        #endregion

        #region Private methods
        private void ActionThreadedLoop()
        {
            while (IsRunning)
            {
                ProcessInputs();
            }
        }

        private void ProcessInputs()
        {

        }
        #endregion
    }
}
