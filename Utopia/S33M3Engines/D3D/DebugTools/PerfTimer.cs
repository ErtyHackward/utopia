using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace S33M3Engines.D3D.DebugTools
{
    public class PerfTimerResult
    {
        public string PerfSamplingName;

        public long BeginValue;

        public long MaxValue = long.MinValue;
        public long MinValue = long.MaxValue;

        public long[] Deltas = new long[100];
        public short DeltaIndex = 0;

        public double AvgInSeconds
        {
            get { return (double)Deltas.Sum() / 100 / Stopwatch.Frequency; }
        }

        public PerfTimerResult()
        {
        }
    }

    public static class PerfTimer
    {
        public static List<PerfTimerResult> PerfTimerResults = new List<PerfTimerResult>();

        public static bool isPerfMeasureActif = false;

        static PerfTimerResult p;
        public static void StartPerfMeasure(int PerfCounterId, string Name)
        {
            if (!isPerfMeasureActif) return;
            if(PerfTimerResults.Count <= PerfCounterId) PerfTimerResults.Add( new PerfTimerResult() { PerfSamplingName = Name });
            p = PerfTimerResults[PerfCounterId];
            p.BeginValue = Stopwatch.GetTimestamp();
        }

        static long deltaTime;
        public static void StopPerfMeasure(int PerfCounterId)
        {
            if (!isPerfMeasureActif || PerfTimerResults.Count <= PerfCounterId) return;
            p = PerfTimerResults[PerfCounterId];

            deltaTime = Stopwatch.GetTimestamp() - PerfTimerResults[PerfCounterId].BeginValue;

            p.Deltas[p.DeltaIndex] = deltaTime;
            p.DeltaIndex++;
            if (p.DeltaIndex >= 100) p.DeltaIndex = 0;
            if (p.MaxValue < deltaTime) p.MaxValue = deltaTime;
            if (p.MinValue > deltaTime) p.MinValue = deltaTime;
        }
    }
}
