using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.World;
using Utopia.Shared.Structs;
using Utopia.Shared.Chunks;
namespace Utopia.Shared.World
{
    public class VisualWorldParameters
    {
        public int ChunkPOWsize;
        WorldParameters _worldParameters;
        public WorldParameters WorldParameters
        {
            get { return _worldParameters; }
            set { _worldParameters = value; newWorldParameters();}
        }

        public Range<int> WorldRange;
        public Location2<int> WrapEnd;
        public Location3<int> WorldVisibleSize;
        public Location2<int> WorldChunkStartUpPosition;

        public VisualWorldParameters(WorldParameters worldParameters)
        {
            WorldParameters = worldParameters;
        }

        private void newWorldParameters()
        {
            WorldVisibleSize = new Location3<int>()
            {
                X = AbstractChunk.ChunkSize.X * _worldParameters.WorldChunkSize.X,
                Y = AbstractChunk.ChunkSize.Y,
                Z = AbstractChunk.ChunkSize.Z * _worldParameters.WorldChunkSize.Z,
            };

            ChunkPOWsize = (int)Math.Log(AbstractChunk.ChunkSize.X, 2);
        }
    }
}
