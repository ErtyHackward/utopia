using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Worlds.Weathers.SharedComp;
using Utopia.Worlds.GameClocks;

namespace Utopia.Worlds.Weather
{
    public class Weather : IWeather
    {
        #region Private variable
        public IClock _clock { get; set; }
        #endregion

        #region Public properties/variable
        public IWind Wind { get; set; }
        #endregion

        public Weather(IClock clock)
        {
            Wind = new Wind();
            _clock = clock;
        }

        #region Public Methods
        public void Initialize()
        {
            Wind.Initialize();
        }

        public void Update(ref S33M3Engines.D3D.GameTime TimeSpend)
        {
            Wind.Update(ref TimeSpend);
        }

        public void Interpolation(ref double interpolation_hd, ref float interpolation_ld)
        {
            Wind.Interpolation(ref interpolation_hd, ref interpolation_ld);
        }

        public void Dispose()
        {
            Wind.Dispose();
        }
        #endregion 

        #region Private methods
        #endregion
    }
}
