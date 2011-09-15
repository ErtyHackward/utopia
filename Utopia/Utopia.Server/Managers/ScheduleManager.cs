using System;
using System.Collections.Generic;
using Utopia.Server.Services;
using System.Threading;

namespace Utopia.Server.Managers
{
    /// <summary>
    /// Provides possibility to schedule tasks. Each task will be executed in separate thread
    /// </summary>
    public class ScheduleManager : IDisposable
    {
        private readonly object _syncRoot = new object();
        private readonly List<ScheduleTask> _tasks = new List<ScheduleTask>();
        private readonly Timer _timer;

        /// <summary>
        /// Gets or sets scheduler clock
        /// </summary>
        public Clock Clock { get; set; }

        /// <summary>
        /// Creates new instance of scheduler
        /// </summary>
        /// <param name="clock">Clock for scheduler</param>
        /// <param name="timerInterval">Timer interval (real time)</param>
        public ScheduleManager(Clock clock, int timerInterval = 100)
        {
            Clock = clock;
            _timer = new Timer(TimerCallback, null, 0, timerInterval);
        }

        private void TimerCallback(object obj)
        {
            lock (_syncRoot)
            {
                var currentTime = Clock.Now;

                for (int i = _tasks.Count - 1; i >= 0 ; i--)
                {
                    var scheduleTask = _tasks[i];
                    

                    if (scheduleTask.StartDateTime <= currentTime)
                    {
                        switch (scheduleTask.CallType)
                        {
                            case ScheduleCallType.Once:
                                if (scheduleTask.ExecuteAt < currentTime.TimeOfDay)
                                {
                                    scheduleTask.Call(currentTime);
                                    RemoveTask(scheduleTask);
                                }
                                break;
                            case ScheduleCallType.Periodic:
                                if(scheduleTask.LastExecuted + scheduleTask.CallInterval < currentTime)
                                    scheduleTask.Call(currentTime);
                                break;
                            case ScheduleCallType.SpecialTime:
                                if (scheduleTask.ExecuteAt < currentTime.TimeOfDay)
                                {
                                    scheduleTask.StartDateTime = currentTime - currentTime.TimeOfDay + TimeSpan.FromDays(1) + scheduleTask.ExecuteAt;
                                    scheduleTask.Call(currentTime);
                                }
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                        
                    }
                }
            }
        }

        /// <summary>
        /// Removes task from manager
        /// </summary>
        /// <param name="scheduleTask"></param>
        public void RemoveTask(ScheduleTask scheduleTask)
        {
            lock (_syncRoot)
            {
                _tasks.Remove(scheduleTask);
            }
        }

        /// <summary>
        /// Adds task that should be called just once
        /// </summary>
        /// <param name="name"></param>
        /// <param name="startAt"></param>
        /// <param name="callAt"></param>
        /// <param name="callDelegate"></param>
        public void AddTaskOnce(string name, DateTime startAt, TimeSpan callAt, ThreadStart callDelegate)
        {
            var task = new ScheduleTask{ Name = name, CallDelegate = callDelegate, CallType = ScheduleCallType.Once, ExecuteAt = callAt, StartDateTime = startAt };

            AddTask(task);
        }

        /// <summary>
        /// Adds task that should be called periodically
        /// </summary>
        /// <param name="name"></param>
        /// <param name="startAt"></param>
        /// <param name="interval">interval of execution</param>
        /// <param name="callDelegate"></param>
        public void AddTaskPeriodic(string name, DateTime startAt, TimeSpan interval, ThreadStart callDelegate)
        {
            var task = new ScheduleTask { Name = name, CallDelegate = callDelegate, ExecuteAt = interval, StartDateTime = startAt};

            AddTask(task);
        }

        /// <summary>
        /// Adds new task
        /// </summary>
        /// <param name="taskName"></param>
        /// <param name="callType"></param>
        /// <param name="callInterval"></param>
        /// <param name="callAt"></param>
        /// <param name="startTaskAt"></param>
        /// <param name="callDelegate"></param>
        public void AddTask(string taskName, ScheduleCallType callType, TimeSpan callInterval, TimeSpan callAt, DateTime startTaskAt, ThreadStart callDelegate)
        {
            var task = new ScheduleTask 
            { 
                CallDelegate = callDelegate, 
                CallInterval = callInterval, 
                CallType = callType, 
                ExecuteAt = callAt, 
                Name = taskName, 
                StartDateTime = startTaskAt 
            };

            AddTask(task);
        }

        /// <summary>
        /// Adds a new task to manager
        /// </summary>
        /// <param name="task"></param>
        public void AddTask(ScheduleTask task)
        {
            lock (_syncRoot)
            {
                _tasks.Add(task);
            }
        }

        /// <summary>
        /// Stops scheduling
        /// </summary>
        public void Dispose()
        {
            _timer.Dispose();
            _tasks.Clear();
        }
    }
}
