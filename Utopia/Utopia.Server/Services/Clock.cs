using System;

namespace Utopia.Server.Services
{
    /// <summary>
    /// Represents a server game clock
    /// </summary>
    public class Clock
    {
        private readonly DateTime _clockStartTime;
        private readonly DateTime _gameStartTime;
        private double _timeFactor;
        private TimeSpan _dayLength;

        /// <summary>
        /// Gets current game time
        /// </summary>
        public DateTime Now
        {
            get {
                var realTimeDiff = DateTime.Now - _clockStartTime;
                return _gameStartTime + TimeSpan.FromSeconds(realTimeDiff.TotalSeconds * _timeFactor);
            }
        }
        
        /// <summary>
        /// Gets the time span that represents 24-hours game day
        /// </summary>
        public TimeSpan DayLength
        {
            get { return _dayLength; }
            private set { 
                _dayLength = value;
                TimeFactor = TimeSpan.FromDays(1).TotalSeconds / value.TotalSeconds;
            }
        }
        
        /// <summary>
        /// Gets the time scaling factor (how many game seconds in one real second)
        /// </summary>
        public double TimeFactor
        {
            get { return _timeFactor; }
            private set { 
                _timeFactor = value;
                _dayLength = TimeSpan.FromSeconds(TimeSpan.FromDays(1).TotalSeconds/value);
            }
        }
        
        protected Clock()
        {
            _clockStartTime = DateTime.Now;
        }

        /// <summary>
        /// Creates new instance of game clock and starts it
        /// </summary>
        /// <param name="startGameTime"></param>
        /// <param name="dayLength"></param>
        public Clock(DateTime startGameTime, TimeSpan dayLength) : this()
        {
            _gameStartTime = startGameTime;
            DayLength = dayLength;
        }
    }
}
