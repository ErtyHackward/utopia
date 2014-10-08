using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Worlds.Weathers.SharedComp;
using S33M3DXEngine.Main.Interfaces;

namespace Utopia.Worlds.Weather
{
    public interface IWeather : IUpdatableComponent,IGameComponent
    {
        IWind Wind { get; set; }
        float MoistureOffset { get; set; }
        float TemperatureOffset { get; set; }
    }
}
