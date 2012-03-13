using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using S33M3DXEngine.Main.Interfaces;
using S33M3DXEngine.Main;
using S33M3DXEngine.Debug.Interfaces;

namespace S33M3DXEngine.Debug
{
    public class PerfMonitor : GameComponent//, IDebugInfo
    {
        #region Private variable
        //private string _debugInfo;
        #endregion

        #region Public variable/properties
        public PerfTimer PerfTimer { get; set; }
        #endregion

        public PerfMonitor()
        {
            PerfTimer = new PerfTimer();
            this.UpdatableChanged += PerfMonitor_UpdatableChanged;
        }

        public override void Dispose()
        {
            this.UpdatableChanged -= PerfMonitor_UpdatableChanged;
 	        base.Dispose();
        }

        #region Private methods
        private void  PerfMonitor_UpdatableChanged(object sender, EventArgs e)
        {
            if (this.Updatable) PerfTimer.ResetMinMax();
        }
        #endregion

        #region Public methods
        public void StartMesure(IGameComponent gc, string Sufix, int indexId = -1)
        {
            PerfTimer.StartPerfMeasure(gc, Sufix, indexId);
        }

        public void StartMesure(string gcName, string Sufix)
        {
            PerfTimer.StartPerfMeasure(gcName, Sufix);
        }

        public void StopMesure(IGameComponent gc, string Sufix, int indexId = -1)
        {
            PerfTimer.StopPerfMeasure(gc, Sufix, indexId);
        }

        public void StopMesure(string gcName, string Sufix)
        {
            PerfTimer.StopPerfMeasure(gcName, Sufix);
        }

        #endregion

        

        //public void GetInfo()
        //{
        //    //_debugInfo = "";
        //    //if (!isActivated) return _debugInfo;
        //    ////Create the string for formatting
        //    //double totalTimeFrame = 0;

        //    //foreach (var data in PerfTimer.PerfTimerResults.Values.OrderByDescending(x => x.AvgInMS))
        //    //{
        //    //    if (!data.PerfSamplingName.Contains("DebugInfo"))
        //    //    {
        //    //        totalTimeFrame += data.AvgInMS;
        //    //        _debugInfo += string.Format("{0,-40}, max : {1:0.000}, Avg : {2:0.000} \n", data.PerfSamplingName, data.MaxInMS, data.AvgInMS);
        //    //    }
        //    //}
        //    //return string.Format("Total frame time : {0:0.000} ms \n", totalTimeFrame) + _debugInfo;
        //}

        //public bool ShowDebugInfo { get; set; }

        //public string GetDebugInfo()
        //{
        //    if (ShowDebugInfo) return null;
        //    return _debugInfo;
        //}
    }
}
