using System;
using System.Collections.Generic;
using Utopia.Shared.Server;

namespace Utopia.Shared.Structs
{
    /// <summary>
    /// Represents a server game clock
    /// </summary>
    public class Clock : IDisposable
    {

        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public class GameClockTimer : IDisposable
        {
            public delegate void timerRaised(DateTime gametime);
            private event timerRaised OnTimerRaised;
            private DateTime _lastTriggerTime = new DateTime();
            private TimeSpan _triggerSpan;
            private Clock _worldClock;

            public GameClockTimer(int Year, int Day, int Hours, int Minute, Clock worldClock, timerRaised callBack)
            {
                OnTimerRaised += callBack;
                _worldClock = worldClock;
                int NbrSeconds = Minute * 60;
                NbrSeconds += Hours * (60 * 60);
                NbrSeconds += Day * (60 * 60 * 24);
                NbrSeconds += Year * (60 * 60 * 24 * worldClock.CalendarDaysPerYear);

                _triggerSpan = new TimeSpan(0, 0, 0, NbrSeconds, 0);

                WorldTimeChanged(worldClock.Now);
            }

            public void WorldTimeChanged(DateTime currentDateTime)
            {
                _lastTriggerTime = new DateTime(2000, 1, 1, 0, 0, 0);

                while (_lastTriggerTime < currentDateTime)
                {
                    _lastTriggerTime += _triggerSpan;
                }

                _lastTriggerTime = _lastTriggerTime -= _triggerSpan;
            }

            public void Update()
            {
                DateTime currentTime = _worldClock.Now;
                if (currentTime - _lastTriggerTime >= _triggerSpan)
                {
                    _lastTriggerTime += _triggerSpan;
                    if (OnTimerRaised != null) OnTimerRaised(_lastTriggerTime);
                }
            }

            public void Dispose()
            {
                if (OnTimerRaised != null)
                {
                    //Remove all Events associated to this control (That haven't been unsubscribed !)
                    foreach (Delegate d in OnTimerRaised.GetInvocationList())
                    {
                        OnTimerRaised -= (timerRaised)d;
                    }
                }
            }
        }

        private GameCalendar _currentGameCalendar;
        private int _calendarDaysPerYear;
        private ServerCore _server;
        private DateTime _clockStartTime;
        private DateTime _gameStartTime;
        private double _timeFactor;
        private TimeSpan _dayLength;
        private List<GameClockTimer> _clockTimers = new List<GameClockTimer>();

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

        public int CalendarDaysPerYear
        {
            get { return _calendarDaysPerYear; }
            set { _calendarDaysPerYear = value; }
        }

        public List<GameClockTimer> ClockTimers
        {
            get { return _clockTimers; }
            set { _clockTimers = value; }
        }

        public GameCalendar CurrentGameCalendar
        {
            get { return _currentGameCalendar; }
            private set { _currentGameCalendar = value; }
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
        public Clock(ServerCore server, DateTime startGameTime, TimeSpan dayLength)
        {
            _calendarDaysPerYear = 10;
            _clockStartTime = DateTime.Now;
            _gameStartTime = startGameTime;
            DayLength = dayLength;

            _server = server;
            _server.Scheduler.AddPeriodic(TimeSpan.FromSeconds(1), Tick);

            _currentGameCalendar.Day = server.CustomStorage.GetVariable<uint>("CalendarDay", 1);
            _currentGameCalendar.Year = server.CustomStorage.GetVariable<uint>("CalendarYear", 1);

            //Create a clock Event for updating the calendar
            this.ClockTimers.Add(new GameClockTimer(0, 1, 0, 0, this, PerDayTrigger));
        }

        //Will be raised at every game day
        private void PerDayTrigger(DateTime gametime)
        {
            //Update Calendar date/year
            _currentGameCalendar.Day += 1;
            if (_currentGameCalendar.Day > _calendarDaysPerYear)
            {
                _currentGameCalendar.Year += 1;
                _currentGameCalendar.Day = 1;
            }
        }

        public void SetCurrentTime(DateTime time)
        {
            _clockStartTime = DateTime.Now;
            _gameStartTime = time;

            //Refresh Timer !
            foreach (var t in _clockTimers) t.WorldTimeChanged(this.Now);
        }

        public void SetCurrentTimeOfDay(TimeSpan time)
        {
            SetCurrentTime(_gameStartTime.Date + time);
            _clockStartTime = DateTime.Now;
        }

        /// <summary>
        /// Will trigger Clock event
        /// </summary>
        private void Tick()
        {
            foreach (var timer in ClockTimers)
            {
                timer.Update();
            }
        }

        public void Dispose()
        {
            foreach (var t in ClockTimers) t.Dispose();
            _server.CustomStorage.SetVariable("CalendarDay", _currentGameCalendar.Day);
            _server.CustomStorage.SetVariable("CalendarYear", _currentGameCalendar.Year);
        }
    }
}
