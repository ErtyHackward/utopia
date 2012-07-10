using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amib.Threading;

namespace S33M3DXEngine.Threading
{
    public enum ThreadPriority
    {
        Low,
        Normal,
        High
    }

    public enum ThreadStatus
    {
        Idle,
        Locked
    }

    public interface IThreadStatus
    {
        ThreadStatus ThreadStatus { get; set; }
    }

    public static class SmartThread
    {
        public static SmartThreadPool ThreadPool;
        public static IWorkItemsGroup ThreadPoolSingleConcurrency;

        static int _totThread = 0;

        public static bool ThreadingActif { get; set; }

        public static void Initialize(int ThreadAllocatedModifier, bool forced)
        {
            SetOptimumNbrThread(ThreadAllocatedModifier, forced);
            ThreadingActif = true;
        }

        public static void CheckInit()
        {
            if (_totThread == 0) SetOptimumNbrThread(0, false);
        }

        private static bool _isBoostMode;

        public static bool IsBoostMode
        {
            get { return SmartThread._isBoostMode; }
            set
            {
                if (value == SmartThread._isBoostMode) return;
                if (value)
                {
                    ThreadPool.MaxThreads = _totThread + 4;
                }
                else
                {
                    ThreadPool.MaxThreads = _totThread;
                }
                SmartThread._isBoostMode = value;
            }
        }

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

                _totThread = 1 + coreCount + ThreadAllocatedModifier; //Remove the one use by the applicatino itself
            }
            else
            {
                _totThread = ThreadAllocatedModifier;
            }

            if (_totThread < 1) _totThread = 2;

            if (ThreadPool != null) ThreadPool.Dispose();
            STPStartInfo _stpInfo = new STPStartInfo() { MaxWorkerThreads = _totThread, MinWorkerThreads = _totThread, ThreadPriority = System.Threading.ThreadPriority.Lowest };
            ThreadPool = new SmartThreadPool(_stpInfo);
            ThreadPoolSingleConcurrency = ThreadPool.CreateWorkItemsGroup(1);

            ThreadingActif = true;

            return _totThread;
        }
    }
}
