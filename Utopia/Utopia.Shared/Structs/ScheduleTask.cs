using System;
using System.Threading;

namespace Utopia.Shared.Structs
{
    /// <summary>
    /// Represents a single scheduled task
    /// </summary>
    public class ScheduleTask
    {
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
        public Action CallDelegate { get; set; }

        /// <summary>
        /// Gets time of last execution
        /// </summary>
        public DateTime LastExecuted { get; set; }
        
        /// <summary>
        /// Performs task. Prevents task to be executed more than one simultaneously
        /// </summary>
        public void Call(DateTime now)
        {
            CallDelegate();

            LastExecuted = now;
        }
    }
}