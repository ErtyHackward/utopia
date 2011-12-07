using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace S33M3Engines.D3D.DebugTools
{
    public class PerfMonitor : IDebugInfo
    {
        #region Private variable
        private string _debugInfo;
        private bool _isActivated;
        #endregion

        #region Public variable/properties
        public bool isActivated
        {
            get
            {
                return _isActivated;
            }
            set
            {
                PerfTimer.ResetMinMax();
                _isActivated = value;
            }
        }
        public PerfTimer PerfTimer { get; set; }
        #endregion

        public PerfMonitor()
        {
            PerfTimer = new PerfTimer();
        }
        
        #region Private methods
        #endregion

        #region Public methods
        public void StartMesure(IGameComponent gc, string Sufix)
        {
            PerfTimer.StartPerfMeasure(gc, Sufix);
        }

        public void StopMesure(IGameComponent gc, string Sufix)
        {
            PerfTimer.StopPerfMeasure(gc, Sufix);
        }

        #endregion

        public string GetInfo()
        {
            _debugInfo = "";
            if (!isActivated) return _debugInfo;
            //Create the string for formatting
            double totalTimeFrame =0;

            foreach (var data in PerfTimer.PerfTimerResults.Values.OrderByDescending(x => x.AvgInMS))
            {
                if (!data.PerfSamplingName.Contains("DebugInfo"))
                {
                    totalTimeFrame += data.AvgInMS;
                    _debugInfo += string.Format("{0,-40}, max : {1:0.000}, Avg : {2:0.000} \n", data.PerfSamplingName, data.MaxInMS, data.AvgInMS);
                }

            }

            return string.Format("Total frame time : {0:0.000} ms \n", totalTimeFrame) +  _debugInfo;
        }
    }
}
