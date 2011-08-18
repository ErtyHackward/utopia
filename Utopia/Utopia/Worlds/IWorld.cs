using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Worlds.Chunks;
using Utopia.Worlds.GameClocks;
using Utopia.Worlds.SkyDomes;
using Utopia.Worlds.Weather;

namespace Utopia.Worlds
{
    public interface IWorld
    {
        IWorldChunks WorldChunks { get; set; }
        IClock WorldClock { get; set; }
        ISkyDome WorldSkyDome { get; set; }
        IWeather WorldWeather { get; set; }
    }
}
