using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3DXEngine.Main;
using S33M3CoreComponents.Maths;
using S33M3Resources.Structs;

namespace Utopia.Worlds.GameClocks
{
    public abstract class Clock : GameComponent, IClock
    {
        #region Inner struct/Class
        //Handle several different possibility to display the time
        public struct VisualClockTime
        {
            public int Hours
            {
                get { return (int)(86400.0f / MathHelper.TwoPi * Time / 3600.0f); }
            }

            public int Minutes
            {
                get { return (int)((86400.0f / MathHelper.TwoPi * Time % 3600.0f) / 60); }
            }

            public int Seconds
            {
                get { return (int)(86400.0f / MathHelper.TwoPi * Time % 60.0f); }
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
                return Hours.ToString("00:") + Minutes.ToString("00:") + Seconds.ToString("00");
            }

            /// <summary>
            /// Will providea value between 0 and 1 with "steps" => Fixe low value during night, then lerping during day to a max, ...
            /// </summary>
            /// <returns></returns>
            public float SmartTimeInterpolation(float min = 0.05f, float max = 1)
            {
                float interpolationValue;
                if (ClockTimeNormalized <= 0.2083944 || ClockTimeNormalized > 0.9583824) // Between 23h00 and 05h00 => Dark night
                {
                    interpolationValue = min;
                }
                else
                {
                    if (ClockTimeNormalized > 0.2083944 && ClockTimeNormalized <= 0.4166951) // Between 05h00 and 10h00 => Go to Full Day
                    {
                        interpolationValue = MathHelper.FullLerp(min, max, 0.2083944, 0.4166951, ClockTimeNormalized);
                    }
                    else
                    {
                        if (ClockTimeNormalized > 0.4166951 && ClockTimeNormalized <= 0.6666929) // Between 10h00 and 16h00 => Full Day
                        {
                            interpolationValue = max;
                        }
                        else
                        {
                            interpolationValue = MathHelper.FullLerp(1, min, 0.6666929, 0.9583824, ClockTimeNormalized); //Go to Full night
                        }
                    }
                }

                return interpolationValue;
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

        #region Public methods
        //Allocate resources
        public override void Initialize()
        {
            ClockTime = new VisualClockTime();
        }

        //Dispose resources
        public override void FTSUpdate(GameTime timeSpend)
        {
        }

        public override void VTSUpdate(double interpolationHd, float interpolationLd, float elapsedTime)
        {
            float recomputedClock = _clockTime.Value;
            if (_clockTime.Value < _clockTime.ValuePrev)
            {
                recomputedClock = _clockTime.Value + MathHelper.TwoPi;
            }

            _clockTime.ValueInterp = MathHelper.Lerp(_clockTime.ValuePrev, recomputedClock, interpolationLd);
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

        public virtual bool ShowDebugInfo { get; set; }
        public virtual string GetDebugInfo()
        {
            return "<Clock Info> Current time : " + ClockTime.ToString(); // +" normalized : " + ClockTime.ClockTimeNormalized2 + " ; " + ClockTime.ClockTimeNormalized + " ; " + ClockTime;
        }
    }
}
