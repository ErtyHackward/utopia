using System;
using Utopia.Shared.Net.Messages;

namespace Utopia.Server.Services
{
    /// <summary>
    /// Represents a server game clock
    /// </summary>
    public class Clock
    {
        private Server _server;
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

        protected Clock(Server server)
        {
            _server = server;
            _clockStartTime = DateTime.Now;

            _server.ConnectionManager.ConnectionAdded +=ConnectionManager_ConnectionAdded;
            _server.ConnectionManager.ConnectionRemoved += ConnectionManager_ConnectionRemoved;
        }

        void ConnectionManager_ConnectionAdded(object sender, ConnectionEventArgs e)
        {
            e.Connection.MessageRequestDateTimeSync += Connection_MessageRequestDateTimeSync;
        }

        void ConnectionManager_ConnectionRemoved(object sender, ConnectionEventArgs e)
        {
            e.Connection.MessageRequestDateTimeSync -= Connection_MessageRequestDateTimeSync;
        }

        /// <summary>
        /// Creates new instance of game clock and starts it
        /// </summary>
        /// <param name="startGameTime"></param>
        /// <param name="dayLength"></param>
        public Clock(Server server, DateTime startGameTime, TimeSpan dayLength)
            : this(server)
        {
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

        //New date time requested by client
        private void Connection_MessageRequestDateTimeSync(object sender, Shared.Net.Connections.ProtocolMessageEventArgs<Shared.Net.Messages.RequestDateTimeSyncMessage> e)
        {
            var connection = (ClientConnection)sender;
            connection.Send(new DateTimeMessage { DateTime = Now, TimeFactor = TimeFactor });
        }
    }
}
