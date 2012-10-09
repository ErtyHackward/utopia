using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Schedulers;

namespace S33M3DXEngine.Threading
{
    /// <summary>
    /// Attempt to use TPL to manage threading in Utopia !
    /// </summary>
    public static class ThreadsManager
    {
        #region Private Variables
        private static QueuedTaskScheduler _monoConcurrencyTaskSheduler;
        private static QueuedTaskScheduler _mainTaskSheduler;
        private static TaskScheduler _lowPrioritySchedduler;
        private static TaskScheduler _normalPrioritySchedduler;
        private static TaskScheduler _highPrioritySchedduler;
        private static int _totThread;
        private static bool _isBoostMode;
        #endregion

        #region Public Properties
        public static int RunningThreads { get { return _mainTaskSheduler.RunningThreads; } }
        public static int TaskQueueSize { get { return _mainTaskSheduler.TaskCount; } }

        public static bool IsBoostMode
        {
            get { return ThreadsManager._isBoostMode; }
            set
            {
                if (value == ThreadsManager._isBoostMode) return;
                ThreadsManager._isBoostMode = value;
                CreateTaskScheduler();
            }
        }
        #endregion

        #region Public Methods
        //Get the qt of Thread to be use in the pool
        public static int SetOptimumNbrThread(int ThreadAllocatedModifier, bool forced = false)
        {
            if (forced == false)
            {
                //Get number or real Core CPU (Not hyper threaded ones)
                int coreCount = 0;
                foreach (var item in new System.Management.ManagementObjectSearcher("Select * from Win32_Processor").Get())
                {
                    coreCount += int.Parse(item["NumberOfCores"].ToString());
                }

                _totThread = 1 + coreCount + ThreadAllocatedModifier; //Remove the one use by the application itself
            }
            else
            {
                _totThread = ThreadAllocatedModifier;
            }

            if (_totThread < 1) _totThread = 2;
            CreateTaskScheduler();
            return _totThread;
        }

        public static void CleanUp()
        {
            if (_mainTaskSheduler != null) _mainTaskSheduler.Dispose();
            if (_monoConcurrencyTaskSheduler != null) _monoConcurrencyTaskSheduler.Dispose();
        }

        public static void CheckInit()
        {
            if (_totThread == 0) SetOptimumNbrThread(0, false);
        }


        public static Task RunAsync(Action action, 
                                ThreadTaskPriority priority = ThreadTaskPriority.Normal,
                                bool singleConcurrencyRun = false)
        {
            return Task.Factory.StartNew(action, CancellationToken.None, TaskCreationOptions.None, getScheduler(priority, singleConcurrencyRun));
        }

        public static Task RunAsync(Action action,
                             Action<Task> CallBack,
                             ThreadTaskPriority priority = ThreadTaskPriority.Normal,
                             bool singleConcurrencyRun = false)
        {
            return Task.Factory.StartNew(action, CancellationToken.None, TaskCreationOptions.None, getScheduler(priority, singleConcurrencyRun)).ContinueWith(CallBack);
        }

        public static Task<TResult> RunAsync<TResult>(Func<TResult> func,
                                Action<Task<TResult>> CallBack = null,
                                ThreadTaskPriority priority = ThreadTaskPriority.Normal,
                                bool singleConcurrencyRun = false)
        {
            Task<TResult> t = Task.Factory.StartNew(func, CancellationToken.None, TaskCreationOptions.None, getScheduler(priority, singleConcurrencyRun));
            if(CallBack != null) t.ContinueWith(CallBack);
            return t;
        }

        #endregion

        #region Private Methods
        private static TaskScheduler getScheduler(ThreadTaskPriority priority, bool singleConcurrencyRun)
        {
            if (singleConcurrencyRun) return _monoConcurrencyTaskSheduler;
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

        private static void CreateTaskScheduler()
        {
            if (_mainTaskSheduler != null) _mainTaskSheduler.Dispose();
            if (_monoConcurrencyTaskSheduler == null)
            {
                _monoConcurrencyTaskSheduler = new QueuedTaskScheduler(TaskScheduler.Default, 1);
            }
            //We will never start more thread than the number of Virutal processor on the computer !
            int nbrThreads = ThreadsManager._isBoostMode ? _totThread + 4 : _totThread;
            _mainTaskSheduler = new QueuedTaskScheduler(TaskScheduler.Default, nbrThreads);
            //Create the Priority queues
            _highPrioritySchedduler = _mainTaskSheduler.ActivateNewQueue(0);
            _normalPrioritySchedduler = _mainTaskSheduler.ActivateNewQueue(1);
            _lowPrioritySchedduler = _mainTaskSheduler.ActivateNewQueue(2);
        }
        #endregion

        public enum ThreadTaskPriority : byte
        {
            High = 0,
            Normal = 1,
            Low = 2
        }

        public enum ThreadStatus
        {
            Idle,
            Locked
        }

    }
}
