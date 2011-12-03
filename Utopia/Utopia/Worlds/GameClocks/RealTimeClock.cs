using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Engines.D3D;
using Utopia.Settings;

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

        public override void Update(ref S33M3Engines.D3D.GameTime timeSpend)
        {
            _clockTime.Value = (float)(DateTime.Now.Hour * 60 + DateTime.Now.Minute) * (float)(Math.PI) / 12.0f / 60.0f;

            _visualClockTime.Time = _clockTime.ValueInterp;
        }

        public override void Interpolation(ref double interpolation_hd, ref float interpolation_ld)
        {
            base.Interpolation(ref interpolation_hd, ref interpolation_ld);
        }

        public override string GetInfo()
        {
            return base.GetInfo();
        }
        #endregion

        #region Private methods
        #endregion


    }
}
