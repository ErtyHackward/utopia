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
        /// Amount of seconds passed from the begginning of the world
        /// </summary>
        [ProtoMember(1)]
        public long TotalSeconds { get; set; }

        public int TotalMinutes
        {
            get { return (int)(TotalSeconds / SecondsPerMinute); }
        }

        public int TotalHours
        {
            get { return (int)(TotalSeconds / SecondsPerHour); }
        }

        public int TotalDays
        {
            get { return (int)(TotalSeconds / SecondsPerDay); }
        }

        public int TotalSeasons
        {
            get { return TotalDays / UtopiaTime.TimeConfiguration.DaysPerSeason; }
        }

        /// <summary>
        /// Gets current game seconds component
        /// </summary>
        public int Seconds
        {
            get { return (int)(TotalSeconds % 60); }
        }

        /// <summary>
        /// Gets current game minute component
        /// </summary>
        public int Minutes
        {
            get { return TotalMinutes % 60; }
        }

        /// <summary>
        /// Gets current game hour component
        /// </summary>
        public int Hours
        {
            get { return TotalHours % 24; }
        }

        /// <summary>
        /// Gets current game day component
        /// </summary>
        public int Days
        {
            get { return TotalDays % UtopiaTime.TimeConfiguration.DaysPerSeason + 1; }
        }

        /// <summary>
        /// Gets current game season
        /// </summary>
        public Season Season
        {
            get { return UtopiaTime.TimeConfiguration.Seasons[TotalDays % UtopiaTime.TimeConfiguration.DaysPerSeason]; }
        }

        public int SeasonNumber
        {
            get { return TotalDays % UtopiaTime.TimeConfiguration.DaysPerSeason + 1; }
        }

        /// <summary>
        /// Gets current game year
        /// </summary>
        public int TotalYears
        {
            get { return (int)(TotalSeconds / SecondsPerDay / UtopiaTime.TimeConfiguration.DaysPerYear) + 1; }
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

        public static UtopiaTimeSpan FromMinutes(int minutes)
        {
            return new UtopiaTimeSpan { TotalSeconds = minutes * 60 };
        }

        public static UtopiaTimeSpan FromHours(int hours)
        {
            return new UtopiaTimeSpan { TotalSeconds = hours * 60 * 60 };
        }

        public static UtopiaTimeSpan FromDays(int days)
        {
            return new UtopiaTimeSpan { TotalSeconds = (long)days * 24 * 60 * 60 };
        }

        public static UtopiaTimeSpan FromSeasons(int seasons)
        {
            return new UtopiaTimeSpan { TotalSeconds = (long)seasons * UtopiaTime.TimeConfiguration.DaysPerSeason * 24 * 60 * 60 };
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