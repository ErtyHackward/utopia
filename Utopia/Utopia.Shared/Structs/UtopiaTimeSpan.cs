using System;
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
            if (TotalYears > 0)
            {
                return string.Format("{0}y {1}d {2:00}:{3:00}", TotalYears, Days, Hours, Minutes);
            }
            if (TotalDays > 0)
            {
                return string.Format("{0}d {1:00}:{2:00}", TotalDays, Hours, Minutes);
            }
            
            return string.Format("{0:00}:{1:00}", Hours, Minutes);
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
            var ts = TimeSpan.Parse(s);
            return FromSeconds(ts.TotalSeconds);
        }
    }
}