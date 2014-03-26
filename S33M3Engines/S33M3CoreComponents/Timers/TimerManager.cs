using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3DXEngine.Main;
using System.Diagnostics;

namespace S33M3CoreComponents.Timers
{
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
        public GameTimer AddTimer(long timerFrequencyMS)
        {
            GameTimer newTimer = ToDispose(new GameTimer());
            newTimer.Initialize(timerFrequencyMS);
            Timers.Add(newTimer);
            return newTimer;
        }

        public void RemoveTimer(GameTimer timer)
        {
            Timers.Remove(timer);
        }

        public override void FTSUpdate(GameTime timeSpend)
        {
            for (int i = 0; i < Timers.Count; i++)
            {
                Timers[i].Update(timeSpend);
            }
        }
        #endregion

        #region Private methods
        #endregion
        public class GameTimer : IDisposable
        {
            public delegate void timerRaised(float elapsedTimeInS);
            public event timerRaised OnTimerRaised;

            private Stopwatch timer = new Stopwatch();
            private long _frequency;
            private float _elapsedTotalTime;
            public int TimerId;

            /// <summary>
            /// The timer frenquency in ms
            /// </summary>
            /// <param name="timerFrequencyMs"></param>
            public void Initialize(long timerFrequencyMs)
            {
                timer = new Stopwatch();
                _elapsedTotalTime = 0;
                timer.Start();
                _frequency = timerFrequencyMs;
            }

            public void Update(GameTime timeSpend)
            {
                _elapsedTotalTime += timeSpend.ElapsedGameTimeInS_LD;
                if (timer.ElapsedMilliseconds > _frequency)
                {
                    if (OnTimerRaised != null) OnTimerRaised(_elapsedTotalTime);
                    _elapsedTotalTime = 0;
                    timer.Restart();
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
    }
}
