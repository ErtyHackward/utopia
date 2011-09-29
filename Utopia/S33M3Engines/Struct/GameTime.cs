using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;


namespace S33M3Engines.D3D
{
    public class GameTime
    {
        public double ElapsedGameTimeInS_HD;
        public float ElapsedGameTimeInS_LD;

        static long _frequency;
        long count = 0;

        public void Update()
        {
            ElapsedGameTimeInS_HD = (1.0 / _frequency) * Game.GameUpdateDelta; //Fixe amount of time elapsed !
            ElapsedGameTimeInS_LD = (float)ElapsedGameTimeInS_HD;
        }

        public GameTime()
        {
            _frequency = Stopwatch.Frequency;
        }
    }
}
