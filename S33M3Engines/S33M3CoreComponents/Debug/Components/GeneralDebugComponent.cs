using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3DXEngine.Main;
using S33M3DXEngine.Threading;
using S33M3DXEngine.Debug.Interfaces;
using SharpDX.Direct3D11;
using System.Diagnostics;
using S33M3CoreComponents.Inputs;
using S33M3CoreComponents.Inputs.Actions;

namespace S33M3CoreComponents.Debug.Components
{
    public class GeneralDebugComponent : DrawableGameComponent, IDebugInfo
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        #region Private Variables
        private PerformanceCounter _cpuCounter;
        private PerformanceCounter _ramCounter;
        private Process _proc;
        private float _fps;
        private float _Updts;
        private InputsManager _inputsManager;

        private float _updateInterval = 1.0f;

        private float _timeSinceLastUpdateFPS = 0.0f;
        private float _framecountFPS = 0;
        private DateTime _prevRealTimeFPS = DateTime.Now;
        private DateTime _prevRealTimeUpdts = DateTime.Now;
        private string _usedRam, _freeRam;
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

        public GeneralDebugComponent(InputsManager inputsManager)
        {
            _inputsManager = inputsManager;
        }

        public override void Initialize()
        {
            _cpuCounter = ToDispose(new PerformanceCounter());
            _cpuCounter.CategoryName = "Processor";
            _cpuCounter.CounterName = "% Processor Time";
            _cpuCounter.InstanceName = "_Total";
            try
            {
                _ramCounter = ToDispose(new PerformanceCounter("Memory", "Available MBytes"));
            }
            catch (InvalidOperationException ex)
            {
                logger.Error(ex.Message);
            }
        }

        public override void Update(GameTime timeSpent)
        {
            if (_inputsManager.MouseManager.CurMouseState.middleButton == Inputs.MouseHandler.ButtonState.Pressed)
            {
                _usedRam = ((int)_proc.PrivateMemorySize64 / (1024 * 1024)).ToString();
                _freeRam = getAvailableRAM();
            }
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

        /// <summary>
        /// Call this method every time you need to know 
        /// the current cpu usage. 
        /// </summary>
        /// <returns></returns>
        private string getCurrentCpuUsage()
        {
            return _cpuCounter.NextValue().ToString("000") + "%";
        }

        /// <summary>
        /// Call this method every time you need to get 
        /// the amount of the available RAM in Mb 
        /// </summary>
        /// <returns></returns>
        private string getAvailableRAM()
        {
            return _ramCounter != null ? _ramCounter.NextValue() + "MB" : "ERROR MB";
        } 

        #region IDebugInfo Members

        public bool ShowDebugInfo { get; set; }

        public string GetDebugInfo()
        {
            _proc = Process.GetCurrentProcess();
            return string.Concat("FPS : ", _fps.ToString("000"), " Active background threads : ", SmartThread.ThreadPool.InUseThreads, " Used Ram : ", _usedRam, "MB Free Ram : ", _freeRam);
        }
        #endregion

    }
}
