using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using S33M3DXEngine.Main;

namespace S33M3DXEngine.Main
{
    public class GameTime
    {
        private long _frequencyInSec;
        private long _frequencyInMiliSec;
        private long _lastUpdate;

        public readonly int FTSTargetedGameUpdatePerSecond; //Number of targeted update per seconds for fixed time step mode
        public readonly long GameUpdateDelta;
        public readonly double ElapsedGameTimeInS_HD;
        public readonly float ElapsedGameTimeInS_LD;
        public readonly double TickPerMS;

        public GameTime(int TargetedGameUpdatePerSecond = 40)
        {
            FTSTargetedGameUpdatePerSecond = TargetedGameUpdatePerSecond;
            //Nbr of tick per second
            _frequencyInSec = Stopwatch.Frequency;
            _frequencyInMiliSec = _frequencyInSec / 1000;
            TickPerMS = (double)_frequencyInSec / 1000.0;

            //Compute nbr of tick per update in fixed time step of FTSTargetedGameUpdatePerSecond
            GameUpdateDelta = Stopwatch.Frequency / FTSTargetedGameUpdatePerSecond;

            //Compute time elapsed (in  between 2 update in fixed time step situation)
            ElapsedGameTimeInS_HD = (1.0 / _frequencyInSec) * GameUpdateDelta;        //Fixe amount of time elapsed !
            ElapsedGameTimeInS_LD = (float)(1.0 / _frequencyInSec) * GameUpdateDelta; //Fixe amount of time elapsed !
        }

        /// <summary>
        /// Time passed in 1/1000 seconds
        /// </summary>
        /// <returns></returns>
        public long GetElapsedTime()
        {
            long elapsedTime = (Stopwatch.GetTimestamp() - _lastUpdate) / _frequencyInMiliSec;
            ResetElapsedTimeCounter();
            return elapsedTime;
        }

        public void ResetElapsedTimeCounter()
        {
            _lastUpdate = Stopwatch.GetTimestamp();
        }

        public double Tick2Ms(long ticks)
        {
            return (double)ticks / TickPerMS;
        }

    }
}
