using System;
using S33M3DXEngine.Main;

namespace Utopia.Worlds.GameClocks
{
    public class RealTimeClock : Clock
    {
        #region Private variables
        private float _startTime;
        #endregion

        #region Public variable/properties
        public float ClockSpeed { get; set; }
        #endregion

        public RealTimeClock(float clockSpeed, float startTime)
        {
            _startTime = startTime;
            ClockSpeed = clockSpeed;
        }

        #region Public methods
        public override void Initialize()
        {
            base._clockTime = _startTime;
            base.Initialize();
        }

        public override void FTSUpdate(GameTime timeSpend)
        {
            _clockTime = (float)(DateTime.Now.Hour * 60 + DateTime.Now.Minute) * (float)(Math.PI) / 12.0f / 60.0f;

            _visualClockTime.ClockTimeNormalized = _clockTime;
        }

        #endregion

        #region Private methods
        #endregion


    }
}
