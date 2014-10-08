using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.Entities;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.World;
using Utopia.Shared.Structs;
using Utopia.Shared.Chunks;
using S33M3Resources.Structs;
using S33M3CoreComponents.Maths;
using System.IO;
using Utopia.Shared.Settings;
using Utopia.Shared.GraphicManagers;

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

        public Vector2I VisibleChunkInWorld; 

        public Range3I WorldRange;
        public Vector2I WrapEnd;
        public Vector3I WorldVisibleSize;
        public int WorldVisibleSizeXY;
        public int WorldVisibleSizeXYZ;
        public Vector3I WorldChunkStartUpPosition;
        public CubeTexturesManager CubeTextureManager;

        public VisualWorldParameters()
        {
            WorldParameters = new WorldParameters();
        }

        public VisualWorldParameters(WorldParameters worldParameters, IDynamicEntity player, Vector2I visibleChunkInWorld, CubeTexturesManager cubeTextureManager)
        {
            CubeTextureManager = cubeTextureManager;
            VisibleChunkInWorld = visibleChunkInWorld;
            WorldParameters = worldParameters;

            //Find the chunk location
            int X = (MathHelper.Floor(player.Position.X / 16) * 16) - ((VisibleChunkInWorld.X / 2) * 16);
            int Z = (MathHelper.Floor(player.Position.Z / 16) * 16) - ((VisibleChunkInWorld.Y / 2) * 16);

            WorldChunkStartUpPosition = new Vector3I(X, 0, Z);
        }

        private void newWorldParameters()
        {
            WorldVisibleSize = new Vector3I()
            {
                X = AbstractChunk.ChunkSize.X * VisibleChunkInWorld.X,
                Y = AbstractChunk.ChunkSize.Y,
                Z = AbstractChunk.ChunkSize.Z * VisibleChunkInWorld.Y,
            };

            WorldVisibleSizeXY = WorldVisibleSize.X * WorldVisibleSize.Y;
            WorldVisibleSizeXYZ = WorldVisibleSize.X * WorldVisibleSize.Y * WorldVisibleSize.Z;

            ChunkPOWsize = (int)Math.Log(AbstractChunk.ChunkSize.X, 2);

            InitCubesProfiles();
        }

        //Will assign Array id to Cube profiles
        public void InitCubesProfiles()
        {
            if (WorldParameters.Configuration != null)
            {
                foreach (var profile in WorldParameters.Configuration.BlockProfiles.Where(x => x != null && x.Name != "System Reserved" && x.Textures != null))
                {
                    //Assign each block profile a texture id !
                    foreach (var blockTexture in profile.Textures.Where(x => x != null && x.Texture.Name != null))
                    {
                        CubeTexturesManager.CubeTextureInfo info;
                        if (CubeTextureManager.CubeTexturesMeta.TryGetValue(blockTexture.Texture.Name, out info))
                        {
                            blockTexture.TextureArrayId = info.TextureArrayId;
                            blockTexture.Texture.AnimationFrames = info.NbrFrame;
                        }
                    }
                }
                WorldParameters.Configuration.isCubesProfilesIDInitialized = true;
            }
        }
    }
}
