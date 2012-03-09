using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.Entities;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.World;
using Utopia.Shared.Structs;
using Utopia.Shared.Chunks;
using S33M3_Resources.Structs;
using S33M3_CoreComponents.Maths;

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
        public Vector2I WrapEnd;
        public Vector3I WorldVisibleSize;
        public int WorldVisibleSizeXY;
        public int WorldVisibleSizeXYZ;
        public Vector2I WorldChunkStartUpPosition;

        public VisualWorldParameters(WorldParameters worldParameters, PlayerCharacter player)
        {
            WorldParameters = worldParameters;

            //Find the chunk location
            int X = (MathHelper.Fastfloor(player.Position.X / 16) * 16) - ((worldParameters.WorldChunkSize.X / 2) * 16);
            int Z = (MathHelper.Fastfloor(player.Position.Z / 16) * 16) - ((worldParameters.WorldChunkSize.Y / 2) * 16);

            WorldChunkStartUpPosition = new Vector2I(X, Z);
        }

        private void newWorldParameters()
        {
            WorldVisibleSize = new Vector3I()
            {
                X = AbstractChunk.ChunkSize.X * _worldParameters.WorldChunkSize.X,
                Y = AbstractChunk.ChunkSize.Y,
                Z = AbstractChunk.ChunkSize.Z * _worldParameters.WorldChunkSize.Y,
            };

            WorldVisibleSizeXY = WorldVisibleSize.X * WorldVisibleSize.Y;
            WorldVisibleSizeXYZ = WorldVisibleSize.X * WorldVisibleSize.Y * WorldVisibleSize.Z;

            ChunkPOWsize = (int)Math.Log(AbstractChunk.ChunkSize.X, 2);
        }
    }
}
