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

        public static Stopwatch ElapsedTime = new Stopwatch();

        public readonly int FTSTargetedGameUpdatePerSecond; //Number of targeted update per seconds for fixed time step mode
        public readonly long GameUpdateDelta;
        public readonly double ElapsedGameTimeInS_HD;
        public readonly float ElapsedGameTimeInS_LD;
        public readonly double TickPerMS;
        public readonly long FTSSafeGuard = Stopwatch.Frequency * 10;

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

            //Start an elapsed time stopWatch for the application
            ElapsedTime.Start();
        }

        /// <summary>
        /// Time passed in seconds
        /// </summary>
        /// <returns></returns>
        public float GetElapsedTime()
        {
            float elapsedTime = (Stopwatch.GetTimestamp() - _lastUpdate) / (float)_frequencyInSec;
            ResetElapsedTimeCounter();
            return elapsedTime;
        }

        public float QueryElapsedTime()
        {
            float elapsedTime = (Stopwatch.GetTimestamp() - _lastUpdate) / (float)_frequencyInSec;
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
