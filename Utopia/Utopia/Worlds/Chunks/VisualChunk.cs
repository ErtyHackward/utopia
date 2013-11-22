using System;
using System.Collections.Generic;
using S33M3Resources.Effects.Basics;
using SharpDX.Direct3D11;
using Utopia.Resources.ModelComp;
using Utopia.Shared.Chunks;
using Utopia.Shared.Structs;
using Utopia.Shared.Interfaces;
using SharpDX;
using Utopia.Shared.World;
using S33M3Resources.Structs;
using S33M3DXEngine.Threading;
using S33M3DXEngine;
using S33M3CoreComponents.WorldFocus;
using S33M3CoreComponents.Cameras.Interfaces;
using S33M3CoreComponents.Cameras;
using S33M3CoreComponents.Maths;
using Utopia.Entities.Voxel;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Worlds.Chunks.ChunkEntityImpacts;
using Utopia.Shared.Configuration;
using Utopia.Shared.Entities.Concrete.Interface;
using Utopia.Shared.Entities.Models;
using Utopia.Shared.Entities.Events;
using Utopia.Entities;
using Utopia.Shared.Structs.Landscape;

namespace Utopia.Worlds.Chunks
{
    /// <summary>
    /// Represents a chunk for 3d rendering using "BigArray" technique
    /// </summary>
    public class VisualChunk : VisualChunkBase, ISingleArrayDataProviderUser
    {
        private readonly SingleArrayChunkContainer _singleArrayContainer;

        public new SingleArrayDataProvider BlockData
        {
            get { return (SingleArrayDataProvider)base.BlockData; }
        }

        public VisualChunk(D3DEngine d3DEngine, 
                            WorldFocusManager worldFocusManager, 
                            VisualWorldParameters visualWorldParameter, 
                            ref Range3I cubeRange, 
                            SingleArrayChunkContainer singleArrayContainer,
                            CameraManager<ICameraFocused> cameraManager,
                            WorldChunks worldChunkManager,
                            VoxelModelManager voxelModelManager,
                            IChunkEntityImpactManager chunkEntityImpactManager, 
                            ChunkDataProvider provider = null)  : 
            base(   d3DEngine,
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
