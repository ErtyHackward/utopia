using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amib.Threading;

namespace S33M3_DXEngine.Threading
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

        public static void SetOptimumNbrThread(int ThreadAllocatedModifier, bool forced = false)
        {
            if (forced == false)
            {
                //Get number or real Core CPU (Not hyper threaded ones)
                int coreCount = 0;
                foreach (var item in new System.Management.ManagementObjectSearcher("Select * from Win32_Processor").Get())
                {
                    coreCount += int.Parse(item["NumberOfCores"].ToString());
                }

                _totThread = coreCount - 1 + ThreadAllocatedModifier; //Remove the one use by the applicatino itself
            }
            else
            {
                _totThread = ThreadAllocatedModifier;
            }

            if (_totThread < 1) _totThread = 1;

            STPStartInfo _stpInfo = new STPStartInfo() { MaxWorkerThreads = _totThread, MinWorkerThreads = _totThread, ThreadPriority = System.Threading.ThreadPriority.Lowest };
            ThreadPool = new SmartThreadPool(_stpInfo);

            ThreadingActif = true;
        }
    }
}
