using System;
using System.Threading;

namespace Utopia.Server.Managers
{
    /// <summary>
    /// Represents a single scheduled task
    /// </summary>
    public class ScheduleTask
    {
        private readonly object _syncRoot = new object();
        private bool _taskIsRunning;

        /// <summary>
        /// Gets or sets task name for debug purposes
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets time of the day when task should be called
        /// </summary>
        public TimeSpan ExecuteAt { get; set; }

        /// <summary>
        /// Gets or sets interval between calls
        /// </summary>
        public TimeSpan CallInterval { get; set; }

        /// <summary>
        /// Gets or sets dateTime when task should be called first time
        /// </summary>
        public DateTime StartDateTime { get; set; }

        /// <summary>
        /// Task call type
        /// </summary>
        public ScheduleCallType CallType { get; set; }

        /// <summary>
        /// Delegate that should be called
        /// </summary>
        public ThreadStart CallDelegate { get; set; }

        /// <summary>
        /// Gets time of last execution
        /// </summary>
        public DateTime LastExecuted { get; set; }

        /// <summary>
        /// Performs task. Prevents task to be executed more than one simultaneously
        /// </summary>
        public void Call(DateTime now)
        {
            lock (_syncRoot)
            {
                if(_taskIsRunning)
                    return;

                _taskIsRunning = true;

                LastExecuted = now;

                CallDelegate.BeginInvoke(TaskIsDone, null);
            }
        }

        private void TaskIsDone(IAsyncResult result)
        {
            // allows task to be performed again
            _taskIsRunning = false;
        }
    }
}