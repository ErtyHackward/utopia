using S33M3CoreComponents.Cameras;
using S33M3CoreComponents.Cameras.Interfaces;
using S33M3CoreComponents.WorldFocus;
using S33M3DXEngine;
using S33M3Resources.Structs;
using Utopia.Entities.Voxel;
using Utopia.Shared.Chunks;
using Utopia.Shared.Structs;
using Utopia.Shared.Structs.Landscape;
using Utopia.Shared.World;
using Utopia.Worlds.Chunks.ChunkEntityImpacts;

namespace Utopia.Worlds.Chunks
{
    /// <summary>
    /// Represents client-side chunk that stores visual information for rendering: light, vertex/index buffers
    /// Block information stored inside each chunk (InsideDataProvider)
    /// </summary>
    public class VisualChunk3D : VisualChunkBase
    {
        private ByteColor[] _lights;

        /// <summary>
        /// Gets light intensity array (R,G,B,A (SUN))
        /// </summary>
        public ByteColor[] Lights { get { return _lights; } }

        public VisualChunk3D(
            D3DEngine d3DEngine, 
            WorldFocusManager worldFocusManager, 
            VisualWorldParameters visualWorldParameter, 
            Range3I cubeRange, 
            CameraManager<ICameraFocused> cameraManager, 
            WorldChunks worldChunkManager, 
            VoxelModelManager voxelModelManager, 
            IChunkEntityImpactManager chunkEntityImpactManager, 
            ChunkDataProvider provider = null) : 
            base(
            d3DEngine, 
            worldFocusManager, 
            visualWorldParameter, 
            cubeRange, 
            cameraManager, 
            worldChunkManager, 
            voxelModelManager, 
            chunkEntityImpactManager, 
            provider)
        {
            _lights = new ByteColor[BlockData.ChunkSize.Volume];
        }

        public override TerraCubeResult GetCube(Vector3I internalPosition)
        {
            TerraCubeResult res;
            TerraCube cube;
            cube.Id = BlockData.GetBlock(internalPosition);
            cube.EmissiveColor = _lights[((internalPosition.Z * BlockData.ChunkSize.X) + internalPosition.X) * BlockData.ChunkSize.Y + internalPosition.Y];
            cube.IsSunLightSource = cube.EmissiveColor.A == 255;
            res.Cube = cube;
            res.IsValid = true;
            return res;
        }
    }
}
