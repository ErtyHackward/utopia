using ProtoBuf;
using Utopia.Shared.Services;

namespace Utopia.Shared.Structs
{
    [ProtoContract]
    public struct UtopiaGameTimeSpan
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
            get { return TotalDays / UtopiaGameTime.TimeConfiguration.DaysPerSeason; }
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
            get { return TotalDays % UtopiaGameTime.TimeConfiguration.DaysPerSeason + 1; }
        }

        /// <summary>
        /// Gets current game season
        /// </summary>
        public Season Season
        {
            get { return UtopiaGameTime.TimeConfiguration.Seasons[TotalDays % UtopiaGameTime.TimeConfiguration.DaysPerSeason]; }
        }

        public int SeasonNumber
        {
            get { return TotalDays % UtopiaGameTime.TimeConfiguration.DaysPerSeason + 1; }
        }

        /// <summary>
        /// Gets current game year
        /// </summary>
        public int TotalYears
        {
            get { return (int)(TotalSeconds / SecondsPerDay / UtopiaGameTime.TimeConfiguration.DaysPerYear) + 1; }
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

        public static bool operator >(UtopiaGameTimeSpan t1, UtopiaGameTimeSpan t2)
        {
            return t1.TotalSeconds > t2.TotalSeconds;
        }

        public static bool operator <(UtopiaGameTimeSpan t1, UtopiaGameTimeSpan t2)
        {
            return t1.TotalSeconds < t2.TotalSeconds;
        }

        public static bool operator ==(UtopiaGameTimeSpan t1, UtopiaGameTimeSpan t2)
        {
            return t1.TotalSeconds == t2.TotalSeconds;
        }

        public static bool operator !=(UtopiaGameTimeSpan t1, UtopiaGameTimeSpan t2)
        {
            return !(t1 == t2);
        }

        public static UtopiaGameTimeSpan operator +(UtopiaGameTimeSpan t1, UtopiaGameTimeSpan t2)
        {
            return FromSeconds(t1.TotalSeconds + t2.TotalSeconds);
        }

        public static UtopiaGameTimeSpan operator -(UtopiaGameTimeSpan t1, UtopiaGameTimeSpan t2)
        {
            return FromSeconds(t1.TotalSeconds - t2.TotalSeconds);
        }

        public static UtopiaGameTimeSpan FromSeconds(long seconds)
        {
            return new UtopiaGameTimeSpan { TotalSeconds = seconds };
        }

        public static UtopiaGameTimeSpan FromMinutes(int minutes)
        {
            return new UtopiaGameTimeSpan { TotalSeconds = minutes * 60 };
        }

        public static UtopiaGameTimeSpan FromHours(int hours)
        {
            return new UtopiaGameTimeSpan { TotalSeconds = hours * 60 * 60 };
        }

        public static UtopiaGameTimeSpan FromDays(int days)
        {
            return new UtopiaGameTimeSpan { TotalSeconds = (long)days * 24 * 60 * 60 };
        }

        public static UtopiaGameTimeSpan FromSeasons(int seasons)
        {
            return new UtopiaGameTimeSpan { TotalSeconds = (long)seasons * UtopiaGameTime.TimeConfiguration.DaysPerSeason * 24 * 60 * 60 };
        }

        public bool Equals(UtopiaGameTimeSpan other)
        {
            return TotalSeconds == other.TotalSeconds;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is UtopiaGameTimeSpan && Equals((UtopiaGameTimeSpan)obj);
        }

        public override int GetHashCode()
        {
            return TotalSeconds.GetHashCode();
        }
    }
}