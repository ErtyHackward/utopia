using System;
using System.Text;
using ProtoBuf;
using Utopia.Shared.Services;

namespace Utopia.Shared.Structs
{
    [ProtoContract]
    public struct UtopiaTimeSpan
    {
        private const int SecondsPerMinute = 60;
        private const int SecondsPerHour = SecondsPerMinute * 60;
        private const int SecondsPerDay = SecondsPerHour * 24;

        /// <summary>
        /// Amount of seconds in this time span
        /// </summary>
        [ProtoMember(1)]
        public long TotalSeconds { get; set; }

        public double TotalMinutes
        {
            get { return (double)TotalSeconds / SecondsPerMinute; }
        }

        public double TotalHours
        {
            get { return (double)TotalSeconds / SecondsPerHour; }
        }

        public double TotalDays
        {
            get { return (double)TotalSeconds / SecondsPerDay; }
        }

        public double TotalSeasons
        {
            get { return TotalDays / UtopiaTime.TimeConfiguration.DaysPerSeason; }
        }

        public double TotalYears
        {
            get { return TotalDays / UtopiaTime.TimeConfiguration.DaysPerYear; }
        }

        public int Seconds
        {
            get { return (int)(TotalSeconds % 60); }
        }

        public int Minutes
        {
            get { return (int)TotalMinutes % 60; }
        }

        public int Hours
        {
            get { return (int)TotalHours % 24; }
        }

        public int Days
        {
            get { return (int)TotalDays % UtopiaTime.TimeConfiguration.DaysPerSeason; }
        }

        public int Seasons
        {
            get { return (int)TotalSeasons % UtopiaTime.TimeConfiguration.Seasons.Count; }
        }

        public int Years
        {
            get { return (int)TotalYears; }
        }

        public static UtopiaTimeSpan Zero {
            get { return new UtopiaTimeSpan();} 
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            if (Years > 0)
            {
                sb.AppendFormat("{0}y ", Years);
            }
            if (Seasons > 0)
            {
                sb.AppendFormat("{0}s ", Seasons);
            }
            if (Days > 0)
            {
                sb.AppendFormat("{0}d ", Days);
            }
            if (Hours != 0 || Minutes != 0)
            {
                sb.AppendFormat("{0:00}:{1:00}", Hours, Minutes);
            }

            return sb.ToString().Trim();
        }

        public static bool operator >(UtopiaTimeSpan t1, UtopiaTimeSpan t2)
        {
            return t1.TotalSeconds > t2.TotalSeconds;
        }

        public static bool operator <(UtopiaTimeSpan t1, UtopiaTimeSpan t2)
        {
            return t1.TotalSeconds < t2.TotalSeconds;
        }

        public static bool operator >=(UtopiaTimeSpan t1, UtopiaTimeSpan t2)
        {
            return t1.TotalSeconds >= t2.TotalSeconds;
        }

        public static bool operator <=(UtopiaTimeSpan t1, UtopiaTimeSpan t2)
        {
            return t1.TotalSeconds <= t2.TotalSeconds;
        }

        public static bool operator ==(UtopiaTimeSpan t1, UtopiaTimeSpan t2)
        {
            return t1.TotalSeconds == t2.TotalSeconds;
        }

        public static bool operator !=(UtopiaTimeSpan t1, UtopiaTimeSpan t2)
        {
            return !(t1 == t2);
        }

        public static UtopiaTimeSpan operator +(UtopiaTimeSpan t1, UtopiaTimeSpan t2)
        {
            return FromSeconds(t1.TotalSeconds + t2.TotalSeconds);
        }

        public static UtopiaTimeSpan operator -(UtopiaTimeSpan t1, UtopiaTimeSpan t2)
        {
            return FromSeconds(t1.TotalSeconds - t2.TotalSeconds);
        }

        public static UtopiaTimeSpan FromSeconds(double seconds)
        {
            return new UtopiaTimeSpan { TotalSeconds = (long)seconds };
        }

        public static UtopiaTimeSpan FromSeconds(long seconds)
        {
            return new UtopiaTimeSpan { TotalSeconds = seconds };
        }

        public static UtopiaTimeSpan FromMinutes(double minutes)
        {
            return new UtopiaTimeSpan { TotalSeconds = (long)(minutes * 60) };
        }

        public static UtopiaTimeSpan FromHours(double hours)
        {
            return new UtopiaTimeSpan { TotalSeconds = (long)(hours * 60 * 60) };
        }

        public static UtopiaTimeSpan FromDays(double days)
        {
            return new UtopiaTimeSpan { TotalSeconds = (long)(days * 24 * 60 * 60) };
        }

        public static UtopiaTimeSpan FromSeasons(double seasons)
        {
            return new UtopiaTimeSpan { TotalSeconds = (long)(seasons * UtopiaTime.TimeConfiguration.DaysPerSeason * 24 * 60 * 60) };
        }

        public static UtopiaTimeSpan FromYears(double years)
        {
            return new UtopiaTimeSpan { TotalSeconds = (long)(years * UtopiaTime.TimeConfiguration.DaysPerYear * 24 * 60 * 60) };
        }

        public bool Equals(UtopiaTimeSpan other)
        {
            return TotalSeconds == other.TotalSeconds;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is UtopiaTimeSpan && Equals((UtopiaTimeSpan)obj);
        }

        public override int GetHashCode()
        {
            return TotalSeconds.GetHashCode();
        }

        public static UtopiaTimeSpan Parse(string s)
        {
            if (!s.Contains(":") || s.Contains(" "))
            {
                var spl = s.ToLower().Split(new []{' '}, StringSplitOptions.RemoveEmptyEntries);

                var span = new UtopiaTimeSpan();

                foreach (var s1 in spl)
                {
                    var value = s1.Remove(s1.Length-1);
                    if (s1.EndsWith("y"))
                    {
                        int years;
                        if (int.TryParse(value, out years))
                            span += FromYears(years);
                    }
                    if (s1.EndsWith("s"))
                    {
                        int seasons;
                        if (int.TryParse(value, out seasons))
                            span += FromSeasons(seasons);
                    }
                    if (s1.EndsWith("d"))
                    {
                        int days;
                        if (int.TryParse(value, out days))
                            span += FromDays(days);
                    }
                    if (s1.EndsWith("h"))
                    {
                        int hours;
                        if (int.TryParse(value, out hours))
                            span += FromHours(hours);
                    }
                    if (s1.Contains(":"))
                    {
                        var ts = TimeSpan.Parse(s1);
                        span += FromSeconds(ts.TotalSeconds);
                    }
                }

                return span;
            }
            else
            {
                var ts = TimeSpan.Parse(s);
                return FromSeconds(ts.TotalSeconds);
            }
        }
    }
}