using S33M3CoreComponents.Maths;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.Structs;

namespace Utopia.Worlds.GameClocks
{
    public struct VisualClockTime
    {
        public int Hours
        {
            get { return (int)(UtopiaTime.SecondsPerDay * ClockTimeNormalized / 3600.0f); }
        }

        public int Minutes
        {
            get { return (int)((UtopiaTime.SecondsPerDay * ClockTimeNormalized % 3600.0f) / 60.0f); }
        }

        public int Seconds
        {
            get { return (int)(UtopiaTime.SecondsPerDay * ClockTimeNormalized % 60.0f); }
        }

        /// <summary> Normalized Period of the day inside a day </summary>
        public float ClockTimeNormalized { get; set; }

        /// <summary> Normalized representation of the hours [0 => 1 => 0], will go from 0 => 1 => 0 => ... </summary>
        public float ClockTimeNormalized2
        {
            get
            {
                if (ClockTimeNormalized <= 0.5)
                {
                    return MathHelper.FullLerp(0, 1, 0, 0.5, ClockTimeNormalized);
                }
                else
                {
                    return 1 - MathHelper.FullLerp(0, 1, 0.5, 1, ClockTimeNormalized);
                }
            }
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

        public override string ToString()
        {
            return Hours.ToString("00:") + Minutes.ToString("00:") + Seconds.ToString("00");
        }
    }
}
