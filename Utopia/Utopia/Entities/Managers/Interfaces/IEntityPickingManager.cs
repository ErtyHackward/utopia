using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using S33M3Engines.Shared.Math;
using Utopia.Entities.Voxel;
using Utopia.Worlds.Chunks;

namespace Utopia.Entities.Managers.Interfaces
{
    public interface IEntityPickingManager
    {
        PlayerEntityManager Player { get; set; }
        IWorldChunks WorldChunks { get; set; }
        bool CheckEntityPicking(ref Ray pickingRay, out VisualEntity pickedEntity);
        void isCollidingWithEntity(ref Vector3D newPosition2Evaluate, ref Vector3D previousPosition);
    }
}
