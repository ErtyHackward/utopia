using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utopia.Shared.Structs.Helpers
{
    public static class TimeSpanHelper
    {
        public static bool Parse(string time, out TimeSpan timeSpan)
        {
            var trimmed = time.TrimEnd().ToLower();

            var val = trimmed.Substring(0, trimmed.Length - 1);

            timeSpan = new TimeSpan();

            if (trimmed.EndsWith("d"))
            {
                int days;
                if (int.TryParse(val, out days))
                {
                    timeSpan = TimeSpan.FromDays(days);
                    return true;
                }
            } 
            else if (trimmed.EndsWith("h"))
            {
                int hours;
                if (int.TryParse(val, out hours))
                {
                    timeSpan = TimeSpan.FromHours(hours);
                    return true;
                }
            }
            else if (trimmed.EndsWith("m"))
            {
                int minutes;
                if (int.TryParse(val, out minutes))
                {
                    timeSpan = TimeSpan.FromMinutes(minutes);
                    return true;
                }
            }
            return false;

        }
    }
}
