using System;
using System.Drawing;

namespace Utopia.Shared.World.PlanGenerator
{
    [Serializable]
    public struct GenerationParameters
    {
        public int ElevationSeed { get; set; }
        public int GridSeed { get; set; }
        public int PolygonsCount { get; set; }
        public int RelaxCount { get; set; }
        public bool CenterElevation { get; set; }
        public Size MapSize { get; set; }
    }
}