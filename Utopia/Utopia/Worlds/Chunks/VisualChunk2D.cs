using Utopia.Shared.Chunks;
using Utopia.Shared.Structs;
using Utopia.Shared.Interfaces;
using Utopia.Shared.World;
using S33M3Resources.Structs;
using S33M3DXEngine;
using S33M3CoreComponents.WorldFocus;
using S33M3CoreComponents.Cameras.Interfaces;
using S33M3CoreComponents.Cameras;
using Utopia.Entities.Voxel;
using Utopia.Worlds.Chunks.ChunkEntityImpacts;
using Utopia.Shared.Structs.Landscape;

namespace Utopia.Worlds.Chunks
{
    /// <summary>
    /// Represents a chunk for 3d rendering using "BigArray" technique in 2d chunk layout
    /// </summary>
    public class VisualChunk2D : VisualChunkBase, ISingleArrayDataProviderUser
    {
        private readonly SingleArrayChunkContainer _singleArrayContainer;

        public new SingleArrayDataProvider BlockData
        {
            get { return (SingleArrayDataProvider)base.BlockData; }
        }

        public VisualChunk2D( D3DEngine d3DEngine, 
                            WorldFocusManager worldFocusManager, 
                            VisualWorldParameters visualWorldParameter, 
                            ref Range3I cubeRange, 
                            SingleArrayChunkContainer singleArrayContainer,
                            CameraManager<ICameraFocused> cameraManager,
                            WorldChunks worldChunkManager,
                            VoxelModelManager voxelModelManager,
                            IChunkEntityImpactManager chunkEntityImpactManager)
            : base( d3DEngine,
                    worldFocusManager, 
                    visualWorldParameter, 
                    cubeRange, 
                    cameraManager, 
                    worldChunkManager, 
                    voxelModelManager, 
                    chunkEntityImpactManager,  
                    new SingleArrayDataProvider(singleArrayContainer))
        {
            ((SingleArrayDataProvider)base.BlockData).DataProviderUser = this; //Didn't find a way to pass it inside the constructor
            _singleArrayContainer = singleArrayContainer;
        }

        public override TerraCubeResult GetCube(Vector3I internalPosition)
        {
            return _singleArrayContainer.GetCube(internalPosition);
        }
    }
}
