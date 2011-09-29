using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;


namespace S33M3Engines.D3D
{
    public class GameTime
    {
        public readonly double ElapsedGameTimeInS_HD;
        public readonly float ElapsedGameTimeInS_LD;

        static long _frequency;
        public GameTime()
        {
            _frequency = Stopwatch.Frequency;
            ElapsedGameTimeInS_HD = (1.0 / _frequency) * Game.GameUpdateDelta; //Fixe amount of time elapsed !
            ElapsedGameTimeInS_LD = (float)(1.0 / _frequency) * Game.GameUpdateDelta; //Fixe amount of time elapsed !
        }
    }
}
