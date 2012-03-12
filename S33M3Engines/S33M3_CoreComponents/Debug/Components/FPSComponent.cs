using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3_DXEngine.Main;
using S33M3_DXEngine.Threading;
using S33M3_DXEngine.Debug.Interfaces;
using SharpDX.Direct3D11;

namespace S33M3_CoreComponents.Debug.Components
{
    public class FPSComponent : DrawableGameComponent, IDebugInfo
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

        public FPSComponent()
        {
        }

        public override void Draw(DeviceContext context, int index)
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

                _framecountFPS = 0;
                _timeSinceLastUpdateFPS -= _updateInterval;
            }
        }

        #region IDebugInfo Members

        public bool ShowDebugInfo { get; set; }

        public string GetDebugInfo()
        {
            return string.Concat("FPS : ", _fps.ToString("000"), " Active background thread(s) : ", SmartThread.ThreadPool.InUseThreads);
        }
        #endregion

    }
}
