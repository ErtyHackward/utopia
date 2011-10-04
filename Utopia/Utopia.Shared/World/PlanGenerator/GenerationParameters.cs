using System;
using System.IO;
using Utopia.Shared.Interfaces;
using Utopia.Shared.Structs;

namespace Utopia.Shared.World.PlanGenerator
{
    /// <summary>
    /// Provides parameters for world plan generation
    /// </summary>
    [Serializable]
    public struct GenerationParameters : IBinaryStorable
    {
        /// <summary>
        /// Elevation noise seed
        /// </summary>
        public int ElevationSeed { get; set; }

        /// <summary>
        /// Voronoi grid seed
        /// </summary>
        public int GridSeed { get; set; }

        /// <summary>
        /// Voronoi polygon count
        /// </summary>
        public int PolygonsCount { get; set; }

        /// <summary>
        /// Lloyd's relaxation count
        /// </summary>
        public int RelaxCount { get; set; }

        /// <summary>
        /// Create center elevation? (Volcano island type)
        /// </summary>
        public bool CenterElevation { get; set; }

        /// <summary>
        /// Map plan size
        /// </summary>
        public Vector2I MapSize { get; set; }

        public void Save(BinaryWriter writer)
        {
            writer.Write(ElevationSeed);
            writer.Write(GridSeed);
            writer.Write(PolygonsCount);
            writer.Write(RelaxCount);
            writer.Write(CenterElevation);
            writer.Write(MapSize);
        }

        public void Load(BinaryReader reader)
        {
            ElevationSeed = reader.ReadInt32();
            GridSeed = reader.ReadInt32();
            PolygonsCount = reader.ReadInt32();
            RelaxCount = reader.ReadInt32();
            CenterElevation = reader.ReadBoolean();
            MapSize = reader.ReadVector2I();
        }
    }
}