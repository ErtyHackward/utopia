using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Engines.D3D;
using S33M3Engines.Struct;
using S33M3Engines.Shared.Math;
using S33M3Engines.D3D.DebugTools;

namespace Utopia.Worlds.GameClocks
{
    public abstract class Clock : IClock, IDebugInfo
    {
        #region Inner struct/Class
        //Handle several different possibility to display the time
        public struct VisualClockTime
        {
            public int Hours
            {
                get { return (int)(Seconds / 3600.0f); }
            }

            public int Minutes
            {
                get { return (int)((Seconds % 3600.0f) / 60); }
            }

            public int Seconds
            {
                get { return (int)(86400.0f / MathHelper.TwoPi * Time); }
            }

            /// <summary> Radian angle representing the Period of time inside a day. 0 = Midi, Pi = SunSleep, 2Pi = Midnight, 3/2Pi : Sunrise Morning </summary>
            public float Time { get; set; }

            /// <summary> Normalized representation of the hours [0 => 1], will go from 0 => 1 </summary>
            public float ClockTimeNormalized
            { get { return (Time / MathHelper.TwoPi); } }

            /// <summary> Normalized representation of the hours [0 => 1 => 0], will go from 0 => 1 => 0 => ... </summary>
            public float ClockTimeNormalized2
            {
                get
                {
                    if (Time <= MathHelper.Pi)
                    {
                        return MathHelper.FullLerp(0, 1, 0, MathHelper.Pi, Time);
                    }
                    else
                    {
                        return 1 - MathHelper.FullLerp(0, 1, MathHelper.Pi, MathHelper.TwoPi, Time);
                    }
                }
            }

            public override string ToString()
            {
                return Hours.ToString("00:") + Minutes.ToString("00") + Seconds.ToString("00");
            }
        }
        #endregion

        #region Private/Protected variable
        protected VisualClockTime _visualClockTime;
        
        // Radian angle representing the Period of time inside a day. 0 = Midi, Pi = SunSleep, 2Pi = Midnight, 3/2Pi : Sunrise Morning
        protected FTSValue<float> _clockTime = new FTSValue<float>();        
        #endregion

        #region Public properties/variables
        public VisualClockTime ClockTime
        {
            get { return _visualClockTime; }
            set { _visualClockTime = value; }
        }
        #endregion

        public Clock()
        {
        }

        #region Public methods
        //Allocate resources
        public virtual void Initialize()
        {
            ClockTime = new VisualClockTime();
        }

        //Dispose resources
        public virtual void Dispose()
        {
        }

        public virtual void Update(ref S33M3Engines.D3D.GameTime TimeSpend)
        {
        }

        public virtual void Interpolation(ref double interpolation_hd, ref float interpolation_ld)
        {
            float recomputedClock = _clockTime.Value;
            if (_clockTime.Value < _clockTime.ValuePrev)
            {
                recomputedClock = _clockTime.Value + MathHelper.TwoPi;
            }

            _clockTime.ValueInterp = MathHelper.Lerp(_clockTime.ValuePrev, recomputedClock, interpolation_ld);
            if (_clockTime.ValueInterp > Math.PI * 2)
            {
                //+1 Day
                _clockTime.ValueInterp = _clockTime.ValueInterp - (2 * (float)Math.PI);
            }
            if (_clockTime.ValueInterp < Math.PI * -2)
            {
                //-1 Day
                _clockTime.ValueInterp = _clockTime.ValueInterp + (2 * (float)Math.PI);
            }

            _visualClockTime.Time = _clockTime.ValueInterp;
        }

        #endregion

        public virtual string GetInfo()
        {
            return "<Clock Info> Current time : " + ClockTime.ToString() + " normalized : " + ClockTime.ClockTimeNormalized2 + " ; " + ClockTime.ClockTimeNormalized + " ; " + ClockTime;
        }
    }
}
