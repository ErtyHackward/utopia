using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Engines.Threading;

namespace S33M3Engines.D3D.DebugTools
{
    public class FPS : GameComponent, IDebugInfo
    {
        #region Private Variables
        private float _fps;
        private float _Updts;

        private float _updateInterval = 1.0f;

        private float _timeSinceLastUpdateFPS = 0.0f;
        private float _framecountFPS = 0;
        private DateTime _prevRealTimeFPS = DateTime.Now;

        private DateTime _prevRealTimeUpdts = DateTime.Now;
        #endregion

        #region Public Methods
        public float Fps
        {
            get { return _fps; }
            set { _fps = value; }
        }

        public float Updts
        {
            get { return _Updts; }
            set { _Updts = value; }
        }

        public bool ShowInGameBar { get; set; }
        #endregion

        public FPS()
        {
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            //ShowInGameBar = true;
            base.Initialize();
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(ref GameTime TimeSpend)
        {
            base.Update(ref TimeSpend);
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void DrawDepth0()
        {
            DateTime currentRealTime = DateTime.Now;
            TimeSpan elapsedRealTime = currentRealTime - _prevRealTimeFPS;
            _prevRealTimeFPS = currentRealTime;

            float elapsed = (float)elapsedRealTime.TotalSeconds;
            _framecountFPS++;
            _timeSinceLastUpdateFPS += elapsed;
            if (_timeSinceLastUpdateFPS > _updateInterval)
            {
                _fps = _framecountFPS / _timeSinceLastUpdateFPS;

                //Game.GameWindow.Text = "FPS: " + _fps.ToString();
                _framecountFPS = 0;
                _timeSinceLastUpdateFPS -= _updateInterval;
            }

        }

        #region IDebugInfo Members

        public string GetInfo()
        {
            return string.Concat("FPS : ", _fps, " Active background thread(s) : ", WorkQueue.ThreadPool.InUseThreads);
        }

        #endregion
    }
}
