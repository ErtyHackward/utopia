using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Worlds.Chunks;
using Utopia.Worlds.SkyDomes;
using Utopia.Worlds.GameClocks;
using Utopia.Worlds.Weather;

namespace Utopia.Worlds
{
    public class World : IWorld
    {
        public IWorldChunks WorldChunks { get; set; }
        public IClock WorldClock { get; set; }
        public ISkyDome WorldSkyDome { get; set; }
        public IWeather WorldWeather { get; set; }
    }
}
