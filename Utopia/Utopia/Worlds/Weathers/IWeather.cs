using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Engines.D3D;
using Utopia.Worlds.Weathers.SharedComp;

namespace Utopia.Worlds.Weather
{
    public interface IWeather : IUpdateableComponent,IGameComponent
    {
        IWind Wind { get; set; }
    }
}
