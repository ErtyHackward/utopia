using System.Collections.Generic;
using ProtoBuf;
using Utopia.Shared.Services;

namespace Utopia.Shared.Structs
{
    /// <summary>
    /// Represents ingame time. Contains seasons instead of months. Provides variable season length.
    /// </summary>
    [ProtoContract]
    public struct UtopiaTime
    {
        public const int SecondsPerMinute = 60;
        public const int SecondsPerHour = SecondsPerMinute * 60;
        public const int SecondsPerDay = SecondsPerHour * 24;

        static UtopiaTime()
        {
            TimeConfiguration = new UtopiaTimeConfiguration();
            TimeConfiguration.DaysPerSeason = 10;
            TimeConfiguration.DayLength = 1200;
            TimeConfiguration.Seasons = new List<Season>();

            TimeConfiguration.Seasons.Add(new Season { Name = "Early Spring",   Temperature = -0.32f,   Moisture = 0.3f });
            TimeConfiguration.Seasons.Add(new Season { Name = "Spring",         Temperature = 0,        Moisture = 0.1f });
            TimeConfiguration.Seasons.Add(new Season { Name = "Late Spring",    Temperature = 0.34f,    Moisture = -0.1f });
            TimeConfiguration.Seasons.Add(new Season { Name = "Early Summer",   Temperature = 0.64f,    Moisture = -0.2f });
            TimeConfiguration.Seasons.Add(new Season { Name = "Summer",         Temperature = 1,        Moisture = -0.3f });
            TimeConfiguration.Seasons.Add(new Season { Name = "Late Summer",    Temperature = 0.67f,    Moisture = -0.2f });
            TimeConfiguration.Seasons.Add(new Season { Name = "Early Autumn",   Temperature = 0.34f,    Moisture = -0.1f });
            TimeConfiguration.Seasons.Add(new Season { Name = "Autumn",         Temperature = 0,        Moisture = 0.1f });
            TimeConfiguration.Seasons.Add(new Season { Name = "Late Autumn",    Temperature = -0.32f,   Moisture = 0.3f });
            TimeConfiguration.Seasons.Add(new Season { Name = "Early Winter",   Temperature = -0.65f,   Moisture = 0.5f });
            TimeConfiguration.Seasons.Add(new Season { Name = "Winter",         Temperature = -1,       Moisture = 0.7f });
            TimeConfiguration.Seasons.Add(new Season { Name = "Late Winter",    Temperature = -0.65f,   Moisture = 0.5f });
        }

        public static UtopiaTimeConfiguration TimeConfiguration { get; set; }

        /// <summary>
        /// Amount of seconds passed from the begginning of the world
        /// </summary>
        [ProtoMember(1)]
        public long TotalSeconds { get; set; }
        
        /// <summary>
        /// Amount of minutes passed from the begginning of the world
        /// </summary>
        public int TotalMinutes
        {
            get { return (int)(TotalSeconds / SecondsPerMinute); }
        }

        /// <summary>
        /// Amount of hours passed from the begginning of the world
        /// </summary>
        public int TotalHours
        {
            get { return (int)(TotalSeconds / SecondsPerHour); }
        }

        /// <summary>
        /// Amount of days passed from the begginning of the world
        /// </summary>
        public int TotalDays
        {
            get { return (int)(TotalSeconds / SecondsPerDay); }
        }

        /// <summary>
        /// Amount of seasons passed from the begginning of the world
        /// </summary>
        public int TotalSeasons
        {
            get { return TotalDays / TimeConfiguration.DaysPerSeason; }
        }

        /// <summary>
        /// Amount of Years passed from the begginning of the world
        /// </summary>
        public int TotalYears
        {
            get { return (int)(TotalDays / UtopiaTime.TimeConfiguration.DaysPerYear); }
        }

        /// <summary>
        /// Gets current game seconds component
        /// </summary>
        public int Second 
        {
            get { return (int)(TotalSeconds % 60); }
        }

        /// <summary>
        /// Gets current game minute component
        /// </summary>
        public int Minute 
        {
            get { return TotalMinutes % 60; }
        }

        /// <summary>
        /// Gets current game hour component
        /// </summary>
        public int Hour 
        {
            get { return TotalHours % 24; }
        }

        /// <summary>
        /// Gets current game day component
        /// </summary>
        public int Day 
        {
            get { return TotalDays % TimeConfiguration.DaysPerSeason + 1; }
        }

        /// <summary>
        /// Gets current game season
        /// </summary>
        public Season Season 
        {
            get
            {
                if (SeasonIndex < TimeConfiguration.Seasons.Count - 1) return TimeConfiguration.Seasons[SeasonIndex];
                return null;
            }
        }

        public int SeasonNumber 
        {
            get { return ((TotalDays % TimeConfiguration.DaysPerYear) / TimeConfiguration.DaysPerSeason) + 1; }
        }

        public int SeasonIndex
        {
            get { return (TotalDays % TimeConfiguration.DaysPerYear) / TimeConfiguration.DaysPerSeason; }
        }
        
        /// <summary>
        /// Gets current game year
        /// </summary>
        public int Year 
        {
            get { return (int)(TotalSeconds / SecondsPerDay / TimeConfiguration.DaysPerYear) + 1; }
        }

        /// <summary>
        /// Returns current date without time component
        /// </summary>
        public UtopiaTime Date {
            get { 
                return new UtopiaTime { 
                    TotalSeconds = TotalSeconds - TotalSeconds % SecondsPerDay 
                }; 
            }
        }

        public UtopiaTimeSpan TimeOfDay {
            get { return UtopiaTimeSpan.FromSeconds(TotalSeconds % SecondsPerDay); }
        }

        public override string ToString()
        {
            return string.Format("Year : {0} Season : <{1}> Day : {2} [{3:00}:{4:00}:{5:00}]", Year, Season.Name, Day, Hour, Minute, Second);
        }

        public static bool operator >(UtopiaTime t1, UtopiaTime t2)
        {
            return t1.TotalSeconds > t2.TotalSeconds;
        }

        public static bool operator <(UtopiaTime t1, UtopiaTime t2)
        {
            return t1.TotalSeconds < t2.TotalSeconds;
        }

        public static bool operator >=(UtopiaTime t1, UtopiaTime t2)
        {
            return t1.TotalSeconds >= t2.TotalSeconds;
        }

        public static bool operator <=(UtopiaTime t1, UtopiaTime t2)
        {
            return t1.TotalSeconds <= t2.TotalSeconds;
        }

        public static bool operator ==(UtopiaTime t1, UtopiaTime t2)
        {
            return t1.TotalSeconds == t2.TotalSeconds;
        }

        public static bool operator !=(UtopiaTime t1, UtopiaTime t2)
        {
            return !(t1 == t2);
        }

        public static UtopiaTime operator +(UtopiaTime t1, UtopiaTimeSpan s1)
        {
            t1.TotalSeconds += s1.TotalSeconds;
            return t1;
        }

        public static UtopiaTime operator -(UtopiaTime t1, UtopiaTimeSpan s1)
        {
            t1.TotalSeconds -= s1.TotalSeconds;
            return t1;
        }

        public static UtopiaTimeSpan operator -(UtopiaTime t1, UtopiaTime t2)
        {
            return UtopiaTimeSpan.FromSeconds(t1.TotalSeconds - t2.TotalSeconds);
        }

        public bool Equals(UtopiaTime other)
        {
            return TotalSeconds == other.TotalSeconds;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is UtopiaTime && Equals((UtopiaTime)obj);
        }

        public override int GetHashCode()
        {
            return TotalSeconds.GetHashCode();
        }
    }
}
