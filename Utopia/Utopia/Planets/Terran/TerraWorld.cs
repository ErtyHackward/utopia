using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Engines.Struct;
using S33M3Engines.D3D;
using Utopia.Planets.Terran.Chunk;
using Utopia.Planets.Terran.Cube;
using Utopia.Planets.Terran.World;
using SharpDX;
using S33M3Engines.Maths;
using Utopia.Shared.Structs;
using Utopia.Shared.Landscaping;
using S33M3Engines.Shared.Math;
using S33M3Engines;
using S33M3Engines.WorldFocus;
using S33M3Engines.Cameras;

namespace Utopia.Planets.Terran
{
    public class TerraWorld
    {

        /// <summary>
        /// Do a quick Translation on the passed in world matrix
        /// </summary>
        /// <param name="WorldMatrix">The original world matrix !!! ==> Can only contains translation information (No rotations !!!!)</param>
        /// <param name="WorldFocusedMatrix">The output</param>
        public static void CenterOnFocus(ref Matrix WorldMatrix, ref Matrix WorldFocusedMatrix, ref Location3<int> FocusPoint)
        {
            WorldFocusedMatrix.M41 = WorldMatrix.M41 - FocusPoint.X;
            WorldFocusedMatrix.M42 = WorldMatrix.M42 - FocusPoint.Y;
            WorldFocusedMatrix.M43 = WorldMatrix.M43 - FocusPoint.Z;
        }

        #region private variables
        internal D3DEngine _d3dEngine;
        internal WorldFocusManager _worldFocusManager;
        internal CameraManager _camManager;
        internal LandscapeBuilder _landscapeBuilder;
        #endregion

        #region public Properties
        public Range<int> WorldRange;
        public Location2<int> WrapEnd;
        public LandScape Landscape;
        public TerraChunk[] Chunks;
        public TerraChunk[] SortedChunks;
        public int WorldSeed;
        public bool ChunkNeed2BeSorted;
        #endregion

        public TerraWorld(D3DEngine d3dEngine, WorldFocusManager worldFocusManager, CameraManager camManager, ref int worldSeed, LandscapeBuilder landscapeBuilder)
        {
            _worldFocusManager = worldFocusManager;
            _camManager = camManager;
            _d3dEngine = d3dEngine;
            WorldSeed = worldSeed;
            _landscapeBuilder = landscapeBuilder;

            Initialize();
        }

        private void Initialize()
        {
            //Defining the World Offset, to be used to reference the 2d circular array of dim defined in chunk
            WorldRange = new Range<int>()
            {
                Min = new Location3<int>(LandscapeBuilder.WorldStartUpX, 0, LandscapeBuilder.WorldStartUpZ),
                Max = new Location3<int>(LandscapeBuilder.WorldStartUpX + LandscapeBuilder.Worldsize.X, LandscapeBuilder.Worldsize.Y, LandscapeBuilder.WorldStartUpZ + LandscapeBuilder.Worldsize.Z)
                                    };

            //Find the next number where mod == 0 !
            int XWrap = LandscapeBuilder.WorldStartUpX;
            int ZWrap = LandscapeBuilder.WorldStartUpZ;

            while (MathHelper.Mod(XWrap, LandscapeBuilder.ChunkGridSize * LandscapeBuilder.Chunksize) != 0) XWrap++;
            while (MathHelper.Mod(ZWrap, LandscapeBuilder.ChunkGridSize * LandscapeBuilder.Chunksize) != 0) ZWrap++;

            WrapEnd = new Location2<int>(XWrap, ZWrap);

            //Init the Main landsape buffer
            // !!!!!!! second Dim = ZZZZ not YYYY !!!!
            this.Landscape = new LandScape(LandscapeBuilder.Worldsize, this);

            //Init the chunks, they are logical entity representing a group of cube and are responsible to construct the Vertex & Index Buffer !
            Chunks = new TerraChunk[LandscapeBuilder.ChunkGridSize * LandscapeBuilder.ChunkGridSize];
            SortedChunks = new TerraChunk[LandscapeBuilder.ChunkGridSize * LandscapeBuilder.ChunkGridSize];

            Range<int> cubeRange;
            TerraChunk chunk;
            int arrayX, arrayZ;
            for (int chunkX = 0; chunkX < LandscapeBuilder.ChunkGridSize; chunkX++)
            {
                for (int chunkZ = 0; chunkZ < LandscapeBuilder.ChunkGridSize; chunkZ++)
                {
                    cubeRange = new Range<int>()
                    {
                        Min = new Location3<int>(LandscapeBuilder.WorldStartUpX + (chunkX * LandscapeBuilder.Chunksize), 0, LandscapeBuilder.WorldStartUpZ + (chunkZ * LandscapeBuilder.Chunksize)),
                        Max = new Location3<int>(LandscapeBuilder.WorldStartUpX + ((chunkX + 1) * LandscapeBuilder.Chunksize), LandscapeBuilder.Worldsize.Y, LandscapeBuilder.WorldStartUpZ + ((chunkZ + 1) * LandscapeBuilder.Chunksize))
                    };

                    arrayX = MathHelper.Mod(cubeRange.Min.X, LandscapeBuilder.Worldsize.X);
                    arrayZ = MathHelper.Mod(cubeRange.Min.Z, LandscapeBuilder.Worldsize.Z);

                    //Block modified ===> the chunk must be rebuilt !
                    chunk = new TerraChunk(_d3dEngine, _worldFocusManager, cubeRange, Landscape, this, _landscapeBuilder);

                    Chunks[(arrayX >> LandscapeBuilder.ChunkPOWsize) + (arrayZ >> LandscapeBuilder.ChunkPOWsize) * LandscapeBuilder.ChunkGridSize] = chunk;
                    SortedChunks[(arrayX >> LandscapeBuilder.ChunkPOWsize) + (arrayZ >> LandscapeBuilder.ChunkPOWsize) * LandscapeBuilder.ChunkGridSize] = chunk;
                }
            }

            ChunkFinder.Init(Landscape, Chunks, LandscapeBuilder.Worldsize);
            ChunkNeed2BeSorted = true;
            SortChunk();
        }

        #region Public properties

        public void SortChunk()
        {
            if (!ChunkNeed2BeSorted || _camManager.ActiveCamera == null) return;
            int index = 0;

            foreach (var chunk in Chunks.OrderBy(x => MVector3.Distance(x.CubeRange.Min, _camManager.ActiveCamera.WorldPosition)))
            {
                SortedChunks[index] = chunk;
                index++;
            }

            ChunkNeed2BeSorted = false;
        }

        public void Dispose()
        {
            foreach (TerraChunk chunk in Chunks)
            {
                if (chunk != null) chunk.Dispose();
            }
        }
        #endregion

        #region Private properties

        #endregion
    }
}
