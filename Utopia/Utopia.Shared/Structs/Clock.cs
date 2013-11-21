using System;

namespace Utopia.Shared.Structs
{
    /// <summary>
    /// Represents a server game clock
    /// </summary>
    public class Clock
    {
        private DateTime _clockStartTime;
        private DateTime _gameStartTime;
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

        /// <summary>
        /// Converts real time span to game time span
        /// </summary>
        /// <param name="realTimeSpan"></param>
        /// <returns></returns>
        public TimeSpan RealToGameSpan(TimeSpan realTimeSpan)
        {
            return TimeSpan.FromSeconds(realTimeSpan.TotalSeconds * _timeFactor);
        }

        public TimeSpan GameToReal(TimeSpan gameSpan)
        {
            return TimeSpan.FromSeconds(gameSpan.TotalSeconds / _timeFactor);
        }

        /// <summary>
        /// Creates new instance of game clock and starts it
        /// </summary>
        /// <param name="startGameTime"></param>
        /// <param name="dayLength"></param>
        public Clock(DateTime startGameTime, TimeSpan dayLength)
        {
            _clockStartTime = DateTime.Now;
            _gameStartTime = startGameTime;
            DayLength = dayLength;
        }

        public void SetCurrentTime(DateTime time)
        {
            _clockStartTime = DateTime.Now;
            _gameStartTime = time;
        }

        public void SetCurrentTimeOfDay(TimeSpan time)
        {
            SetCurrentTime(_gameStartTime.Date + time);
            _clockStartTime = DateTime.Now;
        }
    }
}