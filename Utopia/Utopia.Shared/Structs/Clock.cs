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
        private readonly object _syncRoot = new object();
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public class GameClockTimer : IDisposable
        {
            public delegate void TimerRaised(UtopiaTime gametime);
            private event TimerRaised OnTimerRaised;
            private UtopiaTime _lastTriggerTime;
            private UtopiaTimeSpan _triggerSpan;
            private Clock _worldClock;

            public GameClockTimer(UtopiaTimeSpan interval, Clock worldClock, TimerRaised callBack)
            {
                OnTimerRaised += callBack;
                _worldClock = worldClock;
                _triggerSpan = interval;

                WorldTimeChanged(worldClock.Now);
            }

            public void WorldTimeChanged(UtopiaTime currentDateTime)
            {
                _lastTriggerTime = new UtopiaTime();

                while (_lastTriggerTime < currentDateTime)
                {
                    _lastTriggerTime += _triggerSpan;
                }

                _lastTriggerTime = _lastTriggerTime -= _triggerSpan;
            }

            public void Update()
            {
                var currentTime = _worldClock.Now;
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
                        OnTimerRaised -= (TimerRaised)d;
                    }
                }
            }
        }

        private ServerCore _server;
        private DateTime _clockStartTime;
        private UtopiaTime _gameStartTime;
        private double _timeFactor;
        private TimeSpan _dayLength;
        private List<GameClockTimer> _clockTimers = new List<GameClockTimer>();

        /// <summary>
        /// Gets current game time
        /// </summary>
        public UtopiaTime Now
        {
            get {
                var realTimeDiff = DateTime.Now - _clockStartTime;
                return _gameStartTime + UtopiaTimeSpan.FromSeconds(realTimeDiff.TotalSeconds * _timeFactor);
            }
        }

        public void CreateNewTimer(GameClockTimer newTimer)
        {
            lock (_syncRoot)
            {
                _clockTimers.Add(newTimer);
            }
        }

        /// <summary>
        /// Gets the time span (real time) that represents 24-hours game day
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
        public UtopiaTimeSpan RealToGameSpan(TimeSpan realTimeSpan)
        {
            return UtopiaTimeSpan.FromSeconds(realTimeSpan.TotalSeconds * _timeFactor);
        }

        public TimeSpan GameToReal(UtopiaTimeSpan gameSpan)
        {
            return TimeSpan.FromSeconds(gameSpan.TotalSeconds / _timeFactor);
        }

        /// <summary>
        /// Creates new instance of game clock and starts it
        /// </summary>
        /// <param name="server"></param>
        /// <param name="startGameTime"></param>
        /// <param name="dayLength"></param>
        public Clock(ServerCore server, UtopiaTime startGameTime, TimeSpan dayLength)
        {
            _clockStartTime = DateTime.Now;
            _gameStartTime = startGameTime;
            DayLength = dayLength;

            _server = server;
            _server.Scheduler.AddPeriodic(TimeSpan.FromSeconds(1), Tick);
        }

        public void SetCurrentTime(UtopiaTime time)
        {
            _clockStartTime = DateTime.Now;
            _gameStartTime = time;

            //Refresh Timer !
            foreach (var t in _clockTimers) 
                t.WorldTimeChanged(Now);
        }

        public void SetCurrentTimeOfDay(UtopiaTimeSpan time)
        {
            SetCurrentTime(Now.Date + time);
        }

        /// <summary>
        /// Will trigger Clock event
        /// </summary>
        private void Tick()
        {
            lock (_syncRoot)
            {
                foreach (var timer in _clockTimers)
                {
                    timer.Update();
                }
            }
        }

        public void Dispose()
        {
            foreach (var t in _clockTimers) t.Dispose();
            //Save server Current elapsed time            
            _server.CustomStorage.SetVariable("GameTimeElapsed", Now);
        }
    }
}
