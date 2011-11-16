using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amib.Threading;

namespace S33M3Engines.Threading
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

    public static class WorkQueue
    {
        public static SmartThreadPool ThreadPool;
        public static IWorkItemsGroup ThreadPoolGrp;
       
        static int _totThread;

        public static bool ThreadingActif { get; set; }

        public static void Initialize(int ThreadAllocatedModifier)
        {
            SetOptimumNbrThread(ThreadAllocatedModifier);
            ThreadingActif = true;
        }

        public static void SetOptimumNbrThread(int ThreadAllocatedModifier)
        {
            //Get number or real Core CPU (Not hyper threaded ones)
            int coreCount = 0;
            foreach (var item in new System.Management.ManagementObjectSearcher("Select * from Win32_Processor").Get())
            {
                coreCount += int.Parse(item["NumberOfCores"].ToString());
            }

            _totThread = coreCount - 1 + ThreadAllocatedModifier; //Remove the one use by the applicatino itself
            if (_totThread < 1) _totThread = 1;

            STPStartInfo _stpInfo = new STPStartInfo() { MaxWorkerThreads = _totThread, MinWorkerThreads = _totThread, ThreadPriority = System.Threading.ThreadPriority.Lowest };
            ThreadPool = new SmartThreadPool(_stpInfo);

            ThreadPoolGrp = ThreadPool.CreateWorkItemsGroup(1);
        }

        public static void DoWorkInThread(WorkItemCallback work, IThreadStatus ThreadedObject, bool executeInGroup = false)
        {
            DoWorkInThread(work, null, ThreadedObject, WorkItemPriority.Normal, executeInGroup);
        }

        public static void DoWorkInThread(WorkItemCallback work, object param, IThreadStatus ThreadedObject, bool executeInGroup = false)
        {
            DoWorkInThread(work, param, ThreadedObject, WorkItemPriority.Normal, executeInGroup);
        }

        public static void DoWorkInThreadedGroup(Amib.Threading.Action work)
        {
            ThreadPoolGrp.QueueWorkItem(work);
        }

        public static void DoWorkInThread(WorkItemCallback work, object param, IThreadStatus ThreadedObject, WorkItemPriority priority, bool executeInGroup = false, PostExecuteWorkItemCallback callBack = null)
        {
            if (ThreadPool.IsShuttingdown) return;
            ThreadedObject.ThreadStatus = ThreadStatus.Locked;
            if (callBack == null)
            {
                if (executeInGroup) ThreadPoolGrp.QueueWorkItem(work, param, priority);
                else ThreadPool.QueueWorkItem(work, param, priority);
            }
            else
            {
                if (executeInGroup) ThreadPoolGrp.QueueWorkItem(work, param, callBack, priority);
                else ThreadPool.QueueWorkItem(work, param, callBack, priority);
            }
        }
    }
}
