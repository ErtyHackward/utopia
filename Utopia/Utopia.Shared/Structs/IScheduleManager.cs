using System;

namespace Utopia.Shared.Structs
{
    public interface IScheduleManager
    {
        /// <summary>
        /// Removes task from manager
        /// </summary>
        /// <param name="scheduleTask"></param>
        void RemoveTask(ScheduleTask scheduleTask);

        /// <summary>
        /// Adds a new task to manager
        /// </summary>
        /// <param name="task"></param>
        void AddTask(ScheduleTask task);

        /// <summary>
        /// Adds a periodic task that will be started after the interval
        /// </summary>
        /// <param name="interval"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        ScheduleTask AddPeriodic(TimeSpan interval, Action action);
    }
}