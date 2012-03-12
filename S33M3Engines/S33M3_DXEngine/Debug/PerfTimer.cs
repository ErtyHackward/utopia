using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using S33M3_DXEngine.Main.Interfaces;

namespace S33M3_DXEngine.Debug
{
    public class PerfTimerResult
    {
        public static int Echantillon = 60;

        public string Name;
        public string Suffix;
        public string PerfSamplingName;

        public long BeginValue;

        public long MaxValue = long.MinValue;
        public long MinValue = long.MaxValue;

        public double MaxInMS
        {
            get
            {
                return (double)MaxValue / Stopwatch.Frequency * 1000;
            }
        }

        public double MinInMS
        {
            get
            {
                return (double)MinValue / Stopwatch.Frequency * 1000;
            }
        }

        public void ResetMinMax()
        {
            MaxValue = long.MinValue;
            MinValue = long.MaxValue;
        }

        public long[] Deltas = new long[PerfTimerResult.Echantillon];
        public short DeltaIndex = 0;

        public double AvgInMS
        {
            get { return (double)Deltas.Sum() / PerfTimerResult.Echantillon / Stopwatch.Frequency * 1000; }
        }

        public PerfTimerResult()
        {
        }
    }

    public class PerfTimer
    {
        public Dictionary<string, PerfTimerResult> PerfTimerResults = new Dictionary<string, PerfTimerResult>();

        PerfTimerResult p;

        public void StartPerfMeasure(string componentName, string Sufix)
        {
            if (!PerfTimerResults.TryGetValue(componentName + Sufix, out p))
            {
                p = new PerfTimerResult() { PerfSamplingName = componentName + " " + Sufix, Name = componentName, Suffix = Sufix };
                PerfTimerResults.Add(componentName + Sufix, p);
            }

            p.BeginValue = Stopwatch.GetTimestamp();
        }

        public void StartPerfMeasure(IGameComponent gameComponent, string Sufix, int indexId = -1)
        {
            string name = gameComponent.Name;
            if (indexId > -1)
            {
                string drawName = ((IDrawableComponent)gameComponent).DrawOrders.DrawOrdersCollection[indexId].Name;
                name = string.Concat(name, string.IsNullOrEmpty(drawName) ? drawName : " " + drawName);
            }

            StartPerfMeasure(name, Sufix);
        }

        public void ResetMinMax()
        {
            foreach (var result in PerfTimerResults.Values)
            {
                result.ResetMinMax();
            }
        }

        long deltaTime;
        public void StopPerfMeasure(string componentName, string Sufix)
        {

            if (!PerfTimerResults.TryGetValue(componentName + Sufix, out p)) return;

            deltaTime = Stopwatch.GetTimestamp() - p.BeginValue;

            p.Deltas[p.DeltaIndex] = deltaTime;
            p.DeltaIndex++;
            if (p.DeltaIndex >= PerfTimerResult.Echantillon) p.DeltaIndex = 0;
            if (p.MaxValue < deltaTime) p.MaxValue = deltaTime;
            if (p.MinValue > deltaTime) p.MinValue = deltaTime;
        }

        public void StopPerfMeasure(IGameComponent gameComponent, string Sufix, int indexId = -1)
        {
            string name = gameComponent.Name;
            if (indexId > -1)
            {
                string drawName = ((IDrawableComponent)gameComponent).DrawOrders.DrawOrdersCollection[indexId].Name;
                name = string.Concat(name, string.IsNullOrEmpty(drawName) ? drawName : " " + drawName);
            }

            StopPerfMeasure(name, Sufix);
        }
    }
}
