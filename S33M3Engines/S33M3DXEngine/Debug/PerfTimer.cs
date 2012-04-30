using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using S33M3DXEngine.Main.Interfaces;

namespace S33M3DXEngine.Debug
{
    public class CompDeltaPerfResult
    {
        public string Name;
        public long DeltaValue;
        public double PourcVariation;
        public long LastValue;
        public long PrevValue;
        public double DeltaSum;
        public double WeightedResult
        {
            get { return (PourcVariation * DeltaValue) / DeltaSum; }
        }
    }

    public class PerfTimerResult
    {
        public static int Echantillon = 100;

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

        public long GetLastValue
        {
            get { return Deltas[PreviousDeltaIndex]; }
        }

        public long GetPrevValue
        {
            get { return Deltas[Previous2DeltaIndex]; }
        }

        public double Delta
        {
            get { return Deltas.Max() - Deltas.Min(); }
        }

        public double DeltaAgainstAvg
        {
            get { return (Delta * 100.0) / (Deltas.Sum() / PerfTimerResult.Echantillon); }
        }

        public void ResetMinMax()
        {
            MaxValue = long.MinValue;
            MinValue = long.MaxValue;
        }

        public long[] Deltas = new long[PerfTimerResult.Echantillon];
        public short DeltaIndex = 0;
        public short PreviousDeltaIndex = 0;
        public short Previous2DeltaIndex = 0;

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

        public long GetLastUpdateTime
        {
            get
            {
                long val = 0;
                foreach (var result in PerfTimerResults.Values.Where(x => x.PerfSamplingName.Contains("Update")))
                {
                    val += result.GetLastValue;
                }
                return val;
            }
        }

        public long GetPrevUpdateTime
        {
            get
            {
                long val = 0;
                foreach (var result in PerfTimerResults.Values.Where(x => x.PerfSamplingName.Contains("Update")))
                {
                    val += result.GetPrevValue;
                }
                return val;
            }
        }

        public long GetLastDrawTime
        {
            get
            {
                long val = 0;
                foreach (var result in PerfTimerResults.Values.Where(x => x.PerfSamplingName.Contains("Draw")).Where(y => y.Name.Contains("GPU Rendering") == false))
                {
                    val += result.GetLastValue;
                }
                return val;
            }
        }

        public long GetPrevDrawTime
        {
            get
            {
                long val = 0;
                foreach (var result in PerfTimerResults.Values.Where(x => x.PerfSamplingName.Contains("Draw")).Where(y => y.Name.Contains("GPU Rendering") == false))
                {
                    val += result.GetPrevValue;
                }
                return val;
            }
        }

        public long GetLastPresentTime
        {
            get
            {
                long val = 0;
                foreach (var result in PerfTimerResults.Values.Where(x => x.Name.Contains("GPU Rendering")))
                {
                    val += result.GetLastValue;
                }
                return val;
            }
        }

        public long GetPrevPresentTime
        {
            get
            {
                long val = 0;
                foreach (var result in PerfTimerResults.Values.Where(x => x.Name.Contains("GPU Rendering")))
                {
                    val += result.GetPrevValue;
                }
                return val;
            }
        }

        public IEnumerable<CompDeltaPerfResult> GetComponentByDeltaPerf(int topXValues)
        {
            List<CompDeltaPerfResult> result = new List<CompDeltaPerfResult>();
            //Compute the Delta time from prev to last duraction of each components
            foreach (var comp in PerfTimerResults.Values)
            {
                result.Add(new CompDeltaPerfResult()
                {
                    Name = comp.Name + " " + comp.Suffix,
                    DeltaValue = comp.GetLastValue - comp.Previous2DeltaIndex, //will be > if last value take more time than previous                    
                    PourcVariation = comp.GetLastValue != 0 ? (double)((comp.GetLastValue - comp.GetPrevValue) / (double)comp.GetLastValue) : 1,
                    LastValue = comp.GetLastValue,
                    PrevValue = comp.GetPrevValue
                });
            }

            double SumDeltaValue = result.Sum(x => x.DeltaValue);

            foreach (var comp in result) { comp.DeltaSum = SumDeltaValue; }

            foreach (var comp in result.OrderByDescending(x => x.WeightedResult))
            {
                yield return comp;
                topXValues--;
                if (topXValues < 0) break;
            }
        }

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
            p.Previous2DeltaIndex = p.PreviousDeltaIndex;
            p.PreviousDeltaIndex = p.DeltaIndex; 
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
