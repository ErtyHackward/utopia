using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Settings;
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
            base._clockTime.Value = _startTime;
            base.Initialize();
        }

        public override void Dispose()
        {
            base.Dispose();
        }

        public override void Update(GameTime timeSpend)
        {
            _clockTime.Value = (float)(DateTime.Now.Hour * 60 + DateTime.Now.Minute) * (float)(Math.PI) / 12.0f / 60.0f;

            _visualClockTime.Time = _clockTime.ValueInterp;
        }

        public override void Interpolation(double interpolation_hd, float interpolation_ld, long timePassed)
        {
            base.Interpolation(interpolation_hd, interpolation_ld, timePassed);
        }

        #endregion

        #region Private methods
        #endregion


    }
}
