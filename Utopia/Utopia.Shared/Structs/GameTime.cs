using System.Collections.Generic;
using ProtoBuf;
using Utopia.Shared.Services;

namespace Utopia.Shared.Structs
{
    [ProtoContract]
    public struct GameTime
    {
        private const int SecondsPerMinute = 60;
        private const int SecondsPerHour = SecondsPerMinute * 60;
        private const int SecondsPerDay = SecondsPerHour * 24;

        static GameTime()
        {
            TimeConfiguration = new GameTimeConfiguration();
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

        public static GameTimeConfiguration TimeConfiguration { get; set; }

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
            get { return TotalDays / TimeConfiguration.DaysPerSeason; }
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
            get { return TimeConfiguration.Seasons[TotalDays % TimeConfiguration.DaysPerSeason]; }
        }

        public int SeasonNumber 
        {
            get { return TotalDays % TimeConfiguration.DaysPerSeason + 1; }
        }
        
        /// <summary>
        /// Gets current game year
        /// </summary>
        public int Year 
        {
            get { return (int)(TotalSeconds / SecondsPerDay / TimeConfiguration.DaysPerYear) + 1; }
        }

        public override string ToString()
        {
            return string.Format("{0} {1} {2} {3:00}:{4:00}", Year, Season.Name, Day, Hour, Minute);
        }

        public static bool operator >(GameTime t1, GameTime t2)
        {
            return t1.TotalSeconds > t2.TotalSeconds;
        }

        public static bool operator <(GameTime t1, GameTime t2)
        {
            return t1.TotalSeconds < t2.TotalSeconds;
        }

        public static bool operator ==(GameTime t1, GameTime t2)
        {
            return t1.TotalSeconds == t2.TotalSeconds;
        }

        public static bool operator !=(GameTime t1, GameTime t2)
        {
            return !(t1 == t2);
        }

        public static GameTime operator +(GameTime t1, GameTimeSpan s1)
        {
            t1.TotalSeconds += s1.TotalSeconds;
            return t1;
        }

        public static GameTime operator -(GameTime t1, GameTimeSpan s1)
        {
            t1.TotalSeconds -= s1.TotalSeconds;
            return t1;
        }

        public static GameTimeSpan operator -(GameTime t1, GameTime t2)
        {
            return GameTimeSpan.FromSeconds(t1.TotalSeconds - t2.TotalSeconds);
        }

        public bool Equals(GameTime other)
        {
            return TotalSeconds == other.TotalSeconds;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is GameTime && Equals((GameTime)obj);
        }

        public override int GetHashCode()
        {
            return TotalSeconds.GetHashCode();
        }
    }
}
