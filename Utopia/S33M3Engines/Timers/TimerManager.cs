using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using S33M3Engines.D3D;

namespace S33M3Engines.Timers
{
    /// <summary>
    /// Timer manager for the engine
    /// </summary>
    public class TimerManager : GameComponent
    {
        #region Private variables
        #endregion

        #region Public variables
        public List<GameTimer> Timers { get; set; }
        #endregion

        public TimerManager()
        {
            Timers = new List<GameTimer>();
            this.UpdateOrder = 0;
        }

        #region Public methods
        public GameTimer AddTimer(int timerId, long timerFrequency)
        {
            GameTimer newTimer = new GameTimer();
            newTimer.TimerId = timerId;
            newTimer.Initialize(timerFrequency);
            Timers.Add(newTimer);
            return newTimer;
        }

        public void RemoveTimer(int timerId)
        {
            Timers.RemoveAll(x => x.TimerId == timerId);
        }

        public void RemoveTimer(GameTimer timer)
        {
            Timers.Remove(timer);
        }

        public override void Update(ref GameTime timeSpent)
        {
            for (int i = 0; i < Timers.Count; i++)
            {
                Timers[i].Update();
            }
        }
        #endregion

        #region Private methods
        #endregion
        public class GameTimer
        {
            public delegate void timerRaised();
            public event timerRaised OnTimerRaised;

            private long _nextTriggerTick;
            private long _frequency;
            public int TimerId;

            /// <summary>
            /// The timer frenquency in ms
            /// </summary>
            /// <param name="timerFrequency"></param>
            public void Initialize(long timerFrequency)
            {
                _frequency = timerFrequency;
                SetNextTrigger();
            }

            private void SetNextTrigger()
            {
                _nextTriggerTick = Stopwatch.GetTimestamp() + (Stopwatch.Frequency * _frequency / 1000);
            }

            public void Update()
            {
                if (Stopwatch.GetTimestamp() > _nextTriggerTick)
                {
                    if (OnTimerRaised != null) OnTimerRaised();
                    SetNextTrigger();
                }
            }
        }
    }
}
