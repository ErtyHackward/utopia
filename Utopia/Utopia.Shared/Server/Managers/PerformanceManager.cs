using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Utopia.Shared.Server.Managers
{
    /// <summary>
    /// Provides runtime perfomance measurements
    /// </summary>
    public class PerformanceManager
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly Queue<double> _updateCyclesPerfomance = new Queue<double>();
        private readonly Stopwatch _updateStopwatch = new Stopwatch();
        private readonly DateTime _dateStart;
        private readonly PerformanceCounter _cpuCounter;
        private readonly PerformanceCounter _ramCounter;

        /// <summary>
        /// Gets total up time of the server
        /// </summary>
        public TimeSpan UpTime
        {
            get { return (DateTime.Now - _dateStart); }
        }

        /// <summary>
        /// Gets average dynamic entities update time [ms]
        /// </summary>
        public double UpdateAverageTime
        {
            get { return Math.Round(_updateCyclesPerfomance.Average(), 2); }
        }

        /// <summary>
        /// Gets CPU usage %
        /// </summary>
        public float CpuUsage
        {
            get { return _cpuCounter.NextValue(); }
        }

        /// <summary>
        /// Gets free RAM amount [Mb]
        /// </summary>
        public float FreeRAM
        {
            get { return _ramCounter.NextValue(); }
        }

        [DebuggerStepThrough]
        public PerformanceManager(AreaManager areaManager)
        {
            _dateStart = DateTime.Now;

            _cpuCounter = new PerformanceCounter { CategoryName = "Processor", CounterName = "% Processor Time", InstanceName = "_Total" };
            try
            {
                _ramCounter = new PerformanceCounter("Memory", "Available MBytes");
            }
            catch (InvalidOperationException e)
            {
                logger.Error("Exception {0}", e.Message);
            }

            areaManager.BeforeUpdate += AreaManagerBeforeUpdate;
            areaManager.AfterUpdate += AreaManagerAfterUpdate;
        }

        private void AreaManagerBeforeUpdate(object sender, EventArgs e)
        {
            _updateStopwatch.Restart();
        }

        private void AreaManagerAfterUpdate(object sender, EventArgs e)
        {
            _updateStopwatch.Stop();
            _updateCyclesPerfomance.Enqueue(_updateStopwatch.Elapsed.TotalMilliseconds);
            if (_updateCyclesPerfomance.Count > 10)
                _updateCyclesPerfomance.Dequeue();
        }
    }
}
