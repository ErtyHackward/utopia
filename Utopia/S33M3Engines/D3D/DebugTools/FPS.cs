using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Engines.Threading;

namespace S33M3Engines.D3D.DebugTools
{
    public class FPS : DrawableGameComponent, IDebugInfo
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

        public override void Draw(int Index)
        {
            DateTime currentRealTime = DateTime.Now;
            TimeSpan elapsedRealTime = currentRealTime - _prevRealTimeFPS;
            _prevRealTimeFPS = currentRealTime;

            float elapsed = (float) elapsedRealTime.TotalSeconds;
            _framecountFPS++;
            _timeSinceLastUpdateFPS += elapsed;
            if (_timeSinceLastUpdateFPS > _updateInterval)
            {
                _fps = _framecountFPS/_timeSinceLastUpdateFPS;

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