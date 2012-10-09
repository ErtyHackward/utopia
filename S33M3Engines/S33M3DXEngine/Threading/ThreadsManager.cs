using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Schedulers;

namespace S33M3_DXEngine.Threading
{
    /// <summary>
    /// Attempt to use TPL to manage threading in Utopia !
    /// </summary>
    public static class ThreadsManager
    {
        #region Private Variables
        private static QueuedTaskScheduler _mainTaskSheduler;
        private static TaskScheduler _lowPrioritySchedduler;
        private static TaskScheduler _normalPrioritySchedduler;
        private static TaskScheduler _highPrioritySchedduler;
        #endregion

        #region Public Properties
        public static int RunningThreads { get { return _mainTaskSheduler.RunningThreads; } }
        public static int TaskQueueSize { get { return _mainTaskSheduler.TaskCount; } }
        #endregion

        static ThreadsManager()
        {
            Initialize();
        }

        #region Public Methods
        public static void RunAsync(Action action,
                             ThreadTaskPriority priority = ThreadTaskPriority.Normal)
        {
            Task.Factory.StartNew(action, CancellationToken.None, TaskCreationOptions.None, getScheduler(priority));
        }

        public static void RunAsync(Action action,
                             Action<Task> CallBack,
                             ThreadTaskPriority priority = ThreadTaskPriority.Normal)
        {
            Task.Factory.StartNew(action, CancellationToken.None, TaskCreationOptions.None, getScheduler(priority)).ContinueWith(CallBack);
        }

        public static void RunAsync<T>(Func<T> func,
                                Action<Task<T>> CallBack,
                                ThreadTaskPriority priority = ThreadTaskPriority.Normal)
        {
            Task.Factory.StartNew(func, CancellationToken.None, TaskCreationOptions.None, getScheduler(priority)).ContinueWith(CallBack);
        }

        #endregion

        #region Private Methods
        private static void Initialize()
        {
            //We will never start more thread than the number of Virutal processor on the computer !
            _mainTaskSheduler = new QueuedTaskScheduler(TaskScheduler.Default, 0);
            //Create the Priority queues
            _highPrioritySchedduler = _mainTaskSheduler.ActivateNewQueue(0);
            _normalPrioritySchedduler = _mainTaskSheduler.ActivateNewQueue(1);
            _lowPrioritySchedduler = _mainTaskSheduler.ActivateNewQueue(2);

        }

        private static TaskScheduler getScheduler(ThreadTaskPriority priority)
        {
            switch (priority)
            {
                case ThreadTaskPriority.High:
                    return _highPrioritySchedduler;
                case ThreadTaskPriority.Normal:
                    return _normalPrioritySchedduler;
                case ThreadTaskPriority.Low:
                    return _lowPrioritySchedduler;
                default:
                    return _normalPrioritySchedduler;
            }
        }
        #endregion

        public enum ThreadTaskPriority : byte
        {
            High = 0,
            Normal = 1,
            Low = 2
        }


    }
}
