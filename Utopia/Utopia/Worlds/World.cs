using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Worlds.Chunks;
using Utopia.Worlds.SkyDomes;
using Utopia.Worlds.GameClocks;
using Utopia.Worlds.Weather;
using Utopia.Shared.Chunks;

namespace Utopia.Worlds
{
    public class World : IWorld
    {
        public SingleArrayChunkContainer SingleArrayChunkContainer { get; set; }
        public IWorldChunks WorldChunks { get; set; }
        public IClock WorldClock { get; set; }
        public ISkyDome WorldSkyDome { get; set; }
        public IWeather WorldWeather { get; set; }

        public World(IWorldChunks worldChunks, IClock worldClock, ISkyDome worldSkyDome, IWeather worldWeather, SingleArrayChunkContainer singleArrayChunkContainer)
        {
            this.SingleArrayChunkContainer = singleArrayChunkContainer;
            this.WorldChunks = worldChunks;
            this.WorldClock = worldClock;
            this.WorldSkyDome = worldSkyDome;
            this.WorldWeather = worldWeather;
        }
    }
}
